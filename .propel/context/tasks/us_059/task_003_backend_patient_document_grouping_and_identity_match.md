# Task - TASK_059_003

## Requirement Reference
- User Story: us_059
- Story Location: .propel/context/tasks/us_059/us_059.md
- Acceptance Criteria: 
    - Given multiple documents for the same patient, When processed, Then text is merged before semantic chunking (FR-032).
    - Given patient identification, When documents are linked, Then MRN or name+DOB matching is used (FR-050).

## Task Overview
Implement Backend API services to (1) deterministically select the set of documents belonging to a patient for merge-before-chunk processing, and (2) provide a reusable patient identity matching utility (MRN first, else name+DOB) for document-to-patient linking workflows.

## Dependent Tasks
- [US_053 - Queue documents in RabbitMQ for processing]
- [TASK_059_001 - Contracts: patient merge job contract]

## Impacted Components
- [CREATE: Server/ClinicalIntelligence.Api/Services/Processing/IPatientIdentityMatcher.cs]
- [CREATE: Server/ClinicalIntelligence.Api/Services/Processing/PatientIdentityMatcher.cs]
- [CREATE: Server/ClinicalIntelligence.Api/Services/Processing/IPatientDocumentMergePlanner.cs]
- [CREATE: Server/ClinicalIntelligence.Api/Services/Processing/PatientDocumentMergePlanner.cs]
- [MODIFY: Server/ClinicalIntelligence.Api/Program.cs]

## Implementation Plan
- Implement `IPatientIdentityMatcher` + `PatientIdentityMatcher`:
  - Match by MRN when available
  - Otherwise match by normalized name + DOB
  - Return a result that allows caller to detect ambiguous matches or conflicting identifiers
- Implement `IPatientDocumentMergePlanner` + `PatientDocumentMergePlanner`:
  - Query `ApplicationDbContext.Documents` for documents belonging to a patient
  - Apply a stable ordering suitable for merging and downstream chunking (e.g., `UploadedAt ASC, Id ASC`)
  - Produce a job payload object that maps to the job contract’s patient-merge shape (e.g., `payload.patient_id` + `payload.document_ids`)
- Register services in DI in `Program.cs` so US_053 enqueue logic (or future orchestration endpoints) can depend on abstractions rather than concretes.
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api/Services/Processing/IPatientIdentityMatcher.cs | Abstraction for patient identity matching (MRN or name+DOB) used by document linking workflows |
| CREATE | Server/ClinicalIntelligence.Api/Services/Processing/PatientIdentityMatcher.cs | Implementation providing normalization rules and conflict/ambiguity handling |
| CREATE | Server/ClinicalIntelligence.Api/Services/Processing/IPatientDocumentMergePlanner.cs | Abstraction for selecting and ordering documents for patient-level merge processing |
| CREATE | Server/ClinicalIntelligence.Api/Services/Processing/PatientDocumentMergePlanner.cs | EF-backed implementation that returns document IDs in stable order and produces a contract-aligned merge job payload |
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Register merge planner and identity matcher in DI container |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/ef/core/

## Build Commands
- dotnet build .\Server\ClinicalIntelligence.Api\ClinicalIntelligence.Api.csproj

## Implementation Validation Strategy
- Validate identity matcher:
  - MRN match has priority over name+DOB
  - Name normalization is stable and culture-invariant
  - Conflicting identifiers are surfaced explicitly (no silent merges)
- Validate merge planner:
  - Document selection is restricted to a single patient
  - Ordering is deterministic

## Implementation Checklist
- [ ] Create `IPatientIdentityMatcher` abstraction
- [ ] Implement `PatientIdentityMatcher` with MRN and name+DOB matching and ambiguity handling
- [ ] Create `IPatientDocumentMergePlanner` abstraction
- [ ] Implement `PatientDocumentMergePlanner` querying documents and producing stable ordered document IDs
- [ ] Register both services in `Program.cs`
- [ ] Ensure no PHI is written to logs during matching/selection failures
