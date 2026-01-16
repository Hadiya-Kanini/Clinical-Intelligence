namespace ClinicalIntelligence.Api.Contracts;

/// <summary>
/// Paginated document list response (FR-022, TR-017).
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
/// Individual document item in list response.
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
}
