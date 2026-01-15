# Task - TASK_003

## Requirement Reference
- User Story: us_121
- Story Location: .propel/context/tasks/us_121/us_121.md
- Acceptance Criteria: 
    - AC-1: Given USER table exists, When unique index is created on email column, Then email lookups for authentication complete in <10ms
    - AC-2: Given DOCUMENT table exists, When composite index is created on (patient_id, upload_date), Then document listing queries with date filtering complete in <50ms
    - AC-3: Given PROCESSING_JOB table exists, When index is created on status column, Then job queue queries by status complete in <20ms
    - AC-4: Given EXTRACTED_ENTITY table exists, When index is created on patient_id, Then entity aggregation for Patient 360 completes in <100ms
    - AC-5: Given AUDIT_LOG_EVENT table exists, When index is created on timestamp, Then audit log queries with date range filtering complete in <100ms
    - AC-6: Given DOCUMENT_CHUNK table exists with vector column, When HNSW index is created with parameters (m=16, ef_construction=64), Then top-K similarity search (K=15) completes in <200ms for 768-dimensional vectors
    - AC-7: Given all indexes are created, When EXPLAIN ANALYZE is run on critical queries, Then index scans are used instead of sequential scans

## Task Overview
Extend the existing database integration test suite to verify:
1) required indexes exist by name, and
2) critical queries use index scans (EXPLAIN ANALYZE plan assertions), covering all US_121 acceptance criteria.
Estimated Effort: 6 hours

## Dependent Tasks
- .propel/context/tasks/us_121/task_002_backend_generate_migration_for_document_and_hnsw_indexes.md (TASK_002)

## Impacted Components
- Server/ClinicalIntelligence.Api.Tests/BaselineSchemaMigrationValidationTests.cs
- Server/ClinicalIntelligence.Api.Tests/

## Implementation Plan
- Add index presence assertions (fast, deterministic) by querying `pg_indexes`:
  - `ix_users_email`
  - `ix_documents_patient_id_uploaded_at`
  - `ix_processing_jobs_status`
  - `ix_extracted_entities_patient_id`
  - `ix_audit_log_events_timestamp`
  - `ix_document_chunks_embedding_hnsw`
- Add EXPLAIN-based assertions for the critical queries (AC-7):
  - Document listing query should include `Index Scan` or `Bitmap Index Scan` referencing `ix_documents_patient_id_uploaded_at`.
  - Processing job queue query should reference `ix_processing_jobs_status`.
  - Entity aggregation query should reference `ix_extracted_entities_patient_id`.
  - Audit log date range query should reference `ix_audit_log_events_timestamp`.
  - Vector similarity query should reference `ix_document_chunks_embedding_hnsw`.
- For EXPLAIN parsing:
  - Use `EXPLAIN (ANALYZE, BUFFERS, FORMAT TEXT)` and assert on plan text containing the expected index name.
  - Keep assertions tolerant to minor Postgres plan variations while still enforcing index usage.
- Ensure tests are isolated:
  - Use unique GUIDs per test.
  - Insert minimal rows needed for the query planner to choose the index.
  - Cleanup inserted data when possible.
  - Keep Postgres availability detection and skip behavior consistent with existing tests.
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api.Tests/BaselineSchemaMigrationValidationTests.cs | Add index existence assertions and EXPLAIN-based query plan assertions for US_121 performance/index usage requirements |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/ef/core/testing/
- https://www.postgresql.org/docs/current/using-explain.html
- https://github.com/pgvector/pgvector

## Build Commands
- dotnet test Server/ClinicalIntelligence.Api.Tests/ClinicalIntelligence.Api.Tests.csproj

## Implementation Validation Strategy
- Run integration tests against a Postgres database with migrations applied.
- Confirm all required index names exist.
- Confirm EXPLAIN plans reference the expected indexes (no sequential scans for the targeted filters).

## Implementation Checklist
- [x] Add pg_indexes-based tests for all required indexes in US_121
- [x] Add EXPLAIN plan assertions for document listing, job queue, entity aggregation, audit logs, and vector similarity queries
- [x] Ensure tests skip gracefully when Postgres is not available
- [x] Run the full test suite
