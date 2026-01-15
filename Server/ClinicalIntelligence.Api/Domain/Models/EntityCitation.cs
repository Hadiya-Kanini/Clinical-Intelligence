using System.ComponentModel.DataAnnotations;

namespace ClinicalIntelligence.Api.Domain.Models;

/// <summary>
/// Entity citation linking extracted entities to source document chunks.
/// Maps to the entity_citations table per ERD specification.
/// </summary>
public sealed class EntityCitation
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Reference to the extracted entity.
    /// </summary>
    public Guid ExtractedEntityId { get; set; }

    /// <summary>
    /// Reference to the source document chunk.
    /// </summary>
    public Guid DocumentChunkId { get; set; }

    /// <summary>
    /// Page number in the source document.
    /// </summary>
    public int? Page { get; set; }

    /// <summary>
    /// Section identifier.
    /// </summary>
    [MaxLength(100)]
    public string? Section { get; set; }

    /// <summary>
    /// Positional coordinates.
    /// </summary>
    [MaxLength(100)]
    public string? Coordinates { get; set; }

    /// <summary>
    /// The cited text from the source.
    /// </summary>
    public string? CitedText { get; set; }

    // Navigation properties
    public ExtractedEntity ExtractedEntity { get; set; } = null!;
    public DocumentChunk DocumentChunk { get; set; } = null!;
}
