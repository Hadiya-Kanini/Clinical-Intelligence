namespace ClinicalIntelligence.Api.Contracts;

/// <summary>
/// Document processing job message for RabbitMQ (FR-023).
/// </summary>
public record DocumentProcessingJob
{
    /// <summary>
    /// Unique job identifier.
    /// </summary>
    public Guid JobId { get; init; }
    
    /// <summary>
    /// Document to process.
    /// </summary>
    public Guid DocumentId { get; init; }
    
    /// <summary>
    /// Patient associated with document.
    /// </summary>
    public Guid PatientId { get; init; }
    
    /// <summary>
    /// User who uploaded the document.
    /// </summary>
    public Guid UploadedByUserId { get; init; }
    
    /// <summary>
    /// Original file name.
    /// </summary>
    public string OriginalName { get; init; } = string.Empty;
    
    /// <summary>
    /// MIME type of document.
    /// </summary>
    public string MimeType { get; init; } = string.Empty;
    
    /// <summary>
    /// Storage path for document retrieval.
    /// </summary>
    public string StoragePath { get; init; } = string.Empty;
    
    /// <summary>
    /// File size in bytes.
    /// </summary>
    public long SizeBytes { get; init; }
    
    /// <summary>
    /// Job creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; init; }
    
    /// <summary>
    /// Current retry attempt (0 = first attempt).
    /// </summary>
    public int RetryCount { get; init; }
    
    /// <summary>
    /// Correlation ID for tracing.
    /// </summary>
    public string? CorrelationId { get; init; }
}
