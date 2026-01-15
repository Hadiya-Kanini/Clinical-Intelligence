namespace ClinicalIntelligence.Api.Configuration;

/// <summary>
/// Configuration options for frontend URLs used in email links.
/// </summary>
public sealed class FrontendUrlsOptions
{
    /// <summary>
    /// Configuration section name for binding.
    /// </summary>
    public const string SectionName = "FrontendUrls";

    /// <summary>
    /// Base URL for the frontend application.
    /// Environment variable: FRONTEND_URL
    /// </summary>
    public string BaseUrl { get; set; } = "http://localhost:5173";

    /// <summary>
    /// Path for the password reset page (relative to BaseUrl).
    /// </summary>
    public string PasswordResetPath { get; set; } = "/reset-password";

    /// <summary>
    /// Creates FrontendUrlsOptions from configuration/environment variables.
    /// </summary>
    public static FrontendUrlsOptions FromConfiguration(IConfiguration configuration)
    {
        return new FrontendUrlsOptions
        {
            BaseUrl = configuration["FRONTEND_URL"] ?? "http://localhost:5173",
            PasswordResetPath = configuration["FRONTEND_PASSWORD_RESET_PATH"] ?? "/reset-password"
        };
    }
}
