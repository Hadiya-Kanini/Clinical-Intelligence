using ClinicalIntelligence.Api.Configuration;

namespace ClinicalIntelligence.Api.Services.Auth;

/// <summary>
/// Builds password reset URLs pointing to the frontend reset page.
/// </summary>
public sealed class PasswordResetLinkBuilder : IPasswordResetLinkBuilder
{
    private readonly FrontendUrlsOptions _options;

    public PasswordResetLinkBuilder(FrontendUrlsOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    public string BuildResetUrl(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException("Token cannot be null or empty.", nameof(token));
        }

        var baseUrl = _options.BaseUrl.TrimEnd('/');
        var path = _options.PasswordResetPath.TrimStart('/');
        var encodedToken = Uri.EscapeDataString(token);

        return $"{baseUrl}/{path}?token={encodedToken}";
    }
}
