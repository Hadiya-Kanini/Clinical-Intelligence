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
/// Integration tests for reset password token validation and password reset completion (US_029).
/// Tests cover token validation endpoint and reset-password endpoint with token consumption.
/// </summary>
public class ResetPasswordFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private const string TestEmail = "reset-password-flow-test@example.com";
    private const string TestPassword = "TestPassword123!";
    private const string NewPassword = "NewSecurePassword456!";

    public ResetPasswordFlowTests(WebApplicationFactory<Program> factory)
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
                Name = "Reset Password Flow Test User",
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
        else
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(TestPassword);
            user.Status = "Active";
            user.IsDeleted = false;
            await dbContext.SaveChangesAsync();
        }
        return user;
    }

    private async Task CleanupUserTokensAsync(ApplicationDbContext dbContext, Guid userId)
    {
        var existingTokens = await dbContext.PasswordResetTokens
            .Where(t => t.UserId == userId)
            .ToListAsync();
        dbContext.PasswordResetTokens.RemoveRange(existingTokens);
        await dbContext.SaveChangesAsync();
    }

    private static async Task<(HttpStatusCode statusCode, ApiErrorResponse? errorBody, JsonDocument? successBody)> GetResponse(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        ApiErrorResponse? errorBody = null;
        JsonDocument? successBody = null;

        if (!string.IsNullOrEmpty(content))
        {
            try
            {
                var doc = JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("error", out _))
                {
                    errorBody = JsonSerializer.Deserialize<ApiErrorResponse>(content);
                }
                else
                {
                    successBody = doc;
                }
            }
            catch
            {
                // Ignore parse errors
            }
        }

        return (response.StatusCode, errorBody, successBody);
    }

    #region Token Validation Endpoint Tests

    [Fact]
    public async Task ValidateToken_MissingToken_Returns400()
    {
        if (!IsPostgreSqlAvailable()) return;

        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/auth/reset-password/validate");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var (_, errorBody, _) = await GetResponse(response);
        Assert.NotNull(errorBody);
        Assert.Equal("invalid_input", errorBody.Error.Code);
        Assert.Contains(errorBody.Error.Details, d => d.Contains("token:required"));
    }

    [Fact]
    public async Task ValidateToken_EmptyToken_Returns400()
    {
        if (!IsPostgreSqlAvailable()) return;

        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/auth/reset-password/validate?token=");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var (_, errorBody, _) = await GetResponse(response);
        Assert.NotNull(errorBody);
        Assert.Equal("invalid_input", errorBody.Error.Code);
    }

    [Fact]
    public async Task ValidateToken_MalformedToken_Returns400()
    {
        if (!IsPostgreSqlAvailable()) return;

        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/auth/reset-password/validate?token=!!invalid!!");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var (_, errorBody, _) = await GetResponse(response);
        Assert.NotNull(errorBody);
        Assert.Equal("invalid_input", errorBody.Error.Code);
        Assert.Contains(errorBody.Error.Details, d => d.Contains("token:invalid_format"));
    }

    [Fact]
    public async Task ValidateToken_NonExistentToken_ReturnsUnauthorized()
    {
        if (!IsPostgreSqlAvailable()) return;

        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/auth/reset-password/validate?token=nonexistent-token-abc123");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var (_, errorBody, _) = await GetResponse(response);
        Assert.NotNull(errorBody);
        Assert.Equal("invalid_token", errorBody.Error.Code);
    }

    [Fact]
    public async Task ValidateToken_ExpiredToken_ReturnsTokenExpired()
    {
        if (!IsPostgreSqlAvailable()) return;

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await EnsureTestUserExistsAsync(dbContext);
        await CleanupUserTokensAsync(dbContext, user.Id);

        var plainToken = "expired-test-token-12345678901234567890";
        var tokenHash = PasswordResetTokenService.ComputeTokenHash(plainToken);

        var expiredToken = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddHours(-1),
            UsedAt = null
        };
        dbContext.PasswordResetTokens.Add(expiredToken);
        await dbContext.SaveChangesAsync();

        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/auth/reset-password/validate?token={plainToken}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var (_, errorBody, _) = await GetResponse(response);
        Assert.NotNull(errorBody);
        Assert.Equal("token_expired", errorBody.Error.Code);
    }

    [Fact]
    public async Task ValidateToken_UsedToken_ReturnsTokenUsed()
    {
        if (!IsPostgreSqlAvailable()) return;

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await EnsureTestUserExistsAsync(dbContext);
        await CleanupUserTokensAsync(dbContext, user.Id);

        var plainToken = "used-test-token-12345678901234567890ab";
        var tokenHash = PasswordResetTokenService.ComputeTokenHash(plainToken);

        var usedToken = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            UsedAt = DateTime.UtcNow.AddMinutes(-5)
        };
        dbContext.PasswordResetTokens.Add(usedToken);
        await dbContext.SaveChangesAsync();

        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/auth/reset-password/validate?token={plainToken}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var (_, errorBody, _) = await GetResponse(response);
        Assert.NotNull(errorBody);
        Assert.Equal("token_used", errorBody.Error.Code);
    }

    [Fact]
    public async Task ValidateToken_ValidToken_Returns200WithValidTrue()
    {
        if (!IsPostgreSqlAvailable()) return;

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<IPasswordResetTokenService>();
        var user = await EnsureTestUserExistsAsync(dbContext);
        await CleanupUserTokensAsync(dbContext, user.Id);

        var tokenResult = await tokenService.GenerateTokenAsync(user.Id);

        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/auth/reset-password/validate?token={tokenResult.PlainToken}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var (_, _, successBody) = await GetResponse(response);
        Assert.NotNull(successBody);
        Assert.True(successBody.RootElement.GetProperty("valid").GetBoolean());
        Assert.True(successBody.RootElement.TryGetProperty("expiresAt", out _));
    }

    [Fact]
    public async Task ValidateToken_DeletedUser_ReturnsInvalidToken()
    {
        if (!IsPostgreSqlAvailable()) return;

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await EnsureTestUserExistsAsync(dbContext);
        await CleanupUserTokensAsync(dbContext, user.Id);

        var plainToken = "deleted-user-token-1234567890123456";
        var tokenHash = PasswordResetTokenService.ComputeTokenHash(plainToken);

        var token = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            UsedAt = null
        };
        dbContext.PasswordResetTokens.Add(token);

        user.IsDeleted = true;
        await dbContext.SaveChangesAsync();

        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/auth/reset-password/validate?token={plainToken}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var (_, errorBody, _) = await GetResponse(response);
        Assert.NotNull(errorBody);
        Assert.Equal("invalid_token", errorBody.Error.Code);

        user.IsDeleted = false;
        await dbContext.SaveChangesAsync();
    }

    #endregion

    #region Reset Password Endpoint Tests

    [Fact]
    public async Task ResetPassword_MissingToken_Returns400()
    {
        if (!IsPostgreSqlAvailable()) return;

        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/reset-password", new
        {
            newPassword = NewPassword
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var (_, errorBody, _) = await GetResponse(response);
        Assert.NotNull(errorBody);
        Assert.Equal("invalid_input", errorBody.Error.Code);
        Assert.Contains(errorBody.Error.Details, d => d.Contains("token:required"));
    }

    [Fact]
    public async Task ResetPassword_MissingPassword_Returns400()
    {
        if (!IsPostgreSqlAvailable()) return;

        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/reset-password", new
        {
            token = "some-token"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var (_, errorBody, _) = await GetResponse(response);
        Assert.NotNull(errorBody);
        Assert.Equal("invalid_input", errorBody.Error.Code);
        Assert.Contains(errorBody.Error.Details, d => d.Contains("newPassword:required"));
    }

    [Fact]
    public async Task ResetPassword_WeakPassword_ReturnsPasswordRequirementsNotMet()
    {
        if (!IsPostgreSqlAvailable()) return;

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<IPasswordResetTokenService>();
        var user = await EnsureTestUserExistsAsync(dbContext);
        await CleanupUserTokensAsync(dbContext, user.Id);

        var tokenResult = await tokenService.GenerateTokenAsync(user.Id);

        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/reset-password", new
        {
            token = tokenResult.PlainToken,
            newPassword = "weak"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var (_, errorBody, _) = await GetResponse(response);
        Assert.NotNull(errorBody);
        Assert.Equal("password_requirements_not_met", errorBody.Error.Code);
        Assert.NotEmpty(errorBody.Error.Details);
    }

    [Fact]
    public async Task ResetPassword_InvalidToken_ReturnsUnauthorized()
    {
        if (!IsPostgreSqlAvailable()) return;

        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/reset-password", new
        {
            token = "nonexistent-token-abc123def456",
            newPassword = NewPassword
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var (_, errorBody, _) = await GetResponse(response);
        Assert.NotNull(errorBody);
        Assert.True(errorBody.Error.Code == "invalid_token" || errorBody.Error.Code == "invalid_input");
    }

    [Fact]
    public async Task ResetPassword_ExpiredToken_ReturnsTokenExpired()
    {
        if (!IsPostgreSqlAvailable()) return;

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await EnsureTestUserExistsAsync(dbContext);
        await CleanupUserTokensAsync(dbContext, user.Id);

        var plainToken = "expired-reset-token-123456789012345";
        var tokenHash = PasswordResetTokenService.ComputeTokenHash(plainToken);

        var expiredToken = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddHours(-1),
            UsedAt = null
        };
        dbContext.PasswordResetTokens.Add(expiredToken);
        await dbContext.SaveChangesAsync();

        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/reset-password", new
        {
            token = plainToken,
            newPassword = NewPassword
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var (_, errorBody, _) = await GetResponse(response);
        Assert.NotNull(errorBody);
        Assert.Equal("token_expired", errorBody.Error.Code);
    }

    [Fact]
    public async Task ResetPassword_ValidToken_Returns200AndUpdatesPassword()
    {
        if (!IsPostgreSqlAvailable()) return;

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<IPasswordResetTokenService>();
        var user = await EnsureTestUserExistsAsync(dbContext);
        await CleanupUserTokensAsync(dbContext, user.Id);

        var originalPasswordHash = user.PasswordHash;
        var tokenResult = await tokenService.GenerateTokenAsync(user.Id);

        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/reset-password", new
        {
            token = tokenResult.PlainToken,
            newPassword = NewPassword
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var (_, _, successBody) = await GetResponse(response);
        Assert.NotNull(successBody);
        Assert.True(successBody.RootElement.TryGetProperty("message", out var messageElement));
        Assert.Contains("successful", messageElement.GetString(), StringComparison.OrdinalIgnoreCase);

        await dbContext.Entry(user).ReloadAsync();
        Assert.NotEqual(originalPasswordHash, user.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify(NewPassword, user.PasswordHash));
    }

    [Fact]
    public async Task ResetPassword_ValidToken_MarksTokenAsUsed()
    {
        if (!IsPostgreSqlAvailable()) return;

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<IPasswordResetTokenService>();
        var user = await EnsureTestUserExistsAsync(dbContext);
        await CleanupUserTokensAsync(dbContext, user.Id);

        var tokenResult = await tokenService.GenerateTokenAsync(user.Id);

        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/reset-password", new
        {
            token = tokenResult.PlainToken,
            newPassword = NewPassword
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var token = await dbContext.PasswordResetTokens.FindAsync(tokenResult.TokenId);
        Assert.NotNull(token);
        Assert.NotNull(token.UsedAt);
    }

    [Fact]
    public async Task ResetPassword_UsedToken_CannotBeReused()
    {
        if (!IsPostgreSqlAvailable()) return;

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<IPasswordResetTokenService>();
        var user = await EnsureTestUserExistsAsync(dbContext);
        await CleanupUserTokensAsync(dbContext, user.Id);

        var tokenResult = await tokenService.GenerateTokenAsync(user.Id);

        var client = _factory.CreateClient();

        var response1 = await client.PostAsJsonAsync("/api/v1/auth/reset-password", new
        {
            token = tokenResult.PlainToken,
            newPassword = NewPassword
        });
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

        var response2 = await client.PostAsJsonAsync("/api/v1/auth/reset-password", new
        {
            token = tokenResult.PlainToken,
            newPassword = "AnotherPassword789!"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response2.StatusCode);

        var (_, errorBody, _) = await GetResponse(response2);
        Assert.NotNull(errorBody);
        Assert.True(errorBody.Error.Code == "token_used" || errorBody.Error.Code == "invalid_token");
    }

    [Fact]
    public async Task ResetPassword_ResetsFailedLoginAttemptsAndUnlocksAccount()
    {
        if (!IsPostgreSqlAvailable()) return;

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<IPasswordResetTokenService>();
        var user = await EnsureTestUserExistsAsync(dbContext);
        await CleanupUserTokensAsync(dbContext, user.Id);

        user.FailedLoginAttempts = 5;
        user.LockedUntil = DateTime.UtcNow.AddMinutes(30);
        await dbContext.SaveChangesAsync();

        var tokenResult = await tokenService.GenerateTokenAsync(user.Id);

        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/reset-password", new
        {
            token = tokenResult.PlainToken,
            newPassword = NewPassword
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await dbContext.Entry(user).ReloadAsync();
        Assert.Equal(0, user.FailedLoginAttempts);
        Assert.Null(user.LockedUntil);
    }

    [Fact]
    public async Task ResetPassword_PasswordHashUsesBcrypt()
    {
        if (!IsPostgreSqlAvailable()) return;

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<IPasswordResetTokenService>();
        var user = await EnsureTestUserExistsAsync(dbContext);
        await CleanupUserTokensAsync(dbContext, user.Id);

        var tokenResult = await tokenService.GenerateTokenAsync(user.Id);

        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/reset-password", new
        {
            token = tokenResult.PlainToken,
            newPassword = NewPassword
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await dbContext.Entry(user).ReloadAsync();

        Assert.StartsWith("$2", user.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify(NewPassword, user.PasswordHash));
    }

    #endregion

    #region Token Single-Use and Atomicity Tests (US_030)

    [Fact]
    public async Task ResetPassword_WeakPassword_DoesNotConsumeToken()
    {
        if (!IsPostgreSqlAvailable()) return;

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<IPasswordResetTokenService>();
        var user = await EnsureTestUserExistsAsync(dbContext);
        await CleanupUserTokensAsync(dbContext, user.Id);

        var tokenResult = await tokenService.GenerateTokenAsync(user.Id);

        var client = _factory.CreateClient();

        // Attempt reset with weak password
        var response = await client.PostAsJsonAsync("/api/v1/auth/reset-password", new
        {
            token = tokenResult.PlainToken,
            newPassword = "weak"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var (_, errorBody, _) = await GetResponse(response);
        Assert.NotNull(errorBody);
        Assert.Equal("password_requirements_not_met", errorBody.Error.Code);

        // Verify token is NOT consumed (UsedAt remains null)
        var token = await dbContext.PasswordResetTokens.FindAsync(tokenResult.TokenId);
        Assert.NotNull(token);
        Assert.Null(token.UsedAt);

        // Verify token can still be used with a valid password
        var response2 = await client.PostAsJsonAsync("/api/v1/auth/reset-password", new
        {
            token = tokenResult.PlainToken,
            newPassword = NewPassword
        });

        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_ConcurrentRequests_OnlyOneSucceeds()
    {
        if (!IsPostgreSqlAvailable()) return;

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<IPasswordResetTokenService>();
        var user = await EnsureTestUserExistsAsync(dbContext);
        await CleanupUserTokensAsync(dbContext, user.Id);

        var tokenResult = await tokenService.GenerateTokenAsync(user.Id);

        var client1 = _factory.CreateClient();
        var client2 = _factory.CreateClient();

        // Start both requests concurrently with the same token
        var task1 = client1.PostAsJsonAsync("/api/v1/auth/reset-password", new
        {
            token = tokenResult.PlainToken,
            newPassword = "ConcurrentPassword1!"
        });

        var task2 = client2.PostAsJsonAsync("/api/v1/auth/reset-password", new
        {
            token = tokenResult.PlainToken,
            newPassword = "ConcurrentPassword2!"
        });

        var responses = await Task.WhenAll(task1, task2);

        var statusCodes = responses.Select(r => r.StatusCode).ToList();

        // Exactly one should succeed (200), the other should fail (401)
        var successCount = statusCodes.Count(s => s == HttpStatusCode.OK);
        var failureCount = statusCodes.Count(s => s == HttpStatusCode.Unauthorized);

        Assert.Equal(1, successCount);
        Assert.Equal(1, failureCount);

        // Verify the failed response has correct error code
        var failedResponse = responses.First(r => r.StatusCode == HttpStatusCode.Unauthorized);
        var (_, errorBody, _) = await GetResponse(failedResponse);
        Assert.NotNull(errorBody);
        Assert.True(errorBody.Error.Code == "invalid_token" || errorBody.Error.Code == "token_used");

        // Verify token is consumed exactly once
        var token = await dbContext.PasswordResetTokens.FindAsync(tokenResult.TokenId);
        Assert.NotNull(token);
        Assert.NotNull(token.UsedAt);
    }

    [Fact]
    public async Task ResetPassword_TokenInvalidationIsImmediate()
    {
        if (!IsPostgreSqlAvailable()) return;

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<IPasswordResetTokenService>();
        var user = await EnsureTestUserExistsAsync(dbContext);
        await CleanupUserTokensAsync(dbContext, user.Id);

        var tokenResult = await tokenService.GenerateTokenAsync(user.Id);

        var client = _factory.CreateClient();

        // First reset should succeed
        var response1 = await client.PostAsJsonAsync("/api/v1/auth/reset-password", new
        {
            token = tokenResult.PlainToken,
            newPassword = NewPassword
        });
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

        // Verify UsedAt is set immediately after success
        var token = await dbContext.PasswordResetTokens.FindAsync(tokenResult.TokenId);
        Assert.NotNull(token);
        Assert.NotNull(token.UsedAt);

        // Immediate reuse attempt should fail
        var response2 = await client.PostAsJsonAsync("/api/v1/auth/reset-password", new
        {
            token = tokenResult.PlainToken,
            newPassword = "AnotherPassword789!"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response2.StatusCode);

        var (_, errorBody, _) = await GetResponse(response2);
        Assert.NotNull(errorBody);
        Assert.True(errorBody.Error.Code == "token_used" || errorBody.Error.Code == "invalid_token");
    }

    #endregion

    #region Error Response Structure Tests

    [Fact]
    public async Task ErrorResponses_HaveStandardizedStructure()
    {
        if (!IsPostgreSqlAvailable()) return;

        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/auth/reset-password/validate");

        var content = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("error", out var error));
        Assert.True(error.TryGetProperty("code", out _));
        Assert.True(error.TryGetProperty("message", out _));
        Assert.True(error.TryGetProperty("details", out var details));
        Assert.Equal(JsonValueKind.Array, details.ValueKind);
    }

    [Fact]
    public async Task ErrorResponses_DoNotExposeTokenInLogs()
    {
        if (!IsPostgreSqlAvailable()) return;

        var client = _factory.CreateClient();
        var sensitiveToken = "sensitive-token-should-not-appear-in-response";

        var response = await client.GetAsync($"/api/v1/auth/reset-password/validate?token={sensitiveToken}");

        var content = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain(sensitiveToken, content);
    }

    #endregion
}
