# Unit Test Plan - Backend Contract Validation

## Requirement Reference
- **User Story**: us_001
- **Story Location**: .propel/context/tasks/us_001/us_001.md
- **Layer**: BE (Backend API - .NET)
- **Related Test Plans**: 
  - test_plan_worker_schema_validation.md
  - test_plan_infra_contract_structure.md
- **Acceptance Criteria Covered**:
  - AC-1: Given the Web UI integrates with the Backend API, When the Web UI calls backend endpoints, Then the API contract is explicitly defined (Swagger UI with OpenAPI specification) and the UI uses only versioned endpoints.
  - AC-2: Given a change is proposed to a contract (API schema or job schema), When a developer updates it, Then the change includes a versioning decision (backward compatible vs breaking) and documented migration notes.
  - Edge Case: What happens when the Web UI calls an endpoint that is not part of the published Swagger/OpenAPI contract?

## Test Plan Overview
This test plan validates the Backend API's contract enforcement mechanisms, including Swagger/OpenAPI configuration, versioned endpoint routing, job schema validation, and contract change process compliance. Tests ensure that the Backend API correctly publishes its contract via Swagger UI and properly validates job messages before enqueuing them to RabbitMQ.

## Dependent Tasks
- TASK_002: Establish contracts structure and guardrails
- TASK_003: Define backend to worker job schema
- TASK_004: Define backend API contract baseline

## Components Under Test

| Component | Type | File Path | Responsibilities |
|-----------|------|-----------|------------------|
| SwaggerConfiguration | service/middleware | Server/ClinicalIntelligence.Api/Program.cs | Configures Swagger UI and OpenAPI generation |
| ApiVersioningMiddleware | middleware | Server/ClinicalIntelligence.Api/Middleware/ApiVersioningMiddleware.cs | Enforces versioned endpoint routing |
| JobSchemaValidator | service | Server/ClinicalIntelligence.Api/Services/JobSchemaValidator.cs | Validates job payloads against JSON schema before enqueuing |
| ContractChangeValidator | utility | Server/ClinicalIntelligence.Api/Utilities/ContractChangeValidator.cs | Validates contract changes include versioning and migration notes |

## Test Cases

| Test-ID | Type | Description | Given | When | Then | Assertions |
|---------|------|-------------|-------|------|------|------------|
| TC-001 | positive | Swagger UI is accessible at /swagger endpoint | Backend API is running | GET request to /swagger | Returns 200 OK with Swagger UI HTML | Response contains "swagger-ui", status code 200 |
| TC-002 | positive | OpenAPI spec is generated and accessible | Backend API is running | GET request to /swagger/v1/swagger.json | Returns valid OpenAPI 3.0 JSON | Valid JSON, contains "openapi": "3.0.0", "info", "paths" |
| TC-003 | positive | Versioned endpoint routing works correctly | API has v1 endpoints defined | GET request to /api/v1/ping | Returns 200 OK | Status code 200, response contains "status": "OK" |
| TC-004 | positive | Valid job payload passes schema validation | JobSchemaValidator initialized | ValidateJobPayload called with valid payload | No exception thrown | Method completes successfully |
| TC-005 | negative | Invalid job payload fails schema validation | JobSchemaValidator initialized | ValidateJobPayload called with missing required field | Throws ValidationException | Exception message contains "required field", field name |
| TC-006 | negative | Job payload with invalid schema version fails | JobSchemaValidator initialized | ValidateJobPayload with schema_version "2.0" | Throws ValidationException | Exception message contains "unsupported schema version" |
| TC-007 | negative | Job payload missing document_id fails validation | JobSchemaValidator initialized | ValidateJobPayload without document_id | Throws ValidationException | Exception message contains "document_id is required" |
| TC-008 | negative | Job payload with invalid status enum fails | JobSchemaValidator initialized | ValidateJobPayload with status "invalid_status" | Throws ValidationException | Exception message contains "invalid status value" |
| TC-009 | negative | Non-versioned endpoint returns 400 Bad Request | API running | GET request to /api/ping (no version) | Returns 400 Bad Request | Status code 400, error message "API version required" |
| EC-001 | edge_case | Job payload with null payload field succeeds | JobSchemaValidator initialized | ValidateJobPayload with payload: null | No exception thrown | Validation passes (payload is optional) |
| EC-002 | edge_case | Job payload with empty string document_id fails | JobSchemaValidator initialized | ValidateJobPayload with document_id: "" | Throws ValidationException | Exception message contains "document_id cannot be empty" |
| EC-003 | edge_case | Job payload with malformed UUID job_id fails | JobSchemaValidator initialized | ValidateJobPayload with job_id: "not-a-uuid" | Throws ValidationException | Exception message contains "invalid UUID format" |
| ES-001 | error | Schema file not found throws clear error | Schema file missing | JobSchemaValidator constructor called | Throws FileNotFoundException | Exception message contains schema file path |
| ES-002 | error | Malformed schema JSON throws clear error | Schema file has invalid JSON | JobSchemaValidator constructor called | Throws JsonException | Exception message contains "invalid schema file" |

## Expected Changes

| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api.Tests/Services/JobSchemaValidatorTests.cs | Unit tests for JobSchemaValidator |
| CREATE | Server/ClinicalIntelligence.Api.Tests/Middleware/ApiVersioningMiddlewareTests.cs | Unit tests for API versioning middleware |
| CREATE | Server/ClinicalIntelligence.Api.Tests/Configuration/SwaggerConfigurationTests.cs | Unit tests for Swagger configuration |
| CREATE | Server/ClinicalIntelligence.Api.Tests/TestData/JobPayloads.cs | Test data fixtures for job payloads |
| CREATE | Server/ClinicalIntelligence.Api.Tests/Mocks/MockSchemaProvider.cs | Mock for schema file loading |

## Mocking Strategy

| Dependency | Mock Type | Mock Behavior | Return Value |
|------------|-----------|---------------|--------------|
| IFileSystem | mock | When ReadAllText called for schema file | Return valid JSON schema string |
| ILogger<JobSchemaValidator> | mock | When LogError/LogWarning called | Verify logging calls |
| IConfiguration | mock | When GetValue called for schema path | Return test schema path |
| HttpContext | mock | For middleware testing | Return mock request/response |

## Test Data

| Scenario | Input Data | Expected Output |
|----------|------------|-----------------|
| Valid job payload | `{ "schema_version": "1.0", "job_id": "00000000-0000-0000-0000-000000000000", "document_id": "doc-123", "status": "pending", "payload": {} }` | No exception |
| Missing schema_version | `{ "job_id": "...", "document_id": "doc-123", "status": "pending" }` | ValidationException: "schema_version is required" |
| Invalid status | `{ "schema_version": "1.0", "job_id": "...", "document_id": "doc-123", "status": "invalid" }` | ValidationException: "invalid status value" |
| Missing document_id | `{ "schema_version": "1.0", "job_id": "...", "status": "pending" }` | ValidationException: "document_id is required" |
| Unsupported version | `{ "schema_version": "2.0", "job_id": "...", "document_id": "doc-123", "status": "pending" }` | ValidationException: "unsupported schema version: 2.0" |

## Test Commands
- **Run Tests**: `dotnet test Server/ClinicalIntelligence.Api.Tests/ClinicalIntelligence.Api.Tests.csproj --filter "FullyQualifiedName~JobSchemaValidator"`
- **Run with Coverage**: `dotnet test Server/ClinicalIntelligence.Api.Tests/ClinicalIntelligence.Api.Tests.csproj --collect:"XPlat Code Coverage" --filter "FullyQualifiedName~JobSchemaValidator"`
- **Run Single Test**: `dotnet test --filter "FullyQualifiedName~JobSchemaValidatorTests.ValidateJobPayload_ValidPayload_Succeeds"`
- **Run All Backend Tests**: `dotnet test Server/ClinicalIntelligence.Api.Tests/ClinicalIntelligence.Api.Tests.csproj`

## Coverage Target
- **Line Coverage**: 90%
- **Branch Coverage**: 85%
- **Critical Paths**: 
  - JobSchemaValidator.ValidateJobPayload() - 100% coverage
  - ApiVersioningMiddleware.InvokeAsync() - 100% coverage
  - All exception handling paths - 100% coverage

## Documentation References
- **Framework Docs**: https://xunit.net/docs/getting-started/netcore/cmdline
- **Project Test Patterns**: Server/ClinicalIntelligence.Api.Tests/ApiStartupConfigurationTests.cs
- **Mocking Guide**: https://github.com/moq/moq4/wiki/Quickstart
- **JSON Schema Validation**: https://www.newtonsoft.com/json/help/html/JsonSchema.htm

## Implementation Checklist
- [ ] Create test file structure per Expected Changes
- [ ] Set up test data fixtures per Test Data section (JobPayloads.cs)
- [ ] Configure mocking dependencies per Mocking Strategy (MockSchemaProvider)
- [ ] Implement positive test cases (TC-001 to TC-004)
- [ ] Implement negative test cases (TC-005 to TC-009)
- [ ] Implement edge case tests (EC-001 to EC-003)
- [ ] Implement error scenario tests (ES-001 to ES-002)
- [ ] Run test suite and validate coverage meets 90% target
