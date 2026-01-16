using System.Text.Json.Serialization;

namespace ClinicalIntelligence.Api.Contracts;

/// <summary>
/// Request contract for batch document upload.
/// Patient ID is provided as form data alongside files.
/// </summary>
public record BatchUploadRequest
{
    [JsonPropertyName("patientId")]
    public Guid PatientId { get; init; }
}
