using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicalIntelligence.Api.Domain.Models;

/// <summary>
/// MedicationStatement entity aligned with FHIR MedicationStatement resource.
/// Represents medication history and current medications.
/// </summary>
public sealed class MedicationStatement
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Reference to the patient. Maps to FHIR MedicationStatement.subject.
    /// </summary>
    [Required]
    public Guid PatientId { get; set; }

    [ForeignKey(nameof(PatientId))]
    public Patient Patient { get; set; } = null!;

    /// <summary>
    /// Medication statement status. Maps to FHIR MedicationStatement.status.
    /// Values: active, completed, entered-in-error, intended, stopped, on-hold, unknown, not-taken.
    /// </summary>
    [Required]
    [MaxLength(30)]
    public string Status { get; set; } = "unknown";

    /// <summary>
    /// Medication code. Maps to FHIR MedicationStatement.medication[x].
    /// Preferably RxNorm coded.
    /// </summary>
    [MaxLength(100)]
    public string? MedicationCode { get; set; }

    /// <summary>
    /// Medication display name. Maps to FHIR MedicationStatement.medicationCodeableConcept.text.
    /// </summary>
    [Required]
    [MaxLength(300)]
    public string MedicationName { get; set; } = string.Empty;

    /// <summary>
    /// Dosage instructions. Maps to FHIR MedicationStatement.dosage.
    /// </summary>
    [MaxLength(500)]
    public string? Dosage { get; set; }

    /// <summary>
    /// Route of administration.
    /// </summary>
    [MaxLength(100)]
    public string? Route { get; set; }

    /// <summary>
    /// Frequency of administration.
    /// </summary>
    [MaxLength(100)]
    public string? Frequency { get; set; }

    /// <summary>
    /// When the medication was effective. Maps to FHIR MedicationStatement.effective[x].
    /// </summary>
    public DateTime? EffectiveDate { get; set; }

    /// <summary>
    /// End date if medication was stopped.
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Reason for taking medication. Maps to FHIR MedicationStatement.reasonCode.
    /// </summary>
    [MaxLength(500)]
    public string? ReasonCode { get; set; }

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
