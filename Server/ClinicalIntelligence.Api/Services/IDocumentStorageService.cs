namespace ClinicalIntelligence.Api.Services;

/// <summary>
/// Service interface for document storage operations.
/// Abstracts file system operations for document persistence.
/// </summary>
public interface IDocumentStorageService
{
    /// <summary>
    /// Stores a document file and returns the storage path.
    /// </summary>
    /// <param name="fileStream">The file stream to store.</param>
    /// <param name="fileName">Original file name with extension.</param>
    /// <param name="patientId">Patient ID for path organization.</param>
    /// <param name="documentId">Document ID for unique storage.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Storage result with path information.</returns>
    Task<DocumentStorageResult> StoreAsync(
        Stream fileStream,
        string fileName,
        Guid patientId,
        Guid documentId,
        CancellationToken ct);

    /// <summary>
    /// Retrieves a document file stream by storage path.
    /// </summary>
    /// <param name="storagePath">Relative storage path.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>File stream or null if not found.</returns>
    Task<Stream?> RetrieveAsync(string storagePath, CancellationToken ct);

    /// <summary>
    /// Deletes a document file by storage path.
    /// </summary>
    /// <param name="storagePath">Relative storage path.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if deleted, false if not found.</returns>
    Task<bool> DeleteAsync(string storagePath, CancellationToken ct);

    /// <summary>
    /// Checks if a document exists at the storage path.
    /// </summary>
    /// <param name="storagePath">Relative storage path.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if exists, false otherwise.</returns>
    Task<bool> ExistsAsync(string storagePath, CancellationToken ct);
}

/// <summary>
/// Result of a document storage operation.
/// </summary>
public record DocumentStorageResult
{
    /// <summary>
    /// Whether the storage operation succeeded.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Relative storage path (for database storage).
    /// Pattern: {tenant_id}/{patient_id}/{document_id}/original.{ext}
    /// </summary>
    public string StoragePath { get; init; } = string.Empty;

    /// <summary>
    /// Absolute file system path.
    /// </summary>
    public string AbsolutePath { get; init; } = string.Empty;

    /// <summary>
    /// Number of bytes written.
    /// </summary>
    public long BytesWritten { get; init; }

    /// <summary>
    /// Error message if operation failed.
    /// </summary>
    public string? ErrorMessage { get; init; }
}
