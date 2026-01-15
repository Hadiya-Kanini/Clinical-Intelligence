# Task - [TASK_001]

## Requirement Reference
- User Story: [us_040]
- Story Location: [.propel/context/tasks/us_040/us_040.md]
- Acceptance Criteria: 
    - [Given I am authenticated as Admin, When I navigate to User Management (SCR-014), Then I see a list of all users.]
    - [Given the user list, When displayed, Then it is searchable by name or email (UXR-044).]
    - [Given the user list, When displayed, Then it is sortable by columns (name, email, role, status).]
    - [Given the user list, When there are many users, Then pagination is implemented (TR-017).]

## Task Overview
Add an admin-only backend endpoint to retrieve a paginated list of users to support the Admin User Management screen (SCR-014). The endpoint must support:
- Search by name/email
- Sorting by allowed columns (name, email, role, status)
- Pagination for large user counts

This task only covers the **list** capability required to render the admin table. Create/edit/deactivate operations are handled by dependent user stories (e.g., US_037) and are out of scope here.

## Dependent Tasks
- [US_035 TASK_001] (Backend admin endpoint protection / role enforcement patterns)
- [US_037] (Admin-only user creation endpoint) (dependency for full CRUD flows; not required for read-only listing)

## Impacted Components
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Add `GET /api/v1/admin/users` endpoint with admin-only authorization, query param validation, and response contract]
- [CREATE | Server/ClinicalIntelligence.Api/Contracts/Admin/AdminUsersListResponse.cs | Response DTO for paginated user list]
- [CREATE | Server/ClinicalIntelligence.Api/Contracts/Admin/AdminUsersListQuery.cs | Query binding model (search, sort, paging) with safe defaults]

## Implementation Plan
- Define the endpoint contract:
  - Add `GET /api/v1/admin/users` under the existing `v1` group.
  - Use query parameters:
    - `q` (optional string) search by partial match on `Name` or `Email`
    - `sortBy` in {`name`,`email`,`role`,`status`} (default: `name`)
    - `sortDir` in {`asc`,`desc`} (default: `asc`)
    - `page` (1-based, default: 1)
    - `pageSize` (default: 20, max: 100)
- Enforce admin-only access:
  - Require authentication via `.RequireAuthorization()`.
  - Validate admin role using role claims already present in JWT (`ClaimTypes.Role` and `role`).
  - Return a standardized `403` (`ApiErrorResults.Forbidden`) when a non-admin accesses the endpoint.
- Implement query logic (EF Core):
  - Base query should ignore soft-deleted users and include only relevant statuses as per existing patterns (e.g., `!IsDeleted`).
  - Search:
    - If `q` is present, filter `Name` and `Email` with a case-insensitive `Contains` match.
  - Sort:
    - Implement a whitelist mapping for `sortBy` to avoid dynamic SQL / injection.
  - Pagination:
    - Apply `Skip`/`Take` based on validated `page` and `pageSize`.
    - Return `total` count (pre-pagination) to support client paging UI.
- Security & validation:
  - Rely on `RequestValidationMiddleware` for suspicious query patterns.
  - Add additional server-side validation for `page/pageSize/sortBy/sortDir` and return `400` with standard error shape when invalid.

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Add admin-only `GET /api/v1/admin/users` endpoint with search/sort/pagination |
| CREATE | Server/ClinicalIntelligence.Api/Contracts/Admin/AdminUsersListQuery.cs | Query model for `q`, `sortBy`, `sortDir`, `page`, `pageSize` with defaults/max limits |
| CREATE | Server/ClinicalIntelligence.Api/Contracts/Admin/AdminUsersListResponse.cs | Response DTO containing `items`, `page`, `pageSize`, `total` |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis
- https://learn.microsoft.com/en-us/ef/core/querying/

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Manual/API] As Admin, call `GET /api/v1/admin/users?page=1&pageSize=20` and confirm response returns `items` plus `total`.
- [Manual/API] Verify search: `q=<partial>` filters by `Name` or `Email`.
- [Manual/API] Verify sorting behavior for all allowed `sortBy` values and both directions.
- [Security] As Standard user, confirm `403 Forbidden` is returned.
- [Performance] Seed 200+ users and confirm response times remain acceptable and memory usage is bounded due to pagination.

## Implementation Checklist
- [ ] Add request/response contracts for admin user listing (query + response DTOs)
- [ ] Implement `GET /api/v1/admin/users` with admin-only authorization
- [ ] Add validated search, sort (whitelist), and pagination logic
- [ ] Ensure standardized 400/403 errors via `ApiErrorResults`
- [ ] Validate manually with multiple queries and edge cases (empty list, large list)
