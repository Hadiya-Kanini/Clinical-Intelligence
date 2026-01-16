# Task - TASK_059_002

## Requirement Reference
- User Story: us_059
- Story Location: .propel/context/tasks/us_059/us_059.md
- Acceptance Criteria: 
    - Given multiple documents for the same patient, When processed, Then text is merged before semantic chunking (FR-032).
    - Given document merge, When performed, Then document boundaries and source metadata are preserved.
    - Given merged text, When chunked, Then chunks can span document boundaries while maintaining source references.

## Task Overview
Implement patient-level multi-document text merge logic in the AI Worker. The merge must preserve per-document provenance while producing a single merged stream suitable for semantic chunking (US_060).

## Dependent Tasks
- [US_058 - Extract text with positional metadata from PDF/DOCX]
- [TASK_059_001 - Contracts: patient merge job contract]

## Impacted Components
- [CREATE: worker/pipeline/__init__.py]
- [CREATE: worker/pipeline/patient_text_merge.py]
- [CREATE: worker/tests/test_patient_text_merge.py]

## Implementation Plan
- Introduce a `merge_patient_documents(...)` function (or equivalent) that:
  - Accepts a list of document-level extracted segments (each segment includes `document_id` and location metadata)
  - Produces a merged sequence preserving:
    - Original `document_id` for each segment
    - Segment ordering within each document
    - Stable document ordering across documents (e.g., by upload timestamp or by provided `document_ids` order)
  - Inserts explicit boundary markers (logical, not necessarily literal text) so downstream chunking can keep provenance even when chunks span boundaries
- Add guardrails:
  - Detect conflicting patient identifiers when job payload provides per-document patient identity metadata
  - Handle extremely large merged output by avoiding quadratic concatenation patterns (prefer list accumulation / generators)
- Add worker unit tests that validate:
  - Order stability
  - Provenance retention per segment
  - Boundary handling does not discard location metadata
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | worker/pipeline/__init__.py | Initialize pipeline package for worker processing steps |
| CREATE | worker/pipeline/patient_text_merge.py | Implement patient-level multi-document merge logic that preserves per-document provenance |
| CREATE | worker/tests/test_patient_text_merge.py | Unit tests for merge ordering, boundary preservation, and provenance retention |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://docs.python.org/3/library/itertools.html

## Build Commands
- python -m pip install -r worker/requirements.txt
- python -m pytest worker/tests

## Implementation Validation Strategy
- Verify merged output retains `document_id` + location metadata per segment.
- Verify merge ordering is deterministic for the same input.
- Verify merge logic scales linearly with number of segments (no repeated string concatenation in loops).

## Implementation Checklist
- [ ] Create `worker/pipeline/patient_text_merge.py` with a merge function returning a merged segment stream
- [ ] Define and enforce deterministic ordering rules across documents
- [ ] Preserve per-segment provenance fields required for citations
- [ ] Add boundary representation that downstream chunking can use to maintain source references
- [ ] Add unit tests for multi-document merge behavior
- [ ] Add unit tests for large input handling (performance-focused, non-flaky)
