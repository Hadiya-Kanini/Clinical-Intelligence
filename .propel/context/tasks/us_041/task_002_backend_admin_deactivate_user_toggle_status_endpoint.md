# Task - [TASK_002]

## Requirement Reference
- User Story: [us_041]
- Story Location: [.propel/context/tasks/us_041/us_041.md]
- Acceptance Criteria: 
    - [Given I am an admin, When I deactivate a user, Then their account status changes to deactivated and USER_DEACTIVATED is logged.]

## Task Overview
Implement the admin-only backend operation to deactivate/reactivate user accounts using the existing frontend route pattern `PATCH /api/v1/admin/users/{userId}/toggle-status`. The endpoint must:
- Toggle account status between Active and Inactive
- Enforce self-deactivation protection
- Enforce static admin protection (FR-010c)
- Write best-effort audit events (`USER_DEACTIVATED` when transitioning to inactive)

## Dependent Tasks
- [US_040 TASK_001] (Admin list users endpoint patterns)
- [US_034 TASK_002] (Static admin guard is already implemented and must be used)
- [US_032 TASK_001] (Audit log writer is already implemented and should be reused)

## Impacted Components
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Add admin-only `PATCH /api/v1/admin/users/{userId}/toggle-status` endpoint with guard rails and audit logging]

## Implementation Plan
- Add endpoint:
  - Route: `PATCH /api/v1/admin/users/{userId}/toggle-status`.
  - Require authentication via `.RequireAuthorization()`.
  - Enforce admin role using existing claim checks.
- Validate inputs:
  - Validate `userId` is a valid GUID; return `400 invalid_input` when invalid.
- Enforce protection rules:
  - Prevent admin from deactivating themselves:
    - Compare route `userId` to authenticated user id (`sub` claim).
    - Return `400 invalid_input` (or `403 forbidden`) with a clear message.
  - Prevent status changes to non-active for the static admin account:
    - Use `IStaticAdminGuard.ValidateCanChangeStatusAsync(userId, newStatus)`.
    - Convert `StaticAdminProtectionException` into a consistent API error shape.
- Toggle status:
  - Load target user (exclude soft-deleted users); return `404` if not found.
  - Determine new status:
    - If current `Status == "Active"` -> set to `Inactive`.
    - Otherwise -> set to `Active`.
  - Update `UpdatedAt` and persist.
- Response:
  - Return updated user mapped to the same shape used by the list endpoint (`AdminUserItem`).
- Audit log:
  - Use `IAuditLogWriter` best-effort persistence.
  - When new status is Inactive, write `USER_DEACTIVATED`.
  - Include admin actor id and session id where available.
  - Include minimal metadata (target user id/email + status transition).

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Add `PATCH /api/v1/admin/users/{userId}/toggle-status` endpoint + self/static-admin protections + `USER_DEACTIVATED` audit logging |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Manual/API] As Admin, call PATCH on a standard user and confirm status toggles in `GET /api/v1/admin/users`.
- [Manual/API] Confirm `USER_DEACTIVATED` audit event is written when status transitions to inactive.
- [Security] As Standard user, confirm 403.
- [Edge Case] Confirm attempting to deactivate self fails.
- [Edge Case] Confirm attempting to deactivate static admin fails.

## Implementation Checklist
- [x] Implement `PATCH /api/v1/admin/users/{userId}/toggle-status` endpoint with admin enforcement
- [x] Add self-deactivation check against authenticated `sub`
- [x] Use `IStaticAdminGuard` to block static admin deactivation
- [x] Toggle status and return updated user in `AdminUserItem` shape
- [x] Write `USER_DEACTIVATED` audit event when transitioning to inactive
