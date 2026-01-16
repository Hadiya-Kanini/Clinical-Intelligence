# Task - TASK_058_001

## Requirement Reference
- User Story: us_058
- Story Location: .propel/context/tasks/us_058/us_058.md
- Acceptance Criteria: 
    - Given a PDF file, When processed, Then text is extracted using PyPDFLoader with positional metadata (FR-030).
    - Given a DOCX file, When processed, Then text is extracted using Docx2txtLoader with positional metadata (FR-031).
    - Given extracted text, When stored, Then page number, section, and coordinates (when available) are preserved.

## Task Overview
Define and version the integration contract(s) required to pass extracted text with positional metadata from the AI worker into the backend processing pipeline, without bypassing the canonical `contracts/` boundary. This establishes a stable schema for extracted text segments that can later be chunked, embedded, and stored as `DocumentChunk` rows.

## Dependent Tasks
- [US_053 - Queue documents in RabbitMQ for processing]

## Impacted Components
- [MODIFY: contracts/jobs/v1/job.schema.json]
- [CREATE: contracts/text_extraction/v1/extracted_text.schema.json]
- [CREATE: contracts/migrations/<new_migration_note>.md]
- [MODIFY: worker/tests/fixtures/job_payloads.py]
- [MODIFY: worker/tests/test_job_validation.py]

## Implementation Plan
- Update the job payload contract to include the minimum metadata needed for extraction:
  - `payload.storage_path` (string) for the worker to locate the document
  - `payload.mime_type` (string) to select PDF vs DOCX extraction strategy
  - Keep these fields optional in the schema if necessary to remain backward-compatible with existing producers.
- Create a new extracted-text output contract (`contracts/text_extraction/v1/extracted_text.schema.json`) defining:
  - `schema_version`
  - `document_id`
  - `segments[]` with:
    - `text`
    - `document_location.page` (optional)
    - `document_location.section` (optional)
    - `document_location.coordinates` (optional bbox)
- Add a contract migration note per `contracts/README.md`:
  - Classify as backward-compatible vs breaking change (based on whether any new required fields were added)
  - Specify producer/consumer impacts (Backend API and AI worker)
- Update worker schema validation fixtures/tests to validate the updated job contract shape.
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | contracts/jobs/v1/job.schema.json | Add optional `payload.storage_path` + `payload.mime_type` fields so the worker can determine extraction strategy and read the document from storage |
| CREATE | contracts/text_extraction/v1/extracted_text.schema.json | Canonical schema for extracted text segments including `page`, `section`, and optional `coordinates` |
| CREATE | contracts/migrations/<new_migration_note>.md | Migration note documenting the contract change, versioning decision, and consumer impact |
| MODIFY | worker/tests/fixtures/job_payloads.py | Add fixture(s) that include `payload.storage_path` + `payload.mime_type` for schema validation |
| MODIFY | worker/tests/test_job_validation.py | Validate job payloads including the new extraction-relevant fields |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://json-schema.org/

## Build Commands
- python -m pip install -r worker/requirements.txt
- python -m pytest worker/tests

## Implementation Validation Strategy
- Validate updated job payloads pass schema validation in worker tests.
- Validate the extracted-text schema represents page/section/coordinates in a way that can be mapped to backend `DocumentChunk.Page`, `DocumentChunk.Section`, and `DocumentChunk.Coordinates`.

## Implementation Checklist
- [ ] Extend `contracts/jobs/v1/job.schema.json` with optional `payload.storage_path` and `payload.mime_type`
- [ ] Create `contracts/text_extraction/v1/extracted_text.schema.json` with segment + positional metadata structure
- [ ] Add a migration note documenting the schema/version change and upgrade guidance
- [ ] Update worker schema validation fixtures/tests to cover the new payload fields
