# Task - [TASK_003]

## Requirement Reference
- User Story: [us_063]
- Story Location: [.propel/context/tasks/us_063/us_063.md]
- Acceptance Criteria: 
    - [Given a query embedding, When similarity search runs, Then top 10-15 chunks are retrieved by cosine similarity (FR-036).]
    - [Given retrieval results, When returned, Then they include chunk text and source metadata.]

## Task Overview
Define a versioned contract for retrieval output (top-K chunks) so the AI worker and downstream extractors can consume a stable, validated payload that includes chunk text, source metadata, and similarity ranking information.

## Dependent Tasks
- [US_061 TASK_001] (Embeddings contract shape and normalization expectations established)
- [US_062 TASK_001] (Chunk storage schema established)

## Impacted Components
- [CREATE: contracts/retrieval/v1/retrieval_result.schema.json]
- [CREATE: contracts/retrieval/v1/README.md]
- [CREATE: contracts/migrations/retrieval_v1.md]

## Implementation Plan
- Define `contracts/retrieval/v1/retrieval_result.schema.json` (JSON Schema draft-07) representing:
  - Retrieval request context:
    - `schema_version`
    - `query_embedding_dimensions` (must equal 768)
    - `k_requested`, `k_returned`
    - `query_hash` (optional) for auditability without storing raw query text
  - Results array where each item includes:
    - `chunk_id`
    - `document_id`
    - `text_content`
    - `page`, `section`, `coordinates` (nullable)
    - `rank` (1..k)
    - `score` (either similarity or distance; specify clearly)
  - Empty result semantics:
    - Allow `results: []` with `k_returned = 0`
- Add `contracts/retrieval/v1/README.md` describing:
  - Interpretation of `score` (similarity vs distance)
  - K clamping rules (10-15)
  - Tie-breaking expectations (deterministic)
- Add `contracts/migrations/retrieval_v1.md` documenting version introduction and consumer impact.

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | contracts/retrieval/v1/retrieval_result.schema.json | Versioned schema for top-K retrieval results including chunk text, source metadata, and ranking/score |
| CREATE | contracts/retrieval/v1/README.md | Documents retrieval contract semantics, score meaning, and tie rules |
| CREATE | contracts/migrations/retrieval_v1.md | Migration note for initial retrieval contract release |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://json-schema.org/
- https://semver.org/

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- Validate the schema can represent:
  - A standard top-15 retrieval response
  - An empty retrieval response (no relevant chunks)
  - Deterministic `rank` assignment with explicit score semantics

## Implementation Checklist
- [ ] Create `contracts/retrieval/v1/retrieval_result.schema.json`
- [ ] Create `contracts/retrieval/v1/README.md` clarifying score semantics and K bounds
- [ ] Create `contracts/migrations/retrieval_v1.md` documenting initial release
- [ ] Confirm contract aligns with `document_chunks` metadata fields used downstream
