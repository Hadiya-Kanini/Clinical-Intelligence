using System.Text.Json.Serialization;

namespace ClinicalIntelligence.Api.Contracts;

/// <summary>
/// Response contract for document upload acknowledgment.
/// Returns immediately after validation, before async processing.
/// </summary>
public record UploadAcknowledgmentResponse
{
    [JsonPropertyName("documentId")]
    public Guid DocumentId { get; init; }

    [JsonPropertyName("fileName")]
    public string FileName { get; init; } = string.Empty;

    [JsonPropertyName("fileSize")]
    public long FileSize { get; init; }

    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;

    [JsonPropertyName("isValid")]
    public bool IsValid { get; init; }

    [JsonPropertyName("validationErrors")]
    public List<string> ValidationErrors { get; init; } = new();

    [JsonPropertyName("acknowledgedAt")]
    public DateTime AcknowledgedAt { get; init; }

    /// <summary>
    /// Programmatic error code for validation failures.
    /// Null when validation succeeds.
    /// </summary>
    [JsonPropertyName("errorCode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public FileValidationErrorCode? ErrorCode { get; init; }

    /// <summary>
    /// Error type string for client categorization (e.g., "invalid_extension", "mime_mismatch").
    /// Null when validation succeeds.
    /// </summary>
    [JsonPropertyName("errorType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ErrorType { get; init; }
}

/// <summary>
/// Request contract for document upload.
/// </summary>
public record UploadDocumentRequest
{
    [JsonPropertyName("patientId")]
    public Guid PatientId { get; init; }
}
