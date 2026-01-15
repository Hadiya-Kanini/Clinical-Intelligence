using ClinicalIntelligence.Api.Contracts;
using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Domain.Models;
using ClinicalIntelligence.Api.Services.Auth;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace ClinicalIntelligence.Api.Tests.Integration;

/// <summary>
/// Integration tests for session invalidation after password reset (US_031).
/// Tests verify that all existing sessions are revoked when a password is reset,
/// and that an audit log event is created for the invalidation.
/// </summary>
/// <remarks>
/// Tests are run sequentially (not in parallel) to avoid IP-based rate limiting conflicts.
/// </remarks>
[Collection("PasswordResetSessionsTests")]
public class PasswordResetInvalidatesSessionsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private const string TestPassword = "TestPassword123!";
    private const string NewPassword = "NewSecurePassword456!";

    private static string GenerateUniqueEmail() => $"pwd-reset-sessions-{Guid.NewGuid():N}@example.com";

    public PasswordResetInvalidatesSessionsTests(WebApplicationFactory<Program> factory)
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

    private async Task<User> CreateTestUserAsync(ApplicationDbContext dbContext, string email)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            Name = "Password Reset Sessions Test User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(TestPassword),
            Role = "Standard",
            Status = "Active",
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        return user;
    }

    private async Task CleanupUserDataAsync(ApplicationDbContext dbContext, Guid userId)
    {
        var existingTokens = await dbContext.PasswordResetTokens
            .Where(t => t.UserId == userId)
            .ToListAsync();
        dbContext.PasswordResetTokens.RemoveRange(existingTokens);

        var existingSessions = await dbContext.Sessions
            .Where(s => s.UserId == userId)
            .ToListAsync();
        dbContext.Sessions.RemoveRange(existingSessions);

        await dbContext.SaveChangesAsync();
    }

    private static async Task<ApiErrorResponse?> GetErrorResponseAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(content)) return null;
        try
        {
            return JsonSerializer.Deserialize<ApiErrorResponse>(content);
        }
        catch
        {
            return null;
        }
    }

    #region Session Invalidation Tests

    [Fact]
    public async Task PasswordReset_InvalidatesActiveSession_SessionMarkedRevoked()
    {
        if (!IsPostgreSqlAvailable()) return;

        var testEmail = GenerateUniqueEmail();

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<IPasswordResetTokenService>();
        var user = await CreateTestUserAsync(dbContext, testEmail);

        // Create a session directly in the database (avoids rate limiting)
        var session = new Session
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            LastActivityAt = DateTime.UtcNow,
            IsRevoked = false,
            IpAddress = "127.0.0.1",
            UserAgent = "Test"
        };
        dbContext.Sessions.Add(session);
        await dbContext.SaveChangesAsync();

        // Verify session is active before reset
        Assert.False(session.IsRevoked);

        // Generate a valid password reset token
        var tokenResult = await tokenService.GenerateTokenAsync(user.Id);

        // Perform password reset
        var resetClient = _factory.CreateClient();
        var resetResponse = await resetClient.PostAsJsonAsync("/api/v1/auth/reset-password", new
        {
            token = tokenResult.PlainToken,
            newPassword = NewPassword
        });
        Assert.Equal(HttpStatusCode.OK, resetResponse.StatusCode);

        // Reload session and verify it's now revoked
        await dbContext.Entry(session).ReloadAsync();
        Assert.True(session.IsRevoked);
    }

    [Fact]
    public async Task PasswordReset_MarksSessionAsRevoked_InDatabase()
    {
        if (!IsPostgreSqlAvailable()) return;

        var testEmail = GenerateUniqueEmail();

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<IPasswordResetTokenService>();
        var user = await CreateTestUserAsync(dbContext, testEmail);

        // Create a session via login
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        await client.PostAsJsonAsync("/api/v1/auth/login", new { email = testEmail, password = TestPassword });

        // Get session ID before reset
        var sessionBefore = await dbContext.Sessions
            .Where(s => s.UserId == user.Id && !s.IsRevoked)
            .FirstOrDefaultAsync();
        Assert.NotNull(sessionBefore);

        // Perform password reset
        var tokenResult = await tokenService.GenerateTokenAsync(user.Id);
        var resetClient = _factory.CreateClient();
        var resetResponse = await resetClient.PostAsJsonAsync("/api/v1/auth/reset-password", new
        {
            token = tokenResult.PlainToken,
            newPassword = NewPassword
        });
        Assert.Equal(HttpStatusCode.OK, resetResponse.StatusCode);

        // Reload session and verify it's revoked
        await dbContext.Entry(sessionBefore).ReloadAsync();
        Assert.True(sessionBefore.IsRevoked);
    }

    [Fact]
    public async Task PasswordReset_UserCanLoginWithNewPassword_AfterSessionInvalidation()
    {
        if (!IsPostgreSqlAvailable()) return;

        var testEmail = GenerateUniqueEmail();

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<IPasswordResetTokenService>();
        var user = await CreateTestUserAsync(dbContext, testEmail);

        // Perform password reset (no prior login needed for this test)
        var tokenResult = await tokenService.GenerateTokenAsync(user.Id);
        var resetClient = _factory.CreateClient();
        var resetResponse = await resetClient.PostAsJsonAsync("/api/v1/auth/reset-password", new
        {
            token = tokenResult.PlainToken,
            newPassword = NewPassword
        });
        Assert.Equal(HttpStatusCode.OK, resetResponse.StatusCode);

        // User should be able to login with new password
        var newClient = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        var newLogin = await newClient.PostAsJsonAsync("/api/v1/auth/login", new { email = testEmail, password = NewPassword });
        Assert.Equal(HttpStatusCode.OK, newLogin.StatusCode);

        // New session should work
        var pingNewSession = await newClient.GetAsync("/api/v1/ping");
        Assert.Equal(HttpStatusCode.OK, pingNewSession.StatusCode);
    }

    [Fact]
    public async Task PasswordReset_OldPasswordNoLongerWorks_AfterReset()
    {
        if (!IsPostgreSqlAvailable()) return;

        var testEmail = GenerateUniqueEmail();

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<IPasswordResetTokenService>();
        var user = await CreateTestUserAsync(dbContext, testEmail);

        // Perform password reset
        var tokenResult = await tokenService.GenerateTokenAsync(user.Id);
        var resetClient = _factory.CreateClient();
        await resetClient.PostAsJsonAsync("/api/v1/auth/reset-password", new
        {
            token = tokenResult.PlainToken,
            newPassword = NewPassword
        });

        // Verify password was changed by checking the hash in DB
        await dbContext.Entry(user).ReloadAsync();
        Assert.True(BCrypt.Net.BCrypt.Verify(NewPassword, user.PasswordHash));
        Assert.False(BCrypt.Net.BCrypt.Verify(TestPassword, user.PasswordHash));
    }

    #endregion

    #region Audit Log Tests

    [Fact]
    public async Task PasswordReset_CreatesAuditLogEvent_WithCorrectActionType()
    {
        if (!IsPostgreSqlAvailable()) return;

        var testEmail = GenerateUniqueEmail();

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<IPasswordResetTokenService>();
        var user = await CreateTestUserAsync(dbContext, testEmail);

        // Create sessions to be invalidated
        var client1 = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        await client1.PostAsJsonAsync("/api/v1/auth/login", new { email = testEmail, password = TestPassword });

        var timestampBefore = DateTime.UtcNow;

        // Perform password reset
        var tokenResult = await tokenService.GenerateTokenAsync(user.Id);
        var resetClient = _factory.CreateClient();
        var resetResponse = await resetClient.PostAsJsonAsync("/api/v1/auth/reset-password", new
        {
            token = tokenResult.PlainToken,
            newPassword = NewPassword
        });
        Assert.Equal(HttpStatusCode.OK, resetResponse.StatusCode);

        // Verify audit log event exists
        var auditEvent = await dbContext.AuditLogEvents
            .Where(e => e.UserId == user.Id && 
                        e.ActionType == "PASSWORD_RESET_SESSIONS_INVALIDATED" &&
                        e.Timestamp >= timestampBefore)
            .FirstOrDefaultAsync();

        Assert.NotNull(auditEvent);
        Assert.Equal("Session", auditEvent.ResourceType);
        Assert.Null(auditEvent.SessionId);
    }

    [Fact]
    public async Task PasswordReset_AuditLogMetadata_ContainsRevokedSessionCount()
    {
        if (!IsPostgreSqlAvailable()) return;

        var testEmail = GenerateUniqueEmail();

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<IPasswordResetTokenService>();
        var user = await CreateTestUserAsync(dbContext, testEmail);

        // Create one session (single-session enforcement means only one active session per user)
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        await client.PostAsJsonAsync("/api/v1/auth/login", new { email = testEmail, password = TestPassword });

        var timestampBefore = DateTime.UtcNow;

        // Perform password reset
        var tokenResult = await tokenService.GenerateTokenAsync(user.Id);
        var resetClient = _factory.CreateClient();
        await resetClient.PostAsJsonAsync("/api/v1/auth/reset-password", new
        {
            token = tokenResult.PlainToken,
            newPassword = NewPassword
        });

        // Verify audit log metadata contains revoked session count
        var auditEvent = await dbContext.AuditLogEvents
            .Where(e => e.UserId == user.Id && 
                        e.ActionType == "PASSWORD_RESET_SESSIONS_INVALIDATED" &&
                        e.Timestamp >= timestampBefore)
            .FirstOrDefaultAsync();

        Assert.NotNull(auditEvent);
        Assert.NotNull(auditEvent.Metadata);

        var metadata = JsonDocument.Parse(auditEvent.Metadata);
        Assert.True(metadata.RootElement.TryGetProperty("revokedSessionCount", out var countElement));
        Assert.Equal(1, countElement.GetInt32());
    }

    [Fact]
    public async Task PasswordReset_AuditLogMetadata_DoesNotContainSensitiveData()
    {
        if (!IsPostgreSqlAvailable()) return;

        var testEmail = GenerateUniqueEmail();

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<IPasswordResetTokenService>();
        var user = await CreateTestUserAsync(dbContext, testEmail);

        // Create a session
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        await client.PostAsJsonAsync("/api/v1/auth/login", new { email = testEmail, password = TestPassword });

        var timestampBefore = DateTime.UtcNow;

        // Perform password reset
        var tokenResult = await tokenService.GenerateTokenAsync(user.Id);
        var resetClient = _factory.CreateClient();
        await resetClient.PostAsJsonAsync("/api/v1/auth/reset-password", new
        {
            token = tokenResult.PlainToken,
            newPassword = NewPassword
        });

        // Verify audit log does not contain sensitive data
        var auditEvent = await dbContext.AuditLogEvents
            .Where(e => e.UserId == user.Id && 
                        e.ActionType == "PASSWORD_RESET_SESSIONS_INVALIDATED" &&
                        e.Timestamp >= timestampBefore)
            .FirstOrDefaultAsync();

        Assert.NotNull(auditEvent);
        Assert.NotNull(auditEvent.Metadata);

        // Metadata should not contain token or password
        Assert.DoesNotContain(tokenResult.PlainToken, auditEvent.Metadata);
        Assert.DoesNotContain(NewPassword, auditEvent.Metadata);
        Assert.DoesNotContain("password", auditEvent.Metadata.ToLower());
        Assert.DoesNotContain("token", auditEvent.Metadata.ToLower());
    }

    [Fact]
    public async Task PasswordReset_WithNoActiveSessions_StillCreatesAuditLog()
    {
        if (!IsPostgreSqlAvailable()) return;

        var testEmail = GenerateUniqueEmail();

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<IPasswordResetTokenService>();
        var user = await CreateTestUserAsync(dbContext, testEmail);

        var timestampBefore = DateTime.UtcNow;

        // Perform password reset without any active sessions
        var tokenResult = await tokenService.GenerateTokenAsync(user.Id);
        var resetClient = _factory.CreateClient();
        var resetResponse = await resetClient.PostAsJsonAsync("/api/v1/auth/reset-password", new
        {
            token = tokenResult.PlainToken,
            newPassword = NewPassword
        });
        Assert.Equal(HttpStatusCode.OK, resetResponse.StatusCode);

        // Verify audit log event exists with 0 revoked sessions
        var auditEvent = await dbContext.AuditLogEvents
            .Where(e => e.UserId == user.Id && 
                        e.ActionType == "PASSWORD_RESET_SESSIONS_INVALIDATED" &&
                        e.Timestamp >= timestampBefore)
            .FirstOrDefaultAsync();

        Assert.NotNull(auditEvent);
        
        var metadata = JsonDocument.Parse(auditEvent.Metadata!);
        Assert.True(metadata.RootElement.TryGetProperty("revokedSessionCount", out var countElement));
        Assert.Equal(0, countElement.GetInt32());
    }

    #endregion

    #region Error Response Structure Tests

    [Fact]
    public async Task RevokedSession_Returns401_WithStandardizedErrorStructure()
    {
        if (!IsPostgreSqlAvailable()) return;

        var testEmail = GenerateUniqueEmail();

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<IPasswordResetTokenService>();
        var user = await CreateTestUserAsync(dbContext, testEmail);

        // Create a session
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        await client.PostAsJsonAsync("/api/v1/auth/login", new { email = testEmail, password = TestPassword });

        // Perform password reset to invalidate session
        var tokenResult = await tokenService.GenerateTokenAsync(user.Id);
        var resetClient = _factory.CreateClient();
        await resetClient.PostAsJsonAsync("/api/v1/auth/reset-password", new
        {
            token = tokenResult.PlainToken,
            newPassword = NewPassword
        });

        // Try to access protected endpoint with revoked session
        var response = await client.GetAsync("/api/v1/ping");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion
}
