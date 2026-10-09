using Microsoft.EntityFrameworkCore;

namespace PlantApp.Api;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = "User";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Plant
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = "";
    public string? Species { get; set; }
    public int WateringIntervalDays { get; set; } = 7;
    public DateTime? LastWateredUtc { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ForumPost
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = "";
    public string Category { get; set; } = "General";
    public string Text { get; set; } = "";
    public bool Hidden { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<ForumReply> Replies { get; set; } = new();
}

public class ForumReply
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = "";
    public string Text { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class PostReport
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public int ReporterId { get; set; }
    public string Reason { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class AppDb(DbContextOptions<AppDb> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Plant> Plants => Set<Plant>();
    public DbSet<ForumPost> Posts => Set<ForumPost>();
    public DbSet<ForumReply> Replies => Set<ForumReply>();
    public DbSet<PostReport> Reports => Set<PostReport>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>().HasIndex(u => u.Email).IsUnique();
        b.Entity<User>().HasIndex(u => u.Username).IsUnique();
        b.Entity<ForumPost>().HasMany(p => p.Replies).WithOne().HasForeignKey(r => r.PostId);
    }
}
