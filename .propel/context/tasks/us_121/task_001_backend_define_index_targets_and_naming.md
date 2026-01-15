# Task - TASK_001

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

## Task Overview
Define the concrete index inventory (including exact index names and target columns as implemented in the current EF Core model) for US_121, identify gaps vs acceptance criteria, and align on conventions for creating the remaining indexes safely (including the pgvector HNSW index).
Estimated Effort: 6 hours

## Dependent Tasks
- .propel/context/tasks/us_119/task_004_backend_constraints_indexes_and_schema_validation_for_core_16_tables.md (TASK_004)
- .propel/context/tasks/us_120/task_002_backend_generate_fk_constraints_migration_for_existing_schema.md (TASK_002)

## Impacted Components
- Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs
- Server/ClinicalIntelligence.Api/Migrations/*
- Server/ClinicalIntelligence.Api.Tests/BaselineSchemaMigrationValidationTests.cs

## Implementation Plan
- Confirm current EF-defined indexes already present for US_121 targets:
  - `users.email` unique index name: `ix_users_email` (AC-1)
  - `processing_jobs.status` index name: `ix_processing_jobs_status` (AC-3)
  - `extracted_entities.patient_id` index name: `ix_extracted_entities_patient_id` (AC-4)
  - `audit_log_events.timestamp` index name: `ix_audit_log_events_timestamp` (AC-5)
- Identify gaps where acceptance criteria require a different or additional index:
  - DOCUMENT composite index:
    - Current columns are `documents.PatientId` + `documents.UploadedAt` (note: story says `upload_date`, but schema uses `UploadedAt`).
    - Define target index name to be created: `ix_documents_patient_id_uploaded_at` (AC-2)
  - DOCUMENT_CHUNK HNSW index:
    - EF does not generate HNSW indexes directly; plan to create via raw SQL in migration.
    - Define target index name to be created: `ix_document_chunks_embedding_hnsw` (AC-6)
    - Target expression: `USING hnsw ("Embedding" vector_cosine_ops) WITH (m = 16, ef_construction = 64)`
- Confirm idempotency and safety constraints:
  - Prefer `CREATE INDEX IF NOT EXISTS` for non-concurrent index operations.
  - For large tables, prefer `CREATE INDEX CONCURRENTLY` and ensure the EF migration does not wrap the operation in a transaction.
- Define validation query set to be used later in tests:
  - Authentication lookup: `SELECT * FROM users WHERE "Email" = ...`
  - Document listing: `SELECT * FROM documents WHERE "PatientId" = ... AND "UploadedAt" BETWEEN ... ORDER BY "UploadedAt" DESC LIMIT 20`
  - Processing job queue: `SELECT * FROM processing_jobs WHERE "Status" = ... ORDER BY "Id" LIMIT 50`
  - Entity aggregation: `SELECT * FROM extracted_entities WHERE "PatientId" = ...`
  - Audit log range: `SELECT * FROM audit_log_events WHERE "Timestamp" BETWEEN ... ORDER BY "Timestamp" DESC LIMIT 100`
  - Vector similarity: `SELECT "Id" FROM document_chunks ORDER BY "Embedding" <=> :queryVector LIMIT 15`
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs | Add/adjust EF index definitions needed for US_121 (notably the DOCUMENT composite index) and align database index naming for stable validation |
| MODIFY | Server/ClinicalIntelligence.Api.Tests/BaselineSchemaMigrationValidationTests.cs | Add/adjust validation queries that will be used for EXPLAIN-based assertions in US_121 tests |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/ef/core/modeling/indexes
- https://www.postgresql.org/docs/current/sql-createindex.html
- https://www.postgresql.org/docs/current/progress-reporting.html#PROGRESS-CREATE-INDEX
- https://github.com/pgvector/pgvector

## Build Commands
- dotnet build Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj

## Implementation Validation Strategy
- Verify a complete, concrete index inventory exists that maps each US_121 acceptance criterion to:
  - exact table
  - exact column(s)
  - exact index name
  - creation approach (EF index vs migration SQL)

## Implementation Checklist
- [x] Confirm all existing index names relevant to US_121 (users, processing_jobs, extracted_entities, audit_log_events)
- [x] Define the exact missing index names and DDL for DOCUMENT composite and DOCUMENT_CHUNK HNSW
- [x] Confirm approach for `CONCURRENTLY` + non-transactional migration for large tables
- [x] Define the final EXPLAIN validation query set to be used by integration tests
