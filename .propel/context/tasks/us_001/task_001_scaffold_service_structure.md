# Task - TASK_001

## Requirement Reference
- User Story: us_001
- Story Location: .propel/context/tasks/us_001/us_001.md
- Acceptance Criteria: 
    - Given the repository is at initial scaffolding stage, When the baseline structure is created, Then separate top-level components exist for:
      - Web UI (React)
      - Backend API (.NET)
      - AI Worker (Python)
    - Given a new developer opens the repository, When they inspect the solution structure and integration contracts, Then the Web UI, Backend API, and AI Worker boundaries are clearly defined and each has an explicit contract for how it communicates with the others.

## Task Overview
Create the baseline repository structure that enforces clear service boundaries between Web UI, Backend API, and AI Worker. Establish the canonical top-level directories and minimal build scaffolding per service so teams can work independently without cross-service coupling.

## Dependent Tasks
- []

## Impacted Components
- app/ (React Web UI)
- Server/ (ASP.NET Core Backend API)
- worker/ (Python AI Worker)
- contracts/ (integration contracts root)

## Implementation Plan
- Create top-level service directories (`app/`, `Server/`, `worker/`) and ensure each is independently buildable/runable.
- Add baseline solution/project scaffolding for each service using standard tooling for the stack.
- Add minimal, explicit README at the root of each service describing its responsibility and the allowed integration points.
- Ensure no service directly depends on another service’s internal code (only via contracts).

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | app/ | Web UI service root directory |
| CREATE | Server/ | Backend API service root directory |
| CREATE | worker/ | AI Worker service root directory |
| CREATE | contracts/ | Root directory for versioned integration contracts |
| CREATE | app/README.md | Describes Web UI boundary and allowed integrations |
| CREATE | Server/README.md | Describes Backend API boundary and allowed integrations |
| CREATE | worker/README.md | Describes AI Worker boundary and allowed integrations |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/?view=aspnetcore-8.0
- https://fastapi.tiangolo.com/
- https://vite.dev/

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- Verify service boundaries via repository structure (no cross-imports/cross-references between services).
- Verify each service can be built independently using its standard build tooling.

## Implementation Checklist
- [ ] Create `app/`, `Server/`, `worker/`, and `contracts/` directories
- [ ] Add a short `README.md` in each service describing boundaries and allowed integrations
- [ ] Confirm each service can run/build in isolation
