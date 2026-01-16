using System.Text.Json.Serialization;

namespace ClinicalIntelligence.Api.Contracts.Admin;

/// <summary>
/// Response contract for admin user creation endpoint.
/// </summary>
public sealed class CreateUserResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("credentials_email_sent")]
    public bool CredentialsEmailSent { get; set; }

    [JsonPropertyName("credentials_email_error_code")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CredentialsEmailErrorCode { get; set; }
}
