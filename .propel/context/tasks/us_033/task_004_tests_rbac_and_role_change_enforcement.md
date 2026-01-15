# Task - [TASK_004]

## Requirement Reference
- User Story: [us_033]
- Story Location: [.propel/context/tasks/us_033/us_033.md]
- Acceptance Criteria: 
    - [Given role definitions, When implemented, Then they are enforced at both API and UI levels.]
- Edge Cases:
    - [What happens when a user's role is changed mid-session?]

## Task Overview
Add automated test coverage validating role-based access control and role-change mid-session behavior.

This task focuses on tests and validation harnesses; it assumes RBAC logic has been implemented by backend/frontend tasks.

## Dependent Tasks
- [US_033 TASK_001] (Backend RBAC policies)
- [US_033 TASK_002] (Role-change mid-session handling)
- [US_033 TASK_003] (Frontend role-based enforcement)

## Impacted Components
- [MODIFY | Server/ClinicalIntelligence.Api.Tests/* | Add backend integration tests for 401/403/200 behavior on protected endpoints]
- [MODIFY | app/src/__tests__/* | Add/extend frontend tests to validate route guard behavior]

## Implementation Plan
- Backend tests (API integration):
  - Add tests that:
    - unauthenticated access to admin/system-health endpoints returns `401`.
    - Standard role access returns `403`.
    - Admin role access returns `200`.
  - Add a test for role change mid-session:
    - Issue a token/session for a user with Admin role.
    - Change the role in DB to Standard.
    - Verify next request returns `401` with the chosen invalidation code.
- Frontend tests:
  - Validate `RequireAdmin` behavior:
    - Non-admin users cannot access `/admin` and `/admin/users`.
    - Admin users can access.

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api.Tests/* | Add/extend integration tests for RBAC-protected endpoints and role-change invalidation |
| MODIFY | app/src/__tests__/* | Add/extend tests validating UI route guard behavior for admin vs standard |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Automated] All backend integration tests pass in `ClinicalIntelligence.Api.Tests`.
- [Automated] Frontend tests demonstrate correct routing outcomes for Admin vs Standard.

## Implementation Checklist
- [ ] Add backend integration tests for 401/403/200 across protected endpoints
- [ ] Add backend integration test for role-change mid-session invalidation
- [ ] Add frontend tests for admin route guarding and redirects
- [ ] Ensure tests are deterministic and do not rely on shared state
