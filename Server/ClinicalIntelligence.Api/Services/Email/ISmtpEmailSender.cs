namespace ClinicalIntelligence.Api.Services.Email;

/// <summary>
/// Abstraction for sending emails via SMTP.
/// Follows Dependency Inversion Principle (DIP) for testability.
/// </summary>
public interface ISmtpEmailSender
{
    /// <summary>
    /// Sends an email asynchronously via SMTP.
    /// </summary>
    /// <param name="to">Recipient email address.</param>
    /// <param name="subject">Email subject line.</param>
    /// <param name="htmlBody">HTML body content.</param>
    /// <param name="textBody">Optional plain text body (fallback).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if email was sent successfully, false otherwise.</returns>
    Task<bool> SendAsync(
        string to,
        string subject,
        string htmlBody,
        string? textBody = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Indicates whether the SMTP sender is configured and available.
    /// </summary>
    bool IsConfigured { get; }
}
