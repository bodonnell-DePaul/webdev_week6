using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactClient", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<ReleaseBoardDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=releaseboard.db"));

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseCors("ReactClient");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ReleaseBoardDbContext>();
    await db.Database.MigrateAsync();
    await SeedData.InitializeAsync(db);
}

app.MapGet("/", () => Results.Redirect("/api/health/live"));

app.MapGet("/api/health/live", () => Results.Ok(new HealthDto("live", "ReleaseBoard.Api", DateTimeOffset.UtcNow)));

app.MapGet("/api/health/ready", async (ReleaseBoardDbContext db) =>
{
    var canConnect = await db.Database.CanConnectAsync();
    return canConnect
        ? Results.Ok(new HealthDto("ready", "ReleaseBoard.Api", DateTimeOffset.UtcNow))
        : Results.Problem("Database is unavailable.", statusCode: StatusCodes.Status503ServiceUnavailable);
});

app.MapGet("/api/version", (IConfiguration configuration, IWebHostEnvironment environment) =>
{
    var version = new VersionDto(
        Service: "ReleaseBoard.Api",
        Environment: environment.EnvironmentName,
        GitSha: configuration["RELEASEBOARD_GIT_SHA"] ?? configuration["GITHUB_SHA"] ?? "local-dev",
        BuildNumber: configuration["RELEASEBOARD_BUILD_NUMBER"] ?? configuration["GITHUB_RUN_NUMBER"] ?? "local",
        BuildTime: configuration["RELEASEBOARD_BUILD_TIME"] ?? "not-set");

    return Results.Ok(version);
});

app.MapGet("/api/summary", async (ReleaseBoardDbContext db) =>
{
    var deployments = await db.Deployments.AsNoTracking().ToListAsync();
    var summary = new SummaryDto(
        ReleaseCount: await db.Releases.CountAsync(),
        EnvironmentCount: await db.DeploymentEnvironments.CountAsync(),
        DeploymentCount: deployments.Count,
        SuccessfulDeployments: deployments.Count(deployment => deployment.Status == DeploymentStatus.Succeeded),
        FailedDeployments: deployments.Count(deployment => deployment.Status == DeploymentStatus.Failed),
        EnabledFlags: await db.FeatureFlags.CountAsync(flag => flag.IsEnabled));

    return Results.Ok(summary);
});

app.MapGet("/api/releases", async (ReleaseBoardDbContext db) =>
{
    var releases = await db.Releases
        .AsNoTracking()
        .Include(release => release.Deployments)
            .ThenInclude(deployment => deployment.Environment)
        .OrderByDescending(release => release.Id)
        .Select(release => ReleaseDto.From(release))
        .ToListAsync();

    return Results.Ok(releases);
});

app.MapPost("/api/releases", async (CreateReleaseRequest request, ReleaseBoardDbContext db, HttpRequest httpRequest, IConfiguration configuration) =>
{
    if (!HasDemoAccess(httpRequest, configuration))
    {
        return Results.Unauthorized();
    }

    if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length < 3)
    {
        return Results.BadRequest(new ApiError("Release name must be at least 3 characters."));
    }

    if (string.IsNullOrWhiteSpace(request.ArtifactVersion) || request.ArtifactVersion.Trim().Length < 3)
    {
        return Results.BadRequest(new ApiError("Artifact version must be at least 3 characters."));
    }

    var release = new Release
    {
        Name = request.Name.Trim(),
        ArtifactVersion = request.ArtifactVersion.Trim(),
        GitSha = NormalizeGitSha(request.GitSha),
        Status = ReleaseStatus.Candidate
    };

    db.Releases.Add(release);
    db.AuditEvents.Add(AuditEvent.System("Release candidate created", $"Release {release.Name} points to artifact {release.ArtifactVersion}."));
    await db.SaveChangesAsync();

    return Results.Created($"/api/releases/{release.Id}", ReleaseDto.From(release));
});

app.MapGet("/api/environments", async (ReleaseBoardDbContext db) =>
{
    var environments = await db.DeploymentEnvironments
        .AsNoTracking()
        .OrderBy(environment => environment.SortOrder)
        .Select(environment => EnvironmentDto.From(environment))
        .ToListAsync();

    return Results.Ok(environments);
});

app.MapGet("/api/deployments", async (ReleaseBoardDbContext db) =>
{
    var deployments = await db.Deployments
        .AsNoTracking()
        .Include(deployment => deployment.Release)
        .Include(deployment => deployment.Environment)
        .OrderByDescending(deployment => deployment.Id)
        .Select(deployment => DeploymentDto.From(deployment))
        .ToListAsync();

    return Results.Ok(deployments);
});

app.MapGet("/api/feature-flags", async (ReleaseBoardDbContext db) =>
{
    var flags = await db.FeatureFlags
        .AsNoTracking()
        .OrderBy(flag => flag.Key)
        .Select(flag => FeatureFlagDto.From(flag))
        .ToListAsync();

    return Results.Ok(flags);
});

app.MapPatch("/api/feature-flags/{key}", async (string key, UpdateFeatureFlagRequest request, ReleaseBoardDbContext db, HttpRequest httpRequest, IConfiguration configuration) =>
{
    if (!HasDemoAccess(httpRequest, configuration))
    {
        return Results.Unauthorized();
    }

    var flag = await db.FeatureFlags.SingleOrDefaultAsync(featureFlag => featureFlag.Key == key);
    if (flag is null)
    {
        return Results.NotFound(new ApiError("Feature flag not found."));
    }

    flag.IsEnabled = request.IsEnabled;
    flag.RolloutPercentage = Math.Clamp(request.RolloutPercentage, 0, 100);
    flag.UpdatedAt = DateTimeOffset.UtcNow;

    db.AuditEvents.Add(AuditEvent.System(
        "Feature flag changed",
        $"{flag.Key} is now {(flag.IsEnabled ? "enabled" : "disabled")} at {flag.RolloutPercentage}% rollout."));

    await db.SaveChangesAsync();

    return Results.Ok(FeatureFlagDto.From(flag));
});

app.MapGet("/api/audit-events", async (ReleaseBoardDbContext db) =>
{
    var events = await db.AuditEvents
        .AsNoTracking()
        .OrderByDescending(auditEvent => auditEvent.Id)
        .Take(50)
        .Select(auditEvent => AuditEventDto.From(auditEvent))
        .ToListAsync();

    return Results.Ok(events);
});

app.MapPost("/api/audit-events", async (CreateAuditEventRequest request, ReleaseBoardDbContext db, HttpRequest httpRequest, IConfiguration configuration) =>
{
    if (!HasDemoAccess(httpRequest, configuration))
    {
        return Results.Unauthorized();
    }

    if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Details))
    {
        return Results.BadRequest(new ApiError("Audit event title and details are required."));
    }

    var auditEvent = AuditEvent.System(request.Title.Trim(), request.Details.Trim());
    db.AuditEvents.Add(auditEvent);
    await db.SaveChangesAsync();

    return Results.Created($"/api/audit-events/{auditEvent.Id}", AuditEventDto.From(auditEvent));
});

app.MapPost("/api/demo-reset", async (ReleaseBoardDbContext db, HttpRequest httpRequest, IConfiguration configuration) =>
{
    if (!HasDemoAccess(httpRequest, configuration))
    {
        return Results.Unauthorized();
    }

    await db.Database.ExecuteSqlRawAsync("DELETE FROM AuditEvents");
    await db.Database.ExecuteSqlRawAsync("DELETE FROM Deployments");
    await db.Database.ExecuteSqlRawAsync("DELETE FROM Releases");
    await db.Database.ExecuteSqlRawAsync("DELETE FROM DeploymentEnvironments");
    await db.Database.ExecuteSqlRawAsync("DELETE FROM FeatureFlags");
    await SeedData.InitializeAsync(db, force: true);

    return Results.Ok(new { status = "reset" });
});

app.Run();

static bool HasDemoAccess(HttpRequest request, IConfiguration configuration)
{
    var configuredKey = configuration["ReleaseBoard:DemoApiKey"] ?? "local-demo-key";
    return request.Headers.TryGetValue("X-ReleaseBoard-Demo-Key", out var providedKey)
        && string.Equals(providedKey.ToString(), configuredKey, StringComparison.Ordinal);
}

static string NormalizeGitSha(string? value)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        return "local-dev";
    }

    return value.Trim().Length > 12 ? value.Trim()[..12] : value.Trim();
}

public partial class Program;

public sealed class ReleaseBoardDbContext(DbContextOptions<ReleaseBoardDbContext> options) : DbContext(options)
{
    public DbSet<Release> Releases => Set<Release>();
    public DbSet<DeploymentEnvironment> DeploymentEnvironments => Set<DeploymentEnvironment>();
    public DbSet<Deployment> Deployments => Set<Deployment>();
    public DbSet<FeatureFlag> FeatureFlags => Set<FeatureFlag>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Release>()
            .HasIndex(release => release.ArtifactVersion)
            .IsUnique();

        modelBuilder.Entity<DeploymentEnvironment>()
            .HasIndex(environment => environment.Name)
            .IsUnique();

        modelBuilder.Entity<Deployment>()
            .HasIndex(deployment => new { deployment.ReleaseId, deployment.DeploymentEnvironmentId });

        modelBuilder.Entity<FeatureFlag>()
            .HasIndex(flag => flag.Key)
            .IsUnique();
    }
}

public sealed class Release
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string ArtifactVersion { get; set; }
    public required string GitSha { get; set; }
    public ReleaseStatus Status { get; set; }
    public List<Deployment> Deployments { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class DeploymentEnvironment
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Url { get; set; }
    public bool RequiresApproval { get; set; }
    public int SortOrder { get; set; }
    public List<Deployment> Deployments { get; set; } = [];
}

public sealed class Deployment
{
    public int Id { get; set; }
    public int ReleaseId { get; set; }
    public Release Release { get; set; } = null!;
    public int DeploymentEnvironmentId { get; set; }
    public DeploymentEnvironment Environment { get; set; } = null!;
    public DeploymentStatus Status { get; set; }
    public required string GitSha { get; set; }
    public required string ArtifactVersion { get; set; }
    public required string Strategy { get; set; }
    public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CompletedAt { get; set; }
}

public sealed class FeatureFlag
{
    public int Id { get; set; }
    public required string Key { get; set; }
    public required string Description { get; set; }
    public bool IsEnabled { get; set; }
    public int RolloutPercentage { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class AuditEvent
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Details { get; set; }
    public required string Actor { get; set; }
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;

    public static AuditEvent System(string title, string details) => new()
    {
        Title = title,
        Details = details,
        Actor = "releaseboard-system"
    };
}

public enum ReleaseStatus
{
    Candidate,
    Staging,
    Production,
    RolledBack
}

public enum DeploymentStatus
{
    Pending,
    Running,
    Succeeded,
    Failed,
    RolledBack
}

public sealed record HealthDto(string Status, string Service, DateTimeOffset CheckedAt);
public sealed record VersionDto(string Service, string Environment, string GitSha, string BuildNumber, string BuildTime);
public sealed record SummaryDto(int ReleaseCount, int EnvironmentCount, int DeploymentCount, int SuccessfulDeployments, int FailedDeployments, int EnabledFlags);
public sealed record ApiError(string Message);
public sealed record CreateReleaseRequest(string Name, string ArtifactVersion, string? GitSha);
public sealed record UpdateFeatureFlagRequest(bool IsEnabled, int RolloutPercentage);
public sealed record CreateAuditEventRequest(string Title, string Details);

public sealed record ReleaseDto(int Id, string Name, string ArtifactVersion, string GitSha, ReleaseStatus Status, DateTimeOffset CreatedAt, IReadOnlyList<DeploymentDto> Deployments)
{
    public static ReleaseDto From(Release release) => new(
        release.Id,
        release.Name,
        release.ArtifactVersion,
        release.GitSha,
        release.Status,
        release.CreatedAt,
        release.Deployments
            .OrderByDescending(deployment => deployment.Id)
            .Select(DeploymentDto.From)
            .ToList());
}

public sealed record EnvironmentDto(int Id, string Name, string Url, bool RequiresApproval, int SortOrder)
{
    public static EnvironmentDto From(DeploymentEnvironment environment) => new(environment.Id, environment.Name, environment.Url, environment.RequiresApproval, environment.SortOrder);
}

public sealed record DeploymentDto(int Id, int ReleaseId, string ReleaseName, string EnvironmentName, DeploymentStatus Status, string GitSha, string ArtifactVersion, string Strategy, DateTimeOffset StartedAt, DateTimeOffset? CompletedAt)
{
    public static DeploymentDto From(Deployment deployment) => new(
        deployment.Id,
        deployment.ReleaseId,
        deployment.Release?.Name ?? "unknown",
        deployment.Environment?.Name ?? "unknown",
        deployment.Status,
        deployment.GitSha,
        deployment.ArtifactVersion,
        deployment.Strategy,
        deployment.StartedAt,
        deployment.CompletedAt);
}

public sealed record FeatureFlagDto(int Id, string Key, string Description, bool IsEnabled, int RolloutPercentage, DateTimeOffset UpdatedAt)
{
    public static FeatureFlagDto From(FeatureFlag flag) => new(flag.Id, flag.Key, flag.Description, flag.IsEnabled, flag.RolloutPercentage, flag.UpdatedAt);
}

public sealed record AuditEventDto(int Id, string Title, string Details, string Actor, DateTimeOffset OccurredAt)
{
    public static AuditEventDto From(AuditEvent auditEvent) => new(auditEvent.Id, auditEvent.Title, auditEvent.Details, auditEvent.Actor, auditEvent.OccurredAt);
}

public static class SeedData
{
    public static async Task InitializeAsync(ReleaseBoardDbContext db, bool force = false)
    {
        if (!force && await db.Releases.AnyAsync())
        {
            return;
        }

        var staging = new DeploymentEnvironment { Name = "staging", Url = "https://staging.releaseboard.example", RequiresApproval = false, SortOrder = 1 };
        var productionBlue = new DeploymentEnvironment { Name = "production-blue", Url = "https://blue.releaseboard.example", RequiresApproval = true, SortOrder = 2 };
        var productionGreen = new DeploymentEnvironment { Name = "production-green", Url = "https://green.releaseboard.example", RequiresApproval = true, SortOrder = 3 };

        var release1 = new Release { Name = "Release 2026.05", ArtifactVersion = "releaseboard-api-2026.05.1", GitSha = "a13f09c4e7b2", Status = ReleaseStatus.Production };
        var release2 = new Release { Name = "Release 2026.06 candidate", ArtifactVersion = "releaseboard-api-2026.06.0-rc1", GitSha = "b91c5a7710aa", Status = ReleaseStatus.Staging };
        var release3 = new Release { Name = "Hotfix rollback drill", ArtifactVersion = "releaseboard-api-2026.05.2", GitSha = "c88df192ab07", Status = ReleaseStatus.Candidate };

        db.DeploymentEnvironments.AddRange(staging, productionBlue, productionGreen);
        db.Releases.AddRange(release1, release2, release3);
        await db.SaveChangesAsync();

        db.Deployments.AddRange(
            new Deployment { ReleaseId = release1.Id, DeploymentEnvironmentId = staging.Id, Status = DeploymentStatus.Succeeded, ArtifactVersion = release1.ArtifactVersion, GitSha = release1.GitSha, Strategy = "standard", StartedAt = DateTimeOffset.UtcNow.AddDays(-7), CompletedAt = DateTimeOffset.UtcNow.AddDays(-7).AddMinutes(12) },
            new Deployment { ReleaseId = release1.Id, DeploymentEnvironmentId = productionBlue.Id, Status = DeploymentStatus.Succeeded, ArtifactVersion = release1.ArtifactVersion, GitSha = release1.GitSha, Strategy = "blue/green", StartedAt = DateTimeOffset.UtcNow.AddDays(-6), CompletedAt = DateTimeOffset.UtcNow.AddDays(-6).AddMinutes(18) },
            new Deployment { ReleaseId = release2.Id, DeploymentEnvironmentId = staging.Id, Status = DeploymentStatus.Succeeded, ArtifactVersion = release2.ArtifactVersion, GitSha = release2.GitSha, Strategy = "artifact promotion", StartedAt = DateTimeOffset.UtcNow.AddDays(-1), CompletedAt = DateTimeOffset.UtcNow.AddDays(-1).AddMinutes(11) },
            new Deployment { ReleaseId = release3.Id, DeploymentEnvironmentId = staging.Id, Status = DeploymentStatus.Failed, ArtifactVersion = release3.ArtifactVersion, GitSha = release3.GitSha, Strategy = "rollback drill", StartedAt = DateTimeOffset.UtcNow.AddHours(-8), CompletedAt = DateTimeOffset.UtcNow.AddHours(-8).AddMinutes(5) });

        db.FeatureFlags.AddRange(
            new FeatureFlag { Key = "new-dashboard-layout", Description = "Canary a denser executive dashboard before releasing to all users.", IsEnabled = true, RolloutPercentage = 25 },
            new FeatureFlag { Key = "deployment-freeze-banner", Description = "Show a change-freeze warning banner during restricted release windows.", IsEnabled = false, RolloutPercentage = 0 },
            new FeatureFlag { Key = "audit-event-stream", Description = "Enable near-real-time audit event updates in the operations view.", IsEnabled = true, RolloutPercentage = 100 });

        db.AuditEvents.AddRange(
            AuditEvent.System("Production approval recorded", "Release 2026.05 was approved for production-blue after staging health checks passed."),
            AuditEvent.System("IaC plan reviewed", "Platform owner approved a low-risk environment variable update with no network changes."),
            AuditEvent.System("Rollback drill failed safely", "Hotfix rollback drill was stopped at staging after readiness validation failed."),
            AuditEvent.System("Feature flag canary started", "new-dashboard-layout enabled for 25% of internal users."));

        await db.SaveChangesAsync();
    }
}
