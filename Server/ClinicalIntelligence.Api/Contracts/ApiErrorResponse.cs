using System.Text.Json.Serialization;

namespace ClinicalIntelligence.Api.Contracts;

public record ApiErrorResponse([property: JsonPropertyName("error")] ErrorResponse Error);

public record ErrorResponse(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("details")] string[] Details);
