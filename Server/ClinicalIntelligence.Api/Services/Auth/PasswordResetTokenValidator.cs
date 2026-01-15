using ClinicalIntelligence.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicalIntelligence.Api.Services.Auth;

/// <summary>
/// Implementation of password reset token validation.
/// Validates token hash existence, expiry, and used-state against the database.
/// </summary>
public sealed class PasswordResetTokenValidator : IPasswordResetTokenValidator
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<PasswordResetTokenValidator>? _logger;

    public PasswordResetTokenValidator(
        ApplicationDbContext dbContext,
        ILogger<PasswordResetTokenValidator>? logger = null)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<TokenValidationResult> ValidateTokenAsync(string plainToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(plainToken))
        {
            _logger?.LogDebug("Token validation failed: token is missing or empty");
            return TokenValidationResult.Invalid(TokenInvalidReason.Missing);
        }

        var trimmedToken = plainToken.Trim();

        if (!IsValidTokenFormat(trimmedToken))
        {
            _logger?.LogDebug("Token validation failed: malformed token format");
            return TokenValidationResult.Invalid(TokenInvalidReason.Malformed);
        }

        var tokenHash = PasswordResetTokenService.ComputeTokenHash(trimmedToken);

        var resetToken = await _dbContext.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

        if (resetToken == null)
        {
            _logger?.LogDebug("Token validation failed: token not found in database");
            return TokenValidationResult.Invalid(TokenInvalidReason.NotFound);
        }

        if (resetToken.UsedAt != null)
        {
            _logger?.LogDebug("Token validation failed: token already used");
            return TokenValidationResult.Invalid(TokenInvalidReason.AlreadyUsed);
        }

        if (resetToken.ExpiresAt <= DateTime.UtcNow)
        {
            _logger?.LogDebug("Token validation failed: token expired");
            return TokenValidationResult.Invalid(TokenInvalidReason.Expired);
        }

        var user = resetToken.User;
        if (user == null || user.IsDeleted || user.Status != "Active")
        {
            _logger?.LogDebug("Token validation failed: associated user is invalid");
            return TokenValidationResult.Invalid(TokenInvalidReason.UserInvalid);
        }

        _logger?.LogDebug("Token validation successful for user {UserId}", resetToken.UserId);
        return TokenValidationResult.Valid(resetToken.Id, resetToken.UserId, resetToken.ExpiresAt);
    }

    /// <summary>
    /// Validates the token format (Base64Url-encoded, expected length).
    /// </summary>
    private static bool IsValidTokenFormat(string token)
    {
        if (token.Length < 20 || token.Length > 100)
        {
            return false;
        }

        foreach (var c in token)
        {
            if (!char.IsLetterOrDigit(c) && c != '-' && c != '_')
            {
                return false;
            }
        }

        return true;
    }
}
