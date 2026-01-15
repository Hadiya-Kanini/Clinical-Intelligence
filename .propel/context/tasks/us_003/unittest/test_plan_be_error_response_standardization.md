# Unit Test Plan - Standardize API Error Response Format

## Requirement Reference
- **User Story**: us_003
- **Story Location**: `.propel/context/tasks/us_003/us_003.md`
- **Layer**: BE (Backend)
- **Related Test Plans**: None (standalone backend test plan)
- **Acceptance Criteria Covered**:
  - AC-1: Given a request fails due to invalid input, When the API returns a 4xx response (e.g., 400), Then the response body follows: `error.code` as a stable string identifier, `error.message` as a human-readable summary, `error.details` as an array (empty if not applicable)
  - AC-2: Given a request is unauthorized or forbidden, When the API returns 401/403, Then it returns the same standardized error shape
  - AC-3: Given a request exceeds rate limits, When the API returns 429, Then it returns the same standardized error shape
  - AC-4: Given an unexpected server error occurs, When the API returns 5xx, Then it returns the same standardized error shape and does not leak sensitive configuration values or stack traces

## Test Plan Overview
This test plan covers comprehensive unit testing for the standardized API error response format implementation across all backend components. The plan validates that all error responses conform to the `{ "error": { "code": "string", "message": "string", "details": [] } }` structure for 4xx and 5xx status codes, ensuring consistent error handling across the API surface while preventing sensitive information leakage.

The test suite includes:
1. **Contract validation** - Verify `ApiErrorResponse` contract structure and serialization
2. **Result helper testing** - Validate `ApiErrorResults` factory methods for all HTTP error codes
3. **Middleware testing** - Test `ApiExceptionMiddleware` exception handling and sanitization
4. **Integration testing** - End-to-end validation of error responses across endpoints

## Dependent Tasks
- `.propel/context/tasks/us_002/task_001_implement_api_versioning_convention.md` (TASK_001) - Must be completed first as error standardization builds on versioning middleware

## Components Under Test

| Component | Type | File Path | Responsibilities |
|-----------|------|-----------|------------------|
| ApiErrorResponse | Record/Contract | `Server/ClinicalIntelligence.Api/Contracts/ApiErrorResponse.cs` | Defines standardized error response structure with `code`, `message`, and `details` properties |
| ApiErrorResults | Static Helper Class | `Server/ClinicalIntelligence.Api/Results/ApiErrorResults.cs` | Factory methods to generate standardized `IResult` error responses for 400/401/403/404/409/429/500 status codes |
| ApiExceptionMiddleware | Middleware | `Server/ClinicalIntelligence.Api/Middleware/ApiExceptionMiddleware.cs` | Catches unhandled exceptions and returns sanitized 500 error responses without stack traces or secrets |
| Program.cs (Error Registration) | Configuration | `Server/ClinicalIntelligence.Api/Program.cs` | Registers exception middleware and replaces unsupported version error responses |

## Test Cases

| Test-ID | Type | Description | Given | When | Then | Assertions |
|---------|------|-------------|-------|------|------|------------|
| **ApiErrorResponse Contract Tests** |
| TC-001 | positive | Verify error response contract structure | Valid error data with code, message, and details | ApiErrorResponse is instantiated | Object created successfully | Properties match input values, details is array type |
| TC-002 | positive | Verify error response JSON serialization | ApiErrorResponse with all fields populated | Object is serialized to JSON | JSON matches expected structure | JSON contains `error.code`, `error.message`, `error.details` as array |
| TC-003 | positive | Verify error response with empty details array | ApiErrorResponse with empty details | Object is serialized to JSON | Details serialized as empty array | `error.details` is `[]` not null |
| TC-004 | positive | Verify error response with multiple details | ApiErrorResponse with 3 detail strings | Object is serialized to JSON | All details preserved in array | `error.details` contains all 3 items in order |
| EC-001 | edge_case | Verify error response with null message | ApiErrorResponse with null message | Object is created | Handles null gracefully | No exception thrown, message is null or empty string |
| EC-002 | edge_case | Verify error response with very long message | ApiErrorResponse with 5000 char message | Object is serialized | Serialization succeeds | JSON valid, message preserved |
| **ApiErrorResults Helper Tests** |
| TC-005 | positive | BadRequest returns 400 with standardized error | Error code and message provided | `ApiErrorResults.BadRequest()` called | Returns IResult with 400 status | Status code is 400, body matches error structure |
| TC-006 | positive | Unauthorized returns 401 with standardized error | Error code and message provided | `ApiErrorResults.Unauthorized()` called | Returns IResult with 401 status | Status code is 401, body matches error structure |
| TC-007 | positive | Forbidden returns 403 with standardized error | Error code and message provided | `ApiErrorResults.Forbidden()` called | Returns IResult with 403 status | Status code is 403, body matches error structure |
| TC-008 | positive | NotFound returns 404 with standardized error | Error code and message provided | `ApiErrorResults.NotFound()` called | Returns IResult with 404 status | Status code is 404, body matches error structure |
| TC-009 | positive | Conflict returns 409 with standardized error | Error code and message provided | `ApiErrorResults.Conflict()` called | Returns IResult with 409 status | Status code is 409, body matches error structure |
| TC-010 | positive | TooManyRequests returns 429 with standardized error | Error code and message provided | `ApiErrorResults.TooManyRequests()` called | Returns IResult with 429 status | Status code is 429, body matches error structure |
| TC-011 | positive | InternalServerError returns 500 with standardized error | Error code and message provided | `ApiErrorResults.InternalServerError()` called | Returns IResult with 500 status | Status code is 500, body matches error structure |
| TC-012 | positive | Verify Content-Type is application/json | Any error result created | Result is returned | Content-Type header set | Content-Type is `application/json` |
| TC-013 | positive | Verify details array populated with validation errors | Multiple validation error details | `BadRequest()` called with details | Details array contains all errors | `error.details` array has all validation messages |
| EC-003 | edge_case | Verify error result with empty code string | Empty string as error code | Any error result method called | Handles empty code | No exception, code is empty string |
| EC-004 | edge_case | Verify error result with null details | Null passed as details parameter | Error result method called | Defaults to empty array | `error.details` is `[]` not null |
| **ApiExceptionMiddleware Tests** |
| TC-014 | positive | Middleware catches unhandled exception | Request throws unexpected exception | Middleware processes exception | Returns 500 with standardized error | Status 500, error structure valid, no stack trace |
| TC-015 | positive | Middleware sanitizes exception messages | Exception with sensitive data in message | Middleware processes exception | Sensitive data removed | Response contains generic message, no connection strings/secrets |
| TC-016 | positive | Middleware does not leak stack traces | Exception thrown in controller | Middleware catches exception | Stack trace not in response | Response body contains no stack trace lines |
| TC-017 | positive | Middleware does not leak environment variables | Exception references env variables | Middleware processes exception | Env vars not in response | No environment variable values in response |
| TC-018 | positive | Middleware passes through non-exception responses | Successful request (200 OK) | Middleware invoked | Request passes through | Response unchanged, middleware transparent |
| TC-019 | positive | Middleware logs exception details | Exception thrown | Middleware catches exception | Exception logged | Logger called with exception details |
| TC-020 | negative | Middleware handles null HttpContext | Middleware invoked with null context | Middleware processes request | Handles gracefully | No NullReferenceException thrown |
| ES-001 | error | Middleware handles exception during exception handling | Exception thrown while building error response | Middleware catches nested exception | Fallback error returned | Returns minimal 500 response |
| ES-002 | error | Middleware handles JsonException during serialization | Error response fails to serialize | Middleware attempts response | Fallback to plain text | Returns text/plain 500 response |
| **Integration Tests - End-to-End Error Response Validation** |
| TC-021 | positive | Unsupported API version returns standardized error | Request to `/api/v2/ping` | API returns error | Standardized 400 error returned | Body matches `{ error: { code, message, details } }`, code is `unsupported_api_version` |
| TC-022 | positive | Unauthorized endpoint returns standardized error | Request to protected endpoint without auth | API returns 401 | Standardized error returned | Body matches error structure, status 401 |
| TC-023 | positive | Validation failure returns standardized error with details | Invalid request payload sent | API validates and rejects | 400 with populated details array | `error.details` contains validation failure messages |
| TC-024 | positive | Rate limit exceeded returns standardized error | Requests exceed rate limit threshold | API rate limiter triggers | 429 with standardized error | Status 429, error structure valid |
| TC-025 | positive | Server exception returns standardized error | Endpoint throws unhandled exception | Exception middleware catches | 500 with sanitized error | No stack trace, no secrets, error structure valid |
| TC-026 | positive | Multiple simultaneous validation failures populate details | Request with 3 validation errors | API validates request | All errors in details array | `error.details` has 3 entries |
| EC-005 | edge_case | Database unavailable returns consistent error code | DB connection fails | Endpoint attempts DB access | Standardized 500 error | `error.code` is consistent (e.g., `internal_server_error`) |
| EC-006 | edge_case | External API timeout returns standardized error | External API call times out | Endpoint handles timeout | Standardized 500 or 503 error | Error structure valid, no external API details leaked |
| EC-007 | edge_case | Model binding failure returns standardized error | Malformed JSON in request body | Framework model binding fails | Standardized 400 error | Error structure valid, details explain binding failure |

## Expected Changes

| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | `Server/ClinicalIntelligence.Api.Tests/Contracts/ApiErrorResponseTests.cs` | Unit tests for ApiErrorResponse contract validation and serialization |
| CREATE | `Server/ClinicalIntelligence.Api.Tests/Results/ApiErrorResultsTests.cs` | Unit tests for ApiErrorResults factory methods (400/401/403/404/409/429/500) |
| CREATE | `Server/ClinicalIntelligence.Api.Tests/Middleware/ApiExceptionMiddlewareTests.cs` | Unit tests for exception middleware sanitization and error handling |
| CREATE | `Server/ClinicalIntelligence.Api.Tests/Integration/ErrorResponseIntegrationTests.cs` | End-to-end integration tests validating error responses across endpoints |
| CREATE | `Server/ClinicalIntelligence.Api.Tests/TestData/ErrorResponseTestData.cs` | Test data fixtures for error scenarios (exceptions, validation errors, etc.) |
| CREATE | `Server/ClinicalIntelligence.Api.Tests/Mocks/MockExceptionEndpoint.cs` | Mock endpoint that throws exceptions for middleware testing |

## Mocking Strategy

| Dependency | Mock Type | Mock Behavior | Return Value |
|------------|-----------|---------------|--------------|
| ILogger<ApiExceptionMiddleware> | Mock (Moq) | Verify logging calls during exception handling | Void (verify invocation only) |
| HttpContext | Mock (Moq) | Simulate request/response for middleware testing | Configured HttpContext with Request/Response streams |
| RequestDelegate | Mock (Moq) | Simulate next middleware in pipeline | Throws exception or completes successfully |
| IConfiguration | Mock (Moq) | Provide configuration values for testing | Returns test configuration values |
| DbContext | Mock (Moq) | Simulate database failures for error scenarios | Throws DbUpdateException or connection errors |
| HttpClient | Mock (Moq) | Simulate external API failures | Throws HttpRequestException or timeout |

## Test Data

| Scenario | Input Data | Expected Output |
|----------|------------|-----------------|
| Valid error response | `{ code: "validation_error", message: "Invalid input", details: ["Field X required"] }` | Serialized JSON with all fields |
| Empty details array | `{ code: "not_found", message: "Resource not found", details: [] }` | `{ "error": { "code": "not_found", "message": "Resource not found", "details": [] } }` |
| Multiple validation errors | `details: ["Name required", "Email invalid", "Age must be positive"]` | `error.details` array with 3 entries |
| Unhandled exception | `new InvalidOperationException("Database connection failed")` | `{ "error": { "code": "internal_server_error", "message": "An unexpected error occurred", "details": [] } }` |
| Exception with stack trace | Exception with full stack trace | Response body contains no stack trace text |
| Exception with connection string | Exception message contains `"Server=localhost;Database=..."` | Response contains sanitized message without connection string |
| Unsupported API version | Request to `/api/v99/endpoint` | `{ "error": { "code": "unsupported_api_version", "message": "API version v99 is not supported", "details": ["Supported versions: v1"] } }` |

## Test Commands
- **Run All Tests**: `dotnet test Server/ClinicalIntelligence.Api.Tests/ClinicalIntelligence.Api.Tests.csproj`
- **Run Specific Test Class**: `dotnet test --filter "FullyQualifiedName~ApiErrorResponseTests"`
- **Run with Coverage**: `dotnet test Server/ClinicalIntelligence.Api.Tests/ClinicalIntelligence.Api.Tests.csproj /p:CollectCoverage=true /p:CoverletOutputFormat=opencover`
- **Run Single Test**: `dotnet test --filter "FullyQualifiedName~ApiErrorResponse_ValidData_SerializesCorrectly"`
- **Run Integration Tests Only**: `dotnet test --filter "FullyQualifiedName~Integration"`
- **Run with Verbose Output**: `dotnet test --logger "console;verbosity=detailed"`

## Coverage Target
- **Line Coverage**: 95%
- **Branch Coverage**: 90%
- **Critical Paths**: 
  - Exception middleware exception handling logic (100% coverage required)
  - Error result factory methods for all HTTP status codes (100% coverage required)
  - Sanitization logic that removes stack traces and secrets (100% coverage required)
  - ApiErrorResponse serialization paths (100% coverage required)

## Documentation References
- **xUnit Framework**: https://xunit.net/docs/getting-started/netcore/cmdline
- **Moq Documentation**: https://github.com/moq/moq4/wiki/Quickstart
- **ASP.NET Core Testing**: https://learn.microsoft.com/aspnet/core/test/integration-tests
- **WebApplicationFactory**: https://learn.microsoft.com/aspnet/core/test/integration-tests#basic-tests-with-the-default-webapplicationfactory
- **Project Test Patterns**: `Server/ClinicalIntelligence.Api.Tests/Middleware/ApiVersioningMiddlewareTests.cs`
- **Existing Mock Examples**: `Server/ClinicalIntelligence.Api.Tests/Mocks/MockSchemaProvider.cs`
- **.NET Error Handling**: https://learn.microsoft.com/aspnet/core/fundamentals/error-handling
- **OWASP API Security**: https://owasp.org/www-project-api-security/

## Implementation Checklist
- [x] Create `ApiErrorResponseTests.cs` with contract validation tests (TC-001 to TC-004, EC-001, EC-002)
- [x] Create `ApiErrorResultsTests.cs` with factory method tests for all HTTP status codes (TC-005 to TC-013, EC-003, EC-004)
- [x] Create `ApiExceptionMiddlewareTests.cs` with exception handling and sanitization tests (TC-014 to TC-020, ES-001, ES-002)
- [x] Create `ErrorResponseIntegrationTests.cs` with end-to-end validation tests (TC-021 to TC-026, EC-005 to EC-007)
- [x] Create `ErrorResponseTestData.cs` with test fixtures for exceptions and error scenarios
- [x] Create `MockExceptionEndpoint.cs` for middleware testing
- [x] Implement positive test cases validating standardized error structure across all status codes
- [x] Implement negative test cases for null/invalid inputs and error conditions
- [x] Implement edge case tests for boundary conditions (long messages, empty arrays, null handling)
- [x] Implement error scenario tests for nested exceptions and serialization failures
- [x] Verify sanitization logic prevents stack trace and secret leakage (security-critical tests)
- [x] Run full test suite and validate coverage meets 95% line coverage target
- [x] Verify all critical paths (exception handling, sanitization, serialization) achieve 100% coverage
- [x] Execute integration tests against running API to validate end-to-end error responses
