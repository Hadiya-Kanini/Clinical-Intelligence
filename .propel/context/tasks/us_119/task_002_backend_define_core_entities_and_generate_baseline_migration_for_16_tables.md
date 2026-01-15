# Task - TASK_002

## Requirement Reference
- User Story: us_119
- Story Location: .propel/context/tasks/us_119/us_119.md
- Acceptance Criteria: 
    - AC-1: Given EF Core is configured, When baseline migration is applied, Then all 16 tables are created: USER, SESSION, PASSWORD_RESET_TOKEN, PATIENT, DOCUMENT_BATCH, DOCUMENT, PROCESSING_JOB, DOCUMENT_CHUNK, EXTRACTED_ENTITY, ENTITY_CITATION, CONFLICT, CONFLICT_RESOLUTION, BILLING_CODE_CATALOG_ITEM, CODE_SUGGESTION, AUDIT_LOG_EVENT, VECTOR_QUERY_LOG
    - AC-2: Given tables are created, When schema is inspected, Then all columns match ERD specifications with correct PostgreSQL data types (uuid, varchar, text, timestamp, jsonb, vector(768))

## Task Overview
Implement the baseline ERD schema in EF Core by introducing the 16 core entities and updating `ApplicationDbContext` mappings so a new baseline migration can be generated that creates the required 16-table foundation.
Estimated Effort: 8 hours

## Dependent Tasks
- .propel/context/tasks/us_119/task_001_backend_baseline_schema_migration_strategy_and_existing_state_reconciliation.md (TASK_001)

## Impacted Components
- Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs
- Server/ClinicalIntelligence.Api/Domain/Models/*
- Server/ClinicalIntelligence.Api/Migrations/*

## Implementation Plan
- Implement the 16 ERD entities as C# domain models (records/classes) with properties matching ERD column names and types.
- Update `ApplicationDbContext`:
  - Add `DbSet<>` for each of the 16 entities.
  - Configure table names, keys, required fields, and relationships using Fluent API.
  - Ensure PostgreSQL type mappings align to the ERD intent (uuid, varchar/text, timestamp with time zone, jsonb).
- Integrate with the migration strategy chosen in TASK_001:
  - If using a clean baseline strategy, regenerate the initial migration from the new model.
  - If using a non-destructive strategy, implement the migration as a transition (renames/drops/additions) rather than a replacement.
- Generate the baseline migration for the 16-table schema and ensure it compiles.
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs | Add DbSets + Fluent mappings for the 16 ERD core entities and their relationships |
| CREATE | Server/ClinicalIntelligence.Api/Domain/Models/User.cs | User account entity mapped to USER table with unique email constraint (constraint created in later task) |
| CREATE | Server/ClinicalIntelligence.Api/Domain/Models/Session.cs | Session tracking entity mapped to SESSION table |
| CREATE | Server/ClinicalIntelligence.Api/Domain/Models/PasswordResetToken.cs | Password reset token entity mapped to PASSWORD_RESET_TOKEN table |
| CREATE | Server/ClinicalIntelligence.Api/Domain/Models/DocumentBatch.cs | Document batch entity mapped to DOCUMENT_BATCH table |
| CREATE | Server/ClinicalIntelligence.Api/Domain/Models/Document.cs | Document entity mapped to DOCUMENT table |
| CREATE | Server/ClinicalIntelligence.Api/Domain/Models/ProcessingJob.cs | Processing job entity mapped to PROCESSING_JOB table |
| CREATE | Server/ClinicalIntelligence.Api/Domain/Models/DocumentChunk.cs | Document chunk entity mapped to DOCUMENT_CHUNK table (vector column configured in separate task) |
| CREATE | Server/ClinicalIntelligence.Api/Domain/Models/ExtractedEntity.cs | Extracted entity entity mapped to EXTRACTED_ENTITY table |
| CREATE | Server/ClinicalIntelligence.Api/Domain/Models/EntityCitation.cs | Entity citation entity mapped to ENTITY_CITATION table |
| CREATE | Server/ClinicalIntelligence.Api/Domain/Models/Conflict.cs | Conflict entity mapped to CONFLICT table |
| CREATE | Server/ClinicalIntelligence.Api/Domain/Models/ConflictResolution.cs | Conflict resolution entity mapped to CONFLICT_RESOLUTION table |
| CREATE | Server/ClinicalIntelligence.Api/Domain/Models/BillingCodeCatalogItem.cs | Billing code catalog item entity mapped to BILLING_CODE_CATALOG_ITEM table |
| CREATE | Server/ClinicalIntelligence.Api/Domain/Models/CodeSuggestion.cs | Code suggestion entity mapped to CODE_SUGGESTION table |
| CREATE | Server/ClinicalIntelligence.Api/Domain/Models/AuditLogEvent.cs | Audit log event entity mapped to AUDIT_LOG_EVENT table |
| CREATE | Server/ClinicalIntelligence.Api/Domain/Models/VectorQueryLog.cs | Vector query log entity mapped to VECTOR_QUERY_LOG table |
| MODIFY | Server/ClinicalIntelligence.Api/Migrations/* | Add or update the baseline migration (per TASK_001 strategy) to create the 16 ERD tables |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/ef/core/modeling/
- https://learn.microsoft.com/ef/core/managing-schemas/migrations/

## Build Commands
- dotnet build Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj

## Implementation Validation Strategy
- Verify the migration can be generated and compiled.
- Verify the generated migration contains create-table statements for all 16 ERD tables.

## Implementation Checklist
- [x] Add C# models for all 16 ERD entities with properties aligned to the ERD
- [x] Add `DbSet<>`s and Fluent mappings in `ApplicationDbContext` for all 16 tables
- [x] Configure required foreign keys and delete behaviors per ERD relationships
- [x] Generate the baseline migration using the strategy documented in TASK_001
- [x] Ensure the migration compiles and `dotnet build` succeeds
- [x] Confirm the migration contains all 16 table creates (naming and mapping consistent)
- [x] Confirm the column data types align to the ERD intent (uuid/varchar/text/timestamp/jsonb)
- [x] Confirm any reserved identifier concerns are handled consistently with the documented naming strategy
