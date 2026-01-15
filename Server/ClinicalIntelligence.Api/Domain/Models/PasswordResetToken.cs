using System.ComponentModel.DataAnnotations;

namespace ClinicalIntelligence.Api.Domain.Models;

/// <summary>
/// Password reset token entity for secure password recovery.
/// Maps to the password_reset_tokens table per ERD specification.
/// </summary>
public sealed class PasswordResetToken
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Reference to the user requesting password reset.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Hashed reset token for security.
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>
    /// Token expiration timestamp (typically 1 hour).
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Timestamp when the token was used.
    /// </summary>
    public DateTime? UsedAt { get; set; }

    // Navigation property
    public User User { get; set; } = null!;
}
