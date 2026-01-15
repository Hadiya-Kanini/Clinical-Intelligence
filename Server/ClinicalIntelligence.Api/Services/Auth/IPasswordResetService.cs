namespace ClinicalIntelligence.Api.Services.Auth;

/// <summary>
/// Service interface for applying password reset and consuming the reset token atomically.
/// </summary>
public interface IPasswordResetService
{
    /// <summary>
    /// Resets a user's password using a valid reset token.
    /// Validates the token, updates the password hash, and marks the token as used atomically.
    /// </summary>
    /// <param name="plainToken">The plain token from the reset URL.</param>
    /// <param name="newPassword">The new password to set.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure with reason.</returns>
    Task<PasswordResetResult> ResetPasswordAsync(string plainToken, string newPassword, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of password reset operation.
/// </summary>
public sealed record PasswordResetResult
{
    /// <summary>
    /// Whether the password reset was successful.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Error code when Success is false.
    /// </summary>
    public string? ErrorCode { get; init; }

    /// <summary>
    /// Human-readable error message when Success is false.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Additional error details when Success is false.
    /// </summary>
    public string[]? ErrorDetails { get; init; }

    /// <summary>
    /// User ID whose password was reset (only populated on success).
    /// </summary>
    public Guid? UserId { get; init; }

    /// <summary>
    /// User email for confirmation email (only populated on success).
    /// </summary>
    public string? UserEmail { get; init; }

    /// <summary>
    /// User name for confirmation email (only populated on success).
    /// </summary>
    public string? UserName { get; init; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static PasswordResetResult Succeeded(Guid userId, string userEmail, string userName) => new()
    {
        Success = true,
        UserId = userId,
        UserEmail = userEmail,
        UserName = userName
    };

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    public static PasswordResetResult Failed(string errorCode, string errorMessage, string[]? details = null) => new()
    {
        Success = false,
        ErrorCode = errorCode,
        ErrorMessage = errorMessage,
        ErrorDetails = details ?? Array.Empty<string>()
    };
}
