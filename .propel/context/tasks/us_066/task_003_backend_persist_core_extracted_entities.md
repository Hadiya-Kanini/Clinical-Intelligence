# Task - TASK_066_003

## Requirement Reference
- User Story: us_066
- Story Location: .propel/context/tasks/us_066/us_066.md
- Acceptance Criteria: 
    - Given a document, When processed, Then demographics, allergies, meds, diagnoses, procedures, labs, vitals, social history, notes, and metadata are extracted.
- Edge Cases:
    - What happens when a category has no data in the document?

## Task Overview
Introduce a backend persistence boundary for storing extracted entities produced by the AI worker into the `extracted_entities` table.

This task focuses on:
- Creating a stable backend interface for writing extracted entities (dependency inversion)
- Mapping worker contract fields (`entity_group_name`, `entity_name`, `entity_value`) into the backend `ExtractedEntity` model (`Category`, `Name`, `Value`)
- Ensuring the “missing category => no rows written” behavior is preserved end-to-end

This task does not implement entity citations persistence (the `EntityCitation` relationship is currently disabled) and does not implement the worker extraction itself.

## Dependent Tasks
- [US_119 - Baseline Schema Migration - 16 Core Tables] (ensures `extracted_entities` table exists)
- [US_064 TASK_003] (Worker: parse + validate entity extraction output)
- [US_065 TASK_002] (Worker: validation failure semantics)
- [US_053 - Queue documents in RabbitMQ for processing] (integration point; if applicable)

## Impacted Components
- [CREATE | Server/ClinicalIntelligence.Api/Services/ExtractedEntities/IExtractedEntityWriter.cs | Backend contract for persisting extracted entities for a given document/patient]
- [CREATE | Server/ClinicalIntelligence.Api/Services/ExtractedEntities/DbExtractedEntityWriter.cs | EF Core implementation writing rows to `extracted_entities`]
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Register extracted entity writer in DI]
- [CREATE | Server/ClinicalIntelligence.Api.Tests/ExtractedEntityWriterTests.cs | Persistence tests validating mapping and missing-category behavior]

## Implementation Plan
- Define a backend persistence contract that is independent of worker internals:
  - `IExtractedEntityWriter` accepting:
    - `patientId`
    - `documentId`
    - list of extracted entities with `entity_group_name`, `entity_name`, `entity_value` (DTO internal to Server)
  - The interface should not depend on queue, LLM, or raw document text.
- Implement `DbExtractedEntityWriter` using `ApplicationDbContext`:
  - For each extracted entity, create an `ExtractedEntity` row:
    - `PatientId` = provided patientId
    - `DocumentId` = provided documentId
    - `Category` = entity.entity_group_name
    - `Name` = entity.entity_name
    - `Value` = entity.entity_value
  - Do not write any rows for missing categories (since the worker output should omit them).
  - Ensure values are truncated/validated to fit DB column constraints (max lengths already enforced via EF model).
- Register in DI (`Program.cs`).
- Add tests:
  - Writes correct rows for one document and patient
  - Category/name/value mapping is correct
  - Empty entity list writes zero rows

**Focus on how to implement**

## Current Project State
- Server/ClinicalIntelligence.Api/Domain/Models/ExtractedEntity.cs
- Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs (DbSet<ExtractedEntity>)

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api/Services/ExtractedEntities/IExtractedEntityWriter.cs | Define storage boundary for persisting extracted entities into `extracted_entities` |
| CREATE | Server/ClinicalIntelligence.Api/Services/ExtractedEntities/DbExtractedEntityWriter.cs | EF Core implementation to create `ExtractedEntity` rows per worker entity |
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Register writer in DI container |
| CREATE | Server/ClinicalIntelligence.Api.Tests/ExtractedEntityWriterTests.cs | Validate persistence behavior and mapping correctness |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/ef/core/

## Build Commands
- dotnet build .\Server\ClinicalIntelligence.Api\ClinicalIntelligence.Api.csproj
- dotnet test .\Server\ClinicalIntelligence.Api.Tests\ClinicalIntelligence.Api.Tests.csproj

## Implementation Validation Strategy
- [Unit/DB] Persisted rows match input entities for `Category`, `Name`, `Value`.
- [Unit/DB] Empty `extracted_entities` results in zero inserted rows.

## Implementation Checklist
- [ ] Define `IExtractedEntityWriter` interface and minimal DTO for extracted entities
- [ ] Implement `DbExtractedEntityWriter` using `ApplicationDbContext`
- [ ] Register writer in DI
- [ ] Add tests validating mapping and empty-list behavior
- [ ] Confirm DB constraints (max lengths) are respected; fail deterministically on overflow
