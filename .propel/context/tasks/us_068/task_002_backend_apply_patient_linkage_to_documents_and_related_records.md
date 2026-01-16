# Task - TASK_068_002

## Requirement Reference
- User Story: [us_068]
- Story Location: [.propel/context/tasks/us_068/us_068.md]
- Acceptance Criteria: 
    - [Given multiple documents, When patient matching runs, Then they are linked via MRN match (primary) (FR-050).]
    - [Given no MRN match, When patient matching runs, Then name+DOB matching is used as fallback.]
    - [Given patient linkage, When established, Then all documents contribute to the unified patient view.]

## Task Overview
Implement the backend linkage operation that applies the selected/matched `ErdPatient` to the persisted records for a document (and its batch) so multiple documents aggregate into a single patient scope.

This task focuses on updating `Document.PatientId` / `DocumentBatch.PatientId` and ensuring related, patient-scoped records remain consistent.

## Dependent Tasks
- [TASK_068_001] (Backend patient identity normalization and matching service)

## Impacted Components
- [CREATE: Server/ClinicalIntelligence.Api/Services/PatientMatching/IPatientLinkingService.cs]
- [CREATE: Server/ClinicalIntelligence.Api/Services/PatientMatching/PatientLinkingService.cs]
- [MODIFY: Server/ClinicalIntelligence.Api/Program.cs]
- [CREATE: Server/ClinicalIntelligence.Api.Tests/PatientMatching/PatientLinkingServiceTests.cs]

## Implementation Plan
- Create `IPatientLinkingService` responsible for atomically applying patient linkage.
- Implement `PatientLinkingService` using `ApplicationDbContext` and `IPatientMatcher`:
  - Input should include the target `documentId` and extracted identity values (MRN/name/DOB) required by `IPatientMatcher`.
  - In a transaction:
    - Load the `Document` (and its `DocumentBatchId` when present).
    - Determine the target patient by calling `IPatientMatcher`.
    - If the document is already linked to the target patient, perform no-op.
    - If the document is linked to a different patient, reassign `Document.PatientId` to the target.
    - If the document has a batch, update `DocumentBatch.PatientId` and ensure all documents in that batch point to the same `PatientId`.
  - Update patient-scoped records to remain consistent when reassignment occurs:
    - `ExtractedEntity.PatientId` for rows belonging to the document.
    - `CodeSuggestion.PatientId` for rows linked to entities in the document.
    - `ErdConflict.PatientId` for conflicts derived from the document (if conflicts are persisted for that document).
- Conflict behavior (edge case from US_068):
  - When extracted MRN conflicts with an existing patient match by name+DOB, do not overwrite stored MRN.
  - Persist an `ErdConflict` entry (field = "mrn") with `ConflictingValues` containing both the existing and extracted MRNs.
- Register `IPatientLinkingService` in DI (`Program.cs`) so it can be called by the processing orchestration step when “patient matching runs”.
- Add tests validating:
  - Linking two documents with same MRN results in same `PatientId`.
  - No MRN match but same normalized name+DOB results in same `PatientId`.
  - Reassignment updates `DocumentBatch.PatientId` and all documents in batch.
  - Related `ExtractedEntity` rows for the document are reassigned to the new `PatientId`.
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api/Services/PatientMatching/IPatientLinkingService.cs | Abstraction to apply patient linkage to documents/batches and keep related records consistent |
| CREATE | Server/ClinicalIntelligence.Api/Services/PatientMatching/PatientLinkingService.cs | Transactional implementation that uses `IPatientMatcher` and updates `Document`, `DocumentBatch`, and patient-scoped records |
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Register patient linking service in DI container |
| CREATE | Server/ClinicalIntelligence.Api.Tests/PatientMatching/PatientLinkingServiceTests.cs | Tests for document/batch reassignment behavior and related-record consistency |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/ef/core/saving/transactions

## Build Commands
- dotnet build .\Server\ClinicalIntelligence.Api\ClinicalIntelligence.Api.csproj
- dotnet test .\Server\ClinicalIntelligence.Api.Tests\ClinicalIntelligence.Api.Tests.csproj

## Implementation Validation Strategy
- Validate that multiple documents converge on a single `ErdPatient` when MRN matches.
- Validate that name+DOB fallback links documents when MRN does not match.
- Validate that re-linking preserves referential integrity by updating patient-scoped records in the same transaction.

## Implementation Checklist
- [ ] Define `IPatientLinkingService` entry point and inputs
- [ ] Implement transactional `PatientLinkingService` that:
  - [ ] Loads document + optional batch
  - [ ] Finds/creates target patient via `IPatientMatcher`
  - [ ] Reassigns `Document.PatientId` and batch/documents as needed
  - [ ] Reassigns patient-scoped records (`ExtractedEntity`, `CodeSuggestion`, `ErdConflict`) for the document
- [ ] Register service in DI (`Program.cs`)
- [ ] Add unit/integration tests covering MRN match, fallback match, batch consistency, and reassignment
