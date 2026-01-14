# Task - TASK_002

## Requirement Reference
- User Story: us_001
- Story Location: .propel/context/tasks/us_001/us_001.md
- Acceptance Criteria: 
    - Given a new developer opens the repository, When they inspect the solution structure and integration contracts, Then the Web UI, Backend API, and AI Worker boundaries are clearly defined and each has an explicit contract for how it communicates with the others.
    - Given a change is proposed to a contract (API schema or job schema), When a developer updates it, Then the change includes a versioning decision (backward compatible vs breaking) and documented migration notes.

## Task Overview
Define the canonical structure for integration contracts (Backend API contract and worker job contract), including versioned storage layout and the minimum guardrails that prevent "out of contract" integration.

## Dependent Tasks
- TASK_001

## Impacted Components
- contracts/ (new)
- app/ (consumes API contract)
- Server/ (publishes API contract via Swagger UI, produces job messages)
- worker/ (consumes job contract)

## Implementation Plan
- Define a `contracts/` directory layout that supports versioned contracts for:
  - Backend API (Swagger UI with OpenAPI specification)
  - Backend-to-Worker jobs (message schema)
- Add a lightweight contract governance document describing:
  - Where canonical contracts live
  - How versions are incremented
  - What constitutes a backward compatible vs breaking change
  - Required migration notes for breaking changes
- Add a minimal "no out-of-contract" rule that can be enforced during development (e.g., contract artifacts treated as source-of-truth).

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | contracts/README.md | Explains contract ownership, boundaries, and versioning rules |
| CREATE | contracts/api/ | Folder for versioned Swagger/OpenAPI artifacts |
| CREATE | contracts/jobs/ | Folder for versioned job message schemas |
| CREATE | contracts/migrations/ | Folder for migration notes per contract version change |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://semver.org/

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- Confirm contract artifacts are in a single canonical location and are versioned.
- Confirm the repository documents the only allowed integrations:
  - Web UI -> Backend API via versioned endpoints and Swagger/OpenAPI
  - Backend API -> AI Worker via versioned job message schema

## Implementation Checklist
- [ ] Create `contracts/` folder structure for API and jobs
- [ ] Add `contracts/README.md` describing boundaries, ownership, and versioning
- [ ] Add a `contracts/migrations/` location and document required migration notes
