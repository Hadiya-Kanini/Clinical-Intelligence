# Task - TASK_070_003

## Requirement Reference
- User Story: us_070
- Story Location: .propel/context/tasks/us_070/us_070.md
- Acceptance Criteria: 
    - Given critical entities (diagnoses, procedures, medications), When displayed, Then clickable reference links navigate to source (FR-054).
    - Given a reference click, When triggered, Then the source document section is highlighted (UXR-024).

## Task Overview
Add a secure backend endpoint that allows the frontend to retrieve source document content for citation navigation. This is a prerequisite for in-app viewing and highlighting of cited content.

This task focuses on:
- Streaming a stored document file by `documentId`
- Enforcing authentication and basic access control
- Returning safe, deterministic errors for missing/deleted documents (edge case)

## Dependent Tasks
- [US_056 TASK_002] (Documents API conventions for listing/metadata, if applicable)

## Impacted Components
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Add `GET /api/v1/documents/{documentId}/content` endpoint to stream document bytes]
- [CREATE | Server/ClinicalIntelligence.Api/Services/Documents/DocumentContentReader.cs | Encapsulate document lookup + access checks + file streaming path resolution]
- [CREATE | Server/ClinicalIntelligence.Api.Tests/DocumentContentEndpointTests.cs | Validate auth, 404 behavior, and content headers]

## Implementation Plan
- Define endpoint:
  - Route: `GET /api/v1/documents/{documentId}/content`
  - Auth: `.RequireAuthorization(AuthorizationPolicies.Authenticated)`
  - Response:
    - `200` with file stream (`application/pdf` or stored `MimeType`)
    - `404` if document not found or soft-deleted
- Access control:
  - Enforce authenticated access.
  - Verify requested document exists in `ApplicationDbContext.Documents` and `!IsDeleted`.
  - If additional patient/user scoping rules exist in the codebase, apply them (least privilege).
- File streaming:
  - Resolve file path from `Document.StoragePath`.
  - Stream using `Results.File(stream, contentType, fileDownloadName)`.
  - Ensure no path traversal: treat `StoragePath` as a trusted persisted path and avoid concatenating untrusted input.
- Error handling:
  - Return standardized errors via `ApiErrorResults` for 404.

**Focus on how to implement**

## Current Project State
- `Document` model includes `StoragePath`, `MimeType`, and `OriginalName`.
- `Program.cs` currently does not expose any `/api/v1/documents/*` endpoints.

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Add `GET /api/v1/documents/{documentId}/content` endpoint with auth + safe error handling |
| CREATE | Server/ClinicalIntelligence.Api/Services/Documents/DocumentContentReader.cs | Service for document lookup, access checks, and file stream retrieval |
| CREATE | Server/ClinicalIntelligence.Api.Tests/DocumentContentEndpointTests.cs | Tests for 401/404/200 and content headers |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis

## Build Commands
- dotnet build .\Server\ClinicalIntelligence.Api\ClinicalIntelligence.Api.csproj
- dotnet test .\Server\ClinicalIntelligence.Api.Tests\ClinicalIntelligence.Api.Tests.csproj

## Implementation Validation Strategy
- [API] As authenticated user, request `GET /api/v1/documents/{id}/content` and confirm the response streams the document with correct content type.
- [Security] As unauthenticated user, confirm endpoint returns `401`.
- [Edge Case] For a deleted/missing document, confirm endpoint returns `404` with standardized error response.

## Implementation Checklist
- [ ] Implement `DocumentContentReader` to load document metadata and open a stream safely
- [ ] Add `GET /api/v1/documents/{documentId}/content` endpoint with authorization
- [ ] Return `404` for missing/deleted documents with standardized error shape
- [ ] Add tests for success and failure cases
