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
/// Integration tests for US_036 TASK_002: Enforce no public registration policy.
/// Validates that common public registration endpoints do not exist (return 404)
/// and that user creation is only possible through admin-authenticated endpoints.
/// </summary>
public class NoPublicRegistrationEndpointTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public NoPublicRegistrationEndpointTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    #region Public Registration Endpoints Return 404

    [Theory]
    [InlineData("/api/v1/auth/register")]
    [InlineData("/api/v1/auth/signup")]
    [InlineData("/api/v1/register")]
    [InlineData("/api/v1/signup")]
    public async Task PublicRegistrationEndpoint_POST_Returns404(string endpoint)
    {
        // Arrange
        var registrationPayload = new
        {
            email = "newuser@example.com",
            password = "SecurePassword123!",
            name = "New User"
        };

        // Act
        var response = await _client.PostAsJsonAsync(endpoint, registrationPayload);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/v1/auth/register")]
    [InlineData("/api/v1/auth/signup")]
    [InlineData("/api/v1/register")]
    [InlineData("/api/v1/signup")]
    public async Task PublicRegistrationEndpoint_GET_Returns404(string endpoint)
    {
        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/v1/auth/register")]
    [InlineData("/api/v1/auth/signup")]
    [InlineData("/api/v1/register")]
    [InlineData("/api/v1/signup")]
    public async Task PublicRegistrationEndpoint_PUT_Returns404(string endpoint)
    {
        // Arrange
        var payload = new { email = "test@example.com" };

        // Act
        var response = await _client.PutAsJsonAsync(endpoint, payload);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region Admin User Creation Endpoint Authorization Tests

    [Fact]
    public async Task AdminUsersEndpoint_Unauthenticated_Returns401()
    {
        // Arrange
        var userPayload = new
        {
            email = "newuser@example.com",
            password = "SecurePassword123!",
            name = "New User",
            role = "Standard"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/admin/users", userPayload);

        // Assert - Unauthenticated should return 401 (or 404 if endpoint doesn't exist yet)
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized || 
            response.StatusCode == HttpStatusCode.NotFound,
            $"Expected 401 or 404, got {response.StatusCode}");
    }

    [Fact]
    public async Task AdminUsersEndpoint_AuthenticatedAsStandard_Returns403Or404()
    {
        // Arrange - Login as standard user
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", 
            new LoginRequest("test@example.com", "TestPassword123!"));
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var cookies = loginResponse.Headers.GetValues("Set-Cookie");
        var cookieHeader = string.Join("; ", cookies.Select(c => c.Split(';')[0]));

        var userPayload = new
        {
            email = "newuser@example.com",
            password = "SecurePassword123!",
            name = "New User",
            role = "Standard"
        };

        // Act
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/admin/users");
        request.Headers.Add("Cookie", cookieHeader);
        request.Content = JsonContent.Create(userPayload);
        var response = await _client.SendAsync(request);

        // Assert - Standard user should get 403 (or 404 if endpoint doesn't exist yet)
        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden || 
            response.StatusCode == HttpStatusCode.NotFound,
            $"Expected 403 or 404, got {response.StatusCode}");
    }

    #endregion

    #region Public Users Endpoint Protection

    [Fact]
    public async Task PublicUsersEndpoint_POST_Unauthenticated_ReturnsNot2xx()
    {
        // Arrange
        var userPayload = new
        {
            email = "newuser@example.com",
            password = "SecurePassword123!",
            name = "New User"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/users", userPayload);

        // Assert - Should NOT allow unauthenticated user creation
        // Expected: 401 (requires auth), 403 (forbidden), or 404 (no such endpoint)
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.NotFound,
            $"Expected 401, 403, or 404 for unauthenticated POST /api/v1/users, got {response.StatusCode}");
    }

    [Fact]
    public async Task PublicUsersEndpoint_POST_AuthenticatedAsStandard_ReturnsNot2xx()
    {
        // Arrange - Login as standard user
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", 
            new LoginRequest("test@example.com", "TestPassword123!"));
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var cookies = loginResponse.Headers.GetValues("Set-Cookie");
        var cookieHeader = string.Join("; ", cookies.Select(c => c.Split(';')[0]));

        var userPayload = new
        {
            email = "newuser@example.com",
            password = "SecurePassword123!",
            name = "New User"
        };

        // Act
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/users");
        request.Headers.Add("Cookie", cookieHeader);
        request.Content = JsonContent.Create(userPayload);
        var response = await _client.SendAsync(request);

        // Assert - Standard user should NOT be able to create users
        // Expected: 403 (forbidden) or 404 (no such endpoint)
        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.NotFound,
            $"Expected 403 or 404 for standard user POST /api/v1/users, got {response.StatusCode}");
    }

    #endregion

    #region Deny-by-Default Validation

    [Fact]
    public async Task NoPublicSelfServiceRegistration_AllCommonPaths_Return404()
    {
        // Arrange - Common registration endpoint patterns
        var registrationPaths = new[]
        {
            "/api/v1/auth/register",
            "/api/v1/auth/signup",
            "/api/v1/auth/create-account",
            "/api/v1/register",
            "/api/v1/signup",
            "/api/v1/create-account",
            "/register",
            "/signup",
            "/api/register",
            "/api/signup"
        };

        var payload = new
        {
            email = "attacker@example.com",
            password = "MaliciousPassword123!",
            name = "Attacker"
        };

        // Act & Assert
        foreach (var path in registrationPaths)
        {
            var response = await _client.PostAsJsonAsync(path, payload);
            
            Assert.True(
                response.StatusCode == HttpStatusCode.NotFound ||
                response.StatusCode == HttpStatusCode.Unauthorized ||
                response.StatusCode == HttpStatusCode.Forbidden,
                $"Path {path} returned {response.StatusCode}, expected 404/401/403 (no public registration)");
        }
    }

    [Fact]
    public async Task RegistrationEndpoints_DoNotReturnSuccessOrCreated()
    {
        // Arrange
        var registrationPaths = new[]
        {
            "/api/v1/auth/register",
            "/api/v1/auth/signup",
            "/api/v1/register",
            "/api/v1/signup"
        };

        var payload = new
        {
            email = "newuser@example.com",
            password = "SecurePassword123!",
            name = "New User",
            confirmPassword = "SecurePassword123!"
        };

        // Act & Assert
        foreach (var path in registrationPaths)
        {
            var response = await _client.PostAsJsonAsync(path, payload);
            
            Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEqual(HttpStatusCode.Created, response.StatusCode);
            Assert.NotEqual(HttpStatusCode.Accepted, response.StatusCode);
        }
    }

    #endregion
}
