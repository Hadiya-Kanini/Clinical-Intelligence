# Task - TASK_058_002

## Requirement Reference
- User Story: us_058
- Story Location: .propel/context/tasks/us_058/us_058.md
- Acceptance Criteria: 
    - Given a PDF file, When processed, Then text is extracted using PyPDFLoader with positional metadata (FR-030).
    - Given extracted text, When stored, Then page number, section, and coordinates (when available) are preserved.
    - Given extraction, When completed, Then the text is ready for chunking and embedding.

## Task Overview
Implement PDF text extraction in the AI worker using LangChain `PyPDFLoader`, producing structured text segments with positional metadata (page always when available; coordinates when available) aligned to the extracted-text contract.

## Dependent Tasks
- .propel/context/tasks/us_058/task_001_backend_contracts_text_extraction_payload_and_schema.md (TASK_058_001)

## Impacted Components
- [MODIFY: worker/requirements.txt]
- [CREATE: worker/text_extraction/__init__.py]
- [CREATE: worker/text_extraction/pdf_extractor.py]
- [CREATE: worker/text_extraction/models.py]
- [MODIFY: worker/main.py]
- [CREATE: worker/tests/test_pdf_text_extraction.py]

## Implementation Plan
- Add required dependencies to `worker/requirements.txt` for PDF extraction:
  - LangChain core + community loaders
  - Any PDF parsing dependency required by `PyPDFLoader`
- Define an internal worker model for extracted segments (aligned to `contracts/text_extraction/v1/extracted_text.schema.json`).
- Implement `extract_pdf_text(storage_path: str) -> ExtractedTextResult`:
  - Use `PyPDFLoader(storage_path).load()` to get page-level documents
  - Normalize outputs into `segments[]` with:
    - `text`
    - `page` from loader metadata
    - `section` best-effort (optional; can be null if not derivable)
    - `coordinates` best-effort (optional; may be null when not available from loader)
- Update `worker/main.py` to include a minimal execution path (callable function or CLI path) that:
  - Accepts a job payload (dict)
  - Selects PDF extraction based on `payload.mime_type`
  - Returns extracted-text result ready for downstream chunking
- Add unit tests that:
  - Validate the extractor returns non-empty text for a sample PDF
  - Validate `page` is set when the loader provides it
  - Validate missing coordinates are handled gracefully (null) and do not crash the pipeline
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | worker/requirements.txt | Add LangChain and PDF parsing dependencies required for `PyPDFLoader` |
| CREATE | worker/text_extraction/models.py | Define typed models for extracted text segments and locations (page/section/coordinates) aligned to the extracted-text contract |
| CREATE | worker/text_extraction/pdf_extractor.py | Implement PDF extraction using `PyPDFLoader` and normalize output into `segments[]` with positional metadata |
| MODIFY | worker/main.py | Add a minimal integration seam that routes jobs with PDF `mime_type` to the PDF extractor |
| CREATE | worker/tests/test_pdf_text_extraction.py | Unit tests validating PDF extraction output and positional metadata behavior |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://python.langchain.com/docs/how_to/document_loader_pdf/

## Build Commands
- python -m pip install -r worker/requirements.txt
- python -m pytest worker/tests

## Implementation Validation Strategy
- Verify a known PDF produces at least one segment with non-empty `text`.
- Verify segment metadata includes `page` where supported.
- Verify missing `coordinates` does not fail extraction and is represented as null/absent.

## Implementation Checklist
- [ ] Add/verify worker dependencies for LangChain PDF loading
- [ ] Implement `ExtractedTextResult` + `ExtractedTextSegment` models (page/section/coordinates)
- [ ] Implement `PyPDFLoader` extraction and normalization into segments
- [ ] Wire extraction routing in `worker/main.py` based on `payload.mime_type`
- [ ] Add unit tests for PDF extraction behavior and metadata preservation
