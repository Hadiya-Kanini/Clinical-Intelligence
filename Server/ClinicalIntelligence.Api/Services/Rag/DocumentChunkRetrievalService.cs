using ClinicalIntelligence.Api.Contracts.Rag;
using ClinicalIntelligence.Api.Data;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace ClinicalIntelligence.Api.Services.Rag;

/// <summary>
/// Implementation of document chunk retrieval using pgvector cosine similarity.
/// Relies on DR-005/RLS for access control rather than application-side filtering.
/// </summary>
public sealed class DocumentChunkRetrievalService : IDocumentChunkRetrievalService
{
    private const int MinK = 10;
    private const int MaxK = 15;
    private const int DefaultK = 15;

    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<DocumentChunkRetrievalService> _logger;

    public DocumentChunkRetrievalService(
        ApplicationDbContext dbContext,
        ILogger<DocumentChunkRetrievalService> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IReadOnlyList<RetrievedChunkDto>> RetrieveTopKAsync(
        Vector queryEmbedding,
        int k = DefaultK,
        Guid? documentId = null,
        CancellationToken cancellationToken = default)
    {
        if (queryEmbedding == null)
        {
            throw new ArgumentNullException(nameof(queryEmbedding));
        }

        var clampedK = ClampK(k);
        
        _logger.LogDebug(
            "Retrieving top {K} chunks (requested: {RequestedK}, documentId: {DocumentId})",
            clampedK, k, documentId);

        try
        {
            var query = _dbContext.DocumentChunks
                .Where(c => c.Embedding != null);

            if (documentId.HasValue)
            {
                query = query.Where(c => c.DocumentId == documentId.Value);
            }

            var results = await query
                .OrderBy(c => c.Embedding!.CosineDistance(queryEmbedding))
                .ThenBy(c => c.Id)
                .Take(clampedK)
                .Select(c => new
                {
                    c.Id,
                    c.DocumentId,
                    c.TextContent,
                    c.Page,
                    c.Section,
                    c.Coordinates,
                    Distance = c.Embedding!.CosineDistance(queryEmbedding)
                })
                .ToListAsync(cancellationToken);

            var dtos = results
                .Select((r, index) => new RetrievedChunkDto
                {
                    ChunkId = r.Id,
                    DocumentId = r.DocumentId,
                    TextContent = r.TextContent,
                    Page = r.Page,
                    Section = r.Section,
                    Coordinates = r.Coordinates,
                    Score = 1.0 - r.Distance,
                    Rank = index + 1
                })
                .ToList();

            _logger.LogDebug("Retrieved {Count} chunks", dtos.Count);
            return dtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving chunks for similarity search");
            throw;
        }
    }

    public Task<IReadOnlyList<RetrievedChunkDto>> RetrieveTopKAsync(
        float[] queryEmbedding,
        int k = DefaultK,
        Guid? documentId = null,
        CancellationToken cancellationToken = default)
    {
        if (queryEmbedding == null)
        {
            throw new ArgumentNullException(nameof(queryEmbedding));
        }

        if (queryEmbedding.Length != 768)
        {
            throw new ArgumentException(
                $"Query embedding must be 768 dimensions, got {queryEmbedding.Length}",
                nameof(queryEmbedding));
        }

        var vector = new Vector(queryEmbedding);
        return RetrieveTopKAsync(vector, k, documentId, cancellationToken);
    }

    private static int ClampK(int k)
    {
        if (k < MinK) return MinK;
        if (k > MaxK) return MaxK;
        return k;
    }
}
