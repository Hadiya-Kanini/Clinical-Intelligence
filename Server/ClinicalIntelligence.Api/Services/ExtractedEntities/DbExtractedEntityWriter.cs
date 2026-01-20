using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClinicalIntelligence.Api.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicalIntelligence.Api.Services.ExtractedEntities;

/// <summary>
/// EF Core implementation for persisting extracted entities.
/// Maps worker contract fields to ExtractedEntity domain model.
/// </summary>
public sealed class DbExtractedEntityWriter : IExtractedEntityWriter
{
    private const int MaxCategoryLength = 50;
    private const int MaxNameLength = 200;
    private const int MaxValueLength = 500;

    private readonly IExtractedEntityDbContext _dbContext;
    private readonly ILogger<DbExtractedEntityWriter> _logger;

    public DbExtractedEntityWriter(
        IExtractedEntityDbContext dbContext,
        ILogger<DbExtractedEntityWriter> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<int> WriteEntitiesAsync(
        Guid patientId,
        Guid documentId,
        IReadOnlyList<ExtractedEntityDto> entities,
        CancellationToken cancellationToken = default)
    {
        if (patientId == Guid.Empty)
        {
            throw new ArgumentException("Patient ID cannot be empty.", nameof(patientId));
        }

        if (documentId == Guid.Empty)
        {
            throw new ArgumentException("Document ID cannot be empty.", nameof(documentId));
        }

        if (entities == null || entities.Count == 0)
        {
            _logger.LogDebug(
                "No entities to write for document {DocumentId}",
                documentId);
            return 0;
        }

        var entitiesToAdd = new List<ExtractedEntity>(entities.Count);
        var citationsToAdd = new List<EntityCitation>();

        // Get document chunks for this document to create citations (exclude Embedding to avoid pgvector issues)
        var documentChunks = await _dbContext.DocumentChunks
            .Where(dc => dc.DocumentId == documentId)
            .Select(dc => new DocumentChunk
            {
                Id = dc.Id,
                DocumentId = dc.DocumentId,
                Page = dc.Page,
                Section = dc.Section,
                Coordinates = dc.Coordinates,
                TextContent = dc.TextContent,
                TokenCount = dc.TokenCount,
                ChunkHash = dc.ChunkHash,
                // Exclude Embedding to avoid pgvector serialization issues
                Embedding = null
            })
            .ToListAsync(cancellationToken);

        foreach (var dto in entities)
        {
            var entity = new ExtractedEntity
            {
                Id = Guid.NewGuid(),
                PatientId = patientId,
                DocumentId = documentId,
                Category = TruncateString(dto.EntityGroupName, MaxCategoryLength),
                DisplayCategory = dto.DisplayCategory, // Use mapped category for frontend
                Name = TruncateString(dto.EntityName, MaxNameLength),
                Value = TruncateString(dto.EntityValue, MaxValueLength),
                IsVerified = false,
                // Set nullable properties with defaults
                Units = null,
                ConfidenceScore = null,
                VerifiedByUserId = null,
                VerifiedAt = null,
                EffectiveAt = null
                // Note: Rationale and DataStatus removed as they don't exist in database
            };

            entitiesToAdd.Add(entity);

            // Create entity citations from document location information
            var documentLocation = dto.DocumentLocation;
            if (documentLocation != null)
            {
                // Find the most appropriate document chunk for this entity
                var targetChunk = FindBestMatchingChunk(documentChunks, documentLocation);
                
                if (targetChunk != null)
                {
                    var citation = new EntityCitation
                    {
                        Id = Guid.NewGuid(),
                        ExtractedEntityId = entity.Id,
                        DocumentChunkId = targetChunk.Id,
                        Page = documentLocation.TryGetValue("page", out var pageValue) ? 
                            Convert.ToInt32(pageValue) : (int?)null,
                        Section = documentLocation.TryGetValue("section", out var sectionValue) ? 
                            sectionValue?.ToString() : null,
                        Coordinates = documentLocation.TryGetValue("coordinates", out var coordsValue) ? 
                            coordsValue?.ToString() : null,
                        CitedText = dto.SourceText // Use the source text from the extraction
                    };
                    
                    citationsToAdd.Add(citation);
                }
            }
        }

        // Add entities and citations to database
        _dbContext.ExtractedEntities.AddRange(entitiesToAdd);
        _dbContext.EntityCitations.AddRange(citationsToAdd);
        
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save entities. Error: {Error}", ex.Message);
            throw;
        }

        _logger.LogInformation(
            "Wrote {Count} extracted entities and {CitationCount} citations for document {DocumentId}",
            entitiesToAdd.Count,
            citationsToAdd.Count,
            documentId);

        return entitiesToAdd.Count;
    }

    /// <summary>
    /// Find the best matching document chunk for a given document location.
    /// </summary>
    private static DocumentChunk? FindBestMatchingChunk(
        IReadOnlyList<DocumentChunk> chunks, 
        Dictionary<string, object> documentLocation)
    {
        if (chunks.Count == 0)
            return null;

        // Try to match by page first
        if (documentLocation.TryGetValue("page", out var pageValue))
        {
            var targetPage = Convert.ToInt32(pageValue);
            var pageChunks = chunks.Where(c => c.Page == targetPage).ToList();
            
            if (pageChunks.Count > 0)
            {
                // If multiple chunks on the same page, try to match by section
                if (documentLocation.TryGetValue("section", out var sectionValue))
                {
                    var targetSection = sectionValue?.ToString();
                    var sectionChunks = pageChunks.Where(c => 
                        c.TextContent.Contains(targetSection, StringComparison.OrdinalIgnoreCase)).ToList();
                    
                    if (sectionChunks.Count > 0)
                        return sectionChunks.First();
                }
                
                // Return first chunk on the target page
                return pageChunks.First();
            }
        }

        // Fallback: return the first chunk
        return chunks.First();
    }

    private static string TruncateString(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value ?? string.Empty;
        }

        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
