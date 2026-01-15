using ClinicalIntelligence.Api.Contracts;
using ClinicalIntelligence.Api.Tests.TestData;
using System.Text.Json;
using Xunit;

namespace ClinicalIntelligence.Api.Tests.Contracts;

public class ApiErrorResponseTests
{
    [Fact]
    public void ApiErrorResponse_ValidData_CreatesSuccessfully()
    {
        var errorResponse = new ErrorResponse(
            ErrorResponseTestData.ErrorCodes.ValidationError,
            ErrorResponseTestData.ErrorMessages.InvalidInput,
            ErrorResponseTestData.ValidationDetails.SingleError
        );
        var apiErrorResponse = new ApiErrorResponse(errorResponse);

        Assert.NotNull(apiErrorResponse);
        Assert.NotNull(apiErrorResponse.Error);
        Assert.Equal(ErrorResponseTestData.ErrorCodes.ValidationError, apiErrorResponse.Error.Code);
        Assert.Equal(ErrorResponseTestData.ErrorMessages.InvalidInput, apiErrorResponse.Error.Message);
        Assert.NotNull(apiErrorResponse.Error.Details);
        Assert.IsType<string[]>(apiErrorResponse.Error.Details);
        Assert.Single(apiErrorResponse.Error.Details);
    }

    [Fact]
    public void ApiErrorResponse_ValidData_SerializesCorrectly()
    {
        var errorResponse = new ErrorResponse(
            ErrorResponseTestData.ErrorCodes.ValidationError,
            ErrorResponseTestData.ErrorMessages.InvalidInput,
            ErrorResponseTestData.ValidationDetails.SingleError
        );
        var apiErrorResponse = new ApiErrorResponse(errorResponse);

        var json = JsonSerializer.Serialize(apiErrorResponse);
        var jsonDoc = JsonDocument.Parse(json);
        var root = jsonDoc.RootElement;

        Assert.True(root.TryGetProperty("error", out var error));
        Assert.True(error.TryGetProperty("code", out var code));
        Assert.True(error.TryGetProperty("message", out var message));
        Assert.True(error.TryGetProperty("details", out var details));
        
        Assert.Equal(ErrorResponseTestData.ErrorCodes.ValidationError, code.GetString());
        Assert.Equal(ErrorResponseTestData.ErrorMessages.InvalidInput, message.GetString());
        Assert.Equal(JsonValueKind.Array, details.ValueKind);
    }

    [Fact]
    public void ApiErrorResponse_EmptyDetails_SerializesAsEmptyArray()
    {
        var errorResponse = new ErrorResponse(
            ErrorResponseTestData.ErrorCodes.NotFound,
            ErrorResponseTestData.ErrorMessages.ResourceNotFound,
            ErrorResponseTestData.ValidationDetails.EmptyArray
        );
        var apiErrorResponse = new ApiErrorResponse(errorResponse);

        var json = JsonSerializer.Serialize(apiErrorResponse);
        var jsonDoc = JsonDocument.Parse(json);
        var root = jsonDoc.RootElement;

        Assert.True(root.TryGetProperty("error", out var error));
        Assert.True(error.TryGetProperty("details", out var details));
        Assert.Equal(JsonValueKind.Array, details.ValueKind);
        Assert.Equal(0, details.GetArrayLength());
    }

    [Fact]
    public void ApiErrorResponse_MultipleDetails_PreservesAllItems()
    {
        var errorResponse = new ErrorResponse(
            ErrorResponseTestData.ErrorCodes.ValidationError,
            ErrorResponseTestData.ErrorMessages.InvalidInput,
            ErrorResponseTestData.ValidationDetails.MultipleErrors
        );
        var apiErrorResponse = new ApiErrorResponse(errorResponse);

        var json = JsonSerializer.Serialize(apiErrorResponse);
        var jsonDoc = JsonDocument.Parse(json);
        var root = jsonDoc.RootElement;

        Assert.True(root.TryGetProperty("error", out var error));
        Assert.True(error.TryGetProperty("details", out var details));
        Assert.Equal(JsonValueKind.Array, details.ValueKind);
        Assert.Equal(3, details.GetArrayLength());

        var detailsArray = details.EnumerateArray().Select(d => d.GetString()).ToArray();
        Assert.Equal(ErrorResponseTestData.ValidationDetails.MultipleErrors, detailsArray);
    }

    [Fact]
    public void ApiErrorResponse_NullMessage_HandlesGracefully()
    {
        var errorResponse = new ErrorResponse(
            ErrorResponseTestData.ErrorCodes.InternalServerError,
            null!,
            ErrorResponseTestData.ValidationDetails.EmptyArray
        );
        var apiErrorResponse = new ApiErrorResponse(errorResponse);

        Assert.NotNull(apiErrorResponse);
        Assert.Null(apiErrorResponse.Error.Message);
    }

    [Fact]
    public void ApiErrorResponse_VeryLongMessage_SerializesSuccessfully()
    {
        var longMessage = ErrorResponseTestData.ErrorMessages.VeryLongMessage;
        var errorResponse = new ErrorResponse(
            ErrorResponseTestData.ErrorCodes.InternalServerError,
            longMessage,
            ErrorResponseTestData.ValidationDetails.EmptyArray
        );
        var apiErrorResponse = new ApiErrorResponse(errorResponse);

        var json = JsonSerializer.Serialize(apiErrorResponse);
        var jsonDoc = JsonDocument.Parse(json);
        var root = jsonDoc.RootElement;

        Assert.True(root.TryGetProperty("error", out var error));
        Assert.True(error.TryGetProperty("message", out var message));
        Assert.Equal(longMessage, message.GetString());
        Assert.Equal(5000, message.GetString()!.Length);
    }

    [Fact]
    public void ErrorResponse_Properties_MatchConstructorParameters()
    {
        const string code = "test_code";
        const string message = "test message";
        var details = new[] { "detail1", "detail2" };

        var errorResponse = new ErrorResponse(code, message, details);

        Assert.Equal(code, errorResponse.Code);
        Assert.Equal(message, errorResponse.Message);
        Assert.Equal(details, errorResponse.Details);
    }

    [Fact]
    public void ApiErrorResponse_Deserialization_WorksCorrectly()
    {
        const string json = @"{
            ""error"": {
                ""code"": ""validation_error"",
                ""message"": ""Invalid input"",
                ""details"": [""Field X required""]
            }
        }";

        var apiErrorResponse = JsonSerializer.Deserialize<ApiErrorResponse>(json);

        Assert.NotNull(apiErrorResponse);
        Assert.NotNull(apiErrorResponse.Error);
        Assert.Equal("validation_error", apiErrorResponse.Error.Code);
        Assert.Equal("Invalid input", apiErrorResponse.Error.Message);
        Assert.Single(apiErrorResponse.Error.Details);
        Assert.Equal("Field X required", apiErrorResponse.Error.Details[0]);
    }
}
