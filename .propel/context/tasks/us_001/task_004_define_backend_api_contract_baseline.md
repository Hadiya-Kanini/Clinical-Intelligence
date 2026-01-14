# Task - TASK_004

## Requirement Reference
- User Story: us_001
- Story Location: .propel/context/tasks/us_001/us_001.md
- Acceptance Criteria: 
    - Given the Web UI integrates with the Backend API, When the Web UI calls backend endpoints, Then the API contract is explicitly defined (Swagger UI with OpenAPI specification) and the UI uses only versioned endpoints.
    - Given a change is proposed to a contract (API schema or job schema), When a developer updates it, Then the change includes a versioning decision (backward compatible vs breaking) and documented migration notes.
    - Edge Case: What happens when the Web UI calls an endpoint that is not part of the published Swagger/OpenAPI contract?

## Task Overview
Establish the canonical Swagger UI documentation with OpenAPI specification for the Backend API and define how the Web UI must consume only versioned endpoints described by the published contract.

## Dependent Tasks
- TASK_002

## Impacted Components
- contracts/api/
- Server/ (installs Swashbuckle.AspNetCore, configures Swagger UI, publishes OpenAPI spec)
- app/ (consumes API contract)

## Implementation Plan
- Install and configure Swashbuckle.AspNetCore in the Backend API for Swagger UI generation.
- Define a versioned location for OpenAPI artifacts under `contracts/api/`.
- Configure Swagger UI endpoint (e.g., /swagger or /api/docs) in the Backend API.
- Establish the baseline OpenAPI specification for v1.
- Specify the contract consumption rule for the Web UI:
  - only use routes documented in the Swagger/OpenAPI specification
  - only use versioned endpoints
- Define expected behavior when the UI attempts to call undocumented endpoints (treated as defect and blocked via review/validation).
- Add migration notes guidance for changes to the API contract.

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | contracts/api/v1/openapi.yaml | Baseline v1 OpenAPI contract artifact |
| CREATE | contracts/api/v1/README.md | Explains how the contract is produced/consumed and versioning expectations |
| CREATE | contracts/migrations/api_v1.md | Migration notes and change log for API contract v1 |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://swagger.io/specification/
- https://github.com/domaindrivendev/Swashbuckle.AspNetCore

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- Ensure Swashbuckle.AspNetCore is installed in the Backend API project.
- Ensure Swagger UI is accessible at a defined endpoint (e.g., /swagger).
- Ensure there is a single canonical OpenAPI specification artifact in-repo under a versioned folder.
- Ensure UI integration policy is documented as "Swagger/OpenAPI-first" and "versioned endpoints only".

## Implementation Checklist
- [x] Install Swashbuckle.AspNetCore NuGet package in Backend API
- [x] Configure Swagger UI middleware in Backend API (Program.cs/Startup.cs)
- [x] Create `contracts/api/v1/` and store a baseline OpenAPI specification
- [x] Document how the Backend API publishes Swagger UI and how the UI consumes the contract
- [x] Define contract change process and required migration notes
