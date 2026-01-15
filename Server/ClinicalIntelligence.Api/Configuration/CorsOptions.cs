namespace ClinicalIntelligence.Api.Configuration;

/// <summary>
/// Configuration options for CORS policy (US_023).
/// Supports configuration-driven allowed origins with startup validation.
/// </summary>
public sealed class CorsOptions
{
    /// <summary>
    /// Configuration section name for binding.
    /// </summary>
    public const string SectionName = "Cors";

    /// <summary>
    /// Policy name for frontend CORS policy.
    /// </summary>
    public const string FrontendPolicyName = "AllowFrontend";

    /// <summary>
    /// Environment variable name for allowed origins (semicolon-delimited).
    /// </summary>
    public const string AllowedOriginsEnvVar = "CORS_ALLOWED_ORIGINS";

    /// <summary>
    /// Allowed origins for CORS requests (semicolon-delimited string).
    /// Example: "https://app.example.com;https://staging.example.com"
    /// </summary>
    public string? AllowedOrigins { get; set; }

    /// <summary>
    /// Gets the parsed list of allowed origins.
    /// Returns empty array if AllowedOrigins is null or empty.
    /// </summary>
    public string[] GetParsedOrigins()
    {
        if (string.IsNullOrWhiteSpace(AllowedOrigins))
        {
            return Array.Empty<string>();
        }

        return AllowedOrigins
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(o => !string.IsNullOrWhiteSpace(o))
            .ToArray();
    }

    /// <summary>
    /// Validates the CORS configuration.
    /// Throws InvalidOperationException if configuration is invalid for non-development environments.
    /// </summary>
    /// <param name="isDevelopment">Whether the application is running in development mode.</param>
    /// <exception cref="InvalidOperationException">Thrown when no allowed origins are configured in non-development environments.</exception>
    public void Validate(bool isDevelopment)
    {
        var origins = GetParsedOrigins();

        if (origins.Length == 0 && !isDevelopment)
        {
            throw new InvalidOperationException(
                $"CORS configuration error: No allowed origins configured. " +
                $"Set the '{AllowedOriginsEnvVar}' environment variable or configure '{SectionName}:{nameof(AllowedOrigins)}' in appsettings.json. " +
                $"Example: CORS_ALLOWED_ORIGINS=https://app.example.com;https://staging.example.com");
        }

        // Validate each origin is a valid URI
        foreach (var origin in origins)
        {
            if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
            {
                throw new InvalidOperationException(
                    $"CORS configuration error: Invalid origin '{origin}'. Origins must be valid absolute URIs.");
            }

            // Ensure origin doesn't have a path (CORS origins should be scheme://host:port only)
            if (!string.IsNullOrEmpty(uri.AbsolutePath) && uri.AbsolutePath != "/")
            {
                throw new InvalidOperationException(
                    $"CORS configuration error: Origin '{origin}' should not contain a path. Use scheme://host:port format.");
            }
        }
    }

    /// <summary>
    /// Gets the default development origins for local development.
    /// Only used when no origins are configured in development mode.
    /// </summary>
    public static string[] GetDefaultDevelopmentOrigins()
    {
        return new[]
        {
            "http://localhost:5173",
            "http://localhost:3000",
            "https://localhost:5173",
            "https://localhost:3000"
        };
    }
}
