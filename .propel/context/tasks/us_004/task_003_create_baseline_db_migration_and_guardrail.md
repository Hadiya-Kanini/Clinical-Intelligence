# Task - TASK_003

## Requirement Reference
- User Story: us_004
- Story Location: .propel/context/tasks/us_004/us_004.md
- Acceptance Criteria: 
    - Given the database schema evolves over time, When a schema change is required, Then it is implemented through a migration mechanism (schema versioning) rather than ad-hoc manual edits.

## Task Overview
Create a baseline database migration and add a lightweight guardrail that makes the migration workflow explicit and repeatable (including where migration files live and how they are applied in development).

## Dependent Tasks
- .propel/context/tasks/us_004/task_002_scaffold_database_migration_mechanism.md (TASK_002)

## Impacted Components
- Server/ClinicalIntelligence.Api/

## Implementation Plan
- Create an initial EF Core migration and commit it to the repository.
- Define where migrations are stored and how they are applied locally (developer workflow).
- Add a minimal README entry in the Server area describing “no manual DB edits” and the expected migration process.
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api/Migrations/ | Add initial EF Core migration artifacts (baseline schema) |
| MODIFY | Server/README.md | Document the baseline migration workflow and “no manual schema edits” rule |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/ef/core/managing-schemas/migrations/

## Build Commands
- dotnet build Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj

## Implementation Validation Strategy
- Verify baseline migration can be applied to an empty database in a clean environment.
- Verify developer instructions clearly describe how to create new migrations and apply them.

## Implementation Checklist
- [x] Generate and commit an initial EF Core migration
- [x] Ensure migrations are stored in a consistent folder path
- [x] Update `Server/README.md` with migration workflow + guardrail guidance
