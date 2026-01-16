using ClinicalIntelligence.Api.Configuration;
using ClinicalIntelligence.Api.Services;
using ClinicalIntelligence.Api.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClinicalIntelligence.Api.Tests.Services;

/// <summary>
/// Unit tests for LocalFileStorageService.
/// Tests storage path pattern, file operations, and directory management.
/// </summary>
public class LocalFileStorageServiceTests : IDisposable
{
    private readonly string _testBasePath;
    private readonly DocumentStorageOptions _options;
    private readonly Mock<ILogger<LocalFileStorageService>> _mockLogger;
    private readonly LocalFileStorageService _service;

    public LocalFileStorageServiceTests()
    {
        _testBasePath = DocumentStorageTestHelpers.CreateTempTestDirectory();
        _options = DocumentStorageTestHelpers.CreateTestOptions(_testBasePath);
        _mockLogger = new Mock<ILogger<LocalFileStorageService>>();
        _service = new LocalFileStorageService(_options, _mockLogger.Object);
    }

    public void Dispose()
    {
        DocumentStorageTestHelpers.CleanupTestDirectory(_testBasePath);
    }

    #region Storage Path Pattern Tests

    [Fact]
    public async Task StoreAsync_FollowsPattern_TenantPatientDocumentOriginal()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        using var stream = DocumentStorageTestHelpers.CreateTestFileStream();

        // Act
        var result = await _service.StoreAsync(stream, "test.pdf", patientId, documentId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Contains(_options.DefaultTenantId, result.StoragePath);
        Assert.Contains(patientId.ToString(), result.StoragePath);
        Assert.Contains(documentId.ToString(), result.StoragePath);
        Assert.Contains("original", result.StoragePath);
    }

    [Fact]
    public async Task StoreAsync_PreservesFileExtension()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        using var stream = DocumentStorageTestHelpers.CreateTestFileStream();

        // Act
        var result = await _service.StoreAsync(stream, "document.pdf", patientId, documentId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.EndsWith(".pdf", result.StoragePath);
    }

    [Theory]
    [InlineData(".pdf")]
    [InlineData(".docx")]
    public async Task StoreAsync_SupportsAllowedExtensions(string extension)
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        using var stream = DocumentStorageTestHelpers.CreateTestFileStream();

        // Act
        var result = await _service.StoreAsync(stream, $"document{extension}", patientId, documentId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.EndsWith(extension, result.StoragePath);
    }

    [Fact]
    public async Task StoreAsync_UsesDefaultTenantId()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        using var stream = DocumentStorageTestHelpers.CreateTestFileStream();

        // Act
        var result = await _service.StoreAsync(stream, "test.pdf", patientId, documentId, CancellationToken.None);

        // Assert
        Assert.StartsWith("test-tenant", result.StoragePath);
    }

    [Fact]
    public async Task StoreAsync_IncludesPatientId()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        using var stream = DocumentStorageTestHelpers.CreateTestFileStream();

        // Act
        var result = await _service.StoreAsync(stream, "test.pdf", patientId, documentId, CancellationToken.None);

        // Assert
        Assert.Contains(patientId.ToString(), result.StoragePath);
    }

    [Fact]
    public async Task StoreAsync_IncludesDocumentId()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        using var stream = DocumentStorageTestHelpers.CreateTestFileStream();

        // Act
        var result = await _service.StoreAsync(stream, "test.pdf", patientId, documentId, CancellationToken.None);

        // Assert
        Assert.Contains(documentId.ToString(), result.StoragePath);
    }

    #endregion

    #region Store Operation Tests

    [Fact]
    public async Task StoreAsync_CreatesFile_AtCorrectPath()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        using var stream = DocumentStorageTestHelpers.CreateTestFileStream();

        // Act
        var result = await _service.StoreAsync(stream, "test.pdf", patientId, documentId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(File.Exists(result.AbsolutePath));
    }

    [Fact]
    public async Task StoreAsync_CreatesDirectoryStructure()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        using var stream = DocumentStorageTestHelpers.CreateTestFileStream();

        // Act
        var result = await _service.StoreAsync(stream, "test.pdf", patientId, documentId, CancellationToken.None);

        // Assert
        var directory = Path.GetDirectoryName(result.AbsolutePath);
        Assert.True(Directory.Exists(directory));
    }

    [Fact]
    public async Task StoreAsync_WritesCorrectContent()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        var expectedContent = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };
        using var stream = DocumentStorageTestHelpers.CreateTestFileStreamWithContent(expectedContent);

        // Act
        var result = await _service.StoreAsync(stream, "test.pdf", patientId, documentId, CancellationToken.None);

        // Assert
        var actualContent = DocumentStorageTestHelpers.ReadFileBytes(result.AbsolutePath);
        Assert.Equal(expectedContent, actualContent);
    }

    [Fact]
    public async Task StoreAsync_ReturnsCorrectBytesWritten()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        var content = new byte[2048];
        new Random().NextBytes(content);
        using var stream = DocumentStorageTestHelpers.CreateTestFileStreamWithContent(content);

        // Act
        var result = await _service.StoreAsync(stream, "test.pdf", patientId, documentId, CancellationToken.None);

        // Assert
        Assert.Equal(2048, result.BytesWritten);
    }

    [Fact]
    public async Task StoreAsync_ReturnsRelativeStoragePath()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        using var stream = DocumentStorageTestHelpers.CreateTestFileStream();

        // Act
        var result = await _service.StoreAsync(stream, "test.pdf", patientId, documentId, CancellationToken.None);

        // Assert
        Assert.False(Path.IsPathRooted(result.StoragePath));
    }

    [Fact]
    public async Task StoreAsync_ReturnsAbsolutePath()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        using var stream = DocumentStorageTestHelpers.CreateTestFileStream();

        // Act
        var result = await _service.StoreAsync(stream, "test.pdf", patientId, documentId, CancellationToken.None);

        // Assert
        Assert.True(Path.IsPathRooted(result.AbsolutePath));
    }

    [Fact]
    public async Task StoreAsync_HandlesLargeFiles()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        var largeContent = new byte[5 * 1024 * 1024]; // 5MB
        new Random().NextBytes(largeContent);
        using var stream = DocumentStorageTestHelpers.CreateTestFileStreamWithContent(largeContent);

        // Act
        var result = await _service.StoreAsync(stream, "large.pdf", patientId, documentId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(5 * 1024 * 1024, result.BytesWritten);
    }

    #endregion

    #region Retrieve Operation Tests

    [Fact]
    public async Task RetrieveAsync_ReturnsFileStream_WhenExists()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        using var storeStream = DocumentStorageTestHelpers.CreateTestFileStream();
        var storeResult = await _service.StoreAsync(storeStream, "test.pdf", patientId, documentId, CancellationToken.None);

        // Act
        var retrieveStream = await _service.RetrieveAsync(storeResult.StoragePath, CancellationToken.None);

        // Assert
        Assert.NotNull(retrieveStream);
        retrieveStream?.Dispose();
    }

    [Fact]
    public async Task RetrieveAsync_ReturnsNull_WhenNotExists()
    {
        // Arrange
        var nonExistentPath = "test-tenant/nonexistent/path/original.pdf";

        // Act
        var stream = await _service.RetrieveAsync(nonExistentPath, CancellationToken.None);

        // Assert
        Assert.Null(stream);
    }

    [Fact]
    public async Task RetrieveAsync_StreamContainsCorrectContent()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        var expectedContent = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD };
        using var storeStream = DocumentStorageTestHelpers.CreateTestFileStreamWithContent(expectedContent);
        var storeResult = await _service.StoreAsync(storeStream, "test.pdf", patientId, documentId, CancellationToken.None);

        // Act
        using var retrieveStream = await _service.RetrieveAsync(storeResult.StoragePath, CancellationToken.None);
        using var memoryStream = new MemoryStream();
        await retrieveStream!.CopyToAsync(memoryStream);
        var actualContent = memoryStream.ToArray();

        // Assert
        Assert.Equal(expectedContent, actualContent);
    }

    [Fact]
    public async Task RetrieveAsync_StreamIsReadable()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        using var storeStream = DocumentStorageTestHelpers.CreateTestFileStream();
        var storeResult = await _service.StoreAsync(storeStream, "test.pdf", patientId, documentId, CancellationToken.None);

        // Act
        using var retrieveStream = await _service.RetrieveAsync(storeResult.StoragePath, CancellationToken.None);

        // Assert
        Assert.True(retrieveStream!.CanRead);
    }

    #endregion

    #region Delete Operation Tests

    [Fact]
    public async Task DeleteAsync_RemovesFile_WhenExists()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        using var storeStream = DocumentStorageTestHelpers.CreateTestFileStream();
        var storeResult = await _service.StoreAsync(storeStream, "test.pdf", patientId, documentId, CancellationToken.None);

        // Act
        var deleted = await _service.DeleteAsync(storeResult.StoragePath, CancellationToken.None);

        // Assert
        Assert.True(deleted);
        Assert.False(File.Exists(storeResult.AbsolutePath));
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenNotExists()
    {
        // Arrange
        var nonExistentPath = "test-tenant/nonexistent/path/original.pdf";

        // Act
        var deleted = await _service.DeleteAsync(nonExistentPath, CancellationToken.None);

        // Assert
        Assert.False(deleted);
    }

    [Fact]
    public async Task DeleteAsync_CleansUpEmptyDirectories()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        using var storeStream = DocumentStorageTestHelpers.CreateTestFileStream();
        var storeResult = await _service.StoreAsync(storeStream, "test.pdf", patientId, documentId, CancellationToken.None);
        var documentDir = Path.GetDirectoryName(storeResult.AbsolutePath)!;

        // Act
        await _service.DeleteAsync(storeResult.StoragePath, CancellationToken.None);

        // Assert
        Assert.False(Directory.Exists(documentDir));
    }

    [Fact]
    public async Task DeleteAsync_DoesNotDeleteNonEmptyDirectories()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var documentId1 = Guid.NewGuid();
        var documentId2 = Guid.NewGuid();

        using var storeStream1 = DocumentStorageTestHelpers.CreateTestFileStream();
        var storeResult1 = await _service.StoreAsync(storeStream1, "test1.pdf", patientId, documentId1, CancellationToken.None);

        using var storeStream2 = DocumentStorageTestHelpers.CreateTestFileStream();
        var storeResult2 = await _service.StoreAsync(storeStream2, "test2.pdf", patientId, documentId2, CancellationToken.None);

        var patientDir = Path.Combine(_testBasePath, _options.DefaultTenantId, patientId.ToString());

        // Act
        await _service.DeleteAsync(storeResult1.StoragePath, CancellationToken.None);

        // Assert
        Assert.True(Directory.Exists(patientDir)); // Patient directory should still exist
    }

    #endregion

    #region Exists Operation Tests

    [Fact]
    public async Task ExistsAsync_ReturnsTrue_WhenFileExists()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        using var storeStream = DocumentStorageTestHelpers.CreateTestFileStream();
        var storeResult = await _service.StoreAsync(storeStream, "test.pdf", patientId, documentId, CancellationToken.None);

        // Act
        var exists = await _service.ExistsAsync(storeResult.StoragePath, CancellationToken.None);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsAsync_ReturnsFalse_WhenFileNotExists()
    {
        // Arrange
        var nonExistentPath = "test-tenant/nonexistent/path/original.pdf";

        // Act
        var exists = await _service.ExistsAsync(nonExistentPath, CancellationToken.None);

        // Assert
        Assert.False(exists);
    }

    #endregion

    #region Directory Management Tests

    [Fact]
    public void Constructor_CreatesBaseDirectory_IfNotExists()
    {
        // Arrange
        var newBasePath = Path.Combine(Path.GetTempPath(), $"ci_new_test_{Guid.NewGuid()}");
        var options = DocumentStorageTestHelpers.CreateTestOptions(newBasePath);

        try
        {
            // Act
            var service = new LocalFileStorageService(options, _mockLogger.Object);

            // Assert
            Assert.True(Directory.Exists(newBasePath));
        }
        finally
        {
            DocumentStorageTestHelpers.CleanupTestDirectory(newBasePath);
        }
    }

    [Fact]
    public void Constructor_CreatesTempDirectory_IfNotExists()
    {
        // Arrange
        var newBasePath = Path.Combine(Path.GetTempPath(), $"ci_new_test_{Guid.NewGuid()}");
        var options = DocumentStorageTestHelpers.CreateTestOptions(newBasePath);

        try
        {
            // Act
            var service = new LocalFileStorageService(options, _mockLogger.Object);

            // Assert
            Assert.True(Directory.Exists(options.TempPath));
        }
        finally
        {
            DocumentStorageTestHelpers.CleanupTestDirectory(newBasePath);
        }
    }

    [Fact]
    public async Task StoreAsync_CreatesNestedDirectories()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        using var stream = DocumentStorageTestHelpers.CreateTestFileStream();

        // Act
        var result = await _service.StoreAsync(stream, "test.pdf", patientId, documentId, CancellationToken.None);

        // Assert
        var expectedPath = Path.Combine(
            _testBasePath,
            _options.DefaultTenantId,
            patientId.ToString(),
            documentId.ToString());
        Assert.True(Directory.Exists(expectedPath));
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task StoreAsync_HandlesSpecialCharactersInFileName()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        using var stream = DocumentStorageTestHelpers.CreateTestFileStream();

        // Act - filename with special chars (extension is preserved)
        var result = await _service.StoreAsync(stream, "file with spaces (1).pdf", patientId, documentId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.EndsWith(".pdf", result.StoragePath);
    }

    [Fact]
    public async Task StoreAsync_OverwritesExistingFile()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var documentId = Guid.NewGuid();

        var content1 = new byte[] { 0x01, 0x02, 0x03 };
        using var stream1 = DocumentStorageTestHelpers.CreateTestFileStreamWithContent(content1);
        await _service.StoreAsync(stream1, "test.pdf", patientId, documentId, CancellationToken.None);

        var content2 = new byte[] { 0x04, 0x05, 0x06, 0x07 };
        using var stream2 = DocumentStorageTestHelpers.CreateTestFileStreamWithContent(content2);

        // Act
        var result = await _service.StoreAsync(stream2, "test.pdf", patientId, documentId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var actualContent = DocumentStorageTestHelpers.ReadFileBytes(result.AbsolutePath);
        Assert.Equal(content2, actualContent);
    }

    #endregion
}
