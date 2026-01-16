# Task - [TASK_001]

## Requirement Reference
- User Story: [us_035]
- Story Location: [.propel/context/tasks/us_035/us_035.md]
- Acceptance Criteria: 
    - [Given a Standard User, When they attempt to access admin API endpoints, Then they receive 403 Forbidden.]
    - [Given authorization checks, When performed, Then they occur on every request (not just initial load).]

## Task Overview
Implement backend authorization policies and apply them to all admin-only API endpoints so that:
- unauthenticated requests receive `401 Unauthorized`
- authenticated Standard Users receive `403 Forbidden`
- authenticated Admin users receive `200` (or the endpoint’s expected success status)

This task focuses on enforcing access control at the API boundary (least privilege, deny-by-default) and ensuring role checks are evaluated on each request by the authorization pipeline.

## Dependent Tasks
- [US_033] (Define Admin and Standard User roles)

## Impacted Components
- [CREATE | Server/ClinicalIntelligence.Api/Authorization/Roles.cs | Centralized role constants (`Admin`, `Standard`) to prevent string drift]
- [CREATE | Server/ClinicalIntelligence.Api/Authorization/AuthorizationPolicies.cs | Centralized policy names and mapping to role requirements]
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Register authorization policies and apply `AdminOnly` to admin-only endpoints]

## Implementation Plan
- Introduce centralized role constants:
  - Create `Roles.Admin` and `Roles.Standard` matching database values (`User.Role`) and JWT role claims.
  - Ensure comparisons are case-consistent.
- Introduce authorization policies:
  - Create `AuthorizationPolicies.AdminOnly`.
  - Register with `builder.Services.AddAuthorization(...)` using `RequireRole(Roles.Admin)`.
- Apply to admin-only endpoints:
  - Protect current admin-only operational endpoints:
    - `/health/db`
    - `/health/db/pool`
  - For future-proofing, ensure any admin-only endpoints added later under predictable prefixes (e.g., `/api/v1/admin/*`) are protected with the same policy.
- Confirm correct HTTP semantics:
  - unauthenticated -> `401`
  - authenticated Standard -> `403`

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api/Authorization/Roles.cs | Central role constants to prevent inconsistent role strings across DB/JWT/UI |
| CREATE | Server/ClinicalIntelligence.Api/Authorization/AuthorizationPolicies.cs | Defines policy names and mapping to role requirements |
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Register policies and protect admin-only endpoints (e.g., `/health/db`, `/health/db/pool`) with `AdminOnly` |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles
- https://learn.microsoft.com/en-us/aspnet/core/security/authorization/policies
- https://cheatsheetseries.owasp.org/cheatsheets/Authorization_Cheat_Sheet.html

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Manual/API] Confirm `/health/db` and `/health/db/pool` return `401` when unauthenticated.
- [Manual/API] Confirm `/health/db` and `/health/db/pool` return `403` when authenticated as Standard.
- [Manual/API] Confirm `/health/db` and `/health/db/pool` return `200` when authenticated as Admin.

## Implementation Checklist
- [x] Add `Roles` constants and confirm casing matches DB + JWT claims
- [x] Add `AuthorizationPolicies` and register with `AddAuthorization(...)`
- [x] Apply `AdminOnly` to admin-only endpoints (start with `/health/db` and `/health/db/pool`)
- [x] Validate `401/403/200` behavior across unauthenticated/standard/admin scenarios
- [x] Ensure logs do not leak sensitive authorization details
