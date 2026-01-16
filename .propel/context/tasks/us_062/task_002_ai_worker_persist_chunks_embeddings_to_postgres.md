# Task - [TASK_002]

## Requirement Reference
- User Story: [us_062]
- Story Location: [.propel/context/tasks/us_062/us_062.md]
- Acceptance Criteria: 
    - [Given chunks with embeddings, When stored, Then they are persisted in pgvector with metadata.]
    - [Given chunk storage, When saved, Then metadata is preserved: document_id, page, section, coordinates, chunk_hash (FR-035).]

## Task Overview
Extend the Python AI worker to persist chunk records (text + metadata + 768-d embedding) into PostgreSQL `document_chunks` using pgvector. Persistence must be idempotent to support retries (dedupe using `chunk_hash`) and must handle mid-batch failures safely.

## Dependent Tasks
- [US_061] (Embeddings generation and association to chunk metadata)
- [US_062 TASK_001] (Backend schema/migration adds `document_chunks`, `vector(768)` column, and dedupe constraint/indexes)

## Impacted Components
- [MODIFY | worker/requirements.txt | Add PostgreSQL client dependencies for persistence (e.g., psycopg).]
- [MODIFY | worker/config.py | Add configuration for PostgreSQL connection string (reuse root `DATABASE_CONNECTION_STRING`).]
- [CREATE | worker/storage/document_chunk_store.py | Encapsulate insert/upsert logic for `document_chunks` with batching, dedupe, and failure handling.]
- [CREATE | worker/tests/test_document_chunk_store.py | Unit tests for dedupe/upsert SQL shape and error handling (DB integration tests optional/gated).]

## Implementation Plan
- Add worker configuration for database access:
  - Read `DATABASE_CONNECTION_STRING` from environment (same as backend), fail fast when missing.
  - Keep secrets out of source control (align with `.env.example`).
- Add database dependency:
  - Use a lightweight PostgreSQL driver suitable for Windows dev and CI (e.g., psycopg).
- Implement idempotent persistence for chunks:
  - Write an insert/upsert statement targeting `document_chunks`.
  - Use the dedupe constraint created in TASK_001 (recommended: unique `(DocumentId, ChunkHash)`) and implement:
    - `INSERT ... ON CONFLICT ("DocumentId", "ChunkHash") DO UPDATE SET ...` (update `Embedding`, `TextContent`, `TokenCount`, and metadata fields as needed).
  - Ensure the embedding is inserted as `vector(768)`:
    - Use explicit casting in SQL (`$1::vector`) or a compatible parameter adaptation strategy.
- Batch safety:
  - Use a single transaction per document/job batch.
  - If any row insert fails, rollback the transaction and raise an exception so job retry semantics remain correct.
  - Ensure retry is safe due to idempotent upsert.
- Optional pre-flight checks (non-blocking for local dev):
  - Verify `pg_extension` contains `vector` on startup and emit a clear error if not.

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | worker/requirements.txt | Add PostgreSQL driver dependency for persistence |
| MODIFY | worker/config.py | Load `DATABASE_CONNECTION_STRING` for worker DB access |
| CREATE | worker/storage/document_chunk_store.py | Insert/upsert `document_chunks` rows with metadata + embeddings |
| CREATE | worker/tests/test_document_chunk_store.py | Validate dedupe behavior and error handling |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://github.com/pgvector/pgvector
- https://www.psycopg.org/psycopg3/

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Local] With PostgreSQL configured, run worker persistence routine against a seeded `documents` row and confirm `document_chunks` rows are inserted.
- [Idempotency] Re-run the same batch and confirm no duplicate rows are created (upsert occurs or no-op).
- [Failure] Force an error mid-batch (e.g., invalid vector length) and confirm the transaction rolls back.

## Implementation Checklist
- [ ] Add PostgreSQL driver dependency to `worker/requirements.txt`
- [ ] Add worker configuration for `DATABASE_CONNECTION_STRING` loading and validation
- [ ] Implement chunk insert/upsert with dedupe via `(DocumentId, ChunkHash)`
- [ ] Insert embeddings into `vector(768)` safely (cast/adaptation)
- [ ] Wrap per-document batch writes in a transaction and rollback on failure
- [ ] Add unit tests covering dedupe and rollback/error propagation
