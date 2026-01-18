namespace ClinicalIntelligence.Api.Contracts;

/// <summary>
/// Paginated document list response (FR-022, TR-017, FR-027, FR-028).
/// </summary>
public record DocumentListResponse
{
    public IReadOnlyList<DocumentListItem> Items { get; init; } = Array.Empty<DocumentListItem>();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
}

/// <summary>
/// Individual document item in list response with processing metadata (US_056).
/// </summary>
public record DocumentListItem
{
    public Guid Id { get; init; }
    public Guid PatientId { get; init; }
    public string OriginalName { get; init; } = string.Empty;
    public string MimeType { get; init; } = string.Empty;
    public int SizeBytes { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime UploadedAt { get; init; }
    public string StoragePath { get; init; } = string.Empty;

    // Processing metadata fields (FR-027, FR-028)
    /// <summary>
    /// Most recent processing job ID for this document.
    /// </summary>
    public Guid? JobId { get; init; }

    /// <summary>
    /// Number of retry attempts for the most recent processing job.
    /// </summary>
    public int? RetryCount { get; init; }

    /// <summary>
    /// Timestamp when processing started (UTC).
    /// </summary>
    public DateTime? StartedAt { get; init; }

    /// <summary>
    /// Timestamp when processing completed (UTC).
    /// </summary>
    public DateTime? CompletedAt { get; init; }

    /// <summary>
    /// Processing duration in milliseconds.
    /// </summary>
    public int? ProcessingTimeMs { get; init; }

    /// <summary>
    /// User-safe error message for failed documents. Null for non-failed documents.
    /// </summary>
    public string? ErrorMessage { get; init; }
}
