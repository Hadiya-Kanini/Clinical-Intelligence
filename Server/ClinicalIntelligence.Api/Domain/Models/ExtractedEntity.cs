using System.ComponentModel.DataAnnotations;

namespace ClinicalIntelligence.Api.Domain.Models;

/// <summary>
/// Extracted entity from document processing.
/// Maps to the extracted_entities table per ERD specification.
/// </summary>
public sealed class ExtractedEntity
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Reference to the patient.
    /// </summary>
    public Guid PatientId { get; set; }

    /// <summary>
    /// Reference to the source document.
    /// </summary>
    public Guid DocumentId { get; set; }

    /// <summary>
    /// Entity category: Diagnosis, Medication, Procedure, Allergy, etc.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Entity name/label.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Entity value.
    /// </summary>
    [MaxLength(500)]
    public string? Value { get; set; }

    /// <summary>
    /// Units for numeric values.
    /// </summary>
    [MaxLength(50)]
    public string? Units { get; set; }

    /// <summary>
    /// Confidence score from extraction (0.0 to 1.0).
    /// </summary>
    public float? ConfidenceScore { get; set; }

    /// <summary>
    /// Whether the entity has been verified by a user.
    /// </summary>
    public bool IsVerified { get; set; }

    /// <summary>
    /// Reference to the user who verified the entity.
    /// </summary>
    public Guid? VerifiedByUserId { get; set; }

    /// <summary>
    /// Timestamp when verified.
    /// </summary>
    public DateTime? VerifiedAt { get; set; }

    /// <summary>
    /// Effective date of the entity.
    /// </summary>
    public DateTime? EffectiveAt { get; set; }

    // Navigation properties
    public ErdPatient Patient { get; set; } = null!;
    public Document Document { get; set; } = null!;
    public User? VerifiedByUser { get; set; }
    // TEMPORARY: Commented out for vector DB installation
    // public ICollection<EntityCitation> EntityCitations { get; set; } = new List<EntityCitation>();
    public ICollection<CodeSuggestion> CodeSuggestions { get; set; } = new List<CodeSuggestion>();
}
