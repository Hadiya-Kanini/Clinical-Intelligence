using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicalIntelligence.Api.Domain.Models;

/// <summary>
/// Links internal platform entities to external FHIR resources.
/// Supports multiple FHIR versions and source systems per internal entity.
/// </summary>
public sealed class FhirResourceLink
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Internal entity type name (e.g., Patient, Observation, Encounter).
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string InternalEntityType { get; set; } = string.Empty;

    /// <summary>
    /// Internal entity UUID.
    /// </summary>
    [Required]
    public Guid InternalEntityId { get; set; }

    /// <summary>
    /// FHIR resource type (e.g., Patient, Observation, Encounter).
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string FhirResourceType { get; set; } = string.Empty;

    /// <summary>
    /// External FHIR logical resource ID.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string FhirResourceId { get; set; } = string.Empty;

    /// <summary>
    /// FHIR version (e.g., R4, R5).
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string FhirVersion { get; set; } = "R4";

    /// <summary>
    /// Source system identifier (e.g., external EHR system name).
    /// </summary>
    [MaxLength(100)]
    public string? SourceSystem { get; set; }

    /// <summary>
    /// Full FHIR resource URL if available.
    /// </summary>
    [MaxLength(500)]
    public string? FhirResourceUrl { get; set; }

    /// <summary>
    /// Last synchronization timestamp with external system.
    /// </summary>
    public DateTime? LastSyncedAt { get; set; }

    /// <summary>
    /// Record creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last update timestamp.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
