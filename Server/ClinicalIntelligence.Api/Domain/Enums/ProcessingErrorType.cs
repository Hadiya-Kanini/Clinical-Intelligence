namespace ClinicalIntelligence.Api.Domain.Enums;

/// <summary>
/// Classification of processing errors for retry decisions.
/// </summary>
public enum ProcessingErrorType
{
    /// <summary>
    /// Unknown error type - treat as retryable.
    /// </summary>
    Unknown,
    
    /// <summary>
    /// Transient errors that may succeed on retry.
    /// Examples: network timeout, service unavailable, rate limit.
    /// </summary>
    Transient,
    
    /// <summary>
    /// Permanent errors that will not succeed on retry.
    /// Examples: invalid document format, validation failure, malformed data.
    /// </summary>
    Permanent,
    
    /// <summary>
    /// Resource not found - may be retryable if timing issue.
    /// </summary>
    NotFound,
    
    /// <summary>
    /// Authentication/authorization failure - not retryable.
    /// </summary>
    Unauthorized,
    
    /// <summary>
    /// External service error - retryable.
    /// </summary>
    ExternalService,
    
    /// <summary>
    /// Database error - may be retryable.
    /// </summary>
    Database,
    
    /// <summary>
    /// AI/LLM service error - retryable.
    /// </summary>
    AiService
}
