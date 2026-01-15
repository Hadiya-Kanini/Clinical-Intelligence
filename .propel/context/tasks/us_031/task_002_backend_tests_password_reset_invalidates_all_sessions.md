# Task - [TASK_002]

## Requirement Reference
- User Story: [us_031]
- Story Location: [.propel/context/tasks/us_031/us_031.md]
- Acceptance Criteria: 
    - [Given session invalidation, When a previous session token is used, Then the API returns 401 Unauthorized.]
    - [Given session invalidation, When completed, Then the user must re-authenticate with the new password.]
    - [Given the reset process, When sessions are invalidated, Then the action is logged in the audit trail.]

## Task Overview
Add backend integration tests proving that a successful password reset invalidates all existing sessions for the user.

Because the API uses JWT cookies with a `sid` claim and enforces revocation via `ITokenRevocationStore` + `SessionTrackingMiddleware`, this test suite should focus on end-to-end behavior:
- establish multiple sessions for the same user
- execute `POST /api/v1/auth/reset-password`
- verify prior session cookies now receive `401 Unauthorized` on protected endpoints
- verify an audit event is stored for the invalidation

## Dependent Tasks
- [US_031 TASK_001 - Backend invalidate all sessions on password reset]

## Impacted Components
- [CREATE | Server/ClinicalIntelligence.Api.Tests/Integration/PasswordResetInvalidatesSessionsTests.cs | Integration tests verifying session revocation + audit logging on password reset]

## Implementation Plan
- Create integration test class following existing patterns (see `LogoutTokenRevocationTests`, `SessionInactivityTimeoutTests`):
  - use `WebApplicationFactory<Program>` and `WebApplicationFactoryClientOptions { HandleCookies = true }`
  - use `ApplicationDbContext` from DI scope to seed/inspect DB state
  - skip tests when the backing database is not available (pattern: `dbContext.Database.CanConnect()`)
- Test scenarios:
  - **Invalidate all sessions**:
    - create two clients (`client1`, `client2`), login both as same user to create two `Session` rows + two distinct auth cookies
    - verify both clients can call a protected endpoint (e.g., `GET /api/v1/ping`) before reset
    - generate a valid password reset token in DB (or call `POST /api/v1/auth/forgot-password` and query latest token record)
    - call `POST /api/v1/auth/reset-password` with the token and a new compliant password
    - assert both `client1` and `client2` now receive `401` on `GET /api/v1/ping`
  - **Audit event is created**:
    - query `AuditLogEvents` for the user and assert there is an event with expected `ActionType` used by the implementation
    - assert `Metadata` does not contain reset token or password
- Error shape validation:
  - when asserting `401`, if a JSON error body is returned, validate it matches the standardized error response structure (reuse patterns from `ErrorResponseIntegrationTests`).

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api.Tests/Integration/PasswordResetInvalidatesSessionsTests.cs | Integration tests for session invalidation after password reset and audit log creation |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Automated] Run backend test suite and confirm the new integration tests pass consistently.

## Implementation Checklist
- [x] Add integration test to create multiple sessions for a user (multiple clients) and verify both are invalidated after password reset
- [x] Assert `401` is returned when using old session cookies post-reset
- [x] Assert audit log event exists with expected action type
- [x] Assert audit metadata does not contain sensitive values (token/password)
