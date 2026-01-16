# Task - TASK_066_002

## Requirement Reference
- User Story: us_066
- Story Location: .propel/context/tasks/us_066/us_066.md
- Acceptance Criteria: 
    - Given a document, When processed, Then demographics, allergies, meds, diagnoses, procedures, labs, vitals, social history, notes, and metadata are extracted.
- Edge Cases:
    - What happens when a category has no data in the document?
    - How does the system handle partial or incomplete data?
    - What happens when data is in non-standard formats?

## Task Overview
Implement deterministic handling rules for core-category extraction when data is missing, partial, or represented in non-standard formats.

This task ensures the worker produces a stable `extracted_entities[]` list that:
- Does **not** hallucinate placeholders when a category is absent
- Uses the canonical taxonomy (TASK_066_001) even when values are partial
- Normalizes common non-standard formats into consistent values where feasible (without inventing data)

This task focuses on worker-side output shaping and unit tests. It does not implement grounding enforcement (US_069) or persistence into the backend database.

## Dependent Tasks
- [TASK_066_001 - Worker: core category taxonomy and prompt alignment]
- [US_064 TASK_001] (Worker: prompt builder)
- [US_064 TASK_003] (Worker: parse and validate LLM response)
- [US_065 TASK_001] (Worker: versioned Pydantic entity schemas)
- [US_065 TASK_002] (Worker: Pydantic validation integration + PHI-safe errors)

## Impacted Components
- [MODIFY | worker/entity_extraction/response_parser.py | Add post-parse normalization hooks for missing/partial/non-standard values (must remain PHI-safe)]
- [CREATE | worker/entity_extraction/normalization.py | Deterministic normalization helpers for core categories (no hallucination; safe coercions only)]
- [MODIFY | worker/tests/test_entity_extraction_response_parsing.py | Add test cases for missing categories, partial values, and non-standard formats]
- [MODIFY | worker/tests/fixtures/entity_payloads.py | Add fixtures representing typical non-standard formats and partial fields]

## Implementation Plan
- Define deterministic output rules:
  - Missing category behavior: emit **no entities** for that category (do not emit `entity_value` like "N/A", "Unknown", empty strings).
  - Partial values:
    - Preserve the value as-is if it is grounded and non-empty.
    - Do not fabricate missing fields (e.g., medication dosage).
  - Non-standard formats:
    - Normalize only when a safe transform exists without losing meaning or adding information.
- Implement `normalization.py` utilities (examples):
  - Date normalization (best-effort) to `YYYY-MM-DD` when input clearly matches common patterns (e.g., `MM/DD/YYYY`, `DD-MMM-YYYY`), otherwise leave unchanged.
  - Vital signs canonicalization:
    - Preserve original value if ambiguous.
    - If `BP` is provided as `120/80`, keep as value; do not split unless explicitly present as separate fields.
  - Lab result formatting:
    - If value and unit are combined (e.g., `7.2 mg/dL`), keep as a single `entity_value` unless the model already provided separate unit fields (contract currently does not include a dedicated unit field).
- Apply normalization as a post-parse step after JSON extraction and before final validation/persistence boundaries.
  - Ensure normalization does not introduce PHI into exception strings.
  - Ensure normalization never increases the number of entities beyond what the model returned.
- Add unit tests covering:
  - Absent categories: response returns no entities for those groups.
  - Partial data: entities remain present but do not include invented values.
  - Non-standard formats: safe date normalization transforms; ambiguous formats remain unchanged.

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | worker/entity_extraction/normalization.py | Deterministic normalization utilities for missing/partial/non-standard data handling |
| MODIFY | worker/entity_extraction/response_parser.py | Invoke normalization post-parse and prior to final validation/persistence |
| MODIFY | worker/tests/fixtures/entity_payloads.py | Add fixtures for missing-category, partial-data, and non-standard format scenarios |
| MODIFY | worker/tests/test_entity_extraction_response_parsing.py | Add unit tests validating deterministic behavior for edge cases |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://json-schema.org/understanding-json-schema/

## Build Commands
- python -m pip install -r worker/requirements.txt
- python -m pytest worker/tests

## Implementation Validation Strategy
- [Unit] Given a payload with missing categories, ensure output contains zero entities for those groups.
- [Unit] Given partial values, ensure output preserves known data without inserting placeholders.
- [Unit] Given non-standard date formats, ensure best-effort normalization is applied only when unambiguous.

## Implementation Checklist
- [ ] Define deterministic rules for missing categories, partial values, and non-standard formats
- [ ] Implement normalization utilities with safe transforms only
- [ ] Wire normalization into the parse/validate pipeline without changing schema versioning
- [ ] Add fixtures for missing-category and non-standard format cases
- [ ] Add unit tests validating deterministic behavior and no hallucinated placeholders
- [ ] Confirm errors/logs remain PHI-safe (no raw document or long excerpts)
