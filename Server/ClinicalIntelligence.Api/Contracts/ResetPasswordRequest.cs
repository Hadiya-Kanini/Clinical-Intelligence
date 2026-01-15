using System.Text.Json.Serialization;

namespace ClinicalIntelligence.Api.Contracts;

/// <summary>
/// Request contract for reset password endpoint.
/// </summary>
public sealed record ResetPasswordRequest
{
    /// <summary>
    /// Password reset token from email link.
    /// </summary>
    [JsonPropertyName("token")]
    public string? Token { get; init; }

    /// <summary>
    /// New password to set.
    /// </summary>
    [JsonPropertyName("newPassword")]
    public string? NewPassword { get; init; }
}
