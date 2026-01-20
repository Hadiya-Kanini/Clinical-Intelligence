using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicalIntelligence.Api.Services.ExtractedEntities;

/// <summary>
/// DTO for extracted entity data from the worker.
/// Maps worker contract fields to backend persistence.
/// </summary>
public sealed record ExtractedEntityDto
{
    /// <summary>
    /// Entity category (maps to entity_group_name from worker contract).
    /// </summary>
    public required string EntityGroupName { get; init; }

    /// <summary>
    /// Entity name/label (maps to entity_name from worker contract).
    /// </summary>
    public required string EntityName { get; init; }

    /// <summary>
    /// Entity value (maps to entity_value from worker contract).
    /// </summary>
    public required string EntityValue { get; init; }

    /// <summary>
    /// Display category formatted for frontend (e.g., "Allergies", "Medications").
    /// </summary>
    public string? DisplayCategory { get; init; }

    /// <summary>
    /// Document location information for citation creation.
    /// </summary>
    public Dictionary<string, object>? DocumentLocation { get; init; }

    /// <summary>
    /// Source text from which the entity was extracted.
    /// </summary>
    public string? SourceText { get; init; }
}

/// <summary>
/// Interface for persisting extracted entities to the database.
/// Supports dependency inversion for testability (DIP).
/// </summary>
public interface IExtractedEntityWriter
{
    /// <summary>
    /// Writes extracted entities for a document to the database.
    /// </summary>
    /// <param name="patientId">The patient ID.</param>
    /// <param name="documentId">The document ID.</param>
    /// <param name="entities">List of extracted entities to persist.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of entities written.</returns>
    Task<int> WriteEntitiesAsync(
        Guid patientId,
        Guid documentId,
        IReadOnlyList<ExtractedEntityDto> entities,
        CancellationToken cancellationToken = default);
}
