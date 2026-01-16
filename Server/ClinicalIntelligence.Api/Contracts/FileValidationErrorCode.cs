namespace ClinicalIntelligence.Api.Contracts;

/// <summary>
/// Error codes for file validation failures.
/// Provides programmatic error identification for client handling.
/// </summary>
public enum FileValidationErrorCode
{
    /// <summary>No validation error.</summary>
    None = 0,

    /// <summary>FR-015a: File extension is not allowed (.pdf, .docx only).</summary>
    InvalidExtension = 1,

    /// <summary>FR-015b: MIME type is not allowed for upload.</summary>
    InvalidMimeType = 2,

    /// <summary>FR-015b: MIME type does not match the file extension.</summary>
    MimeExtensionMismatch = 3,

    /// <summary>FR-016: File size exceeds 50MB limit.</summary>
    FileTooLarge = 4,

    /// <summary>FR-015f: File is empty (0 bytes).</summary>
    FileEmpty = 5,

    /// <summary>Security: Double extension detected (e.g., .pdf.exe).</summary>
    DoubleExtension = 6,

    /// <summary>FR-015g: Executable or suspicious content detected.</summary>
    SuspiciousContent = 7,

    /// <summary>FR-015c: File is password-protected.</summary>
    PasswordProtected = 10,

    /// <summary>FR-015d: File is corrupted or malformed.</summary>
    FileCorrupted = 11,

    /// <summary>FR-015e: File structure is invalid.</summary>
    InvalidStructure = 12,

    /// <summary>TR-018, FR-015g: Malware or virus detected.</summary>
    MalwareDetected = 20,

    /// <summary>Malware scanner service is unavailable.</summary>
    ScannerUnavailable = 21,

    /// <summary>Malware scan exceeded timeout.</summary>
    ScanTimeout = 22,

    /// <summary>File storage operation failed.</summary>
    StorageFailed = 30
}
