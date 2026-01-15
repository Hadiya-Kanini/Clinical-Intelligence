using System.ComponentModel.DataAnnotations;

namespace ClinicalIntelligence.Api.Domain.Models;

/// <summary>
/// Audit log event entity for security and compliance tracking.
/// Maps to the audit_log_events table per ERD specification.
/// </summary>
public sealed class AuditLogEvent
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Reference to the user who triggered the event.
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Reference to the session during which the event occurred.
    /// </summary>
    public Guid? SessionId { get; set; }

    /// <summary>
    /// Type of action: LOGIN_SUCCESS, LOGIN_FAILED, USER_CREATED, etc.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string ActionType { get; set; } = string.Empty;

    /// <summary>
    /// Client IP address.
    /// </summary>
    [MaxLength(45)]
    public string? IpAddress { get; set; }

    /// <summary>
    /// Browser/client user agent.
    /// </summary>
    [MaxLength(500)]
    public string? UserAgent { get; set; }

    /// <summary>
    /// Type of resource affected: User, Patient, Document, etc.
    /// </summary>
    [MaxLength(50)]
    public string? ResourceType { get; set; }

    /// <summary>
    /// ID of the affected resource.
    /// </summary>
    public Guid? ResourceId { get; set; }

    /// <summary>
    /// Additional metadata as JSON.
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// Event timestamp.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Integrity hash for tamper detection.
    /// </summary>
    [MaxLength(128)]
    public string? IntegrityHash { get; set; }

    // Navigation properties
    public User? User { get; set; }
    public Session? Session { get; set; }
}
