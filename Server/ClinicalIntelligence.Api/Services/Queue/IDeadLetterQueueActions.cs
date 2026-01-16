using ClinicalIntelligence.Api.Contracts.Dlq;

namespace ClinicalIntelligence.Api.Services.Queue;

/// <summary>
/// Abstraction for DLQ replay/discard operations.
/// Follows ISP (Interface Segregation Principle) - action operations only.
/// </summary>
public interface IDeadLetterQueueActions
{
    /// <summary>
    /// Replays a DLQ entry by re-enqueuing the original job.
    /// </summary>
    /// <param name="deadLetterJobId">ID of the DLQ entry to replay.</param>
    /// <param name="operatorUserId">ID of the operator performing the action.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result of the replay operation.</returns>
    Task<DlqReplayResponse> ReplayAsync(Guid deadLetterJobId, Guid operatorUserId, CancellationToken ct = default);

    /// <summary>
    /// Discards a DLQ entry, marking it as permanently dismissed.
    /// </summary>
    /// <param name="deadLetterJobId">ID of the DLQ entry to discard.</param>
    /// <param name="operatorUserId">ID of the operator performing the action.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result of the discard operation.</returns>
    Task<DlqDiscardResponse> DiscardAsync(Guid deadLetterJobId, Guid operatorUserId, CancellationToken ct = default);
}
