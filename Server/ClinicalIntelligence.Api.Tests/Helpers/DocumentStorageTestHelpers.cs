using ClinicalIntelligence.Api.Configuration;

namespace ClinicalIntelligence.Api.Tests.Helpers;

/// <summary>
/// Helper methods for document storage testing.
/// </summary>
public static class DocumentStorageTestHelpers
{
    /// <summary>
    /// Creates a temporary test directory with a unique name.
    /// </summary>
    public static string CreateTempTestDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"ci_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(path);
        return path;
    }

    /// <summary>
    /// Cleans up a test directory and all its contents.
    /// </summary>
    public static void CleanupTestDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }
    }

    /// <summary>
    /// Creates a test file stream with random content.
    /// </summary>
    public static Stream CreateTestFileStream(int sizeBytes = 1024)
    {
        var content = new byte[sizeBytes];
        new Random(42).NextBytes(content); // Fixed seed for reproducibility
        return new MemoryStream(content);
    }

    /// <summary>
    /// Creates a test file stream with specific content.
    /// </summary>
    public static Stream CreateTestFileStreamWithContent(byte[] content)
    {
        return new MemoryStream(content);
    }

    /// <summary>
    /// Creates DocumentStorageOptions for testing.
    /// </summary>
    public static DocumentStorageOptions CreateTestOptions(string basePath)
    {
        return new DocumentStorageOptions
        {
            BasePath = basePath,
            TempPath = Path.Combine(basePath, "temp"),
            DefaultTenantId = "test-tenant"
        };
    }

    /// <summary>
    /// Reads all bytes from a file at the specified path.
    /// </summary>
    public static byte[] ReadFileBytes(string path)
    {
        return File.ReadAllBytes(path);
    }

    /// <summary>
    /// Checks if a directory is empty.
    /// </summary>
    public static bool IsDirectoryEmpty(string path)
    {
        return Directory.Exists(path) && !Directory.EnumerateFileSystemEntries(path).Any();
    }

    /// <summary>
    /// Creates a file at the specified path with given content.
    /// </summary>
    public static void CreateFileWithContent(string path, byte[] content)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        File.WriteAllBytes(path, content);
    }
}
