# Task - TASK_004

## Requirement Reference
- User Story: us_119
- Story Location: .propel/context/tasks/us_119/us_119.md
- Acceptance Criteria: 
    - AC-2: Given tables are created, When schema is inspected, Then all columns match ERD specifications with correct PostgreSQL data types
    - AC-4: Given USER table exists, When email uniqueness is tested, Then duplicate emails are rejected with constraint violation
    - AC-5: Given migration is complete, When `\dt` is executed in psql, Then all 16 tables are listed with correct ownership

## Task Overview
Finalize schema quality for the 16-table baseline by adding critical constraints and indexes (including unique email) and defining a repeatable validation process (psql inspection + EF tooling) to confirm the database matches the ERD.
Estimated Effort: 8 hours

## Dependent Tasks
- .propel/context/tasks/us_119/task_003_backend_pgvector_extension_and_document_chunk_embedding_vector_768.md (TASK_003)

## Impacted Components
- Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs
- Server/ClinicalIntelligence.Api/Migrations/*
- Server/ClinicalIntelligence.Api.Tests/
- Server/README.md

## Implementation Plan
- Add constraints aligned to the ERD and acceptance criteria:
  - Unique constraint on `USER.email`.
  - Required foreign keys and referential integrity constraints across the 16 tables.
  - Appropriate nullability and max length constraints (varchar lengths) in EF mappings.
- Implement indexing strategy for baseline performance and operability:
  - Minimum: index `user.email` and other high-selectivity/operational columns needed for core flows.
  - If included in the baseline scope, add pgvector index creation for `DOCUMENT_CHUNK.embedding` (matching the chosen index type).
- Add validation coverage:
  - Integration-style tests that apply migrations to a Postgres instance and assert:
    - All expected tables exist.
    - Unique email constraint rejects duplicates.
  - Documentation for manual verification using psql and EF Core CLI.
- Document idempotency expectations:
  - EF migrations apply once per database via migration history.
  - For deployment repeatability, document idempotent script generation and safe re-apply behavior.
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs | Add/adjust Fluent mappings for constraints (unique indexes, required fields, max lengths) and relationships |
| MODIFY | Server/ClinicalIntelligence.Api/Migrations/* | Add migration updates to enforce constraints and indexes (including unique user email) |
| CREATE | Server/ClinicalIntelligence.Api.Tests/BaselineSchemaMigrationValidationTests.cs | Integration-style tests that validate table existence and key constraints (skippable when Postgres not available) |
| MODIFY | Server/README.md | Add validated commands for schema inspection (`\dt`), migration apply/list, and idempotent script generation guidance |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/ef/core/modeling/indexes
- https://learn.microsoft.com/ef/core/managing-schemas/migrations/applying

## Build Commands
- dotnet build Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj
- dotnet test Server/ClinicalIntelligence.Api.Tests/ClinicalIntelligence.Api.Tests.csproj

## Implementation Validation Strategy
- Verify unique email is enforced by the database (duplicate insert fails with constraint violation).
- Verify schema inspection confirms all required tables exist and migrations are applied.
- Verify tests pass (or are conditionally skipped when Postgres is not configured).

## Implementation Checklist
- [x] Add unique constraint/index for `USER.email` and validate behavior
- [x] Add/verify all foreign key constraints across the 16-table ERD
- [x] Add/verify indexes required for baseline operations and acceptance validation
- [x] Update migrations to reflect constraints and indexes
- [x] Add integration-style tests validating table existence and unique email enforcement
- [x] Document validated commands: `dotnet ef database update`, `dotnet ef migrations list`, and psql `\dt`
- [x] Document idempotency expectations and `dotnet ef migrations script --idempotent` usage
- [x] Run build/tests to ensure schema changes compile and validation passes
