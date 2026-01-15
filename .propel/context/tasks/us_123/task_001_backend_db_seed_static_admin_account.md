# Task - TASK_001

## Requirement Reference
- User Story: us_123
- Story Location: .propel/context/tasks/us_123/us_123.md
- Acceptance Criteria: 
    - AC-1: Given database is initialized, When seed migration runs, Then admin account is created with credentials from environment variables
    - AC-4: Given seed migration runs on fresh database, When `ADMIN_EMAIL` or `ADMIN_PASSWORD` environment variables are missing, Then migration fails with clear error: "Required environment variables ADMIN_EMAIL and ADMIN_PASSWORD must be set"
    - AC-5: Given admin account already exists, When seed migration runs again, Then existing account is preserved (idempotent operation)

## Task Overview
Implement an EF Core migration-based seed for a single static Admin user using secure environment variables. The seed must be idempotent, validate required inputs (email format + password complexity), hash the password using bcrypt (12 rounds), and store a durable marker that the account is the protected “static admin”.
Estimated Effort: 8 hours

## Dependent Tasks
- US_119 - Baseline Schema Migration (users table exists and EF migrations are enabled)

## Impacted Components
- Server/ClinicalIntelligence.Api/Domain/Models/User.cs
- Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs
- Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj
- Server/ClinicalIntelligence.Api/Migrations/
- Server/ClinicalIntelligence.Api.Tests/

## Implementation Plan
- Add a durable “static admin” marker on the user record:
  - Add a boolean `IsStaticAdmin` property to `User` (default false)
  - Update `ConfigureUser` mapping to include the column and add an index if needed
- Add bcrypt hashing dependency:
  - Add `BCrypt.Net-Next` (or equivalent) NuGet package to the API project
- Create a new EF Core migration to seed the static admin:
  - In the migration `Up()` method, read `ADMIN_EMAIL` and `ADMIN_PASSWORD` from environment variables
  - If either is missing/blank, throw `InvalidOperationException` with the exact message: "Required environment variables ADMIN_EMAIL and ADMIN_PASSWORD must be set"
  - Validate `ADMIN_EMAIL` format using a server-side validator (e.g., `EmailAddressAttribute` or `MailAddress` parsing)
  - Validate `ADMIN_PASSWORD` complexity per FR-009c (min 8 chars, mixed case, number, special char); fail fast with a clear, non-sensitive message
  - Hash the password using bcrypt with 12 rounds
  - Insert the user using an idempotent approach:
    - Prefer SQL `INSERT ... ON CONFLICT ("Email") DO NOTHING` so reruns don’t alter the existing record
    - Set:
      - `Email = ADMIN_EMAIL`
      - `PasswordHash = bcrypt hash`
      - `Role = "Admin"`
      - `Status = "Active"`
      - `IsStaticAdmin = true`
      - `Name` to a stable value (e.g., "Static Admin")
      - `CreatedAt/UpdatedAt` using database defaults when possible
  - Ensure the migration never logs the email/password values
- Add tests (Postgres-backed, skippable when DB unavailable) to validate:
  - Missing env vars -> migration/application startup fails with the exact required message
  - With env vars present, the admin user exists after migrations and has:
    - `Role == "Admin"`, `Status == "Active"`, `IsStaticAdmin == true`
  - Email uniqueness ensures idempotency behavior (no duplicate seeded accounts)
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Domain/Models/User.cs | Add `IsStaticAdmin` boolean property (default false) to durably mark the protected seeded admin account |
| MODIFY | Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs | Map the `IsStaticAdmin` column in `ConfigureUser` (and index if needed) |
| MODIFY | Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj | Add bcrypt NuGet dependency for hashing/verification |
| CREATE | Server/ClinicalIntelligence.Api/Migrations/*_SeedStaticAdminAccount.cs | New EF migration that validates env vars, hashes password (bcrypt 12 rounds), and inserts static admin idempotently |
| CREATE | Server/ClinicalIntelligence.Api/Migrations/*_SeedStaticAdminAccount.Designer.cs | EF-generated designer for the seed migration |
| MODIFY | Server/ClinicalIntelligence.Api/Migrations/ApplicationDbContextModelSnapshot.cs | EF snapshot update for the new `IsStaticAdmin` column |
| CREATE | Server/ClinicalIntelligence.Api.Tests/StaticAdminSeedMigrationTests.cs | Integration tests validating env-var validation + seeded admin properties (skippable if Postgres unavailable) |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://www.npgsql.org/doc/connection-string-parameters.html
- https://learn.microsoft.com/ef/core/managing-schemas/migrations/
- https://en.wikipedia.org/wiki/Bcrypt

## Build Commands
- dotnet build Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj
- dotnet test Server/ClinicalIntelligence.Api.Tests/ClinicalIntelligence.Api.Tests.csproj

## Implementation Validation Strategy
- Run EF migrations against a fresh PostgreSQL database with `ADMIN_EMAIL` and `ADMIN_PASSWORD` set; validate the seeded admin exists with `Role=Admin`, `Status=Active`, `IsStaticAdmin=true` (AC-1).
- Run migrations without `ADMIN_EMAIL`/`ADMIN_PASSWORD`; validate failure includes the exact required message and does not leak secrets (AC-4).
- Re-run migrations or attempt the seed insert SQL again; validate no duplicate user is created (AC-5).

## Implementation Checklist
- [x] Add `IsStaticAdmin` to `User` model and EF mapping
- [x] Add bcrypt hashing dependency
- [x] Create seed migration that reads env vars, validates inputs, hashes password, and inserts idempotently
- [x] Add integration tests for seed behavior and missing env vars
- [ ] Build and run tests
