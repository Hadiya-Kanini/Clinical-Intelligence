# Task - TASK_071_003

## Requirement Reference
- User Story: us_071
- Story Location: .propel/context/tasks/us_071/us_071.md
- Acceptance Criteria: 
    - Given an entity, When I hover over it, Then extraction rationale is displayed in a tooltip (FR-055).
    - Given the rationale, When displayed, Then it explains why this value was extracted from the source.
    - Given the tooltip, When shown, Then it includes the cited source text.

## Task Overview
Persist extraction rationale for extracted entities in the backend and ensure it is served to the Patient 360 consumer contract so the frontend can render rationale immediately (no additional calls on hover).

This task focuses on the data layer + API contract surface needed for rationale display, not on UI behavior.

## Dependent Tasks
- [US_069 TASK_004] (Backend Patient 360 API includes grounded entities + citations)

## Impacted Components
- [MODIFY | Server/ClinicalIntelligence.Api/Domain/Models/ExtractedEntity.cs | Add persisted `Rationale` field]
- [MODIFY | Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs | Map `Rationale` column for `extracted_entities` and configure length/type]
- [CREATE | Server/ClinicalIntelligence.Api/Migrations/*_AddRationaleToExtractedEntities.cs | EF Core migration adding `rationale` column]
- [MODIFY | Server/ClinicalIntelligence.Api/Contracts/Patients/Patient360Response.cs | Include `rationale` per returned entity (created by US_069 TASK_004)]

## Implementation Plan
- Data model:
  - Extend `ExtractedEntity` domain model with a nullable `Rationale` string.
  - Configure EF mapping:
    - Add a reasonable max length (or use `text` column type) to support long rationale.
- Database migration:
  - Create a migration to add `rationale` column to `extracted_entities`.
  - Ensure migration is backwards compatible (nullable field).
- API surface:
  - Extend the Patient 360 response DTO to include the rationale field per entity.
  - Ensure `sourceText` (cited text) continues to be served via the citation payload (US_069 grounding/citation path), not duplicated in the entity record.
- Edge cases:
  - If rationale is missing/null, the API should return null/empty and the UI will apply a fallback message.

**Focus on how to implement**

## Current Project State
- `ExtractedEntity` model exists but does not currently include a `Rationale` column.
- `entity.schema.json` already defines `rationale` as an optional field in the worker extraction payload.
- Patient 360 API contract/reader is introduced by US_069 tasks (not currently present in codebase).

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Domain/Models/ExtractedEntity.cs | Add nullable `Rationale` field to persist extraction explanation |
| MODIFY | Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs | Update EF mapping for `extracted_entities` to include `Rationale` |
| CREATE | Server/ClinicalIntelligence.Api/Migrations/*_AddRationaleToExtractedEntities.cs | Add new DB column for rationale (nullable) |
| MODIFY | Server/ClinicalIntelligence.Api/Contracts/Patients/Patient360Response.cs | Include `rationale` per entity in API response |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/

## Build Commands
- dotnet build .\Server\ClinicalIntelligence.Api\ClinicalIntelligence.Api.csproj
- dotnet test .\Server\ClinicalIntelligence.Api.Tests\ClinicalIntelligence.Api.Tests.csproj

## Implementation Validation Strategy
- [DB] Apply migration and verify `extracted_entities` contains nullable `rationale` column.
- [API/Unit] Seed an extracted entity with `Rationale` and verify Patient 360 response includes it.
- [API/Unit] Seed an extracted entity without `Rationale` and verify response returns null/empty without errors.

## Implementation Checklist
- [ ] Add `Rationale` property to `ExtractedEntity` domain model
- [ ] Update EF mapping for `extracted_entities` to persist rationale
- [ ] Create and validate EF migration adding nullable `rationale` column
- [ ] Extend Patient 360 response DTO to include rationale per entity
- [ ] Add/extend tests validating rationale round-trip and null handling
