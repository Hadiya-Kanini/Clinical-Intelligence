using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicalIntelligence.Api.Domain.Models;

/// <summary>
/// DocumentReference entity aligned with FHIR DocumentReference resource.
/// Links ingested documents to patients and encounters.
/// </summary>
public sealed class DocumentReference
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Reference to the patient. Maps to FHIR DocumentReference.subject.
    /// </summary>
    [Required]
    public Guid PatientId { get; set; }

    [ForeignKey(nameof(PatientId))]
    public Patient Patient { get; set; } = null!;

    /// <summary>
    /// Document status. Maps to FHIR DocumentReference.status.
    /// Values: current, superseded, entered-in-error.
    /// </summary>
    [Required]
    [MaxLength(30)]
    public string Status { get; set; } = "current";

    /// <summary>
    /// Document type code. Maps to FHIR DocumentReference.type.
    /// </summary>
    [MaxLength(100)]
    public string? Type { get; set; }

    /// <summary>
    /// Category of document. Maps to FHIR DocumentReference.category.
    /// </summary>
    [MaxLength(100)]
    public string? Category { get; set; }

    /// <summary>
    /// When the document was created. Maps to FHIR DocumentReference.date.
    /// </summary>
    public DateTime? Date { get; set; }

    /// <summary>
    /// Human-readable description. Maps to FHIR DocumentReference.description.
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Original file name.
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string OriginalName { get; set; } = string.Empty;

    /// <summary>
    /// MIME type of the document. Maps to FHIR DocumentReference.content.attachment.contentType.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// File size in bytes.
    /// </summary>
    public long SizeBytes { get; set; }

    /// <summary>
    /// Storage path or URL. Maps to FHIR DocumentReference.content.attachment.url.
    /// </summary>
    [Required]
    [MaxLength(1000)]
    public string StoragePath { get; set; } = string.Empty;

    /// <summary>
    /// Processing status for the document.
    /// Values: pending, processing, completed, failed.
    /// </summary>
    [Required]
    [MaxLength(30)]
    public string ProcessingStatus { get; set; } = "pending";

    /// <summary>
    /// Soft delete flag.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Timestamp when soft deleted.
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Record creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last update timestamp.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// JSON storage for FHIR extensions and unmapped data.
    /// Follows the extension strategy defined in fhir_alignment.md.
    /// </summary>
    public string? Extensions { get; set; }
}
