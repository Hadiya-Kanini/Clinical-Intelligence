# Task - [TASK_001]

## Requirement Reference
- User Story: [us_044]
- Story Location: [.propel/context/tasks/us_044/us_044.md]
- Acceptance Criteria: 
    - Given a file is uploaded (≤50MB), When received by the server, Then acknowledgment is returned within 5 seconds (NFR-001).
    - Given upload acknowledgment, When returned, Then it includes validation status for the file.
    - Given the upload endpoint, When processing, Then it validates and acknowledges before queuing for processing.
    - Given performance monitoring, When uploads occur, Then response times are logged for SLA tracking.

## Task Overview
Implement or enhance the backend document upload endpoint to ensure acknowledgment is returned within 5 seconds for files up to 50MB. The endpoint must validate the file, return validation status in the acknowledgment response, and log response times for SLA monitoring. Processing should be queued asynchronously after acknowledgment.

This task focuses on the backend API layer (.NET) and ensures the upload flow meets NFR-001 performance requirements.

## Dependent Tasks
- [EP-DB-001] - Database schema with DOCUMENT table
- [US_042] - Frontend upload UI (provides upload requests)

## Impacted Components
- [MODIFY | Server/ClinicalIntelligence.Api/Endpoints/DocumentEndpoints.cs | Enhance upload endpoint for fast acknowledgment]
- [MODIFY | Server/ClinicalIntelligence.Api/Services/DocumentService.cs | Add validation and async queue logic]
- [CREATE | Server/ClinicalIntelligence.Api/Contracts/UploadAcknowledgmentResponse.cs | Response contract with validation status]

## Implementation Plan

### 1. Optimize Upload Endpoint for Fast Acknowledgment
- Ensure file is received and validated synchronously
- Return acknowledgment immediately after validation
- Queue document for async processing (RabbitMQ)
- Target: <5 seconds for files ≤50MB

### 2. Implement Validation in Acknowledgment
- Validate file type (PDF, DOCX only)
- Validate file size (≤50MB)
- Validate file integrity (not corrupted, not password-protected)
- Return validation status in response (valid/invalid with reasons)

### 3. Response Contract
```csharp
public record UploadAcknowledgmentResponse
{
    public Guid DocumentId { get; init; }
    public string FileName { get; init; }
    public string Status { get; init; } // "Accepted", "ValidationFailed"
    public bool IsValid { get; init; }
    public List<string> ValidationErrors { get; init; }
    public DateTime AcknowledgedAt { get; init; }
}
```

### 4. Performance Logging for SLA Tracking
- Log request start time on endpoint entry
- Log response time on endpoint exit
- Include document ID, file size, and validation result
- Use structured logging for aggregation

### 5. Handle Edge Cases
- **Heavy server load**: Ensure validation is lightweight
- **Files close to 50MB limit**: Test boundary conditions
- **Acknowledgment >5 seconds**: Log warning, investigate bottlenecks
- **Concurrent uploads**: Ensure thread-safe handling

### 6. Async Processing Queue
- After acknowledgment, enqueue job to RabbitMQ
- Document status set to "Pending" in database
- Worker picks up job for full processing

**Focus on how to implement**

## Current Project State
```
Server/ClinicalIntelligence.Api/
├── Endpoints/
│   └── (endpoint files)
├── Services/
│   └── (service files)
├── Contracts/
│   └── (request/response contracts)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Endpoints/DocumentEndpoints.cs | Optimize upload endpoint for <5s acknowledgment |
| MODIFY | Server/ClinicalIntelligence.Api/Services/DocumentService.cs | Add fast validation and async queue logic |
| CREATE | Server/ClinicalIntelligence.Api/Contracts/UploadAcknowledgmentResponse.cs | Response contract with validation status |

## External References
- https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads
- https://learn.microsoft.com/en-us/dotnet/core/extensions/logging

## Build Commands
- dotnet build Server/ClinicalIntelligence.Api
- dotnet test Server/ClinicalIntelligence.Api.Tests

## Implementation Validation Strategy
- [Automated] Unit tests verify acknowledgment response structure
- [Automated] Integration tests verify <5s response time for 50MB file
- [Automated] Tests verify validation status is included in response
- [Manual] Load test with concurrent uploads to verify performance under load
- [Automated] Verify response time logging is captured

## Implementation Checklist
- [x] Create UploadAcknowledgmentResponse contract
- [x] Modify upload endpoint to return acknowledgment immediately after validation
- [x] Implement lightweight file validation (type, size, integrity)
- [x] Add response time logging with structured logging
- [x] Ensure async queue for processing after acknowledgment
- [x] Test with 50MB file to verify <5s response
- [x] Test with invalid files to verify validation errors returned
- [x] Add performance logging for SLA tracking
