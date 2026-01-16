namespace ClinicalIntelligence.Api.Contracts.Dlq;

/// <summary>
/// Response DTO for paginated DLQ listing.
/// </summary>
public record DlqListResponse
{
    /// <summary>
    /// List of DLQ entry summaries.
    /// </summary>
    public IReadOnlyList<DlqItemSummary> Items { get; init; } = Array.Empty<DlqItemSummary>();

    /// <summary>
    /// Pagination metadata.
    /// </summary>
    public PaginationMetadata Pagination { get; init; } = new();
}

/// <summary>
/// Pagination metadata for list responses.
/// </summary>
public record PaginationMetadata
{
    /// <summary>
    /// Current page number (1-indexed).
    /// </summary>
    public int Page { get; init; } = 1;

    /// <summary>
    /// Number of items per page.
    /// </summary>
    public int PageSize { get; init; } = 20;

    /// <summary>
    /// Total number of items matching the query.
    /// </summary>
    public int TotalItems { get; init; }

    /// <summary>
    /// Total number of pages.
    /// </summary>
    public int TotalPages { get; init; }

    /// <summary>
    /// Whether there is a next page.
    /// </summary>
    public bool HasNextPage { get; init; }

    /// <summary>
    /// Whether there is a previous page.
    /// </summary>
    public bool HasPreviousPage { get; init; }
}

/// <summary>
/// Query parameters for DLQ list endpoint.
/// </summary>
public record DlqListQuery
{
    /// <summary>
    /// Page number (1-indexed). Default: 1.
    /// </summary>
    public int Page { get; init; } = 1;

    /// <summary>
    /// Number of items per page. Default: 20, Max: 100.
    /// </summary>
    public int PageSize { get; init; } = 20;

    /// <summary>
    /// Filter by document ID.
    /// </summary>
    public Guid? DocumentId { get; init; }

    /// <summary>
    /// Filter by processing job ID.
    /// </summary>
    public Guid? ProcessingJobId { get; init; }

    /// <summary>
    /// Filter by status (Pending, Replayed, Discarded).
    /// </summary>
    public string? Status { get; init; }

    /// <summary>
    /// Filter by dead-lettered date range start (inclusive).
    /// </summary>
    public DateTime? FromDate { get; init; }

    /// <summary>
    /// Filter by dead-lettered date range end (inclusive).
    /// </summary>
    public DateTime? ToDate { get; init; }
}
