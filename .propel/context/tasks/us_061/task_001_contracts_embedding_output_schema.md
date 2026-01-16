# Task - TASK_061_001

## Requirement Reference
- User Story: us_061
- Story Location: .propel/context/tasks/us_061/us_061.md
- Acceptance Criteria: 
    - Given text chunks, When processed, Then 768-dimensional embeddings are generated using text-embedding-004 model (FR-033).
    - Given embeddings, When generated, Then they are associated with their source chunk metadata.

## Task Overview
Define the canonical, versioned contract for embedding generation output produced by the AI Worker. The contract must represent the 768-dimensional embedding vector along with the full chunk provenance/metadata (document/page/section/coordinates, chunk_index, token_count, chunk_hash) required for downstream storage in `document_chunks` and traceability.

## Dependent Tasks
- .propel/context/tasks/us_060/task_001_contracts_chunking_output_schema.md (TASK_060_001)

## Impacted Components
- [CREATE: contracts/embeddings/v1/embedding_result.schema.json]
- [CREATE: contracts/embeddings/v1/README.md]
- [CREATE: contracts/migrations/embeddings_v1.md]

## Implementation Plan
- Define `contracts/embeddings/v1/embedding_result.schema.json` as a JSON Schema draft-07 document that can represent:
  - Embedding metadata:
    - `embedding_model` (string, e.g., `text-embedding-004` or `gemini-embedding-001`)
    - `embedding_dimensions` (must equal 768)
    - `embedding` as an array of 768 floats
    - `normalized` boolean flag indicating whether L2 normalization was applied (recommended for 768)
  - Chunk identity and provenance fields sufficient for downstream DB storage and citations:
    - `document_id`
    - `chunk_index`
    - `text_content` (optional if avoiding payload bloat; clarify contract requirement)
    - `page`, `section`, `coordinates` (nullable)
    - `token_count` (nullable)
    - `chunk_hash` (nullable)
  - Error representation for partial failures:
    - `status` (e.g., success/failed)
    - `error_code`, `error_message` (when failed)
    - A structure that supports “some chunks failed” while still returning successful embeddings for other chunks.
- Add `contracts/embeddings/v1/README.md` describing:
  - Purpose and consumers of the embeddings contract (US_062 storage)
  - Required vs optional chunk metadata fields
  - Normalization expectations for 768-dimensional vectors
  - Partial-failure semantics and retry responsibility boundaries
- Create `contracts/migrations/embeddings_v1.md` following the conventions in `contracts/migrations/README.md`.
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | contracts/embeddings/v1/embedding_result.schema.json | Defines a versioned schema for embedding generation output including a 768-float embedding vector and required chunk provenance/metadata for downstream storage |
| CREATE | contracts/embeddings/v1/README.md | Documents schema usage rules, normalization expectation for 768-d vectors, and partial-failure semantics |
| CREATE | contracts/migrations/embeddings_v1.md | Migration note for initial embeddings contract release including consumer impact and required actions |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://json-schema.org/
- https://ai.google.dev/gemini-api/docs/embeddings

## Build Commands
- python -m pip install -r worker/requirements.txt

## Implementation Validation Strategy
- Validate the schema can represent:
  - A successful 768-dimensional embedding bound to chunk provenance metadata
  - A failed chunk embedding result with a clear error payload
  - A mixed batch where some chunks succeeded and some failed
- Validate the contract supports US_062 storage requirements without requiring out-of-contract coupling.

## Implementation Checklist
- [ ] Create `contracts/embeddings/v1/embedding_result.schema.json` for embedding output (768-d)
- [ ] Create `contracts/embeddings/v1/README.md` with required fields, examples, and normalization guidance
- [ ] Create `contracts/migrations/embeddings_v1.md` documenting initial release and consumer impact
- [ ] Confirm the contract contains all fields required to associate embeddings with chunk metadata for US_062 storage
