using System.ComponentModel.DataAnnotations;

namespace ClinicalIntelligence.Api.Domain.Models;

/// <summary>
/// Vector query log entity for tracking AI assistant queries.
/// Maps to the vector_query_logs table per ERD specification.
/// </summary>
public sealed class VectorQueryLog
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Reference to the user who ran the query.
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Reference to the patient context for the query.
    /// </summary>
    public Guid? PatientId { get; set; }

    /// <summary>
    /// The natural language query text.
    /// </summary>
    [Required]
    public string QueryText { get; set; } = string.Empty;

    /// <summary>
    /// Number of results returned.
    /// </summary>
    public int ResultCount { get; set; }

    /// <summary>
    /// Response time in milliseconds.
    /// </summary>
    public int? ResponseTimeMs { get; set; }

    /// <summary>
    /// Query timestamp.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Hash of the query for analytics.
    /// </summary>
    [MaxLength(64)]
    public string? QueryHash { get; set; }

    // Navigation properties
    public User? User { get; set; }
    public ErdPatient? Patient { get; set; }
}
