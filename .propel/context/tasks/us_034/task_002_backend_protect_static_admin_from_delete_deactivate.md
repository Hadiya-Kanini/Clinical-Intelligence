# Task - [TASK_002]

## Requirement Reference
- User Story: [us_034]
- Story Location: [.propel/context/tasks/us_034/us_034.md]
- Acceptance Criteria: 
    - [Given the static admin account, When it exists, Then it cannot be deleted or deactivated (FR-010c).]

## Task Overview
Implement application-layer protections to prevent the static admin account (`User.IsStaticAdmin == true`) from being deleted or deactivated, regardless of which API endpoint or internal code path attempts the operation.

This task focuses on enforcing FR-010c at runtime (application behavior). It assumes the static admin record is already created via migration/seed (TASK_001).

## Dependent Tasks
- [US_034 TASK_001 - Backend static admin seed migration]

## Impacted Components
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Identify and harden any user-management endpoints that can change status or soft-delete users]
- [MODIFY | Server/ClinicalIntelligence.Api/Services/* | If a user-management service exists/gets introduced, enforce protection centrally]
- [MODIFY | Server/ClinicalIntelligence.Api/Domain/Models/User.cs | Confirm domain invariants are representable (no changes unless required)]
- [CREATE/MODIFY | Server/ClinicalIntelligence.Api.Tests/Integration/* | Add integration tests that verify protected behavior]

## Implementation Plan
- Identify all code paths that can “delete” or “deactivate” a user:
  - Soft delete (`IsDeleted = true`), hard delete, or status changes (e.g., `Status = "Inactive"`).
  - Any administrative endpoints under `/api/v1` that mutate users.
- Add a single, centralized guard that blocks operations targeting `IsStaticAdmin == true`:
  - Prefer enforcing in an application/service layer rather than duplicating checks in multiple endpoints.
  - Ensure error response is explicit but does not leak credentials.
- Ensure the static admin cannot be deactivated:
  - If “deactivation” is represented by `Status != "Active"`, block transitions away from `Active`.
- Ensure the static admin cannot be deleted:
  - Block setting `IsDeleted = true`.
  - Block any `DELETE` endpoint against that user.
- Add tests:
  - Attempt to delete static admin and assert operation is rejected.
  - Attempt to deactivate static admin and assert operation is rejected.
  - Ensure non-static users can still be updated/deactivated/deleted per policy.

**Focus on how to implement**

## Current Project State
- Created `IStaticAdminGuard` interface in `Services/IStaticAdminGuard.cs`
- Created `StaticAdminGuard` implementation in `Services/StaticAdminGuard.cs`
- Created `StaticAdminProtectionException` in `Services/StaticAdminProtectionException.cs`
- Registered service in DI container in `Program.cs`
- Created comprehensive integration tests in `Tests/Integration/StaticAdminProtectionTests.cs`
- Note: No user management endpoints exist yet; guard is ready for use when they are added

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Add protection checks in relevant endpoints or route to a protected service method |
| CREATE | Server/ClinicalIntelligence.Api.Tests/Integration/StaticAdminProtectionTests.cs | Integration tests ensuring static admin cannot be deleted or deactivated |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Tests] Run integration tests verifying the static admin cannot be mutated into an inactive/deleted state.
- [Manual/API] Attempt the protected operations via Swagger and confirm consistent rejection.

## Implementation Checklist
- [x] Enumerate all user mutation endpoints / code paths
- [x] Implement centralized guard for `IsStaticAdmin` mutation attempts
- [x] Block soft delete and hard delete operations for static admin
- [x] Block status transitions away from `Active` for static admin
- [x] Add integration tests for delete/deactivate rejection
- [x] Validate non-static user operations still function
