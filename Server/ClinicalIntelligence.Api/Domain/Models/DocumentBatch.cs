using System.ComponentModel.DataAnnotations;

namespace ClinicalIntelligence.Api.Domain.Models;

/// <summary>
/// Document batch entity for grouping uploaded documents.
/// Maps to the document_batches table per ERD specification.
/// </summary>
public sealed class DocumentBatch
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Reference to the patient this batch belongs to.
    /// </summary>
    public Guid PatientId { get; set; }

    /// <summary>
    /// Reference to the user who uploaded the batch.
    /// </summary>
    public Guid UploadedByUserId { get; set; }

    /// <summary>
    /// Timestamp when the batch was uploaded.
    /// </summary>
    public DateTime UploadedAt { get; set; }

    // Navigation properties
    public ErdPatient Patient { get; set; } = null!;
    public User UploadedByUser { get; set; } = null!;
    public ICollection<Document> Documents { get; set; } = new List<Document>();
}
