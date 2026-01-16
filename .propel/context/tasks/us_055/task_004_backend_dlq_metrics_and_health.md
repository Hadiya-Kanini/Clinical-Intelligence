# Task - TASK_055_004

## Requirement Reference
- User Story: us_055
- Story Location: .propel/context/tasks/us_055/us_055.md
- Acceptance Criteria: 
    - Given DLQ monitoring, When implemented, Then queue depth is exposed as a metric (NFR-011).

## Task Overview
Add lightweight DLQ monitoring signals to the Backend API so operators can observe DLQ depth and detect abnormal growth. Implement a dedicated health/diagnostics endpoint (and/or health check) backed by database counts.

## Dependent Tasks
- [TASK_055_001 - Persist dead letter jobs]

## Impacted Components
- [MODIFY: Server/ClinicalIntelligence.Api/Program.cs]
- [CREATE: Server/ClinicalIntelligence.Api/Health/DeadLetterQueueHealthCheck.cs]
- [CREATE: Server/ClinicalIntelligence.Api/Contracts/Dlq/DlqMetricsResponse.cs]

## Implementation Plan
- Implement a `DeadLetterQueueHealthCheck` that queries:
  - Total DLQ count
  - Oldest `DeadLetteredAt` age (if any)
- Provide configuration-driven thresholds (e.g., `Dlq:WarningThresholdCount`, `Dlq:CriticalThresholdCount`) with safe defaults for Development.
- Register the health check via `builder.Services.AddHealthChecks()` with a dedicated name/tag.
- Add an endpoint that returns DLQ metrics in JSON, mirroring existing operational endpoints:
  - `GET /health/dlq` (public health-style response)
  - Optionally also expose `GET /api/v1/admin/dlq/metrics` secured for operator use
- Ensure the endpoint is fast and safe for large DLQs (use indexed count queries; avoid loading rows).
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Register DLQ health check and add `GET /health/dlq` endpoint returning DLQ depth metrics |
| CREATE | Server/ClinicalIntelligence.Api/Health/DeadLetterQueueHealthCheck.cs | Health check that queries DLQ count/age and reports Healthy/Degraded/Unhealthy based on thresholds |
| CREATE | Server/ClinicalIntelligence.Api/Contracts/Dlq/DlqMetricsResponse.cs | Response DTO for DLQ depth metrics (count, oldest age, timestamp) |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-8.0

## Build Commands
- dotnet build .\Server\ClinicalIntelligence.Api\ClinicalIntelligence.Api.csproj

## Implementation Validation Strategy
- Verify DLQ metrics endpoint returns correct counts for empty and non-empty DLQ.
- Verify health check transitions to Degraded/Unhealthy when thresholds are exceeded.
- Verify endpoint latency remains stable for large DLQs.

## Implementation Checklist
- [ ] Create `DlqMetricsResponse` contract
- [ ] Implement `DeadLetterQueueHealthCheck` using efficient count/min queries
- [ ] Register DLQ health check in `Program.cs`
- [ ] Add `GET /health/dlq` endpoint returning DLQ depth metrics
- [ ] Add configuration options for warning/critical thresholds
- [ ] Ensure queries use indexed columns and avoid row materialization
- [ ] Ensure responses do not leak PHI
