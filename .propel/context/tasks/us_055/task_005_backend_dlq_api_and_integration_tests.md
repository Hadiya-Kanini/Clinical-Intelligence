# Task - TASK_055_005

## Requirement Reference
- User Story: us_055
- Story Location: .propel/context/tasks/us_055/us_055.md
- Acceptance Criteria: 
    - Given jobs exceed max retries, When moved to DLQ, Then they are available for inspection.
    - Given DLQ monitoring, When implemented, Then queue depth is exposed as a metric (NFR-011).
    - Given failed jobs, When in DLQ, Then operators can replay or discard them.

## Task Overview
Add automated tests to validate DLQ persistence, inspection endpoints, replay/discard behavior, and DLQ metrics/health endpoints. Tests should cover authorization, pagination/filters, and idempotency under repeated calls.

## Dependent Tasks
- [TASK_055_001 - Persist dead letter jobs]
- [TASK_055_002 - DLQ inspection API]
- [TASK_055_003 - DLQ replay and discard]
- [TASK_055_004 - DLQ metrics and health]

## Impacted Components
- [CREATE: Server/ClinicalIntelligence.Api.Tests/Dlq/DlqEndpointsTests.cs]
- [MODIFY: Server/ClinicalIntelligence.Api.Tests/TestWebApplicationFactory.cs]

## Implementation Plan
- Extend the existing test host factory (`TestWebApplicationFactory`) to support:
  - Seeding an admin user and obtaining an authenticated context/cookie
  - Seeding `ProcessingJob` and `DeadLetterJob` records in the test database
- Add endpoint-level tests for:
  - `GET /api/v1/admin/dlq` returns paginated results, stable ordering, and enforces max `pageSize`
  - `GET /api/v1/admin/dlq/{id}` returns 200 for known id, 404 for unknown id
  - `POST /api/v1/admin/dlq/{id}/replay` requires admin auth and is idempotent
  - `DELETE /api/v1/admin/dlq/{id}` marks as discarded and is idempotent
  - `GET /health/dlq` returns count/age metrics and does not require admin auth (if designed as public health endpoint)
- Add negative tests:
  - Non-admin users receive 403/401 for admin endpoints
  - Replay attempts on discarded jobs fail with a consistent, non-sensitive error
- Keep test data non-PHI and synthetic.
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api.Tests/Dlq/DlqEndpointsTests.cs | Integration tests covering DLQ list/detail, replay/discard actions, and DLQ metrics endpoint behavior |
| MODIFY | Server/ClinicalIntelligence.Api.Tests/TestWebApplicationFactory.cs | Add helpers/seeding to support authenticated admin testing and DLQ entity setup |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-8.0

## Build Commands
- dotnet test .\Server\ClinicalIntelligence.Api.Tests\ClinicalIntelligence.Api.Tests.csproj

## Implementation Validation Strategy
- Ensure tests pass deterministically (no reliance on wall-clock timing except controlled timestamps).
- Ensure replay/discard idempotency is asserted with repeated calls.
- Ensure authorization behavior matches Admin-only policy requirements.

## Implementation Checklist
- [ ] Update `TestWebApplicationFactory` to seed admin + provide authenticated requests
- [ ] Add DLQ list endpoint tests (pagination, ordering, max page size)
- [ ] Add DLQ item endpoint tests (200/404)
- [ ] Add replay endpoint tests (admin-only + idempotency)
- [ ] Add discard endpoint tests (admin-only + idempotency)
- [ ] Add DLQ metrics/health endpoint tests
- [ ] Add non-admin authorization negative tests
- [ ] Ensure all test payloads are synthetic and non-PHI
