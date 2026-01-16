# Task - [TASK_003]

## Requirement Reference
- User Story: [us_065]
- Story Location: [.propel/context/tasks/us_065/us_065.md]
- Acceptance Criteria: 
    - [Given validation failure, When detected, Then the job is marked Failed with validation errors persisted (TR-007).]
    - [Given valid entities, When stored, Then they are ready for UI rendering and export.]

## Task Overview
Add backend-side support for persisting AI-worker validation failures into the database-backed `ProcessingJob` record so operators and the UI can understand why processing failed.

This task establishes the minimal backend primitives needed to:
- Store a short `ErrorMessage`
- Store structured error details as JSON in `ErrorDetails` (`jsonb`)
- Mark the `ProcessingJob.Status` as a deterministic failure status when entity validation fails

## Dependent Tasks
- [US_065 TASK_002] (Worker: produces normalized validation error details)

## Impacted Components
- [MODIFY | Server/ClinicalIntelligence.Api/Domain/Models/ProcessingJob.cs | Align status semantics to include a validation-failure status used by processing flow]
- [CREATE | Server/ClinicalIntelligence.Api/Services/ProcessingJobs/ProcessingJobFailureRecorder.cs | Centralize persistence of job failure status + errors]
- [CREATE | Server/ClinicalIntelligence.Api/Services/ProcessingJobs/IProcessingJobFailureRecorder.cs | Abstraction for recording failures (DIP-friendly)]
- [CREATE | Server/ClinicalIntelligence.Api.Tests/Services/ProcessingJobFailureRecorderTests.cs | Unit tests for recording error details + status]

## Implementation Plan
- Define deterministic status semantics:
  - Introduce a single status string for validation failures (e.g., `"Validation_Failed"`) used consistently within the API domain.
  - Map/translate from worker contract `validation_failed` if/when worker output is ingested (future integration point).
- Implement a small service that records failures:
  - `RecordValidationFailureAsync(processingJobId, errorMessage, errorDetailsJson, cancellationToken)`
  - Updates `ProcessingJob.Status`
  - Sets `ProcessingJob.ErrorMessage` and `ProcessingJob.ErrorDetails`
  - Sets `CompletedAt` (if appropriate for your lifecycle)
- Register service in DI so future orchestration endpoints/pipelines can call it.
- Add unit tests using an in-memory or test EF Core context pattern used in this repo.

**Focus on how to implement**

## Current Project State
- Server/ClinicalIntelligence.Api/Domain/Models/ProcessingJob.cs (has `Status`, `ErrorMessage`, `ErrorDetails`)
- Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs (maps `ErrorDetails` to `jsonb`)

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Domain/Models/ProcessingJob.cs | Add/standardize a validation-failure status value used by API processing flow |
| CREATE | Server/ClinicalIntelligence.Api/Services/ProcessingJobs/IProcessingJobFailureRecorder.cs | Interface for persisting job failure status + errors |
| CREATE | Server/ClinicalIntelligence.Api/Services/ProcessingJobs/ProcessingJobFailureRecorder.cs | EF Core implementation to update `ProcessingJob` with failure state + errors |
| CREATE | Server/ClinicalIntelligence.Api.Tests/Services/ProcessingJobFailureRecorderTests.cs | Verify status + error fields are persisted deterministically |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/ef/core/

## Build Commands
- dotnet test Server/ClinicalIntelligence.Api.Tests

## Implementation Validation Strategy
- [Unit] When `RecordValidationFailureAsync` is called, the job status is set to validation-failure status.
- [Unit] `ErrorMessage` is persisted and non-empty.
- [Unit] `ErrorDetails` is persisted as JSON (string) and can round-trip.

## Implementation Checklist
- [ ] Define/standardize the validation failure status string used by backend jobs
- [ ] Implement job failure recorder service that persists `Status`, `ErrorMessage`, `ErrorDetails`
- [ ] Register the service in DI container
- [ ] Add unit tests covering success and missing-job error scenarios
