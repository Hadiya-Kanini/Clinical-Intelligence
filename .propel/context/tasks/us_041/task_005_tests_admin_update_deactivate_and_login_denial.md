# Task - [TASK_005]

## Requirement Reference
- User Story: [us_041]
- Story Location: [.propel/context/tasks/us_041/us_041.md]
- Acceptance Criteria: 
    - [Given I am an admin, When I update a user's details, Then the changes are saved and USER_UPDATED event is logged.]
    - [Given I am an admin, When I deactivate a user, Then their account status changes to deactivated and USER_DEACTIVATED is logged.]
    - [Given a deactivated user, When they attempt to login, Then they are denied access with appropriate message.]

## Task Overview
Add automated test coverage (backend integration tests and frontend E2E tests) to validate the US_041 flows end-to-end at their boundaries:
- Admin update user endpoint behavior
- Admin deactivate/toggle status endpoint behavior (including self/static admin protections)
- Login denial for deactivated users

## Dependent Tasks
- [US_041 TASK_001] (Backend update endpoint)
- [US_041 TASK_002] (Backend toggle-status/deactivate endpoint)
- [US_041 TASK_003] (Backend login denial changes)
- [US_041 TASK_004] (Frontend messaging changes)

## Impacted Components
- [CREATE | Server/ClinicalIntelligence.Api.Tests/Integration/AdminUpdateUserEndpointTests.cs | Integration tests for `PUT /api/v1/admin/users/{id}`]
- [CREATE | Server/ClinicalIntelligence.Api.Tests/Integration/AdminToggleUserStatusEndpointTests.cs | Integration tests for `PATCH /api/v1/admin/users/{id}/toggle-status`]
- [CREATE | Server/ClinicalIntelligence.Api.Tests/Integration/LoginDeactivatedUserDeniedTests.cs | Integration test for login denial when status is inactive]
- [MODIFY | app/src/__tests__/visual/userManagement.spec.ts | Add E2E tests for edit + deactivate flows, using route stubs]

## Implementation Plan
- Backend integration tests:
  - Follow the existing patterns in `AdminCreateUserEndpointTests`:
    - Use `WebApplicationFactory<Program>`.
    - Skip when PostgreSQL isn’t available.
    - Login as admin and obtain CSRF token.
  - `AdminUpdateUserEndpointTests`:
    - Update name/email/role.
    - Validate persistence in DB.
    - Validate `USER_UPDATED` audit event is created.
    - Validate `403` for standard user.
  - `AdminToggleUserStatusEndpointTests`:
    - Toggle a standard user from Active -> Inactive.
    - Validate `USER_DEACTIVATED` audit event.
    - Validate self-deactivation fails.
    - Validate static admin deactivation fails.
  - `LoginDeactivatedUserDeniedTests`:
    - Create user with valid credentials but set status Inactive.
    - Attempt login and assert `403` with expected error code/message.
- Frontend E2E tests:
  - Extend existing `userManagement.spec.ts` with route stubs:
    - Stub `PUT /api/v1/admin/users/{id}` and assert UI updates + toast.
    - Stub `PATCH /api/v1/admin/users/{id}/toggle-status` and assert status label + toast.

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api.Tests/Integration/AdminUpdateUserEndpointTests.cs | Validate admin update endpoint: happy path + validation + audit event |
| CREATE | Server/ClinicalIntelligence.Api.Tests/Integration/AdminToggleUserStatusEndpointTests.cs | Validate deactivate/toggle endpoint: happy path + self/static admin protections + audit |
| CREATE | Server/ClinicalIntelligence.Api.Tests/Integration/LoginDeactivatedUserDeniedTests.cs | Validate login denial for inactive users with expected status/error |
| MODIFY | app/src/__tests__/visual/userManagement.spec.ts | Add E2E coverage for edit + deactivate flows with stable route mocks |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests
- https://playwright.dev/docs/test-intro

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)
- npm --prefix .\\app run test:e2e

## Implementation Validation Strategy
- [Automated] All new integration tests pass in a configured PostgreSQL environment.
- [Automated] Updated Playwright tests pass and are stable across repeated runs.

## Implementation Checklist
- [x] Add backend integration tests for update, deactivate/toggle, and deactivated-login denial
- [x] Ensure tests include CSRF header usage for PUT/PATCH
- [x] Extend frontend Playwright coverage for edit + deactivate flows
- [x] Verify tests cover edge cases: self-deactivation and static admin protection
