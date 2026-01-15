using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text.Json;
using Xunit;

namespace ClinicalIntelligence.Api.Tests.Configuration;

public class SwaggerConfigurationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SwaggerConfigurationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SwaggerUI_IsAccessible_ReturnsOk()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/swagger/index.html");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("swagger-ui", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task OpenApiSpec_IsGenerated_ReturnsValidJson()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        
        var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;
        
        Assert.True(root.TryGetProperty("openapi", out var openapiVersion));
        Assert.StartsWith("3.0", openapiVersion.GetString());
        
        Assert.True(root.TryGetProperty("info", out var info));
        Assert.True(info.TryGetProperty("title", out var title));
        Assert.Equal("Clinical Intelligence API", title.GetString());
        
        Assert.True(root.TryGetProperty("paths", out var paths));
        Assert.True(paths.EnumerateObject().Any());
    }

    [Fact]
    public async Task OpenApiSpec_ContainsHealthEndpoint_ReturnsTrue()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();
        
        var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;
        
        Assert.True(root.TryGetProperty("paths", out var paths));
        Assert.True(paths.TryGetProperty("/health", out _));
    }

    [Fact]
    public async Task OpenApiSpec_ContainsVersionedEndpoints_ReturnsTrue()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();
        
        var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;
        
        Assert.True(root.TryGetProperty("paths", out var paths));
        
        var hasVersionedEndpoint = false;
        foreach (var path in paths.EnumerateObject())
        {
            if (path.Name.StartsWith("/api/v1/", StringComparison.OrdinalIgnoreCase))
            {
                hasVersionedEndpoint = true;
                break;
            }
        }
        
        Assert.True(hasVersionedEndpoint, "OpenAPI spec should contain at least one versioned endpoint starting with /api/v1/");
    }
}
