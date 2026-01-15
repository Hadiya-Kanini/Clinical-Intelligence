# Task - TASK_001

## Requirement Reference
- User Story: us_122
- Story Location: .propel/context/tasks/us_122/us_122.md
- Acceptance Criteria: 
    - AC-1: Given Backend API is configured, When connection pool is initialized at startup, Then minimum 10 connections are pre-established and validated
    - AC-2: Given connection pool is active with 10 connections, When concurrent requests exceed 10, Then additional connections are created dynamically up to maximum 100
    - AC-3: Given maximum 100 connections are in use, When a new connection is requested, Then the request queues with configurable timeout (default 30 seconds)
    - AC-4: Given a connection has been idle for more than 5 minutes, When pool maintenance runs, Then idle connections above minimum are closed
    - AC-6: Given Npgsql connection string, When pooling parameters are set, Then `Minimum Pool Size=10;Maximum Pool Size=100;Connection Idle Lifetime=300;Connection Pruning Interval=10` are applied

## Task Overview
Configure PostgreSQL connection pooling defaults for the Backend API by normalizing the resolved Npgsql connection string to include the required pool sizing and maintenance parameters, and add a startup warm-up that pre-establishes and validates the minimum pool size.
Estimated Effort: 8 hours

## Dependent Tasks
- .propel/context/tasks/us_117/task_003_backend_db_connectivity_and_health_latency_check.md (TASK_003)

## Impacted Components
- Server/ClinicalIntelligence.Api/Configuration/SecretsOptions.cs
- Server/ClinicalIntelligence.Api/Program.cs
- Server/ClinicalIntelligence.Api/Health/DatabaseHealthCheck.cs
- Server/ClinicalIntelligence.Api.Tests/

## Implementation Plan
- Implement a single, centralized connection string normalization step for PostgreSQL only:
  - Use `NpgsqlConnectionStringBuilder` to parse and re-emit the connection string
  - Apply pooling defaults:
    - `Minimum Pool Size=10`
    - `Maximum Pool Size=100`
    - `Connection Idle Lifetime=300` (seconds)
    - `Connection Pruning Interval=10` (seconds)
  - Apply a pool wait timeout default (configurable) for pool exhaustion behavior:
    - Default to 30 seconds
    - Ensure the value is applied via the relevant Npgsql connection string parameter (verify via Npgsql docs)
  - Ensure SQLite connection strings are not modified.
- Add a startup warm-up step that pre-establishes the minimum pool size:
  - Implement an `IHostedService` which opens and validates 10 connections at startup and disposes them back to the pool
  - Validation should be lightweight (e.g., `SELECT 1`) to satisfy AC-1 “validated” requirement
  - On warm-up failure, fail application startup with a clear, non-sensitive message (no credentials/connection string in logs).
- Ensure all DB usage paths use the normalized connection string:
  - EF Core `UseNpgsql(...)`
  - Health checks / DB ping code
- Add tests:
  - Unit test that PostgreSQL connection strings get the required pooling parameters applied (AC-6)
  - Unit test that SQLite connection strings are not modified
  - Startup test that warm-up does not leak secrets on failure (guarded/skippable if DB not available).
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Configuration/SecretsOptions.cs | Normalize Postgres connection string by applying Npgsql pooling parameters and configurable pool wait timeout defaults while leaving SQLite unchanged |
| CREATE | Server/ClinicalIntelligence.Api/Data/DatabaseWarmupHostedService.cs | Pre-establish and validate minimum pool size (10 connections) at startup and fail fast on initialization failure |
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Use the normalized Postgres connection string everywhere (EF Core + health checks) and register the warm-up hosted service |
| MODIFY | Server/ClinicalIntelligence.Api/Health/DatabaseHealthCheck.cs | Ensure the health check uses the normalized connection string (no divergence from runtime DB settings) |
| CREATE | Server/ClinicalIntelligence.Api.Tests/DatabaseConnectionPoolingConfigurationTests.cs | Validates pooling parameter normalization behavior (Postgres vs SQLite) |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://www.npgsql.org/doc/connection-string-parameters.html
- https://www.npgsql.org/doc/performance.html
- https://learn.microsoft.com/ef/core/dbcontext-configuration/

## Build Commands
- dotnet build Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj
- dotnet test Server/ClinicalIntelligence.Api.Tests/ClinicalIntelligence.Api.Tests.csproj

## Implementation Validation Strategy
- Confirm the resolved PostgreSQL connection string contains the expected pooling parameters (AC-6).
- Validate warm-up establishes the minimum pool size by successfully opening and validating 10 connections at startup (AC-1).
- Validate pool behavior under concurrency by running a local load test and confirming connections grow up to the configured maximum (AC-2).
- Validate pool exhaustion behavior by temporarily constraining max pool size and confirming new connection requests wait and time out based on the configured default (AC-3).
- Validate idle connections above the minimum are pruned after 5 minutes (AC-4) by observing pool behavior via logs/metrics.

## Implementation Checklist
- [x] Add Postgres-only connection string normalization using `NpgsqlConnectionStringBuilder`
- [x] Apply required pooling defaults and a configurable pool wait timeout default
- [x] Implement and register startup warm-up hosted service to pre-open 10 connections and validate with `SELECT 1`
- [x] Ensure EF Core and health checks both use the normalized connection string
- [x] Add unit tests for Postgres pooling parameter application and SQLite passthrough
- [x] Build and run tests
