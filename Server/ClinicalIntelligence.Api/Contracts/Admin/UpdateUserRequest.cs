using System.Text.Json.Serialization;

namespace ClinicalIntelligence.Api.Contracts.Admin;

/// <summary>
/// Request contract for admin user update endpoint.
/// Used by PUT /api/v1/admin/users/{userId}.
/// </summary>
public sealed class UpdateUserRequest
{
    /// <summary>
    /// User display name. Required, max length 100.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// User email address. Required, RFC5322 validation applied.
    /// </summary>
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    /// <summary>
    /// User role. Required, allowed values: admin, standard.
    /// </summary>
    [JsonPropertyName("role")]
    public string? Role { get; set; }
}
