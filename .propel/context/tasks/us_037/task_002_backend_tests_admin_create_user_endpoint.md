# Task - [TASK_002]

## Requirement Reference
- User Story: [us_037]
- Story Location: [.propel/context/tasks/us_037/us_037.md]
- Acceptance Criteria: 
    - [Given I am authenticated as Admin, When I submit a user creation request, Then a new user account is created.]
    - [Given user creation request, When submitted, Then all required fields are validated (FR-009l).]
    - [Given a non-admin user, When they attempt to access the endpoint, Then they receive 403 Forbidden.]
    - [Given successful creation, When completed, Then USER_CREATED event is logged in audit trail.]

## Task Overview
Add automated backend tests for the admin-only create user endpoint introduced in `TASK_001`. Tests must validate:
- successful creation path for Admin (including persistence)
- forbidden access for authenticated Standard user
- validation failures for missing/invalid fields
- duplicate email conflict behavior
- audit trail insertion of `USER_CREATED` event

This task is focused on API correctness and regression protection, using the existing `ClinicalIntelligence.Api.Tests` project and its integration-test patterns.

## Dependent Tasks
- [US_037 TASK_001 - Backend admin create Standard user endpoint]

## Impacted Components
- [CREATE | Server/ClinicalIntelligence.Api.Tests/AdminCreateUserEndpointTests.cs | Integration tests covering admin create-user endpoint authorization, validation, conflict, and audit logging]
- [MODIFY | Server/ClinicalIntelligence.Api.Tests/TestWebApplicationFactory.cs | (If needed) Extend seed data or helpers to support repeatable admin/standard authentication flows for tests]

## Implementation Plan
- Follow existing test conventions:
  - Use `IClassFixture<TestWebApplicationFactory<Program>>` to run against the SQLite-backed in-memory-ish test database.
  - Use `HttpClient` created with cookie handling enabled (via `CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true })`) to persist the JWT cookie across requests.
- Implement test cases:
  - **Admin success**:
    - login as seeded admin (`admin@example.com` / `AdminPassword123!` from `TestWebApplicationFactory`) to obtain cookie
    - call `POST /api/v1/admin/users` with a valid request body
    - assert success status (200/201 depending on implementation) and response includes created user id/email/role
    - open a DI scope and query `ApplicationDbContext.Users` to verify the user exists and `Role == "Standard"`
  - **Standard forbidden**:
    - login as seeded standard user (`test@example.com` / `TestPassword123!`)
    - call the endpoint and assert `403 Forbidden` using the standardized error format
  - **Validation failures (FR-009l)**:
    - invalid email format -> `400` with `invalid_input`
    - missing required field(s) -> `400` with `invalid_input`
    - weak password -> `400` with `invalid_input`
  - **Duplicate email conflict**:
    - attempt creation with `test@example.com` (already seeded)
    - assert `409 Conflict` and stable error code/message
  - **Audit trail**:
    - after a successful creation, query `ApplicationDbContext.AuditLogEvents` for an entry with:
      - `ActionType == "USER_CREATED"`
      - `ResourceType == "User"`
      - `ResourceId == <created user id>`
- Ensure tests avoid logging or asserting on secrets:
  - do not log plaintext passwords
  - do not store or print the access token; rely on cookie handling

**Focus on how to implement**

## Current Project State
- ✅ **COMPLETED** - All integration tests implemented and passing (9/9 tests pass)
- ✅ **PostgreSQL database** integration working correctly
- ✅ **Rate limiting** configured for test environment (100 attempts/60 seconds)
- ✅ **CSRF protection** properly handled in tests
- ✅ **All test scenarios** covered: success, authorization, validation, conflict, audit logging

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api.Tests/AdminCreateUserEndpointTests.cs | Integration tests covering admin create-user endpoint behavior (success, 403, 400, 409, audit logging) |
| MODIFY | Server/ClinicalIntelligence.Api.Tests/TestWebApplicationFactory.cs | Optional: add helper(s) or seed adjustments if tests require additional deterministic users |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests
- https://learn.microsoft.com/en-us/dotnet/api/system.net.http.json.httpclientjsonextensions

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Automated] Run the test suite and confirm:
  - the new tests fail when endpoint authorization/validation/audit behavior regresses
  - the new tests pass reliably with SQLite-backed test database
- [Coverage] Ensure tests cover the story’s key acceptance criteria, including the 403 and audit event.

## Implementation Checklist
- [x] Add `AdminCreateUserEndpointTests.cs` with integration tests using `WebApplicationFactory<Program>` (PostgreSQL)
- [x] Add helpers to login as admin/standard and preserve auth cookie with CSRF token handling
- [x] Assert admin success path creates a user with `Role = "Standard"` and expected response shape
- [x] Assert Standard user receives `403 Forbidden` (not `401`) when authenticated
- [x] Assert validation failures return `400` with standardized error payload
- [x] Assert duplicate email returns `409 Conflict` with stable error code/message
- [x] Assert `USER_CREATED` audit log event is persisted and linked to the created user
