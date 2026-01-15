namespace ClinicalIntelligence.Api.Services.Auth;

/// <summary>
/// Abstraction for building password reset URLs.
/// Keeps URL construction logic out of endpoint handlers.
/// </summary>
public interface IPasswordResetLinkBuilder
{
    /// <summary>
    /// Builds a password reset URL with the given token.
    /// </summary>
    /// <param name="token">The plain text reset token.</param>
    /// <returns>Full URL pointing to the frontend reset page with token query parameter.</returns>
    string BuildResetUrl(string token);
}
