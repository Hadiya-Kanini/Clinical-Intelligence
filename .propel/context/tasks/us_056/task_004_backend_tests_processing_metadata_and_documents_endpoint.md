# Task - TASK_056_004

## Requirement Reference
- User Story: us_056
- Story Location: .propel/context/tasks/us_056/us_056.md
- Acceptance Criteria: 
    - Given processing completes, When metadata is recorded, Then job ID, retry count, processing time, and timestamps are stored (FR-027).
    - Given processing fails, When error occurs, Then error message is captured and displayed to user (FR-028).
    - Given the document list, When a document has failed, Then the error message is visible.

## Task Overview
Add backend automated test coverage for the processing metadata write path and the documents list endpoint so regressions are caught early.

This task focuses on validating:
- `processing_jobs` updates (timing + errors)
- `GET /api/v1/documents` contract and behavior

## Dependent Tasks
- [TASK_056_001] (Backend processing metadata persistence)
- [TASK_056_002] (Backend documents list API includes processing metadata)

## Impacted Components
- [CREATE | Server/ClinicalIntelligence.Api.Tests/Integration/DocumentsListEndpointTests.cs | Integration tests for `GET /api/v1/documents` response shape and error surfacing]
- [CREATE | Server/ClinicalIntelligence.Api.Tests/Services/ProcessingJobs/DbProcessingJobMetadataWriterTests.cs | Focused tests for status/timing/error updates]

## Implementation Plan
- Writer tests:
  - Use the existing DB test pattern (skip when PostgreSQL is unavailable).
  - Seed a `Document` and `ProcessingJob` row.
  - Execute writer methods to mark started/completed/failed.
  - Assert persisted fields:
    - `StartedAt`, `CompletedAt`, `ProcessingTimeMs` combinations
    - `ErrorMessage` set only on fail
    - `RetryCount` updates
- Endpoint tests:
  - Use `TestWebApplicationFactory` pattern used by other integration tests.
  - Seed at least one failed document + job.
  - Call `GET /api/v1/documents` under an authenticated standard user context.
  - Assert:
    - Response shape matches `DocumentListResponse`
    - Failed document includes `errorMessage`
    - Non-failed documents do not expose `errorMessage`

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api.Tests/Services/ProcessingJobs/DbProcessingJobMetadataWriterTests.cs | Validate `processing_jobs` updates for timing, retry, and error capture |
| CREATE | Server/ClinicalIntelligence.Api.Tests/Integration/DocumentsListEndpointTests.cs | Validate documents list endpoint includes processing metadata and surfaces error messages for failed jobs |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests
- https://learn.microsoft.com/en-us/ef/core/testing/

## Build Commands
- dotnet build .\Server\ClinicalIntelligence.Api.Tests\ClinicalIntelligence.Api.Tests.csproj
- dotnet test .\Server\ClinicalIntelligence.Api.Tests\ClinicalIntelligence.Api.Tests.csproj

## Implementation Validation Strategy
- Confirm tests pass when DB is available and are skipped gracefully when not.
- Confirm tests validate both data persistence and API response behavior.

## Implementation Checklist
- [ ] Add writer persistence tests for start/complete/fail flows
- [ ] Add endpoint integration tests for `GET /api/v1/documents`
- [ ] Seed failed+completed cases and assert error surfacing rules
- [ ] Ensure tests do not require external services beyond PostgreSQL (skip if not configured)
