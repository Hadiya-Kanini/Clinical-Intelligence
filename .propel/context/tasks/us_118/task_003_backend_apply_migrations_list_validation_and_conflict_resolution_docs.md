# Task - TASK_003

## Requirement Reference
- User Story: us_118
- Story Location: .propel/context/tasks/us_118/us_118.md
- Acceptance Criteria: 
    - AC-4: Given a migration exists, When `dotnet ef database update` is executed, Then the migration is applied to the target database
    - AC-5: Given migrations are applied, When `dotnet ef migrations list` is executed, Then all applied and pending migrations are displayed with timestamps

## Task Overview
Validate end-to-end EF Core migration execution against PostgreSQL (apply, re-run idempotency expectations) and document operational guidance for migration conflicts and failure recovery.
Estimated Effort: 6 hours

## Dependent Tasks
- .propel/context/tasks/us_118/task_002_backend_dbcontext_postgres_configuration_and_migration_scaffolding.md (TASK_002)

## Impacted Components
- Server/README.md
- Server/ClinicalIntelligence.Api.Tests/

## Implementation Plan
- Ensure a PostgreSQL database is available per US_117 (local dev) and connection string is configured.
- Apply migrations to the target database using `dotnet ef database update`.
- Validate migration status reporting using `dotnet ef migrations list`.
- Add automated test coverage where feasible (without forcing a hard dependency on Postgres for all CI runs):
  - Prefer integration-style tests that can be conditionally skipped when Postgres is unavailable.
- Document edge cases and operational guidance:
  - Invalid connection string troubleshooting (configuration sources and examples)
  - Migration conflicts between branches (recommended process using `dotnet ef migrations remove` and re-scaffolding)
  - Failure mid-execution (rollback expectations, logs to check, safe retry guidance)
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/README.md | Add validated commands for `database update` and `migrations list`, plus branch-conflict and failure recovery guidance |
| CREATE | Server/ClinicalIntelligence.Api.Tests/EfMigrationsWorkflowTests.cs | Integration-style tests validating migrations can be applied and queried (skippable when Postgres not available) |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/ef/core/managing-schemas/migrations/applying
- https://learn.microsoft.com/ef/core/cli/dotnet

## Build Commands
- dotnet build Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj
- dotnet test Server/ClinicalIntelligence.Api.Tests/ClinicalIntelligence.Api.Tests.csproj

## Implementation Validation Strategy
- Verify `dotnet ef database update --project Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj` applies migrations successfully to the configured PostgreSQL database.
- Verify `dotnet ef migrations list --project Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj` outputs applied/pending migrations with timestamps.
- Verify documentation includes clear conflict-resolution and recovery steps for common failure modes.

## Implementation Checklist
- [x] Apply migrations with `dotnet ef database update` against Postgres using the configured connection string
- [x] Validate `dotnet ef migrations list` output includes applied/pending migrations with timestamps
- [x] Add integration-style tests for migration apply/list behavior (skippable when Postgres not present)
- [x] Document migration conflict handling steps for branching workflows
- [x] Document failure recovery steps for partial migration execution
- [x] Ensure docs avoid leaking secrets and provide non-sensitive troubleshooting guidance
