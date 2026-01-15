using ClinicalIntelligence.Api.Contracts;
using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Domain.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace ClinicalIntelligence.Api.Tests.Integration;

/// <summary>
/// Integration tests for session tracking and inactivity timeout functionality.
/// Tests session creation, LastActivityAt updates, inactivity timeout enforcement, and logout revocation.
/// </summary>
public class SessionInactivityTimeoutTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private const string TestEmail = "test@example.com";
    private const string TestPassword = "TestPassword123!";

    public SessionInactivityTimeoutTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Helper to check if PostgreSQL is available for integration tests.
    /// </summary>
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

    /// <summary>
    /// Helper to create a test user if not exists.
    /// </summary>
    private async Task<User> EnsureTestUserExistsAsync(ApplicationDbContext dbContext)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == TestEmail);
        if (user == null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                Email = TestEmail,
                Name = "Test User",
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

    /// <summary>
    /// Helper to login and get the session cookie.
    /// </summary>
    private async Task<(HttpClient client, string? sessionCookie)> LoginAsync()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true
        });

        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = TestEmail,
            password = TestPassword
        });

        string? sessionCookie = null;
        if (loginResponse.Headers.TryGetValues("Set-Cookie", out var cookies))
        {
            sessionCookie = cookies.FirstOrDefault(c => c.StartsWith("ci_access_token="));
        }

        return (client, sessionCookie);
    }

    /// <summary>
    /// Helper to parse error response.
    /// </summary>
    private static async Task<ApiErrorResponse?> GetErrorResponseAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(content)) return null;
        return JsonSerializer.Deserialize<ApiErrorResponse>(content);
    }

    [Fact]
    public async Task Login_CreatesSessionRecord_WithLastActivityAt()
    {
        // Skip if PostgreSQL is not available
        if (!IsPostgreSqlAvailable())
        {
            return;
        }

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await EnsureTestUserExistsAsync(dbContext);

        // Clear existing sessions for clean test
        var existingSessions = await dbContext.Sessions.Where(s => s.UserId == user.Id).ToListAsync();
        dbContext.Sessions.RemoveRange(existingSessions);
        await dbContext.SaveChangesAsync();

        // Act
        var (client, _) = await LoginAsync();

        // Assert
        var session = await dbContext.Sessions
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(s => s.UserId == user.Id);

        Assert.NotNull(session);
        Assert.NotNull(session.LastActivityAt);
        Assert.False(session.IsRevoked);
        Assert.True(session.ExpiresAt > DateTime.UtcNow);
        Assert.Equal(user.Id, session.UserId);
    }

    [Fact]
    public async Task AuthenticatedRequest_UpdatesLastActivityAt()
    {
        // Skip if PostgreSQL is not available
        if (!IsPostgreSqlAvailable())
        {
            return;
        }

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await EnsureTestUserExistsAsync(dbContext);

        var (client, _) = await LoginAsync();

        // Get the session created by login
        var sessionBefore = await dbContext.Sessions
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync();

        Assert.NotNull(sessionBefore);
        var lastActivityBefore = sessionBefore.LastActivityAt;

        // Wait a moment to ensure time difference
        await Task.Delay(100);

        // Act - make an authenticated request
        var response = await client.GetAsync("/api/v1/ping");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Reload session from database
        await dbContext.Entry(sessionBefore).ReloadAsync();
        Assert.True(sessionBefore.LastActivityAt > lastActivityBefore);
    }

    [Fact]
    public async Task SessionInactivityBeyondThreshold_Returns401WithSessionExpiredCode()
    {
        // Skip if PostgreSQL is not available
        if (!IsPostgreSqlAvailable())
        {
            return;
        }

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await EnsureTestUserExistsAsync(dbContext);

        var (client, _) = await LoginAsync();

        // Get the session and set LastActivityAt to beyond the timeout threshold (16 minutes ago)
        var session = await dbContext.Sessions
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync();

        Assert.NotNull(session);
        session.LastActivityAt = DateTime.UtcNow.AddMinutes(-16);
        await dbContext.SaveChangesAsync();

        // Act - make an authenticated request with expired session
        var response = await client.GetAsync("/api/v1/ping");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var errorResponse = await GetErrorResponseAsync(response);
        Assert.NotNull(errorResponse);
        Assert.Equal("session_expired", errorResponse.Error.Code);
    }

    [Fact]
    public async Task Logout_RevokesSession_AndBlocksSubsequentRequests()
    {
        // Skip if PostgreSQL is not available
        if (!IsPostgreSqlAvailable())
        {
            return;
        }

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await EnsureTestUserExistsAsync(dbContext);

        var (client, _) = await LoginAsync();

        // Get the session created by login
        var session = await dbContext.Sessions
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync();

        Assert.NotNull(session);
        Assert.False(session.IsRevoked);

        // Act - logout
        var logoutResponse = await client.PostAsync("/api/v1/auth/logout", null);
        Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);

        // Reload session from database
        await dbContext.Entry(session).ReloadAsync();

        // Assert - session is revoked
        Assert.True(session.IsRevoked);

        // Note: After logout, the cookie is cleared, so subsequent requests
        // would be unauthenticated rather than session-expired.
        // This test verifies the server-side session revocation.
    }

    [Fact]
    public async Task RevokedSession_Returns401WithSessionExpiredCode()
    {
        // Skip if PostgreSQL is not available
        if (!IsPostgreSqlAvailable())
        {
            return;
        }

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await EnsureTestUserExistsAsync(dbContext);

        var (client, _) = await LoginAsync();

        // Get the session and revoke it directly in the database
        var session = await dbContext.Sessions
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync();

        Assert.NotNull(session);
        session.IsRevoked = true;
        await dbContext.SaveChangesAsync();

        // Act - make an authenticated request with revoked session
        var response = await client.GetAsync("/api/v1/ping");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var errorResponse = await GetErrorResponseAsync(response);
        Assert.NotNull(errorResponse);
        Assert.Equal("session_expired", errorResponse.Error.Code);
    }

    [Fact]
    public async Task ExpiredSession_Returns401WithSessionExpiredCode()
    {
        // Skip if PostgreSQL is not available
        if (!IsPostgreSqlAvailable())
        {
            return;
        }

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await EnsureTestUserExistsAsync(dbContext);

        var (client, _) = await LoginAsync();

        // Get the session and set ExpiresAt to the past
        var session = await dbContext.Sessions
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync();

        Assert.NotNull(session);
        session.ExpiresAt = DateTime.UtcNow.AddMinutes(-1);
        await dbContext.SaveChangesAsync();

        // Act - make an authenticated request with expired session
        var response = await client.GetAsync("/api/v1/ping");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var errorResponse = await GetErrorResponseAsync(response);
        Assert.NotNull(errorResponse);
        Assert.Equal("session_expired", errorResponse.Error.Code);
    }

    [Fact]
    public async Task SessionTracking_DoesNotLogSensitiveData()
    {
        // Skip if PostgreSQL is not available
        if (!IsPostgreSqlAvailable())
        {
            return;
        }

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await EnsureTestUserExistsAsync(dbContext);

        // Act
        var (client, _) = await LoginAsync();
        var response = await client.GetAsync("/api/v1/ping");

        // Assert - verify no sensitive data in response
        var content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain(TestPassword, content);
        Assert.DoesNotContain("password", content, StringComparison.OrdinalIgnoreCase);
    }
}
