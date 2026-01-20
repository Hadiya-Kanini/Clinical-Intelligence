using ClinicalIntelligence.Api.Contracts.Entities;

namespace ClinicalIntelligence.Api.Services.Entities;

/// <summary>
/// Service for reading flexible entities from extracted_entities_v2 table.
/// </summary>
public interface IFlexibleEntityReader
{
    /// <summary>
    /// Gets all flexible entities for a patient, grouped by section.
    /// </summary>
    Task<Dictionary<string, List<FlexibleEntityDto>>> GetEntitiesGroupedBySectionAsync(
        Guid patientId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets flexible entities for a specific section.
    /// </summary>
    Task<List<FlexibleEntityDto>> GetEntitiesBySectionAsync(
        Guid patientId,
        string sectionName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all flexible entities for a specific document.
    /// </summary>
    Task<List<FlexibleEntityDto>> GetEntitiesByDocumentAsync(
        Guid documentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets summary statistics for a patient's flexible entities.
    /// </summary>
    Task<FlexibleEntitySummary> GetEntitySummaryAsync(
        Guid patientId,
        CancellationToken cancellationToken = default);
}
