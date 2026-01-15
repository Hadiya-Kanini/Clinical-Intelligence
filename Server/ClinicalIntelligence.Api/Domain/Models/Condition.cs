using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicalIntelligence.Api.Domain.Models;

/// <summary>
/// Condition entity aligned with FHIR Condition resource.
/// Represents diagnoses, problems, and health concerns.
/// </summary>
public sealed class Condition
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Reference to the patient. Maps to FHIR Condition.subject.
    /// </summary>
    [Required]
    public Guid PatientId { get; set; }

    [ForeignKey(nameof(PatientId))]
    public Patient Patient { get; set; } = null!;

    /// <summary>
    /// Optional reference to encounter context. Maps to FHIR Condition.encounter.
    /// </summary>
    public Guid? EncounterId { get; set; }

    [ForeignKey(nameof(EncounterId))]
    public Encounter? Encounter { get; set; }

    /// <summary>
    /// Clinical status. Maps to FHIR Condition.clinicalStatus.
    /// Values: active, recurrence, relapse, inactive, remission, resolved.
    /// </summary>
    [MaxLength(30)]
    public string? ClinicalStatus { get; set; }

    /// <summary>
    /// Verification status. Maps to FHIR Condition.verificationStatus.
    /// Values: unconfirmed, provisional, differential, confirmed, refuted, entered-in-error.
    /// </summary>
    [MaxLength(30)]
    public string? VerificationStatus { get; set; }

    /// <summary>
    /// Category of condition. Maps to FHIR Condition.category.
    /// Values: problem-list-item, encounter-diagnosis, health-concern.
    /// </summary>
    [MaxLength(50)]
    public string? Category { get; set; }

    /// <summary>
    /// Condition code. Maps to FHIR Condition.code.
    /// Preferably ICD-10 or SNOMED CT coded.
    /// </summary>
    [MaxLength(100)]
    public string? Code { get; set; }

    /// <summary>
    /// Display name for the condition code.
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string CodeDisplay { get; set; } = string.Empty;

    /// <summary>
    /// When the condition started. Maps to FHIR Condition.onset[x].
    /// </summary>
    public DateTime? OnsetDate { get; set; }

    /// <summary>
    /// When the condition resolved. Maps to FHIR Condition.abatement[x].
    /// </summary>
    public DateTime? AbatementDate { get; set; }

    /// <summary>
    /// Severity of the condition. Maps to FHIR Condition.severity.
    /// Values: mild, moderate, severe.
    /// </summary>
    [MaxLength(30)]
    public string? Severity { get; set; }

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
