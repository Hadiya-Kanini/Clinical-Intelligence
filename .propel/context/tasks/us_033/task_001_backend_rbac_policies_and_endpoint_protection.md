# Task - [TASK_001]

## Requirement Reference
- User Story: [us_033]
- Story Location: [.propel/context/tasks/us_033/us_033.md]
- Acceptance Criteria: 
    - [Given the system, When roles are defined, Then Admin and Standard User roles exist with distinct permission sets.]
    - [Given the Admin role, When assigned, Then the user can access User Management and System Health functions (FR-011).]
    - [Given role definitions, When implemented, Then they are enforced at both API and UI levels.]

## Task Overview
Define and enforce role-based access control (RBAC) at the API layer by establishing a single source of truth for roles (Admin vs Standard) and applying authorization requirements to sensitive endpoints.

This task focuses on backend role definition + endpoint protection only. It does not implement full user management CRUD features.

## Dependent Tasks
- [US_011] (JWT authentication with HttpOnly cookies)

## Impacted Components
- [CREATE | Server/ClinicalIntelligence.Api/Authorization/Roles.cs | Centralized role constants (Admin/Standard) to avoid string drift]
- [CREATE | Server/ClinicalIntelligence.Api/Authorization/AuthorizationPolicies.cs | Centralized policy names and mapping to roles]
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Register authorization policies and apply them to admin/system-health endpoints]
- [MODIFY | Server/ClinicalIntelligence.Api/Middleware/SessionTrackingMiddleware.cs | Ensure authenticated requests have consistent role claims available for authorization]

## Implementation Plan
- Introduce a centralized representation for role names:
  - Use constants for "Admin" and "Standard" to match existing DB seeds and `User.Role` default.
  - Ensure any comparisons are case-consistent (avoid mixing `admin`/`Admin`).
- Add authorization policies:
  - Register policies in DI via `builder.Services.AddAuthorization(...)`.
  - Define at least:
    - `AdminOnly` policy: requires `Admin` role.
    - `Authenticated` policy (optional): requires any authenticated user.
- Apply policies to sensitive endpoints:
  - Protect system health endpoints intended for admins (e.g., `/health/db`, `/health/db/pool`) with `AdminOnly`.
  - Ensure APIs that are admin-only are grouped under a predictable route prefix (e.g., future `/api/v1/admin/*`) and protected by `AdminOnly`.
- Ensure correct HTTP semantics:
  - Unauthenticated: `401`.
  - Authenticated but missing role: `403`.

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api/Authorization/Roles.cs | Central role constants to prevent inconsistent role strings across DB/JWT/UI |
| CREATE | Server/ClinicalIntelligence.Api/Authorization/AuthorizationPolicies.cs | Defines policy names and mapping to role requirements |
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Register policies and protect admin/system-health endpoints with `AdminOnly` |
| MODIFY | Server/ClinicalIntelligence.Api/Middleware/SessionTrackingMiddleware.cs | Ensure role claims are available/consistent for downstream authorization |

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
- [x] Add centralized role constants and ensure casing is consistent with seeded data
- [x] Add authorization policy definitions (at least `AdminOnly`)
- [x] Register authorization policies in `Program.cs`
- [x] Apply `AdminOnly` to admin/system-health endpoints
- [x] Validate `401/403/200` behavior for unauthenticated/standard/admin requests
- [x] Ensure logs do not leak sensitive authorization details
