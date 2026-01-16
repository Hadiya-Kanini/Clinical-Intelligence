# Task - [TASK_002]

## Requirement Reference
- User Story: [us_065]
- Story Location: [.propel/context/tasks/us_065/us_065.md]
- Acceptance Criteria: 
    - [Given extracted entities, When validated, Then they must conform to versioned Pydantic schemas (FR-038).]
    - [Given validation failure, When detected, Then the job is marked Failed with validation errors persisted (TR-007).]
    - [What happens when validation fails for some but not all entities?]

## Task Overview
Integrate Pydantic-based validation into the worker entity extraction flow so that the parsed LLM output is validated using the versioned schema registry and produces **structured, PHI-safe validation errors**.

This task covers:
- Calling the Pydantic schema validator after parsing
- Normalizing Pydantic validation errors into a serializable format suitable for persistence (`ErrorMessage` + `ErrorDetails` concept)
- Defining deterministic behavior when only a subset of entities fails validation

## Dependent Tasks
- [US_064 TASK_003] (Worker: parse response into structured entity payload)
- [US_065 TASK_001] (Worker: versioned Pydantic schemas + registry)

## Impacted Components
- [CREATE | worker/entity_validation/entity_validator.py | Validate entity payloads via Pydantic and return normalized error results]
- [MODIFY | worker/main.py | Invoke entity validator after parsing; map failures to job failure semantics]
- [CREATE | worker/tests/test_pydantic_entity_validation_errors.py | Unit tests for validation success/failure and error normalization]

## Implementation Plan
- Implement `validate_entity_payload_with_pydantic(payload: dict) -> None` (or equivalent) that:
  - Resolves schema model using `worker/entity_schemas/registry.py`
  - Validates payload using Pydantic (`model_validate` / `parse_obj` depending on Pydantic version)
- Implement deterministic error normalization:
  - Convert `pydantic.ValidationError` into:
    - A short human-readable `error_message` (no raw LLM output)
    - A list of `error_details` entries with:
      - JSON path / field location
      - Error type
      - Message
  - Ensure error strings avoid embedding `source_text` / large document excerpts by default (PHI-safe)
- Partial-validation strategy (edge case):
  - If **any** entity fails schema validation, treat the entire payload as invalid and return a validation failure result.
  - (If product later needs partial accept, implement as a new story; keep this deterministic for now.)
- Wire into the worker execution path:
  - After parsing LLM response to a `dict`, call Pydantic validation.
  - On validation success: continue existing flow (ready for persistence/UI rendering/export).
  - On validation failure: raise/return an error type that upstream orchestration can convert to:
    - job status `validation_failed` (worker contract)
    - persisted error details (API/DB concept)

**Focus on how to implement**

## Current Project State
- worker/
  - main.py (contains JSON schema-based `validate_entity_payload` today)
  - tests/test_entity_validation.py
- contracts/
  - entities/v1/entity.schema.json

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | worker/entity_validation/entity_validator.py | Pydantic validation + error normalization helpers for entity payloads |
| MODIFY | worker/main.py | Call Pydantic validation after parsing entity payloads and map failures to validation_failed semantics |
| CREATE | worker/tests/test_pydantic_entity_validation_errors.py | Validate error normalization output shape and deterministic failure behavior |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://docs.pydantic.dev/latest/errors/

## Build Commands
- python -m pip install -r worker/requirements.txt
- python -m pytest worker/tests

## Implementation Validation Strategy
- [Unit] Valid entity payload passes Pydantic validation.
- [Unit] Missing required fields produce normalized error details (paths + messages) without including raw LLM outputs.
- [Unit] Unknown `schema_version` fails deterministically with clear error message.
- [Unit] Payload with one invalid entity fails validation deterministically (no partial accept).

## Implementation Checklist
- [ ] Implement Pydantic-based entity payload validation using schema registry
- [ ] Normalize `ValidationError` into persistence-friendly `error_message` + `error_details[]`
- [ ] Enforce deterministic partial-validation behavior (fail whole payload on any schema violation)
- [ ] Wire validation call into `worker/main.py` after parsing
- [ ] Add unit tests for success + failure + schema version mismatch
