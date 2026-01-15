namespace ClinicalIntelligence.Api.Services.Auth;

/// <summary>
/// Service interface for generating and managing password reset tokens.
/// Implements secure token generation with cryptographic randomness,
/// hash-only storage, and 1-hour expiration policy.
/// </summary>
public interface IPasswordResetTokenService
{
    /// <summary>
    /// Generates a new password reset token for the specified user.
    /// Invalidates any existing active tokens for the user before creating a new one.
    /// </summary>
    /// <param name="userId">The user ID requesting password reset.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A result containing the plain token (for email delivery) and token ID.
    /// The plain token is never persisted - only its hash is stored.
    /// </returns>
    Task<PasswordResetTokenResult> GenerateTokenAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates all active (unused, non-expired) password reset tokens for a user.
    /// </summary>
    /// <param name="userId">The user ID whose tokens should be invalidated.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The count of tokens invalidated.</returns>
    Task<int> InvalidatePreviousTokensAsync(Guid userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of password reset token generation.
/// </summary>
public sealed record PasswordResetTokenResult
{
    /// <summary>
    /// The plain token to be sent to the user via email.
    /// This value is never persisted or logged.
    /// </summary>
    public required string PlainToken { get; init; }

    /// <summary>
    /// The database ID of the created token record.
    /// </summary>
    public required Guid TokenId { get; init; }

    /// <summary>
    /// The expiration time of the token (UTC).
    /// </summary>
    public required DateTime ExpiresAt { get; init; }
}
