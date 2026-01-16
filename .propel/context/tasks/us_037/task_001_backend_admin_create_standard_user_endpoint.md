# Task - [TASK_001]

## Requirement Reference
- User Story: [us_037]
- Story Location: [.propel/context/tasks/us_037/us_037.md]
- Acceptance Criteria: 
    - [Given I am authenticated as Admin, When I submit a user creation request, Then a new user account is created.]
    - [Given user creation request, When submitted, Then all required fields are validated (FR-009l).]
    - [Given a non-admin user, When they attempt to access the endpoint, Then they receive 403 Forbidden.]
    - [Given successful creation, When completed, Then USER_CREATED event is logged in audit trail.]

## Task Overview
Implement an **admin-only** API endpoint for creating **Standard** user accounts. The endpoint must:
- enforce authentication and admin authorization (role-based)
- validate required fields and reject invalid requests with the standardized API error format
- prevent duplicate user creation by email (unique constraint) and return a deterministic conflict response
- persist an audit log event with `ActionType = "USER_CREATED"`

This task focuses on backend API behavior and database persistence; it does not include any UI wiring.

## Dependent Tasks
- [US_034] Initialize static admin account via database seed (admin user must exist for end-to-end verification)
- [US_035] Restrict admin-only features from Standard Users (baseline authorization expectations)

## Impacted Components
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Add `POST /api/v1/admin/users` endpoint with admin-only access and standardized error handling]
- [CREATE | Server/ClinicalIntelligence.Api/Contracts/Admin/CreateUserRequest.cs | Request contract for admin user creation (name/email/password) aligned to validation requirements]
- [CREATE | Server/ClinicalIntelligence.Api/Contracts/Admin/CreateUserResponse.cs | Response contract returning created user identifiers and canonical fields]

## Implementation Plan
- Define request/response contracts:
  - Create `CreateUserRequest` with required fields:
    - `name`
    - `email`
    - `password`
  - Create `CreateUserResponse` returning:
    - `id`
    - `email`
    - `role` (always `standard`)
- Add the endpoint:
  - Implement `POST /api/v1/admin/users` under the existing `var v1 = app.MapGroup("/api/v1");` group.
  - Require authentication using `.RequireAuthorization()`.
  - Enforce admin-only access:
    - Check `context.User.IsInRole("Admin")` OR role claim (`ClaimTypes.Role` / `role`) and return `ApiErrorResults.Forbidden()` when not admin.
- Validate inputs (FR-009l):
  - Email:
    - Validate via `EmailValidation.ValidateWithDetails()` and return `ApiErrorResults.BadRequest("invalid_input", ...)` with error details.
    - Normalize email consistently (via `EmailValidation.Normalize` or existing normalization used by `EmailValidation`).
  - Name:
    - Ensure non-empty and length <= 100 (aligns with `User.Name` max length).
  - Password:
    - Validate via `PasswordPolicy.GetMissingRequirements()` / `PasswordPolicy.IsValid()` and return `ApiErrorResults.BadRequest("invalid_input", ...)` with a safe, non-secret detail list.
- Persist user:
  - Create `User` entity with:
    - `Role = "Standard"`
    - `Status = "Active"`
    - `IsStaticAdmin = false`, `IsDeleted = false`
    - `PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)`
    - `CreatedAt/UpdatedAt = DateTime.UtcNow`
  - Save to DB and handle duplicate email edge case:
    - Rely on existing unique index (`ix_users_email`) and map conflict to `ApiErrorResults.Conflict("duplicate_email", "A user with this email already exists.")`.
    - Ensure conflict handling does not leak extra information beyond the email collision itself.
- Write audit event:
  - Insert `AuditLogEvent` with:
    - `ActionType = "USER_CREATED"`
    - `UserId = <admin user id from JWT subject>`
    - `ResourceType = "User"`
    - `ResourceId = <created user id>`
    - `IpAddress` / `UserAgent` from request
    - `Timestamp = DateTime.UtcNow`
    - `Metadata` JSON (must not include password); include safe fields like `created_user_email` and `created_user_role`.

**Focus on how to implement**

## Current Project State
- ✅ **COMPLETED** - Admin create user endpoint fully implemented in Program.cs (lines 1024-1131)
- ✅ **Contracts created** - CreateUserRequest.cs and CreateUserResponse.cs implemented
- ✅ **All validation** - Email, password, and name validation using existing utilities
- ✅ **Security enforced** - Admin-only authorization with proper error responses
- ✅ **Database integration** - User creation with proper hashing and audit logging
- ✅ **Error handling** - Standardized API errors for all scenarios (400, 401, 403, 409)

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api/Contracts/Admin/CreateUserRequest.cs | Request DTO for admin user creation (validated: name/email/password) |
| CREATE | Server/ClinicalIntelligence.Api/Contracts/Admin/CreateUserResponse.cs | Response DTO for created user information |
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Add `POST /api/v1/admin/users` endpoint with admin-only authorization, validation, duplicate-email conflict handling, and USER_CREATED audit logging |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis
- https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles
- https://learn.microsoft.com/en-us/ef/core/saving/concurrency
- https://learn.microsoft.com/en-us/ef/core/saving/transactions

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Manual/API] Authenticate as Admin and call `POST /api/v1/admin/users`:
  - verify 201/200 success response and created user exists in `users` table
- [Security] Authenticate as Standard User and call the same endpoint:
  - verify 403 Forbidden with standardized error format
- [Validation] Submit requests with:
  - missing name/email/password
  - invalid email format
  - weak password
  - verify `400` with standardized error shape
- [Edge Case] Submit a request with an email that already exists:
  - verify `409 Conflict` with stable error code/message
- [Audit] Verify `audit_log_events` includes a record with `ActionType = "USER_CREATED"` and `ResourceId = created user id`

## Implementation Checklist
- [x] Add admin create-user request/response contracts under `Server/ClinicalIntelligence.Api/Contracts/Admin/`
- [x] Implement `POST /api/v1/admin/users` in `Program.cs` and require authorization
- [x] Enforce admin role check and return 403 for non-admin
- [x] Validate name/email/password using `EmailValidation` and `PasswordPolicy`
- [x] Create user with `Role = "Standard"` and bcrypt-hash the password
- [x] Handle duplicate email as 409 Conflict (stable code/message)
- [x] Insert `USER_CREATED` audit event (no secrets in metadata)
- [x] Validate behavior manually using a local run + API calls
