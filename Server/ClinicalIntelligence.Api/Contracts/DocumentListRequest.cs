namespace ClinicalIntelligence.Api.Contracts;

/// <summary>
/// Request parameters for document list endpoint (TR-017).
/// </summary>
public record DocumentListRequest
{
    /// <summary>
    /// Page number (1-indexed). Default: 1.
    /// </summary>
    public int Page { get; init; } = 1;
    
    /// <summary>
    /// Items per page. Default: 20, Max: 50 (TR-017).
    /// </summary>
    public int PageSize { get; init; } = 20;
    
    /// <summary>
    /// Optional filter by patient ID.
    /// </summary>
    public Guid? PatientId { get; init; }
    
    /// <summary>
    /// Optional filter by status.
    /// </summary>
    public string? Status { get; init; }
    
    /// <summary>
    /// Sort field: uploadedAt, originalName, status. Default: uploadedAt.
    /// </summary>
    public string SortBy { get; init; } = "uploadedAt";
    
    /// <summary>
    /// Sort direction: asc, desc. Default: desc.
    /// </summary>
    public string SortDirection { get; init; } = "desc";
}
