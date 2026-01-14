# Task - TASK_002

## Requirement Reference
- User Story: us_004
- Story Location: .propel/context/tasks/us_004/us_004.md
- Acceptance Criteria: 
    - Given the database schema evolves over time, When a schema change is required, Then it is implemented through a migration mechanism (schema versioning) rather than ad-hoc manual edits.

## Task Overview
Introduce a concrete database migration mechanism in the .NET Backend API by scaffolding an EF Core-based persistence layer (DbContext + provider packages) and wiring it into the API startup, so all future schema changes are captured as migrations.

## Dependent Tasks
- .propel/context/tasks/us_001/task_001_scaffold_service_structure.md (TASK_001)

## Impacted Components
- Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj
- Server/ClinicalIntelligence.Api/Program.cs

## Implementation Plan
- Add EF Core packages (including the chosen relational provider) to the backend solution.
- Introduce a `DbContext` in a dedicated namespace/project to keep persistence concerns separated from API endpoint composition.
- Register the `DbContext` in DI and configure via connection string from configuration/environment.
- Add a minimal “migration workflow” entry point (documented commands) so developers can create/apply migrations consistently.
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj | Add EF Core packages required for migrations and runtime DB access |
| CREATE | Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs | Add EF Core `DbContext` (initially empty/minimal, expanded by future features) |
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Register `ApplicationDbContext` in DI and configure connection string |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/ef/core/managing-schemas/migrations/

## Build Commands
- dotnet build Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj

## Implementation Validation Strategy
- Verify the API boots with a configured connection string and can instantiate the `DbContext`.
- Verify the repository has a documented, repeatable command sequence for creating/applying migrations.

## Implementation Checklist
- [x] Add EF Core runtime + tooling packages
- [x] Create `ApplicationDbContext` (persistence boundary)
- [x] Register `ApplicationDbContext` in `Program.cs` with config-driven connection string
- [x] Document the standard developer workflow for creating/applying migrations
