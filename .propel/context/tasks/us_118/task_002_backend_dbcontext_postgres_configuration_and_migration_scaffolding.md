# Task - TASK_002

## Requirement Reference
- User Story: us_118
- Story Location: .propel/context/tasks/us_118/us_118.md
- Acceptance Criteria: 
    - AC-2: Given EF Core is configured, When the ApplicationDbContext is created, Then it successfully connects to PostgreSQL using the configured connection string
    - AC-3: Given migrations are initialized, When `dotnet ef migrations add InitialCreate` is executed, Then migration files are generated in the Data/Migrations folder

## Task Overview
Confirm `ApplicationDbContext` is correctly registered against PostgreSQL via the configured connection string and standardize the migrations output location so `dotnet ef migrations add` consistently generates files under `Data/Migrations`.
Estimated Effort: 6 hours

## Dependent Tasks
- .propel/context/tasks/us_117/task_003_backend_db_connectivity_and_health_latency_check.md (TASK_003)
- .propel/context/tasks/us_118/task_001_backend_efcore_packages_and_dotnet_ef_tooling.md (TASK_001)

## Impacted Components
- Server/ClinicalIntelligence.Api/Program.cs
- Server/ClinicalIntelligence.Api/Configuration/SecretsOptions.cs
- Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs
- Server/README.md

## Implementation Plan
- Confirm connection string resolution is a single source of truth (environment variable vs `ConnectionStrings:DefaultConnection`).
- Verify `Program.cs` registers `ApplicationDbContext` with `UseNpgsql(connectionString)` when PostgreSQL connection string is provided.
- Ensure the CLI migration command uses the standard output directory:
  - `--output-dir Data/Migrations`
- If the repository already contains existing migrations under a different folder, choose one approach and make it consistent:
  - Option A: Keep existing folder and update acceptance documentation + commands
  - Option B: Move/standardize to `Data/Migrations` and update commands + references
- Execute `dotnet ef migrations add InitialCreate` (or an appropriately named initial migration if one already exists) to validate scaffolding.
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Ensure Postgres is configured via `UseNpgsql` using the resolved connection string and supports migration execution workflows |
| MODIFY | Server/README.md | Standardize and document migration scaffolding commands, including `--output-dir Data/Migrations` |
| CREATE | Server/ClinicalIntelligence.Api/Data/Migrations/* | Migration files generated via `dotnet ef migrations add ... --output-dir Data/Migrations` |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/ef/core/managing-schemas/migrations/
- https://learn.microsoft.com/ef/core/cli/dotnet

## Build Commands
- dotnet build Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj

## Implementation Validation Strategy
- Verify the API starts successfully when `DATABASE_CONNECTION_STRING` is a valid PostgreSQL connection string.
- Verify `dotnet ef migrations add InitialCreate --project Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj --startup-project Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj --output-dir Data/Migrations` generates migration files in the correct folder.
- Verify invalid connection string produces a clear error message with configuration guidance (no secrets leakage).

## Implementation Checklist
- [x] Verify `SecretsOptions.ResolveDatabaseConnectionString` behavior for Postgres vs Development fallback
- [x] Verify `Program.cs` registers `ApplicationDbContext` with `UseNpgsql(connectionString)`
- [x] Align the canonical migrations output folder to `Data/Migrations` (or document the chosen standard)
- [x] Run `dotnet ef migrations add InitialCreate` with `--output-dir Data/Migrations` and confirm generated files
- [x] Confirm generated migration compiles and the project still builds
- [x] Validate invalid connection string path yields actionable error output without leaking credentials
