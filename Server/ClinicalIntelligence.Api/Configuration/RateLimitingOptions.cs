namespace ClinicalIntelligence.Api.Configuration;

/// <summary>
/// Configuration options for rate limiting.
/// Login defaults aligned with US_015: 5 attempts per 60 seconds per IP.
/// Forgot-password defaults aligned with US_028: 3 attempts per 3600 seconds (1 hour) per IP.
/// </summary>
public sealed class RateLimitingOptions
{
    /// <summary>
    /// Configuration section name for binding.
    /// </summary>
    public const string SectionName = "RateLimiting";

    /// <summary>
    /// Policy name for login rate limiting.
    /// </summary>
    public const string LoginPolicyName = "LoginRateLimit";

    /// <summary>
    /// Policy name for forgot-password rate limiting.
    /// </summary>
    public const string ForgotPasswordPolicyName = "ForgotPasswordRateLimit";

    /// <summary>
    /// Maximum number of login attempts allowed within the window.
    /// Default: 5 attempts per US_015 requirements.
    /// </summary>
    public int LoginPermitLimit { get; set; } = 5;

    /// <summary>
    /// Time window in seconds for rate limiting.
    /// Default: 60 seconds (1 minute) per US_015 requirements.
    /// </summary>
    public int LoginWindowSeconds { get; set; } = 60;

    /// <summary>
    /// Gets the login window as a TimeSpan.
    /// </summary>
    public TimeSpan LoginWindow => TimeSpan.FromSeconds(LoginWindowSeconds);

    /// <summary>
    /// Maximum number of forgot-password attempts allowed within the window.
    /// Default: 3 attempts per US_028 requirements.
    /// </summary>
    public int ForgotPasswordPermitLimit { get; set; } = 3;

    /// <summary>
    /// Time window in seconds for forgot-password rate limiting.
    /// Default: 3600 seconds (1 hour) per US_028 requirements.
    /// </summary>
    public int ForgotPasswordWindowSeconds { get; set; } = 3600;

    /// <summary>
    /// Gets the forgot-password window as a TimeSpan.
    /// </summary>
    public TimeSpan ForgotPasswordWindow => TimeSpan.FromSeconds(ForgotPasswordWindowSeconds);
}
