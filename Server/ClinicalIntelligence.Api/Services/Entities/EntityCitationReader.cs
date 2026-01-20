using ClinicalIntelligence.Api.Contracts.Entities;
using ClinicalIntelligence.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicalIntelligence.Api.Services.Entities;

/// <summary>
/// Query helper to retrieve entity citations for Patient 360 usage.
/// </summary>
public interface IEntityCitationReader
{
    /// <summary>
    /// Gets all citations for a specific extracted entity.
    /// </summary>
    Task<IReadOnlyList<EntityCitationDto>> GetCitationsForEntityAsync(
        Guid entityId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all entities with their citations for a patient.
    /// </summary>
    Task<IReadOnlyList<ExtractedEntityWithCitationsDto>> GetEntitiesWithCitationsForPatientAsync(
        Guid patientId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all entities with their citations for a specific document.
    /// </summary>
    Task<IReadOnlyList<ExtractedEntityWithCitationsDto>> GetEntitiesWithCitationsForDocumentAsync(
        Guid documentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets only grounded entities (those with at least one citation) for a patient.
    /// Used by Patient 360 API to enforce grounding requirements.
    /// </summary>
    Task<IReadOnlyList<ExtractedEntityWithCitationsDto>> GetGroundedEntitiesForPatientAsync(
        Guid patientId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// EF Core implementation of entity citation reader.
/// </summary>
public sealed class EntityCitationReader : IEntityCitationReader
{
    private readonly ApplicationDbContext _dbContext;

    public EntityCitationReader(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<EntityCitationDto>> GetCitationsForEntityAsync(
        Guid entityId,
        CancellationToken cancellationToken = default)
    {
        var citations = await _dbContext.EntityCitations
            .Where(c => c.ExtractedEntityId == entityId)
            .Include(c => c.DocumentChunk)
                .ThenInclude(dc => dc.Document)
            .Select(c => new EntityCitationDto
            {
                Id = c.Id,
                DocumentId = c.DocumentChunk.DocumentId,
                DocumentName = c.DocumentChunk.Document.OriginalName,
                Page = c.Page,
                Section = c.Section,
                Coordinates = c.Coordinates,
                CitedText = c.CitedText
            })
            .ToListAsync(cancellationToken);

        return citations;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ExtractedEntityWithCitationsDto>> GetEntitiesWithCitationsForPatientAsync(
        Guid patientId,
        CancellationToken cancellationToken = default)
    {
        var entities = await _dbContext.ExtractedEntities
            .Where(e => e.PatientId == patientId)
            .Include(e => e.EntityCitations)
                .ThenInclude(c => c.DocumentChunk)
                    .ThenInclude(dc => dc.Document)
            .OrderBy(e => e.Category)
            .ThenBy(e => e.Name)
            .ToListAsync(cancellationToken);

        return entities.Select(e => new ExtractedEntityWithCitationsDto
        {
            Id = e.Id,
            Category = e.Category,
            DisplayCategory = e.DisplayCategory,
            Name = e.Name,
            Value = e.Value,
            Units = e.Units,
            ConfidenceScore = e.ConfidenceScore,
            IsVerified = e.IsVerified,
            EffectiveAt = e.EffectiveAt,
            Rationale = null, // Rationale field not implemented in database schema
            Citations = e.EntityCitations.Select(c => new EntityCitationDto
            {
                Id = c.Id,
                DocumentId = c.DocumentChunk?.DocumentId ?? Guid.Empty,
                DocumentName = c.DocumentChunk?.Document?.OriginalName,
                Page = c.Page,
                Section = c.Section,
                Coordinates = c.Coordinates,
                CitedText = c.CitedText
            }).ToList()
        }).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ExtractedEntityWithCitationsDto>> GetEntitiesWithCitationsForDocumentAsync(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        var entities = await _dbContext.ExtractedEntities
            .Where(e => e.DocumentId == documentId)
            .Include(e => e.EntityCitations)
                .ThenInclude(c => c.DocumentChunk)
                    .ThenInclude(dc => dc.Document)
            .OrderBy(e => e.Category)
            .ThenBy(e => e.Name)
            .ToListAsync(cancellationToken);

        return entities.Select(e => new ExtractedEntityWithCitationsDto
        {
            Id = e.Id,
            Category = e.Category,
            DisplayCategory = e.DisplayCategory,
            Name = e.Name,
            Value = e.Value,
            Units = e.Units,
            ConfidenceScore = e.ConfidenceScore,
            IsVerified = e.IsVerified,
            EffectiveAt = e.EffectiveAt,
            Rationale = null, // Rationale field not implemented in database schema
            Citations = e.EntityCitations.Select(c => new EntityCitationDto
            {
                Id = c.Id,
                DocumentId = c.DocumentChunk?.DocumentId ?? Guid.Empty,
                DocumentName = c.DocumentChunk?.Document?.OriginalName,
                Page = c.Page,
                Section = c.Section,
                Coordinates = c.Coordinates,
                CitedText = c.CitedText
            }).ToList()
        }).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ExtractedEntityWithCitationsDto>> GetGroundedEntitiesForPatientAsync(
        Guid patientId,
        CancellationToken cancellationToken = default)
    {
        // Only return entities that have at least one citation
        var entities = await _dbContext.ExtractedEntities
            .Where(e => e.PatientId == patientId)
            .Where(e => e.EntityCitations.Any()) // Filter to grounded entities only
            .Include(e => e.EntityCitations)
                .ThenInclude(c => c.DocumentChunk)
                    .ThenInclude(dc => dc.Document)
            .OrderBy(e => e.Category)
            .ThenBy(e => e.Name)
            .ToListAsync(cancellationToken);

        return entities.Select(e => new ExtractedEntityWithCitationsDto
        {
            Id = e.Id,
            Category = e.Category,
            DisplayCategory = e.DisplayCategory,
            Name = e.Name,
            Value = e.Value,
            Units = e.Units,
            ConfidenceScore = e.ConfidenceScore,
            IsVerified = e.IsVerified,
            EffectiveAt = e.EffectiveAt,
            Rationale = null, // Rationale field not implemented in database schema
            Citations = e.EntityCitations.Select(c => new EntityCitationDto
            {
                Id = c.Id,
                DocumentId = c.DocumentChunk?.DocumentId ?? Guid.Empty,
                DocumentName = c.DocumentChunk?.Document?.OriginalName,
                Page = c.Page,
                Section = c.Section,
                Coordinates = c.Coordinates,
                CitedText = c.CitedText
            }).ToList()
        }).ToList();
    }
}
