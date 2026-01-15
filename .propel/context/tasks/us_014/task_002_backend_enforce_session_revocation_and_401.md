# Task - [TASK_002]

## Requirement Reference
- User Story: [us_014]
- Story Location: [.propel/context/tasks/us_014/us_014.md]
- Acceptance Criteria: 
    - [Given a session is invalidated due to new login, When the original session makes a request, Then the API returns 401 with a "session invalidated" message.]

## Task Overview
Enforce server-side session revocation at request time by validating the session identifier in the JWT against the `sessions` table. If the session is revoked or missing, return a standardized `401 Unauthorized` response that contains a clear `session invalidated` message. Estimated effort: ~6-8 hours.

## Dependent Tasks
- [US_011 - Implement JWT authentication with HttpOnly cookies]
- [US_012 - Implement session tracking and inactivity timeout]
- [TASK_001 - Backend single active session on login]

## Impacted Components
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Add session validation in JWT bearer pipeline (e.g., `JwtBearerEvents.OnTokenValidated` and/or challenge handling)]
- [MODIFY | Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs | Ensure efficient lookup for session validation (indexes already exist on `UserId`, `IsRevoked`, `ExpiresAt`)]
- [CREATE/MODIFY | Server/ClinicalIntelligence.Api.Tests/* | Add integration tests proving invalidated session requests return 401 with standardized error body]

## Implementation Plan
- Extend JWT auth validation to include server-side session checks:
  - On token validated, extract the session identifier claim (introduced in TASK_001).
  - Query `ApplicationDbContext.Sessions` for that session id and verify:
    - Session exists
    - `IsRevoked == false`
    - `ExpiresAt > DateTime.UtcNow`
  - If invalid, fail authentication and ensure the request results in `401`.
- Return a consistent error response body:
  - Ensure the response uses `ApiErrorResults.Unauthorized`.
  - Use an error `code` and `message` that meet the acceptance criteria (message includes `session invalidated`).
  - Ensure the response does not leak sensitive detail (e.g., no device identifiers).
- Add regression tests to ensure:
  - Valid sessions still work.
  - Missing cookie / malformed cookie still returns `401` as today.

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Add request-time validation that rejects revoked/expired/missing sessions and returns standardized `401` with `session invalidated` message |
| CREATE/MODIFY | Server/ClinicalIntelligence.Api.Tests/* | Add integration test: login twice (two clients) -> old client calls protected endpoint -> assert 401 + error message |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwtbearer

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Integration test] Use two `HttpClient` instances:
  - Client A logs in, hits `/api/v1/ping` => 200
  - Client B logs in as same user => 200
  - Client A hits `/api/v1/ping` again => 401 with `session invalidated` message
- [Regression] Existing cookie auth tests remain green.

## Implementation Checklist
- [x] Extract session id claim during JWT validation
- [x] Query DB for session record and validate not revoked + not expired
- [x] Ensure auth failure maps to `401 Unauthorized`
- [x] Ensure response body includes a `session invalidated` message
- [ ] Add integration tests for “login twice invalidates previous session”
- [ ] Add regression test for valid session still returns 200
- [x] Validate behavior for near-simultaneous logins (document expected outcome)
