using System.IO.Compression;
using System.Text;
using ClinicalIntelligence.Api.Contracts;

namespace ClinicalIntelligence.Api.Services;

/// <summary>
/// Result of document integrity validation.
/// </summary>
public record DocumentValidationResult
{
    public bool IsValid { get; init; }
    public FileValidationErrorCode? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
    public bool IsPasswordProtected { get; init; }
    public bool IsCorrupted { get; init; }
    public bool IsEmpty { get; init; }
}

/// <summary>
/// Interface for document integrity validation.
/// Validates document structure, detects password protection, and identifies corruption.
/// </summary>
public interface IDocumentIntegrityValidator
{
    Task<DocumentValidationResult> ValidateAsync(Stream stream, string fileName, string contentType, CancellationToken ct = default);
}

/// <summary>
/// Document integrity validator that checks for password protection and corruption.
/// Implements FR-015c (password protection), FR-015d (corruption), FR-015e (structure validation).
/// </summary>
public class DocumentIntegrityValidator : IDocumentIntegrityValidator
{
    private readonly ILogger<DocumentIntegrityValidator> _logger;

    public DocumentIntegrityValidator(ILogger<DocumentIntegrityValidator> logger)
    {
        _logger = logger;
    }

    public async Task<DocumentValidationResult> ValidateAsync(Stream stream, string fileName, string contentType, CancellationToken ct = default)
    {
        if (stream.Length == 0)
        {
            return new DocumentValidationResult
            {
                IsValid = false,
                ErrorCode = FileValidationErrorCode.FileEmpty,
                ErrorMessage = "Empty files cannot be processed. Please upload a file with content.",
                IsEmpty = true
            };
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        try
        {
            return extension switch
            {
                ".pdf" => await ValidatePdfAsync(stream, ct),
                ".docx" => await ValidateDocxAsync(stream, ct),
                _ => new DocumentValidationResult { IsValid = true }
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Document validation failed for {FileName}", fileName);
            return new DocumentValidationResult
            {
                IsValid = false,
                ErrorCode = FileValidationErrorCode.FileCorrupted,
                ErrorMessage = "File appears to be corrupted. Please try re-downloading or re-exporting the file.",
                IsCorrupted = true
            };
        }
    }

    private async Task<DocumentValidationResult> ValidatePdfAsync(Stream stream, CancellationToken ct)
    {
        stream.Position = 0;

        // Read enough bytes to check header and search for encryption markers
        var bufferSize = Math.Min(stream.Length, 65536); // Read up to 64KB for header analysis
        var buffer = new byte[bufferSize];
        var bytesRead = await stream.ReadAsync(buffer, 0, (int)bufferSize, ct);

        if (bytesRead < 5)
        {
            return new DocumentValidationResult
            {
                IsValid = false,
                ErrorCode = FileValidationErrorCode.FileCorrupted,
                ErrorMessage = "File appears to be corrupted. The file is too small to be a valid PDF.",
                IsCorrupted = true
            };
        }

        // Validate PDF header (%PDF-)
        if (buffer[0] != 0x25 || buffer[1] != 0x50 || buffer[2] != 0x44 || buffer[3] != 0x46 || buffer[4] != 0x2D)
        {
            return new DocumentValidationResult
            {
                IsValid = false,
                ErrorCode = FileValidationErrorCode.FileCorrupted,
                ErrorMessage = "File structure is invalid. The file may be damaged or not a valid PDF.",
                IsCorrupted = true
            };
        }

        // Convert to string for pattern matching
        var content = Encoding.ASCII.GetString(buffer, 0, bytesRead);

        // Check for encryption markers
        if (IsPdfEncrypted(content))
        {
            _logger.LogInformation("Password-protected PDF detected");
            return new DocumentValidationResult
            {
                IsValid = false,
                ErrorCode = FileValidationErrorCode.PasswordProtected,
                ErrorMessage = "Password-protected files are not supported. Please remove password protection and try again.",
                IsPasswordProtected = true
            };
        }

        // Read the end of file to check for %%EOF marker
        stream.Position = Math.Max(0, stream.Length - 1024);
        var tailBuffer = new byte[1024];
        var tailBytesRead = await stream.ReadAsync(tailBuffer, 0, tailBuffer.Length, ct);
        var tailContent = Encoding.ASCII.GetString(tailBuffer, 0, tailBytesRead);

        if (!tailContent.Contains("%%EOF"))
        {
            _logger.LogWarning("PDF missing %%EOF marker - may be truncated");
            return new DocumentValidationResult
            {
                IsValid = false,
                ErrorCode = FileValidationErrorCode.FileCorrupted,
                ErrorMessage = "File appears to be corrupted. The PDF file may be truncated.",
                IsCorrupted = true
            };
        }

        return new DocumentValidationResult { IsValid = true };
    }

    private static bool IsPdfEncrypted(string content)
    {
        // Check for encryption dictionary markers
        // /Encrypt indicates the PDF has encryption
        if (content.Contains("/Encrypt"))
            return true;

        // Check for standard security handler
        if (content.Contains("/Standard") && content.Contains("/Filter"))
            return true;

        // Check for AES encryption markers
        if (content.Contains("/AESV2") || content.Contains("/AESV3"))
            return true;

        // Check for permission restrictions (often indicates encryption)
        // /P with a negative value typically indicates restricted permissions
        var pIndex = content.IndexOf("/P ", StringComparison.Ordinal);
        if (pIndex > 0 && pIndex + 3 < content.Length)
        {
            var afterP = content.Substring(pIndex + 3, Math.Min(20, content.Length - pIndex - 3));
            if (afterP.TrimStart().StartsWith("-"))
                return true;
        }

        return false;
    }

    private async Task<DocumentValidationResult> ValidateDocxAsync(Stream stream, CancellationToken ct)
    {
        stream.Position = 0;

        // Check ZIP signature (PK..)
        var header = new byte[4];
        var headerBytesRead = await stream.ReadAsync(header, 0, 4, ct);

        if (headerBytesRead < 4 || header[0] != 0x50 || header[1] != 0x4B)
        {
            // Check if it's an OLE compound document (encrypted DOCX)
            stream.Position = 0;
            var oleHeader = new byte[8];
            await stream.ReadAsync(oleHeader, 0, 8, ct);

            // OLE compound document signature: D0 CF 11 E0 A1 B1 1A E1
            if (oleHeader[0] == 0xD0 && oleHeader[1] == 0xCF && oleHeader[2] == 0x11 && oleHeader[3] == 0xE0)
            {
                _logger.LogInformation("Password-protected DOCX (OLE format) detected");
                return new DocumentValidationResult
                {
                    IsValid = false,
                    ErrorCode = FileValidationErrorCode.PasswordProtected,
                    ErrorMessage = "Password-protected files are not supported. Please remove password protection and try again.",
                    IsPasswordProtected = true
                };
            }

            return new DocumentValidationResult
            {
                IsValid = false,
                ErrorCode = FileValidationErrorCode.FileCorrupted,
                ErrorMessage = "File structure is invalid. The file may be damaged or not a valid DOCX.",
                IsCorrupted = true
            };
        }

        stream.Position = 0;

        try
        {
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);

            // Check for required DOCX entries
            var hasContentTypes = archive.GetEntry("[Content_Types].xml") != null;
            var hasDocumentXml = archive.GetEntry("word/document.xml") != null;
            var hasRels = archive.GetEntry("_rels/.rels") != null;

            // Check for encryption indicators
            var encryptionEntry = archive.GetEntry("EncryptedPackage");
            var encryptionXml = archive.GetEntry("encryption.xml");

            if (encryptionEntry != null || encryptionXml != null)
            {
                _logger.LogInformation("Password-protected DOCX (encrypted package) detected");
                return new DocumentValidationResult
                {
                    IsValid = false,
                    ErrorCode = FileValidationErrorCode.PasswordProtected,
                    ErrorMessage = "Password-protected files are not supported. Please remove password protection and try again.",
                    IsPasswordProtected = true
                };
            }

            if (!hasContentTypes)
            {
                return new DocumentValidationResult
                {
                    IsValid = false,
                    ErrorCode = FileValidationErrorCode.InvalidStructure,
                    ErrorMessage = "File structure is invalid. Missing required [Content_Types].xml.",
                    IsCorrupted = true
                };
            }

            if (!hasDocumentXml)
            {
                return new DocumentValidationResult
                {
                    IsValid = false,
                    ErrorCode = FileValidationErrorCode.InvalidStructure,
                    ErrorMessage = "File structure is invalid. Missing required word/document.xml.",
                    IsCorrupted = true
                };
            }

            if (!hasRels)
            {
                return new DocumentValidationResult
                {
                    IsValid = false,
                    ErrorCode = FileValidationErrorCode.InvalidStructure,
                    ErrorMessage = "File structure is invalid. Missing required _rels/.rels.",
                    IsCorrupted = true
                };
            }

            // Validate XML content of document.xml
            var documentEntry = archive.GetEntry("word/document.xml");
            if (documentEntry != null)
            {
                using var entryStream = documentEntry.Open();
                using var reader = new StreamReader(entryStream);
                var xmlContent = await reader.ReadToEndAsync(ct);

                if (string.IsNullOrWhiteSpace(xmlContent) || !xmlContent.TrimStart().StartsWith("<?xml") && !xmlContent.TrimStart().StartsWith("<"))
                {
                    return new DocumentValidationResult
                    {
                        IsValid = false,
                        ErrorCode = FileValidationErrorCode.InvalidStructure,
                        ErrorMessage = "File structure is invalid. The document.xml content is not valid XML.",
                        IsCorrupted = true
                    };
                }
            }

            return new DocumentValidationResult { IsValid = true };
        }
        catch (InvalidDataException ex)
        {
            _logger.LogWarning(ex, "Invalid ZIP archive structure in DOCX");
            return new DocumentValidationResult
            {
                IsValid = false,
                ErrorCode = FileValidationErrorCode.FileCorrupted,
                ErrorMessage = "File appears to be corrupted. The DOCX archive structure is invalid.",
                IsCorrupted = true
            };
        }
    }
}
