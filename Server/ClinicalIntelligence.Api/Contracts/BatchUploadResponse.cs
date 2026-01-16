using System.Text.Json.Serialization;

namespace ClinicalIntelligence.Api.Contracts;

/// <summary>
/// Response contract for batch document upload.
/// Contains batch-level summary and per-file results.
/// </summary>
public record BatchUploadResponse
{
    [JsonPropertyName("batchId")]
    public Guid BatchId { get; init; }

    [JsonPropertyName("patientId")]
    public Guid PatientId { get; init; }

    [JsonPropertyName("totalFilesReceived")]
    public int TotalFilesReceived { get; init; }

    [JsonPropertyName("filesAccepted")]
    public int FilesAccepted { get; init; }

    [JsonPropertyName("filesRejected")]
    public int FilesRejected { get; init; }

    [JsonPropertyName("batchLimitExceeded")]
    public bool BatchLimitExceeded { get; init; }

    [JsonPropertyName("batchLimitWarning")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? BatchLimitWarning { get; init; }

    [JsonPropertyName("fileResults")]
    public List<FileUploadResult> FileResults { get; init; } = new();

    [JsonPropertyName("acknowledgedAt")]
    public DateTime AcknowledgedAt { get; init; }
}

/// <summary>
/// Result for an individual file in a batch upload.
/// </summary>
public record FileUploadResult
{
    [JsonPropertyName("fileName")]
    public string FileName { get; init; } = string.Empty;

    [JsonPropertyName("documentId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Guid? DocumentId { get; init; }

    [JsonPropertyName("isAccepted")]
    public bool IsAccepted { get; init; }

    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;

    [JsonPropertyName("validationErrors")]
    public List<string> ValidationErrors { get; init; } = new();

    [JsonPropertyName("rejectionReason")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? RejectionReason { get; init; }
}
