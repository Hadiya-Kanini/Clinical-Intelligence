# Task - TASK_067_003

## Requirement Reference
- User Story: us_067
- Story Location: .propel/context/tasks/us_067/us_067.md
- Acceptance Criteria: 
    - Given new categories, When added, Then they follow the same schema validation pattern.
    - Given extensibility, When implemented, Then existing categories are not affected by additions.

## Task Overview
Add a worker-side validation + normalization step for extracted entity categories so that:
- `entity_group_name` values are resolved via the canonical category registry (alias -> canonical id)
- Unknown categories fail deterministically (forcing registry updates rather than silent drift)
- Deprecated categories are handled explicitly without breaking existing runs

This task ensures extensibility is managed as configuration/data while preserving deterministic validation semantics.

## Dependent Tasks
- [TASK_067_001] (Contracts: entity category registry contract)
- [TASK_067_002] (Worker: category registry loader + prompt builder integration)
- [US_064 TASK_003] (Worker: parse + validate LLM response entities)
- [US_065 TASK_001] (Worker: versioned Pydantic entity schemas)

## Impacted Components
- [MODIFY | worker/entity_extraction/response_parser.py | Add category normalization/validation post-parse (alias resolution, unknown category failure, deprecated handling)]
- [CREATE | worker/entity_extraction/category_normalization.py | Utilities to normalize `entity_group_name` values using the category registry]
- [CREATE | worker/tests/test_entity_category_normalization.py | Unit tests for alias resolution, unknown categories, and deprecated category behavior]
- [MODIFY | worker/tests/fixtures/entity_payloads.py | Add fixtures for alias categories, unknown categories, and deprecated categories]

## Implementation Plan
- Implement `category_normalization.py`:
  - Add `normalize_entity_categories(payload: dict, registry) -> dict` that:
    - Iterates through `extracted_entities[]`
    - Resolves `entity_group_name` via registry alias resolution
    - Preserves `entity_name` and `entity_value` exactly (no content mutations)
  - Validation rules:
    - If a category id (after alias resolution) is not present in the registry, raise a deterministic validation error.
    - If a category is `deprecated`, keep the canonical id but record a warning (PHI-safe; do not include `entity_value` in logs/errors).
- Integrate into `response_parser.py`:
  - Apply category normalization after JSON parsing and before final schema validation/persistence boundaries.
  - Ensure errors are PHI-safe and do not include raw document text.
- Tests:
  - Alias resolution: payload with `entity_group_name` set to an alias results in canonical id.
  - Unknown category: payload with a non-registered category fails deterministically.
  - Deprecated category: payload remains valid (unless policy requires failure), but deprecation is detectable (e.g., by returned warnings or logged message).
  - Regression: core categories remain unchanged.

**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | worker/entity_extraction/category_normalization.py | Normalize and validate `entity_group_name` values using the category registry (alias resolution + deterministic failures) |
| MODIFY | worker/entity_extraction/response_parser.py | Apply category normalization after parsing and before final validation/persistence |
| MODIFY | worker/tests/fixtures/entity_payloads.py | Add fixtures for alias/unknown/deprecated category scenarios |
| CREATE | worker/tests/test_entity_category_normalization.py | Unit tests covering alias resolution, unknown categories, and deprecated category handling |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://json-schema.org/understanding-json-schema/

## Build Commands
- python -m pip install -r worker/requirements.txt
- python -m pytest worker/tests

## Implementation Validation Strategy
- Validate that unknown categories are rejected deterministically (clear error) rather than silently accepted.
- Validate alias categories resolve to canonical ids without changing any other entity fields.
- Validate core categories from US_066 continue to pass without modifications.

## Implementation Checklist
- [ ] Implement category normalization utility using the registry loader API
- [ ] Apply normalization within the response parsing/validation pipeline
- [ ] Add fixtures and unit tests for alias/unknown/deprecated category scenarios
- [ ] Confirm normalization is PHI-safe (no raw values in exception messages)
- [ ] Confirm existing category behavior remains unchanged when new categories are added
