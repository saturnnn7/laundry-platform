using System.Net;
using System.Text.Json;
using Laundry.BuildingBlocks.Api;

namespace Laundry.Tests.Api;

public class ApiTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public ApiTests(ApiFactory factory)
    {
        _factory = factory;
    }

    private static async Task<(JsonElement Json, string Raw)> ReadAsync(HttpResponseMessage response)
    {
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var raw = await response.Content.ReadAsStringAsync();
        return (JsonDocument.Parse(raw).RootElement.Clone(), raw);
    }

    [Fact]
    public async Task UnknownRoute_ReturnsNotFoundProblem()
    {
        var response = await _factory.CreateClient().GetAsync("/nope");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var (json, _) = await ReadAsync(response);
        Assert.Equal("not-found", json.GetProperty("type").GetString());
        Assert.Equal(404, json.GetProperty("status").GetInt32());
    }

    [Fact]
    public async Task TestRoute_AnswersUnderPrefixOnly()
    {
        var client = _factory.CreateClient();

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/v1/test/ok")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/test/ok")).StatusCode);
    }

    [Fact]
    public async Task NotFoundError_UsesItsCodeAsType()
    {
        var response = await _factory.CreateClient().GetAsync("/api/v1/test/not-found");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var (json, _) = await ReadAsync(response);
        Assert.Equal("test-missing", json.GetProperty("type").GetString());
        Assert.Equal("The thing is missing.", json.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task ConflictError_UsesItsCodeAsType()
    {
        var response = await _factory.CreateClient().GetAsync("/api/v1/test/conflict");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var (json, _) = await ReadAsync(response);
        Assert.Equal("test-conflict", json.GetProperty("type").GetString());
    }

    [Fact]
    public async Task UnhandledException_Returns500WithoutDetails()
    {
        var response = await _factory.CreateClient().GetAsync("/api/v1/test/throw");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var (json, raw) = await ReadAsync(response);
        Assert.Equal("internal-error", json.GetProperty("type").GetString());
        Assert.DoesNotContain(TestEndpoint.ExceptionMessage, raw);
        Assert.DoesNotContain(" at ", raw);
        Assert.DoesNotContain("InvalidOperationException", raw);
    }

    [Fact]
    public void AbstractEndpoint_IsNotRegistered()
    {
        var registered = _factory.Services.GetServices<IEndpoint>().Select(e => e.GetType()).ToList();

        Assert.Contains(typeof(TestEndpoint), registered);
        Assert.DoesNotContain(typeof(AbstractTestEndpoint), registered);
    }
}
