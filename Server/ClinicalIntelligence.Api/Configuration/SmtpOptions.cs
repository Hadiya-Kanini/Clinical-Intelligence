namespace ClinicalIntelligence.Api.Configuration;

/// <summary>
/// Strongly typed SMTP configuration options for email sending.
/// Loaded from environment variables or configuration providers.
/// </summary>
public sealed class SmtpOptions
{
    /// <summary>
    /// Configuration section name for binding.
    /// </summary>
    public const string SectionName = "Smtp";

    /// <summary>
    /// SMTP server hostname.
    /// Environment variable: SMTP_HOST
    /// </summary>
    public string? Host { get; set; }

    /// <summary>
    /// SMTP server port. Default is 587 for TLS/STARTTLS.
    /// Environment variable: SMTP_PORT
    /// </summary>
    public int Port { get; set; } = 587;

    /// <summary>
    /// SMTP authentication username.
    /// Environment variable: SMTP_USERNAME
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// SMTP authentication password.
    /// Environment variable: SMTP_PASSWORD
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Email address to send from.
    /// Environment variable: SMTP_FROM_EMAIL
    /// </summary>
    public string? FromEmail { get; set; }

    /// <summary>
    /// Display name for the sender.
    /// Environment variable: SMTP_FROM_NAME
    /// </summary>
    public string FromName { get; set; } = "Clinical Intelligence";

    /// <summary>
    /// Enable TLS/SSL for SMTP connection. Default is true.
    /// When true, uses STARTTLS (TLS 1.2+ enforced).
    /// Environment variable: SMTP_ENABLE_SSL
    /// </summary>
    public bool EnableSsl { get; set; } = true;

    /// <summary>
    /// Connection timeout in seconds. Default is 30 seconds.
    /// Environment variable: SMTP_TIMEOUT_SECONDS
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Indicates whether SMTP is fully configured and ready to use.
    /// </summary>
    public bool IsConfigured => !string.IsNullOrWhiteSpace(Host) && 
                                 !string.IsNullOrWhiteSpace(FromEmail);

    /// <summary>
    /// Creates SmtpOptions from configuration/environment variables.
    /// </summary>
    public static SmtpOptions FromConfiguration(IConfiguration configuration)
    {
        return new SmtpOptions
        {
            Host = configuration["SMTP_HOST"],
            Port = int.TryParse(configuration["SMTP_PORT"], out var port) ? port : 587,
            Username = configuration["SMTP_USERNAME"],
            Password = configuration["SMTP_PASSWORD"],
            FromEmail = configuration["SMTP_FROM_EMAIL"],
            FromName = configuration["SMTP_FROM_NAME"] ?? "Clinical Intelligence",
            EnableSsl = !string.Equals(configuration["SMTP_ENABLE_SSL"], "false", StringComparison.OrdinalIgnoreCase),
            TimeoutSeconds = int.TryParse(configuration["SMTP_TIMEOUT_SECONDS"], out var timeout) ? timeout : 30
        };
    }

    /// <summary>
    /// Validates the SMTP configuration.
    /// </summary>
    /// <returns>True if configured, false if not configured (email disabled).</returns>
    /// <exception cref="InvalidOperationException">Thrown when partially configured.</exception>
    public bool Validate()
    {
        var hasHost = !string.IsNullOrWhiteSpace(Host);
        var hasFromEmail = !string.IsNullOrWhiteSpace(FromEmail);

        if (!hasHost && !hasFromEmail)
        {
            return false;
        }

        if (!hasHost)
        {
            throw new InvalidOperationException(
                "Missing required SMTP configuration: SMTP_HOST must be provided when SMTP_FROM_EMAIL is set.");
        }

        if (!hasFromEmail)
        {
            throw new InvalidOperationException(
                "Missing required SMTP configuration: SMTP_FROM_EMAIL must be provided when SMTP_HOST is set.");
        }

        if (Port < 1 || Port > 65535)
        {
            throw new InvalidOperationException(
                $"Invalid SMTP port: {Port}. Must be between 1 and 65535.");
        }

        return true;
    }
}
