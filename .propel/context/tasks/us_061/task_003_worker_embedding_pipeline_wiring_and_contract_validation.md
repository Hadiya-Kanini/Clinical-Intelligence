# Task - TASK_061_003

## Requirement Reference
- User Story: us_061
- Story Location: .propel/context/tasks/us_061/us_061.md
- Acceptance Criteria: 
    - Given text chunks, When processed, Then 768-dimensional embeddings are generated using text-embedding-004 model (FR-033).
    - Given API errors, When they occur, Then retry with exponential backoff is applied.
    - Given embeddings, When generated, Then they are associated with their source chunk metadata.

## Task Overview
Wire the embedding generation step into the AI Worker processing flow so chunk outputs (US_060) can be transformed into contract-compliant embedding results and validated against the embeddings contract schema. This task ensures embedding generation is invoked deterministically, validated at the integration boundary, and tested for correct metadata association across batches.

## Dependent Tasks
- .propel/context/tasks/us_060/task_003_worker_chunking_pipeline_wiring_and_schema_validation.md (TASK_060_003)
- .propel/context/tasks/us_061/task_001_contracts_embedding_output_schema.md (TASK_061_001)
- .propel/context/tasks/us_061/task_002_worker_generate_embeddings_gemini_rate_limit_retry_normalize.md (TASK_061_002)

## Impacted Components
- [MODIFY: worker/main.py]
- [CREATE: worker/tests/test_embedding_pipeline_contract_validation.py]
- [MODIFY: worker/tests/fixtures/schemas.py]

## Implementation Plan
- Add an embeddings schema loader helper for tests (mirroring existing schema fixture patterns):
  - Load `contracts/embeddings/v1/embedding_result.schema.json` for contract validation.
- Extend `worker/main.py` with a minimal integration seam that:
  - Accepts a representative chunk payload (aligned with the chunking contract)
  - Calls the embedding generation entry point (TASK_061_002)
  - Produces embedding results aligned with the embeddings output contract
  - Validates the emitted payloads using `jsonschema` Draft-07 validator.
- Add unit tests in `worker/tests/test_embedding_pipeline_contract_validation.py` validating:
  - For a representative chunk fixture, the embedding pipeline emits contract-valid results.
  - The emitted results preserve chunk metadata association (document/page/section/coordinates, chunk_index, token_count, chunk_hash) for each embedding.
  - Determinism: the same input chunks yield stable ordering (`chunk_index`) and stable metadata association across runs (embedding values may vary if the remote API is called; for tests, mock the client to return deterministic vectors).
  - Edge cases:
    - Some chunks fail embedding generation: the overall output is still schema-valid and includes explicit failed entries.
    - Very short chunk batches (single chunk) still validate.
- Guardrails:
  - Ensure no raw chunk text is logged by default when validating/serializing embedding outputs (tests may assert absence of raw text in logs if logging is introduced).
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | worker/main.py | Add a minimal integration seam that invokes embedding generation and validates embedding outputs against the embeddings contract schema |
| MODIFY | worker/tests/fixtures/schemas.py | Add schema fixture/loader support for the embeddings contract schema |
| CREATE | worker/tests/test_embedding_pipeline_contract_validation.py | Unit tests validating the embedding pipeline emits schema-compliant outputs, preserves metadata association, and supports partial failures |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://python-jsonschema.readthedocs.io/
- https://ai.google.dev/gemini-api/docs/embeddings

## Build Commands
- python -m pip install -r worker/requirements.txt
- python -m pytest worker/tests

## Implementation Validation Strategy
- Validate embedding outputs are JSON Schema compliant for the embeddings contract.
- Validate each embedding output preserves chunk metadata association.
- Validate deterministic ordering and partial-failure handling via mocked embedding client tests.

## Implementation Checklist
- [ ] Add embeddings schema loader helper for contract validation
- [ ] Wire embedding generation into `worker/main.py` and validate contract output
- [ ] Add unit tests for schema compliance and metadata association
- [ ] Add unit tests covering partial failures and single-chunk batches
- [ ] Confirm no PHI is logged by default when validating/serializing embedding outputs
