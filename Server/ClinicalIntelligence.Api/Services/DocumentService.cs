using ClinicalIntelligence.Api.Contracts;
using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Domain.Models;
using ClinicalIntelligence.Api.Services.Security;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace ClinicalIntelligence.Api.Services;

/// <summary>
/// Service for document upload validation and processing.
/// Implements fast validation for acknowledgment within 5 seconds (NFR-001).
/// </summary>
public interface IDocumentService
{
    Task<UploadAcknowledgmentResponse> ValidateAndAcknowledgeAsync(
        IFormFile file,
        Guid patientId,
        Guid uploadedByUserId,
        CancellationToken cancellationToken = default);

    Task<UploadAcknowledgmentResponse> ValidateAndAcknowledgeAsync(
        IFormFile file,
        Guid patientId,
        Guid uploadedByUserId,
        Guid? batchId,
        CancellationToken cancellationToken = default);
}

public class DocumentService : IDocumentService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<DocumentService> _logger;
    private readonly IDocumentIntegrityValidator? _integrityValidator;
    private readonly IMalwareScanner? _malwareScanner;
    private readonly IAuditLogWriter? _auditLogWriter;
    private readonly IDocumentStorageService? _storageService;

    private const long MaxFileSizeBytes = 50 * 1024 * 1024; // 50MB = 52,428,800 bytes
    
    private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    };
    
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf",
        ".docx"
    };

    private static readonly Dictionary<string, string[]> MimeToExtensionMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "application/pdf", new[] { ".pdf" } },
        { "application/vnd.openxmlformats-officedocument.wordprocessingml.document", new[] { ".docx" } }
    };

    private static readonly HashSet<string> ExecutableExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".exe", ".bat", ".cmd", ".com", ".msi", ".scr", ".pif", ".vbs", ".js", ".jar",
        ".ps1", ".sh", ".dll", ".sys", ".drv", ".ocx", ".cpl", ".inf", ".reg"
    };

    public DocumentService(
        ApplicationDbContext dbContext, 
        ILogger<DocumentService> logger, 
        IDocumentIntegrityValidator? integrityValidator = null,
        IMalwareScanner? malwareScanner = null,
        IAuditLogWriter? auditLogWriter = null,
        IDocumentStorageService? storageService = null)
    {
        _dbContext = dbContext;
        _logger = logger;
        _integrityValidator = integrityValidator;
        _malwareScanner = malwareScanner;
        _auditLogWriter = auditLogWriter;
        _storageService = storageService;
    }

    public Task<UploadAcknowledgmentResponse> ValidateAndAcknowledgeAsync(
        IFormFile file,
        Guid patientId,
        Guid uploadedByUserId,
        CancellationToken cancellationToken = default)
    {
        return ValidateAndAcknowledgeAsync(file, patientId, uploadedByUserId, null, cancellationToken);
    }

    public async Task<UploadAcknowledgmentResponse> ValidateAndAcknowledgeAsync(
        IFormFile file,
        Guid patientId,
        Guid uploadedByUserId,
        Guid? batchId,
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        var documentId = Guid.NewGuid();
        var validationErrors = new List<string>();

        var fileName = file.FileName;
        var fileSize = file.Length;
        var extension = Path.GetExtension(fileName);
        var contentType = file.ContentType;
        
        FileValidationErrorCode? errorCode = null;
        string? errorType = null;

        // 1. Check for empty file (FR-015f)
        if (fileSize == 0)
        {
            validationErrors.Add("Empty files cannot be processed. Please upload a file with content.");
            errorCode = FileValidationErrorCode.FileEmpty;
            errorType = "file_empty";
        }

        // 2. Check file size (FR-016) - exactly 50MB is allowed, 50MB+1 byte is rejected
        if (errorCode == null && fileSize > MaxFileSizeBytes)
        {
            validationErrors.Add($"File size ({fileSize / (1024 * 1024)}MB) exceeds maximum allowed size of 50MB.");
            errorCode = FileValidationErrorCode.FileTooLarge;
            errorType = "file_too_large";
        }

        // 3. Validate extension (FR-015a)
        if (errorCode == null && !AllowedExtensions.Contains(extension))
        {
            validationErrors.Add($"Unsupported file type '{extension}'. Only PDF and DOCX files are allowed.");
            errorCode = FileValidationErrorCode.InvalidExtension;
            errorType = "invalid_extension";
        }

        // 4. Check for double extensions (Security)
        if (errorCode == null && HasDoubleExtension(fileName))
        {
            validationErrors.Add($"File '{fileName}' has a suspicious double extension. Please rename the file and try again.");
            errorCode = FileValidationErrorCode.DoubleExtension;
            errorType = "double_extension";
        }

        // 5. Validate MIME type (FR-015b)
        if (errorCode == null && !AllowedMimeTypes.Contains(contentType))
        {
            validationErrors.Add($"Invalid content type '{contentType}'. Only PDF and DOCX files are allowed.");
            errorCode = FileValidationErrorCode.InvalidMimeType;
            errorType = "invalid_mime_type";
        }

        // 6. Validate MIME-extension match (FR-015b)
        if (errorCode == null && !IsMimeExtensionMatch(contentType, extension))
        {
            validationErrors.Add($"Content type '{contentType}' does not match file extension '{extension}'.");
            errorCode = FileValidationErrorCode.MimeExtensionMismatch;
            errorType = "mime_extension_mismatch";
        }

        // 7. Document integrity validation (password protection, corruption, structure)
        if (errorCode == null && _integrityValidator != null)
        {
            using var stream = file.OpenReadStream();
            var integrityResult = await _integrityValidator.ValidateAsync(stream, fileName, contentType, cancellationToken);
            
            if (!integrityResult.IsValid)
            {
                validationErrors.Add(integrityResult.ErrorMessage ?? "Document validation failed.");
                errorCode = integrityResult.ErrorCode;
                errorType = integrityResult.ErrorCode switch
                {
                    FileValidationErrorCode.PasswordProtected => "password_protected",
                    FileValidationErrorCode.FileCorrupted => "file_corrupted",
                    FileValidationErrorCode.InvalidStructure => "invalid_structure",
                    FileValidationErrorCode.FileEmpty => "file_empty",
                    _ => "validation_failed"
                };
            }
        }
        else if (errorCode == null && extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            // Fallback to basic PDF header validation if integrity validator not available
            var isValidPdf = await ValidatePdfHeaderAsync(file, cancellationToken);
            if (!isValidPdf)
            {
                validationErrors.Add("File appears to be corrupted or is not a valid PDF.");
                errorCode = FileValidationErrorCode.FileCorrupted;
                errorType = "file_corrupted";
            }
        }

        // 8. Malware scanning (TR-018, FR-015g)
        if (errorCode == null && _malwareScanner != null && _malwareScanner.IsAvailable)
        {
            using var scanStream = file.OpenReadStream();
            var scanResult = await _malwareScanner.ScanAsync(scanStream, fileName, cancellationToken);

            if (scanResult.IsMalwareDetected)
            {
                validationErrors.Add($"Security threat detected: {scanResult.ThreatName ?? "Malware"}. File has been quarantined.");
                errorCode = FileValidationErrorCode.MalwareDetected;
                errorType = "malware_detected";

                // Log malware detection audit event
                await LogMalwareDetectionAsync(fileName, fileSize, scanResult, uploadedByUserId, cancellationToken);
            }
            else if (scanResult.TimedOut)
            {
                validationErrors.Add("Security scan timed out. Please try again or contact support.");
                errorCode = FileValidationErrorCode.ScanTimeout;
                errorType = "scan_timeout";
            }
            else if (!scanResult.ScanCompleted)
            {
                _logger.LogWarning("Malware scan failed for {FileName}: {Error}", fileName, scanResult.ErrorMessage);
                // Don't block upload if scanner fails - log and continue
            }
        }

        var isValid = validationErrors.Count == 0;
        var status = isValid ? "Accepted" : "ValidationFailed";

        if (isValid)
        {
            // Store file to file system if storage service is available
            string storagePath = $"pending/{documentId}{extension}";
            
            if (_storageService != null)
            {
                using var stream = file.OpenReadStream();
                var storageResult = await _storageService.StoreAsync(
                    stream,
                    fileName,
                    patientId,
                    documentId,
                    cancellationToken);

                if (!storageResult.IsSuccess)
                {
                    validationErrors.Add($"Failed to store file: {storageResult.ErrorMessage}");
                    isValid = false;
                    status = "StorageFailed";
                    errorCode = FileValidationErrorCode.StorageFailed;
                    errorType = "storage_failed";
                }
                else
                {
                    storagePath = storageResult.StoragePath;
                }
            }

            if (isValid)
            {
                var document = new Document
                {
                    Id = documentId,
                    PatientId = patientId,
                    DocumentBatchId = batchId,
                    UploadedByUserId = uploadedByUserId,
                    OriginalName = fileName,
                    MimeType = contentType,
                    SizeBytes = (int)fileSize,
                    StoragePath = storagePath,
                    Status = "Pending",
                    UploadedAt = DateTime.UtcNow
                };

                _dbContext.Documents.Add(document);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        var acknowledgedAt = DateTime.UtcNow;
        var responseTimeMs = (acknowledgedAt - startTime).TotalMilliseconds;

        _logger.LogInformation(
            "Document upload acknowledgment: DocumentId={DocumentId}, FileName={FileName}, FileSize={FileSize}, " +
            "IsValid={IsValid}, Status={Status}, ResponseTimeMs={ResponseTimeMs}",
            documentId, fileName, fileSize, isValid, status, responseTimeMs);

        if (responseTimeMs > 5000)
        {
            _logger.LogWarning(
                "Document upload acknowledgment exceeded 5 second SLA: DocumentId={DocumentId}, ResponseTimeMs={ResponseTimeMs}",
                documentId, responseTimeMs);
        }

        return new UploadAcknowledgmentResponse
        {
            DocumentId = documentId,
            FileName = fileName,
            FileSize = fileSize,
            Status = status,
            IsValid = isValid,
            ValidationErrors = validationErrors,
            AcknowledgedAt = acknowledgedAt,
            ErrorCode = errorCode,
            ErrorType = errorType
        };
    }

    private static async Task<bool> ValidatePdfHeaderAsync(IFormFile file, CancellationToken cancellationToken)
    {
        try
        {
            using var stream = file.OpenReadStream();
            var buffer = new byte[5];
            var bytesRead = await stream.ReadAsync(buffer, 0, 5, cancellationToken);

            if (bytesRead < 5)
            {
                return false;
            }

            return buffer[0] == 0x25 && // %
                   buffer[1] == 0x50 && // P
                   buffer[2] == 0x44 && // D
                   buffer[3] == 0x46 && // F
                   buffer[4] == 0x2D;   // -
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Detects double extensions that may indicate a security attack (e.g., document.pdf.exe).
    /// Allows legitimate filenames with periods (e.g., my.report.pdf).
    /// </summary>
    private bool HasDoubleExtension(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        var nameWithoutPath = Path.GetFileName(fileName);
        var lastExtension = Path.GetExtension(nameWithoutPath);
        
        if (string.IsNullOrEmpty(lastExtension))
            return false;

        var nameWithoutLastExtension = Path.GetFileNameWithoutExtension(nameWithoutPath);
        var secondExtension = Path.GetExtension(nameWithoutLastExtension);

        if (string.IsNullOrEmpty(secondExtension))
            return false;

        // Check if second extension is an executable type (security concern)
        if (ExecutableExtensions.Contains(lastExtension))
        {
            _logger.LogWarning(
                "Double extension attack detected: FileName={FileName}, LastExtension={LastExtension}",
                fileName, lastExtension);
            return true;
        }

        // Check if the second extension is a known document type followed by another document type
        // e.g., file.pdf.pdf is suspicious
        if (AllowedExtensions.Contains(secondExtension) && AllowedExtensions.Contains(lastExtension))
        {
            _logger.LogWarning(
                "Suspicious double document extension: FileName={FileName}",
                fileName);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Validates that the MIME type matches the file extension.
    /// Prevents MIME type spoofing attacks.
    /// </summary>
    private static bool IsMimeExtensionMatch(string mimeType, string extension)
    {
        if (string.IsNullOrWhiteSpace(mimeType) || string.IsNullOrWhiteSpace(extension))
            return false;

        if (!MimeToExtensionMap.TryGetValue(mimeType, out var allowedExtensions))
            return false;

        return allowedExtensions.Any(ext => ext.Equals(extension, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Logs malware detection event for security audit trail.
    /// </summary>
    private async Task LogMalwareDetectionAsync(
        string fileName, 
        long fileSize, 
        MalwareScanResult scanResult, 
        Guid userId, 
        CancellationToken ct)
    {
        if (_auditLogWriter == null)
        {
            _logger.LogWarning(
                "MALWARE_DETECTED: FileName={FileName}, ThreatName={ThreatName}, ThreatType={ThreatType}, UserId={UserId}",
                fileName, scanResult.ThreatName, scanResult.ThreatType, userId);
            return;
        }

        try
        {
            await _auditLogWriter.WriteAsync(
                actionType: "MALWARE_DETECTED",
                userId: userId,
                sessionId: null,
                resourceType: "Document",
                resourceId: null,
                ipAddress: null,
                userAgent: null,
                metadata: new
                {
                    fileName,
                    fileSize,
                    threatName = scanResult.ThreatName,
                    threatType = scanResult.ThreatType,
                    scannerName = scanResult.ScannerName,
                    scanDurationMs = scanResult.ScanDuration.TotalMilliseconds
                },
                cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to log MALWARE_DETECTED audit event for {FileName}", fileName);
        }
    }
}
