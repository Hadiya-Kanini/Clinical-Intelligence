using System.Text.Json.Serialization;

namespace ClinicalIntelligence.Api.Contracts.Auth;

/// <summary>
/// Response contract for reset password token validation endpoint.
/// </summary>
public sealed record ValidateResetPasswordTokenResponse
{
    /// <summary>
    /// Indicates whether the token is valid.
    /// </summary>
    [JsonPropertyName("valid")]
    public bool Valid { get; init; }

    /// <summary>
    /// Token expiration time in UTC (only present when valid).
    /// </summary>
    [JsonPropertyName("expiresAt")]
    public DateTime? ExpiresAt { get; init; }
}
