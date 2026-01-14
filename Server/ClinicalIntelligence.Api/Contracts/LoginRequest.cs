using System.Text.Json.Serialization;

namespace ClinicalIntelligence.Api.Contracts;

public record LoginRequest(
    [property: JsonPropertyName("email")] string? Email,
    [property: JsonPropertyName("password")] string? Password
);
