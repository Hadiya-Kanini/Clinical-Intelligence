# Task - [TASK_012_001]

## Requirement Reference
- User Story: [us_012] (extracted from input)
- Story Location: [.propel/context/tasks/us_012/us_012.md]
- Acceptance Criteria: 
    - [Given a user is authenticated, When 15 minutes pass without any user activity, Then the session is automatically terminated.]
    - [Given a user performs any action (API call, navigation), When the action is processed, Then the session's last activity timestamp is updated.]
    - [Given a session exists, When the Backend API processes requests, Then it tracks session state server-side for revocation capability.]

## Task Overview
Implement server-side session tracking and inactivity timeout enforcement in the .NET API. This includes persisting session records on login, validating session state on each authenticated request, updating `LastActivityAt`, and revoking/terminating sessions after 15 minutes of inactivity.

## Dependent Tasks
- [US_011 - Implement JWT authentication with HttpOnly cookies]

## Impacted Components
- [MODIFY: Server/ClinicalIntelligence.Api/Program.cs]
- [MODIFY: Server/ClinicalIntelligence.Api/Domain/Models/Session.cs]
- [MODIFY: Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs]
- [CREATE: Server/ClinicalIntelligence.Api/Middleware/SessionTrackingMiddleware.cs]

## Implementation Plan
- [Extend the login flow to create a `Session` record in the database for each successful authentication, capturing `UserId`, `CreatedAt`, `ExpiresAt`, `LastActivityAt`, `UserAgent`, and `IpAddress`.]
- [Include a session identifier in the issued JWT (e.g., a `sid` claim using the created `Session.Id`) so subsequent requests can be linked back to server-side session state.]
- [Add a middleware that runs after `UseAuthentication()` and before endpoint execution to enforce server-side session validity:]
- [Reject requests if the session is missing, revoked, expired, or inactive beyond the 15-minute threshold; return a 401 with a specific error code (e.g., `session_expired`) suitable for frontend handling.]
- [If session is valid, update `LastActivityAt` and (optionally) slide `ExpiresAt` forward to `UtcNow + 15 minutes` to model inactivity timeout.]
- [Update `/api/v1/auth/logout` to revoke the current session server-side (set `IsRevoked = true`) using the `sid` claim.]
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Create and persist `Session` on login; include `sid` claim in JWT; revoke session on logout; register `SessionTrackingMiddleware` in pipeline |
| CREATE | Server/ClinicalIntelligence.Api/Middleware/SessionTrackingMiddleware.cs | Enforce inactivity timeout and session revocation on authenticated requests; update `LastActivityAt` |
| MODIFY | Server/ClinicalIntelligence.Api/Domain/Models/Session.cs | Ensure required fields/constraints support inactivity timeout; confirm `LastActivityAt` usage |
| MODIFY | Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs | Ensure EF configuration for `Session` supports queries used by middleware and login/logout operations |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt-bearer?view=aspnetcore-8.0

## Build Commands
- dotnet build .\Server\ClinicalIntelligence.Api\ClinicalIntelligence.Api.csproj
- dotnet run --project .\Server\ClinicalIntelligence.Api\ClinicalIntelligence.Api.csproj

## Implementation Validation Strategy
- []

## Implementation Checklist
- [x] Add session creation logic to `/api/v1/auth/login` and persist `Session` with `LastActivityAt = UtcNow`.
- [x] Add `sid` claim to JWT and validate it is present for authenticated requests.
- [x] Implement `SessionTrackingMiddleware` to load session by `sid` and enforce inactivity timeout.
- [x] Ensure middleware updates `LastActivityAt` for successful authenticated requests.
- [x] Revoke session on `/api/v1/auth/logout`.
- [x] Ensure middleware returns a consistent 401 error payload/code when session is expired/revoked.
- [x] Confirm behavior does not depend on client clock; all comparisons use server `UtcNow`.
- [x] Add logging hooks (non-PHI) for session expiration/revocation decisions.
