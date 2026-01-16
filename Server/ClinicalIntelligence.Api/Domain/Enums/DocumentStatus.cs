namespace ClinicalIntelligence.Api.Domain.Enums;

/// <summary>
/// Document processing status values per FR-020 and TR-004.
/// </summary>
public enum DocumentStatus
{
    /// <summary>
    /// Document uploaded, awaiting processing.
    /// </summary>
    Pending,
    
    /// <summary>
    /// Document is being processed by AI worker.
    /// </summary>
    Processing,
    
    /// <summary>
    /// Processing completed successfully.
    /// </summary>
    Completed,
    
    /// <summary>
    /// Processing failed due to system error.
    /// </summary>
    Failed,
    
    /// <summary>
    /// Processing failed due to validation errors (TR-004).
    /// </summary>
    ValidationFailed
}
