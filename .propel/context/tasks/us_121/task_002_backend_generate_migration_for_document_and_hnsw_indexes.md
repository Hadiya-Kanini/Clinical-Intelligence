# Task - TASK_002

## Requirement Reference
- User Story: us_121
- Story Location: .propel/context/tasks/us_121/us_121.md
- Acceptance Criteria: 
    - AC-2: Given DOCUMENT table exists, When composite index is created on (patient_id, upload_date), Then document listing queries with date filtering complete in <50ms
    - AC-6: Given DOCUMENT_CHUNK table exists with vector column, When HNSW index is created with parameters (m=16, ef_construction=64), Then top-K similarity search (K=15) completes in <200ms for 768-dimensional vectors

## Task Overview
Create an EF Core migration that adds the missing database indexes required by US_121 that are not already covered by the baseline migrations, including a composite B-tree index for document listing and a pgvector HNSW index for approximate nearest neighbor search.
Estimated Effort: 8 hours

## Dependent Tasks
- .propel/context/tasks/us_121/task_001_backend_define_index_targets_and_naming.md (TASK_001)

## Impacted Components
- Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs
- Server/ClinicalIntelligence.Api/Migrations/*
- Server/ClinicalIntelligence.Api/Data/ApplicationDbContextModelSnapshot.cs

## Implementation Plan
- Add/confirm the EF Core composite index for DOCUMENT listing queries:
  - Target: `documents (PatientId, UploadedAt)`
  - Target name: `ix_documents_patient_id_uploaded_at`
  - Implement in `ConfigureDocument(...)`.
- Generate a dedicated migration for US_121 indexing updates (only index changes; no table/column drift).
- Add HNSW index creation via migration SQL:
  - Target: `document_chunks (Embedding)`
  - Target name: `ix_document_chunks_embedding_hnsw`
  - DDL (pgvector):
    - `CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_document_chunks_embedding_hnsw ON document_chunks USING hnsw ("Embedding" vector_cosine_ops) WITH (m = 16, ef_construction = 64);`
  - Ensure the index is created outside a transaction (required by `CONCURRENTLY`).
  - If the EF provider does not allow suppressing the migration transaction for this statement, split HNSW creation into a separate migration that:
    - uses a provider-supported non-transactional execution option
    - or documents the operational step to apply this statement separately in deployment (only if strictly necessary).
- Validate idempotency and duplicate handling:
  - Confirm the DDL is safe to re-run (IF NOT EXISTS).
- Performance/safety considerations:
  - Prefer `CONCURRENTLY` for production to avoid write locks.
  - Document progress monitoring query using `pg_stat_progress_create_index`.
  - Ensure the migration is safe to apply on large tables.
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs | Add the DOCUMENT composite index definition and keep naming consistent (`ix_documents_patient_id_uploaded_at`) |
| CREATE | Server/ClinicalIntelligence.Api/Migrations/*_US121_DatabaseIndexingStrategy.cs | Adds the DOCUMENT composite index and executes SQL to create the HNSW index on document_chunks.embedding |
| MODIFY | Server/ClinicalIntelligence.Api/Migrations/ApplicationDbContextModelSnapshot.cs | Snapshot updates reflecting new index definitions |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/ef/core/managing-schemas/migrations/
- https://learn.microsoft.com/ef/core/modeling/indexes
- https://www.postgresql.org/docs/current/sql-createindex.html
- https://github.com/pgvector/pgvector

## Build Commands
- dotnet build Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj

## Implementation Validation Strategy
- Apply the migration to a clean Postgres database and ensure it succeeds.
- Apply the migration to an existing baseline database (with US_119 and US_120 applied) and ensure it succeeds.
- Confirm `pg_indexes` contains:
  - `ix_documents_patient_id_uploaded_at`
  - `ix_document_chunks_embedding_hnsw`

## Implementation Checklist
- [x] Implement/confirm `ix_documents_patient_id_uploaded_at` in EF fluent mappings
- [x] Generate a dedicated US_121 migration and inspect SQL output for correctness
- [x] Add the HNSW index creation SQL and ensure `CONCURRENTLY` is applied safely (non-transactional)
- [x] Validate idempotency behavior (`IF NOT EXISTS`) on repeated execution
- [x] Build the API project
