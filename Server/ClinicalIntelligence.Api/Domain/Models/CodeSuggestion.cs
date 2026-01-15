using System.ComponentModel.DataAnnotations;

namespace ClinicalIntelligence.Api.Domain.Models;

/// <summary>
/// Code suggestion entity for AI-suggested billing codes.
/// Maps to the code_suggestions table per ERD specification.
/// </summary>
public sealed class CodeSuggestion
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Reference to the patient.
    /// </summary>
    public Guid PatientId { get; set; }

    /// <summary>
    /// Reference to the extracted entity that triggered the suggestion.
    /// </summary>
    public Guid? ExtractedEntityId { get; set; }

    /// <summary>
    /// The suggested billing code.
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Type of code: ICD-10, CPT, HCPCS, etc.
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string CodeType { get; set; } = string.Empty;

    /// <summary>
    /// Source text that led to the suggestion.
    /// </summary>
    public string? SourceText { get; set; }

    /// <summary>
    /// Suggestion status: Pending, Accepted, Rejected.
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// Reference to the user who made the decision.
    /// </summary>
    public Guid? DecidedByUserId { get; set; }

    /// <summary>
    /// Timestamp when suggestion was created.
    /// </summary>
    public DateTime SuggestedAt { get; set; }

    /// <summary>
    /// Timestamp when decision was made.
    /// </summary>
    public DateTime? DecidedAt { get; set; }

    // Navigation properties
    public ErdPatient Patient { get; set; } = null!;
    public ExtractedEntity? ExtractedEntity { get; set; }
    public BillingCodeCatalogItem? BillingCodeCatalogItem { get; set; }
    public User? DecidedByUser { get; set; }
}
