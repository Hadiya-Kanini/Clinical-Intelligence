using ClinicalIntelligence.Api.Configuration;
using Microsoft.Extensions.Options;

namespace ClinicalIntelligence.Api.Services;

/// <summary>
/// Local file system implementation of document storage.
/// Stores documents following pattern: {tenant_id}/{patient_id}/{document_id}/original.{ext}
/// </summary>
public class LocalFileStorageService : IDocumentStorageService
{
    private readonly DocumentStorageOptions _options;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(
        DocumentStorageOptions options,
        ILogger<LocalFileStorageService> logger)
    {
        _options = options;
        _logger = logger;

        // Ensure base directories exist on startup
        EnsureDirectoryExists(_options.BasePath);
        EnsureDirectoryExists(_options.TempPath);
    }

    public async Task<DocumentStorageResult> StoreAsync(
        Stream fileStream,
        string fileName,
        Guid patientId,
        Guid documentId,
        CancellationToken ct)
    {
        try
        {
            // Build storage path: {tenant_id}/{patient_id}/{document_id}/original.{ext}
            var extension = Path.GetExtension(fileName);
            var relativePath = BuildStoragePath(patientId, documentId, extension);
            var absolutePath = Path.GetFullPath(Path.Combine(_options.BasePath, relativePath));

            // Security: Ensure path is within base path (prevent directory traversal)
            var normalizedBasePath = Path.GetFullPath(_options.BasePath);
            if (!absolutePath.StartsWith(normalizedBasePath, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogError("Path traversal attempt detected: {Path}", relativePath);
                return new DocumentStorageResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Invalid storage path"
                };
            }

            // Ensure directory exists
            var directory = Path.GetDirectoryName(absolutePath);
            if (!string.IsNullOrEmpty(directory))
            {
                EnsureDirectoryExists(directory);
            }

            // Write file with async I/O
            await using var fileStreamOut = new FileStream(
                absolutePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                useAsync: true);

            await fileStream.CopyToAsync(fileStreamOut, ct);
            await fileStreamOut.FlushAsync(ct);

            _logger.LogInformation(
                "Document stored: DocumentId={DocumentId}, Path={Path}, Bytes={Bytes}",
                documentId, relativePath, fileStreamOut.Length);

            return new DocumentStorageResult
            {
                IsSuccess = true,
                StoragePath = relativePath,
                AbsolutePath = absolutePath,
                BytesWritten = fileStreamOut.Length
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store document: DocumentId={DocumentId}, FileName={FileName}", documentId, fileName);
            return new DocumentStorageResult
            {
                IsSuccess = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public Task<Stream?> RetrieveAsync(string storagePath, CancellationToken ct)
    {
        try
        {
            var absolutePath = Path.GetFullPath(Path.Combine(_options.BasePath, storagePath));

            // Security: Ensure path is within base path
            var normalizedBasePath = Path.GetFullPath(_options.BasePath);
            if (!absolutePath.StartsWith(normalizedBasePath, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Path traversal attempt in retrieve: {Path}", storagePath);
                return Task.FromResult<Stream?>(null);
            }

            if (!File.Exists(absolutePath))
            {
                _logger.LogWarning("Document not found: Path={Path}", storagePath);
                return Task.FromResult<Stream?>(null);
            }

            Stream stream = new FileStream(
                absolutePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 81920,
                useAsync: true);

            return Task.FromResult<Stream?>(stream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve document: Path={Path}", storagePath);
            return Task.FromResult<Stream?>(null);
        }
    }

    public Task<bool> DeleteAsync(string storagePath, CancellationToken ct)
    {
        try
        {
            var absolutePath = Path.GetFullPath(Path.Combine(_options.BasePath, storagePath));

            // Security: Ensure path is within base path
            var normalizedBasePath = Path.GetFullPath(_options.BasePath);
            if (!absolutePath.StartsWith(normalizedBasePath, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Path traversal attempt in delete: {Path}", storagePath);
                return Task.FromResult(false);
            }

            if (!File.Exists(absolutePath))
            {
                return Task.FromResult(false);
            }

            File.Delete(absolutePath);

            // Clean up empty parent directories
            var directory = Path.GetDirectoryName(absolutePath);
            if (!string.IsNullOrEmpty(directory))
            {
                CleanupEmptyDirectories(directory);
            }

            _logger.LogInformation("Document deleted: Path={Path}", storagePath);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete document: Path={Path}", storagePath);
            return Task.FromResult(false);
        }
    }

    public Task<bool> ExistsAsync(string storagePath, CancellationToken ct)
    {
        try
        {
            var absolutePath = Path.GetFullPath(Path.Combine(_options.BasePath, storagePath));

            // Security: Ensure path is within base path
            var normalizedBasePath = Path.GetFullPath(_options.BasePath);
            if (!absolutePath.StartsWith(normalizedBasePath, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(File.Exists(absolutePath));
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    private string BuildStoragePath(Guid patientId, Guid documentId, string extension)
    {
        // Pattern: {tenant_id}/{patient_id}/{document_id}/original.{ext}
        return Path.Combine(
            _options.DefaultTenantId,
            patientId.ToString(),
            documentId.ToString(),
            $"original{extension}");
    }

    private static void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    private void CleanupEmptyDirectories(string directory)
    {
        try
        {
            var normalizedBasePath = Path.GetFullPath(_options.BasePath);
            var normalizedDirectory = Path.GetFullPath(directory);

            // Don't delete beyond base path
            if (!normalizedDirectory.StartsWith(normalizedBasePath, StringComparison.OrdinalIgnoreCase) ||
                normalizedDirectory.Equals(normalizedBasePath, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (Directory.Exists(normalizedDirectory) && !Directory.EnumerateFileSystemEntries(normalizedDirectory).Any())
            {
                Directory.Delete(normalizedDirectory);
                var parent = Path.GetDirectoryName(normalizedDirectory);
                if (!string.IsNullOrEmpty(parent))
                {
                    CleanupEmptyDirectories(parent);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to cleanup empty directory: {Directory}", directory);
        }
    }
}
