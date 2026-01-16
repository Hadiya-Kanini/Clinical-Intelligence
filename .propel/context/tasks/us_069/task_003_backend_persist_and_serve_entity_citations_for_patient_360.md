# Task - TASK_069_003

## Requirement Reference
- User Story: us_069 (extracted from input)
- Story Location: [.propel/context/tasks/us_069/us_069.md]
- Acceptance Criteria: 
    - Given an extracted entity, When validated, Then it must have valid source citations or be rejected.
    - Given citations, When stored, Then they include document_id, page, section, coordinates, and cited text (TR-008).
    - Given grounding enforcement, When applied, Then 100% of displayed entities have verifiable sources.

## Task Overview
Introduce a backend persistence and retrieval boundary for entity citations so that grounded provenance can be stored alongside extracted entities and served to the UI (Patient 360 and conflict/code review features). This task focuses on:
- Enabling and persisting `EntityCitation` rows
- Ensuring citations reference real document chunks and entity rows
- Providing backend DTOs/query paths that can supply verifiable sources to consumers

## Dependent Tasks
- [US_119 - Baseline Schema Migration - 16 Core Tables] (tables exist)
- [TASK_069_001] (Contract defines required grounding fields)
- [TASK_069_002] (Worker emits validated grounded entities)
- [US_066 TASK_003] (Backend persists extracted entities boundary)

## Impacted Components
- [MODIFY | Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs | Enable `DbSet<EntityCitation>` and configure EF mapping for `entity_citations` table]
- [MODIFY | Server/ClinicalIntelligence.Api/Domain/Models/ExtractedEntity.cs | Enable navigation collection to `EntityCitation` (remove temporary disabling) if applicable]
- [MODIFY | Server/ClinicalIntelligence.Api/Services/ExtractedEntities | Extend persistence boundary to accept citation payload and write entity + citations atomically]
- [CREATE | Server/ClinicalIntelligence.Api/Contracts/Entities/EntityCitationDto.cs | DTO for citation fields (document_id/page/section/coordinates/cited_text + document name if available)]
- [CREATE | Server/ClinicalIntelligence.Api/Services/Entities/EntityCitationReader.cs | Query helper to load citations for a patient/document/entity]
- [CREATE | Server/ClinicalIntelligence.Api.Tests/EntityCitationPersistenceTests.cs | Tests validating citation persistence and FK integrity]

## Implementation Plan
- Enable EF Core mapping for citations:
  - In `ApplicationDbContext`, uncomment/add:
    - `DbSet<EntityCitation> EntityCitations`
    - `ConfigureEntityCitation(modelBuilder)`
  - Ensure relationships:
    - `EntityCitation.ExtractedEntityId` FK -> `extracted_entities`
    - `EntityCitation.DocumentChunkId` FK -> `document_chunks` (note: if vector-related chunks are still temporarily disabled, coordinate enabling with vector DB work)
- Update `ExtractedEntity` model:
  - Re-enable `ICollection<EntityCitation> EntityCitations` navigation if appropriate.
- Persistence boundary update:
  - Extend the extracted entity persistence service so it can accept, for each extracted entity, one or more citations containing:
    - `DocumentChunkId` (preferred) OR a resolvable location reference (page/section/coordinates) that can be mapped to a chunk.
  - Persist entity row(s) and citation row(s) in one transaction.
  - Fail deterministically if citation references cannot be resolved to existing chunks.
- Retrieval boundary update:
  - Create DTO(s) that include:
    - `document_id`
    - `document_name` (from `documents.original_name`)
    - `page`, `section`, `coordinates`, `cited_text`
  - Implement reader/query method(s) to load citations for patient 360 use.
- Tests:
  - Persist entity + citation and validate:
    - Rows exist in both tables
    - FK relationships are valid
    - Citation fields round-trip correctly

**Focus on how to implement**

## Current Project State
- `EntityCitation` and `DocumentChunk` models exist, but `EntityCitations` are currently disabled in `ApplicationDbContext` and `ExtractedEntity` navigation.
- `document_chunks` and pgvector-related support appears temporarily disabled; citations depend on chunks.

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs | Enable `EntityCitations` DbSet + mapping |
| MODIFY | Server/ClinicalIntelligence.Api/Domain/Models/ExtractedEntity.cs | Re-enable citations navigation collection |
| MODIFY | Server/ClinicalIntelligence.Api/Services/ExtractedEntities/* | Persist citations alongside extracted entities in one transaction |
| CREATE | Server/ClinicalIntelligence.Api/Contracts/Entities/EntityCitationDto.cs | Stable DTO for serving/verifying citation details |
| CREATE | Server/ClinicalIntelligence.Api/Services/Entities/EntityCitationReader.cs | Query helper to retrieve citations by patient/entity |
| CREATE | Server/ClinicalIntelligence.Api.Tests/EntityCitationPersistenceTests.cs | Validate persistence and referential integrity |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/ef/core/modeling/relationships

## Build Commands
- dotnet build .\Server\ClinicalIntelligence.Api\ClinicalIntelligence.Api.csproj
- dotnet test .\Server\ClinicalIntelligence.Api.Tests\ClinicalIntelligence.Api.Tests.csproj

## Implementation Validation Strategy
- [Unit/DB] Persisted citations contain required fields and link to existing extracted entity + document chunk.
- [Unit/DB] Attempting to persist an entity citation with missing/invalid chunk reference fails deterministically.

## Implementation Checklist
- [ ] Enable `EntityCitations` DbSet and EF model configuration
- [ ] Re-enable `ExtractedEntity.EntityCitations` navigation
- [ ] Extend extracted entity writer to persist citations atomically
- [ ] Add DTOs and query helpers to retrieve citations for patient 360 usage
- [ ] Add tests for persistence and invalid-reference failure modes
