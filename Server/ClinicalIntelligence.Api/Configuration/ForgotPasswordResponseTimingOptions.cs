using Microsoft.Extensions.Configuration;

namespace ClinicalIntelligence.Api.Configuration;

/// <summary>
/// Configuration options for forgot-password response timing normalization.
/// Used to prevent timing-based account enumeration attacks by ensuring
/// consistent response times regardless of whether the email exists.
/// </summary>
public class ForgotPasswordResponseTimingOptions
{
    /// <summary>
    /// Configuration section name for binding.
    /// </summary>
    public const string SectionName = "ForgotPasswordTiming";

    /// <summary>
    /// Environment variable name for minimum response delay.
    /// </summary>
    public const string MinDelayEnvVar = "FORGOT_PASSWORD_MIN_DELAY_MS";

    /// <summary>
    /// Environment variable name for jitter.
    /// </summary>
    public const string JitterEnvVar = "FORGOT_PASSWORD_JITTER_MS";

    /// <summary>
    /// Default minimum response delay in milliseconds (500ms).
    /// This ensures all syntactically valid requests take at least this long.
    /// </summary>
    public const int DefaultMinDelayMs = 500;

    /// <summary>
    /// Default jitter in milliseconds (0 = no jitter).
    /// When set, adds random delay between 0 and this value.
    /// </summary>
    public const int DefaultJitterMs = 0;

    /// <summary>
    /// Minimum response delay in milliseconds for syntactically valid requests.
    /// Requests will be padded to at least this duration to prevent timing attacks.
    /// </summary>
    public int MinDelayMs { get; set; } = DefaultMinDelayMs;

    /// <summary>
    /// Optional jitter in milliseconds to add randomness to response times.
    /// When greater than 0, adds a random delay between 0 and this value.
    /// </summary>
    public int JitterMs { get; set; } = DefaultJitterMs;

    /// <summary>
    /// Creates options from configuration, with environment variable overrides.
    /// </summary>
    public static ForgotPasswordResponseTimingOptions FromConfiguration(IConfiguration configuration)
    {
        var options = new ForgotPasswordResponseTimingOptions();

        // Try to bind from configuration section
        var section = configuration.GetSection(SectionName);
        if (section.Exists())
        {
            if (int.TryParse(section["MinDelayMs"], out var minDelay))
            {
                options.MinDelayMs = minDelay;
            }
            if (int.TryParse(section["JitterMs"], out var jitter))
            {
                options.JitterMs = jitter;
            }
        }

        // Environment variables override configuration
        var minDelayEnv = Environment.GetEnvironmentVariable(MinDelayEnvVar);
        if (!string.IsNullOrWhiteSpace(minDelayEnv) && int.TryParse(minDelayEnv, out var minDelayFromEnv))
        {
            options.MinDelayMs = minDelayFromEnv;
        }

        var jitterEnv = Environment.GetEnvironmentVariable(JitterEnvVar);
        if (!string.IsNullOrWhiteSpace(jitterEnv) && int.TryParse(jitterEnv, out var jitterFromEnv))
        {
            options.JitterMs = jitterFromEnv;
        }

        return options;
    }

    /// <summary>
    /// Validates the options and ensures values are within acceptable ranges.
    /// </summary>
    public void Validate()
    {
        if (MinDelayMs < 0)
        {
            throw new InvalidOperationException($"{nameof(MinDelayMs)} cannot be negative.");
        }

        if (JitterMs < 0)
        {
            throw new InvalidOperationException($"{nameof(JitterMs)} cannot be negative.");
        }

        // Warn if delay is too low for production (but don't fail - tests may use low values)
        if (MinDelayMs < 100)
        {
            Console.WriteLine($"Warning: {nameof(MinDelayMs)} is set to {MinDelayMs}ms which may not provide adequate timing protection.");
        }
    }
}
