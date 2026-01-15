# Task - TASK_002

## Requirement Reference
- User Story: us_120
- Story Location: .propel/context/tasks/us_120/us_120.md
- Acceptance Criteria: 
    - AC-1: Given baseline tables exist, When FK constraints are applied, Then all relationships from ERD are enforced
    - AC-2: Given FK constraint exists on DOCUMENT.patient_id, When patient is soft-deleted (is_deleted=true), Then documents remain accessible but patient cannot be hard-deleted while documents exist
    - AC-5: Given RESTRICT is configured on PATIENT.id, When patient deletion is attempted with existing documents, Then deletion is blocked with referential integrity error

## Task Overview
Generate and apply an EF Core migration that introduces/updates foreign key constraints for the 16 ERD baseline tables, ensuring safe ordering of constraint creation and correct referential actions (CASCADE/RESTRICT/SET NULL) for existing schema.
Estimated Effort: 8 hours

## Dependent Tasks
- .propel/context/tasks/us_120/task_001_backend_define_fk_relationships_and_delete_behaviors.md (TASK_001)

## Impacted Components
- Server/ClinicalIntelligence.Api/Migrations/*
- Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs
- Server/ClinicalIntelligence.Api/Data/ApplicationDbContextModelSnapshot.cs

## Implementation Plan
- Review the current baseline migration(s) and snapshot to confirm which FKs already exist and which must be added/adjusted.
- Generate a new migration from the updated model:
  - Ensure migration only adds/updates FK constraints and does not unintentionally rename tables/columns.
  - Verify referential actions match the intended delete behaviors for each relationship.
- Handle ordering/circular dependency concerns:
  - Prefer creation order that ensures parent tables exist before adding child FKs.
  - If EF generates cyclic constraints, validate whether deferrable constraints are needed; otherwise split into multiple `AddForeignKey` statements in `Up` with stable ordering.
- Ensure patient hard-delete is blocked while documents exist:
  - Confirm FK from `documents.patient_id` to `erd_patients.id` uses RESTRICT/NO ACTION.
  - Confirm soft delete behavior stays at the application/query filter layer, not via FK.
- Verify nullable FKs:
  - `extracted_entities.verified_by_user_id`, `code_suggestions.decided_by_user_id`, `audit_log_events.user_id`, `audit_log_events.session_id` should allow NULLs.
- Confirm constraint naming is stable:
  - If tests rely on constraint names, ensure EF-generated names are predictable or set explicit names in Fluent mapping.
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api/Migrations/*_US120_ForeignKeysAndReferentialIntegrity.cs | Migration that adds/updates FK constraints and delete behaviors per US_120 |
| MODIFY | Server/ClinicalIntelligence.Api/Migrations/ApplicationDbContextModelSnapshot.cs | Snapshot updates reflecting the final FK graph |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/ef/core/managing-schemas/migrations/
- https://learn.microsoft.com/ef/core/saving/cascade-delete

## Build Commands
- dotnet build Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj
- dotnet test Server/ClinicalIntelligence.Api.Tests/ClinicalIntelligence.Api.Tests.csproj

## Implementation Validation Strategy
- Apply migration to a clean Postgres database and ensure it succeeds.
- Apply migration to an existing baseline database (with US_119 applied) and ensure it succeeds without data loss.
- Verify `DELETE` behavior for critical relationships (patient/documents restrict; user/sessions cascade) via integration tests.

## Implementation Checklist
- [X] Review existing baseline migrations/snapshot for current FK coverage
- [X] Generate a dedicated US_120 FK migration and inspect produced SQL for correctness
- [X] Adjust migration ordering if needed to resolve dependency/cycle issues
- [X] Confirm RESTRICT/NO ACTION semantics for patient-linked critical tables
- [X] Confirm nullable FKs remain nullable and do not break existing rows
- [X] Run build + tests
