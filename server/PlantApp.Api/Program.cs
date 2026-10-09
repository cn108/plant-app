using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.RateLimiting;
using PlantApp.Api;

var builder = WebApplication.CreateBuilder(args);

var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
if (string.IsNullOrWhiteSpace(jwt.Key))
{
    if (!builder.Environment.IsDevelopment() && !builder.Environment.IsEnvironment("Testing"))
        throw new InvalidOperationException("Jwt:Key must be configured (at least 32 characters).");
    jwt.Key = "development-only-signing-key-change-me-0123456789";
}
if (jwt.Key.Length < 32) throw new InvalidOperationException("Jwt:Key must be at least 32 characters.");

builder.Services.AddSingleton(jwt);
builder.Services.AddSingleton<TokenService>();
builder.Services.AddDbContext<AppDb>(o =>
    o.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=plantapp.db"));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o => o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = jwt.Issuer,
        ValidAudience = jwt.Audience,
        IssuerSigningKey = jwt.SigningKey,
        ClockSkew = TimeSpan.FromMinutes(1),
    });
builder.Services.AddAuthorization();
builder.Services.AddHttpClient("plantnet", c => c.BaseAddress = new Uri("https://my-api.plantnet.org/"));
builder.Services.AddHealthChecks();
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = 429;
    o.AddFixedWindowLimiter("auth", l => { l.PermitLimit = 100; l.Window = TimeSpan.FromMinutes(1); });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<AppDb>().Database.EnsureCreated();

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

static bool IsValid(object model, out IResult problem)
{
    var errors = new List<ValidationResult>();
    if (Validator.TryValidateObject(model, new ValidationContext(model), errors, true))
    {
        problem = Results.Ok();
        return true;
    }
    problem = Results.ValidationProblem(errors
        .GroupBy(e => e.MemberNames.FirstOrDefault() ?? "")
        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage ?? "Invalid").ToArray()));
    return false;
}

// ---- Auth ----
var auth = app.MapGroup("/api/auth").RequireRateLimiting("auth");

auth.MapPost("/register", async (RegisterRequest req, AppDb db, TokenService tokens) =>
{
    if (!IsValid(req, out var problem)) return problem;
    var email = req.Email.Trim().ToLowerInvariant();
    var username = req.Username.Trim();
    if (await db.Users.AnyAsync(u => u.Email == email || u.Username == username))
        return Results.Conflict(new { error = "Username or email already in use." });
    var user = new User { Username = username, Email = email, PasswordHash = PasswordHasher.Hash(req.Password) };
    db.Users.Add(user);
    await db.SaveChangesAsync();
    return Results.Ok(new AuthResponse(tokens.Create(user), user.Id, user.Username, user.Role));
});

auth.MapPost("/login", async (LoginRequest req, AppDb db, TokenService tokens) =>
{
    var email = (req.Email ?? "").Trim().ToLowerInvariant();
    var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
    if (user is null || !PasswordHasher.Verify(req.Password ?? "", user.PasswordHash))
        return Results.Unauthorized();
    return Results.Ok(new AuthResponse(tokens.Create(user), user.Id, user.Username, user.Role));
});

// ---- Plants ----
var plants = app.MapGroup("/api/plants").RequireAuthorization();

plants.MapGet("/", async (ClaimsPrincipal me, AppDb db) =>
{
    var list = await db.Plants.Where(p => p.UserId == me.Id()).OrderBy(p => p.Name).ToListAsync();
    var now = DateTime.UtcNow;
    return list.Select(p => PlantDto.From(p, CareScheduler.Evaluate(p, now)));
});

plants.MapPost("/", async (PlantRequest req, ClaimsPrincipal me, AppDb db) =>
{
    if (!IsValid(req, out var problem)) return problem;
    var plant = new Plant
    {
        UserId = me.Id(), Name = req.Name.Trim(), Species = req.Species?.Trim(),
        WateringIntervalDays = req.WateringIntervalDays, Notes = req.Notes,
    };
    db.Plants.Add(plant);
    await db.SaveChangesAsync();
    return Results.Created($"/api/plants/{plant.Id}", PlantDto.From(plant, CareScheduler.Evaluate(plant, DateTime.UtcNow)));
});

plants.MapPost("/{id:int}/water", async (int id, ClaimsPrincipal me, AppDb db) =>
{
    var plant = await db.Plants.FirstOrDefaultAsync(p => p.Id == id && p.UserId == me.Id());
    if (plant is null) return Results.NotFound();
    plant.LastWateredUtc = DateTime.UtcNow;
    await db.SaveChangesAsync();
    return Results.Ok(PlantDto.From(plant, CareScheduler.Evaluate(plant, DateTime.UtcNow)));
});

plants.MapDelete("/{id:int}", async (int id, ClaimsPrincipal me, AppDb db) =>
{
    var plant = await db.Plants.FirstOrDefaultAsync(p => p.Id == id && p.UserId == me.Id());
    if (plant is null) return Results.NotFound();
    db.Plants.Remove(plant);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// ---- Forum ----
var forum = app.MapGroup("/api/forum");

forum.MapGet("/", async (AppDb db, string? category) =>
{
    var q = db.Posts.Include(p => p.Replies).Where(p => !p.Hidden);
    if (!string.IsNullOrWhiteSpace(category)) q = q.Where(p => p.Category == category);
    return await q.OrderByDescending(p => p.CreatedAt).Take(100).ToListAsync();
});

forum.MapPost("/", async (PostRequest req, ClaimsPrincipal me, AppDb db) =>
{
    if (!IsValid(req, out var problem)) return problem;
    var post = new ForumPost { UserId = me.Id(), Username = me.Name(), Category = req.Category, Text = req.Text.Trim() };
    db.Posts.Add(post);
    await db.SaveChangesAsync();
    return Results.Created($"/api/forum/{post.Id}", post);
}).RequireAuthorization();

forum.MapPost("/{id:int}/replies", async (int id, ReplyRequest req, ClaimsPrincipal me, AppDb db) =>
{
    if (!IsValid(req, out var problem)) return problem;
    if (!await db.Posts.AnyAsync(p => p.Id == id && !p.Hidden)) return Results.NotFound();
    var reply = new ForumReply { PostId = id, UserId = me.Id(), Username = me.Name(), Text = req.Text.Trim() };
    db.Replies.Add(reply);
    await db.SaveChangesAsync();
    return Results.Created($"/api/forum/{id}", reply);
}).RequireAuthorization();

forum.MapPost("/{id:int}/report", async (int id, ReportRequest req, ClaimsPrincipal me, AppDb db) =>
{
    if (!await db.Posts.AnyAsync(p => p.Id == id)) return Results.NotFound();
    db.Reports.Add(new PostReport { PostId = id, ReporterId = me.Id(), Reason = req.Reason ?? "" });
    await db.SaveChangesAsync();
    return Results.Accepted();
}).RequireAuthorization();

// ---- Admin / moderation ----
var admin = app.MapGroup("/api/admin").RequireAuthorization(p => p.RequireRole("Admin"));

admin.MapGet("/reports", async (AppDb db) => await db.Reports.OrderByDescending(r => r.CreatedAt).ToListAsync());

admin.MapPost("/posts/{id:int}/hide", async (int id, AppDb db) =>
{
    var post = await db.Posts.FindAsync(id);
    if (post is null) return Results.NotFound();
    post.Hidden = true;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

admin.MapGet("/users", async (AppDb db) =>
    await db.Users.Select(u => new { u.Id, u.Username, u.Email, u.Role, u.CreatedAt }).ToListAsync());

// ---- Identification proxy: the Pl@ntNet key stays on the server ----
app.MapPost("/api/identify", async (HttpRequest request, IHttpClientFactory factory, IConfiguration config) =>
{
    var key = config["PlantNet:ApiKey"];
    if (string.IsNullOrWhiteSpace(key))
        return Results.Problem("Identification is not configured on the server.", statusCode: 503);
    if (!request.HasFormContentType)
        return Results.BadRequest(new { error = "Send multipart/form-data with an 'image' file." });

    var form = await request.ReadFormAsync();
    var file = form.Files.GetFile("image");
    if (file is null || file.Length == 0 || file.Length > 5_000_000)
        return Results.BadRequest(new { error = "Provide an image under 5 MB." });

    using var content = new MultipartFormDataContent();
    var image = new StreamContent(file.OpenReadStream());
    image.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
    content.Add(image, "images", file.FileName);
    content.Add(new StringContent("auto"), "organs");

    var client = factory.CreateClient("plantnet");
    var response = await client.PostAsync($"v2/identify/all?api-key={Uri.EscapeDataString(key)}&lang=en", content);
    var body = await response.Content.ReadAsStringAsync();
    return response.IsSuccessStatusCode
        ? Results.Content(body, "application/json")
        : Results.Problem("Identification service error.", statusCode: 502);
}).RequireAuthorization().DisableAntiforgery();

app.Run();

public record RegisterRequest(
    [property: Required, StringLength(32, MinimumLength = 3)] string Username,
    [property: Required, EmailAddress] string Email,
    [property: Required, StringLength(128, MinimumLength = 8)] string Password);

public record LoginRequest(string? Email, string? Password);
public record AuthResponse(string Token, int UserId, string Username, string Role);

public record PlantRequest(
    [property: Required, StringLength(80, MinimumLength = 1)] string Name,
    string? Species,
    [property: Range(1, 365)] int WateringIntervalDays = 7,
    string? Notes = null);

public record PlantDto(int Id, string Name, string? Species, int WateringIntervalDays,
    DateTime? LastWateredUtc, DateTime NextWateringUtc, bool Overdue, string? Notes)
{
    public static PlantDto From(Plant p, CareStatus c) =>
        new(p.Id, p.Name, p.Species, p.WateringIntervalDays, p.LastWateredUtc, c.NextWateringUtc, c.Overdue, p.Notes);
}

public record PostRequest(
    [property: Required, StringLength(40)] string Category,
    [property: Required, StringLength(2000, MinimumLength = 1)] string Text);

public record ReplyRequest([property: Required, StringLength(2000, MinimumLength = 1)] string Text);
public record ReportRequest(string? Reason);

public partial class Program;
