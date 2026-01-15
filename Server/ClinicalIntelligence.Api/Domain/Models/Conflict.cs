using System.ComponentModel.DataAnnotations;

namespace ClinicalIntelligence.Api.Domain.Models;

/// <summary>
/// Data conflict entity for tracking conflicting information across documents.
/// Maps to the conflicts table per ERD specification.
/// Note: This is the ERD Conflict entity, distinct from any existing Condition entity.
/// </summary>
public sealed class ErdConflict
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Reference to the patient.
    /// </summary>
    public Guid PatientId { get; set; }

    /// <summary>
    /// Field name where conflict was detected.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Field { get; set; } = string.Empty;

    /// <summary>
    /// Entity category related to the conflict.
    /// </summary>
    [MaxLength(50)]
    public string? EntityCategory { get; set; }

    /// <summary>
    /// JSON array of conflicting values.
    /// </summary>
    public string? ConflictingValues { get; set; }

    /// <summary>
    /// Conflict severity: Low, Medium, High, Critical.
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Severity { get; set; } = "Medium";

    /// <summary>
    /// Conflict status: Pending, Resolved, Ignored.
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// Timestamp when conflict was detected.
    /// </summary>
    public DateTime DetectedAt { get; set; }

    // Navigation properties
    public ErdPatient Patient { get; set; } = null!;
    public ConflictResolution? Resolution { get; set; }
}
