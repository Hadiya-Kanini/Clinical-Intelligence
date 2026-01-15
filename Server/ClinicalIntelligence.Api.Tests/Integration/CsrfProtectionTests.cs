using ClinicalIntelligence.Api.Contracts;
using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Domain.Models;
using ClinicalIntelligence.Api.Middleware;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace ClinicalIntelligence.Api.Tests.Integration;

/// <summary>
/// Integration tests for CSRF protection (US_022).
/// Tests CSRF token issuance, enforcement on state-changing requests,
/// per-session isolation, and 403 behavior for invalid/missing tokens.
/// </summary>
public class CsrfProtectionTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private const string TestEmail = "csrf-test@example.com";
    private const string TestPassword = "TestPassword123!";
    private const string CsrfHeaderName = "X-CSRF-TOKEN";

    public CsrfProtectionTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private bool IsPostgreSqlAvailable()
    {
        try
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            return dbContext.Database.CanConnect();
        }
        catch
        {
            return false;
        }
    }

    private async Task<User> EnsureTestUserExistsAsync(ApplicationDbContext dbContext)
    {
        var user = await dbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == TestEmail);

        if (user == null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                Email = TestEmail,
                Name = "CSRF Test User",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(TestPassword),
                Role = "Standard",
                Status = "Active",
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
        }
        return user;
    }

    private async Task<(HttpClient client, string? csrfToken)> LoginAndGetClientWithCsrfAsync(ApplicationDbContext dbContext)
    {
        await EnsureTestUserExistsAsync(dbContext);

        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true
        });

        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = TestEmail,
            password = TestPassword
        });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        // Fetch CSRF token
        var csrfResponse = await client.GetAsync("/api/v1/auth/csrf");
        string? csrfToken = null;
        if (csrfResponse.IsSuccessStatusCode)
        {
            var csrfData = await csrfResponse.Content.ReadFromJsonAsync<CsrfTokenResponse>();
            csrfToken = csrfData?.Token;
        }

        return (client, csrfToken);
    }

    [Fact]
    public async Task CsrfEndpoint_RequiresAuthorization_Returns401WhenUnauthenticated()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/auth/csrf");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CsrfEndpoint_AuthenticatedUser_ReturnsToken()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var (client, csrfToken) = await LoginAndGetClientWithCsrfAsync(dbContext);

        // Assert
        Assert.NotNull(csrfToken);
        Assert.NotEmpty(csrfToken);
    }

    [Fact]
    public async Task StateChangingRequest_WithoutCsrfToken_Returns403()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await EnsureTestUserExistsAsync(dbContext);

        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true
        });

        // Login to establish session
        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = TestEmail,
            password = TestPassword
        });
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        // Act - attempt logout without CSRF token
        var logoutResponse = await client.PostAsync("/api/v1/auth/logout", null);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, logoutResponse.StatusCode);

        var content = await logoutResponse.Content.ReadAsStringAsync();
        Assert.Contains("csrf", content.ToLower());
    }

    [Fact]
    public async Task StateChangingRequest_WithValidCsrfToken_Returns200()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var (client, csrfToken) = await LoginAndGetClientWithCsrfAsync(dbContext);

        Assert.NotNull(csrfToken);

        // Act - logout with valid CSRF token
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/logout");
        request.Headers.Add(CsrfHeaderName, csrfToken);

        var logoutResponse = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);

        var content = await logoutResponse.Content.ReadAsStringAsync();
        Assert.Contains("logged_out", content);
    }

    [Fact]
    public async Task StateChangingRequest_WithInvalidCsrfToken_Returns403()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var (client, _) = await LoginAndGetClientWithCsrfAsync(dbContext);

        // Act - logout with invalid CSRF token
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/logout");
        request.Headers.Add(CsrfHeaderName, "invalid-token-value");

        var logoutResponse = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, logoutResponse.StatusCode);

        var content = await logoutResponse.Content.ReadAsStringAsync();
        Assert.Contains("csrf", content.ToLower());
    }

    [Fact]
    public async Task CsrfToken_IsPerSession_DifferentSessionsHaveDifferentTokens()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Create two separate test users for independent sessions
        var user1Email = "csrf-test-user1@example.com";
        var user2Email = "csrf-test-user2@example.com";

        var user1 = await dbContext.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Email == user1Email);
        if (user1 == null)
        {
            user1 = new User
            {
                Id = Guid.NewGuid(),
                Email = user1Email,
                Name = "CSRF Test User 1",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(TestPassword),
                Role = "Standard",
                Status = "Active",
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            dbContext.Users.Add(user1);
        }

        var user2 = await dbContext.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Email == user2Email);
        if (user2 == null)
        {
            user2 = new User
            {
                Id = Guid.NewGuid(),
                Email = user2Email,
                Name = "CSRF Test User 2",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(TestPassword),
                Role = "Standard",
                Status = "Active",
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            dbContext.Users.Add(user2);
        }
        await dbContext.SaveChangesAsync();

        // Create two separate clients (separate sessions)
        var client1 = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        var client2 = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });

        // Login with both clients
        var login1 = await client1.PostAsJsonAsync("/api/v1/auth/login", new { email = user1Email, password = TestPassword });
        var login2 = await client2.PostAsJsonAsync("/api/v1/auth/login", new { email = user2Email, password = TestPassword });
        Assert.Equal(HttpStatusCode.OK, login1.StatusCode);
        Assert.Equal(HttpStatusCode.OK, login2.StatusCode);

        // Get CSRF tokens for both sessions
        var csrf1Response = await client1.GetAsync("/api/v1/auth/csrf");
        var csrf2Response = await client2.GetAsync("/api/v1/auth/csrf");
        Assert.True(csrf1Response.IsSuccessStatusCode);
        Assert.True(csrf2Response.IsSuccessStatusCode);

        var csrf1Data = await csrf1Response.Content.ReadFromJsonAsync<CsrfTokenResponse>();
        var csrf2Data = await csrf2Response.Content.ReadFromJsonAsync<CsrfTokenResponse>();

        // Assert - tokens should be different (per-session)
        Assert.NotNull(csrf1Data?.Token);
        Assert.NotNull(csrf2Data?.Token);
        Assert.NotEqual(csrf1Data.Token, csrf2Data.Token);
    }

    [Fact]
    public async Task CsrfToken_CannotBeReusedAcrossSessions()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Create two separate test users
        var userAEmail = "csrf-cross-session-a@example.com";
        var userBEmail = "csrf-cross-session-b@example.com";

        var userA = await dbContext.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Email == userAEmail);
        if (userA == null)
        {
            userA = new User
            {
                Id = Guid.NewGuid(),
                Email = userAEmail,
                Name = "CSRF Cross Session A",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(TestPassword),
                Role = "Standard",
                Status = "Active",
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            dbContext.Users.Add(userA);
        }

        var userB = await dbContext.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Email == userBEmail);
        if (userB == null)
        {
            userB = new User
            {
                Id = Guid.NewGuid(),
                Email = userBEmail,
                Name = "CSRF Cross Session B",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(TestPassword),
                Role = "Standard",
                Status = "Active",
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            dbContext.Users.Add(userB);
        }
        await dbContext.SaveChangesAsync();

        // Create two separate clients
        var clientA = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        var clientB = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });

        // Login with both clients
        var loginA = await clientA.PostAsJsonAsync("/api/v1/auth/login", new { email = userAEmail, password = TestPassword });
        var loginB = await clientB.PostAsJsonAsync("/api/v1/auth/login", new { email = userBEmail, password = TestPassword });
        Assert.Equal(HttpStatusCode.OK, loginA.StatusCode);
        Assert.Equal(HttpStatusCode.OK, loginB.StatusCode);

        // Get CSRF token from session A
        var csrfAResponse = await clientA.GetAsync("/api/v1/auth/csrf");
        Assert.True(csrfAResponse.IsSuccessStatusCode);
        var csrfAData = await csrfAResponse.Content.ReadFromJsonAsync<CsrfTokenResponse>();
        Assert.NotNull(csrfAData?.Token);

        // Act - try to use session A's CSRF token with session B's cookie
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/logout");
        request.Headers.Add(CsrfHeaderName, csrfAData.Token);

        var response = await clientB.SendAsync(request);

        // Assert - should fail because token doesn't match session B
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetRequest_DoesNotRequireCsrfToken()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await EnsureTestUserExistsAsync(dbContext);

        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true
        });

        // Login
        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = TestEmail,
            password = TestPassword
        });
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        // Act - GET request without CSRF token
        var meResponse = await client.GetAsync("/api/v1/auth/me");

        // Assert - GET requests should not require CSRF
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);
    }

    [Fact]
    public async Task LoginEndpoint_IsExemptFromCsrf()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await EnsureTestUserExistsAsync(dbContext);

        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true
        });

        // Act - login without CSRF token (should be exempt)
        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = TestEmail,
            password = TestPassword
        });

        // Assert - login should succeed without CSRF token
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
    }

    [Fact]
    public async Task CsrfToken_RefreshOnEachRequest_ReturnsNewToken()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var (client, firstToken) = await LoginAndGetClientWithCsrfAsync(dbContext);

        Assert.NotNull(firstToken);

        // Act - request another CSRF token
        var secondCsrfResponse = await client.GetAsync("/api/v1/auth/csrf");
        Assert.True(secondCsrfResponse.IsSuccessStatusCode);
        var secondCsrfData = await secondCsrfResponse.Content.ReadFromJsonAsync<CsrfTokenResponse>();

        // Assert - new token should be different (security best practice)
        Assert.NotNull(secondCsrfData?.Token);
        Assert.NotEqual(firstToken, secondCsrfData.Token);
    }

    [Fact]
    public async Task CsrfToken_MostRecentTokenIsValid()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var (client, firstToken) = await LoginAndGetClientWithCsrfAsync(dbContext);

        Assert.NotNull(firstToken);

        // Get a second token (invalidates the first)
        var secondCsrfResponse = await client.GetAsync("/api/v1/auth/csrf");
        var secondCsrfData = await secondCsrfResponse.Content.ReadFromJsonAsync<CsrfTokenResponse>();
        Assert.NotNull(secondCsrfData?.Token);

        // Act - use the most recent token
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/logout");
        request.Headers.Add(CsrfHeaderName, secondCsrfData.Token);

        var logoutResponse = await client.SendAsync(request);

        // Assert - most recent token should work
        Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);
    }

    [Fact]
    public async Task CsrfToken_OldTokenInvalidatedAfterRefresh()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var (client, firstToken) = await LoginAndGetClientWithCsrfAsync(dbContext);

        Assert.NotNull(firstToken);

        // Get a second token (invalidates the first)
        var secondCsrfResponse = await client.GetAsync("/api/v1/auth/csrf");
        var secondCsrfData = await secondCsrfResponse.Content.ReadFromJsonAsync<CsrfTokenResponse>();
        Assert.NotNull(secondCsrfData?.Token);

        // Act - try to use the old (first) token
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/logout");
        request.Headers.Add(CsrfHeaderName, firstToken);

        var logoutResponse = await client.SendAsync(request);

        // Assert - old token should be rejected
        Assert.Equal(HttpStatusCode.Forbidden, logoutResponse.StatusCode);
    }
}
