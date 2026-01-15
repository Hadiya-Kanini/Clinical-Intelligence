# Task - TASK_002

## Requirement Reference
- User Story: us_115
- Story Location: .propel/context/tasks/us_115/us_115.md
- Acceptance Criteria: 
    - Given patient records, When stored, Then they follow patient-centric organization (demographics, encounters, observations, medications, diagnoses, procedures).
    - Given the schema, When designed, Then it supports Phase 1 standalone usage while being integration-ready.
    - Given entity relationships, When defined, Then they align with FHIR resource relationships.

## Task Overview
Implement the initial EF Core entity model and relationships for a patient-centric schema, aligned to the FHIR mapping decisions from TASK_001, and generate a database migration that establishes the core tables and referential integrity.

## Dependent Tasks
- .propel/context/tasks/us_004/task_002_scaffold_database_migration_mechanism.md (TASK_002)
- .propel/context/tasks/us_004/task_003_create_baseline_db_migration_and_guardrail.md (TASK_003)
- .propel/context/tasks/us_115/task_001_define_fhir_alignment_mapping_matrix.md (TASK_001)

## Impacted Components
- Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs
- Server/ClinicalIntelligence.Api/Migrations/

## Implementation Plan
- Define EF Core entities for the core patient-centric schema:
  - Patient (demographics)
  - Encounter (visit context)
  - Observation (labs/vitals/measurements)
  - MedicationStatement (medications)
  - Condition (diagnoses)
  - Procedure (procedures)
  - DocumentReference (linking ingested documents to patient/encounter)
- Implement the relationships consistent with FHIR references:
  - Patient 1..* Encounter
  - Patient 1..* Observation/MedicationStatement/Condition/Procedure
  - Encounter 1..* Observation/Procedure (where applicable)
  - Patient 1..* DocumentReference
- Update `ApplicationDbContext` with DbSets and explicit `OnModelCreating` constraints/indexes for:
  - Patient identity matching (MRN, name + DOB)
  - Foreign key relationships and cascade behaviors
- Generate a new EF Core migration that creates the tables and foreign keys.
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs | Add DbSets and relationship configuration for the patient-centric schema |
| CREATE | Server/ClinicalIntelligence.Api/Domain/Models/Patient.cs | Patient entity aligned to FHIR Patient (demographics + identifiers) |
| CREATE | Server/ClinicalIntelligence.Api/Domain/Models/Encounter.cs | Encounter/visit entity aligned to FHIR Encounter |
| CREATE | Server/ClinicalIntelligence.Api/Domain/Models/Observation.cs | Observation entity aligned to FHIR Observation (labs/vitals) |
| CREATE | Server/ClinicalIntelligence.Api/Domain/Models/MedicationStatement.cs | MedicationStatement entity aligned to FHIR MedicationStatement |
| CREATE | Server/ClinicalIntelligence.Api/Domain/Models/Condition.cs | Condition entity aligned to FHIR Condition (diagnoses) |
| CREATE | Server/ClinicalIntelligence.Api/Domain/Models/Procedure.cs | Procedure entity aligned to FHIR Procedure |
| CREATE | Server/ClinicalIntelligence.Api/Domain/Models/DocumentReference.cs | DocumentReference entity aligned to FHIR DocumentReference to link ingested documents |
| CREATE | Server/ClinicalIntelligence.Api/Migrations/*_AddPatientCentricDomainModel.cs | EF Core migration establishing tables + constraints |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/ef/core/modeling/
- https://hl7.org/fhir/patient.html

## Build Commands
- dotnet build Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj
- dotnet test Server/ClinicalIntelligence.Api.Tests/ClinicalIntelligence.Api.Tests.csproj

## Implementation Validation Strategy
- Verify EF migration creates tables and foreign keys for each core entity.
- Verify referential integrity exists for Patient-centric relationships (Patient -> Encounter, Patient -> clinical facts).
- Verify the schema can represent Phase 1 data without requiring any external EHR identifiers.

## Implementation Checklist
- [x] Add EF Core entities for Patient, Encounter, Observation, MedicationStatement, Condition, Procedure, DocumentReference
- [x] Configure relationships and indexes in ApplicationDbContext
- [x] Generate and commit migration for the new schema
- [x] Run API tests to ensure DbContext can be created and migration applies cleanly
