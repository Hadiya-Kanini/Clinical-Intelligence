# Task - [TASK_003]

## Requirement Reference
- User Story: [us_040]
- Story Location: [.propel/context/tasks/us_040/us_040.md]
- Acceptance Criteria: 
    - [Given I am authenticated as Admin, When I navigate to User Management (SCR-014), Then I see a list of all users.]
    - [Given the user list, When displayed, Then it is searchable by name or email (UXR-044).]
    - [Given the user list, When displayed, Then it is sortable by columns (name, email, role, status).]
    - [Given the user list, When there are many users, Then pagination is implemented (TR-017).]

## Task Overview
Add backend integration tests validating the admin users listing endpoint behavior:
- Admin-only access
- Correct search, sort, and pagination semantics
- Stable response contract shape

## Dependent Tasks
- [US_040 TASK_001] (Backend admin list users endpoint)

## Impacted Components
- [CREATE | Server/ClinicalIntelligence.Api.Tests/AdminUsersListEndpointTests.cs | Integration tests for `GET /api/v1/admin/users`]

## Implementation Plan
- Add a new integration test class following existing patterns in `ClinicalIntelligence.Api.Tests`:
  - Use `WebApplicationFactory<Program>`.
  - Skip tests when PostgreSQL/database env is not configured (consistent with existing tests).
- Create test data:
  - Insert a small set of users with distinct names/emails/roles/statuses.
  - Ensure test data cleanup is handled (transaction rollback or explicit deletes) to keep tests deterministic.
- Cover core assertions:
  - Authorization:
    - Unauthenticated returns 401.
    - Standard user returns 403.
    - Admin user returns 200.
  - Pagination:
    - `page=1&pageSize=2` returns 2 items and `total` reflects full set.
  - Search:
    - `q=<partial>` matches expected rows by name/email.
  - Sorting:
    - `sortBy=name&sortDir=asc/desc` changes ordering deterministically.
    - Validate behavior for each allowed sort column.

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api.Tests/AdminUsersListEndpointTests.cs | Verify `GET /api/v1/admin/users` auth + search + sort + pagination behavior |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Automated] Tests pass locally in an environment with PostgreSQL configured.
- [Security] Explicitly verify 401 vs 403 behavior is correct.

## Implementation Checklist
- [x] Add `AdminUsersListEndpointTests` with skip logic when DB is unavailable
- [x] Seed deterministic users covering search/sort permutations
- [x] Assert response schema (`items`, `page`, `pageSize`, `total`) and ordering
- [x] Verify 401/403 enforcement
