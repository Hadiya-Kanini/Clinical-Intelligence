# Task - [TASK_001]

## Requirement Reference
- User Story: [us_063]
- Story Location: [.propel/context/tasks/us_063/us_063.md]
- Acceptance Criteria: 
    - [Given a query embedding, When similarity search runs, Then top 10-15 chunks are retrieved by cosine similarity (FR-036).]
    - [Given retrieval, When performed, Then chunks are ranked by similarity score.]
    - [Given access controls, When search is performed, Then only authorized documents are included (DR-005).]
    - [Given retrieval results, When returned, Then they include chunk text and source metadata.]

## Task Overview
Implement a backend retrieval service that queries `document_chunks` using pgvector cosine distance to return top-K (default 15, range 10-15) most relevant chunks for a given query embedding. Results must be deterministically ordered by similarity and include chunk text plus source metadata (document_id, page, section, coordinates). Access controls must be enforced via DR-005 (RLS / user-context propagation) rather than application-side filtering.

## Dependent Tasks
- [US_062 TASK_001] (Create `document_chunks` table with `vector(768)` and HNSW cosine index)
- [US_062 TASK_003] (Enable DR-005 enforcement for `document_chunks` via RLS and user context propagation)

## Impacted Components
- [CREATE | Server/ClinicalIntelligence.Api/Services/Rag/IDocumentChunkRetrievalService.cs | Abstraction for cosine similarity retrieval (DIP).]
- [CREATE | Server/ClinicalIntelligence.Api/Services/Rag/DocumentChunkRetrievalService.cs | Implementation using parameterized SQL/EF Core + pgvector cosine distance ordering.]
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Register retrieval service in DI container.]
- [CREATE | Server/ClinicalIntelligence.Api/Contracts/Rag/RetrievedChunkDto.cs | Response DTO for retrieved chunk payload (text + source metadata + score).]
- [CREATE | Server/ClinicalIntelligence.Api.Tests/Integration/DocumentChunkRetrievalIntegrationTests.cs | Postgres-only integration tests for ranking, K bounds, deterministic ordering, and DR-005 enforcement behavior.]

## Implementation Plan
- Define a DTO that includes:
  - `ChunkId`, `DocumentId`, `TextContent`, `Page`, `Section`, `Coordinates`
  - `Score` (similarity score or distance; choose one and keep consistent)
- Create `IDocumentChunkRetrievalService` with an async API that accepts:
  - `Vector queryEmbedding` (768-d)
  - Optional scoping parameters if already available in the domain (e.g., `documentId` or `patientId`) to reduce search space
  - `k` with validation and clamping to 10-15
- Implement cosine similarity retrieval with parameterized query:
  - Use pgvector cosine distance operator (`<=>`) for ordering (smaller distance = more similar)
  - Add a deterministic tie-breaker (e.g., `ORDER BY distance ASC, "Id" ASC`)
  - Ensure the query returns chunk text and source metadata
- Security/DR-005:
  - Do not implement application-side “authorization filters” that can be bypassed; rely on DR-005/RLS.
  - Ensure queries are parameterized to prevent SQL injection.
- Tests:
  - Seed minimal documents + chunks, insert embeddings, and verify ranking order.
  - Verify `k` behavior: requests outside 10-15 are clamped.
  - Verify empty-result behavior returns empty list (no exceptions).

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api/Services/Rag/IDocumentChunkRetrievalService.cs | Defines the retrieval service interface for top-K cosine similarity search |
| CREATE | Server/ClinicalIntelligence.Api/Services/Rag/DocumentChunkRetrievalService.cs | Implements top-K cosine similarity query against `document_chunks` using pgvector |
| CREATE | Server/ClinicalIntelligence.Api/Contracts/Rag/RetrievedChunkDto.cs | DTO describing retrieved chunks (text + source metadata + score) |
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Register retrieval service for DI |
| CREATE | Server/ClinicalIntelligence.Api.Tests/Integration/DocumentChunkRetrievalIntegrationTests.cs | Validates ranking + K bounds + deterministic ordering; Postgres-only |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://github.com/pgvector/pgvector
- https://www.npgsql.org/efcore/

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Integration] Run Postgres-backed tests to validate that returned chunks are ordered by cosine distance and include required metadata.
- [Security] Validate query uses parameters (no string concatenation) and that DR-005/RLS restrictions apply to retrieval results.
- [Edge case] Validate “no relevant chunks” returns an empty list and caller can proceed safely.

## Implementation Checklist
- [ ] Add retrieval DTO for returned chunk payload
- [ ] Add `IDocumentChunkRetrievalService` abstraction (async)
- [ ] Implement cosine similarity query with deterministic ordering and K clamping (10-15)
- [ ] Ensure query is parameterized and relies on DR-005/RLS for access control
- [ ] Add Postgres-only integration tests for ranking, clamping, and empty results
- [ ] Register retrieval service in `Program.cs`
