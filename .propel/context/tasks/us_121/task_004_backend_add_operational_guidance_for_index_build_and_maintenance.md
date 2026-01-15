# Task - TASK_004

## Requirement Reference
- User Story: us_121
- Story Location: .propel/context/tasks/us_121/us_121.md
- Acceptance Criteria: 
    - AC-6: Given DOCUMENT_CHUNK table exists with vector column, When HNSW index is created with parameters (m=16, ef_construction=64), Then top-K similarity search (K=15) completes in <200ms for 768-dimensional vectors

## Task Overview
Document operational guidance for safe index creation and long-term maintenance for US_121 indexes, including monitoring long-running `CONCURRENTLY` builds, HNSW tuning guidance, and reindex/health monitoring.
Estimated Effort: 4 hours

## Dependent Tasks
- .propel/context/tasks/us_121/task_002_backend_generate_migration_for_document_and_hnsw_indexes.md (TASK_002)

## Impacted Components
- Server/README.md
- scripts/db/*

## Implementation Plan
- Add index build guidance (production-safe defaults):
  - Use `CREATE INDEX CONCURRENTLY` for large tables.
  - Use `pg_stat_progress_create_index` to monitor progress.
  - Document expected runtime factors (table size, I/O throughput, CPU, maintenance_work_mem).
- Add HNSW tuning notes for `m` and `ef_construction`:
  - Default: `m = 16`, `ef_construction = 64`.
  - Guidance for memory pressure / OOM scenarios (reduce `m` or `ef_construction`, increase memory if possible).
  - Note `ef_search` is a query-time parameter and should be tuned separately.
- Add index health / bloat monitoring guidance:
  - Use `pg_stat_user_indexes` to monitor scan counts and index usage.
  - Provide REINDEX guidance for bloat / maintenance windows.
  - Include ANALYZE guidance if query planner ignores indexes.
- Align documentation with existing DB scripts:
  - Reference `scripts/db/validate_pgvector_hnsw.sql` for validation and example DDL.
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/README.md | Add operational guidance for index build monitoring, HNSW parameter tuning, and reindex/ANALYZE recommendations |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://www.postgresql.org/docs/current/progress-reporting.html#PROGRESS-CREATE-INDEX
- https://www.postgresql.org/docs/current/monitoring-stats.html
- https://www.postgresql.org/docs/current/sql-reindex.html
- https://github.com/pgvector/pgvector

## Build Commands
- dotnet build Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj

## Implementation Validation Strategy
- Verify documentation includes:
  - monitoring query for progress
  - HNSW tuning guidance
  - reindex and analyze guidance

## Implementation Checklist
- [x] Document progress monitoring using `pg_stat_progress_create_index`
- [x] Add HNSW parameter tuning notes (m, ef_construction, ef_search)
- [x] Document index usage monitoring via `pg_stat_user_indexes`
- [x] Document REINDEX and ANALYZE guidance for long-term maintenance
