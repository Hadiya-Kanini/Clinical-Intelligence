# Unit Test Plan - API Versioning Convention

## Requirement Reference
- **User Story**: us_002
- **Story Location**: .propel/context/tasks/us_002/us_002.md
- **Layer**: BE (Backend API - ASP.NET Core)
- **Related Test Plans**: None (standalone infrastructure test)
- **Acceptance Criteria Covered**:
  - AC-1: Given the Backend API exposes application endpoints, When an endpoint is implemented, Then its route is prefixed with `/api/v1/`.
  - AC-2: Given a client sends a request to an unversioned route (e.g., `/auth/login`), When the API receives the request, Then it responds with a not found/unsupported route result and does not serve application functionality from unversioned paths.
  - AC-3: Given a client sends a request to an unsupported version (e.g., `/api/v2/...`), When the API receives the request, Then it responds with a clear unsupported version error using the standardized error format.
  - AC-4: Given the API publishes documentation, When Swagger UI is generated, Then the documented endpoints reflect the `/api/v1/` prefix.

## Test Plan Overview
This test plan validates the API versioning middleware and route group configuration to ensure all application endpoints follow the `/api/v1/` convention, unversioned application routes are blocked, unsupported API versions return standardized errors, and Swagger documentation reflects the versioned endpoints correctly.

## Dependent Tasks
- TASK_001: Implement API versioning convention (completed)

## Components Under Test

| Component | Type | File Path | Responsibilities |
|-----------|------|-----------|------------------|
| API Versioning Middleware | Middleware | Server/ClinicalIntelligence.Api/Program.cs (lines 97-115) | Intercepts requests to `/api/v{N}` and validates version; returns error for unsupported versions |
| v1 Route Group | Route Group | Server/ClinicalIntelligence.Api/Program.cs (line 127) | Groups all v1 endpoints under `/api/v1/` prefix |
| Health Endpoint | Endpoint | Server/ClinicalIntelligence.Api/Program.cs (lines 123-125) | Unversioned operational endpoint at `/health` |
| Ping Endpoint | Endpoint | Server/ClinicalIntelligence.Api/Program.cs (lines 182-185) | Versioned test endpoint at `/api/v1/ping` |
| Swagger Configuration | Configuration | Server/ClinicalIntelligence.Api/Program.cs (lines 72-79, 117-121) | Generates OpenAPI documentation with versioned endpoints |

## Test Cases

| Test-ID | Type | Description | Given | When | Then | Assertions |
|---------|------|-------------|-------|------|------|------------|
| TC-001 | positive | Versioned endpoint v1 returns expected response | API running with v1 route group | GET request to `/api/v1/ping` | Endpoint is accessible (401 Unauthorized due to auth requirement) | StatusCode == Unauthorized |
| TC-002 | positive | Health endpoint accessible without version | API running | GET request to `/health` | Returns 200 OK with health status | StatusCode == OK, response contains `status: "Healthy"` |
| TC-003 | positive | Swagger UI accessible without version | API running | GET request to `/swagger/index.html` | Returns 200 OK with Swagger UI | StatusCode == OK |
| TC-004 | positive | Versioned endpoint case-insensitive (uppercase) | API running | GET request to `/API/V1/ping` | Endpoint is accessible | StatusCode == Unauthorized |
| TC-005 | positive | Versioned endpoint case-insensitive (mixed case) | API running | GET request to `/Api/v1/ping` | Endpoint is accessible | StatusCode == Unauthorized |
| TC-006 | negative | Unversioned application route returns not found | API running | GET request to `/api/ping` | Returns 404 Not Found | StatusCode == NotFound |
| TC-007 | negative | Unsupported version v2 returns error | API running | GET request to `/api/v2/ping` | Returns 400 Bad Request with standardized error | StatusCode == BadRequest, error.code == "unsupported_api_version" |
| TC-008 | negative | Unsupported version v3 returns error | API running | GET request to `/api/v3/test` | Returns 400 Bad Request with version details | StatusCode == BadRequest, error.code == "unsupported_api_version", details contains "v3" |
| TC-009 | negative | Unsupported version v0 returns error | API running | GET request to `/api/v0/endpoint` | Returns 400 Bad Request | StatusCode == BadRequest, error.code == "unsupported_api_version" |
| TC-010 | negative | Unsupported version v10 returns error | API running | GET request to `/api/v10/endpoint` | Returns 400 Bad Request | StatusCode == BadRequest, error.code == "unsupported_api_version" |
| EC-001 | edge_case | Versioned endpoint with trailing slash | API running | GET request to `/api/v1/ping/` | Endpoint is accessible | StatusCode == Unauthorized or NotFound (ASP.NET routing behavior) |
| EC-002 | edge_case | Version prefix without endpoint path | API running | GET request to `/api/v1` or `/api/v1/` | Returns appropriate response | StatusCode == NotFound or MethodNotAllowed |
| EC-003 | edge_case | Malformed version format (non-numeric) | API running | GET request to `/api/vX/endpoint` | Returns 404 Not Found (not caught by version middleware) | StatusCode == NotFound |
| EC-004 | edge_case | Double version prefix | API running | GET request to `/api/v1/api/v1/ping` | Returns 404 Not Found | StatusCode == NotFound |
| ES-001 | error | Swagger JSON reflects v1 prefix | API running | GET request to `/swagger/v1/swagger.json` | Swagger document contains `/api/v1/` paths | Response contains paths starting with `/api/v1/` |
| ES-002 | error | Health endpoint not prefixed in Swagger | API running | GET request to `/swagger/v1/swagger.json` | Swagger document contains `/health` without version | Response contains `/health` path |
| ES-003 | error | All application endpoints under v1 in Swagger | API running | Parse Swagger JSON | All non-operational endpoints have `/api/v1/` prefix | All paths except `/health`, `/swagger` start with `/api/v1/` |

## Expected Changes

| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api.Tests/Middleware/ApiVersioningMiddlewareTests.cs | Enhance existing tests with additional edge cases and Swagger validation |
| CREATE | Server/ClinicalIntelligence.Api.Tests/Configuration/SwaggerVersioningTests.cs | Unit tests for Swagger document validation |
| CREATE | Server/ClinicalIntelligence.Api.Tests/TestData/ApiVersioningTestData.cs | Test data fixtures for versioned endpoints |

## Mocking Strategy

| Dependency | Mock Type | Mock Behavior | Return Value |
|------------|-----------|---------------|--------------|
| WebApplicationFactory<Program> | Test fixture | Provides in-memory test server | HTTP client for integration tests |
| HttpClient | Created from factory | Makes HTTP requests to test server | HTTP responses |
| ApplicationDbContext | In-memory database | Provides test database context | Test data context |

## Test Data

| Scenario | Input Data | Expected Output |
|----------|------------|-----------------|
| Valid v1 endpoint | `GET /api/v1/ping` | `401 Unauthorized` (auth required) |
| Unversioned health | `GET /health` | `200 OK { "status": "Healthy" }` |
| Unsupported v2 | `GET /api/v2/ping` | `400 BadRequest { "error": { "code": "unsupported_api_version", "message": "...", "details": ["v2"] } }` |
| Unsupported v3 | `GET /api/v3/test` | `400 BadRequest { "error": { "code": "unsupported_api_version", "details": ["v3"] } }` |
| Unversioned app route | `GET /api/ping` | `404 NotFound` |
| Case insensitive | `GET /API/V1/ping` | `401 Unauthorized` |

## Test Commands
- **Run Tests**: `dotnet test Server/ClinicalIntelligence.Api.Tests/ClinicalIntelligence.Api.Tests.csproj --filter "FullyQualifiedName~ApiVersioning"`
- **Run with Coverage**: `dotnet test Server/ClinicalIntelligence.Api.Tests/ClinicalIntelligence.Api.Tests.csproj --collect:"XPlat Code Coverage" --filter "FullyQualifiedName~ApiVersioning"`
- **Run Single Test**: `dotnet test Server/ClinicalIntelligence.Api.Tests/ClinicalIntelligence.Api.Tests.csproj --filter "FullyQualifiedName~ApiVersioningMiddlewareTests.VersionedEndpoint_V1_ReturnsOk"`
- **Run All Backend Tests**: `dotnet test Server/ClinicalIntelligence.Api.Tests/ClinicalIntelligence.Api.Tests.csproj`

## Coverage Target
- **Line Coverage**: 95%
- **Branch Coverage**: 90%
- **Critical Paths**: 
  - API versioning middleware (lines 97-115 in Program.cs) - 100% coverage
  - Route group configuration (line 127 in Program.cs) - 100% coverage
  - All version validation branches - 100% coverage
  - Swagger configuration for versioned endpoints - 100% coverage

## Documentation References
- **Framework Docs**: https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests
- **Project Test Patterns**: Server/ClinicalIntelligence.Api.Tests/Middleware/ApiVersioningMiddlewareTests.cs
- **Mocking Guide**: https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests#customize-webapplicationfactory
- **xUnit Documentation**: https://xunit.net/docs/getting-started/netcore/cmdline
- **ASP.NET Core Routing**: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/routing
- **Minimal APIs**: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis

## Implementation Checklist
- [x] Create test file structure per Expected Changes
- [x] Set up test data fixtures per Test Data section
- [x] Configure WebApplicationFactory for integration tests
- [x] Implement positive test cases (TC-001 to TC-005)
- [x] Implement negative test cases (TC-006 to TC-010)
- [x] Implement edge case tests (EC-001 to EC-004)
- [x] Implement Swagger validation tests (ES-001 to ES-003)
- [x] Run test suite and validate coverage meets 95% target
- [x] Validate all acceptance criteria are covered by tests

## Additional Test Scenarios

### Swagger Document Validation Tests
These tests ensure the OpenAPI specification correctly reflects the versioning convention:

| Test-ID | Description | Validation |
|---------|-------------|------------|
| SV-001 | All application endpoints have `/api/v1/` prefix | Parse Swagger JSON, verify all paths (except `/health`, `/swagger`) start with `/api/v1/` |
| SV-002 | Swagger info version matches API version | Verify `info.version` is "1.0.0" |
| SV-003 | Operational endpoints excluded from versioning | Verify `/health` exists without `/api/v1/` prefix |
| SV-004 | Swagger UI endpoint accessible | GET `/swagger/index.html` returns 200 OK |
| SV-005 | Swagger JSON endpoint accessible | GET `/swagger/v1/swagger.json` returns valid JSON |

### Route Group Configuration Tests
These tests validate the route group setup:

| Test-ID | Description | Validation |
|---------|-------------|------------|
| RG-001 | v1 route group accepts valid endpoints | Endpoints mapped to v1 group are accessible under `/api/v1/` |
| RG-002 | Endpoints outside v1 group not accessible at v1 path | Verify no endpoint leakage |
| RG-003 | Route group applies to all HTTP methods | Test GET, POST, PUT, DELETE, PATCH under `/api/v1/` |

## Test Execution Strategy

### Unit Test Execution Order
1. **Positive Tests First**: Validate core functionality works (TC-001 to TC-005)
2. **Negative Tests**: Verify error handling (TC-006 to TC-010)
3. **Edge Cases**: Test boundary conditions (EC-001 to EC-004)
4. **Swagger Validation**: Verify documentation (ES-001 to ES-003)

### Integration Test Approach
- Use `WebApplicationFactory<Program>` for in-memory test server
- Each test creates isolated HTTP client
- Tests are independent and can run in parallel
- No external dependencies required (self-contained)

### Coverage Analysis
- Focus on middleware logic (version detection and validation)
- Ensure all error paths are tested
- Validate case-insensitive routing behavior
- Confirm Swagger generation includes versioned paths

## Test Maintenance Guidelines

### When to Update Tests
- New API version added (e.g., v2): Add new positive tests for v2, update unsupported version tests
- New endpoint added: Verify it's under `/api/v1/` group
- Middleware logic changes: Update relevant test cases
- Error format changes: Update assertion expectations

### Test Naming Convention
Follow .NET convention: `MethodName_Condition_ExpectedResult()`
- Example: `VersionedEndpoint_V1_ReturnsOk()`
- Example: `UnsupportedVersion_V2_ReturnsBadRequest()`

### Assertion Best Practices
- Assert HTTP status code first
- Parse and validate JSON response structure
- Verify error codes match standardized format
- Check response headers when relevant
