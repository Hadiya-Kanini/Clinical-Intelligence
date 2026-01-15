using System.Security.Cryptography;
using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicalIntelligence.Api.Services.Auth;

/// <summary>
/// Implementation of password reset token service using cryptographically secure
/// random generation, SHA-256 hashing for storage, and 1-hour expiration policy.
/// </summary>
public sealed class PasswordResetTokenService : IPasswordResetTokenService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<PasswordResetTokenService>? _logger;

    private const int TokenByteLength = 32;
    private const int TokenExpirationHours = 1;

    public PasswordResetTokenService(
        ApplicationDbContext dbContext,
        ILogger<PasswordResetTokenService>? logger = null)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PasswordResetTokenResult> GenerateTokenAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Invalidate previous tokens first
        await InvalidatePreviousTokensAsync(userId, cancellationToken);

        // Generate cryptographically secure random token
        var plainToken = GenerateSecureToken();

        // Compute hash for storage (never store plain token)
        var tokenHash = ComputeTokenHash(plainToken);

        var expiresAt = DateTime.UtcNow.AddHours(TokenExpirationHours);

        var resetToken = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
            UsedAt = null
        };

        _dbContext.PasswordResetTokens.Add(resetToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger?.LogInformation(
            "Password reset token generated for user {UserId}, expires at {ExpiresAt}",
            userId,
            expiresAt);

        return new PasswordResetTokenResult
        {
            PlainToken = plainToken,
            TokenId = resetToken.Id,
            ExpiresAt = expiresAt
        };
    }

    /// <inheritdoc />
    public async Task<int> InvalidatePreviousTokensAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var activeTokens = await _dbContext.PasswordResetTokens
            .Where(t => t.UserId == userId && t.UsedAt == null && t.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        if (activeTokens.Count == 0)
        {
            return 0;
        }

        foreach (var token in activeTokens)
        {
            // Invalidate by setting expiration to now
            token.ExpiresAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger?.LogInformation(
            "Invalidated {Count} previous password reset tokens for user {UserId}",
            activeTokens.Count,
            userId);

        return activeTokens.Count;
    }

    /// <summary>
    /// Generates a cryptographically secure URL-safe token.
    /// Uses RandomNumberGenerator for secure random bytes.
    /// </summary>
    private static string GenerateSecureToken()
    {
        var tokenBytes = RandomNumberGenerator.GetBytes(TokenByteLength);
        // Use Base64Url encoding for URL-safe token
        return Convert.ToBase64String(tokenBytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    /// <summary>
    /// Computes SHA-256 hash of the token for secure storage.
    /// </summary>
    public static string ComputeTokenHash(string plainToken)
    {
        var tokenBytes = System.Text.Encoding.UTF8.GetBytes(plainToken);
        var hashBytes = SHA256.HashData(tokenBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
