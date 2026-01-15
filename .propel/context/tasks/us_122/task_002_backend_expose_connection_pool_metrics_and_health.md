# Task - TASK_002

## Requirement Reference
- User Story: us_122
- Story Location: .propel/context/tasks/us_122/us_122.md
- Acceptance Criteria: 
    - AC-5: Given connection pool is configured, When pool statistics are queried, Then active, idle, and total connection counts are available for monitoring

## Task Overview
Expose a concrete monitoring path for PostgreSQL pool statistics (active/idle/total connections) by enabling Npgsql metrics collection and surfacing the required counts via a lightweight API endpoint or health/diagnostics route.
Estimated Effort: 8 hours

## Dependent Tasks
- .propel/context/tasks/us_122/task_001_backend_configure_npgsql_connection_pooling.md (TASK_001)

## Impacted Components
- Server/ClinicalIntelligence.Api/Program.cs
- Server/ClinicalIntelligence.Api/Health/DatabaseHealthCheck.cs
- Server/ClinicalIntelligence.Api.Tests/

## Implementation Plan
- Decide on a single monitoring mechanism consistent with existing health endpoints:
  - Option A (preferred if feasible): Add a dedicated endpoint (e.g., `/health/db/pool`) returning:
    - active connection count
    - idle connection count
    - total connection count
    - configured min/max values
  - Option B: Extend existing `/health/db` output with pool fields (only if it won’t break existing consumers).
- Implement pool statistics collection using Npgsql’s diagnostics/metrics integration:
  - Use the `System.Diagnostics.Metrics` API and Npgsql’s emitted metrics (Npgsql 8 supports OpenTelemetry-style metrics)
  - Capture per-pool counts (idle/used) and max configuration
  - Ensure no sensitive connection string data is emitted in tags or responses.
- Add unit/integration tests:
  - If Postgres is available, assert the endpoint returns numeric fields for active/idle/total and that they are internally consistent (total == active + idle).
  - If Postgres is not available, validate endpoint returns a non-sensitive failure or is skipped.
- Document how to query the pool metrics during development:
  - Provide a command line validation approach using `dotnet-counters` against the running process (development-only validation).
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Register any required metrics/diagnostics components and map an endpoint (or extend `/health/db`) to return pool statistics |
| MODIFY | Server/ClinicalIntelligence.Api/Health/DatabaseHealthCheck.cs | Optionally include pool-related diagnostic fields if using the health route approach |
| CREATE | Server/ClinicalIntelligence.Api/Diagnostics/DbPoolMetricsSnapshot.cs | Encapsulate reading current pool counters (active/idle/total) from metrics/diagnostics sources |
| CREATE | Server/ClinicalIntelligence.Api.Tests/DatabasePoolMetricsEndpointTests.cs | Verifies pool statistics endpoint contract and non-sensitive output (skippable if DB not available) |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://www.npgsql.org/doc/diagnostics/metrics.html
- https://www.npgsql.org/doc/release-notes/8.0.html
- https://learn.microsoft.com/dotnet/core/diagnostics/dotnet-counters

## Build Commands
- dotnet build Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj
- dotnet test Server/ClinicalIntelligence.Api.Tests/ClinicalIntelligence.Api.Tests.csproj

## Implementation Validation Strategy
- Run the API and query the pool stats endpoint; validate the response includes active, idle, and total connection counts (AC-5).
- Validate pool stats do not leak the connection string, host, username, or password.
- Validate consistency: `total == active + idle`.

## Implementation Checklist
- [x] Choose endpoint strategy for pool stats (new route vs augment `/health/db`)
- [x] Implement pool snapshot collection using Npgsql metrics/diagnostics
- [x] Ensure response contains active/idle/total fields and is non-sensitive
- [x] Add tests validating contract and consistency (and skip when Postgres unavailable)
- [x] Validate metrics emission via `dotnet-counters` during development
