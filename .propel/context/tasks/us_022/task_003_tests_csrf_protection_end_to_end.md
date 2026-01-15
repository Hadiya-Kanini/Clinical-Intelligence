# Task - [TASK_003]

## Requirement Reference
- User Story: [us_022]
- Story Location: [.propel/context/tasks/us_022/us_022.md]
- Acceptance Criteria: 
    - [Given a state-changing request (POST, PUT, DELETE), When processed, Then CSRF token validation is required.]
    - [Given a request without valid CSRF token, When submitted, Then the API returns 403 Forbidden.]
    - [Given CSRF protection, When implemented, Then tokens are generated per-session and validated server-side.]

## Task Overview
Add automated test coverage to verify CSRF token issuance and enforcement across the authenticated request lifecycle (token issuance, header requirement, 403 behavior). Estimated effort: ~5-7 hours.

## Dependent Tasks
- [TASK_001 - Backend CSRF token issuance and validation]

## Impacted Components
- [CREATE/MODIFY | Server/ClinicalIntelligence.Api.Tests/Integration/* | Integration tests for CSRF token issuance and enforcement]

## Implementation Plan
- Add integration tests for CSRF enforcement on a known state-changing endpoint:
  - Login to establish cookie-based auth.
  - Call `POST /api/v1/auth/logout` without CSRF header and assert `403`.
  - Call `GET /api/v1/auth/csrf` to retrieve token.
  - Call `POST /api/v1/auth/logout` with `X-CSRF-TOKEN` and assert `200`.
- Validate token is per-session and validated server-side:
  - Establish two independent sessions (separate clients), retrieve CSRF tokens, and assert tokens are different (per-session).
  - Attempt using token from session A with session B cookie and assert `403`.
- Validate expiry behavior:
  - Simulate expiration by manipulating persisted CSRF token metadata in DB (if stored there) and assert `403`.

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE/MODIFY | Server/ClinicalIntelligence.Api.Tests/Integration/CsrfProtectionTests.cs | Add integration tests for CSRF token issuance, enforcement, per-session isolation, and 403 behavior |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests
- https://cheatsheetseries.owasp.org/cheatsheets/Cross-Site_Request_Forgery_Prevention_Cheat_Sheet.html

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Automated] Run backend test suite and confirm CSRF tests are deterministic (no long sleeps).
- [Automated] Confirm tests pass with PostgreSQL available and are skipped gracefully if DB is unavailable (consistent with existing integration tests).

## Implementation Checklist
- [ ] Add test that unauthenticated CSRF token endpoint requires authorization (if applicable)
- [ ] Add test asserting state-changing request without CSRF header returns 403
- [ ] Add test asserting state-changing request with valid CSRF header returns 200
- [ ] Add per-session isolation test (token cannot be reused across sessions)
- [ ] Add expiry-path test for invalid/expired token
