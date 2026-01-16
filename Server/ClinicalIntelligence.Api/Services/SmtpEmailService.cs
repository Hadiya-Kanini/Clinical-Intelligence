using ClinicalIntelligence.Api.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace ClinicalIntelligence.Api.Services;

/// <summary>
/// SMTP-based email service implementation using MailKit.
/// Supports password reset emails and account notifications with retry logic.
/// </summary>
public sealed class SmtpEmailService : IEmailService
{
    private readonly ILogger<SmtpEmailService> _logger;
    private readonly SecretsOptions _secrets;
    private const int MaxRetryAttempts = 3;
    private static readonly TimeSpan[] RetryDelays = { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(4) };

    public SmtpEmailService(ILogger<SmtpEmailService> logger, SecretsOptions secrets)
    {
        _logger = logger;
        _secrets = secrets;
    }

    /// <inheritdoc />
    public bool IsConfigured => !string.IsNullOrWhiteSpace(_secrets.SmtpHost) && 
                                 !string.IsNullOrWhiteSpace(_secrets.SmtpFromEmail);

    /// <inheritdoc />
    public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        if (!IsConfigured)
        {
            _logger.LogWarning("Email service is not configured. Skipping email to {To}", to);
            return false;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_secrets.SmtpFromName, _secrets.SmtpFromEmail));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder();
        if (isHtml)
        {
            bodyBuilder.HtmlBody = body;
            bodyBuilder.TextBody = StripHtml(body);
        }
        else
        {
            bodyBuilder.TextBody = body;
        }
        message.Body = bodyBuilder.ToMessageBody();

        return await SendWithRetryAsync(message, to);
    }

    /// <inheritdoc />
    public async Task<bool> SendPasswordResetEmailAsync(string to, string resetToken, string userName, string resetUrl)
    {
        var subject = "Reset Your Password - Clinical Intelligence";
        var htmlBody = GeneratePasswordResetEmailHtml(userName, resetUrl);
        
        _logger.LogInformation("Sending password reset email to {Email}", to);
        return await SendEmailAsync(to, subject, htmlBody, isHtml: true);
    }

    /// <inheritdoc />
    public async Task<bool> SendPasswordResetConfirmationAsync(string to, string userName)
    {
        var subject = "Password Successfully Changed - Clinical Intelligence";
        var htmlBody = GeneratePasswordResetConfirmationHtml(userName);
        
        _logger.LogInformation("Sending password reset confirmation email to {Email}", to);
        return await SendEmailAsync(to, subject, htmlBody, isHtml: true);
    }

    /// <inheritdoc />
    public async Task<bool> SendAccountLockedEmailAsync(string to, string userName, DateTime lockedUntil)
    {
        var subject = "Account Temporarily Locked - Clinical Intelligence";
        var htmlBody = GenerateAccountLockedEmailHtml(userName, lockedUntil);
        
        _logger.LogInformation("Sending account locked email to {Email}", to);
        return await SendEmailAsync(to, subject, htmlBody, isHtml: true);
    }

    /// <inheritdoc />
    public async Task<bool> SendNewUserCredentialsEmailAsync(string to, string userName, string temporaryPassword)
    {
        var subject = "Your Clinical Intelligence Account Credentials";
        var htmlBody = GenerateNewUserCredentialsEmailHtml(userName, to, temporaryPassword);
        
        _logger.LogInformation("Sending new user credentials email to {Email}", to);
        return await SendEmailAsync(to, subject, htmlBody, isHtml: true);
    }

    private async Task<bool> SendWithRetryAsync(MimeMessage message, string recipientEmail)
    {
        for (var attempt = 0; attempt < MaxRetryAttempts; attempt++)
        {
            try
            {
                using var client = new SmtpClient();
                
                var secureSocketOptions = _secrets.SmtpEnableSsl 
                    ? SecureSocketOptions.StartTls 
                    : SecureSocketOptions.None;

                await client.ConnectAsync(_secrets.SmtpHost, _secrets.SmtpPort, secureSocketOptions);

                if (!string.IsNullOrWhiteSpace(_secrets.SmtpUsername) && 
                    !string.IsNullOrWhiteSpace(_secrets.SmtpPassword))
                {
                    await client.AuthenticateAsync(_secrets.SmtpUsername, _secrets.SmtpPassword);
                }

                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation(
                    "Email sent successfully to {Recipient} on attempt {Attempt}",
                    recipientEmail, attempt + 1);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to send email to {Recipient} on attempt {Attempt}/{MaxAttempts}",
                    recipientEmail, attempt + 1, MaxRetryAttempts);

                if (attempt < MaxRetryAttempts - 1)
                {
                    await Task.Delay(RetryDelays[attempt]);
                }
            }
        }

        _logger.LogError(
            "Failed to send email to {Recipient} after {MaxAttempts} attempts",
            recipientEmail, MaxRetryAttempts);
        return false;
    }

    private static string GeneratePasswordResetEmailHtml(string userName, string resetUrl)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Reset Your Password</title>
</head>
<body style=""margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; background-color: #f5f5f5;"">
    <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""max-width: 600px; margin: 0 auto; padding: 40px 20px;"">
        <tr>
            <td style=""background-color: #ffffff; border-radius: 8px; padding: 40px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);"">
                <h1 style=""color: #1a1a1a; font-size: 24px; margin: 0 0 24px 0;"">Reset Your Password</h1>
                
                <p style=""color: #4a4a4a; font-size: 16px; line-height: 1.6; margin: 0 0 16px 0;"">
                    Hi {System.Net.WebUtility.HtmlEncode(userName)},
                </p>
                
                <p style=""color: #4a4a4a; font-size: 16px; line-height: 1.6; margin: 0 0 24px 0;"">
                    We received a request to reset your password for your Clinical Intelligence account. Click the button below to create a new password:
                </p>
                
                <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" style=""margin: 0 0 24px 0;"">
                    <tr>
                        <td style=""background-color: #2563eb; border-radius: 6px;"">
                            <a href=""{System.Net.WebUtility.HtmlEncode(resetUrl)}"" style=""display: inline-block; padding: 14px 28px; color: #ffffff; text-decoration: none; font-size: 16px; font-weight: 600;"">Reset Password</a>
                        </td>
                    </tr>
                </table>
                
                <p style=""color: #6b7280; font-size: 14px; line-height: 1.6; margin: 0 0 16px 0;"">
                    This link will expire in <strong>1 hour</strong>. If you didn't request a password reset, you can safely ignore this email.
                </p>
                
                <p style=""color: #6b7280; font-size: 14px; line-height: 1.6; margin: 0 0 16px 0;"">
                    If the button doesn't work, copy and paste this link into your browser:
                </p>
                
                <p style=""color: #2563eb; font-size: 14px; word-break: break-all; margin: 0 0 24px 0;"">
                    {System.Net.WebUtility.HtmlEncode(resetUrl)}
                </p>
                
                <hr style=""border: none; border-top: 1px solid #e5e7eb; margin: 24px 0;"">
                
                <p style=""color: #9ca3af; font-size: 12px; line-height: 1.5; margin: 0;"">
                    <strong>Security Tips:</strong><br>
                    • Never share this link with anyone<br>
                    • Clinical Intelligence will never ask for your password via email<br>
                    • If you didn't request this reset, please contact support
                </p>
            </td>
        </tr>
        <tr>
            <td style=""padding: 24px; text-align: center;"">
                <p style=""color: #9ca3af; font-size: 12px; margin: 0;"">
                    © {DateTime.UtcNow.Year} Clinical Intelligence. All rights reserved.
                </p>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    private static string GeneratePasswordResetConfirmationHtml(string userName)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Password Changed Successfully</title>
</head>
<body style=""margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; background-color: #f5f5f5;"">
    <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""max-width: 600px; margin: 0 auto; padding: 40px 20px;"">
        <tr>
            <td style=""background-color: #ffffff; border-radius: 8px; padding: 40px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);"">
                <div style=""text-align: center; margin-bottom: 24px;"">
                    <div style=""display: inline-block; width: 64px; height: 64px; background-color: #dcfce7; border-radius: 50%; line-height: 64px; font-size: 32px;"">✓</div>
                </div>
                
                <h1 style=""color: #1a1a1a; font-size: 24px; margin: 0 0 24px 0; text-align: center;"">Password Changed Successfully</h1>
                
                <p style=""color: #4a4a4a; font-size: 16px; line-height: 1.6; margin: 0 0 16px 0;"">
                    Hi {System.Net.WebUtility.HtmlEncode(userName)},
                </p>
                
                <p style=""color: #4a4a4a; font-size: 16px; line-height: 1.6; margin: 0 0 24px 0;"">
                    Your password has been successfully changed. You can now log in to your Clinical Intelligence account with your new password.
                </p>
                
                <p style=""color: #6b7280; font-size: 14px; line-height: 1.6; margin: 0 0 16px 0;"">
                    <strong>Didn't make this change?</strong><br>
                    If you didn't reset your password, please contact our support team immediately as your account may have been compromised.
                </p>
                
                <hr style=""border: none; border-top: 1px solid #e5e7eb; margin: 24px 0;"">
                
                <p style=""color: #9ca3af; font-size: 12px; line-height: 1.5; margin: 0;"">
                    This is an automated security notification from Clinical Intelligence.
                </p>
            </td>
        </tr>
        <tr>
            <td style=""padding: 24px; text-align: center;"">
                <p style=""color: #9ca3af; font-size: 12px; margin: 0;"">
                    © {DateTime.UtcNow.Year} Clinical Intelligence. All rights reserved.
                </p>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    private static string GenerateAccountLockedEmailHtml(string userName, DateTime lockedUntil)
    {
        var unlockTimeFormatted = lockedUntil.ToString("MMMM d, yyyy 'at' h:mm tt 'UTC'");
        
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Account Temporarily Locked</title>
</head>
<body style=""margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; background-color: #f5f5f5;"">
    <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""max-width: 600px; margin: 0 auto; padding: 40px 20px;"">
        <tr>
            <td style=""background-color: #ffffff; border-radius: 8px; padding: 40px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);"">
                <div style=""text-align: center; margin-bottom: 24px;"">
                    <div style=""display: inline-block; width: 64px; height: 64px; background-color: #fef3c7; border-radius: 50%; line-height: 64px; font-size: 32px;"">⚠</div>
                </div>
                
                <h1 style=""color: #1a1a1a; font-size: 24px; margin: 0 0 24px 0; text-align: center;"">Account Temporarily Locked</h1>
                
                <p style=""color: #4a4a4a; font-size: 16px; line-height: 1.6; margin: 0 0 16px 0;"">
                    Hi {System.Net.WebUtility.HtmlEncode(userName)},
                </p>
                
                <p style=""color: #4a4a4a; font-size: 16px; line-height: 1.6; margin: 0 0 24px 0;"">
                    Your Clinical Intelligence account has been temporarily locked due to multiple failed login attempts. This is a security measure to protect your account.
                </p>
                
                <p style=""color: #4a4a4a; font-size: 16px; line-height: 1.6; margin: 0 0 24px 0;"">
                    Your account will be automatically unlocked on:<br>
                    <strong>{unlockTimeFormatted}</strong>
                </p>
                
                <p style=""color: #6b7280; font-size: 14px; line-height: 1.6; margin: 0 0 16px 0;"">
                    <strong>Wasn't you?</strong><br>
                    If you didn't attempt to log in, someone else may be trying to access your account. We recommend resetting your password once your account is unlocked.
                </p>
                
                <hr style=""border: none; border-top: 1px solid #e5e7eb; margin: 24px 0;"">
                
                <p style=""color: #9ca3af; font-size: 12px; line-height: 1.5; margin: 0;"">
                    This is an automated security notification from Clinical Intelligence.
                </p>
            </td>
        </tr>
        <tr>
            <td style=""padding: 24px; text-align: center;"">
                <p style=""color: #9ca3af; font-size: 12px; margin: 0;"">
                    © {DateTime.UtcNow.Year} Clinical Intelligence. All rights reserved.
                </p>
            </td>
        </tr>
    </table>
</body>
</html>";
    }

    private static string GenerateNewUserCredentialsEmailHtml(string userName, string email, string temporaryPassword)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Your Account Credentials</title>
</head>
<body style=""margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; background-color: #f5f5f5;"">
    <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""max-width: 600px; margin: 0 auto; padding: 40px 20px;"">
        <tr>
            <td style=""background-color: #ffffff; border-radius: 8px; padding: 40px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);"">
                <h1 style=""color: #1a1a1a; font-size: 24px; margin: 0 0 24px 0;"">Welcome to Clinical Intelligence</h1>
                
                <p style=""color: #4a4a4a; font-size: 16px; line-height: 1.6; margin: 0 0 16px 0;"">
                    Hi {System.Net.WebUtility.HtmlEncode(userName)},
                </p>
                
                <p style=""color: #4a4a4a; font-size: 16px; line-height: 1.6; margin: 0 0 24px 0;"">
                    Your Clinical Intelligence account has been created. Below are your login credentials:
                </p>
                
                <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" style=""margin: 0 0 24px 0; background-color: #f8f9fa; border-radius: 6px; padding: 20px; width: 100%;"">
                    <tr>
                        <td style=""padding: 8px 0;"">
                            <strong style=""color: #4a4a4a;"">Email:</strong>
                            <span style=""color: #1a1a1a; margin-left: 8px;"">{System.Net.WebUtility.HtmlEncode(email)}</span>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 8px 0;"">
                            <strong style=""color: #4a4a4a;"">Temporary Password:</strong>
                            <code style=""color: #1a1a1a; margin-left: 8px; background-color: #e9ecef; padding: 4px 8px; border-radius: 4px; font-family: monospace;"">{System.Net.WebUtility.HtmlEncode(temporaryPassword)}</code>
                        </td>
                    </tr>
                </table>
                
                <div style=""background-color: #fff3cd; border: 1px solid #ffc107; border-radius: 6px; padding: 16px; margin: 0 0 24px 0;"">
                    <p style=""color: #856404; font-size: 14px; line-height: 1.5; margin: 0;"">
                        <strong>⚠ Security Notice:</strong><br>
                        For your security, please change your password immediately after your first login.
                    </p>
                </div>
                
                <hr style=""border: none; border-top: 1px solid #e5e7eb; margin: 24px 0;"">
                
                <p style=""color: #9ca3af; font-size: 12px; line-height: 1.5; margin: 0;"">
                    <strong>Security Tips:</strong><br>
                    • Never share your password with anyone<br>
                    • Clinical Intelligence will never ask for your password via email<br>
                    • If you did not expect this email, please contact support
                </p>
            </td>
        </tr>
        <tr>
            <td style=""padding: 24px; text-align: center;"">
                <p style=""color: #9ca3af; font-size: 12px; margin: 0;"">
                    © {DateTime.UtcNow.Year} Clinical Intelligence. All rights reserved.
                </p>
            </td>
        </tr>
    </table>
</body>
</html>";
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
