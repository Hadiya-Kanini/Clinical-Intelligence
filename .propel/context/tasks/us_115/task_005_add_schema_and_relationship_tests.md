# Task - TASK_005

## Requirement Reference
- User Story: us_115
- Story Location: .propel/context/tasks/us_115/us_115.md
- Acceptance Criteria: 
    - Given the data model, When designed, Then it is compatible with future FHIR mapping (DR-001).
    - Given entity relationships, When defined, Then they align with FHIR resource relationships.
    - Given patient records, When stored, Then they follow patient-centric organization (demographics, encounters, observations, medications, diagnoses, procedures).

## Task Overview
Add automated tests that validate the patient-centric schema invariants (relationships and required fields) and enforce alignment to the documented FHIR mapping decisions, so future schema changes do not accidentally break integration readiness.

## Dependent Tasks
- .propel/context/tasks/us_115/task_001_define_fhir_alignment_mapping_matrix.md (TASK_001)
- .propel/context/tasks/us_115/task_002_implement_patient_centric_efcore_schema.md (TASK_002)

## Impacted Components
- Server/ClinicalIntelligence.Api.Tests/

## Implementation Plan
- Add EF Core model validation tests that assert:
  - Required relationships exist (e.g., Patient -> Encounters; Patient -> Observations/Medications/Conditions/Procedures)
  - Foreign keys and cascade rules behave as intended
  - Key indexes/constraints exist for patient matching (MRN, name + DOB)
- Add lightweight contract-alignment tests that verify the mapping document exists and is kept up to date when schema changes occur.
- Ensure tests are stable and runnable in CI without external dependencies.
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api.Tests/PatientDomainModelTests.cs | Tests for EF Core model relationships and constraints |
| CREATE | Server/ClinicalIntelligence.Api.Tests/FhirAlignmentContractTests.cs | Tests to ensure FHIR alignment docs exist and remain consistent |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/ef/core/testing/

## Build Commands
- dotnet test Server/ClinicalIntelligence.Api.Tests/ClinicalIntelligence.Api.Tests.csproj

## Implementation Validation Strategy
- Verify tests fail when required relationships are removed or renamed.
- Verify tests fail when the FHIR alignment artifact is missing.
- Verify tests run deterministically in a clean environment.

## Implementation Checklist
- [x] Add EF Core model invariant tests for patient-centric relationships
- [x] Add FHIR alignment artifact presence/consistency tests
- [x] Ensure tests run in CI and provide actionable failures
