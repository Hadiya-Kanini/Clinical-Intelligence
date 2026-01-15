using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicalIntelligence.Api.Domain.Models;

/// <summary>
/// Procedure entity aligned with FHIR Procedure resource.
/// Represents clinical procedures performed on a patient.
/// </summary>
public sealed class Procedure
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Reference to the patient. Maps to FHIR Procedure.subject.
    /// </summary>
    [Required]
    public Guid PatientId { get; set; }

    [ForeignKey(nameof(PatientId))]
    public Patient Patient { get; set; } = null!;

    /// <summary>
    /// Optional reference to encounter context. Maps to FHIR Procedure.encounter.
    /// </summary>
    public Guid? EncounterId { get; set; }

    [ForeignKey(nameof(EncounterId))]
    public Encounter? Encounter { get; set; }

    /// <summary>
    /// Procedure status. Maps to FHIR Procedure.status.
    /// Values: preparation, in-progress, not-done, on-hold, stopped, completed, entered-in-error, unknown.
    /// </summary>
    [Required]
    [MaxLength(30)]
    public string Status { get; set; } = "unknown";

    /// <summary>
    /// Procedure code. Maps to FHIR Procedure.code.
    /// Preferably CPT or SNOMED CT coded.
    /// </summary>
    [MaxLength(100)]
    public string? Code { get; set; }

    /// <summary>
    /// Display name for the procedure code.
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string CodeDisplay { get; set; } = string.Empty;

    /// <summary>
    /// Category of procedure. Maps to FHIR Procedure.category.
    /// </summary>
    [MaxLength(100)]
    public string? Category { get; set; }

    /// <summary>
    /// When the procedure was performed. Maps to FHIR Procedure.performed[x].
    /// </summary>
    public DateTime? PerformedDate { get; set; }

    /// <summary>
    /// End date if procedure spans a period.
    /// </summary>
    public DateTime? PerformedEndDate { get; set; }

    /// <summary>
    /// Reason for the procedure. Maps to FHIR Procedure.reasonCode.
    /// </summary>
    [MaxLength(500)]
    public string? ReasonCode { get; set; }

    /// <summary>
    /// Body site where procedure was performed. Maps to FHIR Procedure.bodySite.
    /// </summary>
    [MaxLength(200)]
    public string? BodySite { get; set; }

    /// <summary>
    /// Outcome of the procedure. Maps to FHIR Procedure.outcome.
    /// </summary>
    [MaxLength(200)]
    public string? Outcome { get; set; }

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
