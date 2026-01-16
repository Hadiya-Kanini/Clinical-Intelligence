using ClinicalIntelligence.Api.Contracts.Dlq;

namespace ClinicalIntelligence.Api.Services.Queue;

/// <summary>
/// Abstraction for querying DLQ entries for inspection APIs.
/// Follows ISP (Interface Segregation Principle) - read operations only.
/// </summary>
public interface IDeadLetterQueueReader
{
    /// <summary>
    /// Gets a paginated list of DLQ entries with optional filters.
    /// </summary>
    /// <param name="query">Query parameters including pagination and filters.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Paginated list of DLQ entry summaries.</returns>
    Task<DlqListResponse> GetListAsync(DlqListQuery query, CancellationToken ct = default);

    /// <summary>
    /// Gets a single DLQ entry by ID with full details.
    /// </summary>
    /// <param name="id">DLQ entry ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Full DLQ entry details, or null if not found.</returns>
    Task<DlqItemResponse?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets DLQ metrics for health monitoring.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>DLQ depth metrics.</returns>
    Task<DlqMetricsResponse> GetMetricsAsync(CancellationToken ct = default);
}
