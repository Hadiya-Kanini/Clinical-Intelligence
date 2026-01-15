# Task - TASK_001

## Requirement Reference
- User Story: us_119
- Story Location: .propel/context/tasks/us_119/us_119.md
- Acceptance Criteria: 
    - AC-1: Given EF Core is configured, When baseline migration is applied, Then all 16 tables are created
    - AC-2: Given tables are created, When schema is inspected, Then all columns match ERD specifications with correct PostgreSQL data types

## Task Overview
Establish the authoritative baseline-schema approach for US_119, including reconciling any existing EF Core migrations/entities already present in the repository and standardizing the naming conventions (tables, schemas, migrations folder) before implementing the 16-table ERD.
Estimated Effort: 6 hours

## Dependent Tasks
- .propel/context/tasks/us_118/task_001_backend_efcore_packages_and_dotnet_ef_tooling.md (TASK_001)
- .propel/context/tasks/us_118/task_002_backend_dbcontext_postgres_configuration_and_migration_scaffolding.md (TASK_002)
- .propel/context/tasks/us_118/task_003_backend_apply_migrations_list_validation_and_conflict_resolution_docs.md (TASK_003)

## Impacted Components
- Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs
- Server/ClinicalIntelligence.Api/Migrations/*
- Server/README.md

## Implementation Plan
- Audit current EF Core state:
  - Existing migration history (files under `Server/ClinicalIntelligence.Api/Migrations/`)
  - Current entity set and table mappings in `ApplicationDbContext`
  - Current DB naming conventions (pluralization, snake_case) and whether any deployed environments already exist
- Decide and document the baseline migration strategy:
  - If no environments rely on current migrations, choose a clean baseline approach (reset/squash migrations and regenerate an initial baseline matching the ERD)
  - If migrations may already be applied anywhere, choose a non-destructive approach (new migration that transitions from current schema to ERD schema, with explicit renames/drops/additions)
- Standardize how the baseline schema will be represented:
  - Confirm canonical migrations folder (`Server/ClinicalIntelligence.Api/Migrations/` vs `Data/Migrations/`) and document the standard command(s)
  - Confirm table naming rules for Postgres (avoid reserved keywords like `user`; prefer `users`/`sessions` etc, or document quoting strategy if singular names must be used)
- Define pgvector prerequisite handling approach:
  - Decide whether migrations will automatically create the `vector` extension (recommended for dev) vs fail fast with a clear prerequisite message (recommended for restricted environments)
  - Align this with existing `scripts/db/enable_pgvector.sql`
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/README.md | Add a documented decision for the US_119 baseline migration strategy, naming conventions, and canonical EF Core commands (including idempotent script generation guidance) |
| MODIFY | Server/ClinicalIntelligence.Api/Migrations/* | Remove/replace/transition existing migrations as required by the chosen baseline strategy (documented decision) |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/ef/core/managing-schemas/migrations/
- https://learn.microsoft.com/ef/core/cli/dotnet

## Build Commands
- dotnet build Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj

## Implementation Validation Strategy
- Verify the chosen migration strategy is documented and actionable (devs can follow it end-to-end).
- Verify the repository has a single, unambiguous baseline migration path (no competing "initial" migrations).

## Implementation Checklist
- [x] Audit current `ApplicationDbContext` entities and the existing migrations under `Server/ClinicalIntelligence.Api/Migrations/`
- [x] Decide baseline strategy (clean baseline vs non-destructive transition) and document the decision
- [x] Standardize naming conventions for tables and migrations folder and document canonical `dotnet ef` commands
- [x] Decide and document pgvector prerequisite handling (auto-create extension vs clear failure)
- [x] Confirm strategy supports idempotent deployment scripts (document `dotnet ef migrations script --idempotent` usage)
- [x] Confirm strategy addresses partial failure recovery expectations (rollback guidance at minimum via documentation)
