using System.Text.Json.Serialization;

namespace ClinicalIntelligence.Api.Contracts;

/// <summary>
/// Payload for storing extracted entities from the worker.
/// </summary>
public sealed class EntityStorePayload
{
    [JsonPropertyName("patientId")]
    public string PatientId { get; set; } = string.Empty;

    [JsonPropertyName("documentId")]
    public string DocumentId { get; set; } = string.Empty;

    [JsonPropertyName("entities")]
    public List<EntityDto> Entities { get; set; } = new();
}

/// <summary>
/// Individual entity DTO from worker.
/// </summary>
public sealed class EntityDto
{
    [JsonPropertyName("entityGroupName")]
    public string EntityGroupName { get; set; } = string.Empty;

    [JsonPropertyName("entityName")]
    public string EntityName { get; set; } = string.Empty;

    [JsonPropertyName("entityValue")]
    public string EntityValue { get; set; } = string.Empty;

    [JsonPropertyName("rationale")]
    public string? Rationale { get; set; }

    [JsonPropertyName("sourceText")]
    public string? SourceText { get; set; }

    [JsonPropertyName("confidence")]
    public double? Confidence { get; set; }

    [JsonPropertyName("documentLocation")]
    public DocumentLocationDto? DocumentLocation { get; set; }

    [JsonPropertyName("mappedCategory")]
    public string? MappedCategory { get; set; }
}

/// <summary>
/// Document location information.
/// </summary>
public sealed class DocumentLocationDto
{
    [JsonPropertyName("page")]
    public int? Page { get; set; }

    [JsonPropertyName("section")]
    public string? Section { get; set; }

    [JsonPropertyName("coordinates")]
    public CoordinatesDto? Coordinates { get; set; }
}

/// <summary>
/// Coordinates information.
/// </summary>
public sealed class CoordinatesDto
{
    [JsonPropertyName("x")]
    public double X { get; set; }

    [JsonPropertyName("y")]
    public double Y { get; set; }

    [JsonPropertyName("width")]
    public double Width { get; set; }

    [JsonPropertyName("height")]
    public double Height { get; set; }
}
