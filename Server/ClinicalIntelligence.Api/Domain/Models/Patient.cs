using System.ComponentModel.DataAnnotations;

namespace ClinicalIntelligence.Api.Domain.Models;

/// <summary>
/// Patient entity aligned with FHIR Patient resource.
/// Represents demographics and identity information for patient-centric data organization.
/// </summary>
public sealed class Patient
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Medical Record Number - unique identifier within the platform.
    /// Maps to FHIR Patient.identifier[mrn].
    /// </summary>
    [MaxLength(50)]
    public string? Mrn { get; set; }

    /// <summary>
    /// Patient's given/first name. Maps to FHIR Patient.name.given.
    /// </summary>
    [MaxLength(100)]
    public string? GivenName { get; set; }

    /// <summary>
    /// Patient's family/last name. Maps to FHIR Patient.name.family.
    /// </summary>
    [MaxLength(100)]
    public string? FamilyName { get; set; }

    /// <summary>
    /// Date of birth. Maps to FHIR Patient.birthDate.
    /// </summary>
    public DateOnly? DateOfBirth { get; set; }

    /// <summary>
    /// Administrative gender. Maps to FHIR Patient.gender.
    /// Values: male, female, other, unknown.
    /// </summary>
    [MaxLength(20)]
    public string? Gender { get; set; }

    /// <summary>
    /// Structured address as JSON. Maps to FHIR Patient.address.
    /// </summary>
    [MaxLength(500)]
    public string? Address { get; set; }

    /// <summary>
    /// Phone contact. Maps to FHIR Patient.telecom[phone].
    /// </summary>
    [MaxLength(20)]
    public string? Phone { get; set; }

    /// <summary>
    /// Email contact. Maps to FHIR Patient.telecom[email].
    /// </summary>
    [MaxLength(255)]
    public string? Email { get; set; }

    /// <summary>
    /// Whether the patient record is active. Maps to FHIR Patient.active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Soft delete flag for GDPR compliance.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Timestamp when soft deleted.
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Record creation timestamp. Maps to FHIR Patient.meta.lastUpdated.
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

    // Navigation properties for patient-centric relationships
    public ICollection<Encounter> Encounters { get; set; } = new List<Encounter>();
    public ICollection<Observation> Observations { get; set; } = new List<Observation>();
    public ICollection<MedicationStatement> MedicationStatements { get; set; } = new List<MedicationStatement>();
    public ICollection<Condition> Conditions { get; set; } = new List<Condition>();
    public ICollection<Procedure> Procedures { get; set; } = new List<Procedure>();
    public ICollection<DocumentReference> DocumentReferences { get; set; } = new List<DocumentReference>();
}
