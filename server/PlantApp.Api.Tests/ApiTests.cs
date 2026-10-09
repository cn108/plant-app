using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PlantApp.Api;

namespace PlantApp.Api.Tests;

public class ApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        _connection.Open();
        builder.ConfigureServices(s =>
        {
            s.RemoveAll<DbContextOptions<AppDb>>();
            s.AddDbContext<AppDb>(o => o.UseSqlite(_connection));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection.Dispose();
    }

    public async Task<HttpClient> LoggedInClient(string name = "alice")
    {
        var client = CreateClient();
        var res = await client.PostAsJsonAsync("/api/auth/register",
            new { username = name, email = $"{name}@example.com", password = "Sup3rSecret!" });
        res.EnsureSuccessStatusCode();
        var auth = (await res.Content.ReadFromJsonAsync<AuthResponse>())!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);
        return client;
    }
}

public class AuthTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Register_then_login_works_and_password_is_hashed()
    {
        var c = f.CreateClient();
        Assert.Equal(HttpStatusCode.OK, (await c.PostAsJsonAsync("/api/auth/register",
            new { username = "bob", email = "Bob@Example.com", password = "longenough1" })).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await c.PostAsJsonAsync("/api/auth/login",
            new { email = "bob@example.com", password = "longenough1" })).StatusCode);

        using var scope = f.Services.CreateScope();
        var user = scope.ServiceProvider.GetRequiredService<AppDb>().Users.Single(u => u.Username == "bob");
        Assert.DoesNotContain("longenough1", user.PasswordHash);
    }

    [Fact]
    public async Task Wrong_password_is_rejected()
    {
        var c = f.CreateClient();
        await c.PostAsJsonAsync("/api/auth/register", new { username = "carol", email = "carol@example.com", password = "longenough1" });
        var res = await c.PostAsJsonAsync("/api/auth/login", new { email = "carol@example.com", password = "wrong" });
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Duplicate_and_weak_registrations_are_rejected()
    {
        var c = f.CreateClient();
        var body = new { username = "dave", email = "dave@example.com", password = "longenough1" };
        await c.PostAsJsonAsync("/api/auth/register", body);
        Assert.Equal(HttpStatusCode.Conflict, (await c.PostAsJsonAsync("/api/auth/register", body)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await c.PostAsJsonAsync("/api/auth/register",
            new { username = "eve", email = "not-an-email", password = "short" })).StatusCode);
    }

    [Fact]
    public async Task Protected_endpoints_require_a_token()
    {
        var c = f.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await c.GetAsync("/api/plants")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await c.PostAsync("/api/identify", null)).StatusCode);
    }
}

public class PlantTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Users_only_see_and_modify_their_own_plants()
    {
        var alice = await f.LoggedInClient("alice");
        var mallory = await f.LoggedInClient("mallory");

        var created = await alice.PostAsJsonAsync("/api/plants", new { name = "Basil", wateringIntervalDays = 3 });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var plant = (await created.Content.ReadFromJsonAsync<PlantDto>())!;

        Assert.Empty((await mallory.GetFromJsonAsync<List<PlantDto>>("/api/plants"))!);
        Assert.Equal(HttpStatusCode.NotFound, (await mallory.PostAsync($"/api/plants/{plant.Id}/water", null)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await mallory.DeleteAsync($"/api/plants/{plant.Id}")).StatusCode);

        var watered = await alice.PostAsync($"/api/plants/{plant.Id}/water", null);
        var dto = (await watered.Content.ReadFromJsonAsync<PlantDto>())!;
        Assert.NotNull(dto.LastWateredUtc);
        Assert.False(dto.Overdue);
    }

    [Fact]
    public async Task Invalid_plant_is_rejected()
    {
        var alice = await f.LoggedInClient("alice2");
        Assert.Equal(HttpStatusCode.BadRequest,
            (await alice.PostAsJsonAsync("/api/plants", new { name = "", wateringIntervalDays = 0 })).StatusCode);
    }
}

public class ForumTests(ApiFactory f) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Posting_replying_reporting_works_and_admin_endpoints_are_forbidden_to_users()
    {
        var user = await f.LoggedInClient("poster");
        var post = await user.PostAsJsonAsync("/api/forum", new { category = "Tips", text = "Water in the morning" });
        Assert.Equal(HttpStatusCode.Created, post.StatusCode);
        var id = (await post.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>()).GetProperty("id").GetInt32();

        Assert.Equal(HttpStatusCode.Created,
            (await user.PostAsJsonAsync($"/api/forum/{id}/replies", new { text = "Thanks!" })).StatusCode);
        Assert.Equal(HttpStatusCode.Accepted,
            (await user.PostAsJsonAsync($"/api/forum/{id}/report", new { reason = "spam" })).StatusCode);

        Assert.Equal(HttpStatusCode.Forbidden, (await user.GetAsync("/api/admin/reports")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await f.CreateClient().PostAsJsonAsync("/api/forum",
            new { category = "Tips", text = "anon" })).StatusCode);
    }
}

public class CareSchedulerTests
{
    private static Plant P(int interval, DateTime last) =>
        new() { WateringIntervalDays = interval, LastWateredUtc = last, CreatedAt = last };

    [Fact]
    public void Plant_is_overdue_after_its_interval()
    {
        var now = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc);
        Assert.True(CareScheduler.Evaluate(P(3, now.AddDays(-4)), now).Overdue);
        Assert.False(CareScheduler.Evaluate(P(3, now.AddDays(-1)), now).Overdue);
    }

    [Theory]
    [InlineData(7, 35.0, 0.0, 5)]
    [InlineData(7, 5.0, 0.0, 9)]
    [InlineData(7, 20.0, 10.0, 8)]
    [InlineData(1, 35.0, 0.0, 1)]
    public void Interval_adapts_to_weather(int baseDays, double temp, double rain, int expected) =>
        Assert.Equal(expected, CareScheduler.AdjustedInterval(baseDays, temp, rain));

    [Fact]
    public void Password_hash_verifies_only_the_right_password()
    {
        var hash = PasswordHasher.Hash("correct horse");
        Assert.True(PasswordHasher.Verify("correct horse", hash));
        Assert.False(PasswordHasher.Verify("battery staple", hash));
    }
}
