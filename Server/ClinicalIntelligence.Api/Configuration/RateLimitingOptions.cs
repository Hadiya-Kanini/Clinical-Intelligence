namespace ClinicalIntelligence.Api.Configuration;

/// <summary>
/// Configuration options for login rate limiting.
/// Defaults aligned with US_015: 5 attempts per 60 seconds per IP.
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
}
