using System.Text.Json.Serialization;

namespace ClinicalIntelligence.Api.Contracts;

/// <summary>
/// Request contract for forgot password endpoint.
/// </summary>
public sealed record ForgotPasswordRequest
{
    /// <summary>
    /// Email address for password reset.
    /// </summary>
    [JsonPropertyName("email")]
    public string? Email { get; init; }
}
