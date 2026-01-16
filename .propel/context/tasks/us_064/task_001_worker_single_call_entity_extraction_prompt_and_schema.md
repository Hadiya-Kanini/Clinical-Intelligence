# Task - TASK_064_001

## Requirement Reference
- User Story: us_064
- Story Location: .propel/context/tasks/us_064/us_064.md
- Acceptance Criteria: 
    - Given the extraction prompt, When designed, Then it includes structured output format for all categories.
    - Given retrieved chunks, When extraction runs, Then all core entity categories are extracted in one Gemini call (FR-037, TR-006).
    - Given extraction, When performed, Then conflict detection is included in the output (TR-006).

## Task Overview
Design and implement the single-call entity extraction prompt and output contract alignment for Gemini 2.5 Flash. This task focuses on prompt engineering + deterministic structured output requirements so downstream parsing/validation can be reliable.

## Dependent Tasks
- [US_063 - Implement cosine similarity search for top-K retrieval]
- [US_060 - Split text into semantic chunks with overlap] (context size control)

## Impacted Components
- [CREATE: worker/entity_extraction/prompt_builder.py]
- [CREATE: worker/entity_extraction/models.py]
- [CREATE: worker/entity_extraction/__init__.py]
- [CREATE: worker/tests/test_entity_extraction_prompt_builder.py]
- [MODIFY: contracts/entities/v1/README.md] (only if contract documentation needs updates for conflict output expectations)

## Implementation Plan
- Define the extraction categories in one prompt covering the 10 core categories:
  - Patient demographics, allergies, medications, diagnoses, procedures, labs, vitals, social history, clinical notes, document metadata
- Define a single deterministic response shape aligned to `contracts/entities/v1/entity.schema.json`:
  - `schema_version`, `document_id`, `extracted_entities[]`, optional `additional_entities`
  - Ensure `extracted_entities[]` includes `entity_group_name`, `entity_name`, `entity_value`, plus optional `rationale`, `source_text`, and `document_location`.
  - Ensure `conflicts[]` output format is explicitly described (per-entity conflicts list including conflicting values + sources).
- Implement `prompt_builder.build_entity_extraction_prompt(...)` that accepts:
  - `document_id`
  - retrieved chunk texts + per-chunk metadata (document/page/section/coordinates when available)
  - and returns a prompt string suitable for a single Gemini call.
- Prompt requirements:
  - Instruct strict JSON-only output (no markdown)
  - Instruct grounding requirement: every entity must include `source_text` and `document_location` when possible
  - Instruct conflict detection: if multiple conflicting values appear across chunks, include them in `conflicts[]` under the primary entity record
  - Handle missing data: omit entities not present rather than hallucinating
- Add unit tests validating:
  - Prompt includes all required categories
  - Prompt contains explicit JSON output constraints
  - Prompt embeds retrieved chunks with their provenance in a bounded, consistent structure

**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | worker/entity_extraction/prompt_builder.py | Build a single-call Gemini extraction prompt that requests all categories and a strict JSON response aligned to the entity contract |
| CREATE | worker/entity_extraction/models.py | Internal typed structures for retrieval chunks (text + provenance) and prompt inputs |
| CREATE | worker/entity_extraction/__init__.py | Public exports for prompt builder and extraction models |
| CREATE | worker/tests/test_entity_extraction_prompt_builder.py | Unit tests to validate prompt content, category coverage, and JSON-only response constraints |
| MODIFY | contracts/entities/v1/README.md | Update field documentation only if new conflict/grounding expectations must be documented for consumers |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://ai.google.dev/gemini-api/docs
- https://json-schema.org/understanding-json-schema/

## Build Commands
- python -m pip install -r worker/requirements.txt
- python -m pytest worker/tests

## Implementation Validation Strategy
- Validate the prompt explicitly instructs Gemini to return JSON-only output matching `contracts/entities/v1/entity.schema.json`.
- Validate prompt includes conflict detection instructions and grounding requirements for `source_text` + `document_location`.

## Implementation Checklist
- [ ] Define the complete list of core entity categories to be extracted in a single call
- [ ] Define the exact JSON response shape aligned to `contracts/entities/v1/entity.schema.json`
- [ ] Implement `build_entity_extraction_prompt(...)` with bounded chunk formatting and provenance
- [ ] Ensure prompt includes conflict detection and grounding requirements
- [ ] Add unit tests validating prompt determinism and category coverage
- [ ] Confirm prompt avoids leaking secrets (no API keys) and avoids logging PHI by default
