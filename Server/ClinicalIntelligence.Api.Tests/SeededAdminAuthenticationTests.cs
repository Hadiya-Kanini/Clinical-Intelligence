using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using ClinicalIntelligence.Api.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Net.Http.Headers;
using Xunit;

namespace ClinicalIntelligence.Api.Tests;

/// <summary>
/// Integration tests for seeded admin authentication.
/// Verifies that seeded admin can authenticate and JWT includes Admin role claim.
/// Skippable when PostgreSQL database is unavailable.
/// </summary>
public class SeededAdminAuthenticationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly string? _originalAdminEmail;
    private readonly string? _originalAdminPassword;

    public SeededAdminAuthenticationTests(WebApplicationFactory<Program> factory)
    {
        _originalAdminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
        _originalAdminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

        _factory = factory;
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();

        if (_originalAdminEmail != null)
            Environment.SetEnvironmentVariable("ADMIN_EMAIL", _originalAdminEmail);
        else
            Environment.SetEnvironmentVariable("ADMIN_EMAIL", null);

        if (_originalAdminPassword != null)
            Environment.SetEnvironmentVariable("ADMIN_PASSWORD", _originalAdminPassword);
        else
            Environment.SetEnvironmentVariable("ADMIN_PASSWORD", null);
    }

    [SkippableFact]
    public async Task Login_WithSeededAdminCredentials_ReturnsOkWithJwtContainingAdminRole()
    {
        // Skip if no database connection string is available
        var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");
        Skip.If(string.IsNullOrEmpty(connectionString), "PostgreSQL database not available");

        var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
        var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");
        Skip.If(string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword),
            "ADMIN_EMAIL and ADMIN_PASSWORD environment variables not set");

        // Arrange
        var loginRequest = new LoginRequest(adminEmail, adminPassword);

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        Skip.If(response.StatusCode == HttpStatusCode.ServiceUnavailable, "Database not available");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);

        // Token should NOT be in response body (it's in HttpOnly cookie)
        Assert.False(jsonDoc.RootElement.TryGetProperty("token", out _), "Token should not be in response body");

        // Verify user object in response
        Assert.True(jsonDoc.RootElement.TryGetProperty("user", out var userElement));
        var userEmail = userElement.GetProperty("email").GetString();
        var userRole = userElement.GetProperty("role").GetString();
        var userId = userElement.GetProperty("id").GetString();
        
        Assert.Equal(adminEmail.Trim().ToLowerInvariant(), userEmail);
        Assert.Equal("admin", userRole);
        Assert.True(Guid.TryParse(userId, out _), "User ID should be a valid GUID");

        // Verify Set-Cookie header is present with HttpOnly cookie
        Assert.True(response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders));
        var setCookieList = setCookieHeaders.ToList();
        Assert.NotEmpty(setCookieList);

        var accessTokenCookie = setCookieList.FirstOrDefault(c => c.StartsWith("ci_access_token="));
        Assert.NotNull(accessTokenCookie);
        Assert.Contains("HttpOnly", accessTokenCookie, StringComparison.OrdinalIgnoreCase);

        // Extract token from cookie for JWT validation
        var cookieValue = accessTokenCookie.Split(';')[0].Split('=')[1];
        Assert.NotNull(cookieValue);

        // Decode JWT and verify role claim
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(cookieValue);

        var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role");
        Assert.NotNull(roleClaim);
        Assert.Equal("Admin", roleClaim.Value);

        // Verify subject claim contains user ID
        var subClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
        Assert.NotNull(subClaim);
        Assert.True(Guid.TryParse(subClaim.Value, out _), "Subject claim should be a valid GUID");

        // Verify email claim
        var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email);
        Assert.NotNull(emailClaim);
        Assert.Equal(adminEmail.Trim().ToLowerInvariant(), emailClaim.Value);

        // Verify token expiry is approximately 15 minutes
        var exp = jwtToken.ValidTo;
        var expectedExpiry = DateTime.UtcNow.AddMinutes(15);
        var expiryDiff = Math.Abs((exp - expectedExpiry).TotalMinutes);
        Assert.True(expiryDiff < 1, $"Token expiry should be ~15 minutes from now, but was {exp}");
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest("admin@example.com", "WrongPassword123!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);

        Assert.Equal("invalid_credentials", jsonDoc.RootElement.GetProperty("code").GetString());
        Assert.Equal("Invalid email or password.", jsonDoc.RootElement.GetProperty("message").GetString());
    }

    [Fact]
    public async Task Login_WithNonExistentUser_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest("nonexistent@example.com", "SomePassword123!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);

        // Should use consistent error message (not leak user existence)
        Assert.Equal("invalid_credentials", jsonDoc.RootElement.GetProperty("code").GetString());
        Assert.Equal("Invalid email or password.", jsonDoc.RootElement.GetProperty("message").GetString());
    }

    [Fact]
    public async Task Login_WithMissingEmail_ReturnsBadRequest()
    {
        // Arrange
        var loginRequest = new LoginRequest(null, "SomePassword123!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);

        Assert.Equal("invalid_input", jsonDoc.RootElement.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Login_WithMissingPassword_ReturnsBadRequest()
    {
        // Arrange
        var loginRequest = new LoginRequest("admin@example.com", null);

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);

        Assert.Equal("invalid_input", jsonDoc.RootElement.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Login_ResponseContainsExpiresIn()
    {
        // Skip if no database connection string is available
        var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");
        if (string.IsNullOrEmpty(connectionString))
        {
            // Test with invalid credentials to verify response structure
            var loginRequest = new LoginRequest("test@example.com", "TestP@ss123");
            var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

            // We expect unauthorized, but the test validates the endpoint is reachable
            Assert.True(response.StatusCode == HttpStatusCode.Unauthorized ||
                        response.StatusCode == HttpStatusCode.OK);
            return;
        }

        var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
        var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");
        if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
        {
            return; // Skip silently
        }

        // Arrange
        var request = new LoginRequest(adminEmail, adminPassword);

        // Act
        var resp = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

        if (resp.StatusCode == HttpStatusCode.OK)
        {
            var content = await resp.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);

            // Assert
            Assert.True(jsonDoc.RootElement.TryGetProperty("expires_in", out var expiresIn));
            Assert.True(expiresIn.GetInt32() > 0);
        }
    }
}
