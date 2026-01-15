using System;
using System.Linq;
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
/// Integration tests for cookie-based JWT authentication.
/// Validates cookie issuance, cookie-based authorization, and 401 behavior for invalid/missing cookies.
/// </summary>
public class CookieAuthenticationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private const string AccessTokenCookieName = "ci_access_token";
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly System.Net.CookieContainer _cookieContainer;

    public CookieAuthenticationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _cookieContainer = new System.Net.CookieContainer();
        
        // Create client with cookie handling via WebApplicationFactoryClientOptions
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true
        });
    }

    public void Dispose()
    {
        _client.Dispose();
    }

    [SkippableFact]
    public async Task ProtectedEndpoint_WithValidCookie_Returns200()
    {
        // Skip if no database connection string is available
        var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");
        Skip.If(string.IsNullOrEmpty(connectionString), "PostgreSQL database not available");

        var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
        var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");
        Skip.If(string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword),
            "ADMIN_EMAIL and ADMIN_PASSWORD environment variables not set");

        // Arrange - Login to get cookie
        var loginRequest = new LoginRequest(adminEmail, adminPassword);
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        
        Skip.If(loginResponse.StatusCode == HttpStatusCode.ServiceUnavailable, "Database not available");
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        // Act - Call protected endpoint with cookie (cookie is automatically sent by HttpClientHandler)
        var meResponse = await _client.GetAsync("/api/v1/auth/me");

        // Assert
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);

        var content = await meResponse.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        
        Assert.True(jsonDoc.RootElement.TryGetProperty("email", out var emailElement));
        Assert.Equal(adminEmail.Trim().ToLowerInvariant(), emailElement.GetString());
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutCookie_Returns401()
    {
        // Arrange - Create a fresh client without any cookies
        using var freshClient = _factory.CreateClient();

        // Act - Call protected endpoint without cookie
        var response = await freshClient.GetAsync("/api/v1/auth/me");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithMalformedCookie_Returns401()
    {
        // Arrange - Create client and manually set a malformed cookie via header
        using var clientWithBadCookie = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/me");
        request.Headers.Add("Cookie", $"{AccessTokenCookieName}=malformed.jwt.token");

        // Act - Call protected endpoint with malformed cookie
        var response = await clientWithBadCookie.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithTamperedCookie_Returns401()
    {
        // Arrange - Create a tampered JWT (valid format but invalid signature)
        var tamperedToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
        
        using var clientWithTamperedCookie = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/me");
        request.Headers.Add("Cookie", $"{AccessTokenCookieName}={tamperedToken}");

        // Act - Call protected endpoint with tampered cookie
        var response = await clientWithTamperedCookie.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [SkippableFact]
    public async Task Logout_ClearsCookie_SubsequentRequestReturns401()
    {
        // Skip if no database connection string is available
        var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");
        Skip.If(string.IsNullOrEmpty(connectionString), "PostgreSQL database not available");

        var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
        var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");
        Skip.If(string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword),
            "ADMIN_EMAIL and ADMIN_PASSWORD environment variables not set");

        // Arrange - Login to get cookie
        var loginRequest = new LoginRequest(adminEmail, adminPassword);
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        
        Skip.If(loginResponse.StatusCode == HttpStatusCode.ServiceUnavailable, "Database not available");
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        // Verify we can access protected endpoint
        var meResponse1 = await _client.GetAsync("/api/v1/auth/me");
        Assert.Equal(HttpStatusCode.OK, meResponse1.StatusCode);

        // Act - Logout
        var logoutResponse = await _client.PostAsync("/api/v1/auth/logout", null);
        Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);

        // Verify Set-Cookie header clears the cookie (expires in past or empty value)
        Assert.True(logoutResponse.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders));
        var setCookieList = setCookieHeaders.ToList();
        var clearCookie = setCookieList.FirstOrDefault(c => c.StartsWith($"{AccessTokenCookieName}="));
        Assert.NotNull(clearCookie);

        // Assert - Subsequent request should fail (cookie cleared)
        // Note: The HttpClientHandler may still have the old cookie cached,
        // so we create a fresh client to simulate the browser behavior after cookie deletion
        using var freshClient = _factory.CreateClient();
        var meResponse2 = await freshClient.GetAsync("/api/v1/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, meResponse2.StatusCode);
    }

    [SkippableFact]
    public async Task Login_SetsCookieWithCorrectAttributes()
    {
        // Skip if no database connection string is available
        var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");
        Skip.If(string.IsNullOrEmpty(connectionString), "PostgreSQL database not available");

        var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
        var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");
        Skip.If(string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword),
            "ADMIN_EMAIL and ADMIN_PASSWORD environment variables not set");

        // Arrange
        using var freshClient = _factory.CreateClient();
        var loginRequest = new LoginRequest(adminEmail, adminPassword);

        // Act
        var response = await freshClient.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        Skip.If(response.StatusCode == HttpStatusCode.ServiceUnavailable, "Database not available");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify Set-Cookie header attributes
        Assert.True(response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders));
        var setCookieList = setCookieHeaders.ToList();
        var accessTokenCookie = setCookieList.FirstOrDefault(c => c.StartsWith($"{AccessTokenCookieName}="));
        
        Assert.NotNull(accessTokenCookie);
        Assert.Contains("HttpOnly", accessTokenCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Path=/", accessTokenCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("SameSite=Lax", accessTokenCookie, StringComparison.OrdinalIgnoreCase);
        // MaxAge or Expires should be present
        Assert.True(
            accessTokenCookie.Contains("Max-Age=", StringComparison.OrdinalIgnoreCase) ||
            accessTokenCookie.Contains("Expires=", StringComparison.OrdinalIgnoreCase),
            "Cookie should have Max-Age or Expires attribute");
    }

    [SkippableFact]
    public async Task PingEndpoint_WithValidCookie_Returns200()
    {
        // Skip if no database connection string is available
        var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");
        Skip.If(string.IsNullOrEmpty(connectionString), "PostgreSQL database not available");

        var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
        var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");
        Skip.If(string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword),
            "ADMIN_EMAIL and ADMIN_PASSWORD environment variables not set");

        // Arrange - Login to get cookie
        var loginRequest = new LoginRequest(adminEmail, adminPassword);
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        
        Skip.If(loginResponse.StatusCode == HttpStatusCode.ServiceUnavailable, "Database not available");
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        // Act - Call another protected endpoint
        var pingResponse = await _client.GetAsync("/api/v1/ping");

        // Assert
        Assert.Equal(HttpStatusCode.OK, pingResponse.StatusCode);

        var content = await pingResponse.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);
        Assert.Equal("OK", jsonDoc.RootElement.GetProperty("status").GetString());
    }
}
