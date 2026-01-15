# Task - [TASK_002]

## Requirement Reference
- User Story: [us_033]
- Story Location: [.propel/context/tasks/us_033/us_033.md]
- Acceptance Criteria: 
    - [Given role definitions, When implemented, Then they are enforced at both API and UI levels.]
- Edge Cases:
    - [What happens when a user's role is changed mid-session?]
    - [How does the system handle requests to endpoints outside the user's role permissions?]

## Task Overview
Implement a safe, deterministic behavior for the edge case where a user's role is changed while they still have an active session.

Goal: ensure that a user does not retain access beyond their current role assignment.

## Dependent Tasks
- [US_011] (JWT authentication with HttpOnly cookies)
- [US_033 TASK_001] (Backend RBAC policies and endpoint protection)

## Impacted Components
- [MODIFY | Server/ClinicalIntelligence.Api/Middleware/SessionTrackingMiddleware.cs | Detect role mismatch between JWT claims and database role, and invalidate session]
- [MODIFY | Server/ClinicalIntelligence.Api/Services/Auth/DbTokenRevocationStore.cs | Reuse revocation behavior for session invalidation on role mismatch]
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Ensure middleware ordering supports authentication -> session tracking -> authorization]
- [MODIFY | Server/ClinicalIntelligence.Api/Domain/Models/Session.cs | Ensure session data supports invalidation scenarios (no schema change expected)]

## Implementation Plan
- Detect role mismatch on authenticated requests:
  - In `SessionTrackingMiddleware`, after loading the `Session`, load the corresponding `User` role using `session.UserId`.
  - Extract the role from the authenticated principal (e.g., `ClaimTypes.Role` or `role`).
  - If the role in DB does not match the role in the JWT:
    - Mark the session as revoked (`session.IsRevoked = true`) and persist.
    - Return `401` with an error code aligned to existing behavior (recommended: reuse `session_invalidated` to leverage existing UI flows).
- Ensure consistent response semantics:
  - Mid-session role update should be treated as a session invalidation event.
  - Subsequent requests must be blocked.
- Auditability:
  - Emit an audit log event (if available) indicating role change invalidated an active session, without exposing sensitive information.

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Middleware/SessionTrackingMiddleware.cs | Add database role verification; revoke session and return `401` when role claim differs from DB role |
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Confirm middleware ordering ensures role checks execute before protected endpoint logic |
| MODIFY | Server/ClinicalIntelligence.Api/Services/Auth/DbTokenRevocationStore.cs | Ensure revocation logic remains idempotent and usable for role mismatch invalidation |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://cheatsheetseries.owasp.org/cheatsheets/Authorization_Cheat_Sheet.html
- https://learn.microsoft.com/en-us/aspnet/core/security/authorization/introduction

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Manual/API] Authenticate as Admin, then change the user to Standard (via DB or admin tooling) and confirm next request returns `401` with `session_invalidated`.
- [Manual/API] Authenticate as Standard, promote to Admin, confirm existing session is invalidated and user must re-login to gain Admin privileges.
- [Security] Verify that role changes cannot be abused to bypass authorization checks.

## Implementation Checklist
- [x] Load user role from DB during session validation
- [x] Compare DB role to JWT role claim for the active session
- [x] Revoke session and return `401` with consistent error code on mismatch
- [x] Add audit logging for role-mismatch invalidations
- [x] Validate behavior for both downgrade (Admin->Standard) and upgrade (Standard->Admin)
