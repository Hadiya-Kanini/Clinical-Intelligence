using System.ComponentModel.DataAnnotations;

namespace ClinicalIntelligence.Api.Domain.Models;

/// <summary>
/// Conflict resolution entity for tracking how conflicts were resolved.
/// Maps to the conflict_resolutions table per ERD specification.
/// </summary>
public sealed class ConflictResolution
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Reference to the resolved conflict.
    /// </summary>
    public Guid ConflictId { get; set; }

    /// <summary>
    /// Reference to the user who resolved the conflict.
    /// </summary>
    public Guid ResolvedByUserId { get; set; }

    /// <summary>
    /// The resolved value chosen or entered.
    /// </summary>
    public string? ResolvedValue { get; set; }

    /// <summary>
    /// Timestamp when resolved.
    /// </summary>
    public DateTime ResolvedAt { get; set; }

    // Navigation properties
    public ErdConflict Conflict { get; set; } = null!;
    public User ResolvedByUser { get; set; } = null!;
}
