# Task - TASK_003

## Requirement Reference
- User Story: us_115
- Story Location: .propel/context/tasks/us_115/us_115.md
- Acceptance Criteria: 
    - Given the data model, When designed, Then it is compatible with future FHIR mapping (DR-001).
    - Given the schema, When designed, Then it supports Phase 1 standalone usage while being integration-ready.

## Task Overview
Add explicit schema support for future EHR/FHIR integration by introducing resource-linking metadata (external IDs, resource types, and versioning) and an intentional extension strategy for data that does not map cleanly to FHIR.

## Dependent Tasks
- .propel/context/tasks/us_115/task_001_define_fhir_alignment_mapping_matrix.md (TASK_001)
- .propel/context/tasks/us_115/task_002_implement_patient_centric_efcore_schema.md (TASK_002)

## Impacted Components
- Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs
- Server/ClinicalIntelligence.Api/Migrations/

## Implementation Plan
- Define a `FhirResourceLink` (or equivalent) entity that can associate platform entities (Patient, Encounter, Observation, etc.) to external FHIR resources by:
  - resource_type (e.g., Patient, Encounter, Observation)
  - resource_id (FHIR logical id)
  - fhir_version (e.g., R4 baseline; support storing multiple)
  - source_system identifier (optional)
- Add an extension/unmapped-data strategy for fields that do not map cleanly to FHIR:
  - Prefer a structured JSON column for extensions (e.g., `jsonb` in PostgreSQL)
  - Document allowed content and PHI handling constraints
- Generate an EF Core migration introducing the new table/columns.
- Ensure the design supports Phase 1 standalone usage (no external IDs required) while allowing gradual enrichment later.
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api/Domain/Models/FhirResourceLink.cs | Maps internal entities to external FHIR resources with version support |
| MODIFY | Server/ClinicalIntelligence.Api/Domain/Models/Patient.cs | Add optional extension/unmapped-data storage and link support |
| MODIFY | Server/ClinicalIntelligence.Api/Domain/Models/Encounter.cs | Add optional extension/unmapped-data storage and link support |
| MODIFY | Server/ClinicalIntelligence.Api/Domain/Models/Observation.cs | Add optional extension/unmapped-data storage and link support |
| MODIFY | Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs | Configure FhirResourceLink and any extension field mappings |
| CREATE | Server/ClinicalIntelligence.Api/Migrations/*_AddFhirResourceLinking.cs | Migration for FHIR linking + extension support |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://hl7.org/fhir/extensibility.html
- https://hl7.org/fhir/resource.html

## Build Commands
- dotnet build Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj
- dotnet test Server/ClinicalIntelligence.Api.Tests/ClinicalIntelligence.Api.Tests.csproj

## Implementation Validation Strategy
- Verify records can be stored without any FHIR identifiers (Phase 1).
- Verify optional FHIR links can be added later without schema changes.
- Verify unmappable data can be stored intentionally and safely (documented schema expectations).

## Implementation Checklist
- [x] Add FhirResourceLink entity + relationships
- [x] Add extension/unmapped-data fields to core entities per mapping doc
- [x] Generate migration and validate schema changes apply cleanly
- [x] Ensure schema choices support multiple FHIR versions without rework
