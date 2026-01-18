using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ClinicalIntelligence.Api.Domain.Models;
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
        }

        // Disable navigation properties temporarily to avoid issues
        _dbContext.ExtractedEntities.AddRange(entitiesToAdd);
        
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
            "Wrote {Count} extracted entities for document {DocumentId}",
            entitiesToAdd.Count,
            documentId);

        return entitiesToAdd.Count;
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
