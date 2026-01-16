# Task - TASK_067_002

## Requirement Reference
- User Story: us_067
- Story Location: .propel/context/tasks/us_067/us_067.md
- Acceptance Criteria: 
    - Given the prompt template, When designed, Then it is configurable for additional categories.
    - Given extensibility, When implemented, Then existing categories are not affected by additions.

## Task Overview
Extend the AI worker extraction prompt system to load extraction categories from the canonical category registry contract, so that adding a new category requires only updating data/config (registry file) and not modifying prompt builder code.

This task focuses on:
- Loading and validating the category registry (`entity_categories.json`) using a deterministic schema validation pattern
- Integrating the registry with the entity extraction prompt builder so categories are enumerated dynamically
- Preserving prompt determinism and backward compatibility

## Dependent Tasks
- [TASK_067_001] (Contracts: entity category registry contract)
- [US_064 TASK_001] (Worker prompt builder module exists and is used for single-call extraction)

## Impacted Components
- [CREATE | worker/entity_extraction/category_registry.py | Load and validate the category registry and expose a stable API for active categories]
- [MODIFY | worker/entity_extraction/prompt_builder.py | Replace hard-coded category list with registry-driven category enumeration]
- [CREATE | worker/tests/test_entity_category_registry.py | Unit tests for registry load, validation, ordering, and alias conflict detection]
- [MODIFY | worker/tests/test_entity_extraction_prompt_builder.py | Update prompt tests to assert registry-driven categories appear in the prompt]
- [MODIFY | worker/config.py | Add config for category registry path with safe default to `contracts/entities/v1/entity_categories.json`]

## Implementation Plan
- Add worker configuration:
  - Add `ENTITY_CATEGORIES_PATH` (or equivalent) in `worker/config.py`.
  - Default to the repo-relative `contracts/entities/v1/entity_categories.json` path.
  - Fail fast with a clear error when the file is missing.
- Implement `category_registry.py`:
  - Load `entity_categories.json`.
  - Load and validate against `contracts/entities/v1/entity_categories.schema.json` using the same validation approach as existing worker schema validation (Draft7 JSON schema validation).
  - Provide API methods:
    - `get_active_category_ids()` returning stable, deterministic ordering.
    - `resolve_category_id(input_id)` that resolves aliases to canonical ids.
  - Ensure registry conflicts fail deterministically (duplicate ids, alias collisions).
- Integrate into prompt builder:
  - Update `build_entity_extraction_prompt(...)` to enumerate categories from `get_active_category_ids()`.
  - Ensure prompt ordering is deterministic and not dependent on dictionary iteration.
  - Preserve existing output format aligned to `contracts/entities/v1/entity.schema.json`.
- Testing:
  - Add unit tests to validate registry loading, schema validation behavior, and deterministic ordering.
  - Update prompt builder tests to assert that adding a new category in the registry results in prompt changes without code changes.

**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | worker/entity_extraction/category_registry.py | Load and validate `entity_categories.json` and expose APIs for active categories + alias resolution |
| MODIFY | worker/entity_extraction/prompt_builder.py | Use registry-driven categories instead of hard-coded core list |
| CREATE | worker/tests/test_entity_category_registry.py | Validate schema validation, ordering, and conflict detection for category registry |
| MODIFY | worker/tests/test_entity_extraction_prompt_builder.py | Update tests to cover registry-driven categories in prompt generation |
| MODIFY | worker/config.py | Configure path to category registry with safe default and fail-fast behavior |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://json-schema.org/understanding-json-schema/

## Build Commands
- python -m pip install -r worker/requirements.txt
- python -m pytest worker/tests

## Implementation Validation Strategy
- Validate worker startup fails with a clear error if `entity_categories.json` is missing or invalid.
- Validate prompt includes exactly the set of active categories from the registry and remains deterministic.
- Validate adding a new category in the registry (no code changes) results in prompt including that category.

## Implementation Checklist
- [ ] Add worker config for registry path with safe default
- [ ] Implement registry loader + schema validation
- [ ] Implement alias resolution and deterministic ordering
- [ ] Update prompt builder to use registry-driven categories
- [ ] Add/update unit tests for registry behavior and prompt generation
- [ ] Confirm existing categories remain unchanged when new categories are added
