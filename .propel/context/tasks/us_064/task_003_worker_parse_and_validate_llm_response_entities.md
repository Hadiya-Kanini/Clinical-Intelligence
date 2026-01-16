# Task - TASK_064_003

## Requirement Reference
- User Story: us_064
- Story Location: .propel/context/tasks/us_064/us_064.md
- Acceptance Criteria: 
    - Given LLM response, When received, Then it is parsed into structured entity objects.
    - What happens when LLM response is malformed or incomplete?
    - Given extraction, When performed, Then conflict detection is included in the output (TR-006).

## Task Overview
Implement robust parsing and validation of the single-call Gemini extraction response into the structured entity result format. This task ensures malformed output is handled deterministically and validated against `contracts/entities/v1/entity.schema.json`.

## Dependent Tasks
- [TASK_064_002 - Worker: Gemini single-call entity extraction client]
- [contracts/entities/v1/entity.schema.json] (existing contract)

## Impacted Components
- [CREATE: worker/entity_extraction/response_parser.py]
- [MODIFY: worker/main.py]
- [MODIFY: worker/main.py] (reuse existing `validate_entity_payload` for schema validation)
- [CREATE: worker/tests/test_entity_extraction_response_parsing.py]
- [MODIFY: worker/tests/fixtures/entity_payloads.py]

## Implementation Plan
- Implement `parse_entity_extraction_response(raw_text: str) -> dict` that:
  - Extracts the first valid JSON object from the model output
  - Rejects non-JSON outputs deterministically with a clear error
- Validate parsed payload:
  - Ensure `schema_version` is present and supported
  - Validate against `contracts/entities/v1/entity.schema.json` using existing `validate_entity_payload`
- Normalize and harden edge cases:
  - If entity `conflicts` is present, validate each conflict object shape
  - If optional fields (`rationale`, `source_text`, `document_location`) are missing, allow them
  - If required fields are missing, fail validation
- Error handling requirements:
  - Return/raise an error type that allows upstream job handling to mark the job as failed or validation_failed
  - Ensure error details avoid including full raw response/chunk text by default (PHI-safe)
- Add tests:
  - Valid JSON response parses and validates
  - Malformed response (non-JSON, truncated JSON) fails deterministically
  - Response with conflicts parses and validates
  - Response missing required fields fails validation

**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | worker/entity_extraction/response_parser.py | Parse Gemini raw output into JSON and validate against the entity contract using existing schema validation |
| MODIFY | worker/main.py | Wire response parsing/validation after the single-call extraction step |
| MODIFY | worker/tests/fixtures/entity_payloads.py | Add fixtures representing valid conflict-bearing payloads and malformed outputs |
| CREATE | worker/tests/test_entity_extraction_response_parsing.py | Unit tests for parsing success, malformed responses, conflict payloads, and schema failures |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://json-schema.org/understanding-json-schema/

## Build Commands
- python -m pip install -r worker/requirements.txt
- python -m pytest worker/tests

## Implementation Validation Strategy
- Validate that the parser extracts JSON-only content and fails deterministically for malformed outputs.
- Validate schema compliance using `validate_entity_payload` and the canonical contract schema.
- Validate conflict payloads are accepted when they follow the contract.

## Implementation Checklist
- [ ] Implement JSON extraction/parsing from Gemini output (reject non-JSON deterministically)
- [ ] Validate parsed payload against `contracts/entities/v1/entity.schema.json`
- [ ] Ensure conflict payloads are supported and validated
- [ ] Ensure malformed/incomplete outputs raise clear, PHI-safe errors
- [ ] Add unit tests for valid payloads, malformed outputs, missing required fields, and conflict payloads
- [ ] Wire parsing + validation into the worker execution path after extraction
