namespace ClinicalIntelligence.Api.Contracts.Entities;

/// <summary>
/// DTO for entity citation details used in Patient 360 and entity verification.
/// Contains document metadata and location information for source verification.
/// </summary>
public sealed record EntityCitationDto
{
    /// <summary>
    /// Unique identifier for the citation.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Reference to the source document.
    /// </summary>
    public Guid DocumentId { get; init; }

    /// <summary>
    /// Original name of the source document.
    /// </summary>
    public string? DocumentName { get; init; }

    /// <summary>
    /// Page number in the source document.
    /// </summary>
    public int? Page { get; init; }

    /// <summary>
    /// Section identifier within the document.
    /// </summary>
    public string? Section { get; init; }

    /// <summary>
    /// Positional coordinates for highlighting (JSON format: {x, y, width, height}).
    /// </summary>
    public string? Coordinates { get; init; }

    /// <summary>
    /// The cited text from the source document.
    /// </summary>
    public string? CitedText { get; init; }
}

/// <summary>
/// Coordinate details for precise document location.
/// </summary>
public sealed record CitationCoordinatesDto
{
    public double X { get; init; }
    public double Y { get; init; }
    public double Width { get; init; }
    public double Height { get; init; }
}

/// <summary>
/// Extended entity DTO including citation information for Patient 360.
/// </summary>
public sealed record ExtractedEntityWithCitationsDto
{
    /// <summary>
    /// Unique identifier for the extracted entity.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Entity category (e.g., Diagnosis, Medication, Allergy).
    /// </summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>
    /// Display-friendly category name.
    /// </summary>
    public string? DisplayCategory { get; init; }

    /// <summary>
    /// Entity name/label.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Entity value.
    /// </summary>
    public string? Value { get; init; }

    /// <summary>
    /// Units for numeric values.
    /// </summary>
    public string? Units { get; init; }

    /// <summary>
    /// Confidence score from extraction (0.0 to 1.0).
    /// </summary>
    public float? ConfidenceScore { get; init; }

    /// <summary>
    /// Whether the entity has been verified by a user.
    /// </summary>
    public bool IsVerified { get; init; }

    /// <summary>
    /// Effective date of the entity.
    /// </summary>
    public DateTime? EffectiveAt { get; init; }

    /// <summary>
    /// Extraction rationale explaining why this value was extracted from the source.
    /// </summary>
    public string? Rationale { get; init; }

    /// <summary>
    /// Source citations for this entity.
    /// </summary>
    public IReadOnlyList<EntityCitationDto> Citations { get; init; } = Array.Empty<EntityCitationDto>();

    /// <summary>
    /// Indicates whether this entity has valid grounding (at least one citation).
    /// </summary>
    public bool IsGrounded => Citations.Count > 0;
}
