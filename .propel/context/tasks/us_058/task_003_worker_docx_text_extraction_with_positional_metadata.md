# Task - TASK_058_003

## Requirement Reference
- User Story: us_058
- Story Location: .propel/context/tasks/us_058/us_058.md
- Acceptance Criteria: 
    - Given a DOCX file, When processed, Then text is extracted using Docx2txtLoader with positional metadata (FR-031).
    - Given extracted text, When stored, Then page number, section, and coordinates (when available) are preserved.
    - Given extraction, When completed, Then the text is ready for chunking and embedding.

## Task Overview
Implement DOCX text extraction in the AI worker using LangChain `Docx2txtLoader`, producing structured text segments with best-effort section metadata (e.g., heading-based) and preserving coordinates when available.

## Dependent Tasks
- .propel/context/tasks/us_058/task_001_backend_contracts_text_extraction_payload_and_schema.md (TASK_058_001)

## Impacted Components
- [MODIFY: worker/requirements.txt]
- [CREATE: worker/text_extraction/docx_extractor.py]
- [MODIFY: worker/main.py]
- [CREATE: worker/tests/test_docx_text_extraction.py]

## Implementation Plan
- Add required dependencies to `worker/requirements.txt` for DOCX extraction:
  - LangChain community loaders
  - Any DOCX parsing dependency required by `Docx2txtLoader`
- Implement `extract_docx_text(storage_path: str) -> ExtractedTextResult`:
  - Use `Docx2txtLoader(storage_path).load()` to get document text
  - Normalize into `segments[]`:
    - `text`
    - `section` best-effort (e.g., split on headings if the loader provides paragraph metadata; otherwise keep null)
    - `page` is typically unavailable for DOCX; keep null unless derivable
    - `coordinates` typically unavailable; keep null unless derivable
- Update `worker/main.py` routing to call DOCX extractor when `payload.mime_type` indicates DOCX.
- Add unit tests that:
  - Validate the extractor returns non-empty text for a sample DOCX
  - Validate section metadata is optional and safe when not available
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | worker/requirements.txt | Add LangChain and DOCX parsing dependencies required for `Docx2txtLoader` |
| CREATE | worker/text_extraction/docx_extractor.py | Implement DOCX extraction using `Docx2txtLoader` and normalize output into `segments[]` with best-effort `section` metadata |
| MODIFY | worker/main.py | Add routing that sends DOCX jobs (by `payload.mime_type`) to the DOCX extractor |
| CREATE | worker/tests/test_docx_text_extraction.py | Unit tests validating DOCX extraction output and optional positional metadata behavior |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://python.langchain.com/docs/integrations/document_loaders/docx/

## Build Commands
- python -m pip install -r worker/requirements.txt
- python -m pytest worker/tests

## Implementation Validation Strategy
- Verify a known DOCX produces at least one segment with non-empty `text`.
- Verify `section` is present when derivable, and safely null otherwise.
- Verify the extractor produces output aligned to the extracted-text contract.

## Implementation Checklist
- [ ] Add/verify worker dependencies for LangChain DOCX loading
- [ ] Implement DOCX extraction and normalization into segments (best-effort section/page/coordinates)
- [ ] Wire extraction routing in `worker/main.py` based on `payload.mime_type`
- [ ] Add unit tests for DOCX extraction behavior and metadata preservation
