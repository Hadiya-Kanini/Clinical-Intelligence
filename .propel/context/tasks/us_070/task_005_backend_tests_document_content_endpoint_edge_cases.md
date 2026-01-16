# Task - TASK_070_005

## Requirement Reference
- User Story: us_070
- Story Location: .propel/context/tasks/us_070/us_070.md
- Acceptance Criteria: 
    - Given an entity, When displayed, Then source document name, page number, and section are shown with clickable link.
    - What happens when source document is no longer available?

## Task Overview
Add backend automated coverage for source document retrieval edge cases so the system fails safely and predictably when documents are missing, deleted, or inaccessible.

This task focuses on tests only (no feature work):
- 404 behavior for missing/deleted documents
- Authorization behavior
- Content headers consistency for successful responses

## Dependent Tasks
- [TASK_070_003] (Backend document content endpoint)

## Impacted Components
- [MODIFY | Server/ClinicalIntelligence.Api.Tests/DocumentContentEndpointTests.cs | Expand coverage for missing/deleted document cases and auth enforcement]

## Implementation Plan
- Expand endpoint tests to cover:
  - `401` when unauthenticated
  - `404` when document does not exist
  - `404` when document exists but `IsDeleted = true`
  - `200` returns expected content type and content-disposition filename
- Keep assertions aligned to `ApiErrorResults` standardized response shape.

**Focus on how to implement**

## Current Project State
- Test project exists: `Server/ClinicalIntelligence.Api.Tests`.

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api.Tests/DocumentContentEndpointTests.cs | Add/extend test cases for missing/deleted documents and auth requirements |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests

## Build Commands
- dotnet test .\Server\ClinicalIntelligence.Api.Tests\ClinicalIntelligence.Api.Tests.csproj

## Implementation Validation Strategy
- [Tests] All endpoint edge-case tests pass and prevent regressions on error codes.

## Implementation Checklist
- [ ] Add unauthenticated (401) coverage for document content endpoint
- [ ] Add missing document (404) coverage
- [ ] Add deleted document (404) coverage
- [ ] Add success response header assertions (content-type, filename)
