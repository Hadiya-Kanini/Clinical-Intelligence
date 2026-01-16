# Task - TASK_060_002

## Requirement Reference
- User Story: us_060
- Story Location: .propel/context/tasks/us_060/us_060.md
- Acceptance Criteria: 
    - Given merged text, When chunked, Then chunks are 500-1000 tokens in size (TR-005).
    - Given chunking, When performed, Then 100-token overlap is maintained between adjacent chunks.
    - Given chunking, When implemented, Then RecursiveCharacterTextSplitter from LangChain is used (FR-032a).
    - Given chunks, When created, Then they preserve document source metadata for each chunk.

## Task Overview
Implement semantic chunking in the AI Worker using LangChain `RecursiveCharacterTextSplitter` configured to produce 500-1000 token chunks with a 100-token overlap. Chunking must operate on patient-level merged text (US_059) while preserving per-chunk provenance metadata required for citations and downstream embedding/storage.

## Dependent Tasks
- [US_058 - Extract text with positional metadata from PDF/DOCX]
- [US_059 - Merge multi-document text per patient before chunking]
- [TASK_059_002 - Worker: patient-level multi-document text merge]
- [TASK_060_001 - Contracts: chunking output schema]

## Impacted Components
- [MODIFY: worker/requirements.txt]
- [MODIFY: worker/pipeline/__init__.py]
- [CREATE: worker/pipeline/text_chunking.py]
- [CREATE: worker/tests/test_text_chunking.py]

## Implementation Plan
- Add/confirm worker dependencies required for token-aware chunking and LangChain splitters:
  - LangChain packages required for `RecursiveCharacterTextSplitter`
  - A tokenizer/token-count dependency used to implement token-based sizing (for example via a `length_function` passed to the splitter)
- Implement a chunking entry point (e.g., `chunk_merged_text(...)`) that:
  - Accepts the merged patient text representation produced by US_059 (including per-segment provenance)
  - Produces chunked outputs aligned with `contracts/chunking/v1/chunked_text.schema.json`
- Configure `RecursiveCharacterTextSplitter` to:
  - Target ~1000 tokens max per chunk with a floor of ~500 tokens (enforced via configuration and post-validation)
  - Use 100-token overlap between adjacent chunks
  - Prefer splitting on paragraph/newline/sentence boundaries via an explicit separator list, while allowing fallback splitting when documents have unusual formatting
- Provenance preservation strategy:
  - Maintain a mapping from the merged text back to its source segments (including `document_id`, `page`, `section`, `coordinates` when available)
  - For each produced chunk, compute the set of source segments that contribute text to that chunk and emit them as provenance entries
  - Ensure chunks spanning document boundaries retain provenance for all contributing documents
- Handle edge cases explicitly:
  - Documents shorter than 500 tokens: emit a single chunk (no overlap) and preserve provenance
  - Unusual formatting (tables, excessive whitespace): preserve text but ensure chunker remains deterministic and does not drop metadata
  - Mid-sentence boundaries: tune separators to minimize splits; allow unavoidable splits but ensure overlap mitigates context loss
- Add unit tests validating:
  - Chunk token counts are within 500-1000 for typical inputs
  - Overlap is approximately 100 tokens (as measured by the selected tokenizer)
  - Provenance metadata is preserved for each chunk, including multi-document chunks
  - Deterministic output for the same input (ordering, chunk_index, provenance ordering)
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | worker/requirements.txt | Add/confirm LangChain splitter and token-count dependencies required for token-aware `RecursiveCharacterTextSplitter` sizing and overlap |
| MODIFY | worker/pipeline/__init__.py | Export chunking entry point for downstream pipeline steps (US_061 embeddings)
| CREATE | worker/pipeline/text_chunking.py | Implement token-aware chunking using `RecursiveCharacterTextSplitter` with 500-1000 token sizing, 100-token overlap, and provenance preservation |
| CREATE | worker/tests/test_text_chunking.py | Unit tests for chunk sizing, overlap, determinism, and provenance preservation across single- and multi-document inputs |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://python.langchain.com/docs/concepts/text_splitters/

## Build Commands
- python -m pip install -r worker/requirements.txt
- python -m pytest worker/tests

## Implementation Validation Strategy
- Validate chunk sizing and overlap via unit tests using representative merged-text fixtures.
- Validate provenance preservation by asserting each chunk includes source metadata and that multi-document spans yield multiple provenance entries.
- Validate edge cases (short documents, unusual formatting, mid-sentence boundaries) produce stable, non-empty chunk outputs.

## Implementation Checklist
- [ ] Update `worker/requirements.txt` with required dependencies for `RecursiveCharacterTextSplitter` and token counting
- [ ] Implement `worker/pipeline/text_chunking.py` with token-aware chunk sizing and 100-token overlap
- [ ] Preserve provenance metadata per chunk and support chunks spanning document boundaries
- [ ] Export chunking entry point via `worker/pipeline/__init__.py`
- [ ] Add unit tests for chunk sizing, overlap, determinism, and provenance preservation
- [ ] Add unit tests for short-document and unusual-formatting edge cases
