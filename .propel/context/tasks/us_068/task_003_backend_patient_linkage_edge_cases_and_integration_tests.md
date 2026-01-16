# Task - TASK_068_003

## Requirement Reference
- User Story: [us_068]
- Story Location: [.propel/context/tasks/us_068/us_068.md]
- Acceptance Criteria: 
    - [Given multiple documents, When patient matching runs, Then they are linked via MRN match (primary) (FR-050).]
    - [Given no MRN match, When patient matching runs, Then name+DOB matching is used as fallback.]
    - [Given matching rules, When applied, Then they handle variations in name formatting.]

## Task Overview
Add focused test coverage for patient linkage edge cases to ensure deterministic matching behavior and safe handling of conflicting identifiers.

This task emphasizes the US_068 edge cases:
- Conflicting identifiers across documents.
- Typos / name formatting differences.
- DOB format differences.

## Dependent Tasks
- [TASK_068_001] (Patient identity normalization and matching)
- [TASK_068_002] (Apply patient linkage to documents and related records)

## Impacted Components
- [MODIFY: Server/ClinicalIntelligence.Api.Tests/ClinicalIntelligence.Api.Tests.csproj]
- [CREATE: Server/ClinicalIntelligence.Api.Tests/PatientMatching/PatientIdentityEdgeCaseTests.cs]

## Implementation Plan
- Add a dedicated test suite `PatientIdentityEdgeCaseTests` that validates:
  - Conflicting identifier scenarios:
    - Two documents with the same name+DOB but different MRNs.
    - Expected behavior: link via MRN when MRN matches an existing patient; otherwise use name+DOB fallback and record an `ErdConflict` for MRN conflicts.
  - Name variation scenarios:
    - Differences in casing, extra whitespace, punctuation, ordering variants commonly seen in headers.
    - Expected behavior: normalization produces the same match key.
  - DOB format scenarios:
    - Multiple supported formats are parsed to the same `DateOnly`.
    - Invalid DOB strings do not throw and do not cause incorrect matches.
- Ensure tests are deterministic and do not rely on external services.
- If required by existing test infrastructure, add helper methods/fixtures to create in-memory patients/documents and execute matcher/linker operations.
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api.Tests/ClinicalIntelligence.Api.Tests.csproj | Ensure test project references required dependencies for running patient matching/linkage tests (if additional packages are required) |
| CREATE | Server/ClinicalIntelligence.Api.Tests/PatientMatching/PatientIdentityEdgeCaseTests.cs | Test coverage for MRN conflicts, name normalization variations, and DOB parsing variations |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/dotnet/core/testing/

## Build Commands
- dotnet test .\Server\ClinicalIntelligence.Api.Tests\ClinicalIntelligence.Api.Tests.csproj

## Implementation Validation Strategy
- Confirm edge-case tests cover MRN conflicts and do not allow silent overwrites.
- Confirm name normalization correctly links formatting variants.
- Confirm DOB parsing is robust and does not throw on invalid inputs.

## Implementation Checklist
- [ ] Add edge-case tests for MRN conflict scenarios and expected conflict persistence behavior
- [ ] Add edge-case tests for name formatting variations
- [ ] Add edge-case tests for DOB format variations and invalid values
- [ ] Ensure tests run reliably in CI/local without external dependencies
