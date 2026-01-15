using ClinicalIntelligence.Api.Configuration;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Xunit;

namespace ClinicalIntelligence.Api.Tests.Integration;

/// <summary>
/// Integration tests for CORS policy enforcement (US_023).
/// Tests that only configured origins receive CORS headers, credentials are supported,
/// preflight requests are handled correctly, and disallowed origins are rejected.
/// </summary>
public class CorsPolicyIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    
    private const string AllowedOrigin = "http://localhost:5173";
    private const string DisallowedOrigin = "https://evil.example.com";
    private const string AccessControlAllowOrigin = "Access-Control-Allow-Origin";
    private const string AccessControlAllowCredentials = "Access-Control-Allow-Credentials";
    private const string AccessControlAllowMethods = "Access-Control-Allow-Methods";
    private const string AccessControlAllowHeaders = "Access-Control-Allow-Headers";
    private const string AccessControlRequestMethod = "Access-Control-Request-Method";
    private const string AccessControlRequestHeaders = "Access-Control-Request-Headers";

    public CorsPolicyIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetHealth_WithAllowedOrigin_ReturnsAccessControlAllowOriginHeader()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("Origin", AllowedOrigin);

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains(AccessControlAllowOrigin),
            $"Response should contain {AccessControlAllowOrigin} header for allowed origin");
        
        var allowOriginValue = response.Headers.GetValues(AccessControlAllowOrigin).FirstOrDefault();
        Assert.Equal(AllowedOrigin, allowOriginValue);
    }

    [Fact]
    public async Task GetHealth_WithAllowedOrigin_ReturnsAccessControlAllowCredentialsTrue()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("Origin", AllowedOrigin);

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains(AccessControlAllowCredentials),
            $"Response should contain {AccessControlAllowCredentials} header for credentialed requests");
        
        var allowCredentialsValue = response.Headers.GetValues(AccessControlAllowCredentials).FirstOrDefault();
        Assert.Equal("true", allowCredentialsValue);
    }

    [Fact]
    public async Task GetHealth_WithDisallowedOrigin_DoesNotReturnAccessControlAllowOriginHeader()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("Origin", DisallowedOrigin);

        // Act
        var response = await client.SendAsync(request);

        // Assert
        // Note: The endpoint still returns 200 OK, but CORS headers should be absent
        // The browser would block the response due to missing CORS headers
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(response.Headers.Contains(AccessControlAllowOrigin),
            $"Response should NOT contain {AccessControlAllowOrigin} header for disallowed origin");
    }

    [Fact]
    public async Task GetHealth_WithDisallowedOrigin_DoesNotReturnAccessControlAllowCredentialsHeader()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("Origin", DisallowedOrigin);

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(response.Headers.Contains(AccessControlAllowCredentials),
            $"Response should NOT contain {AccessControlAllowCredentials} header for disallowed origin");
    }

    [Fact]
    public async Task OptionsPreflight_WithAllowedOrigin_ReturnsPreflightHeaders()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/v1/ping");
        request.Headers.Add("Origin", AllowedOrigin);
        request.Headers.Add(AccessControlRequestMethod, "GET");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        // Preflight may return 200 or 204 depending on configuration
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent,
            $"Preflight response should return 200 or 204, got {response.StatusCode}");
        
        Assert.True(response.Headers.Contains(AccessControlAllowOrigin),
            $"Preflight response should contain {AccessControlAllowOrigin} header");
        
        var allowOriginValue = response.Headers.GetValues(AccessControlAllowOrigin).FirstOrDefault();
        Assert.Equal(AllowedOrigin, allowOriginValue);
        
        Assert.True(response.Headers.Contains(AccessControlAllowMethods),
            $"Preflight response should contain {AccessControlAllowMethods} header");
    }

    [Fact]
    public async Task OptionsPreflight_WithAllowedOriginAndRequestHeaders_ReturnsAllowHeadersHeader()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/v1/auth/login");
        request.Headers.Add("Origin", AllowedOrigin);
        request.Headers.Add(AccessControlRequestMethod, "POST");
        request.Headers.Add(AccessControlRequestHeaders, "content-type");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent,
            $"Preflight response should return 200 or 204, got {response.StatusCode}");
        
        Assert.True(response.Headers.Contains(AccessControlAllowHeaders),
            $"Preflight response should contain {AccessControlAllowHeaders} header when request headers are specified");
    }

    [Fact]
    public async Task OptionsPreflight_WithDisallowedOrigin_DoesNotReturnCorsHeaders()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/v1/ping");
        request.Headers.Add("Origin", DisallowedOrigin);
        request.Headers.Add(AccessControlRequestMethod, "GET");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        // The preflight request may still return a response, but CORS headers should be absent
        Assert.False(response.Headers.Contains(AccessControlAllowOrigin),
            $"Preflight response should NOT contain {AccessControlAllowOrigin} header for disallowed origin");
        Assert.False(response.Headers.Contains(AccessControlAllowMethods),
            $"Preflight response should NOT contain {AccessControlAllowMethods} header for disallowed origin");
    }

    [Fact]
    public async Task GetHealth_WithoutOriginHeader_DoesNotReturnCorsHeaders()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        // No Origin header - same-origin request

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        // CORS headers are only added for cross-origin requests (when Origin header is present)
        Assert.False(response.Headers.Contains(AccessControlAllowOrigin),
            $"Response should NOT contain {AccessControlAllowOrigin} header for same-origin requests");
    }

    [Fact]
    public async Task GetApiEndpoint_WithAllowedOrigin_ReturnsCorrectCorsHeaders()
    {
        // Arrange - Test with an API endpoint that requires auth (will return 401 but should still have CORS headers)
        var client = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/ping");
        request.Headers.Add("Origin", AllowedOrigin);

        // Act
        var response = await client.SendAsync(request);

        // Assert
        // Even for 401 responses, CORS headers should be present for allowed origins
        Assert.True(response.Headers.Contains(AccessControlAllowOrigin),
            $"Response should contain {AccessControlAllowOrigin} header even for protected endpoints");
        
        var allowOriginValue = response.Headers.GetValues(AccessControlAllowOrigin).FirstOrDefault();
        Assert.Equal(AllowedOrigin, allowOriginValue);
        
        Assert.True(response.Headers.Contains(AccessControlAllowCredentials),
            $"Response should contain {AccessControlAllowCredentials} header");
    }

    [Theory]
    [InlineData("http://localhost:3000")]
    [InlineData("https://localhost:5173")]
    [InlineData("https://localhost:3000")]
    public async Task GetHealth_WithOtherAllowedOrigins_ReturnsAccessControlAllowOriginHeader(string origin)
    {
        // Arrange - Test all default development origins
        var client = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("Origin", origin);

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains(AccessControlAllowOrigin),
            $"Response should contain {AccessControlAllowOrigin} header for allowed origin {origin}");
        
        var allowOriginValue = response.Headers.GetValues(AccessControlAllowOrigin).FirstOrDefault();
        Assert.Equal(origin, allowOriginValue);
    }

    [Theory]
    [InlineData("https://malicious-site.com")]
    [InlineData("http://attacker.local")]
    [InlineData("https://phishing.example.org")]
    public async Task GetHealth_WithVariousDisallowedOrigins_DoesNotReturnCorsHeaders(string origin)
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("Origin", origin);

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(response.Headers.Contains(AccessControlAllowOrigin),
            $"Response should NOT contain {AccessControlAllowOrigin} header for disallowed origin {origin}");
    }
}
