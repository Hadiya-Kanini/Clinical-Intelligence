# Task - [TASK_001]

## Requirement Reference
- User Story: [us_034]
- Story Location: [.propel/context/tasks/us_034/us_034.md]
- Acceptance Criteria: 
    - [Given database initialization, When the seed/migration runs, Then a static admin account is created.]
    - [Given the static admin account, When created, Then credentials are read from secure environment variables (FR-010b).]
    - [Given the static admin account, When created, Then the password is hashed using bcrypt.]

## Task Overview
Align and validate the existing EF Core migration-based seed mechanism for creating a static admin account during database initialization. This includes validating required environment variables, enforcing email/password policies, hashing the password with bcrypt, and ensuring the seed is idempotent.

This task focuses on the seed/migration behavior itself, not on enforcing “cannot be deleted or deactivated” protections (handled in TASK_002).

## Dependent Tasks
- [US_033] (Roles exist; seed should assign Role = "Admin")

## Impacted Components
- [MODIFY | Server/ClinicalIntelligence.Api/Migrations/20260115100000_SeedStaticAdminAccount.cs | Ensure the migration meets US_034 acceptance criteria and security constraints]
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Confirm migrations are applied consistently in target environments (already applies migrations in Development)]
- [MODIFY | Server/ClinicalIntelligence.Api.Tests/StaticAdminSeedMigrationTests.cs | Ensure tests cover env-var validation, bcrypt hashing expectations, and idempotency]

## Implementation Plan
- Review the existing migration `SeedStaticAdminAccount`:
  - Validate the migration reads `ADMIN_EMAIL` and `ADMIN_PASSWORD` from the environment.
  - Confirm email normalization and RFC validation path is consistent with `ClinicalIntelligence.Api.Validation.EmailValidation`.
  - Confirm password complexity validation uses centralized `PasswordPolicy`.
  - Confirm bcrypt hashing is performed (target work factor documented/consistent).
  - Confirm idempotency behavior (e.g., `ON CONFLICT ("Email") DO NOTHING`) and that reruns do not create duplicates.
- Ensure migration execution is part of database initialization flow:
  - Verify how migrations are applied (`dbContext.Database.Migrate()` is currently executed in Development).
  - Decide whether non-Development environments should also migrate automatically or rely on external migration execution (document expected operational approach).
- Update/add tests to validate:
  - Missing `ADMIN_EMAIL` or `ADMIN_PASSWORD` fails fast.
  - Invalid email format fails fast.
  - Weak password fails fast.
  - Seeded admin exists with expected properties (`Role`, `Status`, `IsStaticAdmin`, `IsDeleted`).
  - Idempotency: exactly one seeded static admin.

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Migrations/20260115100000_SeedStaticAdminAccount.cs | Validate and adjust seed migration logic to fully satisfy US_034 AC (env vars, bcrypt hashing, idempotency) |
| MODIFY | Server/ClinicalIntelligence.Api.Tests/StaticAdminSeedMigrationTests.cs | Expand/adjust integration tests for env-var validation, seeded properties, and idempotency |
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Confirm/document expected migration execution path during database initialization |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/
- https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying
- https://github.com/BcryptNet/bcrypt.net

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Manual/DB] Apply migrations against a development PostgreSQL instance and verify a static admin row exists in `users`.
- [Security] Confirm `ADMIN_PASSWORD` is never logged and is stored only as a bcrypt hash.
- [Tests] Run the backend test suite including `StaticAdminSeedMigrationTests` (skippable if DB not available).

## Implementation Checklist
- [ ] Review `SeedStaticAdminAccount` migration for env var validation, normalization, and policy enforcement
- [ ] Confirm bcrypt hashing behavior and documented work factor alignment
- [ ] Confirm idempotency behavior and ensure reruns do not create duplicates
- [ ] Validate how/when migrations run during initialization and document expectations
- [ ] Update/add tests for missing env vars, invalid email, weak password
- [ ] Update/add tests for seeded admin properties and idempotency
