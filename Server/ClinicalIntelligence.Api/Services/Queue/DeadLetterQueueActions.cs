using ClinicalIntelligence.Api.Contracts;
using ClinicalIntelligence.Api.Contracts.Dlq;
using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Domain.Models;
using ClinicalIntelligence.Api.Services.Security;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ClinicalIntelligence.Api.Services.Queue;

/// <summary>
/// Implementation of DLQ replay/discard operations with concurrency protection and audit logging.
/// </summary>
public class DeadLetterQueueActions : IDeadLetterQueueActions
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMessagePublisher _messagePublisher;
    private readonly IAuditLogWriter _auditLogWriter;
    private readonly ILogger<DeadLetterQueueActions> _logger;

    public DeadLetterQueueActions(
        ApplicationDbContext dbContext,
        IMessagePublisher messagePublisher,
        IAuditLogWriter auditLogWriter,
        ILogger<DeadLetterQueueActions> logger)
    {
        _dbContext = dbContext;
        _messagePublisher = messagePublisher;
        _auditLogWriter = auditLogWriter;
        _logger = logger;
    }

    public async Task<DlqReplayResponse> ReplayAsync(Guid deadLetterJobId, Guid operatorUserId, CancellationToken ct = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);

        try
        {
            // Load DLQ entry with concurrency protection
            var dlqEntry = await _dbContext.DeadLetterJobs
                .FirstOrDefaultAsync(d => d.Id == deadLetterJobId, ct);

            if (dlqEntry == null)
            {
                return new DlqReplayResponse
                {
                    DeadLetterJobId = deadLetterJobId,
                    Success = false,
                    Message = "DLQ entry not found."
                };
            }

            // Validate entry is eligible for replay
            if (dlqEntry.Status == DeadLetterJobStatus.Discarded)
            {
                return new DlqReplayResponse
                {
                    DeadLetterJobId = deadLetterJobId,
                    Status = dlqEntry.Status,
                    ReplayAttempts = dlqEntry.ReplayAttempts,
                    Success = false,
                    Message = "Cannot replay a discarded entry."
                };
            }

            // Idempotency: If already replayed successfully, return success
            if (dlqEntry.Status == DeadLetterJobStatus.Replayed && dlqEntry.ReplayedJobId.HasValue)
            {
                return new DlqReplayResponse
                {
                    DeadLetterJobId = deadLetterJobId,
                    NewJobId = dlqEntry.ReplayedJobId,
                    Status = dlqEntry.Status,
                    ReplayAttempts = dlqEntry.ReplayAttempts,
                    Success = true,
                    Message = "Entry was already replayed successfully."
                };
            }

            // Deserialize original message to create new job
            DocumentProcessingJob? originalJob;
            try
            {
                originalJob = DeserializeOriginalMessage(dlqEntry.OriginalMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize original message for DLQ entry {DeadLetterJobId}", deadLetterJobId);
                
                dlqEntry.ReplayAttempts++;
                dlqEntry.LastReplayError = $"Failed to deserialize original message: {ex.Message}";
                dlqEntry.LastActionAt = DateTime.UtcNow;
                dlqEntry.LastActionByUserId = operatorUserId;
                await _dbContext.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                return new DlqReplayResponse
                {
                    DeadLetterJobId = deadLetterJobId,
                    Status = dlqEntry.Status,
                    ReplayAttempts = dlqEntry.ReplayAttempts,
                    Success = false,
                    Message = "Failed to deserialize original message."
                };
            }

            // Create new job with fresh ID and reset retry count
            var newJobId = Guid.NewGuid();
            var replayJob = originalJob with
            {
                JobId = newJobId,
                RetryCount = 0,
                CreatedAt = DateTime.UtcNow,
                CorrelationId = $"replay:{deadLetterJobId}"
            };

            // Attempt to enqueue the job
            var published = await _messagePublisher.PublishDocumentJobAsync(replayJob, ct);

            if (!published)
            {
                dlqEntry.ReplayAttempts++;
                dlqEntry.LastReplayError = "Failed to publish job to queue.";
                dlqEntry.LastActionAt = DateTime.UtcNow;
                dlqEntry.LastActionByUserId = operatorUserId;
                await _dbContext.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                return new DlqReplayResponse
                {
                    DeadLetterJobId = deadLetterJobId,
                    Status = dlqEntry.Status,
                    ReplayAttempts = dlqEntry.ReplayAttempts,
                    Success = false,
                    Message = "Failed to publish job to queue."
                };
            }

            // Update DLQ entry as replayed
            dlqEntry.Status = DeadLetterJobStatus.Replayed;
            dlqEntry.ReplayAttempts++;
            dlqEntry.ReplayedJobId = newJobId;
            dlqEntry.LastActionAt = DateTime.UtcNow;
            dlqEntry.LastActionByUserId = operatorUserId;
            dlqEntry.LastReplayError = null;

            await _dbContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            // Audit log (best-effort, no PHI)
            _ = _auditLogWriter.WriteAsync(
                actionType: "DLQ_REPLAY",
                userId: operatorUserId,
                sessionId: null,
                resourceType: "DeadLetterJob",
                resourceId: deadLetterJobId,
                ipAddress: null,
                userAgent: null,
                metadata: new { deadLetterJobId, newJobId, replayAttempts = dlqEntry.ReplayAttempts },
                cancellationToken: ct);

            _logger.LogInformation(
                "DLQ entry replayed: DeadLetterJobId={DeadLetterJobId}, NewJobId={NewJobId}, OperatorUserId={OperatorUserId}",
                deadLetterJobId, newJobId, operatorUserId);

            return new DlqReplayResponse
            {
                DeadLetterJobId = deadLetterJobId,
                NewJobId = newJobId,
                Status = dlqEntry.Status,
                ReplayAttempts = dlqEntry.ReplayAttempts,
                Success = true,
                Message = "Job replayed successfully."
            };
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await transaction.RollbackAsync(ct);
            _logger.LogWarning(ex, "Concurrency conflict during replay of DLQ entry {DeadLetterJobId}", deadLetterJobId);

            return new DlqReplayResponse
            {
                DeadLetterJobId = deadLetterJobId,
                Success = false,
                Message = "Concurrent modification detected. Please retry."
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct);
            _logger.LogError(ex, "Failed to replay DLQ entry {DeadLetterJobId}", deadLetterJobId);

            return new DlqReplayResponse
            {
                DeadLetterJobId = deadLetterJobId,
                Success = false,
                Message = "An error occurred during replay."
            };
        }
    }

    public async Task<DlqDiscardResponse> DiscardAsync(Guid deadLetterJobId, Guid operatorUserId, CancellationToken ct = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);

        try
        {
            var dlqEntry = await _dbContext.DeadLetterJobs
                .FirstOrDefaultAsync(d => d.Id == deadLetterJobId, ct);

            if (dlqEntry == null)
            {
                return new DlqDiscardResponse
                {
                    DeadLetterJobId = deadLetterJobId,
                    Success = false,
                    Message = "DLQ entry not found."
                };
            }

            // Idempotency: If already discarded, return success
            if (dlqEntry.Status == DeadLetterJobStatus.Discarded)
            {
                return new DlqDiscardResponse
                {
                    DeadLetterJobId = deadLetterJobId,
                    Status = dlqEntry.Status,
                    Success = true,
                    Message = "Entry was already discarded."
                };
            }

            // Update DLQ entry as discarded
            dlqEntry.Status = DeadLetterJobStatus.Discarded;
            dlqEntry.LastActionAt = DateTime.UtcNow;
            dlqEntry.LastActionByUserId = operatorUserId;

            await _dbContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            // Audit log (best-effort, no PHI)
            _ = _auditLogWriter.WriteAsync(
                actionType: "DLQ_DISCARD",
                userId: operatorUserId,
                sessionId: null,
                resourceType: "DeadLetterJob",
                resourceId: deadLetterJobId,
                ipAddress: null,
                userAgent: null,
                metadata: new { deadLetterJobId },
                cancellationToken: ct);

            _logger.LogInformation(
                "DLQ entry discarded: DeadLetterJobId={DeadLetterJobId}, OperatorUserId={OperatorUserId}",
                deadLetterJobId, operatorUserId);

            return new DlqDiscardResponse
            {
                DeadLetterJobId = deadLetterJobId,
                Status = dlqEntry.Status,
                Success = true,
                Message = "Entry discarded successfully."
            };
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await transaction.RollbackAsync(ct);
            _logger.LogWarning(ex, "Concurrency conflict during discard of DLQ entry {DeadLetterJobId}", deadLetterJobId);

            return new DlqDiscardResponse
            {
                DeadLetterJobId = deadLetterJobId,
                Success = false,
                Message = "Concurrent modification detected. Please retry."
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct);
            _logger.LogError(ex, "Failed to discard DLQ entry {DeadLetterJobId}", deadLetterJobId);

            return new DlqDiscardResponse
            {
                DeadLetterJobId = deadLetterJobId,
                Success = false,
                Message = "An error occurred during discard."
            };
        }
    }

    private static DocumentProcessingJob DeserializeOriginalMessage(string originalMessage)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        var job = JsonSerializer.Deserialize<DocumentProcessingJob>(originalMessage, options);
        
        if (job == null)
        {
            throw new InvalidOperationException("Deserialized job is null.");
        }

        return job;
    }
}
