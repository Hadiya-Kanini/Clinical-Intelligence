# Task - TASK_055_003

## Requirement Reference
- User Story: us_055
- Story Location: .propel/context/tasks/us_055/us_055.md
- Acceptance Criteria: 
    - Given failed jobs, When in DLQ, Then operators can replay or discard them.

## Task Overview
Implement operator actions for DLQ entries: replay (re-enqueue) and discard. Ensure these actions are auditable, idempotent, and safe under concurrent operator activity.

## Dependent Tasks
- [TASK_055_001 - Persist dead letter jobs]
- [TASK_055_002 - DLQ inspection API]
- [US_053 - Queue documents in RabbitMQ for processing]

## Impacted Components
- [MODIFY: Server/ClinicalIntelligence.Api/Program.cs]
- [CREATE: Server/ClinicalIntelligence.Api/Contracts/Dlq/DlqReplayResponse.cs]
- [CREATE: Server/ClinicalIntelligence.Api/Services/Queue/IDeadLetterQueueActions.cs]
- [CREATE: Server/ClinicalIntelligence.Api/Services/Queue/DeadLetterQueueActions.cs]
- [MODIFY: Server/ClinicalIntelligence.Api/Domain/Models/DeadLetterJob.cs]

## Implementation Plan
- Extend `DeadLetterJob` to track operator actions:
  - `Status` (e.g., `Pending`, `Replayed`, `Discarded`)
  - `LastActionAt`, `LastActionByUserId` (if available)
  - `ReplayAttempts` and `LastReplayError` (for the edge case “replay fails again”)
- Create `IDeadLetterQueueActions` to encapsulate the replay/discard workflow.
- Implement `DeadLetterQueueActions` to:
  - Load DLQ entry with concurrency protection (optimistic concurrency token or transaction + status check)
  - For replay:
    - Validate DLQ entry is eligible (not discarded, not already replayed)
    - Use the existing job contract payload (from `OriginalMessage`) as the source of truth
    - Enqueue a new job using the system’s enqueue abstraction from US_053 (or create an internal adapter if the enqueue module is introduced there)
    - Mark DLQ entry as `Replayed` and update audit fields
  - For discard:
    - Mark DLQ entry as `Discarded` and update audit fields
    - Ensure discard is idempotent (discarding an already discarded entry is a no-op)
- Add versioned admin endpoints in `Program.cs`:
  - `POST /api/v1/admin/dlq/{deadLetterJobId}/replay`
  - `DELETE /api/v1/admin/dlq/{deadLetterJobId}` (discard)
- Emit audit log events via the existing `IAuditLogWriter` for replay/discard actions (no PHI).
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Add admin endpoints to replay/discard DLQ entries; register `IDeadLetterQueueActions` |
| CREATE | Server/ClinicalIntelligence.Api/Contracts/Dlq/DlqReplayResponse.cs | Response DTO for replay result (e.g., new job id, status, replay attempt count) |
| CREATE | Server/ClinicalIntelligence.Api/Services/Queue/IDeadLetterQueueActions.cs | Abstraction for DLQ replay/discard operations |
| CREATE | Server/ClinicalIntelligence.Api/Services/Queue/DeadLetterQueueActions.cs | Implementation that updates DLQ entry status and re-enqueues jobs via existing enqueue module |
| MODIFY | Server/ClinicalIntelligence.Api/Domain/Models/DeadLetterJob.cs | Add fields to track replay/discard state and replay attempt outcomes |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://www.rabbitmq.com/reliability.html

## Build Commands
- dotnet build .\Server\ClinicalIntelligence.Api\ClinicalIntelligence.Api.csproj

## Implementation Validation Strategy
- Verify replay is idempotent (replaying an already replayed entry does not enqueue duplicates).
- Verify discard is idempotent.
- Verify concurrent operator actions do not corrupt status.
- Verify audit events are created for replay/discard.

## Implementation Checklist
- [x] Extend `DeadLetterJob` with status + action tracking fields
- [x] Create `IDeadLetterQueueActions` abstraction
- [x] Implement `DeadLetterQueueActions` with concurrency protection and idempotency
- [x] Add `POST /api/v1/admin/dlq/{id}/replay` endpoint
- [x] Add `DELETE /api/v1/admin/dlq/{id}` discard endpoint
- [x] Require `AuthorizationPolicies.AdminOnly` on both endpoints
- [x] Emit audit log events for replay/discard actions (no PHI)
- [x] Add failure handling for replay attempts (store last error + attempt count)
