using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Domain.Models;
using ClinicalIntelligence.Api.Services.Auth;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace ClinicalIntelligence.Api.Tests.Integration;

/// <summary>
/// Integration tests for password reset token lifecycle (US_025).
/// Tests secure token generation, 1-hour expiry, hash-only storage,
/// and invalidation of previous tokens.
/// </summary>
public class ForgotPasswordTokenLifecycleTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private const string TestEmail = "password-reset-test@example.com";
    private const string TestPassword = "TestPassword123!";

    public ForgotPasswordTokenLifecycleTests(WebApplicationFactory<Program> factory)
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
                Name = "Password Reset Test User",
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

    private async Task CleanupUserTokensAsync(ApplicationDbContext dbContext, Guid userId)
    {
        var existingTokens = await dbContext.PasswordResetTokens
            .Where(t => t.UserId == userId)
            .ToListAsync();
        dbContext.PasswordResetTokens.RemoveRange(existingTokens);
        await dbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task ForgotPassword_ExistingUser_CreatesTokenRecord()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await EnsureTestUserExistsAsync(dbContext);
        await CleanupUserTokensAsync(dbContext, user.Id);

        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new
        {
            email = TestEmail
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var token = await dbContext.PasswordResetTokens
            .FirstOrDefaultAsync(t => t.UserId == user.Id);

        Assert.NotNull(token);
        Assert.Equal(user.Id, token.UserId);
        Assert.Null(token.UsedAt);
    }

    [Fact]
    public async Task ForgotPassword_TokenExpiry_IsApproximatelyOneHour()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await EnsureTestUserExistsAsync(dbContext);
        await CleanupUserTokensAsync(dbContext, user.Id);

        var client = _factory.CreateClient();
        var beforeRequest = DateTime.UtcNow;

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new
        {
            email = TestEmail
        });

        var afterRequest = DateTime.UtcNow;

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var token = await dbContext.PasswordResetTokens
            .FirstOrDefaultAsync(t => t.UserId == user.Id && t.UsedAt == null);

        Assert.NotNull(token);

        // Token should expire approximately 1 hour from now (with 5 minute tolerance)
        var expectedExpiryMin = beforeRequest.AddHours(1).AddMinutes(-5);
        var expectedExpiryMax = afterRequest.AddHours(1).AddMinutes(5);

        Assert.True(token.ExpiresAt >= expectedExpiryMin,
            $"Token expiry {token.ExpiresAt:O} should be >= {expectedExpiryMin:O}");
        Assert.True(token.ExpiresAt <= expectedExpiryMax,
            $"Token expiry {token.ExpiresAt:O} should be <= {expectedExpiryMax:O}");
    }

    [Fact]
    public async Task ForgotPassword_OnlyTokenHashStored_NeverPlainToken()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await EnsureTestUserExistsAsync(dbContext);
        await CleanupUserTokensAsync(dbContext, user.Id);

        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new
        {
            email = TestEmail
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var token = await dbContext.PasswordResetTokens
            .FirstOrDefaultAsync(t => t.UserId == user.Id);

        Assert.NotNull(token);
        Assert.NotNull(token.TokenHash);
        Assert.NotEmpty(token.TokenHash);

        // TokenHash should be a hex-encoded SHA-256 hash (64 characters)
        Assert.Equal(64, token.TokenHash.Length);
        Assert.True(token.TokenHash.All(c => char.IsLetterOrDigit(c)),
            "TokenHash should be hex-encoded");

        // Verify it's lowercase hex
        Assert.Equal(token.TokenHash.ToLowerInvariant(), token.TokenHash);
    }

    [Fact]
    public async Task ForgotPassword_SecondRequest_InvalidatesPreviousToken()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await EnsureTestUserExistsAsync(dbContext);
        await CleanupUserTokensAsync(dbContext, user.Id);

        var client = _factory.CreateClient();

        // Act - First request
        var response1 = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new
        {
            email = TestEmail
        });
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

        var firstToken = await dbContext.PasswordResetTokens
            .FirstOrDefaultAsync(t => t.UserId == user.Id && t.ExpiresAt > DateTime.UtcNow);
        Assert.NotNull(firstToken);
        var firstTokenId = firstToken.Id;
        var firstTokenExpiresAt = firstToken.ExpiresAt;

        // Act - Second request
        var response2 = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new
        {
            email = TestEmail
        });
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);

        // Assert - Reload first token and verify it's invalidated
        await dbContext.Entry(firstToken).ReloadAsync();

        // First token should be invalidated (ExpiresAt set to past or UsedAt set)
        Assert.True(firstToken.ExpiresAt <= DateTime.UtcNow,
            $"First token should be invalidated (ExpiresAt: {firstToken.ExpiresAt:O}, Now: {DateTime.UtcNow:O})");

        // New token should exist and be valid
        var activeTokens = await dbContext.PasswordResetTokens
            .Where(t => t.UserId == user.Id && t.ExpiresAt > DateTime.UtcNow && t.UsedAt == null)
            .ToListAsync();

        Assert.Single(activeTokens);
        Assert.NotEqual(firstTokenId, activeTokens[0].Id);
    }

    [Fact]
    public async Task ForgotPassword_NonExistingEmail_ReturnsOk_NoTokenCreated()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange
        var nonExistingEmail = $"nonexistent-{Guid.NewGuid()}@example.com";
        var client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var tokenCountBefore = await dbContext.PasswordResetTokens.CountAsync();

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new
        {
            email = nonExistingEmail
        });

        // Assert - Same response as existing email (no enumeration)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("If the email exists", content);

        // No new token should be created
        var tokenCountAfter = await dbContext.PasswordResetTokens.CountAsync();
        Assert.Equal(tokenCountBefore, tokenCountAfter);
    }

    [Fact]
    public async Task ForgotPassword_ExistingVsNonExisting_ResponsesIndistinguishable()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await EnsureTestUserExistsAsync(dbContext);

        var client = _factory.CreateClient();
        var nonExistingEmail = $"nonexistent-{Guid.NewGuid()}@example.com";

        // Act
        var responseExisting = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new
        {
            email = TestEmail
        });

        var responseNonExisting = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new
        {
            email = nonExistingEmail
        });

        // Assert - Both should return 200 with same message structure
        Assert.Equal(HttpStatusCode.OK, responseExisting.StatusCode);
        Assert.Equal(HttpStatusCode.OK, responseNonExisting.StatusCode);

        var contentExisting = await responseExisting.Content.ReadAsStringAsync();
        var contentNonExisting = await responseNonExisting.Content.ReadAsStringAsync();

        // Both should contain the same generic message
        Assert.Contains("If the email exists", contentExisting);
        Assert.Contains("If the email exists", contentNonExisting);

        // Response structure should be identical
        Assert.Equal(contentExisting, contentNonExisting);
    }

    [Fact]
    public async Task ForgotPassword_TokenUsesSecureRandomGeneration()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await EnsureTestUserExistsAsync(dbContext);
        await CleanupUserTokensAsync(dbContext, user.Id);

        var client = _factory.CreateClient();

        // Act - Generate multiple tokens and verify they're unique
        var tokenHashes = new List<string>();

        for (int i = 0; i < 3; i++)
        {
            // Clean up between requests to avoid rate limiting
            await CleanupUserTokensAsync(dbContext, user.Id);

            var response = await client.PostAsJsonAsync("/api/v1/auth/forgot-password", new
            {
                email = TestEmail
            });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var token = await dbContext.PasswordResetTokens
                .FirstOrDefaultAsync(t => t.UserId == user.Id);

            Assert.NotNull(token);
            tokenHashes.Add(token.TokenHash);
        }

        // Assert - All token hashes should be unique (cryptographically random)
        Assert.Equal(tokenHashes.Count, tokenHashes.Distinct().Count());
    }

    [Fact]
    public async Task PasswordResetTokenService_GenerateToken_ProducesValidHash()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await EnsureTestUserExistsAsync(dbContext);
        await CleanupUserTokensAsync(dbContext, user.Id);

        var tokenService = scope.ServiceProvider.GetRequiredService<IPasswordResetTokenService>();

        // Act
        var result = await tokenService.GenerateTokenAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.PlainToken);
        Assert.NotEqual(Guid.Empty, result.TokenId);
        Assert.True(result.ExpiresAt > DateTime.UtcNow);

        // Verify the stored hash matches what we'd compute from the plain token
        var token = await dbContext.PasswordResetTokens.FindAsync(result.TokenId);
        Assert.NotNull(token);

        var expectedHash = PasswordResetTokenService.ComputeTokenHash(result.PlainToken);
        Assert.Equal(expectedHash, token.TokenHash);
    }

    [Fact]
    public async Task PasswordResetTokenService_InvalidatePreviousTokens_SetsExpiryToNow()
    {
        if (!IsPostgreSqlAvailable()) return;

        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await EnsureTestUserExistsAsync(dbContext);
        await CleanupUserTokensAsync(dbContext, user.Id);

        var tokenService = scope.ServiceProvider.GetRequiredService<IPasswordResetTokenService>();

        // Create initial token
        var firstResult = await tokenService.GenerateTokenAsync(user.Id);
        var firstTokenId = firstResult.TokenId;

        // Act - Generate second token (should invalidate first)
        var secondResult = await tokenService.GenerateTokenAsync(user.Id);

        // Assert
        var firstToken = await dbContext.PasswordResetTokens.FindAsync(firstTokenId);
        Assert.NotNull(firstToken);
        Assert.True(firstToken.ExpiresAt <= DateTime.UtcNow,
            "First token should be invalidated");

        var secondToken = await dbContext.PasswordResetTokens.FindAsync(secondResult.TokenId);
        Assert.NotNull(secondToken);
        Assert.True(secondToken.ExpiresAt > DateTime.UtcNow,
            "Second token should still be valid");
    }
}
