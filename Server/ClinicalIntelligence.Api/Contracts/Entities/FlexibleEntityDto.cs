using System.Text.Json;
using ClinicalIntelligence.Api.Contracts.Patients;

namespace ClinicalIntelligence.Api.Contracts.Entities;

/// <summary>
/// DTO for flexible, schema-agnostic extracted entities.
/// Represents entities stored in extracted_entities_v2 table.
/// </summary>
public sealed record FlexibleEntityDto
{
    /// <summary>
    /// Unique identifier for the extracted entity.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Section name (e.g., "Medications", "Lab Results", "Vital Signs").
    /// </summary>
    public string SectionName { get; init; } = string.Empty;

    /// <summary>
    /// Entity data as a dynamic object.
    /// Contains all fields extracted for this entity.
    /// </summary>
    public Dictionary<string, object> EntityData { get; init; } = new();

    /// <summary>
    /// Source reference indicating where the entity was extracted from.
    /// </summary>
    public string? SourceReference { get; init; }

    /// <summary>
    /// Timestamp when the entity was extracted.
    /// </summary>
    public DateTime ExtractedAt { get; init; }

    /// <summary>
    /// Whether the entity has been verified by a user.
    /// </summary>
    public bool IsVerified { get; init; }

    /// <summary>
    /// Timestamp when verified.
    /// </summary>
    public DateTime? VerifiedAt { get; init; }
}

/// <summary>
/// Request DTO for storing flexible entities.
/// </summary>
public sealed record StoreFlexibleEntitiesRequest
{
    /// <summary>
    /// Patient ID for the entities.
    /// </summary>
    public Guid PatientId { get; init; }

    /// <summary>
    /// Document ID for the entities.
    /// </summary>
    public Guid DocumentId { get; init; }

    /// <summary>
    /// List of entity records to store.
    /// </summary>
    public List<FlexibleEntityRecord> Entities { get; init; } = new();
}

/// <summary>
/// Individual entity record for storage.
/// </summary>
public sealed record FlexibleEntityRecord
{
    /// <summary>
    /// Section name.
    /// </summary>
    public string SectionName { get; init; } = string.Empty;

    /// <summary>
    /// Entity data as dictionary.
    /// </summary>
    public Dictionary<string, object> EntityData { get; init; } = new();

    /// <summary>
    /// Source reference.
    /// </summary>
    public string? SourceReference { get; init; }
}

/// <summary>
/// Response DTO for Patient 360 with flexible entities grouped by section.
/// </summary>
public sealed record Patient360FlexibleResponse
{
    /// <summary>
    /// Patient identifier.
    /// </summary>
    public Guid PatientId { get; init; }

    /// <summary>
    /// Medical Record Number.
    /// </summary>
    public string? Mrn { get; init; }

    /// <summary>
    /// Patient name.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Date of birth.
    /// </summary>
    public DateOnly? Dob { get; init; }

    /// <summary>
    /// Patient address.
    /// </summary>
    public string? Address { get; init; }

    /// <summary>
    /// Contact information.
    /// </summary>
    public string? Contact { get; init; }

    /// <summary>
    /// Clinical data grouped by section.
    /// Key: Section name (e.g., "Medications", "Lab Results")
    /// Value: List of entities in that section
    /// </summary>
    public Dictionary<string, List<FlexibleEntityDto>> ClinicalData { get; init; } = new();

    /// <summary>
    /// Total count of entities across all sections.
    /// </summary>
    public int TotalEntityCount => ClinicalData.Values.Sum(list => list.Count);

    /// <summary>
    /// Number of sections with data.
    /// </summary>
    public int SectionCount => ClinicalData.Count;

    /// <summary>
    /// Documents associated with this patient.
    /// </summary>
    public IReadOnlyList<Patient360DocumentDto> Documents { get; init; } = Array.Empty<Patient360DocumentDto>();

    /// <summary>
    /// Timestamp when the response was generated.
    /// </summary>
    public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Summary statistics for flexible entity extraction.
/// </summary>
public sealed record FlexibleEntitySummary
{
    /// <summary>
    /// Total number of entities.
    /// </summary>
    public int TotalEntities { get; init; }

    /// <summary>
    /// Number of sections.
    /// </summary>
    public int SectionCount { get; init; }

    /// <summary>
    /// Breakdown by section.
    /// </summary>
    public Dictionary<string, int> SectionBreakdown { get; init; } = new();

    /// <summary>
    /// Number of entities with source references.
    /// </summary>
    public int EntitiesWithSources { get; init; }

    /// <summary>
    /// Number of verified entities.
    /// </summary>
    public int VerifiedEntities { get; init; }
}
