using System.IO.Compression;
using System.Text;

namespace ClinicalIntelligence.Api.Tests.Helpers;

/// <summary>
/// Helper class for generating test documents programmatically.
/// Creates valid, corrupted, and password-protected test files for unit testing.
/// </summary>
public static class TestDocumentGenerator
{
    /// <summary>
    /// Creates a valid PDF content with proper header and EOF marker.
    /// </summary>
    public static byte[] CreateValidPdf(int sizeInBytes = 1024)
    {
        var pdfContent = @"%PDF-1.4
1 0 obj
<< /Type /Catalog /Pages 2 0 R >>
endobj
2 0 obj
<< /Type /Pages /Kids [3 0 R] /Count 1 >>
endobj
3 0 obj
<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] >>
endobj
xref
0 4
0000000000 65535 f 
0000000009 00000 n 
0000000058 00000 n 
0000000115 00000 n 
trailer
<< /Size 4 /Root 1 0 R >>
startxref
190
%%EOF";
        var baseContent = Encoding.ASCII.GetBytes(pdfContent);
        
        if (sizeInBytes <= baseContent.Length)
            return baseContent;

        // Pad to requested size while keeping valid structure
        var result = new byte[sizeInBytes];
        Array.Copy(baseContent, result, baseContent.Length);
        return result;
    }

    /// <summary>
    /// Creates a PDF with encryption markers (simulates password-protected PDF).
    /// </summary>
    public static byte[] CreatePasswordProtectedPdf()
    {
        var pdfContent = @"%PDF-1.4
1 0 obj
<< /Type /Catalog /Pages 2 0 R >>
endobj
2 0 obj
<< /Type /Pages /Kids [3 0 R] /Count 1 >>
endobj
3 0 obj
<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] >>
endobj
4 0 obj
<< /Filter /Standard /V 2 /R 3 /O (owner) /U (user) /P -3904 >>
endobj
5 0 obj
<< /Type /Encrypt /Filter /Standard /V 2 /R 3 /Length 128 >>
endobj
xref
0 6
0000000000 65535 f 
0000000009 00000 n 
0000000058 00000 n 
0000000115 00000 n 
0000000180 00000 n 
0000000260 00000 n 
trailer
<< /Size 6 /Root 1 0 R /Encrypt 5 0 R >>
startxref
340
%%EOF";
        return Encoding.ASCII.GetBytes(pdfContent);
    }

    /// <summary>
    /// Creates a PDF with AES encryption markers.
    /// </summary>
    public static byte[] CreateAesEncryptedPdf()
    {
        var pdfContent = @"%PDF-1.6
1 0 obj
<< /Type /Catalog /Pages 2 0 R >>
endobj
2 0 obj
<< /Type /Pages /Kids [3 0 R] /Count 1 >>
endobj
3 0 obj
<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] >>
endobj
4 0 obj
<< /Filter /Standard /V 4 /R 4 /CF << /StdCF << /CFM /AESV2 /Length 16 >> >> >>
endobj
xref
0 5
0000000000 65535 f 
0000000009 00000 n 
0000000058 00000 n 
0000000115 00000 n 
0000000180 00000 n 
trailer
<< /Size 5 /Root 1 0 R /Encrypt 4 0 R >>
startxref
300
%%EOF";
        return Encoding.ASCII.GetBytes(pdfContent);
    }

    /// <summary>
    /// Creates a PDF with missing header (corrupted).
    /// </summary>
    public static byte[] CreateCorruptedPdfMissingHeader()
    {
        return Encoding.ASCII.GetBytes("This is not a valid PDF file - missing header");
    }

    /// <summary>
    /// Creates a PDF with missing EOF marker (truncated).
    /// </summary>
    public static byte[] CreateCorruptedPdfMissingEof()
    {
        var pdfContent = @"%PDF-1.4
1 0 obj
<< /Type /Catalog /Pages 2 0 R >>
endobj
2 0 obj
<< /Type /Pages /Kids [3 0 R] /Count 1 >>
endobj
3 0 obj
<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] >>
endobj
xref
0 4
trailer
<< /Size 4 /Root 1 0 R >>
startxref
190";
        return Encoding.ASCII.GetBytes(pdfContent);
    }

    /// <summary>
    /// Creates random bytes (completely corrupted file).
    /// </summary>
    public static byte[] CreateRandomBytes(int size = 1024)
    {
        var random = new Random(42); // Fixed seed for reproducibility
        var bytes = new byte[size];
        random.NextBytes(bytes);
        return bytes;
    }

    /// <summary>
    /// Creates a valid DOCX file (ZIP archive with required entries).
    /// </summary>
    public static byte[] CreateValidDocx()
    {
        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            // [Content_Types].xml
            var contentTypesEntry = archive.CreateEntry("[Content_Types].xml");
            using (var writer = new StreamWriter(contentTypesEntry.Open()))
            {
                writer.Write(@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Types xmlns=""http://schemas.openxmlformats.org/package/2006/content-types"">
    <Default Extension=""rels"" ContentType=""application/vnd.openxmlformats-package.relationships+xml""/>
    <Default Extension=""xml"" ContentType=""application/xml""/>
    <Override PartName=""/word/document.xml"" ContentType=""application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml""/>
</Types>");
            }

            // _rels/.rels
            var relsEntry = archive.CreateEntry("_rels/.rels");
            using (var writer = new StreamWriter(relsEntry.Open()))
            {
                writer.Write(@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">
    <Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"" Target=""word/document.xml""/>
</Relationships>");
            }

            // word/document.xml
            var documentEntry = archive.CreateEntry("word/document.xml");
            using (var writer = new StreamWriter(documentEntry.Open()))
            {
                writer.Write(@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<w:document xmlns:w=""http://schemas.openxmlformats.org/wordprocessingml/2006/main"">
    <w:body>
        <w:p>
            <w:r>
                <w:t>Test Document</w:t>
            </w:r>
        </w:p>
    </w:body>
</w:document>");
            }
        }

        return memoryStream.ToArray();
    }

    /// <summary>
    /// Creates a DOCX with encryption entry (simulates password-protected DOCX).
    /// </summary>
    public static byte[] CreatePasswordProtectedDocx()
    {
        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            // [Content_Types].xml
            var contentTypesEntry = archive.CreateEntry("[Content_Types].xml");
            using (var writer = new StreamWriter(contentTypesEntry.Open()))
            {
                writer.Write(@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Types xmlns=""http://schemas.openxmlformats.org/package/2006/content-types"">
    <Default Extension=""xml"" ContentType=""application/xml""/>
</Types>");
            }

            // EncryptedPackage (indicates encryption)
            var encryptedEntry = archive.CreateEntry("EncryptedPackage");
            using (var writer = new StreamWriter(encryptedEntry.Open()))
            {
                writer.Write("encrypted content placeholder");
            }

            // encryption.xml
            var encryptionXmlEntry = archive.CreateEntry("encryption.xml");
            using (var writer = new StreamWriter(encryptionXmlEntry.Open()))
            {
                writer.Write(@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<encryption xmlns=""http://schemas.microsoft.com/office/2006/encryption"">
    <keyData saltSize=""16"" blockSize=""16"" keyBits=""128"" hashSize=""20"" cipherAlgorithm=""AES"" cipherChaining=""ChainingModeCBC"" hashAlgorithm=""SHA1""/>
</encryption>");
            }
        }

        return memoryStream.ToArray();
    }

    /// <summary>
    /// Creates an OLE compound document (encrypted Office format).
    /// </summary>
    public static byte[] CreateOleEncryptedDocx()
    {
        // OLE compound document signature: D0 CF 11 E0 A1 B1 1A E1
        var oleHeader = new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 };
        var content = new byte[512];
        Array.Copy(oleHeader, content, oleHeader.Length);
        return content;
    }

    /// <summary>
    /// Creates a DOCX with invalid ZIP signature (corrupted).
    /// </summary>
    public static byte[] CreateCorruptedDocxInvalidZip()
    {
        return Encoding.ASCII.GetBytes("This is not a valid ZIP/DOCX file");
    }

    /// <summary>
    /// Creates a DOCX missing [Content_Types].xml (corrupted structure).
    /// </summary>
    public static byte[] CreateCorruptedDocxMissingContentTypes()
    {
        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            // Only add _rels/.rels and word/document.xml (missing [Content_Types].xml)
            var relsEntry = archive.CreateEntry("_rels/.rels");
            using (var writer = new StreamWriter(relsEntry.Open()))
            {
                writer.Write(@"<?xml version=""1.0"" encoding=""UTF-8""?><Relationships/>");
            }

            var documentEntry = archive.CreateEntry("word/document.xml");
            using (var writer = new StreamWriter(documentEntry.Open()))
            {
                writer.Write(@"<?xml version=""1.0"" encoding=""UTF-8""?><document/>");
            }
        }

        return memoryStream.ToArray();
    }

    /// <summary>
    /// Creates a DOCX missing word/document.xml (corrupted structure).
    /// </summary>
    public static byte[] CreateCorruptedDocxMissingDocument()
    {
        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            var contentTypesEntry = archive.CreateEntry("[Content_Types].xml");
            using (var writer = new StreamWriter(contentTypesEntry.Open()))
            {
                writer.Write(@"<?xml version=""1.0"" encoding=""UTF-8""?><Types/>");
            }

            var relsEntry = archive.CreateEntry("_rels/.rels");
            using (var writer = new StreamWriter(relsEntry.Open()))
            {
                writer.Write(@"<?xml version=""1.0"" encoding=""UTF-8""?><Relationships/>");
            }
        }

        return memoryStream.ToArray();
    }

    /// <summary>
    /// Creates a truncated ZIP file (corrupted DOCX).
    /// </summary>
    public static byte[] CreateTruncatedDocx()
    {
        var validDocx = CreateValidDocx();
        // Return only first half of the file
        var truncated = new byte[validDocx.Length / 2];
        Array.Copy(validDocx, truncated, truncated.Length);
        return truncated;
    }

    /// <summary>
    /// Creates an empty file (0 bytes).
    /// </summary>
    public static byte[] CreateEmptyFile()
    {
        return Array.Empty<byte>();
    }

    /// <summary>
    /// Creates a minimal valid PDF (smallest possible valid PDF).
    /// </summary>
    public static byte[] CreateMinimalValidPdf()
    {
        var pdfContent = @"%PDF-1.0
1 0 obj<</Type/Catalog/Pages 2 0 R>>endobj
2 0 obj<</Type/Pages/Count 0/Kids[]>>endobj
xref
0 3
0000000000 65535 f 
0000000009 00000 n 
0000000052 00000 n 
trailer<</Size 3/Root 1 0 R>>
startxref
97
%%EOF";
        return Encoding.ASCII.GetBytes(pdfContent);
    }
}
