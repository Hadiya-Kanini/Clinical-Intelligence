using System;
using System.Text.Json;

// Test what the backend is actually serializing
var job = new
{
    JobId = Guid.NewGuid(),
    DocumentId = Guid.NewGuid(),
    PatientId = (Guid?)null,
    StoragePath = "test/path.pdf",
    MimeType = "application/pdf"
};

var workerJob = new
{
    schema_version = "1.0",
    job_id = job.JobId.ToString(),
    document_id = job.DocumentId.ToString(),
    status = "pending",
    payload = new
    {
        storage_path = job.StoragePath,
        mime_type = job.MimeType,
        patient_id = job.PatientId?.ToString(),
        document_id = job.DocumentId.ToString()
    }
};

var messageBody = JsonSerializer.Serialize(workerJob);
Console.WriteLine("Backend serialized job:");
Console.WriteLine(messageBody);

// Pretty print
var prettyJson = JsonSerializer.Serialize(workerJob, new JsonSerializerOptions { WriteIndented = true });
Console.WriteLine("\nPretty format:");
Console.WriteLine(prettyJson);
