# Task - TASK_003

## Requirement Reference
- User Story: us_001
- Story Location: .propel/context/tasks/us_001/us_001.md
- Acceptance Criteria: 
    - Given the system uses asynchronous processing, When the Backend API enqueues work for the AI Worker, Then the job message schema is defined and versioned (fields, required/optional, status transitions) and is the only supported integration for background processing.
    - Given a change is proposed to a contract (API schema or job schema), When a developer updates it, Then the change includes a versioning decision (backward compatible vs breaking) and documented migration notes.
    - Edge Case: What happens when the AI Worker receives a job payload missing required fields (e.g., document_id)?
    - Edge Case: How does the Backend API handle an unknown/unsupported job schema version?

## Task Overview
Define a versioned, explicit job message schema for Backend API -> AI Worker processing through RabbitMQ, including required/optional fields and status/state transitions. Document how producer and consumer handle invalid payloads and unsupported schema versions.

## Dependent Tasks
- TASK_002

## Impacted Components
- contracts/jobs/
- Server/ (produces messages)
- worker/ (consumes messages)

## Implementation Plan
- Define the canonical job message schema fields and constraints (required vs optional).
- Define a schema version field and allowed versions.
- Define allowed job status transitions (e.g., Pending -> Processing -> Completed/Failed/Validation_Failed).
- Specify producer behavior for schema versioning and consumer behavior for:
  - missing required fields
  - unknown schema version
- Store the schema in a versioned location under `contracts/jobs/`.
- Add migration notes guidance when schema changes.

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | contracts/jobs/v1/job.schema.json | Canonical JSON schema for v1 job messages |
| CREATE | contracts/jobs/v1/README.md | Explains v1 fields, required/optional, and status transitions |
| CREATE | contracts/migrations/jobs_v1.md | Migration notes and change log for job contract v1 |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://www.rabbitmq.com/docs/dlx

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- Validate schema is machine-readable and versioned.
- Validate edge-case behavior is explicitly defined for:
  - missing required fields
  - unsupported schema versions
- Ensure Background processing integration is described as “contract-only” (no direct calls between services).

## Implementation Checklist
- [ ] Create `contracts/jobs/v1/` and define a v1 schema artifact
- [ ] Define required fields (including `document_id`) and version field
- [ ] Define allowed job status values and status transition rules
- [ ] Document producer/consumer behavior for invalid payloads and unknown versions
- [ ] Add migration notes for future schema changes
