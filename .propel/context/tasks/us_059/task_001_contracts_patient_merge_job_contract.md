# Task - TASK_059_001

## Requirement Reference
- User Story: us_059
- Story Location: .propel/context/tasks/us_059/us_059.md
- Acceptance Criteria: 
    - Given multiple documents for the same patient, When processed, Then text is merged before semantic chunking (FR-032).
    - Given document merge, When performed, Then document boundaries and source metadata are preserved.
    - Given patient identification, When documents are linked, Then MRN or name+DOB matching is used (FR-050).

## Task Overview
Define and version the integration contract(s) required to support patient-level multi-document text merge prior to chunking. This establishes a stable, contract-driven way for the Backend API to instruct the AI Worker to merge multiple documents for a single patient, and for the worker to emit a merged, source-preserving representation suitable for downstream chunking (US_060).

## Dependent Tasks
- [US_058 - Extract text with positional metadata from PDF/DOCX]
- [TASK_058_001 - Backend contracts: text extraction payload and schema]

## Impacted Components
- [MODIFY: contracts/jobs/v1/job.schema.json]
- [MODIFY: contracts/jobs/v1/README.md]
- [MODIFY: contracts/migrations/jobs_v1.md]
- [CREATE: contracts/text_merge/v1/merged_text.schema.json]
- [CREATE: contracts/migrations/text_merge_v1.md]

## Implementation Plan
- Extend `contracts/jobs/v1/job.schema.json` in a backward-compatible way to support patient-level merge orchestration:
  - Add optional `payload.patient_id` (string UUID) so the job can be explicitly scoped to a patient
  - Add optional `payload.document_ids` (array of string IDs) for multi-document processing
  - Add optional `payload.patient_identifiers` (object) to support MRN or name+DOB matching inputs when `patient_id` is not supplied
- Update `contracts/jobs/v1/README.md` to document:
  - The new optional fields
  - The expected worker behavior when `document_ids` is present
  - Any precedence rules (e.g., `patient_id` wins over derived identity)
- Update `contracts/migrations/jobs_v1.md` with a new entry capturing:
  - Version bump decision (e.g., 1.1)
  - Change type classification
  - Required actions for Backend API and AI Worker
- Introduce a new canonical contract for the merge output:
  - Create `contracts/text_merge/v1/merged_text.schema.json` defining merged text segments that preserve per-document provenance (document_id + location metadata)
  - Create `contracts/migrations/text_merge_v1.md` as the migration log for this new contract
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | contracts/jobs/v1/job.schema.json | Add optional payload fields (`patient_id`, `document_ids`, `patient_identifiers`) to support patient-level multi-document processing without breaking existing producers |
| MODIFY | contracts/jobs/v1/README.md | Document new optional fields and expected behavior when `document_ids` is supplied |
| MODIFY | contracts/migrations/jobs_v1.md | Add a new migration entry documenting the schema change, classification, and consumer actions |
| CREATE | contracts/text_merge/v1/merged_text.schema.json | Canonical schema for merged text that preserves per-document boundaries and source metadata for downstream chunking |
| CREATE | contracts/migrations/text_merge_v1.md | Migration notes and change log for the new `text_merge` contract |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://json-schema.org/
- https://semver.org/

## Build Commands
- python -m pip install -r worker/requirements.txt
- python -m pytest worker/tests

## Implementation Validation Strategy
- Validate job payloads supporting patient merge can be expressed without adding any new required fields to the existing job schema.
- Validate merged-text schema can represent document provenance (document_id + location metadata) in a way that downstream chunking can map to `DocumentChunk` metadata.

## Implementation Checklist
- [ ] Update `contracts/jobs/v1/job.schema.json` with optional patient merge fields
- [ ] Update `contracts/jobs/v1/README.md` to document the new optional fields and semantics
- [ ] Append a new version entry to `contracts/migrations/jobs_v1.md`
- [ ] Create `contracts/text_merge/v1/merged_text.schema.json` for merged text output
- [ ] Create `contracts/migrations/text_merge_v1.md` for the new contract
- [ ] Ensure no changes require out-of-contract direct integrations between Backend API and AI Worker
