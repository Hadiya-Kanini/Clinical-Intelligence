# Task - TASK_060_001

## Requirement Reference
- User Story: us_060
- Story Location: .propel/context/tasks/us_060/us_060.md
- Acceptance Criteria: 
    - Given merged text, When chunked, Then chunks are 500-1000 tokens in size (TR-005).
    - Given chunking, When performed, Then 100-token overlap is maintained between adjacent chunks.
    - Given chunks, When created, Then they preserve document source metadata for each chunk.

## Task Overview
Define the canonical, versioned contract for chunked text produced by the AI Worker after patient-level merge. The contract must represent chunk text plus enough provenance metadata to support downstream embedding generation and pgvector storage while meeting the chunk size and overlap requirements in US_060.

## Dependent Tasks
- [US_059 - Merge multi-document text per patient before chunking]
- [TASK_059_001 - Contracts: patient merge job contract]

## Impacted Components
- [CREATE: contracts/chunking/v1/chunked_text.schema.json]
- [CREATE: contracts/chunking/v1/README.md]
- [CREATE: contracts/migrations/chunking_v1.md]

## Implementation Plan
- Define `contracts/chunking/v1/chunked_text.schema.json` as a JSON Schema draft-07 document that can represent:
  - Chunk content (`text`)
  - Chunk ordering (`chunk_index`)
  - Chunk sizing metadata (e.g., `token_count`, `chunk_size_target_tokens`, `chunk_overlap_tokens`)
  - Source/provenance metadata for each chunk, including at minimum:
    - `document_id`
    - `page` / `section` / `coordinates` (when available)
  - A representation that supports chunks spanning multiple documents while retaining provenance for every included source segment.
- Add `contracts/chunking/v1/README.md` describing:
  - Purpose and consumers of the chunking contract
  - Required vs optional provenance fields
  - How multi-document chunks should represent multiple provenance entries
  - Versioning rules and compatibility expectations
- Create `contracts/migrations/chunking_v1.md` following the conventions in `contracts/migrations/README.md`:
  - Contract name
  - Version
  - Change Type
  - Description
  - Impact
  - Required Actions
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | contracts/chunking/v1/chunked_text.schema.json | Defines a versioned schema for chunked text output including chunk sizing metadata and per-chunk provenance fields required for downstream embedding and storage |
| CREATE | contracts/chunking/v1/README.md | Documents schema usage rules, provenance expectations, and multi-document chunk representation |
| CREATE | contracts/migrations/chunking_v1.md | Migration note for initial chunking contract release including consumer impact and required actions |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://json-schema.org/
- https://semver.org/

## Build Commands
- python -m pip install -r worker/requirements.txt

## Implementation Validation Strategy
- Validate the schema can represent:
  - Single-document chunks with page/section/coordinates
  - Multi-document chunks with multiple provenance entries
  - Chunk sizing and overlap metadata required by TR-005
- Validate the schema can be used as an integration boundary for US_061/US_062 without requiring out-of-contract coupling.

## Implementation Checklist
- [ ] Create `contracts/chunking/v1/chunked_text.schema.json` for chunked text output
- [ ] Create `contracts/chunking/v1/README.md` with required fields and examples
- [ ] Create `contracts/migrations/chunking_v1.md` for initial release documentation
- [ ] Confirm schema supports chunks spanning document boundaries while preserving provenance for every chunk
