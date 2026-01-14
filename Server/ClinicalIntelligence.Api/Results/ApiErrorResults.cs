using ClinicalIntelligence.Api.Contracts;

namespace ClinicalIntelligence.Api.Results;

public static class ApiErrorResults
{
    private static IResult Create(int statusCode, string code, string message, IEnumerable<string>? details = null) =>
        global::Microsoft.AspNetCore.Http.Results.Json(
            new ApiErrorResponse(
                new ErrorResponse(
                    code,
                    message,
                    (details ?? Array.Empty<string>()).ToArray()
                )),
            contentType: "application/json",
            statusCode: statusCode
        );

    public static IResult BadRequest(string code, string message, IEnumerable<string>? details = null) =>
        Create(StatusCodes.Status400BadRequest, code, message, details);

    public static IResult Unauthorized(string code = "unauthorized", string message = "Unauthorized.", IEnumerable<string>? details = null) =>
        Create(StatusCodes.Status401Unauthorized, code, message, details);

    public static IResult Forbidden(string code = "forbidden", string message = "Forbidden.", IEnumerable<string>? details = null) =>
        Create(StatusCodes.Status403Forbidden, code, message, details);

    public static IResult NotFound(string code = "not_found", string message = "Not found.", IEnumerable<string>? details = null) =>
        Create(StatusCodes.Status404NotFound, code, message, details);

    public static IResult Conflict(string code = "conflict", string message = "Conflict.", IEnumerable<string>? details = null) =>
        Create(StatusCodes.Status409Conflict, code, message, details);

    public static IResult TooManyRequests(string code = "rate_limited", string message = "Too many requests.", IEnumerable<string>? details = null) =>
        Create(StatusCodes.Status429TooManyRequests, code, message, details);

    public static IResult UnsupportedApiVersion(string requestedVersion) =>
        BadRequest(
            "unsupported_api_version",
            "The requested API version is not supported.",
            new[] { $"requested_version:{requestedVersion}" }
        );

    public static IResult InternalServerError() =>
        Create(StatusCodes.Status500InternalServerError, "internal_server_error", "An unexpected error occurred.");
}
