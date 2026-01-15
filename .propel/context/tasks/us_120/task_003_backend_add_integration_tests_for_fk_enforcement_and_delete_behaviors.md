# Task - TASK_003

## Requirement Reference
- User Story: us_120
- Story Location: .propel/context/tasks/us_120/us_120.md
- Acceptance Criteria: 
    - AC-1: Given baseline tables exist, When FK constraints are applied, Then all relationships from ERD are enforced
    - AC-3: Given FK constraint exists, When an INSERT with invalid foreign key is attempted, Then PostgreSQL returns constraint violation error with clear message
    - AC-4: Given CASCADE DELETE is configured on SESSION.user_id, When user is deleted, Then all associated sessions are automatically deleted
    - AC-5: Given RESTRICT is configured on PATIENT.id, When patient deletion is attempted with existing documents, Then deletion is blocked with referential integrity error

## Task Overview
Extend database integration tests to validate foreign key enforcement and referential actions (cascade/restrict/set-null) across the ERD baseline tables, specifically covering the US_120 acceptance criteria scenarios.
Estimated Effort: 6 hours

## Dependent Tasks
- .propel/context/tasks/us_120/task_002_backend_generate_fk_constraints_migration_for_existing_schema.md (TASK_002)

## Impacted Components
- Server/ClinicalIntelligence.Api.Tests/BaselineSchemaMigrationValidationTests.cs
- Server/ClinicalIntelligence.Api.Tests/

## Implementation Plan
- Extend the existing integration test suite used for US_119 baseline validation to include US_120 scenarios:
  - FK enforcement (AC-3): attempt to insert rows with non-existent parents for multiple key relationships (not just SESSION -> USER) to ensure database enforces constraints.
  - CASCADE behavior (AC-4): confirm deleting a USER cascades deletion to SESSION rows (existing test can be retained/expanded).
  - RESTRICT behavior (AC-5):
    - Create an ERD patient, create a document referencing that patient, then attempt to delete the patient record.
    - Assert the operation fails with a `DbUpdateException` and that the error message indicates a FK violation.
- Prefer deterministic assertions:
  - Assert on constraint names if stable (e.g., `fk_documents_patient_id`), otherwise assert on the referenced table/column names in Postgres error message.
- Ensure tests are isolated:
  - Use unique GUIDs per test.
  - Cleanup inserted data when possible.
  - Keep Postgres availability detection and skip behavior consistent with existing tests.
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api.Tests/BaselineSchemaMigrationValidationTests.cs | Add tests for FK constraint violations and RESTRICT behavior on patient deletion when dependent records exist |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/ef/core/testing/
- https://www.postgresql.org/docs/current/errcodes-appendix.html

## Build Commands
- dotnet test Server/ClinicalIntelligence.Api.Tests/ClinicalIntelligence.Api.Tests.csproj

## Implementation Validation Strategy
- Run integration tests against a Postgres database with migrations applied.
- Verify that invalid FK inserts fail atomically and surface constraint violations.
- Verify that delete behaviors match configured referential actions (CASCADE vs RESTRICT).

## Implementation Checklist
- [X] Add FK enforcement tests for representative relationships (session->user, document->patient, extracted_entity->document)
- [X] Add RESTRICT deletion test for patient with existing documents
- [X] Confirm CASCADE deletion test for user->sessions still passes
- [X] Ensure tests skip gracefully when Postgres is not available
- [X] Run test suite
