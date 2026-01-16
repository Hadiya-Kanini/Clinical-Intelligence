# Task - [TASK_002]

## Requirement Reference
- User Story: [us_035]
- Story Location: [.propel/context/tasks/us_035/us_035.md]
- Acceptance Criteria: 
    - [Given authorization checks, When performed, Then they occur on every request (not just initial load).]
- Edge Cases:
    - [How does the system handle cached admin pages after role downgrade?]

## Task Overview
Ensure server-side authorization decisions remain correct if a user’s role changes mid-session (e.g., Admin downgraded to Standard). Because JWT role claims are issued at login time, relying solely on token claims can allow stale privileges.

This task hardens backend enforcement by ensuring that on each authenticated request, the effective role used for authorization is consistent with the current persisted user role. If the role has changed, the request must no longer be treated as Admin.

## Dependent Tasks
- [US_033] (Define Admin and Standard User roles)
- [US_035 TASK_001] (Backend admin-only endpoint authorization and 403 behavior)

## Impacted Components
- [MODIFY | Server/ClinicalIntelligence.Api/Middleware/SessionTrackingMiddleware.cs | Validate that token role aligns with current user role or enforce session invalidation]
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Register any required services/options for role revalidation]

## Implementation Plan
- Decide a consistent enforcement strategy (prefer deny-by-default):
  - Option A (recommended): Revalidate role from the database on each request and ensure authorization evaluates against current role.
  - Option B: Invalidate the server-side session immediately when role downgrade is detected (request returns `401` with a consistent code).
- Implement role mismatch handling:
  - Extract user ID from the authenticated principal.
  - Load user from DB and compare `User.Role` with role claim.
  - If mismatch indicates loss of privilege (e.g., claim Admin but DB Standard):
    - deny access (either `401` via session invalidation or `403` based on chosen semantics)
  - Ensure middleware failure modes are safe:
    - any DB lookup failure or unexpected state should fail closed for admin-only endpoints.
- Ensure behavior is deterministic and testable.

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Middleware/SessionTrackingMiddleware.cs | Add role revalidation / role-change detection to prevent stale Admin privileges after downgrade |
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Register any required dependencies for role revalidation (if introduced) |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://cheatsheetseries.owasp.org/cheatsheets/Authorization_Cheat_Sheet.html

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Manual/API] Authenticate as Admin, then downgrade the user to Standard in DB and confirm admin-only endpoints no longer succeed.
- [Security] Confirm behavior fails closed on middleware/DB lookup failures.

## Implementation Checklist
- [x] Implement server-side role revalidation strategy for each authenticated request
- [x] Ensure a downgraded user cannot access admin-only endpoints without re-login
- [x] Ensure failures in role revalidation do not grant access (fail closed)
- [x] Validate behavior with manual downgrade scenario
- [x] Confirm returned status codes align with chosen semantics (`401` vs `403`) and are consistent across endpoints
