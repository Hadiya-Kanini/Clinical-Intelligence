# Task - TASK_004

## Requirement Reference
- User Story: us_004
- Story Location: .propel/context/tasks/us_004/us_004.md
- Acceptance Criteria: 
    - Given the AI Worker entity schemas evolve, When entity output or validation schemas change, Then the schema is versioned and the system can distinguish versions (to support safe processing and retries).

## Task Overview
Introduce a versioned, contract-first location for AI Worker entity validation schemas and update the worker to load/validate against the correct schema version, so retries can safely process older jobs without ambiguity.

## Dependent Tasks
- .propel/context/tasks/us_001/task_003_define_backend_to_worker_job_schema.md (TASK_003)

## Impacted Components
- contracts/
- scripts/validate_contracts.py
- worker/

## Implementation Plan
- Define a canonical contract location for worker entity validation schemas under `contracts/` with explicit versioning.
- Add an initial schema artifact for entity extraction output/validation that includes a version identifier.
- Extend `scripts/validate_contracts.py` to validate the presence/shape of the worker entity schema contract(s).
- Update the worker to load the schema based on a version field and reject unknown versions with a clear error.
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | contracts/entities/v1/entity.schema.json | Initial versioned JSON schema for worker entity output/validation (includes version identifier) |
| CREATE | contracts/entities/v1/README.md | Documents the schema contract, versioning rules, and compatibility expectations |
| MODIFY | scripts/validate_contracts.py | Validate that entity schema contract exists and includes required keys/version identifiers |
| MODIFY | worker/main.py | Load entity schema by version and validate payloads; reject unknown versions deterministically |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://json-schema.org/

## Build Commands
- python scripts/validate_contracts.py
- python worker/main.py

## Implementation Validation Strategy
- Verify `validate_contracts.py` fails if the entity schema contract is missing or malformed.
- Verify the worker can validate payloads for the current schema version and rejects unknown schema versions.

## Implementation Checklist
- [x] Add versioned `contracts/entities/v1/` schema artifact(s)
- [x] Extend `validate_contracts.py` to validate entity schema contracts
- [x] Update worker schema loader/validator to select schema by version and reject unknown versions
