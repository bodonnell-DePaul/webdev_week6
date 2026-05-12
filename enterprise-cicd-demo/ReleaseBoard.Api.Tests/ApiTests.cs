using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace ReleaseBoard.Api.Tests;

public sealed class ApiTests(ReleaseBoardFactory factory) : IClassFixture<ReleaseBoardFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task ReadyHealthCheckReportsDatabaseAvailability()
    {
        var response = await _client.GetAsync("/api/health/ready");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var health = await response.Content.ReadFromJsonAsync<HealthResponse>();
        Assert.Equal("ready", health?.Status);
    }

    [Fact]
    public async Task VersionEndpointExposesArtifactIdentity()
    {
        var version = await _client.GetFromJsonAsync<VersionResponse>("/api/version");

        Assert.Equal("ReleaseBoard.Api", version?.Service);
        Assert.Equal("test-sha", version?.GitSha);
        Assert.Equal("42", version?.BuildNumber);
    }

    [Fact]
    public async Task SeedDataIncludesReleaseHistory()
    {
        var releases = await _client.GetFromJsonAsync<List<ReleaseResponse>>("/api/releases");

        Assert.NotNull(releases);
        Assert.True(releases.Count >= 3);
        Assert.Contains(releases, release => release.Deployments.Count > 0);
    }

    [Fact]
    public async Task MutationsRequireDemoKey()
    {
        var response = await _client.PatchAsJsonAsync("/api/feature-flags/new-dashboard-layout", new { isEnabled = false, rolloutPercentage = 0 });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task FeatureFlagMutationWritesAuditEvent()
    {
        using var request = new HttpRequestMessage(HttpMethod.Patch, "/api/feature-flags/new-dashboard-layout");
        request.Headers.Add("X-ReleaseBoard-Demo-Key", "test-key");
        request.Content = JsonContent.Create(new { isEnabled = false, rolloutPercentage = 0 });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var events = await _client.GetFromJsonAsync<List<AuditEventResponse>>("/api/audit-events");
        Assert.Contains(events!, auditEvent => auditEvent.Title == "Feature flag changed");
    }

    [Fact]
    public async Task InvalidReleaseIsRejected()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/releases");
        request.Headers.Add("X-ReleaseBoard-Demo-Key", "test-key");
        request.Content = JsonContent.Create(new { name = "x", artifactVersion = "v1", gitSha = "abc" });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

public sealed class ReleaseBoardFactory : WebApplicationFactory<Program>
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"releaseboard-tests-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = $"Data Source={_databasePath}",
                ["ReleaseBoard:DemoApiKey"] = "test-key",
                ["RELEASEBOARD_GIT_SHA"] = "test-sha",
                ["RELEASEBOARD_BUILD_NUMBER"] = "42",
                ["RELEASEBOARD_BUILD_TIME"] = "test-build"
            });
        });
    }
}

public sealed record HealthResponse(string Status, string Service, DateTimeOffset CheckedAt);
public sealed record VersionResponse(string Service, string Environment, string GitSha, string BuildNumber, string BuildTime);
public sealed record ReleaseResponse(int Id, string Name, string ArtifactVersion, string GitSha, string Status, DateTimeOffset CreatedAt, List<DeploymentResponse> Deployments);
public sealed record DeploymentResponse(int Id, int ReleaseId, string ReleaseName, string EnvironmentName, string Status, string GitSha, string ArtifactVersion, string Strategy, DateTimeOffset StartedAt, DateTimeOffset? CompletedAt);
public sealed record AuditEventResponse(int Id, string Title, string Details, string Actor, DateTimeOffset OccurredAt);
