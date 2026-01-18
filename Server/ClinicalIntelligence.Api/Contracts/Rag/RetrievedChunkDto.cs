namespace ClinicalIntelligence.Api.Contracts.Rag;

/// <summary>
/// DTO representing a retrieved document chunk from similarity search.
/// </summary>
public sealed record RetrievedChunkDto
{
    /// <summary>
    /// Unique identifier of the chunk.
    /// </summary>
    public Guid ChunkId { get; init; }

    /// <summary>
    /// Identifier of the source document.
    /// </summary>
    public Guid DocumentId { get; init; }

    /// <summary>
    /// The text content of the chunk.
    /// </summary>
    public string TextContent { get; init; } = string.Empty;

    /// <summary>
    /// Page number in the source document (nullable).
    /// </summary>
    public int? Page { get; init; }

    /// <summary>
    /// Section identifier within the document (nullable).
    /// </summary>
    public string? Section { get; init; }

    /// <summary>
    /// Positional coordinates within the page (nullable).
    /// </summary>
    public string? Coordinates { get; init; }

    /// <summary>
    /// Cosine similarity score (1 - distance). Higher is more similar.
    /// Range: 0.0 to 1.0 where 1.0 is identical.
    /// </summary>
    public double Score { get; init; }

    /// <summary>
    /// Rank position in the result set (1-based).
    /// </summary>
    public int Rank { get; init; }
}
