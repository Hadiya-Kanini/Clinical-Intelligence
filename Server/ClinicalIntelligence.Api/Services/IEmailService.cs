namespace ClinicalIntelligence.Api.Services;

/// <summary>
/// Interface for email service operations.
/// Supports password reset emails and account notifications.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends a generic email asynchronously.
    /// </summary>
    /// <param name="to">Recipient email address.</param>
    /// <param name="subject">Email subject line.</param>
    /// <param name="body">Email body content.</param>
    /// <param name="isHtml">Whether the body is HTML formatted (default: true).</param>
    /// <returns>True if email was sent successfully, false otherwise.</returns>
    Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true);

    /// <summary>
    /// Sends a password reset email with reset link.
    /// </summary>
    /// <param name="to">Recipient email address.</param>
    /// <param name="resetToken">Plain text reset token for URL.</param>
    /// <param name="userName">User's display name.</param>
    /// <param name="resetUrl">Full URL for password reset.</param>
    /// <returns>True if email was sent successfully, false otherwise.</returns>
    Task<bool> SendPasswordResetEmailAsync(string to, string resetToken, string userName, string resetUrl);

    /// <summary>
    /// Sends a password reset confirmation email after successful reset.
    /// </summary>
    /// <param name="to">Recipient email address.</param>
    /// <param name="userName">User's display name.</param>
    /// <returns>True if email was sent successfully, false otherwise.</returns>
    Task<bool> SendPasswordResetConfirmationAsync(string to, string userName);

    /// <summary>
    /// Sends an account locked notification email.
    /// </summary>
    /// <param name="to">Recipient email address.</param>
    /// <param name="userName">User's display name.</param>
    /// <param name="lockedUntil">Timestamp when account will be unlocked.</param>
    /// <returns>True if email was sent successfully, false otherwise.</returns>
    Task<bool> SendAccountLockedEmailAsync(string to, string userName, DateTime lockedUntil);

    /// <summary>
    /// Indicates whether the email service is configured and available.
    /// </summary>
    bool IsConfigured { get; }

    /// <summary>
    /// Sends a new user credentials email with login information.
    /// </summary>
    /// <param name="to">Recipient email address.</param>
    /// <param name="userName">User's display name.</param>
    /// <param name="temporaryPassword">The temporary password for initial login.</param>
    /// <returns>True if email was sent successfully, false otherwise.</returns>
    Task<bool> SendNewUserCredentialsEmailAsync(string to, string userName, string temporaryPassword);
}
