using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Domain.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace ClinicalIntelligence.Api.Tests.Integration;

/// <summary>
/// Integration tests for account lockout behavior (US_016).
/// Tests lockout threshold, remaining-time response, automatic unlock, and audit logging.
/// </summary>
public class AccountLockoutTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private const string TestEmail = "lockout-test@example.com";
    private const string TestPassword = "TestPassword123!";
    private const string WrongPassword = "WrongPassword!";
    private const int LockoutThreshold = 5;

    public AccountLockoutTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                // Use high rate limit to avoid interference with lockout tests
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["RateLimiting:LoginPermitLimit"] = "100",
                    ["RateLimiting:LoginWindowSeconds"] = "60"
                });
            });
        });
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

    private async Task<User> EnsureTestUserExistsAsync(ApplicationDbContext dbContext, bool resetLockout = true)
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
                Name = "Lockout Test User",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(TestPassword),
                Role = "Standard",
                Status = "Active",
                IsDeleted = false,
                FailedLoginAttempts = 0,
                LockedUntil = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
        }
        else if (resetLockout)
        {
            user.FailedLoginAttempts = 0;
            user.LockedUntil = null;
            user.UpdatedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();
        }

        return user;
    }

    private HttpClient CreateTestClient()
    {
        return _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true
        });
    }

    [Fact]
    public async Task Login_FiveFailedAttempts_LocksAccountFor30Minutes()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await EnsureTestUserExistsAsync(dbContext);

        var client = CreateTestClient();

        // Act - make 5 failed login attempts
        for (int i = 0; i < LockoutThreshold; i++)
        {
            await client.PostAsJsonAsync("/api/v1/auth/login", new
            {
                email = TestEmail,
                password = WrongPassword
            });
        }

        // Refresh user from database
        await dbContext.Entry(user).ReloadAsync();

        // Assert - account should be locked
        Assert.NotNull(user.LockedUntil);
        Assert.True(user.LockedUntil > DateTime.UtcNow);

        // Verify lockout duration is approximately 30 minutes
        var lockoutDuration = user.LockedUntil.Value - DateTime.UtcNow;
        Assert.True(lockoutDuration.TotalMinutes >= 29 && lockoutDuration.TotalMinutes <= 31,
            $"Expected lockout duration ~30 minutes, got {lockoutDuration.TotalMinutes} minutes");
    }

    [Fact]
    public async Task Login_LockedAccount_Returns403WithRemainingTime()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await EnsureTestUserExistsAsync(dbContext);

        // Manually lock the account
        user.LockedUntil = DateTime.UtcNow.AddMinutes(30);
        user.FailedLoginAttempts = 5;
        await dbContext.SaveChangesAsync();

        var client = CreateTestClient();

        // Act - attempt login while locked
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = TestEmail,
            password = TestPassword
        });

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("error", out var errorElement));
        Assert.True(errorElement.TryGetProperty("code", out var codeElement));
        Assert.Equal("account_locked", codeElement.GetString());

        Assert.True(errorElement.TryGetProperty("details", out var detailsElement));
        var details = detailsElement.EnumerateArray().Select(d => d.GetString()).ToList();

        // Verify unlock_at is present and parseable
        var unlockAtDetail = details.FirstOrDefault(d => d?.StartsWith("unlock_at:") == true);
        Assert.NotNull(unlockAtDetail);
        var unlockAtStr = unlockAtDetail.Replace("unlock_at:", "");
        Assert.True(DateTime.TryParse(unlockAtStr, out var unlockAt));
        Assert.True(unlockAt > DateTime.UtcNow);

        // Verify remaining_seconds is present and positive
        var remainingDetail = details.FirstOrDefault(d => d?.StartsWith("remaining_seconds:") == true);
        Assert.NotNull(remainingDetail);
        var remainingSecondsStr = remainingDetail.Replace("remaining_seconds:", "");
        Assert.True(int.TryParse(remainingSecondsStr, out var remainingSeconds));
        Assert.True(remainingSeconds > 0);
    }

    [Fact]
    public async Task Login_AfterLockoutExpires_AllowsLoginAndResetsCounters()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await EnsureTestUserExistsAsync(dbContext);

        // Set lockout to have expired (in the past)
        user.LockedUntil = DateTime.UtcNow.AddMinutes(-1);
        user.FailedLoginAttempts = 5;
        await dbContext.SaveChangesAsync();

        var client = CreateTestClient();

        // Act - attempt login after lockout expired
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = TestEmail,
            password = TestPassword
        });

        // Assert - should succeed
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify counters were reset
        await dbContext.Entry(user).ReloadAsync();
        Assert.Equal(0, user.FailedLoginAttempts);
        Assert.Null(user.LockedUntil);
    }

    [Fact]
    public async Task Login_AfterLockoutExpires_WrongPassword_ResetsCountersAndIncrementsAgain()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await EnsureTestUserExistsAsync(dbContext);

        // Set lockout to have expired (in the past)
        user.LockedUntil = DateTime.UtcNow.AddMinutes(-1);
        user.FailedLoginAttempts = 5;
        await dbContext.SaveChangesAsync();

        var client = CreateTestClient();

        // Act - attempt login with wrong password after lockout expired
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = TestEmail,
            password = WrongPassword
        });

        // Assert - should return 401 (not 403 locked)
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        // Verify counters were reset and then incremented
        await dbContext.Entry(user).ReloadAsync();
        Assert.Equal(1, user.FailedLoginAttempts);
        Assert.Null(user.LockedUntil);
    }

    [Fact]
    public async Task Login_LockoutTriggered_CreatesAuditLogEvent()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await EnsureTestUserExistsAsync(dbContext);

        // Clear existing ACCOUNT_LOCKED audit events for this user
        var existingEvents = await dbContext.AuditLogEvents
            .Where(e => e.UserId == user.Id && e.ActionType == "ACCOUNT_LOCKED")
            .ToListAsync();
        dbContext.AuditLogEvents.RemoveRange(existingEvents);
        await dbContext.SaveChangesAsync();

        var client = CreateTestClient();

        // Act - make 5 failed login attempts to trigger lockout
        for (int i = 0; i < LockoutThreshold; i++)
        {
            await client.PostAsJsonAsync("/api/v1/auth/login", new
            {
                email = TestEmail,
                password = WrongPassword
            });
        }

        // Assert - verify ACCOUNT_LOCKED audit event was created
        var auditEvent = await dbContext.AuditLogEvents
            .OrderByDescending(e => e.Timestamp)
            .FirstOrDefaultAsync(e => e.UserId == user.Id && e.ActionType == "ACCOUNT_LOCKED");

        Assert.NotNull(auditEvent);
        Assert.Equal("ACCOUNT_LOCKED", auditEvent.ActionType);
        Assert.Equal("Auth", auditEvent.ResourceType);
        Assert.Equal(user.Id, auditEvent.UserId);
        Assert.Null(auditEvent.SessionId);
        Assert.NotNull(auditEvent.Metadata);

        // Verify metadata contains expected fields
        using var metadataDoc = JsonDocument.Parse(auditEvent.Metadata);
        var metadata = metadataDoc.RootElement;
        Assert.True(metadata.TryGetProperty("unlock_at", out _));
        Assert.True(metadata.TryGetProperty("failed_attempts", out var failedAttemptsElement));
        Assert.Equal(5, failedAttemptsElement.GetInt32());
        Assert.True(metadata.TryGetProperty("threshold", out var thresholdElement));
        Assert.Equal(5, thresholdElement.GetInt32());
    }

    [Fact]
    public async Task Login_AlreadyLocked_DoesNotCreateDuplicateAuditEvent()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await EnsureTestUserExistsAsync(dbContext);

        // Manually lock the account
        user.LockedUntil = DateTime.UtcNow.AddMinutes(30);
        user.FailedLoginAttempts = 5;
        await dbContext.SaveChangesAsync();

        // Clear existing ACCOUNT_LOCKED audit events
        var existingEvents = await dbContext.AuditLogEvents
            .Where(e => e.UserId == user.Id && e.ActionType == "ACCOUNT_LOCKED")
            .ToListAsync();
        dbContext.AuditLogEvents.RemoveRange(existingEvents);
        await dbContext.SaveChangesAsync();

        var client = CreateTestClient();

        // Act - attempt login while already locked (multiple times)
        for (int i = 0; i < 3; i++)
        {
            await client.PostAsJsonAsync("/api/v1/auth/login", new
            {
                email = TestEmail,
                password = WrongPassword
            });
        }

        // Assert - no new ACCOUNT_LOCKED events should be created
        var auditEvents = await dbContext.AuditLogEvents
            .Where(e => e.UserId == user.Id && e.ActionType == "ACCOUNT_LOCKED")
            .ToListAsync();

        Assert.Empty(auditEvents);
    }

    [Fact]
    public async Task Login_LockedAccount_DoesNotIncrementFailedAttempts()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await EnsureTestUserExistsAsync(dbContext);

        // Manually lock the account
        user.LockedUntil = DateTime.UtcNow.AddMinutes(30);
        user.FailedLoginAttempts = 5;
        await dbContext.SaveChangesAsync();

        var client = CreateTestClient();

        // Act - attempt login while locked
        await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = TestEmail,
            password = WrongPassword
        });

        // Assert - failed attempts should not increase
        await dbContext.Entry(user).ReloadAsync();
        Assert.Equal(5, user.FailedLoginAttempts);
    }

    [Fact]
    public async Task Login_SuccessfulLogin_ResetsFailedAttempts()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await EnsureTestUserExistsAsync(dbContext);

        // Set some failed attempts (but not enough to lock)
        user.FailedLoginAttempts = 3;
        await dbContext.SaveChangesAsync();

        var client = CreateTestClient();

        // Act - successful login
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = TestEmail,
            password = TestPassword
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await dbContext.Entry(user).ReloadAsync();
        Assert.Equal(0, user.FailedLoginAttempts);
        Assert.Null(user.LockedUntil);
    }

    [Fact]
    public async Task Login_LockedAccountResponse_DoesNotLeakSensitiveInfo()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await EnsureTestUserExistsAsync(dbContext);

        // Manually lock the account
        user.LockedUntil = DateTime.UtcNow.AddMinutes(30);
        user.FailedLoginAttempts = 5;
        await dbContext.SaveChangesAsync();

        var client = CreateTestClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = TestEmail,
            password = TestPassword
        });

        // Assert
        var content = await response.Content.ReadAsStringAsync();

        // Should not contain sensitive information
        Assert.DoesNotContain(TestEmail, content);
        Assert.DoesNotContain("password", content.ToLower());
        Assert.DoesNotContain(user.Id.ToString(), content);
    }
}
