using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicalIntelligence.Api.Domain.Models;

/// <summary>
/// Encounter entity aligned with FHIR Encounter resource.
/// Represents a visit or episode of care context.
/// </summary>
public sealed class Encounter
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Reference to the patient. Maps to FHIR Encounter.subject.
    /// </summary>
    [Required]
    public Guid PatientId { get; set; }

    [ForeignKey(nameof(PatientId))]
    public Patient Patient { get; set; } = null!;

    /// <summary>
    /// Encounter status. Maps to FHIR Encounter.status.
    /// Values: planned, arrived, triaged, in-progress, onleave, finished, cancelled, entered-in-error, unknown.
    /// </summary>
    [Required]
    [MaxLength(30)]
    public string Status { get; set; } = "unknown";

    /// <summary>
    /// Classification of the encounter. Maps to FHIR Encounter.class.
    /// Values: AMB (ambulatory), EMER (emergency), IMP (inpatient), etc.
    /// </summary>
    [MaxLength(30)]
    public string? Class { get; set; }

    /// <summary>
    /// Specific type of encounter. Maps to FHIR Encounter.type.
    /// </summary>
    [MaxLength(100)]
    public string? Type { get; set; }

    /// <summary>
    /// Start of the encounter period. Maps to FHIR Encounter.period.start.
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// End of the encounter period. Maps to FHIR Encounter.period.end.
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Reason for the encounter. Maps to FHIR Encounter.reasonCode.
    /// </summary>
    [MaxLength(500)]
    public string? ReasonCode { get; set; }

    /// <summary>
    /// Location where encounter took place. Maps to FHIR Encounter.location.
    /// </summary>
    [MaxLength(200)]
    public string? Location { get; set; }

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

    // Navigation properties for encounter-context relationships
    public ICollection<Observation> Observations { get; set; } = new List<Observation>();
    public ICollection<Procedure> Procedures { get; set; } = new List<Procedure>();
    public ICollection<Condition> Conditions { get; set; } = new List<Condition>();
}
