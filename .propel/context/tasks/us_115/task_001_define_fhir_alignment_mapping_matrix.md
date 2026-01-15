# Task - TASK_001

## Requirement Reference
- User Story: us_115
- Story Location: .propel/context/tasks/us_115/us_115.md
- Acceptance Criteria: 
    - Given the data model, When designed, Then it is compatible with future FHIR mapping (DR-001).
    - Given entity relationships, When defined, Then they align with FHIR resource relationships.

## Task Overview
Define the canonical, patient-centric domain model boundaries and produce a concrete mapping matrix between the platform entities and HL7 FHIR resources (with explicit handling for unmappable data and FHIR version evolution), so the database schema and APIs can be implemented without future refactoring.

## Dependent Tasks
- .propel/context/tasks/us_001/task_002_establish_contracts_structure_and_guardrails.md (TASK_002)

## Impacted Components
- contracts/entities/v1/
- contracts/migrations/

## Implementation Plan
- Define the patient-centric aggregates and their responsibilities (e.g., Patient, Encounter/Visit, Clinical Facts such as Observation/Medication/Condition/Procedure).
- Create a mapping matrix document that maps:
  - Internal entity -> FHIR resource type
  - Internal field -> FHIR element path
  - Cardinality and relationship direction
  - Notes on differences / transformations
- Define how the platform will handle:
  - Data that does not map cleanly to FHIR (extensions vs free-form JSON)
  - FHIR versioning changes (baseline version, compatibility approach, and upgrade path)
  - Multiple FHIR versions (storage strategy for external identifiers and resource linkage)
- Add required migration notes scaffolding specific to the domain model versioning so future schema changes remain auditable and intentional.
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | contracts/entities/v1/fhir_alignment.md | Mapping matrix: internal domain entities/fields to FHIR resources/elements; includes relationship mapping and transformation notes |
| MODIFY | contracts/entities/v1/README.md | Document how the FHIR alignment doc relates to entity.schema.json and how versions are managed |
| CREATE | contracts/migrations/domain_model_v1.md | Migration notes and compatibility guidance for the patient-centric domain model |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://hl7.org/fhir/
- https://hl7.org/fhir/patient.html
- https://hl7.org/fhir/encounter.html
- https://hl7.org/fhir/observation.html
- https://hl7.org/fhir/medicationstatement.html
- https://hl7.org/fhir/condition.html
- https://hl7.org/fhir/procedure.html

## Build Commands
- python scripts/validate_contracts.py

## Implementation Validation Strategy
- Verify the mapping matrix covers the Phase 1 categories (demographics, encounters, observations, medications, diagnoses, procedures) and explicitly identifies any intentional gaps.
- Verify relationship mappings are consistent with FHIR references (e.g., Observation.subject -> Patient, Encounter.subject -> Patient, Condition.subject -> Patient).
- Verify a clear approach is documented for unmappable data and FHIR version evolution.

## Implementation Checklist
- [x] Define patient-centric aggregates and cross-aggregate relationships
- [x] Produce internal-to-FHIR mapping matrix (resources + element paths)
- [x] Document edge-case handling (unmappable data, FHIR changes, multi-version support)
- [x] Add and validate domain model migration notes artifact
