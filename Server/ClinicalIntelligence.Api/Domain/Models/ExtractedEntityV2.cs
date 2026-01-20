using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicalIntelligence.Api.Domain.Models;

/// <summary>
/// Flexible extracted entity model for schema-agnostic clinical data extraction.
/// Stores entities with dynamic structure in JSONB format.
/// Maps to the extracted_entities_v2 table.
/// </summary>
[Table("extracted_entities_v2")]
public sealed class ExtractedEntityV2
{
    /// <summary>
    /// Unique identifier for the extracted entity.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Reference to the patient.
    /// </summary>
    [Required]
    public Guid PatientId { get; set; }

    /// <summary>
    /// Reference to the source document.
    /// </summary>
    [Required]
    public Guid DocumentId { get; set; }

    /// <summary>
    /// Section name (e.g., "Medications", "Lab Results", "Vital Signs").
    /// Preserves the original section name from extraction.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string SectionName { get; set; } = string.Empty;

    /// <summary>
    /// Entity data stored as JSONB.
    /// Contains the full entity object with all fields and values.
    /// </summary>
    [Required]
    [Column(TypeName = "jsonb")]
    public string EntityData { get; set; } = string.Empty;

    /// <summary>
    /// Source reference from the _source field in the entity.
    /// Indicates where the entity was extracted from.
    /// </summary>
    public string? SourceReference { get; set; }

    /// <summary>
    /// Timestamp when the entity was extracted.
    /// </summary>
    [Required]
    public DateTime ExtractedAt { get; set; }

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

    // Navigation properties
    public ErdPatient Patient { get; set; } = null!;
    public Document Document { get; set; } = null!;
    public User? VerifiedByUser { get; set; }
}
