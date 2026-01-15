# Task - [TASK_002]

## Requirement Reference
- User Story: [us_020]
- Story Location: [.propel/context/tasks/us_020/us_020.md]
- Acceptance Criteria: 
    - [Given input sanitization, When implemented, Then parameterized queries are used for all database operations.]
    - [Given any user input, When processed by the backend, Then it is sanitized against SQL injection attacks.]

## Task Overview
Establish a backend guardrail to ensure all database operations are performed safely using EF Core parameterization patterns (and safe parameter binding for any direct Npgsql usage), and prevent introduction of raw SQL string concatenation in future code.

## Dependent Tasks
- [N/A]

## Impacted Components
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Review existing EF Core + Npgsql usage to ensure safe patterns remain (no ad-hoc raw SQL) and document/centralize DB access conventions]
- [MODIFY | Server/ClinicalIntelligence.Api/Health/DatabaseHealthCheck.cs | Confirm direct SQL statements remain constant and not built from user input]
- [MODIFY | Server/ClinicalIntelligence.Api/Diagnostics/DbPoolMetricsSnapshot.cs | Confirm direct SQL statements remain constant and not built from user input]
- [MODIFY | Server/ClinicalIntelligence.Api/Data/DatabaseWarmupHostedService.cs | Confirm direct SQL statements remain constant and not built from user input]

## Implementation Plan
- Inventory DB access patterns in the current API:
  - EF Core LINQ queries (parameterized by default).
  - Direct Npgsql usage for health check, pool metrics, and warmup.
- Define a minimal “safe query” policy for the codebase:
  - For EF Core: prefer LINQ; avoid `FromSqlRaw`/`ExecuteSqlRaw` unless strictly necessary; if needed, use interpolated variants or parameter objects.
  - For Npgsql: use `NpgsqlCommand` parameters for any user-influenced values; never concatenate user-provided strings into SQL.
- Add automated enforcement to reduce regression risk:
  - Add a lightweight test that scans `Server/ClinicalIntelligence.Api/**/*.cs` (excluding `obj/bin`) for known risky APIs/usages (e.g., `FromSqlRaw`, `ExecuteSqlRaw`) and fails if introduced.
  - Keep the rule narrow to avoid false positives.

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api.Tests/Security/SqlInjectionGuardrailTests.cs | Add a guardrail test to detect introduction of risky raw SQL APIs/usages in production code |
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Verify existing DB access is via EF Core LINQ and ensure any future raw SQL guidance is enforced via the guardrail test |
| MODIFY | Server/ClinicalIntelligence.Api/Health/DatabaseHealthCheck.cs | Validate constant query usage remains safe (no user input) |
| MODIFY | Server/ClinicalIntelligence.Api/Diagnostics/DbPoolMetricsSnapshot.cs | Validate constant query usage remains safe (no user input) |
| MODIFY | Server/ClinicalIntelligence.Api/Data/DatabaseWarmupHostedService.cs | Validate constant query usage remains safe (no user input) |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/ef/core/querying/sql-queries

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Automated] Run backend test suite; ensure `SqlInjectionGuardrailTests` fails if risky raw SQL APIs are introduced.
- [Code review] Confirm any Npgsql command uses parameters (if any new user-influenced SQL is added).

## Implementation Checklist
- [ ] Inventory current DB access paths (EF Core LINQ vs any direct SQL)
- [ ] Confirm no user input is ever used to construct SQL text in existing Npgsql usage
- [ ] Add `SqlInjectionGuardrailTests` to prevent introduction of risky raw SQL patterns
- [ ] Verify guardrail excludes generated build artifacts (`obj/`, `bin/`) to avoid noise
- [ ] Confirm acceptance criteria coverage for parameterized queries (AC: “parameterized queries are used for all database operations”)
