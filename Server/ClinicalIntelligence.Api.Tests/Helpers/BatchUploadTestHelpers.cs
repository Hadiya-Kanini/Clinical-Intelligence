using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace ClinicalIntelligence.Api.Tests.Helpers;

/// <summary>
/// Helper methods for batch upload testing.
/// </summary>
public static class BatchUploadTestHelpers
{
    /// <summary>
    /// Creates a mock IFormFileCollection with the specified number of files.
    /// </summary>
    public static IFormFileCollection CreateMockFiles(int count, string extension = ".pdf")
    {
        var files = new FormFileCollection();
        for (int i = 0; i < count; i++)
        {
            files.Add(CreateMockFile($"file{i + 1}{extension}"));
        }
        return files;
    }

    /// <summary>
    /// Creates a mock IFormFile with the specified filename.
    /// </summary>
    public static IFormFile CreateMockFile(string fileName, byte[]? content = null)
    {
        content ??= TestDocumentGenerator.CreateValidPdf();
        var stream = new MemoryStream(content);
        var contentType = GetContentType(fileName);

        return new FormFile(stream, 0, content.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    /// <summary>
    /// Creates MultipartFormDataContent for batch upload HTTP requests.
    /// </summary>
    public static MultipartFormDataContent CreateBatchUploadContent(
        Guid patientId,
        int fileCount,
        string extension = ".pdf")
    {
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(patientId.ToString()), "patientId");

        for (int i = 0; i < fileCount; i++)
        {
            var fileContent = new ByteArrayContent(TestDocumentGenerator.CreateValidPdf());
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(GetContentType($"file{i + 1}{extension}"));
            content.Add(fileContent, "files", $"file{i + 1}{extension}");
        }

        return content;
    }

    /// <summary>
    /// Creates MultipartFormDataContent with mixed valid and invalid files.
    /// </summary>
    public static MultipartFormDataContent CreateMixedBatchUploadContent(
        Guid patientId,
        int validCount,
        int invalidCount)
    {
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(patientId.ToString()), "patientId");

        // Add valid PDF files
        for (int i = 0; i < validCount; i++)
        {
            var fileContent = new ByteArrayContent(TestDocumentGenerator.CreateValidPdf());
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            content.Add(fileContent, "files", $"valid{i + 1}.pdf");
        }

        // Add invalid files (wrong extension)
        for (int i = 0; i < invalidCount; i++)
        {
            var fileContent = new ByteArrayContent(new byte[] { 0x00, 0x01, 0x02 });
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            content.Add(fileContent, "files", $"invalid{i + 1}.exe");
        }

        return content;
    }

    /// <summary>
    /// Gets the MIME type for a file based on its extension.
    /// </summary>
    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => "application/octet-stream"
        };
    }
}
