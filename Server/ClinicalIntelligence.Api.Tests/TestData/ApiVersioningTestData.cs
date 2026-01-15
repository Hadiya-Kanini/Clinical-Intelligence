namespace ClinicalIntelligence.Api.Tests.TestData;

public static class ApiVersioningTestData
{
    public static class Endpoints
    {
        public const string VersionedPing = "/api/v1/ping";
        public const string VersionedAuthLogin = "/api/v1/auth/login";
        public const string VersionedAuthLogout = "/api/v1/auth/logout";
        public const string Health = "/health";
        public const string SwaggerUi = "/swagger/index.html";
        public const string SwaggerJson = "/swagger/v1/swagger.json";
    }

    public static class UnsupportedVersions
    {
        public const string V0Endpoint = "/api/v0/endpoint";
        public const string V2Ping = "/api/v2/ping";
        public const string V3Test = "/api/v3/test";
        public const string V10Endpoint = "/api/v10/endpoint";
    }

    public static class UnversionedRoutes
    {
        public const string ApiPing = "/api/ping";
        public const string AuthLogin = "/auth/login";
        public const string ApiTest = "/api/test";
    }

    public static class EdgeCases
    {
        public const string PingWithTrailingSlash = "/api/v1/ping/";
        public const string VersionPrefixOnly = "/api/v1";
        public const string VersionPrefixWithSlash = "/api/v1/";
        public const string MalformedVersionNonNumeric = "/api/vX/endpoint";
        public const string DoubleVersionPrefix = "/api/v1/api/v1/ping";
    }

    public static class CaseVariations
    {
        public const string UpperCase = "/API/V1/ping";
        public const string MixedCase = "/Api/v1/ping";
        public const string LowerCase = "/api/v1/ping";
    }

    public static class ExpectedErrorCodes
    {
        public const string UnsupportedApiVersion = "unsupported_api_version";
        public const string InvalidCredentials = "invalid_credentials";
        public const string BadRequest = "invalid_input";
    }

    public static class ExpectedStatusMessages
    {
        public const string Healthy = "Healthy";
        public const string Ok = "OK";
    }
}
