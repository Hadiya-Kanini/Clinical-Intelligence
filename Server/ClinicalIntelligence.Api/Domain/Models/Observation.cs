using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicalIntelligence.Api.Domain.Models;

/// <summary>
/// Observation entity aligned with FHIR Observation resource.
/// Represents labs, vitals, and clinical measurements.
/// </summary>
public sealed class Observation
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Reference to the patient. Maps to FHIR Observation.subject.
    /// </summary>
    [Required]
    public Guid PatientId { get; set; }

    [ForeignKey(nameof(PatientId))]
    public Patient Patient { get; set; } = null!;

    /// <summary>
    /// Optional reference to encounter context. Maps to FHIR Observation.encounter.
    /// </summary>
    public Guid? EncounterId { get; set; }

    [ForeignKey(nameof(EncounterId))]
    public Encounter? Encounter { get; set; }

    /// <summary>
    /// Observation status. Maps to FHIR Observation.status.
    /// Values: registered, preliminary, final, amended, corrected, cancelled, entered-in-error, unknown.
    /// </summary>
    [Required]
    [MaxLength(30)]
    public string Status { get; set; } = "unknown";

    /// <summary>
    /// Category of observation. Maps to FHIR Observation.category.
    /// Values: vital-signs, laboratory, imaging, procedure, survey, exam, therapy, activity.
    /// </summary>
    [MaxLength(50)]
    public string? Category { get; set; }

    /// <summary>
    /// Code identifying the observation type. Maps to FHIR Observation.code.
    /// Preferably LOINC coded.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Display name for the observation code.
    /// </summary>
    [MaxLength(200)]
    public string? CodeDisplay { get; set; }

    /// <summary>
    /// Observation value. Maps to FHIR Observation.value[x].
    /// </summary>
    [MaxLength(500)]
    public string? Value { get; set; }

    /// <summary>
    /// Unit of measurement. Maps to FHIR Observation.valueQuantity.unit.
    /// Preferably UCUM coded.
    /// </summary>
    [MaxLength(50)]
    public string? Unit { get; set; }

    /// <summary>
    /// When the observation was made. Maps to FHIR Observation.effective[x].
    /// </summary>
    public DateTime? EffectiveDate { get; set; }

    /// <summary>
    /// Interpretation of the observation. Maps to FHIR Observation.interpretation.
    /// Values: H (high), L (low), N (normal), A (abnormal), etc.
    /// </summary>
    [MaxLength(30)]
    public string? Interpretation { get; set; }

    /// <summary>
    /// Reference range low bound. Maps to FHIR Observation.referenceRange.low.
    /// </summary>
    [MaxLength(50)]
    public string? ReferenceRangeLow { get; set; }

    /// <summary>
    /// Reference range high bound. Maps to FHIR Observation.referenceRange.high.
    /// </summary>
    [MaxLength(50)]
    public string? ReferenceRangeHigh { get; set; }

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
