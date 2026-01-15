using System.ComponentModel.DataAnnotations;
using Pgvector;

namespace ClinicalIntelligence.Api.Domain.Models;

/// <summary>
/// Document chunk entity for storing text segments with embeddings.
/// Maps to the document_chunks table per ERD specification.
/// </summary>
public sealed class DocumentChunk
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Reference to the source document.
    /// </summary>
    public Guid DocumentId { get; set; }

    /// <summary>
    /// Page number in the source document.
    /// </summary>
    public int? Page { get; set; }

    /// <summary>
    /// Section identifier within the document.
    /// </summary>
    [MaxLength(100)]
    public string? Section { get; set; }

    /// <summary>
    /// Positional coordinates within the page.
    /// </summary>
    [MaxLength(100)]
    public string? Coordinates { get; set; }

    /// <summary>
    /// Extracted text content.
    /// </summary>
    [Required]
    public string TextContent { get; set; } = string.Empty;

    /// <summary>
    /// 768-dimensional embedding vector for similarity search.
    /// </summary>
    public Vector? Embedding { get; set; }

    /// <summary>
    /// Token count for the chunk.
    /// </summary>
    public int? TokenCount { get; set; }

    /// <summary>
    /// Hash of the chunk content for deduplication.
    /// </summary>
    [MaxLength(64)]
    public string? ChunkHash { get; set; }

    // Navigation properties
    public Document Document { get; set; } = null!;
    public ICollection<EntityCitation> EntityCitations { get; set; } = new List<EntityCitation>();
}
