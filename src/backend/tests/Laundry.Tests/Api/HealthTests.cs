using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Laundry.Tests.Api;

public class HealthTests
{
    private const string Unreachable = "Host=127.0.0.1;Port=1;Database=laundry;Username=laundry;Timeout=2";

    private static WebApplicationFactory<Program> CreateFactory(string connectionString) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Postgres", connectionString);
        });

    [Fact]
    public async Task Liveness_IsHealthy_EvenWithoutConnectionString()
    {
        using var factory = CreateFactory(string.Empty);

        var response = await factory.CreateClient().GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Readiness_IsUnhealthy_WithoutConnectionString()
    {
        using var factory = CreateFactory(string.Empty);

        var response = await factory.CreateClient().GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal("Unhealthy", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Readiness_IsUnhealthy_WhenServerIsUnreachable_AndHidesDetails()
    {
        using var factory = CreateFactory(Unreachable);

        var response = await factory.CreateClient().GetAsync("/health/ready");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal("Unhealthy", body);
        Assert.DoesNotContain("127.0.0.1", body);
        Assert.DoesNotContain("laundry", body);
    }

    [Fact]
    public async Task HealthRoute_IsNotUnderApiPrefix()
    {
        using var factory = CreateFactory(string.Empty);

        var response = await factory.CreateClient().GetAsync("/api/v1/health");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Assert.Equal("not-found", json.GetProperty("type").GetString());
    }
}
