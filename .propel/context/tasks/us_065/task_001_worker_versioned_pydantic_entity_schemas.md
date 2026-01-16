# Task - [TASK_001]

## Requirement Reference
- User Story: [us_065]
- Story Location: [.propel/context/tasks/us_065/us_065.md]
- Acceptance Criteria: 
    - [Given extracted entities, When validated, Then they must conform to versioned Pydantic schemas.]
    - [Given schema versions, When updated, Then backward compatibility is maintained.]

## Task Overview
Introduce **versioned Pydantic schemas** for extracted entity payloads (initially `v1`) so the AI worker can validate structured LLM output using Pydantic (TR-007 / FR-038) while preserving an explicit schema versioning strategy for future upgrades.

This task focuses on defining the Pydantic models and a lightweight version registry/selector that can validate by `schema_version` without breaking older versions.

## Dependent Tasks
- [US_064 TASK_001] (Worker: extraction prompt outputs aligned to `contracts/entities/v1/entity.schema.json`)
- [US_064 TASK_003] (Worker: parse + validate LLM response into structured entity payload)

## Impacted Components
- [CREATE | worker/entity_schemas/__init__.py | Package for versioned entity Pydantic schemas]
- [CREATE | worker/entity_schemas/v1.py | Pydantic models for entity extraction output schema v1]
- [CREATE | worker/entity_schemas/registry.py | Registry/selector to resolve schema validators by version]
- [MODIFY | worker/requirements.txt | Add Pydantic dependency for schema validation]

## Implementation Plan
- Define Pydantic models for schema version `1.0` based on the existing contract:
  - `contracts/entities/v1/entity.schema.json`
  - Required: `schema_version`, `document_id`, `extracted_entities[]`
  - Optional: `additional_entities`
- Model nested structures in `v1.py`:
  - `DocumentLocation` (optional fields + optional nested `coordinates`)
  - `Conflict` (required `conflicting_value`; optional provenance fields)
  - `ExtractedEntity` (required `entity_group_name`, `entity_name`, `entity_value`; optional `rationale`, `source_text`, `document_location`, `conflicts[]`)
  - `EntityExtractionResultV1` (top-level payload)
- Enforce strict schema version semantics:
  - Pydantic model should constrain `schema_version` to the supported value (e.g., literal `"1.0"`)
- Add a `registry.py` that:
  - Exposes `get_entity_schema(schema_version: str)` (returns the correct Pydantic model/type)
  - Provides a single authoritative list of supported versions
  - Fails deterministically (clear exception) for unknown versions
- Ensure backward compatibility strategy is explicitly supported:
  - `registry.py` should be the only place that is updated when new versions are introduced.

**Focus on how to implement**

## Current Project State
- contracts/
  - entities/v1/entity.schema.json
  - jobs/v1/job.schema.json
- worker/
  - main.py
  - requirements.txt
  - tests/

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | worker/entity_schemas/__init__.py | Initialize entity schema package and expose public APIs |
| CREATE | worker/entity_schemas/v1.py | Define Pydantic v1 models aligned to `contracts/entities/v1/entity.schema.json` |
| CREATE | worker/entity_schemas/registry.py | Resolve correct Pydantic model by `schema_version` and centralize supported versions |
| MODIFY | worker/requirements.txt | Add `pydantic` dependency for runtime validation |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://docs.pydantic.dev/

## Build Commands
- python -m pip install -r worker/requirements.txt
- python -m pytest worker/tests

## Implementation Validation Strategy
- Validate that schema version selection works:
  - `schema_version="1.0"` resolves to v1 Pydantic model
  - unknown versions raise clear, deterministic errors
- Validate that required fields and nested structures match the JSON contract expectations.

## Implementation Checklist
- [ ] Implement Pydantic v1 models matching `contracts/entities/v1/entity.schema.json`
- [ ] Implement schema registry that resolves validators by `schema_version`
- [ ] Ensure unknown schema versions fail deterministically
- [ ] Update `worker/requirements.txt` to include Pydantic
- [ ] Confirm schema versioning strategy supports backward compatibility for future versions
