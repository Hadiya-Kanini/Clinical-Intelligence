using System.Net.Security;
using System.Security.Authentication;
using ClinicalIntelligence.Api.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace ClinicalIntelligence.Api.Services.Email;

/// <summary>
/// SMTP email sender implementation using MailKit.
/// Enforces TLS 1.2+ for secure transmission per security requirements.
/// Provides structured logging for send attempts and outcomes.
/// </summary>
public sealed class SmtpEmailSender : ISmtpEmailSender
{
    private readonly SmtpOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;
    private const int MaxRetryAttempts = 3;
    private static readonly TimeSpan[] RetryDelays = { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(4) };

    public SmtpEmailSender(SmtpOptions options, ILogger<SmtpEmailSender> logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public bool IsConfigured => _options.IsConfigured;

    /// <inheritdoc />
    public async Task<bool> SendAsync(
        string to,
        string subject,
        string htmlBody,
        string? textBody = null,
        CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            _logger.LogWarning(
                "SMTP email sender is not configured. Email to {RecipientDomain} skipped",
                GetDomainForLogging(to));
            return false;
        }

        if (string.IsNullOrWhiteSpace(to))
        {
            _logger.LogWarning("Cannot send email: recipient address is empty");
            return false;
        }

        var message = BuildMessage(to, subject, htmlBody, textBody);
        return await SendWithRetryAsync(message, to, cancellationToken);
    }

    private MimeMessage BuildMessage(string to, string subject, string htmlBody, string? textBody)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.FromName, _options.FromEmail));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = htmlBody,
            TextBody = textBody ?? StripHtml(htmlBody)
        };
        message.Body = bodyBuilder.ToMessageBody();

        return message;
    }

    private async Task<bool> SendWithRetryAsync(MimeMessage message, string recipientEmail, CancellationToken cancellationToken)
    {
        var recipientDomain = GetDomainForLogging(recipientEmail);

        for (var attempt = 0; attempt < MaxRetryAttempts; attempt++)
        {
            try
            {
                using var client = new SmtpClient();

                // Configure TLS 1.2+ enforcement
                client.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;

                // Set timeout
                client.Timeout = _options.TimeoutSeconds * 1000;

                // Determine secure socket options based on configuration
                var secureSocketOptions = _options.EnableSsl
                    ? SecureSocketOptions.StartTls
                    : SecureSocketOptions.None;

                _logger.LogDebug(
                    "Connecting to SMTP server {Host}:{Port} with TLS={EnableSsl}",
                    _options.Host, _options.Port, _options.EnableSsl);

                await client.ConnectAsync(
                    _options.Host,
                    _options.Port,
                    secureSocketOptions,
                    cancellationToken);

                // Authenticate if credentials provided
                if (!string.IsNullOrWhiteSpace(_options.Username) &&
                    !string.IsNullOrWhiteSpace(_options.Password))
                {
                    await client.AuthenticateAsync(_options.Username, _options.Password, cancellationToken);
                }

                await client.SendAsync(message, cancellationToken);
                await client.DisconnectAsync(true, cancellationToken);

                _logger.LogInformation(
                    "Email sent successfully to domain {RecipientDomain} on attempt {Attempt}",
                    recipientDomain, attempt + 1);

                return true;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning(
                    "Email send to {RecipientDomain} cancelled on attempt {Attempt}",
                    recipientDomain, attempt + 1);
                throw;
            }
            catch (System.Security.Authentication.AuthenticationException ex)
            {
                _logger.LogError(
                    ex,
                    "TLS authentication failed for SMTP connection. Ensure TLS 1.2+ is supported by the server");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to send email to {RecipientDomain} on attempt {Attempt}/{MaxAttempts}. Error: {ErrorType}",
                    recipientDomain, attempt + 1, MaxRetryAttempts, ex.GetType().Name);

                if (attempt < MaxRetryAttempts - 1)
                {
                    try
                    {
                        await Task.Delay(RetryDelays[attempt], cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                }
            }
        }

        _logger.LogError(
            "Failed to send email to {RecipientDomain} after {MaxAttempts} attempts",
            recipientDomain, MaxRetryAttempts);

        return false;
    }

    private static string GetDomainForLogging(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return "[empty]";

        var atIndex = email.IndexOf('@');
        return atIndex > 0 && atIndex < email.Length - 1
            ? email[(atIndex + 1)..]
            : "[invalid]";
    }

    private static string StripHtml(string html)
    {
        if (string.IsNullOrEmpty(html))
            return string.Empty;

        var text = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", " ");
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ");
        return System.Net.WebUtility.HtmlDecode(text).Trim();
    }
}
