using System.ComponentModel.DataAnnotations;

namespace ClinicalIntelligence.Api.Domain.Models;

/// <summary>
/// Session tracking entity for user authentication sessions.
/// Maps to the sessions table per ERD specification.
/// </summary>
public sealed class Session
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Reference to the authenticated user.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Browser/client user agent string.
    /// </summary>
    [MaxLength(500)]
    public string? UserAgent { get; set; }

    /// <summary>
    /// Client IP address.
    /// </summary>
    [MaxLength(45)]
    public string? IpAddress { get; set; }

    /// <summary>
    /// Whether the session has been revoked.
    /// </summary>
    public bool IsRevoked { get; set; }

    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Session expiration timestamp.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Last activity timestamp for session timeout tracking.
    /// </summary>
    public DateTime? LastActivityAt { get; set; }

    // Navigation property
    public User User { get; set; } = null!;
    public ICollection<AuditLogEvent> AuditLogEvents { get; set; } = new List<AuditLogEvent>();
}
