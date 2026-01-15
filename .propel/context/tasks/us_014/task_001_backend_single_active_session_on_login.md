# Task - [TASK_001]

## Requirement Reference
- User Story: [us_014]
- Story Location: [.propel/context/tasks/us_014/us_014.md]
- Acceptance Criteria: 
    - [Given a user is already authenticated with an active session, When they login from another device/browser, Then the previous session is invalidated.]

## Task Overview
Implement backend support for a single active session per user by revoking existing active sessions at login and issuing a new session-bound authentication context. Estimated effort: ~6-8 hours.

## Dependent Tasks
- [US_011 - Implement JWT authentication with HttpOnly cookies]
- [US_012 - Implement session tracking and inactivity timeout]

## Impacted Components
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Update `/api/v1/auth/login` to create a new `Session` record and revoke existing active sessions for the same user]
- [MODIFY | Server/ClinicalIntelligence.Api/Domain/Models/Session.cs | Extend session model if required to better support replacement tracking (e.g., revoked timestamp / revoked reason / replaced-by)]
- [MODIFY | Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs | Update EF model configuration if new session fields/indexes are introduced]
- [CREATE | Server/ClinicalIntelligence.Api/Migrations/* | EF migration for session schema changes, if any]

## Implementation Plan
- Implement “single active session” behavior during login:
  - Query active sessions for the authenticating user (e.g., `IsRevoked == false` and `ExpiresAt > DateTime.UtcNow`).
  - Mark all existing active sessions as revoked in the same request.
  - Create a new `Session` record capturing:
    - `UserId`
    - `CreatedAt`, `ExpiresAt`
    - `UserAgent` from `HttpContext.Request.Headers.UserAgent`
    - `IpAddress` from `HttpContext.Connection.RemoteIpAddress`
  - Save changes atomically (use a DB transaction to reduce race conditions on near-simultaneous logins).
- Bind the authentication token to the new session:
  - Ensure the JWT contains a claim identifying the session (e.g., session id claim such as `sid`).
  - Ensure the cookie issuance remains HttpOnly / Secure / SameSite settings aligned with US_011.
- Add minimal guardrails for the “legitimate device switch” edge case:
  - The new login becomes the active session immediately; old sessions are revoked (expected behavior).

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | In login endpoint, revoke existing active sessions for user and create a new `Session` record; include session identifier claim in JWT issuance |
| MODIFY | Server/ClinicalIntelligence.Api/Domain/Models/Session.cs | Add any required fields to support session replacement tracking and/or revocation details |
| MODIFY | Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs | Update EF mapping/index configuration if new session fields/indexes are added |
| CREATE | Server/ClinicalIntelligence.Api/Migrations/* | Add migration for any session schema updates |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt
- https://learn.microsoft.com/en-us/ef/core/saving/transactions

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Manual validation] Login twice using two separate browsers/profiles and confirm the first session becomes invalid.
- [Automated validation] Add or extend integration tests to cover session replacement flows (task_002 covers request-time enforcement and 401).

## Implementation Checklist
- [x] Identify "active session" predicate in DB (revoked + expiry)
- [x] Revoke any existing active sessions for the user at login
- [x] Create a new `Session` record at login with user agent + IP
- [x] Wrap revoke + create in a transaction to avoid race conditions
- [x] Add a session identifier claim to the issued JWT
- [x] Confirm cookie issuance remains aligned to US_011 security settings
- [x] Validate behavior for quick device switching (new login wins)
