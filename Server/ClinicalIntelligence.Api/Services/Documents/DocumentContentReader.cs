using ClinicalIntelligence.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicalIntelligence.Api.Services.Documents;

/// <summary>
/// Result of document content retrieval operation.
/// </summary>
public sealed record DocumentContentResult
{
    public bool Success { get; init; }
    public Stream? ContentStream { get; init; }
    public string? MimeType { get; init; }
    public string? FileName { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }

    public static DocumentContentResult NotFound() => new()
    {
        Success = false,
        ErrorCode = "document_not_found",
        ErrorMessage = "The requested document was not found."
    };

    public static DocumentContentResult FileNotFound() => new()
    {
        Success = false,
        ErrorCode = "file_not_found",
        ErrorMessage = "The document file could not be located."
    };

    public static DocumentContentResult Ok(Stream stream, string mimeType, string fileName) => new()
    {
        Success = true,
        ContentStream = stream,
        MimeType = mimeType,
        FileName = fileName
    };
}

/// <summary>
/// Service for retrieving document content for source navigation (US_070 TASK_003).
/// Handles document lookup, access checks, and file streaming.
/// </summary>
public interface IDocumentContentReader
{
    /// <summary>
    /// Gets the document content stream for the specified document ID.
    /// </summary>
    /// <param name="documentId">The document ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Document content result with stream or error information.</returns>
    Task<DocumentContentResult> GetDocumentContentAsync(
        Guid documentId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// EF Core implementation of document content reader.
/// </summary>
public sealed class DocumentContentReader : IDocumentContentReader
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<DocumentContentReader> _logger;

    public DocumentContentReader(
        ApplicationDbContext dbContext,
        ILogger<DocumentContentReader> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<DocumentContentResult> GetDocumentContentAsync(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        // Load document metadata (exclude soft-deleted)
        var document = await _dbContext.Documents
            .AsNoTracking()
            .Where(d => d.Id == documentId && !d.IsDeleted)
            .Select(d => new
            {
                d.Id,
                d.StoragePath,
                d.MimeType,
                d.OriginalName
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (document == null)
        {
            _logger.LogDebug("Document not found or deleted: {DocumentId}", documentId);
            return DocumentContentResult.NotFound();
        }

        // Validate storage path exists
        if (string.IsNullOrWhiteSpace(document.StoragePath))
        {
            _logger.LogWarning("Document {DocumentId} has no storage path", documentId);
            return DocumentContentResult.FileNotFound();
        }

        // Check if file exists on disk
        if (!File.Exists(document.StoragePath))
        {
            _logger.LogWarning(
                "Document file not found on disk: {DocumentId}, Path: {StoragePath}",
                documentId,
                document.StoragePath);
            return DocumentContentResult.FileNotFound();
        }

        try
        {
            // Open file stream for reading
            var stream = new FileStream(
                document.StoragePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 4096,
                useAsync: true);

            return DocumentContentResult.Ok(
                stream,
                document.MimeType ?? "application/octet-stream",
                document.OriginalName);
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Failed to open document file: {DocumentId}", documentId);
            return DocumentContentResult.FileNotFound();
        }
    }
}
