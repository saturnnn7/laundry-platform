using System.Net;
using System.Text.Json;

namespace Laundry.Tests.Api;

public class ApiDocsTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public ApiDocsTests(ApiFactory factory)
    {
        _factory = factory;
    }

    private async Task<JsonElement> GetDocumentAsync()
    {
        var response = await _factory.CreateClient().GetAsync("/openapi/v1.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("json", response.Content.Headers.ContentType?.MediaType);
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement.Clone();
    }

    [Fact]
    public async Task Document_HasVersionAndTitle()
    {
        var document = await GetDocumentAsync();

        Assert.True(document.TryGetProperty("openapi", out _));
        Assert.Equal("Laundry Platform API", document.GetProperty("info").GetProperty("title").GetString());
    }

    [Fact]
    public async Task Document_ListsEndpointsUnderApiPrefix()
    {
        var document = await GetDocumentAsync();

        var paths = document.GetProperty("paths").EnumerateObject().Select(p => p.Name);
        Assert.Contains(paths, p => p.StartsWith("/api/v1/", StringComparison.Ordinal));
    }

    [Fact]
    public async Task SwaggerUi_ServesHtml()
    {
        var response = await _factory.CreateClient().GetAsync("/swagger/index.html");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
    }
}
