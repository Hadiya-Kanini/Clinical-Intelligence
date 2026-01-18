using ClinicalIntelligence.Api.Contracts.Entities;

namespace ClinicalIntelligence.Api.Contracts.Patients;

/// <summary>
/// Response DTO for Patient 360 API including grounded entities with citations.
/// Only entities with valid source citations are included (FR-051, FR-056).
/// </summary>
public sealed record Patient360Response
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
    /// Grounded extracted entities with source citations.
    /// Only entities with at least one valid citation are included.
    /// </summary>
    public IReadOnlyList<ExtractedEntityWithCitationsDto> Entities { get; init; } = Array.Empty<ExtractedEntityWithCitationsDto>();

    /// <summary>
    /// Total count of grounded entities.
    /// </summary>
    public int EntityCount => Entities.Count;

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
/// Document summary for Patient 360 response.
/// </summary>
public sealed record Patient360DocumentDto
{
    /// <summary>
    /// Document identifier.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Original filename.
    /// </summary>
    public string OriginalName { get; init; } = string.Empty;

    /// <summary>
    /// Processing status.
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// Upload timestamp.
    /// </summary>
    public DateTime UploadedAt { get; init; }

    /// <summary>
    /// Number of grounded entities extracted from this document.
    /// </summary>
    public int GroundedEntityCount { get; init; }
}
