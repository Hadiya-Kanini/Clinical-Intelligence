using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace ClinicalIntelligence.Api.Services;

public class JobSchemaValidator
{
    private readonly JSchema _schema;
    private readonly ILogger<JobSchemaValidator> _logger;

    public JobSchemaValidator(IConfiguration configuration, ILogger<JobSchemaValidator> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        var schemaPath = configuration.GetValue<string>("JobSchemaPath") 
            ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "contracts", "jobs", "v1", "job.schema.json");
        
        if (!File.Exists(schemaPath))
        {
            throw new FileNotFoundException($"Job schema file not found at path: {schemaPath}", schemaPath);
        }

        try
        {
            var schemaJson = File.ReadAllText(schemaPath);
            _schema = JSchema.Parse(schemaJson);
        }
        catch (Exception ex) when (ex is JsonException || ex is JSchemaReaderException)
        {
            throw new JsonException($"Failed to parse invalid schema file at: {schemaPath}", ex);
        }
    }

    public JobSchemaValidator(string schemaJson, ILogger<JobSchemaValidator> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        if (string.IsNullOrEmpty(schemaJson))
        {
            throw new ArgumentException("Schema JSON cannot be null or empty", nameof(schemaJson));
        }

        try
        {
            _schema = JSchema.Parse(schemaJson);
        }
        catch (Exception ex) when (ex is JsonException || ex is JSchemaReaderException)
        {
            throw new JsonException("Failed to parse invalid schema file", ex);
        }
    }

    public void ValidateJobPayload(string jobPayloadJson)
    {
        if (string.IsNullOrEmpty(jobPayloadJson))
        {
            throw new ValidationException("Job payload cannot be null or empty");
        }

        JObject jobPayload;
        try
        {
            jobPayload = JObject.Parse(jobPayloadJson);
        }
        catch (JsonException ex)
        {
            throw new ValidationException("Invalid JSON format", ex);
        }

        IList<string> validationErrors;
        bool isValid = jobPayload.IsValid(_schema, out validationErrors);

        if (!isValid)
        {
            var errorMessage = string.Join("; ", validationErrors);
            
            if (errorMessage.Contains("schema_version"))
            {
                var schemaVersion = jobPayload["schema_version"]?.ToString();
                if (!string.IsNullOrEmpty(schemaVersion) && schemaVersion != "1.0")
                {
                    throw new ValidationException($"Unsupported schema version: {schemaVersion}");
                }
                throw new ValidationException("schema_version is required");
            }

            if (errorMessage.Contains("document_id"))
            {
                var documentId = jobPayload["document_id"]?.ToString();
                if (string.IsNullOrEmpty(documentId))
                {
                    throw new ValidationException("document_id is required");
                }
                if (documentId == string.Empty)
                {
                    throw new ValidationException("document_id cannot be empty");
                }
            }

            if (errorMessage.Contains("job_id"))
            {
                var jobId = jobPayload["job_id"]?.ToString();
                if (!string.IsNullOrEmpty(jobId))
                {
                    throw new ValidationException("Invalid UUID format for job_id");
                }
                throw new ValidationException("job_id is required");
            }

            if (errorMessage.Contains("status"))
            {
                var status = jobPayload["status"]?.ToString();
                if (!string.IsNullOrEmpty(status))
                {
                    throw new ValidationException($"Invalid status value: {status}");
                }
                throw new ValidationException("status is required");
            }

            _logger.LogWarning("Job payload validation failed: {Errors}", errorMessage);
            throw new ValidationException(errorMessage);
        }

        _logger.LogInformation("Job payload validation succeeded");
    }
}

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message)
    {
    }

    public ValidationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
