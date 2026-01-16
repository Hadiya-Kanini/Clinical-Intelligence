# Task - TASK_055_001

## Requirement Reference
- User Story: us_055
- Story Location: .propel/context/tasks/us_055/us_055.md
- Acceptance Criteria: 
    - Given jobs exceed max retries, When moved to DLQ, Then they are available for inspection.
    - Given the DLQ, When jobs are present, Then they include original message, error details, and retry history.

## Task Overview
Implement a durable, queryable dead-letter persistence layer in the Backend API so failed processing jobs (after max retries) are captured with sufficient detail for operator inspection and later remediation.

## Dependent Tasks
- [US_053 - Queue documents in RabbitMQ for processing]
- [US_054 - Implement retry with exponential backoff]

## Impacted Components
- [CREATE: Server/ClinicalIntelligence.Api/Domain/Models/DeadLetterJob.cs]
- [MODIFY: Server/ClinicalIntelligence.Api/Domain/Models/ProcessingJob.cs]
- [MODIFY: Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs]
- [CREATE: Server/ClinicalIntelligence.Api/Services/Queue/IDeadLetterQueueWriter.cs]
- [CREATE: Server/ClinicalIntelligence.Api/Services/Queue/DbDeadLetterQueueWriter.cs]
- [CREATE: Server/ClinicalIntelligence.Api/Migrations/<new_migration>_AddDeadLetterJobs.cs]

## Implementation Plan
- Define a `DeadLetterJob` entity that stores:
  - The originating `ProcessingJobId` (and `DocumentId` for operator context)
  - `OriginalMessage` (JSON) and/or an explicit schema version
  - `ErrorMessage`, `ErrorDetails` (JSON), and a `RetryHistory` structure (JSON)
  - `DeadLetteredAt` (UTC) and `DeadLetterReason`
- Add EF Core configuration:
  - Table name `dead_letter_jobs`
  - Indexes supporting operator workflows (e.g., `DeadLetteredAt DESC`, `DocumentId`, `ProcessingJobId` unique)
- Update `ProcessingJob` status conventions so that a job can be clearly marked as dead-lettered (e.g., `Status = "DeadLettered"` or equivalent) without breaking existing status logic.
- Implement `IDeadLetterQueueWriter` + a DB-backed implementation (`DbDeadLetterQueueWriter`) responsible for atomically:
  - Writing a `DeadLetterJob` record
  - Updating the associated `ProcessingJob` status and final error fields
- Provide an explicit integration seam for US_054 retry exhaustion handling (the retry component should call `IDeadLetterQueueWriter` when max retries are exceeded).
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api/Domain/Models/DeadLetterJob.cs | New EF entity representing a dead-lettered processing job, including original message and retry history metadata |
| MODIFY | Server/ClinicalIntelligence.Api/Domain/Models/ProcessingJob.cs | Ensure statuses support a terminal dead-letter state; confirm fields needed to link to `DeadLetterJob` |
| MODIFY | Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs | Register `DbSet<DeadLetterJob>` and configure mappings + indexes for `dead_letter_jobs` |
| CREATE | Server/ClinicalIntelligence.Api/Services/Queue/IDeadLetterQueueWriter.cs | Abstraction for recording DLQ entries from the retry exhaustion path |
| CREATE | Server/ClinicalIntelligence.Api/Services/Queue/DbDeadLetterQueueWriter.cs | DB-backed writer that persists DLQ entries and updates `ProcessingJob` atomically |
| CREATE | Server/ClinicalIntelligence.Api/Migrations/<new_migration>_AddDeadLetterJobs.cs | EF migration creating `dead_letter_jobs` and relevant indexes/constraints |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://www.rabbitmq.com/dlx.html
- https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/

## Build Commands
- dotnet build .\Server\ClinicalIntelligence.Api\ClinicalIntelligence.Api.csproj

## Implementation Validation Strategy
- Validate EF migration creates `dead_letter_jobs` with expected indexes/constraints.
- Validate `DbDeadLetterQueueWriter` writes a DLQ entry and marks the associated `ProcessingJob` as dead-lettered within a single transaction.
- Validate `DeadLetterJob` can store and retrieve `OriginalMessage`, `ErrorDetails`, and `RetryHistory` as JSON without loss.

## Implementation Checklist
- [x] Create `DeadLetterJob` domain model with fields required for inspection (original message, error details, retry history)
- [x] Add `DbSet<DeadLetterJob>` and EF mapping for `dead_letter_jobs` (including indexes)
- [x] Add EF migration for DLQ persistence
- [x] Implement `IDeadLetterQueueWriter` abstraction
- [x] Implement `DbDeadLetterQueueWriter` with transactional write + `ProcessingJob` status update
- [x] Document the explicit call site contract for US_054 (invoke writer on retry exhaustion)
- [x] Verify no PHI is logged by default when persisting/serializing DLQ payloads
