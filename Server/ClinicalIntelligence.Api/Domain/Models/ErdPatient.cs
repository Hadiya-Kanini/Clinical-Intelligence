using System.ComponentModel.DataAnnotations;

namespace ClinicalIntelligence.Api.Domain.Models;

/// <summary>
/// Patient entity per ERD specification for the Trust-First Clinical Intelligence Platform.
/// Maps to the erd_patients table to avoid conflict with existing FHIR-aligned patients table.
/// </summary>
public sealed class ErdPatient
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Medical Record Number - unique identifier.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Mrn { get; set; } = string.Empty;

    /// <summary>
    /// Patient's full name.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Date of birth.
    /// </summary>
    public DateOnly? Dob { get; set; }

    /// <summary>
    /// Patient address.
    /// </summary>
    [MaxLength(500)]
    public string? Address { get; set; }

    /// <summary>
    /// Contact information.
    /// </summary>
    [MaxLength(100)]
    public string? Contact { get; set; }

    /// <summary>
    /// Soft delete flag.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Timestamp when soft deleted.
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<DocumentBatch> DocumentBatches { get; set; } = new List<DocumentBatch>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();
    public ICollection<ExtractedEntity> ExtractedEntities { get; set; } = new List<ExtractedEntity>();
    public ICollection<ErdConflict> Conflicts { get; set; } = new List<ErdConflict>();
    public ICollection<CodeSuggestion> CodeSuggestions { get; set; } = new List<CodeSuggestion>();
    // TEMPORARY: Commented out for vector DB installation
    // public ICollection<VectorQueryLog> VectorQueryLogs { get; set; } = new List<VectorQueryLog>();
}
