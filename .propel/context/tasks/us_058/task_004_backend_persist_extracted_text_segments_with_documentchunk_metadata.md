# Task - TASK_058_004

## Requirement Reference
- User Story: us_058
- Story Location: .propel/context/tasks/us_058/us_058.md
- Acceptance Criteria: 
    - Given extracted text, When stored, Then page number, section, and coordinates (when available) are preserved.
    - Given extraction, When completed, Then the text is ready for chunking and embedding.

## Task Overview
Implement a backend persistence boundary for extracted text segments so the system can store extracted text along with positional metadata in the existing `DocumentChunk` model (`Page`, `Section`, `Coordinates`). This makes extracted text durable and ready for downstream chunking + embedding tasks.

## Dependent Tasks
- [US_053 - Queue documents in RabbitMQ for processing]
- [US_119 - Baseline Schema Migration - 16 Core Tables] (for `document_chunks` persistence)

## Impacted Components
- [MODIFY: Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs]
- [CREATE: Server/ClinicalIntelligence.Api/Services/DocumentChunks/IExtractedTextSegmentWriter.cs]
- [CREATE: Server/ClinicalIntelligence.Api/Services/DocumentChunks/DbExtractedTextSegmentWriter.cs]
- [MODIFY: Server/ClinicalIntelligence.Api/Program.cs]
- [CREATE: Server/ClinicalIntelligence.Api.Tests/ExtractedTextSegmentWriterTests.cs]

## Implementation Plan
- Ensure the backend has an explicit application boundary for persisting extracted segments (DIP):
  - Define `IExtractedTextSegmentWriter` that accepts:
    - `DocumentId`
    - list of extracted segments (`text`, `page`, `section`, `coordinates`)
  - Keep the interface storage-oriented and independent of worker implementation details.
- Implement `DbExtractedTextSegmentWriter` using `ApplicationDbContext`:
  - Persist one `DocumentChunk` row per extracted segment
  - Set:
    - `DocumentId`
    - `TextContent`
    - `Page` / `Section`
    - `Coordinates` (store serialized JSON for bbox when provided)
  - Leave `Embedding` null at this stage (embedding is handled by US_061/US_062)
- Update DI registration in `Program.cs`.
- Add tests to validate:
  - Segments are persisted with correct metadata
  - Coordinates serialization round-trips as expected (string JSON stored)
  - Null metadata values are handled safely
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs | Ensure `DocumentChunk` persistence is enabled/registered for storing extracted text segments and metadata |
| CREATE | Server/ClinicalIntelligence.Api/Services/DocumentChunks/IExtractedTextSegmentWriter.cs | Define the contract for persisting extracted text segments into `document_chunks` with page/section/coordinates |
| CREATE | Server/ClinicalIntelligence.Api/Services/DocumentChunks/DbExtractedTextSegmentWriter.cs | EF Core-backed implementation that writes `DocumentChunk` rows (with `Embedding` unset) |
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Register `IExtractedTextSegmentWriter` in DI for consumption by queue/worker integration |
| CREATE | Server/ClinicalIntelligence.Api.Tests/ExtractedTextSegmentWriterTests.cs | Persistence tests validating metadata preservation for stored extracted segments |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/ef/core/

## Build Commands
- dotnet build .\Server\ClinicalIntelligence.Api\ClinicalIntelligence.Api.csproj
- dotnet test .\Server\ClinicalIntelligence.Api.Tests\ClinicalIntelligence.Api.Tests.csproj

## Implementation Validation Strategy
- Verify that stored `DocumentChunk` rows preserve `Page`, `Section`, and `Coordinates` exactly as provided.
- Verify that extracted text is persisted without requiring embeddings.

## Implementation Checklist
- [ ] Define `IExtractedTextSegmentWriter` interface for persisting extracted segments
- [ ] Implement `DbExtractedTextSegmentWriter` using EF Core
- [ ] Enable/confirm `DocumentChunk` persistence path in `ApplicationDbContext`
- [ ] Register writer in DI (`Program.cs`)
- [ ] Add tests validating metadata preservation and safe handling of missing coordinates
