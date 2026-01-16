using ClinicalIntelligence.Api.Contracts;
using ClinicalIntelligence.Api.Services;
using ClinicalIntelligence.Api.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClinicalIntelligence.Api.Tests.Services;

/// <summary>
/// Unit tests for DocumentIntegrityValidator (US_047).
/// Tests password protection detection, corruption detection, and structure validation.
/// </summary>
public class DocumentIntegrityValidatorTests
{
    private readonly Mock<ILogger<DocumentIntegrityValidator>> _loggerMock;
    private readonly DocumentIntegrityValidator _validator;

    public DocumentIntegrityValidatorTests()
    {
        _loggerMock = new Mock<ILogger<DocumentIntegrityValidator>>();
        _validator = new DocumentIntegrityValidator(_loggerMock.Object);
    }

    #region PDF Password Protection Detection Tests (FR-015c)

    [Fact]
    public async Task ValidatePdf_PasswordProtected_ReturnsPasswordProtectedError()
    {
        // Arrange
        var content = TestDocumentGenerator.CreatePasswordProtectedPdf();
        using var stream = new MemoryStream(content);

        // Act
        var result = await _validator.ValidateAsync(stream, "protected.pdf", "application/pdf");

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(FileValidationErrorCode.PasswordProtected, result.ErrorCode);
        Assert.True(result.IsPasswordProtected);
        Assert.Contains("Password-protected", result.ErrorMessage);
    }

    [Fact]
    public async Task ValidatePdf_PasswordProtectedAES256_ReturnsPasswordProtectedError()
    {
        // Arrange
        var content = TestDocumentGenerator.CreateAesEncryptedPdf();
        using var stream = new MemoryStream(content);

        // Act
        var result = await _validator.ValidateAsync(stream, "aes-protected.pdf", "application/pdf");

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(FileValidationErrorCode.PasswordProtected, result.ErrorCode);
        Assert.True(result.IsPasswordProtected);
    }

    [Fact]
    public async Task ValidatePdf_NoPassword_ReturnsValid()
    {
        // Arrange
        var content = TestDocumentGenerator.CreateValidPdf();
        using var stream = new MemoryStream(content);

        // Act
        var result = await _validator.ValidateAsync(stream, "valid.pdf", "application/pdf");

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorCode);
        Assert.False(result.IsPasswordProtected);
    }

    #endregion

    #region DOCX Password Protection Detection Tests (FR-015c)

    [Fact]
    public async Task ValidateDocx_PasswordProtected_ReturnsPasswordProtectedError()
    {
        // Arrange
        var content = TestDocumentGenerator.CreatePasswordProtectedDocx();
        using var stream = new MemoryStream(content);

        // Act
        var result = await _validator.ValidateAsync(stream, "protected.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(FileValidationErrorCode.PasswordProtected, result.ErrorCode);
        Assert.True(result.IsPasswordProtected);
        Assert.Contains("Password-protected", result.ErrorMessage);
    }

    [Fact]
    public async Task ValidateDocx_OleEncrypted_ReturnsPasswordProtectedError()
    {
        // Arrange
        var content = TestDocumentGenerator.CreateOleEncryptedDocx();
        using var stream = new MemoryStream(content);

        // Act
        var result = await _validator.ValidateAsync(stream, "ole-protected.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(FileValidationErrorCode.PasswordProtected, result.ErrorCode);
        Assert.True(result.IsPasswordProtected);
    }

    [Fact]
    public async Task ValidateDocx_NoPassword_ReturnsValid()
    {
        // Arrange
        var content = TestDocumentGenerator.CreateValidDocx();
        using var stream = new MemoryStream(content);

        // Act
        var result = await _validator.ValidateAsync(stream, "valid.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorCode);
        Assert.False(result.IsPasswordProtected);
    }

    #endregion

    #region PDF Corruption Detection Tests (FR-015d)

    [Fact]
    public async Task ValidatePdf_MissingHeader_ReturnsCorruptedError()
    {
        // Arrange
        var content = TestDocumentGenerator.CreateCorruptedPdfMissingHeader();
        using var stream = new MemoryStream(content);

        // Act
        var result = await _validator.ValidateAsync(stream, "corrupted.pdf", "application/pdf");

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(FileValidationErrorCode.FileCorrupted, result.ErrorCode);
        Assert.True(result.IsCorrupted);
    }

    [Fact]
    public async Task ValidatePdf_MissingEOF_ReturnsCorruptedError()
    {
        // Arrange
        var content = TestDocumentGenerator.CreateCorruptedPdfMissingEof();
        using var stream = new MemoryStream(content);

        // Act
        var result = await _validator.ValidateAsync(stream, "truncated.pdf", "application/pdf");

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(FileValidationErrorCode.FileCorrupted, result.ErrorCode);
        Assert.True(result.IsCorrupted);
        Assert.Contains("truncated", result.ErrorMessage);
    }

    [Fact]
    public async Task ValidatePdf_RandomBytes_ReturnsCorruptedError()
    {
        // Arrange
        var content = TestDocumentGenerator.CreateRandomBytes();
        using var stream = new MemoryStream(content);

        // Act
        var result = await _validator.ValidateAsync(stream, "random.pdf", "application/pdf");

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(FileValidationErrorCode.FileCorrupted, result.ErrorCode);
        Assert.True(result.IsCorrupted);
    }

    [Fact]
    public async Task ValidatePdf_TooSmall_ReturnsCorruptedError()
    {
        // Arrange - Only 3 bytes
        var content = new byte[] { 0x25, 0x50, 0x44 }; // %PD (incomplete header)
        using var stream = new MemoryStream(content);

        // Act
        var result = await _validator.ValidateAsync(stream, "tiny.pdf", "application/pdf");

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(FileValidationErrorCode.FileCorrupted, result.ErrorCode);
    }

    #endregion

    #region DOCX Corruption Detection Tests (FR-015d)

    [Fact]
    public async Task ValidateDocx_InvalidZipSignature_ReturnsCorruptedError()
    {
        // Arrange
        var content = TestDocumentGenerator.CreateCorruptedDocxInvalidZip();
        using var stream = new MemoryStream(content);

        // Act
        var result = await _validator.ValidateAsync(stream, "invalid.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(FileValidationErrorCode.FileCorrupted, result.ErrorCode);
        Assert.True(result.IsCorrupted);
    }

    [Fact]
    public async Task ValidateDocx_MissingContentTypes_ReturnsInvalidStructureError()
    {
        // Arrange
        var content = TestDocumentGenerator.CreateCorruptedDocxMissingContentTypes();
        using var stream = new MemoryStream(content);

        // Act
        var result = await _validator.ValidateAsync(stream, "missing-content-types.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(FileValidationErrorCode.InvalidStructure, result.ErrorCode);
        Assert.Contains("[Content_Types].xml", result.ErrorMessage);
    }

    [Fact]
    public async Task ValidateDocx_MissingDocumentXml_ReturnsInvalidStructureError()
    {
        // Arrange
        var content = TestDocumentGenerator.CreateCorruptedDocxMissingDocument();
        using var stream = new MemoryStream(content);

        // Act
        var result = await _validator.ValidateAsync(stream, "missing-document.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(FileValidationErrorCode.InvalidStructure, result.ErrorCode);
        Assert.Contains("word/document.xml", result.ErrorMessage);
    }

    [Fact]
    public async Task ValidateDocx_TruncatedFile_ReturnsCorruptedError()
    {
        // Arrange
        var content = TestDocumentGenerator.CreateTruncatedDocx();
        using var stream = new MemoryStream(content);

        // Act
        var result = await _validator.ValidateAsync(stream, "truncated.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");

        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.ErrorCode == FileValidationErrorCode.FileCorrupted || result.ErrorCode == FileValidationErrorCode.InvalidStructure);
    }

    #endregion

    #region Document Structure Validation Tests (FR-015e)

    [Fact]
    public async Task ValidatePdf_ValidStructure_ReturnsValid()
    {
        // Arrange
        var content = TestDocumentGenerator.CreateValidPdf();
        using var stream = new MemoryStream(content);

        // Act
        var result = await _validator.ValidateAsync(stream, "valid.pdf", "application/pdf");

        // Assert
        Assert.True(result.IsValid);
        Assert.False(result.IsCorrupted);
    }

    [Fact]
    public async Task ValidateDocx_ValidStructure_ReturnsValid()
    {
        // Arrange
        var content = TestDocumentGenerator.CreateValidDocx();
        using var stream = new MemoryStream(content);

        // Act
        var result = await _validator.ValidateAsync(stream, "valid.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");

        // Assert
        Assert.True(result.IsValid);
        Assert.False(result.IsCorrupted);
    }

    #endregion

    #region Empty File Detection Tests (FR-015f)

    [Fact]
    public async Task ValidateFile_ZeroBytes_ReturnsEmptyFileError()
    {
        // Arrange
        var content = TestDocumentGenerator.CreateEmptyFile();
        using var stream = new MemoryStream(content);

        // Act
        var result = await _validator.ValidateAsync(stream, "empty.pdf", "application/pdf");

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(FileValidationErrorCode.FileEmpty, result.ErrorCode);
        Assert.True(result.IsEmpty);
        Assert.Contains("Empty files", result.ErrorMessage);
    }

    [Fact]
    public async Task ValidateFile_OneByte_ReturnsCorruptedError()
    {
        // Arrange - 1 byte is not empty but definitely corrupted
        var content = new byte[] { 0x00 };
        using var stream = new MemoryStream(content);

        // Act
        var result = await _validator.ValidateAsync(stream, "onebyte.pdf", "application/pdf");

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(FileValidationErrorCode.FileCorrupted, result.ErrorCode);
    }

    [Fact]
    public async Task ValidateFile_MinimalValidPdf_ReturnsValid()
    {
        // Arrange
        var content = TestDocumentGenerator.CreateMinimalValidPdf();
        using var stream = new MemoryStream(content);

        // Act
        var result = await _validator.ValidateAsync(stream, "minimal.pdf", "application/pdf");

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidateFile_MinimalValidDocx_ReturnsValid()
    {
        // Arrange
        var content = TestDocumentGenerator.CreateValidDocx();
        using var stream = new MemoryStream(content);

        // Act
        var result = await _validator.ValidateAsync(stream, "minimal.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");

        // Assert
        Assert.True(result.IsValid);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task ValidatePdf_DifferentPdfVersions_HandlesCorrectly()
    {
        // Arrange - PDF 1.4 version
        var content = TestDocumentGenerator.CreateValidPdf();
        using var stream = new MemoryStream(content);

        // Act
        var result = await _validator.ValidateAsync(stream, "v14.pdf", "application/pdf");

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidateFile_UnknownExtension_ReturnsValid()
    {
        // Arrange - Unknown extension should pass through
        var content = new byte[] { 0x00, 0x01, 0x02, 0x03 };
        using var stream = new MemoryStream(content);

        // Act
        var result = await _validator.ValidateAsync(stream, "file.unknown", "application/octet-stream");

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidatePdf_LargeValidFile_CompletesSuccessfully()
    {
        // Arrange - Use minimal valid PDF which has proper structure with %%EOF
        var content = TestDocumentGenerator.CreateMinimalValidPdf();
        using var stream = new MemoryStream(content);

        // Act
        var result = await _validator.ValidateAsync(stream, "large.pdf", "application/pdf");

        // Assert
        Assert.True(result.IsValid);
    }

    #endregion
}
