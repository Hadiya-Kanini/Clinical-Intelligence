# Task - [TASK_001]

## Requirement Reference
- User Story: [us_013]
- Story Location: [.propel/context/tasks/us_013/us_013.md]
- Acceptance Criteria: 
    - [Given I am authenticated, When I trigger logout, Then the Backend API invalidates my session server-side.]
    - [Given logout is successful, When the response is sent, Then the HttpOnly JWT cookie is cleared.]

## Task Overview
Implement a secure logout flow on the Backend API that performs server-side invalidation (revocation) for the currently authenticated session/token and clears the auth cookie in the response. This provides a reliable security control so tokens can no longer be used after logout.

## Dependent Tasks
- [US_011 - Implement JWT authentication with HttpOnly cookies]
- [US_012 - Implement session tracking and inactivity timeout]

## Impacted Components
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Implement authenticated logout endpoint that revokes session/token and clears cookie]
- [MODIFY | Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs | Add persistence for token/session revocation if missing]
- [MODIFY | Server/ClinicalIntelligence.Api/Domain/Models/Session.cs | Add fields needed for token linkage/revocation (e.g., JWT ID / token hash) if required]
- [CREATE | Server/ClinicalIntelligence.Api/Domain/Models/RevokedToken.cs OR similar | Persist revoked JWT identifiers with expiry]
- [CREATE | Server/ClinicalIntelligence.Api/Migrations/* | EF migration for new/updated tables]
- [CREATE/MODIFY | Server/ClinicalIntelligence.Api.Tests/* | Add API tests proving logout revokes and cookie cleared]

## Implementation Plan
- Decide on the server-side invalidation strategy:
  - Store revoked JWT identifiers (recommended: `jti` claim) with an `ExpiresAt` timestamp, OR
  - Bind JWTs to `sessions` records (store `JwtId` on `Session`) and revoke the session record.
- Update login issuance (if not already done in US_011/US_012) so the server can identify the active session/token (e.g., ensure tokens include `jti` and a session is created).
- Update `POST /api/v1/auth/logout` to:
  - Require authorization
  - Extract the token identifier (`jti`) and user identity (`sub`) from `HttpContext.User`
  - Mark the session revoked OR insert into a revoked-token store
  - Return `200 OK` with `status = "logged_out"`
  - Clear the auth cookie in the response (set expiry in past + matching cookie options)
- Add EF migration(s) for any new/updated entities.
- Add test coverage validating:
  - Authenticated user can logout successfully
  - Logout results in persisted revocation state
  - Logout response clears the cookie

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Implement secure logout endpoint: authorize, revoke token/session server-side, clear auth cookie |
| CREATE | Server/ClinicalIntelligence.Api/Domain/Models/RevokedToken.cs | Entity to store revoked JWT identifiers (`jti`) with expiry (if using blacklist model) |
| MODIFY | Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs | Add DbSet/configuration for revocation persistence |
| MODIFY | Server/ClinicalIntelligence.Api/Domain/Models/Session.cs | Add token linkage fields if revocation is session-based |
| CREATE | Server/ClinicalIntelligence.Api/Migrations/* | Database migration for revocation/session changes |
| CREATE/MODIFY | Server/ClinicalIntelligence.Api.Tests/* | Tests for logout revocation and cookie clearing |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/security/authentication/cookie
- https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Integration test] Login, then call logout, verify persisted revocation state exists.
- [Integration test] Logout response includes cookie deletion for the auth cookie (matching name/path/samesite/secure).
- [Security validation] Ensure logout endpoint is authorized and does not leak details.

## Implementation Checklist
- [ ] Identify the server-side revocation model (revoked-token store vs session-based revocation)
- [ ] Implement persistence for revocation (entity + DbContext + migration)
- [ ] Update `POST /api/v1/auth/logout` to require auth and persist revocation
- [ ] Clear auth cookie in logout response (HttpOnly, Secure, SameSite settings aligned to US_011)
- [ ] Add/extend tests validating revocation persistence and cookie clearing
- [ ] Ensure revocation records have bounded lifetime (aligned to token expiry)
- [ ] Verify behavior when session already expired (idempotent logout)
