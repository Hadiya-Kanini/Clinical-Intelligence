using System.Text.Json;
using ClinicalIntelligence.Api.Contracts.Entities;
using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicalIntelligence.Api.Services.Entities;

/// <summary>
/// Service for reading flexible entities from extracted_entities_v2 table.
/// </summary>
public sealed class FlexibleEntityReader : IFlexibleEntityReader
{
    private readonly ApplicationDbContext _context;

    public FlexibleEntityReader(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, List<FlexibleEntityDto>>> GetEntitiesGroupedBySectionAsync(
        Guid patientId,
        CancellationToken cancellationToken = default)
    {
        var entities = await _context.ExtractedEntitiesV2
            .Where(e => e.PatientId == patientId)
            .OrderBy(e => e.SectionName)
            .ThenBy(e => e.ExtractedAt)
            .ToListAsync(cancellationToken);

        var grouped = entities
            .GroupBy(e => e.SectionName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(MapToDto).ToList()
            );

        return grouped;
    }

    /// <inheritdoc />
    public async Task<List<FlexibleEntityDto>> GetEntitiesBySectionAsync(
        Guid patientId,
        string sectionName,
        CancellationToken cancellationToken = default)
    {
        var entities = await _context.ExtractedEntitiesV2
            .Where(e => e.PatientId == patientId && e.SectionName == sectionName)
            .OrderBy(e => e.ExtractedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToDto).ToList();
    }

    /// <inheritdoc />
    public async Task<List<FlexibleEntityDto>> GetEntitiesByDocumentAsync(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        var entities = await _context.ExtractedEntitiesV2
            .Where(e => e.DocumentId == documentId)
            .OrderBy(e => e.SectionName)
            .ThenBy(e => e.ExtractedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToDto).ToList();
    }

    /// <inheritdoc />
    public async Task<FlexibleEntitySummary> GetEntitySummaryAsync(
        Guid patientId,
        CancellationToken cancellationToken = default)
    {
        var entities = await _context.ExtractedEntitiesV2
            .Where(e => e.PatientId == patientId)
            .ToListAsync(cancellationToken);

        var sectionBreakdown = entities
            .GroupBy(e => e.SectionName)
            .ToDictionary(g => g.Key, g => g.Count());

        var entitiesWithSources = entities.Count(e => !string.IsNullOrEmpty(e.SourceReference));
        var verifiedEntities = entities.Count(e => e.IsVerified);

        return new FlexibleEntitySummary
        {
            TotalEntities = entities.Count,
            SectionCount = sectionBreakdown.Count,
            SectionBreakdown = sectionBreakdown,
            EntitiesWithSources = entitiesWithSources,
            VerifiedEntities = verifiedEntities
        };
    }

    private static FlexibleEntityDto MapToDto(ExtractedEntityV2 entity)
    {
        // Parse JSONB entity data
        var entityData = new Dictionary<string, object>();
        
        try
        {
            if (!string.IsNullOrEmpty(entity.EntityData))
            {
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(entity.EntityData);
                entityData = JsonElementToDictionary(jsonElement);
            }
        }
        catch (JsonException)
        {
            // If parsing fails, return empty dictionary
            entityData = new Dictionary<string, object>();
        }

        return new FlexibleEntityDto
        {
            Id = entity.Id,
            SectionName = entity.SectionName,
            EntityData = entityData,
            SourceReference = entity.SourceReference,
            ExtractedAt = entity.ExtractedAt,
            IsVerified = entity.IsVerified,
            VerifiedAt = entity.VerifiedAt
        };
    }

    private static Dictionary<string, object> JsonElementToDictionary(JsonElement element)
    {
        var dictionary = new Dictionary<string, object>();

        if (element.ValueKind != JsonValueKind.Object)
        {
            return dictionary;
        }

        foreach (var property in element.EnumerateObject())
        {
            dictionary[property.Name] = JsonElementToObject(property.Value);
        }

        return dictionary;
    }

    private static object JsonElementToObject(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? string.Empty,
            JsonValueKind.Number => element.TryGetInt64(out var longValue) ? longValue : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Array => element.EnumerateArray().Select(JsonElementToObject).ToList(),
            JsonValueKind.Object => JsonElementToDictionary(element),
            JsonValueKind.Null => null!,
            _ => element.GetRawText()
        };
    }
}
