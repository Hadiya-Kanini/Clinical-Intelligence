# Task - TASK_069_001

## Requirement Reference
- User Story: us_069 (extracted from input)
- Story Location: [.propel/context/tasks/us_069/us_069.md]
- Acceptance Criteria: 
    - Given an extracted entity, When validated, Then it must have valid source citations (FR-051).
    - Given entities without citations, When detected, Then they are rejected and not stored as final (FR-056, NFR-006).
    - Given citations, When stored, Then they include document_id, page, section, coordinates, and cited text (TR-008).

## Task Overview
Update the entity extraction output contract to support deterministic enforcement of 100% grounding by introducing a grounded contract variant (`schema_version` = `1.1`) that requires per-entity citation fields. This ensures downstream worker/backend validators can reliably reject ungrounded entities and persist grounded provenance consistently.

## Dependent Tasks
- [US_064 TASK_001] (Worker prompt builder includes grounding instructions and `document_location` / `source_text` fields)
- [US_065 TASK_002] (Worker validation failure semantics)

## Impacted Components
- [MODIFY | contracts/entities/v1/entity.schema.json | Add schema_version `1.1` and enforce citation requirements (via draft-07 `if/then`) for grounded entities]
- [MODIFY | contracts/entities/v1/README.md | Document grounding requirements and schema_version differences (`1.0` vs `1.1`)]
- [CREATE | contracts/migrations/entity_contract_v1_1_grounding_required.md | Contract migration note for consumers (worker/backend) describing required citation fields and enforcement expectations]

## Implementation Plan
- Extend `contracts/entities/v1/entity.schema.json` to include `schema_version` enum value `"1.1"`.
- Implement JSON Schema draft-07 conditional validation:
  - If `schema_version == "1.1"`:
    - Require `extracted_entities[].source_text` (cited text) and `extracted_entities[].document_location`.
    - Require `document_location.page`, `document_location.section`, and `document_location.coordinates`.
    - Ensure `coordinates` requires `x`, `y`, `width`, `height` (already present).
    - Add minimal non-empty constraints for `source_text` (e.g., `minLength: 1`) so citations are not blank.
- Keep `schema_version == "1.0"` behavior unchanged for backward compatibility, but document that US_069 enforcement expects `1.1` for production runs.
- Update `contracts/entities/v1/README.md`:
  - Define “valid citation” for this contract (required fields, coordinate expectations).
  - Document consumer responsibilities:
    - Worker must emit `schema_version = 1.1` once grounding is enforced.
    - Backend must reject/avoid persistence if citations are missing (FR-056).
- Add a migration note under `contracts/migrations/` summarizing:
  - What changed
  - Compatibility notes
  - Required rollout steps for worker/backend

**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | contracts/entities/v1/entity.schema.json | Add `schema_version: 1.1` and require `document_location` + `source_text` for grounded entities using `if/then` |
| MODIFY | contracts/entities/v1/README.md | Document grounding requirements and rollout guidance for consumers |
| CREATE | contracts/migrations/entity_contract_v1_1_grounding_required.md | Contract migration note for introducing grounded entity schema v1.1 |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://json-schema.org/draft-07/json-schema-validation.html

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Contract] Validate that a `schema_version: 1.1` payload missing `source_text` fails schema validation.
- [Contract] Validate that a `schema_version: 1.1` payload missing `document_location.coordinates.{x,y,width,height}` fails schema validation.
- [Contract] Validate that a `schema_version: 1.0` payload continues to validate as before (no breaking schema enforcement).

## Implementation Checklist
- [ ] Add `schema_version: 1.1` and draft-07 `if/then` enforcement for citation fields
- [ ] Require `source_text` with `minLength: 1` for grounded payloads
- [ ] Require `document_location.page`, `section`, and `coordinates` for grounded payloads
- [ ] Update `contracts/entities/v1/README.md` with grounding rules and rollout notes
- [ ] Add contract migration note documenting the new grounded requirements and consumer actions
