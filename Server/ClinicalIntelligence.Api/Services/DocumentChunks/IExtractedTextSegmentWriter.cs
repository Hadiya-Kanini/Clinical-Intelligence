using ClinicalIntelligence.Api.Domain.Models;

namespace ClinicalIntelligence.Api.Services.DocumentChunks;

/// <summary>
/// Abstraction for persisting extracted text segments into document_chunks.
/// Follows DIP: consumers depend on this interface, not the concrete implementation.
/// </summary>
public interface IExtractedTextSegmentWriter
{
    /// <summary>
    /// Persists extracted text segments as DocumentChunk rows.
    /// </summary>
    /// <param name="documentId">The source document identifier.</param>
    /// <param name="segments">List of extracted segments with positional metadata.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of created DocumentChunk IDs.</returns>
    Task<IReadOnlyList<Guid>> WriteSegmentsAsync(
        Guid documentId,
        IEnumerable<ExtractedSegmentInput> segments,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Input model for an extracted text segment to be persisted.
/// </summary>
public sealed record ExtractedSegmentInput
{
    /// <summary>
    /// The extracted text content.
    /// </summary>
    public required string Text { get; init; }

    /// <summary>
    /// Page number (1-indexed) where the text appears. Null if not available.
    /// </summary>
    public int? Page { get; init; }

    /// <summary>
    /// Section identifier or heading name. Null if not available.
    /// </summary>
    public string? Section { get; init; }

    /// <summary>
    /// JSON-serialized bounding box coordinates. Null if not available.
    /// </summary>
    public string? Coordinates { get; init; }
}
