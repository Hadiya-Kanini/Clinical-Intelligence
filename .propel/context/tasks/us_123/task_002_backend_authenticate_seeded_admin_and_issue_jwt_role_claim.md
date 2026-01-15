# Task - TASK_002

## Requirement Reference
- User Story: us_123
- Story Location: .propel/context/tasks/us_123/us_123.md
- Acceptance Criteria: 
    - AC-2: Given admin account is seeded, When login is attempted with the seeded credentials, Then authentication succeeds and JWT is issued with Admin role claim

## Task Overview
Replace the current development-only login behavior with database-backed authentication against the `users` table, using bcrypt password verification and issuing a JWT that includes the correct role claim for authorization.
Estimated Effort: 8 hours

## Dependent Tasks
- .propel/context/tasks/us_123/task_001_backend_db_seed_static_admin_account.md (TASK_001)

## Impacted Components
- Server/ClinicalIntelligence.Api/Program.cs
- Server/ClinicalIntelligence.Api/Contracts/LoginRequest.cs
- Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs
- Server/ClinicalIntelligence.Api/Domain/Models/User.cs
- Server/ClinicalIntelligence.Api.Tests/

## Implementation Plan
- Update the `/api/v1/auth/login` endpoint to authenticate against the database:
  - Resolve and normalize `email` (trim + lower-case)
  - Query `ApplicationDbContext.Users` by email
  - Reject login if user is missing, `IsDeleted==true`, or `Status != "Active"`
  - Verify password using bcrypt against `User.PasswordHash` (bcrypt 12 rounds as stored)
  - Avoid leaking whether a user exists (use a consistent "Invalid email or password" message)
- Update JWT generation to include role claims:
  - Include a role claim that ASP.NET authorization will recognize (e.g., `ClaimTypes.Role`)
  - Also include a simple `"role"` claim if needed for frontend consumption
  - Include a stable subject identifier (`sub`) using user id or email (choose one and keep consistent)
- Ensure the login endpoint remains compatible with existing request/response shapes:
  - Maintain `LoginRequest` fields
  - Keep response fields consistent (e.g., `token`, `expires_in`)
- Add tests:
  - Integration test that, with migrations applied and `ADMIN_EMAIL/ADMIN_PASSWORD` set, calling `/api/v1/auth/login` returns 200 and a JWT containing the Admin role claim
  - Negative tests for invalid password and inactive user status
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Update `/api/v1/auth/login` to validate credentials against the `users` table and include role claims in JWT |
| MODIFY | Server/ClinicalIntelligence.Api/Contracts/LoginRequest.cs | Adjust request contract only if needed to support normalization/validation (keep backward compatible) |
| MODIFY | Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs | Ensure query filters / mappings support the auth query path (no functional changes beyond what TASK_001 introduces) |
| CREATE | Server/ClinicalIntelligence.Api.Tests/SeededAdminAuthenticationTests.cs | Verifies seeded admin can authenticate and JWT includes Admin role claim (skippable if Postgres unavailable) |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/aspnet/core/security/authentication/jwt-bearer
- https://learn.microsoft.com/dotnet/api/system.security.claims.claimtypes.role

## Build Commands
- dotnet build Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj
- dotnet test Server/ClinicalIntelligence.Api.Tests/ClinicalIntelligence.Api.Tests.csproj

## Implementation Validation Strategy
- Seed a database using migrations with `ADMIN_EMAIL`/`ADMIN_PASSWORD` set, then call `/api/v1/auth/login` and confirm 200 OK and a JWT containing an Admin role claim (AC-2).
- Confirm invalid credentials return a non-sensitive 401 with a standardized error response.

## Implementation Checklist
- [x] Replace dev-only password checks with database-backed authentication using bcrypt verification
- [x] Add role claim(s) to JWT so downstream authorization can enforce Admin-only access
- [x] Add integration tests covering successful login and role claim presence
- [ ] Build and run tests
