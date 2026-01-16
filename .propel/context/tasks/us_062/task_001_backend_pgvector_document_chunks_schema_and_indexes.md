# Task - [TASK_001]

## Requirement Reference
- User Story: [us_062]
- Story Location: [.propel/context/tasks/us_062/us_062.md]
- Acceptance Criteria: 
    - [Given chunks with embeddings, When stored, Then they are persisted in the document_chunks table with pgvector (FR-034).]
    - [Given chunk storage, When saved, Then metadata is preserved: document_id, page, section, coordinates, chunk_hash (FR-035).]
    - [Given vector storage, When indexed, Then HNSW index is used for efficient similarity search.]

## Task Overview
Enable chunk+embedding persistence in PostgreSQL using pgvector by wiring up the `document_chunks` table in EF Core, including correct `vector(768)` storage, metadata columns, referential integrity to `documents`, and required indexes (including HNSW for ANN search).

## Dependent Tasks
- [US_061] (Embeddings generation and association to chunk metadata)

## Impacted Components
- [MODIFY | Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs | Re-enable pgvector extension and EF mappings for `DocumentChunk` (and related entity mappings needed for relationships).]
- [MODIFY | Server/ClinicalIntelligence.Api/Domain/Models/Document.cs | Re-enable navigation from `Document` to `DocumentChunks` if required by mapping and cascade delete behavior.]
- [MODIFY | Server/ClinicalIntelligence.Api/Domain/Models/DocumentChunk.cs | Confirm fields match storage requirements (document_id, page, section, coordinates, text_content, embedding, token_count, chunk_hash).]
- [CREATE | Server/ClinicalIntelligence.Api/Migrations/<new_migration> | Add `document_chunks` table with `vector(768)` column and indexes, including HNSW index for similarity search.]
- [MODIFY | Server/ClinicalIntelligence.Api.Tests/BaselineSchemaMigrationValidationTests.cs | Add/adjust schema assertions for `document_chunks` (vector type + index presence) if needed.]

## Implementation Plan
- Confirm pgvector is enabled for PostgreSQL:
  - Ensure EF model enables the extension via `modelBuilder.HasPostgresExtension("vector")`.
  - Ensure developer workflow remains compatible with `scripts/db/enable_pgvector.sql` for restricted environments.
- Re-enable EF Core mappings for `DocumentChunk`:
  - Add `DbSet<DocumentChunk> DocumentChunks`.
  - Re-enable `ConfigureDocumentChunk(modelBuilder)` and ensure:
    - Table name: `document_chunks`
    - `Embedding` column type: `vector(768)`
    - Indexes:
      - `ix_document_chunks_document_id` (btree)
      - `ix_document_chunks_chunk_hash` (btree)
      - `ix_document_chunks_embedding_hnsw` (HNSW; cosine ops)
    - Dedupe constraint strategy:
      - Add a **unique** constraint on `(DocumentId, ChunkHash)` (to allow same content across different documents while preventing duplicates within a document).
- Create a new EF Core migration:
  - Add missing table(s)/constraints/indexes required for `DocumentChunk`.
  - Create HNSW index using pgvector cosine ops (`vector_cosine_ops`) with recommended parameters (`m=16`, `ef_construction=64`).
  - If production requires non-transactional index builds, document operational step to run a separate SQL script (see `Server/README.md` guidance).
- Validation:
  - Use existing validation mechanisms:
    - `scripts/db/validate_pgvector_hnsw.sql` for extension + HNSW sanity.
    - `BaselineSchemaMigrationValidationTests` for schema inspection and index existence.

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs | Enable pgvector extension + map `DocumentChunk` (and required navigation wiring) |
| MODIFY | Server/ClinicalIntelligence.Api/Domain/Models/Document.cs | Re-enable `DocumentChunks` navigation if needed by EF mapping |
| CREATE | Server/ClinicalIntelligence.Api/Migrations/<new_migration>.cs | Add `document_chunks` table, `vector(768)` column, btree indexes, HNSW index, unique constraint on (DocumentId, ChunkHash) |
| MODIFY | Server/ClinicalIntelligence.Api.Tests/BaselineSchemaMigrationValidationTests.cs | Ensure tests validate vector column and HNSW index presence |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://github.com/pgvector/pgvector
- https://github.com/pgvector/pgvector-dotnet
- https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [DB] Apply migrations on PostgreSQL and verify `document_chunks` exists and `Embedding` is `vector(768)`.
- [DB] Verify HNSW index exists and is created with `USING hnsw` and cosine ops.
- [Data] Insert a small set of chunks with embeddings and confirm unique constraint prevents duplicates for same `(DocumentId, ChunkHash)`.
- [Regression] Run `BaselineSchemaMigrationValidationTests` to confirm schema contracts.

## Implementation Checklist
- [ ] Re-enable pgvector extension usage in EF model (`HasPostgresExtension("vector")`)
- [ ] Re-enable `DocumentChunk` DbSet + mapping and validate column types and max lengths
- [ ] Add dedupe constraint on `(DocumentId, ChunkHash)`
- [ ] Create a migration that adds `document_chunks` and required indexes
- [ ] Add HNSW index for `Embedding` using cosine ops with recommended parameters
- [ ] Validate schema via test suite + `scripts/db/validate_pgvector_hnsw.sql`
