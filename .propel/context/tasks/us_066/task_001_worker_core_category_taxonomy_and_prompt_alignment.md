# Task - TASK_066_001

## Requirement Reference
- User Story: us_066
- Story Location: .propel/context/tasks/us_066/us_066.md
- Acceptance Criteria: 
    - Given a document, When processed, Then Patient Demographics are extracted: Name, DOB, Address, Contact, MRN (FR-039).
    - Given a document, When processed, Then Allergies, Medications, Diagnoses, Procedures are extracted (FR-040-043).
    - Given a document, When processed, Then Lab Results, Vital Signs, Social History, Clinical Notes are extracted (FR-044-047).
    - Given a document, When processed, Then Document Metadata is extracted: type, date, provider, facility (FR-048).

## Task Overview
Define and enforce a canonical taxonomy for the 10 core clinical categories (FR-039 to FR-048) in the AI worker’s single-call extraction prompt so downstream parsing/validation and UI rendering can rely on consistent `entity_group_name` and `entity_name` values.

This task is focused on **category coverage and naming consistency** only. It does not implement the Gemini call orchestration or schema validation (handled in dependent tasks).

## Dependent Tasks
- [US_064 TASK_001] (Worker: single-call entity extraction prompt and schema alignment)
- [US_064 TASK_002] (Worker: Gemini single-call entity extraction client)
- [US_064 TASK_003] (Worker: parse and validate LLM response entities)
- [US_065 TASK_001] (Worker: versioned Pydantic entity schemas)

## Impacted Components
- [MODIFY | worker/entity_extraction/prompt_builder.py | Update prompt instructions to explicitly require canonical group/name keys for each core category]
- [MODIFY | worker/entity_extraction/models.py | Add canonical constants/enums for `entity_group_name` and recommended `entity_name` keys]
- [MODIFY | worker/tests/test_entity_extraction_prompt_builder.py | Extend tests to assert canonical taxonomy is embedded in the prompt]

## Implementation Plan
- Define canonical `entity_group_name` values for the 10 core categories in one authoritative place (worker code constants), aligned to the contract field `ExtractedEntity.entity_group_name`:
  - patient_demographics
  - allergies
  - medications
  - diagnoses
  - procedures
  - lab_results
  - vital_signs
  - social_history
  - clinical_notes
  - document_metadata
- Define a recommended set of `entity_name` keys for each group (non-exhaustive but deterministic defaults), aligned to FR-039..FR-048. Examples:
  - patient_demographics: name, dob, address, contact, mrn
  - allergies: allergen, reaction, severity
  - medications: medication_name, dosage, frequency, route
  - diagnoses: condition, date
  - procedures: procedure_name, date
  - lab_results: test_name, value, unit, reference_range
  - vital_signs: bp, hr, temp, spo2, weight, height
  - social_history: smoking, alcohol, occupation
  - clinical_notes: provider_notes, assessment, plan
  - document_metadata: type, date, provider, facility
- Update the single-call prompt builder so the prompt:
  - Enumerates the canonical categories and their expected keys
  - Instructs JSON-only output aligned to `contracts/entities/v1/entity.schema.json`
  - Instructs grounding requirements (source_text + document_location when possible)
  - Instructs omission of entities that are not present (no hallucinated placeholders)

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | worker/entity_extraction/prompt_builder.py | Ensure prompt explicitly requests core categories using canonical `entity_group_name` and recommended `entity_name` keys |
| MODIFY | worker/entity_extraction/models.py | Add canonical taxonomy constants/enums to prevent drift across modules |
| MODIFY | worker/tests/test_entity_extraction_prompt_builder.py | Assert prompt contains the canonical taxonomy mapping for all 10 categories |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://json-schema.org/understanding-json-schema/
- https://ai.google.dev/gemini-api/docs

## Build Commands
- python -m pip install -r worker/requirements.txt
- python -m pytest worker/tests

## Implementation Validation Strategy
- [Unit] Verify prompt includes all 10 core categories and their canonical `entity_group_name` values.
- [Unit] Verify prompt includes explicit instruction to omit missing categories (no hallucinated placeholders like "N/A").

## Implementation Checklist
- [ ] Define canonical `entity_group_name` constants for the 10 core categories
- [ ] Define recommended `entity_name` keys per category aligned to FR-039..FR-048
- [ ] Update `prompt_builder` to embed the canonical taxonomy and output constraints
- [ ] Ensure prompt instructs omission of missing data/categories
- [ ] Extend unit tests to validate canonical taxonomy presence in the prompt
- [ ] Confirm prompt guidance supports grounding (`source_text` + `document_location`) without forcing unavailable fields
