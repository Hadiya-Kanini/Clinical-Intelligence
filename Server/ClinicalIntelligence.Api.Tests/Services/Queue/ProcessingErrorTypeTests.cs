using ClinicalIntelligence.Api.Domain.Enums;
using Xunit;

namespace ClinicalIntelligence.Api.Tests.Services.Queue;

public class ProcessingErrorTypeTests
{
    [Fact]
    public void ProcessingErrorType_HasExpectedValues()
    {
        // Assert - verify all expected enum values exist
        Assert.Equal(0, (int)ProcessingErrorType.Unknown);
        Assert.Equal(1, (int)ProcessingErrorType.Transient);
        Assert.Equal(2, (int)ProcessingErrorType.Permanent);
        Assert.Equal(3, (int)ProcessingErrorType.NotFound);
        Assert.Equal(4, (int)ProcessingErrorType.Unauthorized);
        Assert.Equal(5, (int)ProcessingErrorType.ExternalService);
        Assert.Equal(6, (int)ProcessingErrorType.Database);
        Assert.Equal(7, (int)ProcessingErrorType.AiService);
    }
    
    [Fact]
    public void ProcessingErrorType_CanBeParsedFromString()
    {
        // Act & Assert
        Assert.True(Enum.TryParse<ProcessingErrorType>("Transient", out var transient));
        Assert.Equal(ProcessingErrorType.Transient, transient);
        
        Assert.True(Enum.TryParse<ProcessingErrorType>("Permanent", out var permanent));
        Assert.Equal(ProcessingErrorType.Permanent, permanent);
    }
    
    [Fact]
    public void ProcessingErrorType_ToStringReturnsName()
    {
        // Assert
        Assert.Equal("Transient", ProcessingErrorType.Transient.ToString());
        Assert.Equal("Permanent", ProcessingErrorType.Permanent.ToString());
        Assert.Equal("ExternalService", ProcessingErrorType.ExternalService.ToString());
    }
}
