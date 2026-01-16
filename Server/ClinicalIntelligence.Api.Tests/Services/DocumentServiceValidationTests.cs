using System.Text;
using ClinicalIntelligence.Api.Contracts;
using ClinicalIntelligence.Api.Data;
using ClinicalIntelligence.Api.Services;
using ClinicalIntelligence.Api.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClinicalIntelligence.Api.Tests.Services;

/// <summary>
/// Unit tests for DocumentService file validation logic (US_046).
/// Tests extension validation, MIME type validation, size validation, and double extension detection.
/// </summary>
public class DocumentServiceValidationTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<DocumentService>> _loggerMock;
    private readonly DocumentService _service;

    public DocumentServiceValidationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<DocumentService>>();
        _service = new DocumentService(_dbContext, _loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    private static IFormFile CreateMockFile(string fileName, byte[] content, string contentType)
    {
        var stream = new MemoryStream(content);
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.Length).Returns(content.Length);
        fileMock.Setup(f => f.ContentType).Returns(contentType);
        fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
        return fileMock.Object;
    }

    private static byte[] CreateValidPdfContent(int sizeInBytes = 1024)
    {
        var pdfHeader = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D }; // %PDF-
        var content = new byte[sizeInBytes];
        Array.Copy(pdfHeader, content, Math.Min(pdfHeader.Length, sizeInBytes));
        return content;
    }

    private static byte[] CreateValidDocxContent(int sizeInBytes = 1024)
    {
        // Use TestDocumentGenerator for proper DOCX structure
        return TestDocumentGenerator.CreateValidDocx();
    }

    #region Extension Validation Tests (FR-015a)

    [Theory]
    [InlineData(".pdf", true)]
    [InlineData(".docx", true)]
    [InlineData(".PDF", true)]
    [InlineData(".DOCX", true)]
    [InlineData(".Pdf", true)]
    [InlineData(".DocX", true)]
    public async Task ValidateFile_AllowedExtension_ReturnsIsValidTrue(string extension, bool expectedValid)
    {
        // Arrange
        var content = extension.ToLowerInvariant() == ".pdf" ? CreateValidPdfContent() : CreateValidDocxContent();
        var mimeType = extension.ToLowerInvariant() == ".pdf" 
            ? "application/pdf" 
            : "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
        var file = CreateMockFile($"test{extension}", content, mimeType);

        // Act
        var result = await _service.ValidateAndAcknowledgeAsync(file, Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.Equal(expectedValid, result.IsValid);
        if (expectedValid)
        {
            Assert.Null(result.ErrorCode);
            Assert.Null(result.ErrorType);
        }
    }

    [Theory]
    [InlineData(".txt")]
    [InlineData(".exe")]
    [InlineData(".doc")]
    [InlineData(".xls")]
    [InlineData(".jpg")]
    [InlineData(".png")]
    [InlineData("")]
    public async Task ValidateFile_DisallowedExtension_ReturnsInvalidExtensionError(string extension)
    {
        // Arrange
        var content = Encoding.UTF8.GetBytes("test content");
        var file = CreateMockFile($"test{extension}", content, "text/plain");

        // Act
        var result = await _service.ValidateAndAcknowledgeAsync(file, Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(FileValidationErrorCode.InvalidExtension, result.ErrorCode);
        Assert.Equal("invalid_extension", result.ErrorType);
        Assert.Contains(result.ValidationErrors, e => e.Contains("Unsupported file type"));
    }

    #endregion

    #region MIME Type Validation Tests (FR-015b)

    [Theory]
    [InlineData("application/pdf", ".pdf", true)]
    [InlineData("application/vnd.openxmlformats-officedocument.wordprocessingml.document", ".docx", true)]
    public async Task ValidateFile_ValidMimeType_ReturnsIsValidTrue(string mimeType, string extension, bool expectedValid)
    {
        // Arrange
        var content = extension == ".pdf" ? CreateValidPdfContent() : CreateValidDocxContent();
        var file = CreateMockFile($"test{extension}", content, mimeType);

        // Act
        var result = await _service.ValidateAndAcknowledgeAsync(file, Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.Equal(expectedValid, result.IsValid);
    }

    [Theory]
    [InlineData("text/plain", ".pdf")]
    [InlineData("application/octet-stream", ".pdf")]
    [InlineData("image/jpeg", ".pdf")]
    public async Task ValidateFile_InvalidMimeType_ReturnsInvalidMimeTypeError(string mimeType, string extension)
    {
        // Arrange
        var content = CreateValidPdfContent();
        var file = CreateMockFile($"test{extension}", content, mimeType);

        // Act
        var result = await _service.ValidateAndAcknowledgeAsync(file, Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(FileValidationErrorCode.InvalidMimeType, result.ErrorCode);
        Assert.Equal("invalid_mime_type", result.ErrorType);
    }

    [Theory]
    [InlineData("application/pdf", ".docx")]
    [InlineData("application/vnd.openxmlformats-officedocument.wordprocessingml.document", ".pdf")]
    public async Task ValidateFile_MimeExtensionMismatch_ReturnsMimeExtensionMismatchError(string mimeType, string extension)
    {
        // Arrange
        var content = extension == ".pdf" ? CreateValidPdfContent() : CreateValidDocxContent();
        var file = CreateMockFile($"test{extension}", content, mimeType);

        // Act
        var result = await _service.ValidateAndAcknowledgeAsync(file, Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(FileValidationErrorCode.MimeExtensionMismatch, result.ErrorCode);
        Assert.Equal("mime_extension_mismatch", result.ErrorType);
        Assert.Contains(result.ValidationErrors, e => e.Contains("does not match"));
    }

    #endregion

    #region Size Validation Tests (FR-016)

    [Fact]
    public async Task ValidateFile_EmptyFile_ReturnsFileEmptyError()
    {
        // Arrange
        var file = CreateMockFile("empty.pdf", Array.Empty<byte>(), "application/pdf");

        // Act
        var result = await _service.ValidateAndAcknowledgeAsync(file, Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(FileValidationErrorCode.FileEmpty, result.ErrorCode);
        Assert.Equal("file_empty", result.ErrorType);
        Assert.Contains(result.ValidationErrors, e => e.Contains("Empty files"));
    }

    [Fact]
    public async Task ValidateFile_SmallFile_ReturnsIsValidTrue()
    {
        // Arrange - 1KB file
        var content = CreateValidPdfContent(1024);
        var file = CreateMockFile("small.pdf", content, "application/pdf");

        // Act
        var result = await _service.ValidateAndAcknowledgeAsync(file, Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorCode);
    }

    [Fact]
    public async Task ValidateFile_Exactly50MB_ReturnsIsValidTrue()
    {
        // Arrange - Exactly 50MB (52,428,800 bytes)
        var content = CreateValidPdfContent(50 * 1024 * 1024);
        var file = CreateMockFile("exactly50mb.pdf", content, "application/pdf");

        // Act
        var result = await _service.ValidateAndAcknowledgeAsync(file, Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorCode);
    }

    [Fact]
    public async Task ValidateFile_Over50MBByOneByte_ReturnsFileTooLargeError()
    {
        // Arrange - 50MB + 1 byte (52,428,801 bytes)
        var content = CreateValidPdfContent(50 * 1024 * 1024 + 1);
        var file = CreateMockFile("over50mb.pdf", content, "application/pdf");

        // Act
        var result = await _service.ValidateAndAcknowledgeAsync(file, Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(FileValidationErrorCode.FileTooLarge, result.ErrorCode);
        Assert.Equal("file_too_large", result.ErrorType);
        Assert.Contains(result.ValidationErrors, e => e.Contains("exceeds maximum"));
    }

    [Fact]
    public async Task ValidateFile_100MB_ReturnsFileTooLargeError()
    {
        // Arrange - 100MB file
        var content = CreateValidPdfContent(100 * 1024 * 1024);
        var file = CreateMockFile("large.pdf", content, "application/pdf");

        // Act
        var result = await _service.ValidateAndAcknowledgeAsync(file, Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(FileValidationErrorCode.FileTooLarge, result.ErrorCode);
    }

    #endregion

    #region Double Extension Detection Tests

    [Theory]
    [InlineData("document.pdf", true)]
    [InlineData("document.docx", true)]
    [InlineData("my.report.pdf", true)]
    [InlineData("file-with-dash.pdf", true)]
    [InlineData("file_with_underscore.pdf", true)]
    public async Task ValidateFile_ValidFilename_ReturnsIsValidTrue(string fileName, bool expectedValid)
    {
        // Arrange
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var content = extension == ".docx" ? CreateValidDocxContent() : CreateValidPdfContent();
        var mimeType = extension == ".docx" 
            ? "application/vnd.openxmlformats-officedocument.wordprocessingml.document" 
            : "application/pdf";
        var file = CreateMockFile(fileName, content, mimeType);

        // Act
        var result = await _service.ValidateAndAcknowledgeAsync(file, Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.Equal(expectedValid, result.IsValid);
    }

    [Theory]
    [InlineData("document.pdf.exe")]
    [InlineData("document.docx.bat")]
    [InlineData("file.pdf.cmd")]
    [InlineData("report.docx.vbs")]
    public async Task ValidateFile_DoubleExtensionWithExecutable_ReturnsDoubleExtensionError(string fileName)
    {
        // Arrange - These files have executable extensions so will fail extension check first
        var content = CreateValidPdfContent();
        var file = CreateMockFile(fileName, content, "application/pdf");

        // Act
        var result = await _service.ValidateAndAcknowledgeAsync(file, Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.False(result.IsValid);
        // Will fail on extension validation first since .exe, .bat, etc. are not allowed
        Assert.Equal(FileValidationErrorCode.InvalidExtension, result.ErrorCode);
    }

    [Theory]
    [InlineData("file.pdf.pdf")]
    [InlineData("document.docx.docx")]
    public async Task ValidateFile_DoubleDocumentExtension_ReturnsDoubleExtensionError(string fileName)
    {
        // Arrange
        var extension = Path.GetExtension(fileName);
        var content = extension == ".pdf" ? CreateValidPdfContent() : CreateValidDocxContent();
        var mimeType = extension == ".pdf" 
            ? "application/pdf" 
            : "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
        var file = CreateMockFile(fileName, content, mimeType);

        // Act
        var result = await _service.ValidateAndAcknowledgeAsync(file, Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(FileValidationErrorCode.DoubleExtension, result.ErrorCode);
        Assert.Equal("double_extension", result.ErrorType);
        Assert.Contains(result.ValidationErrors, e => e.Contains("double extension"));
    }

    #endregion

    #region Error Code Assignment Tests

    [Fact]
    public async Task ValidateFile_InvalidExtension_ReturnsInvalidExtensionErrorCode()
    {
        // Arrange
        var file = CreateMockFile("test.txt", Encoding.UTF8.GetBytes("content"), "text/plain");

        // Act
        var result = await _service.ValidateAndAcknowledgeAsync(file, Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.Equal(FileValidationErrorCode.InvalidExtension, result.ErrorCode);
        Assert.Equal("invalid_extension", result.ErrorType);
        Assert.Equal("ValidationFailed", result.Status);
    }

    [Fact]
    public async Task ValidateFile_MimeMismatch_ReturnsMimeExtensionMismatchErrorCode()
    {
        // Arrange - PDF extension with DOCX MIME type
        var content = CreateValidPdfContent();
        var file = CreateMockFile("test.pdf", content, "application/vnd.openxmlformats-officedocument.wordprocessingml.document");

        // Act
        var result = await _service.ValidateAndAcknowledgeAsync(file, Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.Equal(FileValidationErrorCode.MimeExtensionMismatch, result.ErrorCode);
        Assert.Equal("mime_extension_mismatch", result.ErrorType);
    }

    [Fact]
    public async Task ValidateFile_FileTooLarge_ReturnsFileTooLargeErrorCode()
    {
        // Arrange
        var content = CreateValidPdfContent(51 * 1024 * 1024); // 51MB
        var file = CreateMockFile("large.pdf", content, "application/pdf");

        // Act
        var result = await _service.ValidateAndAcknowledgeAsync(file, Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.Equal(FileValidationErrorCode.FileTooLarge, result.ErrorCode);
        Assert.Equal("file_too_large", result.ErrorType);
    }

    [Fact]
    public async Task ValidateFile_EmptyFile_ReturnsFileEmptyErrorCode()
    {
        // Arrange
        var file = CreateMockFile("empty.pdf", Array.Empty<byte>(), "application/pdf");

        // Act
        var result = await _service.ValidateAndAcknowledgeAsync(file, Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.Equal(FileValidationErrorCode.FileEmpty, result.ErrorCode);
        Assert.Equal("file_empty", result.ErrorType);
    }

    #endregion

    #region Edge Case Tests

    [Theory]
    [InlineData("TEST.PDF")]
    [InlineData("test.PDF")]
    [InlineData("Test.Pdf")]
    public async Task ValidateFile_CaseInsensitiveExtension_AcceptsPdf(string fileName)
    {
        // Arrange
        var content = CreateValidPdfContent();
        var file = CreateMockFile(fileName, content, "application/pdf");

        // Act
        var result = await _service.ValidateAndAcknowledgeAsync(file, Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidateFile_WhitespaceInFilename_HandlesCorrectly()
    {
        // Arrange
        var content = CreateValidPdfContent();
        var file = CreateMockFile("my document with spaces.pdf", content, "application/pdf");

        // Act
        var result = await _service.ValidateAndAcknowledgeAsync(file, Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidateFile_UnicodeFilename_HandlesCorrectly()
    {
        // Arrange
        var content = CreateValidPdfContent();
        var file = CreateMockFile("документ_файл.pdf", content, "application/pdf");

        // Act
        var result = await _service.ValidateAndAcknowledgeAsync(file, Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidateFile_VeryLongFilename_HandlesCorrectly()
    {
        // Arrange
        var longName = new string('a', 200) + ".pdf";
        var content = CreateValidPdfContent();
        var file = CreateMockFile(longName, content, "application/pdf");

        // Act
        var result = await _service.ValidateAndAcknowledgeAsync(file, Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidateFile_ValidFile_ReturnsDocumentId()
    {
        // Arrange
        var content = CreateValidPdfContent();
        var file = CreateMockFile("test.pdf", content, "application/pdf");

        // Act
        var result = await _service.ValidateAndAcknowledgeAsync(file, Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.NotEqual(Guid.Empty, result.DocumentId);
    }

    [Fact]
    public async Task ValidateFile_ValidFile_ReturnsCorrectFileName()
    {
        // Arrange
        var content = CreateValidPdfContent();
        var file = CreateMockFile("my-document.pdf", content, "application/pdf");

        // Act
        var result = await _service.ValidateAndAcknowledgeAsync(file, Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.Equal("my-document.pdf", result.FileName);
    }

    [Fact]
    public async Task ValidateFile_ValidFile_ReturnsCorrectFileSize()
    {
        // Arrange
        var content = CreateValidPdfContent(2048);
        var file = CreateMockFile("test.pdf", content, "application/pdf");

        // Act
        var result = await _service.ValidateAndAcknowledgeAsync(file, Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.Equal(2048, result.FileSize);
    }

    [Fact]
    public async Task ValidateFile_ValidFile_ReturnsAcknowledgedAtTimestamp()
    {
        // Arrange
        var content = CreateValidPdfContent();
        var file = CreateMockFile("test.pdf", content, "application/pdf");
        var beforeRequest = DateTime.UtcNow;

        // Act
        var result = await _service.ValidateAndAcknowledgeAsync(file, Guid.NewGuid(), Guid.NewGuid());

        var afterRequest = DateTime.UtcNow;

        // Assert
        Assert.True(result.AcknowledgedAt >= beforeRequest.AddSeconds(-1));
        Assert.True(result.AcknowledgedAt <= afterRequest.AddSeconds(1));
    }

    #endregion
}
