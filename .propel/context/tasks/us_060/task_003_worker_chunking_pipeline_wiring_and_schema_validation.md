# Task - TASK_060_003

## Requirement Reference
- User Story: us_060
- Story Location: .propel/context/tasks/us_060/us_060.md
- Acceptance Criteria: 
    - Given merged text, When chunked, Then chunks are 500-1000 tokens in size (TR-005).
    - Given chunking, When performed, Then 100-token overlap is maintained between adjacent chunks.
    - Given chunking, When implemented, Then RecursiveCharacterTextSplitter from LangChain is used (FR-032a).
    - Given chunks, When created, Then they preserve document source metadata for each chunk.

## Task Overview
Wire the chunking step into the worker processing pipeline so patient-level merged text (US_059) can be transformed into chunked outputs and validated against the chunking contract schema. This task ensures the chunking behavior is not only implemented (TASK_060_002) but also invoked deterministically in the pipeline and verified to emit contract-compliant payloads.

## Dependent Tasks
- [TASK_059_002 - Worker: patient-level multi-document text merge]
- [TASK_060_001 - Contracts: chunking output schema]
- [TASK_060_002 - Worker: semantic chunking with overlap]

## Impacted Components
- [MODIFY: worker/main.py]
- [CREATE: worker/tests/test_chunking_pipeline_contract_validation.py]
- [MODIFY: worker/tests/fixtures/schemas.py]

## Implementation Plan
- Add a minimal worker pipeline seam (similar to existing validation usage in `worker/main.py`) that:
  - Loads/accepts a job payload that includes the multi-document context required by US_059
  - Calls the merge step to produce merged text + provenance
  - Calls the chunking step to produce chunked output aligned with `contracts/chunking/v1/chunked_text.schema.json`
- Add schema validation for chunked outputs using `jsonschema`:
  - Load the chunking schema from `contracts/chunking/v1/chunked_text.schema.json`
  - Validate emitted payloads in a dedicated validation function (keep pure and testable)
- Add unit tests to validate:
  - A representative merged-text fixture produces contract-valid chunk outputs
  - Chunk outputs are deterministic (stable `chunk_index` ordering and stable provenance ordering for the same input)
  - Edge case: very short merged text still validates against the schema
- Guardrails:
  - Ensure no PHI is logged by default when validating or serializing chunk outputs (tests may assert absence of raw text in logs if a logging layer exists)
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | worker/main.py | Add a minimal pipeline wiring that invokes merge + chunking and validates chunk outputs against the chunking contract schema |
| MODIFY | worker/tests/fixtures/schemas.py | Add loader helper for the chunking schema (similar to job/entity schema fixtures) |
| CREATE | worker/tests/test_chunking_pipeline_contract_validation.py | Unit tests validating chunk pipeline emits schema-compliant outputs and remains deterministic across runs |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://python-jsonschema.readthedocs.io/

## Build Commands
- python -m pip install -r worker/requirements.txt
- python -m pytest worker/tests

## Implementation Validation Strategy
- Validate that chunk outputs produced by the pipeline pass JSON Schema validation for the chunking contract.
- Validate determinism and edge cases via repeatable unit tests.

## Implementation Checklist
- [ ] Add pipeline wiring in `worker/main.py` to invoke merge + chunking
- [ ] Implement JSON schema validation for chunking output (contract enforcement)
- [ ] Add schema loader helper in worker test fixtures
- [ ] Add unit tests for contract validation, determinism, and short-document behavior
- [ ] Confirm no PHI is logged by default when validating/serializing chunk outputs
