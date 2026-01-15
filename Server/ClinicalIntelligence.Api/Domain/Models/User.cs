using System.ComponentModel.DataAnnotations;

namespace ClinicalIntelligence.Api.Domain.Models;

/// <summary>
/// User account entity for authentication and authorization.
/// Maps to the users table per ERD specification.
/// </summary>
public sealed class User
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Unique email address for login.
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Hashed password using secure algorithm.
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Display name for the user.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// User role: Admin or Standard.
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = "Standard";

    /// <summary>
    /// Account status: Active, Inactive, Locked.
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Active";

    /// <summary>
    /// Count of consecutive failed login attempts.
    /// </summary>
    public int FailedLoginAttempts { get; set; }

    /// <summary>
    /// Timestamp until which the account is locked.
    /// </summary>
    public DateTime? LockedUntil { get; set; }

    /// <summary>
    /// Indicates if this is the protected static admin account seeded via migration.
    /// </summary>
    public bool IsStaticAdmin { get; set; }

    /// <summary>
    /// Soft delete flag.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Timestamp when soft deleted.
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
    public ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();
    public ICollection<DocumentBatch> UploadedBatches { get; set; } = new List<DocumentBatch>();
    public ICollection<Document> UploadedDocuments { get; set; } = new List<Document>();
    public ICollection<ExtractedEntity> VerifiedEntities { get; set; } = new List<ExtractedEntity>();
    public ICollection<ConflictResolution> ConflictResolutions { get; set; } = new List<ConflictResolution>();
    public ICollection<CodeSuggestion> CodeDecisions { get; set; } = new List<CodeSuggestion>();
    public ICollection<AuditLogEvent> AuditLogEvents { get; set; } = new List<AuditLogEvent>();
    // TEMPORARY: Commented out for vector DB installation
    // public ICollection<VectorQueryLog> VectorQueryLogs { get; set; } = new List<VectorQueryLog>();
}
