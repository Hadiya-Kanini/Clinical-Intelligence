using ClinicalIntelligence.Api.Contracts;
using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ClinicalIntelligence.Api.Services.Queue;

/// <summary>
/// Database-backed implementation of IDeadLetterQueueWriter.
/// Persists DLQ entries and updates ProcessingJob status atomically within a transaction.
/// </summary>
public class DbDeadLetterQueueWriter : IDeadLetterQueueWriter
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<DbDeadLetterQueueWriter> _logger;

    public DbDeadLetterQueueWriter(
        ApplicationDbContext dbContext,
        ILogger<DbDeadLetterQueueWriter> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Guid?> WriteAsync(
        DocumentProcessingJob job,
        string reason,
        string? errorMessage,
        string? errorDetails,
        string? retryHistory,
        CancellationToken ct = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);
        
        try
        {
            // Serialize original message (exclude sensitive data patterns)
            var originalMessage = SerializeOriginalMessage(job);

            // Create DLQ entry
            var deadLetterJob = new DeadLetterJob
            {
                Id = Guid.NewGuid(),
                ProcessingJobId = job.JobId,
                DocumentId = job.DocumentId,
                OriginalMessage = originalMessage,
                MessageSchemaVersion = "1.0",
                ErrorMessage = TruncateIfNeeded(errorMessage, 4000),
                ErrorDetails = errorDetails,
                RetryHistory = retryHistory,
                RetryCount = job.RetryCount,
                DeadLetterReason = TruncateIfNeeded(reason, 200) ?? "Unknown",
                DeadLetteredAt = DateTime.UtcNow,
                Status = DeadLetterJobStatus.Pending,
                ReplayAttempts = 0
            };

            _dbContext.DeadLetterJobs.Add(deadLetterJob);

            // Update associated ProcessingJob status to DeadLettered
            var processingJob = await _dbContext.ProcessingJobs
                .FirstOrDefaultAsync(pj => pj.Id == job.JobId, ct);

            if (processingJob != null)
            {
                processingJob.Status = "DeadLettered";
                processingJob.ErrorMessage = TruncateIfNeeded(errorMessage, 4000);
                processingJob.ErrorDetails = errorDetails;
                processingJob.CompletedAt = DateTime.UtcNow;
            }
            else
            {
                _logger.LogWarning(
                    "ProcessingJob {JobId} not found when writing to DLQ. DLQ entry created without ProcessingJob update.",
                    job.JobId);
            }

            await _dbContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            _logger.LogInformation(
                "Job moved to DLQ: DeadLetterJobId={DeadLetterJobId}, ProcessingJobId={ProcessingJobId}, DocumentId={DocumentId}, Reason={Reason}",
                deadLetterJob.Id, job.JobId, job.DocumentId, reason);

            return deadLetterJob.Id;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct);
            
            _logger.LogError(ex,
                "Failed to write job to DLQ: ProcessingJobId={ProcessingJobId}, DocumentId={DocumentId}",
                job.JobId, job.DocumentId);
            
            return null;
        }
    }

    private static string SerializeOriginalMessage(DocumentProcessingJob job)
    {
        // Serialize job without potentially sensitive storage paths in logs
        var safeJob = new
        {
            job.JobId,
            job.DocumentId,
            job.PatientId,
            job.UploadedByUserId,
            job.OriginalName,
            job.MimeType,
            StoragePath = "[REDACTED]", // Redact storage path for security
            job.SizeBytes,
            job.CreatedAt,
            job.RetryCount,
            job.CorrelationId
        };

        return JsonSerializer.Serialize(safeJob, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });
    }

    private static string? TruncateIfNeeded(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
