# Task - TASK_004

## Requirement Reference
- User Story: us_115
- Story Location: .propel/context/tasks/us_115/us_115.md
- Acceptance Criteria: 
    - Given patient records, When stored, Then they follow patient-centric organization (demographics, encounters, observations, medications, diagnoses, procedures).
    - Given the schema, When designed, Then it supports Phase 1 standalone usage while being integration-ready.

## Task Overview
Introduce a patient aggregate retrieval contract (DTO/query model) and repository/service query pattern that returns the patient-centric organization required by downstream features (Patient 360 and exports), while preserving the underlying FHIR-aligned relationships.

## Dependent Tasks
- .propel/context/tasks/us_115/task_002_implement_patient_centric_efcore_schema.md (TASK_002)

## Impacted Components
- Server/ClinicalIntelligence.Api/

## Implementation Plan
- Define a patient aggregate DTO that organizes data into:
  - demographics
  - encounters
  - observations
  - medications
  - diagnoses
  - procedures
- Implement a read-oriented repository/service that can load the aggregate efficiently from EF Core, including relationship traversal for:
  - Patient -> Encounters
  - Patient -> clinical facts (observations/medications/conditions/procedures)
- Keep the read contract decoupled from future FHIR import/export representations so Phase 1 consumers remain stable.
- Add minimal API contract stubs only if needed to validate the query shape (do not implement feature-level UX here).
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api/Contracts/PatientAggregateResponse.cs | Read contract organizing patient data by category for downstream usage |
| CREATE | Server/ClinicalIntelligence.Api/Services/PatientAggregateService.cs | Loads the aggregate from EF Core using patient-centric organization |
| CREATE | Server/ClinicalIntelligence.Api/Services/IPatientAggregateService.cs | Abstraction for dependency injection and testing |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/ef/core/querying/related-data/

## Build Commands
- dotnet build Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj
- dotnet test Server/ClinicalIntelligence.Api.Tests/ClinicalIntelligence.Api.Tests.csproj

## Implementation Validation Strategy
- Verify the aggregate contract includes all Phase 1 categories and is stable across internal schema evolution.
- Verify relationships are loaded correctly and missing categories produce empty collections (not nulls) for predictable consumers.

## Implementation Checklist
- [x] Define PatientAggregateResponse contract structure aligned with acceptance criteria
- [x] Implement PatientAggregateService query logic and register with DI
- [x] Add unit tests for the service query behavior and shape
