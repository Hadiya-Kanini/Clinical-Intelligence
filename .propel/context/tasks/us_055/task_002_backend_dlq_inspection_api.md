# Task - TASK_055_002

## Requirement Reference
- User Story: us_055
- Story Location: .propel/context/tasks/us_055/us_055.md
- Acceptance Criteria: 
    - Given jobs exceed max retries, When moved to DLQ, Then they are available for inspection.
    - Given the DLQ, When jobs are present, Then they include original message, error details, and retry history.

## Task Overview
Expose operator-facing Backend API endpoints to inspect dead-lettered jobs stored in the database. This includes listing DLQ entries with pagination and retrieving a single DLQ entry with full details.

## Dependent Tasks
- [TASK_055_001 - Persist dead letter jobs]

## Impacted Components
- [MODIFY: Server/ClinicalIntelligence.Api/Program.cs]
- [CREATE: Server/ClinicalIntelligence.Api/Contracts/Dlq/DlqListResponse.cs]
- [CREATE: Server/ClinicalIntelligence.Api/Contracts/Dlq/DlqItemResponse.cs]
- [CREATE: Server/ClinicalIntelligence.Api/Services/Queue/IDeadLetterQueueReader.cs]
- [CREATE: Server/ClinicalIntelligence.Api/Services/Queue/DbDeadLetterQueueReader.cs]

## Implementation Plan
- Add a `IDeadLetterQueueReader` abstraction for querying DLQ entries without leaking EF query concerns into endpoint handlers.
- Implement `DbDeadLetterQueueReader` using `ApplicationDbContext` with:
  - Pagination (`page`, `pageSize`) and stable sorting (`DeadLetteredAt DESC`, `Id` tie-break)
  - Optional filters (at minimum `documentId`, `processingJobId`, date range)
  - Projection to response DTOs to avoid returning full EF entities
- Add versioned API endpoints in `Program.cs` under `v1` using existing minimal API style:
  - `GET /api/v1/admin/dlq` (list + filters + pagination)
  - `GET /api/v1/admin/dlq/{deadLetterJobId}` (single item)
- Secure endpoints with admin authorization (`AuthorizationPolicies.AdminOnly`) to ensure operator-only access.
- Ensure responses include:
  - Original message payload (or redacted/normalized version if needed)
  - Error message + error details
  - Retry history / retry count metadata
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Register DLQ reader service and map `GET /api/v1/admin/dlq` + `GET /api/v1/admin/dlq/{id}` endpoints with Admin-only authorization |
| CREATE | Server/ClinicalIntelligence.Api/Contracts/Dlq/DlqListResponse.cs | Response DTO(s) for paginated DLQ listing (items + paging metadata) |
| CREATE | Server/ClinicalIntelligence.Api/Contracts/Dlq/DlqItemResponse.cs | Response DTO for a single DLQ entry including original message, error details, and retry history |
| CREATE | Server/ClinicalIntelligence.Api/Services/Queue/IDeadLetterQueueReader.cs | Abstraction for DLQ query operations required by the inspection APIs |
| CREATE | Server/ClinicalIntelligence.Api/Services/Queue/DbDeadLetterQueueReader.cs | EF Core implementation for DLQ queries with pagination, filtering, and projections |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-8.0

## Build Commands
- dotnet build .\Server\ClinicalIntelligence.Api\ClinicalIntelligence.Api.csproj

## Implementation Validation Strategy
- Verify admin-only authorization blocks non-admin users.
- Verify `GET /api/v1/admin/dlq` supports pagination and remains stable with concurrent inserts.
- Verify `GET /api/v1/admin/dlq/{id}` returns complete details (original message, error details, retry history) or 404 for unknown IDs.

## Implementation Checklist
- [ ] Create DLQ response contracts for list + item views
- [ ] Implement `IDeadLetterQueueReader`
- [ ] Implement `DbDeadLetterQueueReader` with pagination + filtering
- [ ] Register reader in DI container
- [ ] Add `GET /api/v1/admin/dlq` endpoint with pagination + filters
- [ ] Add `GET /api/v1/admin/dlq/{id}` endpoint
- [ ] Require `AuthorizationPolicies.AdminOnly` for both endpoints
- [ ] Add guardrails for large DLQ (enforce max `pageSize` and return paging metadata)
