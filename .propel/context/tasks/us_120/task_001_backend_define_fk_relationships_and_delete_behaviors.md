# Task - TASK_001

## Requirement Reference
- User Story: us_120
- Story Location: .propel/context/tasks/us_120/us_120.md
- Acceptance Criteria: 
    - AC-1: Given baseline tables exist, When FK constraints are applied, Then all relationships from ERD are enforced
    - AC-2: Given FK constraint exists on DOCUMENT.patient_id, When patient is soft-deleted (is_deleted=true), Then documents remain accessible but patient cannot be hard-deleted while documents exist
    - AC-4: Given CASCADE DELETE is configured on SESSION.user_id, When user is deleted, Then all associated sessions are automatically deleted
    - AC-5: Given RESTRICT is configured on PATIENT.id, When patient deletion is attempted with existing documents, Then deletion is blocked with referential integrity error

## Task Overview
Define and/or adjust EF Core relationship mappings so that all ERD foreign keys for the 16 baseline tables are represented, and the intended delete behaviors (CASCADE / RESTRICT / SET NULL) are correctly configured in the model prior to generating a migration.
Estimated Effort: 8 hours

## Dependent Tasks
- .propel/context/tasks/us_119/task_004_backend_constraints_indexes_and_schema_validation_for_core_16_tables.md (TASK_004)

## Impacted Components
- Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs
- Server/ClinicalIntelligence.Api/Domain/Models/*

## Implementation Plan
- Validate the complete FK relationship inventory from US_120 AC-1 against the current EF model:
  - USER -> SESSION, PASSWORD_RESET_TOKEN
  - ERD_PATIENT -> DOCUMENT_BATCH, DOCUMENT, EXTRACTED_ENTITY, CONFLICT, CODE_SUGGESTION, VECTOR_QUERY_LOG
  - USER -> DOCUMENT_BATCH.uploaded_by_user_id, DOCUMENT.uploaded_by_user_id
  - DOCUMENT_BATCH -> DOCUMENT
  - DOCUMENT -> PROCESSING_JOB, DOCUMENT_CHUNK, EXTRACTED_ENTITY
  - EXTRACTED_ENTITY -> ENTITY_CITATION, CODE_SUGGESTION
  - DOCUMENT_CHUNK -> ENTITY_CITATION
  - CONFLICT -> CONFLICT_RESOLUTION
  - BILLING_CODE_CATALOG_ITEM -> CODE_SUGGESTION
  - USER (nullable) -> EXTRACTED_ENTITY.verified_by_user_id, CODE_SUGGESTION.decided_by_user_id
  - USER/SESSION (nullable) -> AUDIT_LOG_EVENT.user_id / session_id
- For each relationship, confirm:
  - FK is configured (via `HasForeignKey(...)`) and nullability matches the domain model (nullable vs required).
  - Delete behavior is explicitly set and aligned to US_120:
    - Keep `SESSION.user_id` as `DeleteBehavior.Cascade` (AC-4).
    - Change patient-linked relationships that must block hard deletes to `DeleteBehavior.Restrict` / `NoAction` where appropriate (AC-2, AC-5), ensuring soft-deleted rows remain queryable/consistent.
    - Keep nullable user-linked relationships as `DeleteBehavior.SetNull` where specified (e.g., verified/decided/audit user references).
- If required for deterministic/clear error messages, add explicit FK constraint names in Fluent mappings (e.g., via `HasConstraintName(...)`) for relationships that are asserted in tests.
- Reconcile any mismatches between model property names and column naming (ensuring the EF model produces the desired FK columns).
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs | Add/adjust Fluent mappings for all ERD FK relationships and delete behaviors (cascade/restrict/set-null) per US_120 acceptance criteria |
| MODIFY | Server/ClinicalIntelligence.Api/Domain/Models/* | If necessary, adjust navigation properties / FK nullable types to match intended nullability (e.g., nullable user references) |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/ef/core/modeling/relationships
- https://learn.microsoft.com/ef/core/saving/cascade-delete

## Build Commands
- dotnet build Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj

## Implementation Validation Strategy
- Confirm the EF model expresses all relationships listed in US_120 AC-1.
- Confirm patient hard-delete is blocked by FK RESTRICT/NO ACTION where required (AC-2, AC-5).
- Confirm session deletion behavior remains CASCADE from USER -> SESSION (AC-4).

## Implementation Checklist
- [X] Cross-check all US_120 AC-1 relationships against EF mappings and domain model properties
- [X] Set explicit delete behaviors for each FK (CASCADE / RESTRICT / SET NULL)
- [X] Ensure nullable FK columns are modeled as nullable and enforce constraints only when non-null
- [X] Add explicit FK constraint names where tests must assert exact constraint identifiers
- [X] Build the API project to ensure mappings compile
