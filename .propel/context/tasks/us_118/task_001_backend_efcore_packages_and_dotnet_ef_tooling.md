# Task - TASK_001

## Requirement Reference
- User Story: us_118
- Story Location: .propel/context/tasks/us_118/us_118.md
- Acceptance Criteria: 
    - AC-1: Given the .NET Backend API project, When EF Core packages are installed, Then Microsoft.EntityFrameworkCore (8.x), Npgsql.EntityFrameworkCore.PostgreSQL, and Microsoft.EntityFrameworkCore.Design are available

## Task Overview
Ensure the Backend API project has the correct EF Core + Npgsql provider packages and developer tooling to run `dotnet ef` commands consistently across environments.
Estimated Effort: 4 hours

## Dependent Tasks
- .propel/context/tasks/us_117/task_001_install_postgresql_15_and_configure_service.md (TASK_001)

## Impacted Components
- Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj
- Server/README.md

## Implementation Plan
- Confirm the API project targets .NET 8 and references:
  - `Microsoft.EntityFrameworkCore` (8.x)
  - `Npgsql.EntityFrameworkCore.PostgreSQL` (8.x)
  - `Microsoft.EntityFrameworkCore.Design` (8.x)
  - `Microsoft.EntityFrameworkCore.Tools` (8.x)
- Normalize EF Core package versions to a consistent 8.x patch level to reduce tooling/runtime mismatch risk.
- Ensure `dotnet-ef` tooling is available for developers:
  - Prefer local tool via a tool manifest if the repository standardizes on it; otherwise document global install (`dotnet tool install --global dotnet-ef`).
- Validate the solution builds after package changes.
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj | Ensure EF Core + Npgsql provider packages are present and align versions to EF Core 8.x consistency |
| MODIFY | Server/README.md | Document the supported `dotnet ef` installation approach (global vs local tool manifest) and basic migration commands |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/ef/core/cli/dotnet
- https://www.npgsql.org/efcore/

## Build Commands
- dotnet build Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj

## Implementation Validation Strategy
- Verify `dotnet build` succeeds for the API project after package/tooling changes.
- Verify `dotnet ef --version` runs successfully on a clean dev machine profile.

## Implementation Checklist
- [x] Audit existing EF Core / Npgsql package references and versions
- [x] Align EF Core packages to consistent 8.x versions (EFCore / Design / Tools)
- [x] Confirm Npgsql EF Core provider package is present and compatible
- [x] Decide and document `dotnet-ef` installation approach (local tool manifest vs global)
- [x] Validate `dotnet build` succeeds for API project
- [x] Validate `dotnet ef --version` works and points at the expected SDK/runtime
