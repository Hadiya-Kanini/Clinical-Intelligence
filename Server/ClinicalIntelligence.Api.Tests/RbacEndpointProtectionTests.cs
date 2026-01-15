using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using ClinicalIntelligence.Api.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ClinicalIntelligence.Api.Tests;

/// <summary>
/// Integration tests for RBAC endpoint protection (US_033 TASK_001, TASK_004).
/// Validates 401/403/200 behavior for protected endpoints based on authentication and role.
/// </summary>
public class RbacEndpointProtectionTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public RbacEndpointProtectionTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    #region /health/db Endpoint Tests

    [Fact]
    public async Task HealthDb_Unauthenticated_Returns401()
    {
        // Act
        var response = await _client.GetAsync("/health/db");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task HealthDb_AuthenticatedAsStandard_Returns403()
    {
        // Arrange - Login as standard user
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", 
            new LoginRequest("test@example.com", "TestPassword123!"));
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        // Extract cookies for subsequent request
        var cookies = loginResponse.Headers.GetValues("Set-Cookie");
        var cookieHeader = string.Join("; ", cookies.Select(c => c.Split(';')[0]));

        // Act - Access admin endpoint as standard user
        var request = new HttpRequestMessage(HttpMethod.Get, "/health/db");
        request.Headers.Add("Cookie", cookieHeader);
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task HealthDb_AuthenticatedAsAdmin_Returns200()
    {
        // Arrange - Login as admin user
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", 
            new LoginRequest("admin@example.com", "AdminPassword123!"));
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        // Extract cookies for subsequent request
        var cookies = loginResponse.Headers.GetValues("Set-Cookie");
        var cookieHeader = string.Join("; ", cookies.Select(c => c.Split(';')[0]));

        // Act - Access admin endpoint as admin user
        var request = new HttpRequestMessage(HttpMethod.Get, "/health/db");
        request.Headers.Add("Cookie", cookieHeader);
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        Assert.True(jsonDoc.RootElement.TryGetProperty("status", out _));
    }

    #endregion

    #region /health/db/pool Endpoint Tests

    [Fact]
    public async Task HealthDbPool_Unauthenticated_Returns401()
    {
        // Act
        var response = await _client.GetAsync("/health/db/pool");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task HealthDbPool_AuthenticatedAsStandard_Returns403()
    {
        // Arrange - Login as standard user
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", 
            new LoginRequest("test@example.com", "TestPassword123!"));
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        // Extract cookies for subsequent request
        var cookies = loginResponse.Headers.GetValues("Set-Cookie");
        var cookieHeader = string.Join("; ", cookies.Select(c => c.Split(';')[0]));

        // Act - Access admin endpoint as standard user
        var request = new HttpRequestMessage(HttpMethod.Get, "/health/db/pool");
        request.Headers.Add("Cookie", cookieHeader);
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task HealthDbPool_AuthenticatedAsAdmin_ReturnsSuccessOrServiceUnavailable()
    {
        // Arrange - Login as admin user
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", 
            new LoginRequest("admin@example.com", "AdminPassword123!"));
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        // Extract cookies for subsequent request
        var cookies = loginResponse.Headers.GetValues("Set-Cookie");
        var cookieHeader = string.Join("; ", cookies.Select(c => c.Split(';')[0]));

        // Act - Access admin endpoint as admin user
        var request = new HttpRequestMessage(HttpMethod.Get, "/health/db/pool");
        request.Headers.Add("Cookie", cookieHeader);
        var response = await _client.SendAsync(request);

        // Assert - Either 200 (pool available) or 503 (pool not configured for SQLite test DB)
        // The key assertion is that it's NOT 401 or 403
        Assert.True(
            response.StatusCode == HttpStatusCode.OK || 
            response.StatusCode == HttpStatusCode.ServiceUnavailable,
            $"Expected 200 or 503, got {response.StatusCode}");
    }

    #endregion

    #region HTTP Semantics Validation

    [Fact]
    public async Task AdminEndpoint_Unauthenticated_Returns401NotRedirect()
    {
        // Act
        var response = await _client.GetAsync("/health/db");

        // Assert - Should be 401, not a redirect to login
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.False(response.Headers.Location != null, "Should not redirect unauthenticated requests");
    }

    [Fact]
    public async Task AdminEndpoint_WrongRole_Returns403NotRedirect()
    {
        // Arrange - Login as standard user
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", 
            new LoginRequest("test@example.com", "TestPassword123!"));
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var cookies = loginResponse.Headers.GetValues("Set-Cookie");
        var cookieHeader = string.Join("; ", cookies.Select(c => c.Split(';')[0]));

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, "/health/db");
        request.Headers.Add("Cookie", cookieHeader);
        var response = await _client.SendAsync(request);

        // Assert - Should be 403, not a redirect
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.False(response.Headers.Location != null, "Should not redirect forbidden requests");
    }

    #endregion
}
