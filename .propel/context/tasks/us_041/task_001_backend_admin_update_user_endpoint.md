# Task - [TASK_001]

## Requirement Reference
- User Story: [us_041]
- Story Location: [.propel/context/tasks/us_041/us_041.md]
- Acceptance Criteria: 
    - [Given I am an admin, When I update a user's details, Then the changes are saved and USER_UPDATED event is logged.]

## Task Overview
Implement the admin-only backend operation to update an existing user’s details (name, email, role). This includes strict input validation, safe handling of duplicate emails, and best-effort audit logging of a `USER_UPDATED` event.

## Dependent Tasks
- [US_040 TASK_001] (Admin list users endpoint patterns + response DTO reuse)
- [US_032 TASK_001] (Audit log writer is already implemented and should be reused)

## Impacted Components
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Add admin-only `PUT /api/v1/admin/users/{userId}` endpoint with validation, update logic, and audit logging]
- [CREATE | Server/ClinicalIntelligence.Api/Contracts/Admin/UpdateUserRequest.cs | Request contract for updating user details (name/email/role)]

## Implementation Plan
- Define request/response contract:
  - Add `UpdateUserRequest` with fields:
    - `name` (required, max length 100)
    - `email` (required, RFC5322 validation and normalization)
    - `role` (required, allowed values: `Admin`, `Standard`)
  - Response should reuse `AdminUserItem` (id, name, email, role, status) to keep frontend typing consistent.
- Add endpoint:
  - Route: `PUT /api/v1/admin/users/{userId}`.
  - Require authentication via `.RequireAuthorization()`.
  - Enforce admin role using the existing claim checks used by other admin endpoints.
- Validate inputs:
  - Validate `userId` is a valid GUID.
  - Validate request fields (name, email, role) and return `400` (`ApiErrorResults.BadRequest`) with `invalid_input` when invalid.
  - Validate duplicate email case-insensitively:
    - `IgnoreQueryFilters()` and check for any other user with the same normalized email.
    - Allow the current user to keep their email.
- Apply update:
  - Load target user from `dbContext.Users` (exclude soft-deleted users).
  - Return `404` if not found.
  - Update `Name`, `Email`, `Role`, and `UpdatedAt`.
  - Persist changes.
- Audit log:
  - Use `IAuditLogWriter` best-effort persistence.
  - Action type: `USER_UPDATED`.
  - `userId` should be the admin actor (from JWT `sub`), `resourceId` should be the updated user’s ID.
  - Metadata should include changed fields (do not include any secrets).

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Add `PUT /api/v1/admin/users/{userId}` endpoint with validation + update logic + `USER_UPDATED` audit event |
| CREATE | Server/ClinicalIntelligence.Api/Contracts/Admin/UpdateUserRequest.cs | Request DTO for admin user update |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis
- https://learn.microsoft.com/en-us/ef/core/querying/

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Manual/API] As Admin, update a user via `PUT /api/v1/admin/users/{id}` and verify persisted changes are visible in `GET /api/v1/admin/users`.
- [Manual/API] Validate `409` on duplicate email (for a different user).
- [Manual/API] Validate `403` for non-admin callers.
- [Audit] Confirm an `AuditLogEvent` exists with `ActionType = USER_UPDATED` and `ResourceId = <target userId>`.

## Implementation Checklist
- [x] Create `UpdateUserRequest` contract and ensure JSON field names match frontend expectations
- [x] Implement `PUT /api/v1/admin/users/{userId}` endpoint with admin enforcement
- [x] Add validation for name/email/role and safe duplicate email checks
- [x] Persist updates and return updated `AdminUserItem`
- [x] Write best-effort `USER_UPDATED` audit log event with minimal metadata
