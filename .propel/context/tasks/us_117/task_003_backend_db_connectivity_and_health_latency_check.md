# Task - TASK_003

## Requirement Reference
- User Story: us_117
- Story Location: .propel/context/tasks/us_117/us_117.md
- Acceptance Criteria: 
    - AC-4: Given the database is installed, When connection is attempted from the Backend API, Then the connection succeeds using the configured connection string
    - AC-5: Given the installation is complete, When database health check is performed, Then PostgreSQL responds within 100ms

## Task Overview
Ensure the Backend API can reliably connect to PostgreSQL using the configured connection string and provide a concrete database health check that measures and enforces the 100ms responsiveness target.
Estimated Effort: 6 hours

## Dependent Tasks
- .propel/context/tasks/us_117/task_001_install_postgresql_15_and_configure_service.md (TASK_001)

## Impacted Components
- Server/ClinicalIntelligence.Api/Program.cs
- Server/ClinicalIntelligence.Api.Tests/

## Implementation Plan
- Confirm `SecretsOptions.ResolveDatabaseConnectionString` and `UseNpgsql` are the single source of truth for Postgres connectivity.
- Add a DB-aware health endpoint variant (or augment the existing `/health`) that:
  - Opens a connection to Postgres and runs a lightweight query (`SELECT 1`)
  - Measures round-trip time and includes it in the response
  - Fails the health check if latency exceeds 100ms
- Add automated tests validating:
  - API fails fast in non-Development if connection string is missing (already present)
  - When a valid Postgres connection string is provided, the DB health endpoint returns success (integration-style; can be guarded/skipped when Postgres not available)
- Ensure health endpoint behavior does not leak secrets in error messages.
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Add DB connectivity/latency-aware health check implementation while preserving existing `/health` behavior |
| CREATE | Server/ClinicalIntelligence.Api/Health/DatabaseHealthCheck.cs | Encapsulates DB ping/latency measurement and threshold evaluation |
| CREATE | Server/ClinicalIntelligence.Api.Tests/DatabaseHealthEndpointTests.cs | Validates DB health endpoint contract and non-sensitive error behavior (skippable if DB not available) |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/aspnet/core/host-and-deploy/health-checks
- https://www.npgsql.org/doc/basic-usage.html

## Build Commands
- dotnet build Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj
- dotnet test Server/ClinicalIntelligence.Api.Tests/ClinicalIntelligence.Api.Tests.csproj

## Implementation Validation Strategy
- Verify API connects to PostgreSQL using the configured `DATABASE_CONNECTION_STRING` (or `ConnectionStrings:DefaultConnection`).
- Verify DB health endpoint returns success when latency is below 100ms and returns failure when latency exceeds 100ms.
- Verify error messages from health check do not include connection strings or credentials.

## Implementation Checklist
- [x] Implement `DatabaseHealthCheck` to run `SELECT 1` and capture latency
- [x] Add/augment API health endpoint to include DB connectivity + latency and enforce 100ms threshold
- [x] Add tests for the health endpoint contract and non-sensitive error output
- [x] Validate behavior in Development (SQLite default) vs explicit PostgreSQL configuration
