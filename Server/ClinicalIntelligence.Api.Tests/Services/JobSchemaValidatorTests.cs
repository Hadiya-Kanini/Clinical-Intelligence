using ClinicalIntelligence.Api.Services;
using ClinicalIntelligence.Api.Tests.Mocks;
using ClinicalIntelligence.Api.Tests.TestData;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClinicalIntelligence.Api.Tests.Services;

public class JobSchemaValidatorTests
{
    private readonly Mock<ILogger<JobSchemaValidator>> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfiguration;

    public JobSchemaValidatorTests()
    {
        _mockLogger = new Mock<ILogger<JobSchemaValidator>>();
        _mockConfiguration = new Mock<IConfiguration>();
    }

    private JobSchemaValidator CreateValidator()
    {
        return new JobSchemaValidator(MockSchemaProvider.ValidJobSchema, _mockLogger.Object);
    }

    [Fact]
    public void ValidateJobPayload_ValidPayload_Succeeds()
    {
        var validator = CreateValidator();

        var exception = Record.Exception(() => validator.ValidateJobPayload(JobPayloads.ValidJobPayload));

        Assert.Null(exception);
    }

    [Fact]
    public void ValidateJobPayload_ValidPayloadWithNullPayload_Succeeds()
    {
        var validator = CreateValidator();

        var exception = Record.Exception(() => validator.ValidateJobPayload(JobPayloads.ValidJobPayloadWithNullPayload));

        Assert.Null(exception);
    }

    [Fact]
    public void ValidateJobPayload_ValidPayloadCompleted_Succeeds()
    {
        var validator = CreateValidator();

        var exception = Record.Exception(() => validator.ValidateJobPayload(JobPayloads.ValidJobPayloadCompleted));

        Assert.Null(exception);
    }

    [Fact]
    public void ValidateJobPayload_ValidPayloadFailed_Succeeds()
    {
        var validator = CreateValidator();

        var exception = Record.Exception(() => validator.ValidateJobPayload(JobPayloads.ValidJobPayloadFailed));

        Assert.Null(exception);
    }

    [Fact]
    public void ValidateJobPayload_MissingSchemaVersion_ThrowsValidationException()
    {
        var validator = CreateValidator();

        var exception = Assert.Throws<ValidationException>(() => 
            validator.ValidateJobPayload(JobPayloads.MissingSchemaVersion));

        Assert.Contains("schema_version", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateJobPayload_InvalidSchemaVersion_ThrowsValidationException()
    {
        var validator = CreateValidator();

        var exception = Assert.Throws<ValidationException>(() => 
            validator.ValidateJobPayload(JobPayloads.InvalidSchemaVersion));

        Assert.Contains("unsupported schema version", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("2.0", exception.Message);
    }

    [Fact]
    public void ValidateJobPayload_MissingDocumentId_ThrowsValidationException()
    {
        var validator = CreateValidator();

        var exception = Assert.Throws<ValidationException>(() => 
            validator.ValidateJobPayload(JobPayloads.MissingDocumentId));

        Assert.Contains("document_id", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateJobPayload_EmptyDocumentId_ThrowsValidationException()
    {
        var validator = CreateValidator();

        var exception = Assert.Throws<ValidationException>(() => 
            validator.ValidateJobPayload(JobPayloads.EmptyDocumentId));

        Assert.Contains("document_id", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateJobPayload_InvalidStatus_ThrowsValidationException()
    {
        var validator = CreateValidator();

        var exception = Assert.Throws<ValidationException>(() => 
            validator.ValidateJobPayload(JobPayloads.InvalidStatus));

        Assert.Contains("status", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateJobPayload_MalformedJobId_ThrowsValidationException()
    {
        var validator = CreateValidator();

        var exception = Assert.Throws<ValidationException>(() => 
            validator.ValidateJobPayload(JobPayloads.MalformedJobId));

        Assert.Contains("UUID", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateJobPayload_MissingJobId_ThrowsValidationException()
    {
        var validator = CreateValidator();

        var exception = Assert.Throws<ValidationException>(() => 
            validator.ValidateJobPayload(JobPayloads.MissingJobId));

        Assert.Contains("job_id", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateJobPayload_NullPayload_ThrowsValidationException()
    {
        var validator = CreateValidator();

        var exception = Assert.Throws<ValidationException>(() => 
            validator.ValidateJobPayload(null!));

        Assert.Contains("cannot be null or empty", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateJobPayload_EmptyPayload_ThrowsValidationException()
    {
        var validator = CreateValidator();

        var exception = Assert.Throws<ValidationException>(() => 
            validator.ValidateJobPayload(string.Empty));

        Assert.Contains("cannot be null or empty", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Constructor_SchemaFileNotFound_ThrowsFileNotFoundException()
    {
        _mockConfiguration.Setup(c => c.GetSection(It.IsAny<string>()).Value)
            .Returns("/nonexistent/path/schema.json");

        var exception = Assert.Throws<FileNotFoundException>(() => 
            new JobSchemaValidator(_mockConfiguration.Object, _mockLogger.Object));

        Assert.Contains("schema file not found", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Constructor_MalformedSchemaJson_ThrowsJsonException()
    {
        var exception = Assert.Throws<Newtonsoft.Json.JsonException>(() => 
            new JobSchemaValidator(MockSchemaProvider.MalformedSchema, _mockLogger.Object));

        Assert.Contains("invalid schema file", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => 
            new JobSchemaValidator(MockSchemaProvider.ValidJobSchema, null!));

        Assert.Equal("logger", exception.ParamName);
    }

    [Fact]
    public void Constructor_NullSchemaJson_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() => 
            new JobSchemaValidator((string)null!, _mockLogger.Object));

        Assert.Contains("Schema JSON cannot be null or empty", exception.Message);
    }

    [Fact]
    public void Constructor_EmptySchemaJson_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() => 
            new JobSchemaValidator(string.Empty, _mockLogger.Object));

        Assert.Contains("Schema JSON cannot be null or empty", exception.Message);
    }
}
