namespace ClinicalIntelligence.Api.Services.Auth;

/// <summary>
/// Service interface for validating password reset tokens.
/// Encapsulates token lifecycle checks: existence, expiry, and used-state.
/// </summary>
public interface IPasswordResetTokenValidator
{
    /// <summary>
    /// Validates a password reset token without consuming it.
    /// </summary>
    /// <param name="plainToken">The plain token from the reset URL.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation result indicating validity and reason if invalid.</returns>
    Task<TokenValidationResult> ValidateTokenAsync(string plainToken, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of password reset token validation.
/// </summary>
public sealed record TokenValidationResult
{
    /// <summary>
    /// Whether the token is valid (exists, not expired, not used).
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Reason for invalidity when IsValid is false.
    /// </summary>
    public TokenInvalidReason? InvalidReason { get; init; }

    /// <summary>
    /// Token expiration time (only populated when valid).
    /// </summary>
    public DateTime? ExpiresAt { get; init; }

    /// <summary>
    /// User ID associated with the token (only populated when valid).
    /// </summary>
    public Guid? UserId { get; init; }

    /// <summary>
    /// Token ID in the database (only populated when valid).
    /// </summary>
    public Guid? TokenId { get; init; }

    /// <summary>
    /// Creates a valid result.
    /// </summary>
    public static TokenValidationResult Valid(Guid tokenId, Guid userId, DateTime expiresAt) => new()
    {
        IsValid = true,
        TokenId = tokenId,
        UserId = userId,
        ExpiresAt = expiresAt
    };

    /// <summary>
    /// Creates an invalid result with the specified reason.
    /// </summary>
    public static TokenValidationResult Invalid(TokenInvalidReason reason) => new()
    {
        IsValid = false,
        InvalidReason = reason
    };
}

/// <summary>
/// Reasons why a token may be invalid.
/// </summary>
public enum TokenInvalidReason
{
    /// <summary>
    /// Token is missing or empty.
    /// </summary>
    Missing,

    /// <summary>
    /// Token format is malformed.
    /// </summary>
    Malformed,

    /// <summary>
    /// Token does not exist in the database.
    /// </summary>
    NotFound,

    /// <summary>
    /// Token has expired.
    /// </summary>
    Expired,

    /// <summary>
    /// Token has already been used.
    /// </summary>
    AlreadyUsed,

    /// <summary>
    /// Associated user is deleted or inactive.
    /// </summary>
    UserInvalid
}
