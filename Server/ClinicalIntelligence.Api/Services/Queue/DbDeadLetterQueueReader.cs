using ClinicalIntelligence.Api.Contracts.Dlq;
using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicalIntelligence.Api.Services.Queue;

/// <summary>
/// EF Core implementation for DLQ queries with pagination, filtering, and projections.
/// </summary>
public class DbDeadLetterQueueReader : IDeadLetterQueueReader
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<DbDeadLetterQueueReader> _logger;
    
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 20;

    public DbDeadLetterQueueReader(
        ApplicationDbContext dbContext,
        ILogger<DbDeadLetterQueueReader> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<DlqListResponse> GetListAsync(DlqListQuery query, CancellationToken ct = default)
    {
        // Normalize pagination parameters
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);

        // Build query with filters
        var baseQuery = _dbContext.DeadLetterJobs.AsNoTracking();

        if (query.DocumentId.HasValue)
        {
            baseQuery = baseQuery.Where(d => d.DocumentId == query.DocumentId.Value);
        }

        if (query.ProcessingJobId.HasValue)
        {
            baseQuery = baseQuery.Where(d => d.ProcessingJobId == query.ProcessingJobId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            baseQuery = baseQuery.Where(d => d.Status == query.Status);
        }

        if (query.FromDate.HasValue)
        {
            baseQuery = baseQuery.Where(d => d.DeadLetteredAt >= query.FromDate.Value);
        }

        if (query.ToDate.HasValue)
        {
            baseQuery = baseQuery.Where(d => d.DeadLetteredAt <= query.ToDate.Value);
        }

        // Get total count for pagination
        var totalItems = await baseQuery.CountAsync(ct);

        // Apply stable sorting: DeadLetteredAt DESC, Id as tie-breaker
        var items = await baseQuery
            .OrderByDescending(d => d.DeadLetteredAt)
            .ThenBy(d => d.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new DlqItemSummary
            {
                Id = d.Id,
                ProcessingJobId = d.ProcessingJobId,
                DocumentId = d.DocumentId,
                ErrorMessage = d.ErrorMessage,
                RetryCount = d.RetryCount,
                DeadLetterReason = d.DeadLetterReason,
                DeadLetteredAt = d.DeadLetteredAt,
                Status = d.Status,
                LastActionAt = d.LastActionAt,
                ReplayAttempts = d.ReplayAttempts
            })
            .ToListAsync(ct);

        var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        return new DlqListResponse
        {
            Items = items,
            Pagination = new PaginationMetadata
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages,
                HasNextPage = page < totalPages,
                HasPreviousPage = page > 1
            }
        };
    }

    public async Task<DlqItemResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _dbContext.DeadLetterJobs
            .AsNoTracking()
            .Where(d => d.Id == id)
            .Select(d => new DlqItemResponse
            {
                Id = d.Id,
                ProcessingJobId = d.ProcessingJobId,
                DocumentId = d.DocumentId,
                OriginalMessage = d.OriginalMessage,
                MessageSchemaVersion = d.MessageSchemaVersion,
                ErrorMessage = d.ErrorMessage,
                ErrorDetails = d.ErrorDetails,
                RetryHistory = d.RetryHistory,
                RetryCount = d.RetryCount,
                DeadLetterReason = d.DeadLetterReason,
                DeadLetteredAt = d.DeadLetteredAt,
                Status = d.Status,
                LastActionAt = d.LastActionAt,
                LastActionByUserId = d.LastActionByUserId,
                ReplayAttempts = d.ReplayAttempts,
                LastReplayError = d.LastReplayError,
                ReplayedJobId = d.ReplayedJobId
            })
            .FirstOrDefaultAsync(ct);

        return entity;
    }

    public async Task<DlqMetricsResponse> GetMetricsAsync(CancellationToken ct = default)
    {
        // Use efficient count queries with indexed columns
        var totalCount = await _dbContext.DeadLetterJobs.CountAsync(ct);
        
        var pendingCount = await _dbContext.DeadLetterJobs
            .Where(d => d.Status == DeadLetterJobStatus.Pending)
            .CountAsync(ct);
        
        var replayedCount = await _dbContext.DeadLetterJobs
            .Where(d => d.Status == DeadLetterJobStatus.Replayed)
            .CountAsync(ct);
        
        var discardedCount = await _dbContext.DeadLetterJobs
            .Where(d => d.Status == DeadLetterJobStatus.Discarded)
            .CountAsync(ct);

        // Get oldest pending entry (uses indexed column)
        var oldestPending = await _dbContext.DeadLetterJobs
            .Where(d => d.Status == DeadLetterJobStatus.Pending)
            .OrderBy(d => d.DeadLetteredAt)
            .Select(d => (DateTime?)d.DeadLetteredAt)
            .FirstOrDefaultAsync(ct);

        long? oldestAgeSeconds = null;
        if (oldestPending.HasValue)
        {
            oldestAgeSeconds = (long)(DateTime.UtcNow - oldestPending.Value).TotalSeconds;
        }

        return new DlqMetricsResponse
        {
            TotalCount = totalCount,
            PendingCount = pendingCount,
            ReplayedCount = replayedCount,
            DiscardedCount = discardedCount,
            OldestPendingAgeSeconds = oldestAgeSeconds,
            OldestPendingAt = oldestPending,
            HealthStatus = DetermineHealthStatus(pendingCount, oldestAgeSeconds),
            Timestamp = DateTime.UtcNow
        };
    }

    private static string DetermineHealthStatus(int pendingCount, long? oldestAgeSeconds)
    {
        // Default thresholds (can be made configurable)
        const int warningThreshold = 100;
        const int criticalThreshold = 500;
        const long ageWarningSeconds = 3600; // 1 hour
        const long ageCriticalSeconds = 86400; // 24 hours

        if (pendingCount >= criticalThreshold || (oldestAgeSeconds.HasValue && oldestAgeSeconds.Value >= ageCriticalSeconds))
        {
            return "Unhealthy";
        }

        if (pendingCount >= warningThreshold || (oldestAgeSeconds.HasValue && oldestAgeSeconds.Value >= ageWarningSeconds))
        {
            return "Degraded";
        }

        return "Healthy";
    }
}
