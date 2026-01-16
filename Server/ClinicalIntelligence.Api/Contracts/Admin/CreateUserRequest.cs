using System.Text.Json.Serialization;

namespace ClinicalIntelligence.Api.Contracts.Admin;

/// <summary>
/// Request contract for admin user creation endpoint.
/// </summary>
public sealed class CreateUserRequest
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("password")]
    public string? Password { get; set; }
}
