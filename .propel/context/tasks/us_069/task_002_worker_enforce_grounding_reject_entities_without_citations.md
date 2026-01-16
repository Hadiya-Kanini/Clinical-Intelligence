# Task - TASK_069_002

## Requirement Reference
- User Story: us_069 (extracted from input)
- Story Location: [.propel/context/tasks/us_069/us_069.md]
- Acceptance Criteria: 
    - Given an extracted entity, When validated, Then it must have valid source citations (FR-051).
    - Given entities without citations, When detected, Then they are rejected and not stored as final (FR-056, NFR-006).
    - Given citations, When stored, Then they include document_id, page, section, coordinates, and cited text (TR-008).

## Task Overview
Implement worker-side grounding enforcement so that entity extraction results are only considered valid when every extracted entity includes required source citation fields. This task introduces deterministic validation rules (schema + explicit checks) and standardizes the worker’s failure behavior when citations are missing or invalid.

## Dependent Tasks
- [TASK_069_001] (Contracts: grounded entity schema v1.1)
- [US_064 TASK_001] (Prompt includes grounding instructions and provenance)
- [US_065 TASK_002] (Structured validation error semantics)

## Impacted Components
- [MODIFY | worker/main.py | Support schema_version `1.1` and ensure schema validation is applied]
- [CREATE | worker/entity_validation/grounding_validator.py | Enforce “100% of entities must be grounded” with explicit, PHI-safe error messages]
- [MODIFY | worker/tests/test_entity_validation.py | Add tests for `schema_version: 1.1` grounded payload requirements]
- [CREATE | worker/tests/test_grounding_validator.py | Unit tests for missing/invalid citation edge cases]

## Implementation Plan
- Extend worker schema loading in `worker/main.py`:
  - Update `_load_entity_schema(schema_version: str)` to accept `"1.1"` and load the same `contracts/entities/v1/entity.schema.json`.
  - Ensure `validate_entity_payload` continues to run JSON Schema validation using Draft-07.
- Add `grounding_validator.py` to enforce deterministic “100% grounding” beyond schema validation:
  - Validate every item in `extracted_entities[]` includes:
    - `source_text` (non-empty)
    - `document_location.page` (>=1)
    - `document_location.section` (non-empty)
    - `document_location.coordinates` with numeric `{x,y,width,height}`
  - Return/raise a PHI-safe error summary:
    - Do not include raw `source_text` or entity values in exception strings.
    - Include only index + missing fields (e.g., `extracted_entities[3].document_location.section missing`).
- Wire grounding validation into the worker flow:
  - After JSON Schema validation succeeds, call `validate_grounding(payload)`.
  - If grounding fails, raise a deterministic error that upstream orchestration can map to job failure (consistent with US_065 semantics).
- Tests:
  - Add positive test: fully grounded payload passes.
  - Add negative tests:
    - Missing `source_text`
    - Missing `document_location`
    - `page` null/0
    - Missing coordinates keys
    - Non-numeric coordinate fields

**Focus on how to implement**

## Current Project State
- worker/main.py contains JSON Schema validation for entity payloads
- contracts/entities/v1/entity.schema.json defines `document_location` and `source_text` as optional today

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | worker/main.py | Accept `schema_version: 1.1` and run validation using the same entity schema file |
| CREATE | worker/entity_validation/grounding_validator.py | Implement explicit “100% grounding required” validator with PHI-safe errors |
| MODIFY | worker/tests/test_entity_validation.py | Add schema-level tests for `schema_version: 1.1` grounding-required payloads |
| CREATE | worker/tests/test_grounding_validator.py | Unit tests covering grounding edge cases and deterministic failure behavior |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://json-schema.org/understanding-json-schema/

## Build Commands
- python -m pip install -r worker/requirements.txt
- python -m pytest worker/tests

## Implementation Validation Strategy
- [Unit] Payload with any ungrounded entity fails deterministically and does not leak PHI in error messages.
- [Unit] Fully grounded payload passes and produces no warnings.
- [Regression] `schema_version: 1.0` validation behavior remains unchanged.

## Implementation Checklist
- [ ] Update worker schema loader to accept `schema_version: 1.1`
- [ ] Implement `validate_grounding(payload)` enforcing per-entity citation requirements
- [ ] Integrate grounding validator after schema validation
- [ ] Add tests for grounded success and ungrounded failure scenarios
- [ ] Confirm error messages are PHI-safe (no raw text/value inclusion)
