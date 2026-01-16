# Task - [TASK_002]

## Requirement Reference
- User Story: [us_052]
- Story Location: [.propel/context/tasks/us_052/us_052.md]
- Acceptance Criteria: 
    - Given I navigate to Document List (SCR-006), When displayed, Then I see all my documents.
    - Given the document list, When displayed, Then each document shows status, upload date, and processing metadata (FR-022).
    - Given many documents, When displayed, Then pagination is implemented with configurable page size (TR-017).

## Task Overview
Implement a paginated document list API endpoint that returns documents with status, upload date, and metadata. The endpoint supports filtering by patient, sorting, and configurable page sizes per TR-017.

## Dependent Tasks
- [US_051/task_001] - Backend document status service

## Impacted Components
- [CREATE | Server/ClinicalIntelligence.Api/Contracts/DocumentListRequest.cs | Request DTO with pagination params]
- [CREATE | Server/ClinicalIntelligence.Api/Contracts/DocumentListResponse.cs | Response DTO with pagination metadata]
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Add GET /documents endpoint]

## Implementation Plan

### 1. Create Request DTO
```csharp
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
```

### 2. Create Response DTO
```csharp
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
```

### 3. Implement Endpoint
```csharp
// GET /api/v1/documents
v1.MapGet("/documents", async (
    [AsParameters] DocumentListRequest request,
    HttpContext context,
    ApplicationDbContext dbContext,
    ILogger<Program> logger) =>
{
    var userIdClaim = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
    if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
    {
        return ApiErrorResults.Unauthorized("unauthorized", "User authentication required.");
    }
    
    // Validate pagination parameters (TR-017)
    var page = Math.Max(1, request.Page);
    var pageSize = Math.Clamp(request.PageSize, 1, 50);
    
    // Build query
    var query = dbContext.Documents
        .AsNoTracking()
        .Where(d => !d.IsDeleted);
    
    // Apply filters
    if (request.PatientId.HasValue)
    {
        query = query.Where(d => d.PatientId == request.PatientId.Value);
    }
    
    if (!string.IsNullOrEmpty(request.Status))
    {
        query = query.Where(d => d.Status == request.Status);
    }
    
    // Get total count
    var totalCount = await query.CountAsync(context.RequestAborted);
    var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
    
    // Apply sorting
    query = request.SortBy.ToLowerInvariant() switch
    {
        "originalname" => request.SortDirection.ToLowerInvariant() == "asc" 
            ? query.OrderBy(d => d.OriginalName) 
            : query.OrderByDescending(d => d.OriginalName),
        "status" => request.SortDirection.ToLowerInvariant() == "asc" 
            ? query.OrderBy(d => d.Status) 
            : query.OrderByDescending(d => d.Status),
        _ => request.SortDirection.ToLowerInvariant() == "asc" 
            ? query.OrderBy(d => d.UploadedAt) 
            : query.OrderByDescending(d => d.UploadedAt),
    };
    
    // Apply pagination
    var items = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(d => new DocumentListItem
        {
            Id = d.Id,
            PatientId = d.PatientId,
            OriginalName = d.OriginalName,
            MimeType = d.MimeType,
            SizeBytes = d.SizeBytes,
            Status = d.Status,
            UploadedAt = d.UploadedAt,
            StoragePath = d.StoragePath
        })
        .ToListAsync(context.RequestAborted);
    
    logger.LogInformation(
        "Document list retrieved: UserId={UserId}, Page={Page}, PageSize={PageSize}, TotalCount={TotalCount}",
        userId, page, pageSize, totalCount);
    
    return Results.Ok(new DocumentListResponse
    {
        Items = items,
        TotalCount = totalCount,
        Page = page,
        PageSize = pageSize,
        TotalPages = totalPages
    });
})
    .RequireAuthorization()
    .WithName("GetDocuments")
    .WithOpenApi(operation => new(operation)
    {
        Summary = "List documents with pagination",
        Description = "Returns a paginated list of documents with status and metadata."
    });
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api/Contracts/DocumentListRequest.cs | Request DTO |
| CREATE | Server/ClinicalIntelligence.Api/Contracts/DocumentListResponse.cs | Response DTO |
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Add GET /documents endpoint |

## Build Commands
- dotnet build Server/ClinicalIntelligence.Api
- dotnet test Server/ClinicalIntelligence.Api.Tests

## Implementation Validation Strategy
- [Automated] Unit tests verify pagination logic
- [Automated] Integration tests verify endpoint returns correct data
- [Manual] Verify page size clamping (max 50)
- [Manual] Verify sorting works correctly

## Implementation Checklist
- [x] Create DocumentListRequest DTO
- [x] Create DocumentListResponse and DocumentListItem DTOs
- [x] Implement GET /documents endpoint with pagination
- [x] Add filtering by patientId and status
- [x] Add sorting support
- [x] Add integration tests
- [x] Update OpenAPI documentation
