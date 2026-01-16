# Task - TASK_056_001

## Requirement Reference
- User Story: us_056
- Story Location: .propel/context/tasks/us_056/us_056.md
- Acceptance Criteria: 
    - Given processing completes, When metadata is recorded, Then job ID, retry count, processing time, and timestamps are stored (FR-027).
    - Given processing fails, When error occurs, Then error message is captured and displayed to user (FR-028).

## Task Overview
Implement a backend persistence/write path for processing metadata (job timing and errors) so that the system can reliably store:
- Job ID
- Retry count
- Start time, completion time, and duration
- Error message/details on failure

This task focuses on creating a clean application boundary (interface + implementation) for recording job metadata, so that the queue worker / retry layer can call it without duplicating DB logic.

## Dependent Tasks
- [US_053 - Queue documents in RabbitMQ for processing]

## Impacted Components
- [CREATE | Server/ClinicalIntelligence.Api/Services/ProcessingJobs/IProcessingJobMetadataWriter.cs | Abstraction for recording job status transitions, timing, retry count, and errors]
- [CREATE | Server/ClinicalIntelligence.Api/Services/ProcessingJobs/DbProcessingJobMetadataWriter.cs | EF Core-backed implementation that updates `processing_jobs` fields consistently]
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Register the new writer in DI so it can be used by queue/worker integration]

## Implementation Plan
- Define an `IProcessingJobMetadataWriter` interface that supports the minimum required operations:
  - Mark job started (`Status`, `StartedAt`)
  - Mark job completed (`Status`, `CompletedAt`, compute `ProcessingTimeMs`)
  - Mark job failed (`Status`, `CompletedAt`, `ProcessingTimeMs`, `ErrorMessage`, `ErrorDetails`)
  - Update `RetryCount` in a single method (or as part of the above methods)
- Implement `DbProcessingJobMetadataWriter` using `ApplicationDbContext`:
  - Load the `ProcessingJob` by `Id`
  - Update only the relevant columns for each transition
  - Use UTC timestamps (`DateTime.UtcNow`) and compute `ProcessingTimeMs` defensively
  - Ensure error fields are safe to store (avoid leaking PHI in structured `ErrorDetails` by default)
- Establish status naming consistency:
  - Align with `Document.Status` values where possible (`Pending`, `Processing`, `Completed`, `Failed`) to avoid UI confusion
- Define the integration seam:
  - Document the call contract for the queue consumer / retry component (where to call Start/Complete/Fail)
  - The worker integration itself remains out of scope for this task, but must have a clear API to depend on

**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api/Services/ProcessingJobs/IProcessingJobMetadataWriter.cs | Define the contract for recording processing job metadata (timing, retries, error capture) |
| CREATE | Server/ClinicalIntelligence.Api/Services/ProcessingJobs/DbProcessingJobMetadataWriter.cs | Implement metadata persistence using EF Core updates to `processing_jobs` |
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Register the new writer in DI for later consumption by queue/worker integration |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/ef/core/

## Build Commands
- dotnet build .\Server\ClinicalIntelligence.Api\ClinicalIntelligence.Api.csproj

## Implementation Validation Strategy
- Validate that calling the writer methods updates `processing_jobs` with:
  - `StartedAt`, `CompletedAt`, and `ProcessingTimeMs` in expected combinations
  - `ErrorMessage` populated only for failed jobs
  - `RetryCount` updated correctly
- Validate that updates use UTC and do not throw when `StartedAt` is missing (compute duration safely)

## Implementation Checklist
- [ ] Create `IProcessingJobMetadataWriter` interface covering start/complete/fail + retry updates
- [ ] Implement `DbProcessingJobMetadataWriter` with consistent status/timestamp handling
- [ ] Register writer in DI (`Program.cs`)
- [ ] Add defensive handling for missing start times and very long error messages
- [ ] Ensure error persistence does not store sensitive data by default
