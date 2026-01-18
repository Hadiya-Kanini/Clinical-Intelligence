using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicalIntelligence.Api.Services.ProcessingJobs;

/// <summary>
/// EF Core implementation for recording processing job failures.
/// Updates ProcessingJob with failure status and error details.
/// </summary>
public sealed class ProcessingJobFailureRecorder : IProcessingJobFailureRecorder
{
    /// <summary>
    /// Status value for validation failures.
    /// Used consistently across the API domain for entity validation failures.
    /// </summary>
    public const string ValidationFailedStatus = "Validation_Failed";

    private readonly IProcessingJobFailureDbContext _dbContext;
    private readonly ILogger<ProcessingJobFailureRecorder> _logger;

    public ProcessingJobFailureRecorder(
        IProcessingJobFailureDbContext dbContext,
        ILogger<ProcessingJobFailureRecorder> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<bool> RecordValidationFailureAsync(
        Guid processingJobId,
        string errorMessage,
        string? errorDetailsJson,
        CancellationToken cancellationToken = default)
    {
        if (processingJobId == Guid.Empty)
        {
            throw new ArgumentException("Processing job ID cannot be empty.", nameof(processingJobId));
        }

        if (string.IsNullOrWhiteSpace(errorMessage))
        {
            throw new ArgumentException("Error message cannot be empty.", nameof(errorMessage));
        }

        var job = await _dbContext.ProcessingJobs
            .FirstOrDefaultAsync(j => j.Id == processingJobId, cancellationToken);

        if (job == null)
        {
            _logger.LogWarning(
                "Processing job {JobId} not found when recording validation failure",
                processingJobId);
            return false;
        }

        job.Status = ValidationFailedStatus;
        job.ErrorMessage = errorMessage;
        job.ErrorDetails = errorDetailsJson;
        job.CompletedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Recorded validation failure for processing job {JobId}: {ErrorMessage}",
            processingJobId,
            errorMessage);

        return true;
    }
}
