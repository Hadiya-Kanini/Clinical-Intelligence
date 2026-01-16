# Task - TASK_056_002

## Requirement Reference
- User Story: us_056
- Story Location: .propel/context/tasks/us_056/us_056.md
- Acceptance Criteria: 
    - Given processing completes, When metadata is recorded, Then job ID, retry count, processing time, and timestamps are stored (FR-027).
    - Given processing fails, When error occurs, Then error message is captured and displayed to user (FR-028).
    - Given the document list, When a document has failed, Then the error message is visible.

## Task Overview
Expose processing metadata through a backend API response so the frontend can display job timing and errors in the document list.

Because the API currently uses minimal endpoints in `Program.cs`, this task focuses on:
- Adding a typed response contract
- Implementing a `GET` endpoint for listing documents and their most recent processing job metadata

## Dependent Tasks
- [TASK_056_001] (Backend processing metadata persistence)
- [US_053 - Queue documents in RabbitMQ for processing]

## Impacted Components
- [CREATE | Server/ClinicalIntelligence.Api/Contracts/Documents/DocumentListItemResponse.cs | Response DTO for document list rows, including status + processing metadata]
- [CREATE | Server/ClinicalIntelligence.Api/Contracts/Documents/DocumentListResponse.cs | Envelope DTO for list results]
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Add `GET /api/v1/documents` minimal API endpoint returning list rows with processing metadata]

## Implementation Plan
- Add response contracts under `Server/ClinicalIntelligence.Api/Contracts/Documents/`:
  - `DocumentListItemResponse` should include:
    - `id`, `originalName`, `uploadedAt`, `status`
    - Processing metadata fields needed by the UI:
      - `jobId`, `retryCount`, `startedAt`, `completedAt`, `processingTimeMs`, `errorMessage`
  - `DocumentListResponse` should include an `items` array (and optionally `total` if pagination is added later)
- Implement a minimal API endpoint in `Program.cs`:
  - Route: `GET /api/v1/documents`
  - Require authorization (consistent with other v1 endpoints)
  - Query the `documents` table, join the latest `processing_jobs` record per document (e.g., by `CompletedAt`/`StartedAt` or by job `Id` if that is the stable ordering)
  - Ensure query is efficient (avoid N+1) and returns only needed columns
- Error handling and security:
  - Return safe `errorMessage` suitable for UI display
  - Truncate or normalize long error messages to prevent oversized responses
  - Do not expose raw `ErrorDetails` unless explicitly required (keep internal)

**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api/Contracts/Documents/DocumentListItemResponse.cs | DTO for document list row including processing metadata fields required by UI |
| CREATE | Server/ClinicalIntelligence.Api/Contracts/Documents/DocumentListResponse.cs | DTO envelope for the document list endpoint |
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Add `GET /api/v1/documents` endpoint that returns documents joined with latest `ProcessingJob` metadata |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis
- https://learn.microsoft.com/en-us/ef/core/querying/

## Build Commands
- dotnet build .\Server\ClinicalIntelligence.Api\ClinicalIntelligence.Api.csproj

## Implementation Validation Strategy
- Validate endpoint returns expected shape with at least:
  - One completed job including timestamps + duration
  - One failed job including `errorMessage`
- Validate that `errorMessage` is present for failed documents and absent/null for non-failed
- Validate query does not issue per-document queries (review SQL logging or use EF query inspection)

## Implementation Checklist
- [ ] Add typed response contracts for document list + processing metadata
- [ ] Implement `GET /api/v1/documents` minimal API endpoint
- [ ] Join latest `ProcessingJob` per document and map to response DTOs
- [ ] Ensure failed jobs surface a safe `errorMessage` and long messages are truncated
- [ ] Add basic automated test coverage (or defer to dedicated test task if preferred)
