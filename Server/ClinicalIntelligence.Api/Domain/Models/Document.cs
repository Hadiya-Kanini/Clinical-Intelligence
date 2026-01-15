using System.ComponentModel.DataAnnotations;

namespace ClinicalIntelligence.Api.Domain.Models;

/// <summary>
/// Document entity for uploaded clinical documents.
/// Maps to the documents table per ERD specification.
/// </summary>
public sealed class Document
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Reference to the patient this document belongs to.
    /// </summary>
    public Guid PatientId { get; set; }

    /// <summary>
    /// Reference to the batch this document was uploaded in.
    /// </summary>
    public Guid? DocumentBatchId { get; set; }

    /// <summary>
    /// Reference to the user who uploaded the document.
    /// </summary>
    public Guid UploadedByUserId { get; set; }

    /// <summary>
    /// Original filename.
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string OriginalName { get; set; } = string.Empty;

    /// <summary>
    /// MIME type of the document.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string MimeType { get; set; } = string.Empty;

    /// <summary>
    /// File size in bytes.
    /// </summary>
    public int SizeBytes { get; set; }

    /// <summary>
    /// Path to the stored file.
    /// </summary>
    [Required]
    [MaxLength(1000)]
    public string StoragePath { get; set; } = string.Empty;

    /// <summary>
    /// Processing status: Pending, Processing, Completed, Failed.
    /// </summary>
    [Required]
    [MaxLength(30)]
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// Soft delete flag.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Timestamp when soft deleted.
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Timestamp when uploaded.
    /// </summary>
    public DateTime UploadedAt { get; set; }

    // Navigation properties
    public ErdPatient Patient { get; set; } = null!;
    public DocumentBatch? DocumentBatch { get; set; }
    public User UploadedByUser { get; set; } = null!;
    public ICollection<ProcessingJob> ProcessingJobs { get; set; } = new List<ProcessingJob>();
    // TEMPORARY: Commented out for vector DB installation
    // public ICollection<DocumentChunk> DocumentChunks { get; set; } = new List<DocumentChunk>();
    public ICollection<ExtractedEntity> ExtractedEntities { get; set; } = new List<ExtractedEntity>();
}
