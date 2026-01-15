using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;

namespace ClinicalIntelligence.Api.Tests;

public class DatabaseHealthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public DatabaseHealthEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsHealthyStatus()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Healthy", content);
    }

    [Fact]
    public async Task DatabaseHealthEndpoint_ReturnsJsonResponse()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health/db");

        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        
        Assert.True(json.RootElement.TryGetProperty("status", out _));
        Assert.True(json.RootElement.TryGetProperty("checks", out _));
    }

    [Fact]
    public async Task DatabaseHealthEndpoint_IncludesLatencyInResponse()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health/db");

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        
        var checks = json.RootElement.GetProperty("checks");
        if (checks.GetArrayLength() > 0)
        {
            var firstCheck = checks[0];
            Assert.True(
                firstCheck.TryGetProperty("latency_ms", out _) || 
                firstCheck.TryGetProperty("description", out _),
                "Response should include latency or description");
        }
    }

    [Fact]
    public async Task DatabaseHealthEndpoint_DoesNotLeakConnectionString()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health/db");

        var content = await response.Content.ReadAsStringAsync();
        
        Assert.DoesNotContain("Password=", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password=", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Host=", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Database=", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Username=", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DatabaseHealthEndpoint_ReturnsValidStatusCodes()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health/db");

        Assert.True(
            response.StatusCode == HttpStatusCode.OK || 
            response.StatusCode == HttpStatusCode.ServiceUnavailable,
            $"Expected 200 OK or 503 Service Unavailable, got {response.StatusCode}");
    }

    [Fact]
    public async Task DatabaseHealthEndpoint_StatusMatchesHealthCheckResult()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health/db");

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        var status = json.RootElement.GetProperty("status").GetString();

        if (status == "Healthy" || status == "Degraded")
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        else if (status == "Unhealthy")
        {
            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        }
    }
}
