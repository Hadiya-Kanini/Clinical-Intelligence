# Task - TASK_067_001

## Requirement Reference
- User Story: us_067
- Story Location: .propel/context/tasks/us_067/us_067.md
- Acceptance Criteria: 
    - Given the extraction system, When designed, Then it supports adding new categories without major code changes (FR-049).
    - Given new categories, When added, Then they follow the same schema validation pattern.

## Task Overview
Introduce a canonical, versioned “entity category registry” contract that defines extraction categories as data (not code). This registry will be consumed by the AI worker prompt builder (and optionally other services) to enumerate categories in a configurable way.

This task establishes:
- A stable place to declare categories (core + future extensions)
- Deterministic validation of category definitions (schema-validated)
- Explicit handling for category conflicts, aliases, and deprecation states

## Dependent Tasks
- [US_064 TASK_001] (Worker prompt builder uses category list)

## Impacted Components
- [CREATE | contracts/entities/v1/entity_categories.schema.json | JSON schema for validating the category registry structure (ids, display names, status, aliases)]
- [CREATE | contracts/entities/v1/entity_categories.json | Versioned category registry data file used by prompt builder]
- [MODIFY | contracts/entities/v1/README.md | Document how categories are extended via the registry and how validation/compatibility works]
- [CREATE | contracts/migrations/<new_migration_note>.md | Contract migration note documenting new category registry artifacts]

## Implementation Plan
- Define a new contract schema `entity_categories.schema.json` that validates:
  - `schema_version`
  - `categories[]` entries with:
    - `category_id` (stable identifier used as `entity_group_name`)
    - `display_name`
    - `status` in {`active`,`deprecated`}
    - `aliases[]` (optional alternate ids that map to a canonical category)
  - Constraints:
    - Unique `category_id`
    - No alias conflicts (an alias cannot equal another category_id)
- Create the initial `entity_categories.json`:
  - Seed with the core categories from US_066 (patient demographics, allergies, medications, diagnoses, procedures, labs, vitals, social history, clinical notes, document metadata)
  - Keep ids within backend DB constraints when persisted (e.g., <=50 chars to align with `ExtractedEntity.Category` max length)
- Update `contracts/entities/v1/README.md`:
  - Clarify that `entity_group_name` in `entity.schema.json` must match a `category_id` (or an alias that resolves to one)
  - Define deprecation behavior: deprecated categories are accepted but should be flagged by consumers
  - Define conflict behavior: registry load must fail deterministically if ids/aliases collide
- Add a migration note under `contracts/migrations/` documenting:
  - New artifacts added
  - Backward compatibility expectations
  - Consumer responsibilities (worker must load/validate the registry)

**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | contracts/entities/v1/entity_categories.schema.json | JSON schema for the entity category registry (ids, status, aliases) |
| CREATE | contracts/entities/v1/entity_categories.json | Initial category registry seeded with core categories + extensibility fields |
| MODIFY | contracts/entities/v1/README.md | Document category extensibility, conflict handling, and deprecation guidance |
| CREATE | contracts/migrations/<new_migration_note>.md | Record contract addition and consumer impact |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://json-schema.org/

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- Validate `entity_categories.json` passes `entity_categories.schema.json` validation.
- Validate the registry contains the 10 core categories from US_066 without collisions.
- Validate that introducing a duplicate `category_id` or alias conflict fails schema/validation deterministically.

## Implementation Checklist
- [ ] Add `entity_categories.schema.json` with required fields and conflict-avoidance constraints
- [ ] Add initial `entity_categories.json` seeded with core categories and stable ids
- [ ] Update `contracts/entities/v1/README.md` with extensibility + deprecation rules
- [ ] Add a contract migration note documenting the new registry artifacts
- [ ] Manually validate (or add a small validator harness if already present) that the JSON file conforms to the schema
