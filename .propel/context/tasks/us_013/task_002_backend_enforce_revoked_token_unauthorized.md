# Task - [TASK_002]

## Requirement Reference
- User Story: [us_013]
- Story Location: [.propel/context/tasks/us_013/us_013.md]
- Acceptance Criteria: 
    - [Given a token has been invalidated, When any request uses that token, Then the API returns 401 Unauthorized.]

## Task Overview
Ensure that once a JWT/session is invalidated (revoked) during logout, any subsequent API request presenting that token is rejected with `401 Unauthorized`. This completes the server-side security guarantee that tokens cannot be replayed after logout.

## Dependent Tasks
- [US_011 - Implement JWT authentication with HttpOnly cookies]
- [US_012 - Implement session tracking and inactivity timeout]
- [TASK_001 - Backend secure logout token invalidation]

## Impacted Components
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Add revocation check to JWT authentication pipeline (e.g., JwtBearer events)]
- [CREATE | Server/ClinicalIntelligence.Api/Contracts/Auth/ITokenRevocationStore.cs | Abstraction for revocation lookup]
- [CREATE | Server/ClinicalIntelligence.Api/Services/Auth/DbTokenRevocationStore.cs | DB-backed implementation]
- [MODIFY | Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs | Query support for revoked tokens/sessions]
- [CREATE/MODIFY | Server/ClinicalIntelligence.Api.Tests/* | Add tests verifying revoked token returns 401]

## Implementation Plan
- Implement a revocation-check mechanism used during authentication:
  - For a revoked-token store: look up `jti` in DB and ensure it is not revoked.
  - For session-based revocation: resolve the current `Session` record and verify `IsRevoked == false` and `ExpiresAt` has not passed.
- Wire the check into the JWT bearer pipeline (recommended options):
  - Add `JwtBearerOptions.Events.OnTokenValidated` to call the revocation store.
  - If revoked, fail auth and ensure downstream sees `401 Unauthorized`.
- Ensure the revocation lookup is efficient:
  - Add DB index (migration-driven) for `jti` or `SessionId` lookup.
  - Ensure revoked-token records are bounded by expiry (cleanup strategy can be separate backlog item).
- Add tests:
  - Generate a token, revoke it via logout, then call a protected endpoint (e.g., `/api/v1/ping` or `/api/v1/auth/me`) with that token and assert `401`.
  - Ensure a non-revoked token still succeeds.

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Add revocation validation during JWT auth and enforce `401` for revoked tokens |
| CREATE | Server/ClinicalIntelligence.Api/Contracts/Auth/ITokenRevocationStore.cs | Interface for checking whether a token/session is revoked |
| CREATE | Server/ClinicalIntelligence.Api/Services/Auth/DbTokenRevocationStore.cs | DB implementation that queries revoked tokens/sessions |
| CREATE/MODIFY | Server/ClinicalIntelligence.Api.Tests/* | Integration tests ensuring revoked token cannot access protected endpoints |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwtbearer

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Automated test] Validate `401 Unauthorized` occurs when a revoked token is used.
- [Regression test] Validate normal authenticated requests continue to work.

## Implementation Checklist
- [x] Create `ITokenRevocationStore` contract and DB-backed implementation
- [x] Add revocation check during `OnTokenValidated` (or equivalent) in `Program.cs`
- [x] Ensure auth failure maps to `401 Unauthorized` response
- [x] Add test: logout -> call protected endpoint with same token -> assert `401`
- [x] Add test: non-revoked token -> call protected endpoint -> assert `200`
- [x] Confirm behavior for expired session/token remains `401` and is distinguishable only by status code (no sensitive detail)
