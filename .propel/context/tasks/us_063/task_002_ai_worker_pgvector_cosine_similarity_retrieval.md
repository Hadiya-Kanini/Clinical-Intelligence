# Task - [TASK_002]

## Requirement Reference
- User Story: [us_063]
- Story Location: [.propel/context/tasks/us_063/us_063.md]
- Acceptance Criteria: 
    - [Given a query embedding, When similarity search runs, Then top 10-15 chunks are retrieved by cosine similarity (FR-036).]
    - [Given retrieval, When performed, Then chunks are ranked by similarity score.]
    - [Given retrieval results, When returned, Then they include chunk text and source metadata.]
    - [Given access controls, When search is performed, Then only authorized documents are included (DR-005).]

## Task Overview
Extend the Python AI worker to perform cosine similarity retrieval against PostgreSQL/pgvector and return the top-K (default 15, range 10-15) most relevant chunks for downstream entity extraction. The worker must use parameterized queries, validate embedding dimensionality (768), and handle edge cases (no results, ties) deterministically.

## Dependent Tasks
- [US_062 TASK_002] (Worker persistence + DB connectivity patterns established)
- [US_062 TASK_001] (Backend migrations create `document_chunks` with `vector(768)` + HNSW cosine index)
- [US_062 TASK_003] (DR-005 enforcement for chunk visibility)

## Impacted Components
- [MODIFY | worker/requirements.txt | Add PostgreSQL driver dependency if not already present (e.g., psycopg).]
- [MODIFY | worker/config.py | Add `database_connection_string` configuration sourced from `DATABASE_CONNECTION_STRING`.]
- [CREATE | worker/retrieval/document_chunk_retriever.py | Encapsulate cosine similarity retrieval logic (top-K) and returned metadata shape.]
- [CREATE | worker/tests/test_document_chunk_retriever.py | Unit tests for query parameterization, K clamping, tie ordering, and empty results (DB integration tests optional/gated).]

## Implementation Plan
- Add worker configuration:
  - Read `DATABASE_CONNECTION_STRING` from environment.
  - Fail fast when missing.
- Add retrieval module:
  - Implement `retrieve_top_k_chunks(query_embedding, k=15, ...)`.
  - Validate `query_embedding` length is exactly 768 and contains numeric values.
  - Clamp `k` into [10, 15].
- Implement cosine similarity retrieval query (parameterized):
  - Use `embedding <=> $1::vector` (cosine distance) ordering ascending.
  - Return: `document_id`, `id` (chunk_id), `text_content`, `page`, `section`, `coordinates` plus `distance` or derived `similarity_score`.
  - Add deterministic tie-breaker by chunk id.
- Edge cases:
  - No rows: return `[]`.
  - Optional threshold: if configured (e.g., max cosine distance), filter out low-similarity results; when all filtered, return `[]`.
- Security/DR-005:
  - Keep SQL parameterized to prevent injection.
  - Do not attempt to bypass DR-005 controls; rely on backend-established policies.

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | worker/requirements.txt | Add PostgreSQL driver dependency for retrieval (if missing) |
| MODIFY | worker/config.py | Load `DATABASE_CONNECTION_STRING` for worker retrieval DB access |
| CREATE | worker/retrieval/document_chunk_retriever.py | Execute top-K cosine similarity retrieval query against `document_chunks` |
| CREATE | worker/tests/test_document_chunk_retriever.py | Validate K clamping, deterministic ordering, and empty-result behavior |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://github.com/pgvector/pgvector
- https://www.psycopg.org/psycopg3/

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Unit] Validate clamping to 10-15 and deterministic ordering for ties.
- [Integration] With a Postgres database seeded with chunks/embeddings, confirm top-K retrieval matches cosine similarity ordering.
- [Edge case] Confirm “no relevant chunks” returns an empty list and downstream extraction can proceed with a clear fallback.

## Implementation Checklist
- [ ] Add worker config for `DATABASE_CONNECTION_STRING`
- [ ] Add retrieval module with embedding length validation (768)
- [ ] Implement parameterized cosine similarity query with deterministic tie ordering
- [ ] Clamp `k` to 10-15 (default 15)
- [ ] Add unit tests for empty result and tie behavior
- [ ] Add optional similarity threshold configuration and tests (if adopted)
