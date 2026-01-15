namespace ClinicalIntelligence.Api.Tests.TestData;

public static class ErrorResponseTestData
{
    public static class ErrorCodes
    {
        public const string ValidationError = "validation_error";
        public const string NotFound = "not_found";
        public const string Unauthorized = "unauthorized";
        public const string Forbidden = "forbidden";
        public const string Conflict = "conflict";
        public const string RateLimited = "rate_limited";
        public const string InternalServerError = "internal_server_error";
        public const string UnsupportedApiVersion = "unsupported_api_version";
    }

    public static class ErrorMessages
    {
        public const string InvalidInput = "Invalid input provided.";
        public const string ResourceNotFound = "The requested resource was not found.";
        public const string UnauthorizedAccess = "Unauthorized access.";
        public const string ForbiddenAccess = "Access to this resource is forbidden.";
        public const string ResourceConflict = "The resource already exists.";
        public const string RateLimitExceeded = "Too many requests. Please try again later.";
        public const string UnexpectedError = "An unexpected error occurred.";
        public const string UnsupportedVersion = "The requested API version is not supported.";
        public static readonly string VeryLongMessage = new string('A', 5000);
    }

    public static class ValidationDetails
    {
        public static readonly string[] SingleError = new[] { "Field X is required" };
        public static readonly string[] MultipleErrors = new[] 
        { 
            "Name is required", 
            "Email is invalid", 
            "Age must be positive" 
        };
        public static readonly string[] EmptyArray = Array.Empty<string>();
    }

    public static class ExceptionMessages
    {
        public const string DatabaseConnectionFailed = "Database connection failed";
        public const string WithConnectionString = "Server=localhost;Database=TestDb;User=admin;Password=secret123";
        public const string WithEnvironmentVariable = "API_KEY=sk_test_12345";
        public const string GenericError = "An error occurred";
    }

    public static class SensitivePatterns
    {
        public static readonly string[] ConnectionStringPatterns = new[]
        {
            "Server=",
            "Database=",
            "Password=",
            "User Id=",
            "Connection String="
        };

        public static readonly string[] EnvironmentVariablePatterns = new[]
        {
            "API_KEY=",
            "SECRET=",
            "TOKEN=",
            "PASSWORD="
        };

        public static readonly string[] StackTracePatterns = new[]
        {
            "at System.",
            "at Microsoft.",
            "at ClinicalIntelligence.",
            "in \\",
            ":line "
        };
    }
}
