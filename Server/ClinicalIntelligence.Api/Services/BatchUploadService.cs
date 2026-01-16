using ClinicalIntelligence.Api.Contracts;
using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Domain.Models;

namespace ClinicalIntelligence.Api.Services;

/// <summary>
/// Service interface for batch document upload operations.
/// </summary>
public interface IBatchUploadService
{
    /// <summary>
    /// Processes a batch of files for upload, enforcing the 10-file limit.
    /// </summary>
    Task<BatchUploadResponse> ProcessBatchAsync(
        IFormFileCollection files,
        Guid patientId,
        Guid uploadedByUserId,
        CancellationToken ct);
}

/// <summary>
/// Service for batch document upload with 10-file limit enforcement (FR-014).
/// Orchestrates validation and storage for multiple files in a single batch.
/// </summary>
public class BatchUploadService : IBatchUploadService
{
    private const int MaxFilesPerBatch = 10;

    private readonly ApplicationDbContext _dbContext;
    private readonly IDocumentService _documentService;
    private readonly ILogger<BatchUploadService> _logger;

    public BatchUploadService(
        ApplicationDbContext dbContext,
        IDocumentService documentService,
        ILogger<BatchUploadService> logger)
    {
        _dbContext = dbContext;
        _documentService = documentService;
        _logger = logger;
    }

    public async Task<BatchUploadResponse> ProcessBatchAsync(
        IFormFileCollection files,
        Guid patientId,
        Guid uploadedByUserId,
        CancellationToken ct)
    {
        var batchId = Guid.NewGuid();
        var fileResults = new List<FileUploadResult>();
        var totalReceived = files.Count;
        var batchLimitExceeded = totalReceived > MaxFilesPerBatch;

        _logger.LogInformation(
            "Processing batch upload: BatchId={BatchId}, TotalFiles={TotalFiles}, LimitExceeded={LimitExceeded}",
            batchId, totalReceived, batchLimitExceeded);

        // Create DocumentBatch entity
        var batch = new DocumentBatch
        {
            Id = batchId,
            PatientId = patientId,
            UploadedByUserId = uploadedByUserId,
            UploadedAt = DateTime.UtcNow
        };

        _dbContext.DocumentBatches.Add(batch);
        await _dbContext.SaveChangesAsync(ct);

        // Process first 10 files (or all if under limit)
        var filesToProcess = files.Take(MaxFilesPerBatch);
        var acceptedCount = 0;
        var rejectedCount = 0;

        foreach (var file in filesToProcess)
        {
            try
            {
                var result = await _documentService.ValidateAndAcknowledgeAsync(
                    file,
                    patientId,
                    uploadedByUserId,
                    batchId,
                    ct);

                var fileResult = new FileUploadResult
                {
                    FileName = file.FileName,
                    DocumentId = result.IsValid ? result.DocumentId : null,
                    IsAccepted = result.IsValid,
                    Status = result.Status,
                    ValidationErrors = result.ValidationErrors,
                    RejectionReason = result.IsValid ? null : string.Join("; ", result.ValidationErrors)
                };

                fileResults.Add(fileResult);

                if (result.IsValid)
                {
                    acceptedCount++;
                }
                else
                {
                    rejectedCount++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing file {FileName} in batch {BatchId}", file.FileName, batchId);

                fileResults.Add(new FileUploadResult
                {
                    FileName = file.FileName,
                    IsAccepted = false,
                    Status = "ProcessingError",
                    ValidationErrors = new List<string> { "An error occurred while processing this file." },
                    RejectionReason = "Processing error"
                });

                rejectedCount++;
            }
        }

        // Mark excess files as rejected due to batch limit
        var excessFiles = files.Skip(MaxFilesPerBatch);
        foreach (var file in excessFiles)
        {
            fileResults.Add(new FileUploadResult
            {
                FileName = file.FileName,
                IsAccepted = false,
                Status = "BatchLimitExceeded",
                ValidationErrors = new List<string> { "File exceeds batch limit of 10 files per upload." },
                RejectionReason = "File exceeds batch limit of 10 files"
            });

            rejectedCount++;
        }

        var acknowledgedAt = DateTime.UtcNow;

        // Generate warning message if limit exceeded
        string? batchLimitWarning = null;
        if (batchLimitExceeded)
        {
            batchLimitWarning = GenerateBatchLimitWarning(totalReceived, MaxFilesPerBatch);

            _logger.LogWarning(
                "Batch upload limit exceeded: BatchId={BatchId}, Received={Received}, Accepted={Accepted}, Rejected={Rejected}",
                batchId, totalReceived, acceptedCount, rejectedCount);
        }

        _logger.LogInformation(
            "Batch upload completed: BatchId={BatchId}, Accepted={Accepted}, Rejected={Rejected}",
            batchId, acceptedCount, rejectedCount);

        return new BatchUploadResponse
        {
            BatchId = batchId,
            PatientId = patientId,
            TotalFilesReceived = totalReceived,
            FilesAccepted = acceptedCount,
            FilesRejected = rejectedCount,
            BatchLimitExceeded = batchLimitExceeded,
            BatchLimitWarning = batchLimitWarning,
            FileResults = fileResults,
            AcknowledgedAt = acknowledgedAt
        };
    }

    private static string GenerateBatchLimitWarning(int totalReceived, int accepted)
    {
        var rejected = totalReceived - accepted;
        return $"Batch limit of {MaxFilesPerBatch} files exceeded. {accepted} files were accepted, " +
               $"{rejected} files were not processed. Please upload remaining files in a separate batch.";
    }
}
