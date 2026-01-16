# Task - TASK_068_001

## Requirement Reference
- User Story: [us_068]
- Story Location: [.propel/context/tasks/us_068/us_068.md]
- Acceptance Criteria: 
    - [Given multiple documents, When patient matching runs, Then they are linked via MRN match (primary) (FR-050).]
    - [Given no MRN match, When patient matching runs, Then name+DOB matching is used as fallback.]
    - [Given matching rules, When applied, Then they handle variations in name formatting.]

## Task Overview
Implement backend-side patient identity normalization and deterministic matching logic to select (or create) an `ErdPatient` for a document based on extracted demographics.

This task defines the matching rules for US_068:
- MRN is the primary match key.
- If MRN does not match any existing patient, use normalized name + DOB as a fallback key.
- Normalize name formatting (case/whitespace/punctuation) and parse DOB formats to reduce mismatches.

## Dependent Tasks
- [US_066] (Extract Patient Demographics including MRN, Name, DOB)

## Impacted Components
- [MODIFY: Server/ClinicalIntelligence.Api/Program.cs]
- [CREATE: Server/ClinicalIntelligence.Api/Services/PatientMatching/IPatientMatcher.cs]
- [CREATE: Server/ClinicalIntelligence.Api/Services/PatientMatching/PatientMatcher.cs]
- [CREATE: Server/ClinicalIntelligence.Api/Services/PatientMatching/PatientIdentityNormalizer.cs]
- [CREATE: Server/ClinicalIntelligence.Api.Tests/PatientMatching/PatientMatcherTests.cs]

## Implementation Plan
- Define a small matching input model (internal to the service layer) representing the extracted identity values:
  - MRN (string?)
  - Name (string?)
  - DOB (string?) from extraction; parse to `DateOnly?`
- Implement `PatientIdentityNormalizer` utilities:
  - MRN normalization:
    - Trim, remove common separators (spaces, dashes), case-insensitive comparison via `ToUpperInvariant()`
  - Name normalization:
    - Trim, collapse consecutive whitespace, remove punctuation (e.g., commas, periods), case-insensitive comparison via `ToUpperInvariant()`
    - Ensure normalization is deterministic for variations like "DOE, JANE" vs "Jane   Doe"
  - DOB parsing:
    - Parse common formats (`yyyy-MM-dd`, `MM/dd/yyyy`, `dd-MM-yyyy`) into `DateOnly`.
    - If DOB cannot be parsed, treat as missing for fallback matching.
- Implement `IPatientMatcher` with a single entry point (e.g., `FindOrCreatePatientAsync(...)`) that:
  - Attempts MRN match (primary):
    - If normalized MRN is present, query `ApplicationDbContext.ErdPatients` by MRN.
  - If no MRN match, attempts name + DOB match (fallback):
    - Require both normalized name and parsed DOB for fallback matching.
    - Query by DOB + case-insensitive name match.
  - If no patient matched, create a new `ErdPatient`:
    - Prefer using extracted MRN if available.
    - If MRN is missing, generate a synthetic MRN value (e.g., `AUTO-{Guid.NewGuid():N}`) so the record satisfies required DB constraints.
    - Persist `Name` and `Dob` when available.
- Ensure the matcher does not silently overwrite existing identity fields on matched patients.
- Register the matcher in DI in `Program.cs`.
- Add unit tests validating:
  - MRN primary match wins even when name formatting differs.
  - Fallback match works with name formatting variations (case/whitespace/punctuation).
  - DOB parsing accepts multiple formats.
  - When MRN is missing, patient creation uses synthetic MRN.
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Register patient matcher services in DI container for later use by processing orchestration |
| CREATE | Server/ClinicalIntelligence.Api/Services/PatientMatching/IPatientMatcher.cs | Abstraction for deterministic patient matching based on MRN and fallback name+DOB |
| CREATE | Server/ClinicalIntelligence.Api/Services/PatientMatching/PatientIdentityNormalizer.cs | Centralized normalization + DOB parsing utilities for patient identifiers |
| CREATE | Server/ClinicalIntelligence.Api/Services/PatientMatching/PatientMatcher.cs | EF Core implementation of matching + create-new behavior aligned to FR-050 |
| CREATE | Server/ClinicalIntelligence.Api.Tests/PatientMatching/PatientMatcherTests.cs | Unit tests for MRN-first logic, fallback matching, normalization, and DOB parsing |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/dotnet/api/system.globalization.stringnormalizeextensions
- https://learn.microsoft.com/en-us/dotnet/api/system.dateonly

## Build Commands
- dotnet build .\Server\ClinicalIntelligence.Api\ClinicalIntelligence.Api.csproj
- dotnet test .\Server\ClinicalIntelligence.Api.Tests\ClinicalIntelligence.Api.Tests.csproj

## Implementation Validation Strategy
- Verify MRN-first matching links documents consistently when multiple documents share MRN.
- Verify fallback name+DOB matching links documents when MRN does not match, including name formatting variations.
- Verify DOB parsing supports multiple formats and does not throw on invalid values.

## Implementation Checklist
- [ ] Create `PatientIdentityNormalizer` with MRN normalization, name normalization, and DOB parsing
- [ ] Define `IPatientMatcher` abstraction
- [ ] Implement `PatientMatcher` with MRN-first, name+DOB fallback, and create-new behavior (including synthetic MRN when missing)
- [ ] Register matcher in DI (`Program.cs`)
- [ ] Add unit tests for normalization, DOB parsing, MRN match, fallback match, and creation behavior
