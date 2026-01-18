using ClinicalIntelligence.Api.Contracts.Rag;
using Pgvector;

namespace ClinicalIntelligence.Api.Services.Rag;

/// <summary>
/// Service interface for retrieving document chunks using cosine similarity search.
/// </summary>
public interface IDocumentChunkRetrievalService
{
    /// <summary>
    /// Retrieves the top-K most similar document chunks for a given query embedding.
    /// </summary>
    /// <param name="queryEmbedding">The 768-dimensional query embedding vector.</param>
    /// <param name="k">Number of results to return (clamped to 10-15 range, default 15).</param>
    /// <param name="documentId">Optional document ID to scope the search.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of retrieved chunks ordered by similarity (most similar first).</returns>
    Task<IReadOnlyList<RetrievedChunkDto>> RetrieveTopKAsync(
        Vector queryEmbedding,
        int k = 15,
        Guid? documentId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the top-K most similar document chunks for a given query embedding array.
    /// </summary>
    /// <param name="queryEmbedding">The 768-dimensional query embedding as float array.</param>
    /// <param name="k">Number of results to return (clamped to 10-15 range, default 15).</param>
    /// <param name="documentId">Optional document ID to scope the search.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of retrieved chunks ordered by similarity (most similar first).</returns>
    Task<IReadOnlyList<RetrievedChunkDto>> RetrieveTopKAsync(
        float[] queryEmbedding,
        int k = 15,
        Guid? documentId = null,
        CancellationToken cancellationToken = default);
}
