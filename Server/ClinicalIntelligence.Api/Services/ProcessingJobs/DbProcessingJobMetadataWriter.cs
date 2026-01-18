using ClinicalIntelligence.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicalIntelligence.Api.Services.ProcessingJobs;

/// <summary>
/// EF Core-backed implementation of IProcessingJobMetadataWriter.
/// Updates processing_jobs fields consistently with UTC timestamps.
/// </summary>
public sealed class DbProcessingJobMetadataWriter : IProcessingJobMetadataWriter
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<DbProcessingJobMetadataWriter> _logger;

    private const int MaxErrorMessageLength = 500;

    public DbProcessingJobMetadataWriter(
        ApplicationDbContext dbContext,
        ILogger<DbProcessingJobMetadataWriter> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> MarkStartedAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        var job = await _dbContext.ProcessingJobs
            .FirstOrDefaultAsync(j => j.Id == jobId, cancellationToken);

        if (job == null)
        {
            _logger.LogWarning("MarkStartedAsync: ProcessingJob {JobId} not found", jobId);
            return false;
        }

        job.Status = "Processing";
        job.StartedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "ProcessingJob {JobId} marked as started at {StartedAt}",
            jobId, job.StartedAt);

        return true;
    }

    public async Task<bool> MarkCompletedAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        var job = await _dbContext.ProcessingJobs
            .FirstOrDefaultAsync(j => j.Id == jobId, cancellationToken);

        if (job == null)
        {
            _logger.LogWarning("MarkCompletedAsync: ProcessingJob {JobId} not found", jobId);
            return false;
        }

        var completedAt = DateTime.UtcNow;
        job.Status = "Completed";
        job.CompletedAt = completedAt;
        job.ProcessingTimeMs = ComputeProcessingTimeMs(job.StartedAt, completedAt);

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "ProcessingJob {JobId} marked as completed at {CompletedAt}, ProcessingTimeMs={ProcessingTimeMs}",
            jobId, job.CompletedAt, job.ProcessingTimeMs);

        return true;
    }

    public async Task<bool> MarkFailedAsync(
        Guid jobId,
        string errorMessage,
        string? errorDetails = null,
        CancellationToken cancellationToken = default)
    {
        var job = await _dbContext.ProcessingJobs
            .FirstOrDefaultAsync(j => j.Id == jobId, cancellationToken);

        if (job == null)
        {
            _logger.LogWarning("MarkFailedAsync: ProcessingJob {JobId} not found", jobId);
            return false;
        }

        var completedAt = DateTime.UtcNow;
        job.Status = "Failed";
        job.CompletedAt = completedAt;
        job.ProcessingTimeMs = ComputeProcessingTimeMs(job.StartedAt, completedAt);
        job.ErrorMessage = TruncateErrorMessage(errorMessage);
        job.ErrorDetails = errorDetails;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "ProcessingJob {JobId} marked as failed at {CompletedAt}, ErrorMessage={ErrorMessage}",
            jobId, job.CompletedAt, job.ErrorMessage);

        return true;
    }

    public async Task<int> IncrementRetryCountAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        var job = await _dbContext.ProcessingJobs
            .FirstOrDefaultAsync(j => j.Id == jobId, cancellationToken);

        if (job == null)
        {
            _logger.LogWarning("IncrementRetryCountAsync: ProcessingJob {JobId} not found", jobId);
            return -1;
        }

        job.RetryCount++;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "ProcessingJob {JobId} retry count incremented to {RetryCount}",
            jobId, job.RetryCount);

        return job.RetryCount;
    }

    /// <summary>
    /// Computes processing time in milliseconds. Returns null if StartedAt is missing.
    /// </summary>
    private static int? ComputeProcessingTimeMs(DateTime? startedAt, DateTime completedAt)
    {
        if (!startedAt.HasValue)
        {
            return null;
        }

        var duration = completedAt - startedAt.Value;
        
        // Clamp to int.MaxValue to avoid overflow for very long-running jobs
        if (duration.TotalMilliseconds > int.MaxValue)
        {
            return int.MaxValue;
        }

        return (int)Math.Max(0, duration.TotalMilliseconds);
    }

    /// <summary>
    /// Truncates error message to prevent oversized storage.
    /// Avoids storing sensitive data by limiting length.
    /// </summary>
    private static string TruncateErrorMessage(string? errorMessage)
    {
        if (string.IsNullOrEmpty(errorMessage))
        {
            return "An error occurred during processing.";
        }

        if (errorMessage.Length <= MaxErrorMessageLength)
        {
            return errorMessage;
        }

        return errorMessage.Substring(0, MaxErrorMessageLength - 3) + "...";
    }
}
