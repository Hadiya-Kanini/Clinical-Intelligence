# Task - TASK_001

## Requirement Reference
- User Story: us_002
- Story Location: .propel/context/tasks/us_002/us_002.md
- Acceptance Criteria: 
    - Given the Backend API exposes application endpoints, When an endpoint is implemented, Then its route is prefixed with `/api/v1/`.
    - Given a client sends a request to an unversioned route (e.g., `/auth/login`), When the API receives the request, Then it responds with a not found/unsupported route result and does not serve application functionality from unversioned paths.
    - Given a client sends a request to an unsupported version (e.g., `/api/v2/...`), When the API receives the request, Then it responds with a clear unsupported version error using the standardized error format.
    - Given the API publishes documentation, When Swagger UI is generated, Then the documented endpoints reflect the `/api/v1/` prefix.

## Task Overview
Implement and enforce a stable URL versioning convention for the Backend API by introducing a versioned route group under `/api/v1`, preventing application functionality from being served from unversioned paths, and returning a clear standardized error response for unsupported API versions.

## Dependent Tasks
- TASK_004

## Impacted Components
- Server/ClinicalIntelligence.Api/Program.cs
- contracts/api/v1/openapi.yaml
- scripts/validate_contracts.py

## Implementation Plan
- Establish a single canonical versioned route prefix for application endpoints:
  - Use ASP.NET Core minimal API route groups to define `var v1 = app.MapGroup("/api/v1");` and map all application endpoints under this group.
  - Keep operational endpoints (e.g., `/health`) explicitly unversioned if required by platform conventions, but ensure no application functionality is exposed from unversioned paths.
- Ensure unversioned application routes are not accidentally introduced:
  - Add a routing guardrail pattern (team convention + code review expectation) that all non-operational endpoints must be mapped through the `/api/v1` group.
  - Add at least one sample/versioned endpoint (e.g., `/api/v1/ping`) to make the convention concrete and visible in Swagger.
- Handle unsupported versions:
  - Add a small middleware (or endpoint-level handler) that detects requests starting with `/api/v` where the version is not `v1` and returns a standardized error response.
  - Use a consistent error format (recommendation: RFC7807 `ProblemDetails` via `Results.Problem(...)`), including a clear message such as "Unsupported API version" and the requested version.
- Align Swagger/OpenAPI output:
  - Ensure the Swagger document reflects `/api/v1/...` for versioned endpoints by mapping those endpoints with the prefix.
  - Update the committed contract (`contracts/api/v1/openapi.yaml`) to include the new versioned endpoint(s) and preserve `/health`.
- Update repository guardrails:
  - Update `scripts/validate_contracts.py` if needed to keep `/health` as an allowed exception and enforce `/api/v1` for all other paths.

**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Introduce `/api/v1` route group; map a sample v1 endpoint; add unsupported-version handling; ensure Swagger reflects versioned routes |
| MODIFY | contracts/api/v1/openapi.yaml | Add `/api/v1/...` path(s) for any new v1 endpoint(s) while preserving `/health` |
| MODIFY | scripts/validate_contracts.py | Ensure contract validation enforces `/api/v1` for all non-`/health` paths (update only if required) |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis
- https://learn.microsoft.com/aspnet/core/fundamentals/routing
- https://datatracker.ietf.org/doc/html/rfc7807

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- Verify all application endpoints are reachable under `/api/v1/...`.
- Verify unversioned application paths (e.g., `/auth/login`) are not mapped and return a not-found / unsupported-route response.
- Verify unsupported version requests (e.g., `/api/v2/ping`) return the standardized error format with a clear "unsupported version" message.
- Verify Swagger UI shows versioned endpoints with the `/api/v1/` prefix.
- Verify contract guardrails pass:
  - `python scripts/validate_contracts.py`

## Implementation Checklist
- [x] Add `/api/v1` route group and map versioned endpoints under it
- [x] Implement unsupported-version handling for `/api/v{N}` where `N != 1`
- [x] Add at least one v1 endpoint for validation and Swagger visibility
- [x] Update `contracts/api/v1/openapi.yaml` to reflect versioned endpoints
- [x] Run contract validation script and confirm expected behavior via manual HTTP calls
