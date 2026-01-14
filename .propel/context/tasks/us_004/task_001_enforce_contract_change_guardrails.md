# Task - TASK_001

## Requirement Reference
- User Story: us_004
- Story Location: .propel/context/tasks/us_004/us_004.md
- Acceptance Criteria: 
    - Given the Backend API evolves over time, When API endpoints are added or changed, Then the Swagger/OpenAPI contract is updated and the versioning strategy is followed (including documenting any breaking changes).
    - Given the solution contains multiple services, When new functionality is implemented, Then it is placed in the correct service (Web UI vs Backend API vs AI Worker) and does not bypass the published contracts.

## Task Overview
Add enforceable guardrails that make contract-first development and boundary compliance difficult to bypass:
- ensure contract changes (OpenAPI + job schema) are accompanied by migration notes
- provide a single canonical checklist developers follow when adding features that touch multiple services

## Dependent Tasks
- .propel/context/tasks/us_001/task_005_define_contract_change_process.md (TASK_005)
- .propel/context/tasks/us_002/task_001_implement_api_versioning_convention.md (TASK_001)
- .propel/context/tasks/us_003/task_001_standardize_api_error_response_format.md (TASK_001)

## Impacted Components
- .github/workflows/contracts.yml
- scripts/validate_contracts.py
- contracts/README.md
- contracts/migrations/api_v1.md
- contracts/migrations/jobs_v1.md

## Implementation Plan
- Define a small CI-time rule for contract changes:
  - If `contracts/api/**` changes, require `contracts/migrations/api_v1.md` to be updated in the same PR.
  - If `contracts/jobs/**` changes, require `contracts/migrations/jobs_v1.md` to be updated in the same PR.
- Implement the rule as a GitHub Actions step that compares PR base to head and fails with a clear message.
- Extend `scripts/validate_contracts.py` to validate the migration-note files exist and are non-empty.
- Update `contracts/README.md` with a short “feature change checklist” emphasizing service boundaries + contract updates.
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | .github/workflows/contracts.yml | Add PR diff-based enforcement requiring migration notes when API/job contracts are modified |
| MODIFY | scripts/validate_contracts.py | Add basic checks that required migration-note files exist and are non-empty |
| MODIFY | contracts/README.md | Add a concise checklist covering service boundary placement and contract/migration updates |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://docs.github.com/actions/using-workflows/events-that-trigger-workflows#pull_request

## Build Commands
- python scripts/validate_contracts.py

## Implementation Validation Strategy
- Verify a PR that edits `contracts/api/v1/openapi.yaml` without updating `contracts/migrations/api_v1.md` fails CI.
- Verify a PR that edits `contracts/jobs/v1/job.schema.json` without updating `contracts/migrations/jobs_v1.md` fails CI.
- Verify `python scripts/validate_contracts.py` fails if migration-note files are missing/empty.

## Implementation Checklist
- [x] Add PR diff-based checks in `contracts.yml` for contract-to-migration-note coupling
- [x] Extend `validate_contracts.py` to validate migration-note presence and non-empty content
- [x] Update `contracts/README.md` with a short, enforceable “feature change checklist”
