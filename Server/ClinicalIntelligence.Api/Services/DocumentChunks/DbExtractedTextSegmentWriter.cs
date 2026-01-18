using System.Security.Cryptography;
using System.Text;
using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicalIntelligence.Api.Services.DocumentChunks;

/// <summary>
/// EF Core-backed implementation that writes DocumentChunk rows for extracted text segments.
/// Embedding is left null at this stage (handled by US_061/US_062).
/// </summary>
public sealed class DbExtractedTextSegmentWriter : IExtractedTextSegmentWriter
{
    private readonly ApplicationDbContext _dbContext;

    public DbExtractedTextSegmentWriter(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Guid>> WriteSegmentsAsync(
        Guid documentId,
        IEnumerable<ExtractedSegmentInput> segments,
        CancellationToken cancellationToken = default)
    {
        if (segments == null)
            throw new ArgumentNullException(nameof(segments));

        var documentExists = await _dbContext.Documents
            .AnyAsync(d => d.Id == documentId, cancellationToken);

        if (!documentExists)
            throw new InvalidOperationException($"Document with ID {documentId} does not exist.");

        var createdIds = new List<Guid>();

        foreach (var segment in segments)
        {
            if (string.IsNullOrWhiteSpace(segment.Text))
                continue;

            var chunk = new DocumentChunk
            {
                Id = Guid.NewGuid(),
                DocumentId = documentId,
                TextContent = segment.Text,
                Page = segment.Page,
                Section = TruncateIfNeeded(segment.Section, 100),
                Coordinates = TruncateIfNeeded(segment.Coordinates, 100),
                ChunkHash = ComputeHash(segment.Text),
                Embedding = null,
                TokenCount = null
            };

            _dbContext.DocumentChunks.Add(chunk);
            createdIds.Add(chunk.Id);
        }

        if (createdIds.Count > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return createdIds;
    }

    private static string? TruncateIfNeeded(string? value, int maxLength)
    {
        if (value == null)
            return null;

        return value.Length <= maxLength ? value : value[..maxLength];
    }

    private static string ComputeHash(string text)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
