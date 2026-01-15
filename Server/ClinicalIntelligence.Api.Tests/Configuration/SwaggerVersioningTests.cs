using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text.Json;
using Xunit;

namespace ClinicalIntelligence.Api.Tests.Configuration;

public class SwaggerVersioningTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SwaggerVersioningTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SwaggerJson_ReflectsV1Prefix()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;
        
        Assert.True(root.TryGetProperty("paths", out var paths));
        
        var hasV1Paths = false;
        foreach (var path in paths.EnumerateObject())
        {
            if (path.Name.StartsWith("/api/v1/", StringComparison.OrdinalIgnoreCase))
            {
                hasV1Paths = true;
                break;
            }
        }
        
        Assert.True(hasV1Paths, "Swagger document should contain at least one path starting with /api/v1/");
    }

    [Fact]
    public async Task SwaggerJson_HealthEndpointNotPrefixed()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;
        
        Assert.True(root.TryGetProperty("paths", out var paths));
        
        var hasHealthPath = false;
        foreach (var path in paths.EnumerateObject())
        {
            if (path.Name.Equals("/health", StringComparison.OrdinalIgnoreCase))
            {
                hasHealthPath = true;
                break;
            }
        }
        
        Assert.True(hasHealthPath, "Swagger document should contain /health endpoint without version prefix");
    }

    [Fact]
    public async Task SwaggerJson_AllApplicationEndpointsUnderV1()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;
        
        Assert.True(root.TryGetProperty("paths", out var paths));
        
        var operationalPaths = new[] { "/health", "/swagger" };
        
        foreach (var path in paths.EnumerateObject())
        {
            var pathName = path.Name;
            
            var isOperationalPath = operationalPaths.Any(op => 
                pathName.StartsWith(op, StringComparison.OrdinalIgnoreCase));
            
            if (!isOperationalPath)
            {
                Assert.True(
                    pathName.StartsWith("/api/v1/", StringComparison.OrdinalIgnoreCase),
                    $"Application endpoint '{pathName}' should start with /api/v1/ prefix"
                );
            }
        }
    }

    [Fact]
    public async Task SwaggerInfo_VersionMatchesApiVersion()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;
        
        Assert.True(root.TryGetProperty("info", out var info));
        Assert.True(info.TryGetProperty("version", out var version));
        Assert.Equal("1.0.0", version.GetString());
    }

    [Fact]
    public async Task SwaggerUI_Accessible()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/swagger/index.html");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SwaggerJson_Accessible()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        
        var isValidJson = false;
        try
        {
            using var jsonDoc = JsonDocument.Parse(content);
            isValidJson = jsonDoc.RootElement.ValueKind == JsonValueKind.Object;
        }
        catch
        {
            isValidJson = false;
        }
        
        Assert.True(isValidJson, "Swagger JSON endpoint should return valid JSON");
    }

    [Fact]
    public async Task SwaggerJson_ContainsRequiredProperties()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;
        
        Assert.True(root.TryGetProperty("openapi", out _), "Swagger document should contain 'openapi' property");
        Assert.True(root.TryGetProperty("info", out _), "Swagger document should contain 'info' property");
        Assert.True(root.TryGetProperty("paths", out _), "Swagger document should contain 'paths' property");
    }
}
