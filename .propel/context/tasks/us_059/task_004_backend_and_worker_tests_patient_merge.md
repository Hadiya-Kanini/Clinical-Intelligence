# Task - TASK_059_004

## Requirement Reference
- User Story: us_059
- Story Location: .propel/context/tasks/us_059/us_059.md
- Acceptance Criteria: 
    - Given multiple documents for the same patient, When processed, Then text is merged before semantic chunking.
    - Given document merge, When performed, Then document boundaries and source metadata are preserved.
    - Given patient identification, When documents are linked, Then MRN or name+DOB matching is used.

## Task Overview
Add automated tests validating patient identity matching and document selection ordering in the Backend API, and validating worker merge behavior against the contract expectations.

## Dependent Tasks
- [TASK_059_002 - Worker: patient text merge]
- [TASK_059_003 - Backend: document grouping and identity match]

## Impacted Components
- [CREATE: Server/ClinicalIntelligence.Api.Tests/Processing/PatientIdentityMatcherTests.cs]
- [CREATE: Server/ClinicalIntelligence.Api.Tests/Processing/PatientDocumentMergePlannerTests.cs]
- [MODIFY: worker/tests/test_job_validation.py]

## Implementation Plan
- Add Backend API unit tests:
  - `PatientIdentityMatcherTests` covering MRN priority and name+DOB matching
  - `PatientDocumentMergePlannerTests` covering deterministic ordering and patient scoping
- Extend worker schema validation tests to include at least one valid patient-merge job payload fixture (new optional payload fields)
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api.Tests/Processing/PatientIdentityMatcherTests.cs | Unit tests for MRN-first and name+DOB patient identity matching behavior |
| CREATE | Server/ClinicalIntelligence.Api.Tests/Processing/PatientDocumentMergePlannerTests.cs | Unit tests for stable document ordering and patient scoping in merge planning |
| MODIFY | worker/tests/test_job_validation.py | Add/validate a patient-merge style job payload fixture covering new optional payload fields |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/dotnet/core/testing/

## Build Commands
- dotnet test .\Server\ClinicalIntelligence.Api.Tests\ClinicalIntelligence.Api.Tests.csproj
- python -m pytest worker/tests

## Implementation Validation Strategy
- Ensure tests are deterministic and do not rely on wall-clock time.
- Ensure synthetic/non-PHI test data is used for all patient identity examples.

## Implementation Checklist
- [ ] Add `PatientIdentityMatcherTests` for MRN and name+DOB scenarios
- [ ] Add `PatientDocumentMergePlannerTests` for document scoping and ordering
- [ ] Update worker job schema validation tests to cover patient-merge job payload shape
- [ ] Ensure all test fixtures are synthetic and contain no PHI
