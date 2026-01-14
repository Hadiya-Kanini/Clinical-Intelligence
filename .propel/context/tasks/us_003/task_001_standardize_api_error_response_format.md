# Task - TASK_001

## Requirement Reference
- User Story: us_003
- Story Location: .propel/context/tasks/us_003/us_003.md
- Acceptance Criteria: 
    - Given a request fails due to invalid input, When the API returns a 4xx response (e.g., 400), Then the response body follows:
      - `error.code` as a stable string identifier
      - `error.message` as a human-readable summary
      - `error.details` as an array (empty if not applicable)
    - Given a request is unauthorized or forbidden, When the API returns 401/403, Then it returns the same standardized error shape.
    - Given a request exceeds rate limits, When the API returns 429, Then it returns the same standardized error shape.
    - Given an unexpected server error occurs, When the API returns 5xx, Then it returns the same standardized error shape and does not leak sensitive configuration values or stack traces.

## Task Overview
Standardize backend API error responses across all endpoints and middleware to return a consistent JSON payload:
`{ "error": { "code": "string", "message": "string", "details": [] } }`.

This task introduces a reusable error contract type and centralized helpers/middleware so:
- 4xx and 5xx responses have a consistent shape
- error codes are stable and machine-actionable for frontend handling
- unexpected exceptions return sanitized errors (no stack traces/secrets)

## Dependent Tasks
- .propel/context/tasks/us_002/task_001_implement_api_versioning_convention.md (TASK_001)

## Impacted Components
- Server/ClinicalIntelligence.Api/Program.cs
- contracts/api/v1/openapi.yaml

## Implementation Plan
- Define the standardized error contract:
  - Introduce an API contract type representing `{ error: { code, message, details } }`.
  - Keep `error.details` consistently as an array (empty array when not applicable).
- Implement consistent error response creation:
  - Add a small helper (extension methods or factory) to create `IResult` for error responses (400/401/403/404/409/429/500).
  - Ensure all error responses set `Content-Type: application/json`.
- Centralize exception handling:
  - Add an exception-handling middleware that catches unhandled exceptions and returns the standardized 500 error payload.
  - Ensure the middleware does not include stack traces or configuration values in the response body.
- Standardize existing error paths:
  - Update the existing unsupported API version middleware to return the standardized error payload (replacing the current RFC7807 `Results.Problem(...)` payload).
  - Ensure any future endpoints use the helper instead of ad-hoc anonymous error objects.
- Align OpenAPI contract:
  - Add reusable `components/schemas` entries for the standardized error shape.
  - Add representative error responses (4xx/5xx) for documented endpoints (at minimum `/api/v1/ping`, plus any globally-applied errors relevant to current endpoints).
- Add minimal validation coverage (as applicable to current minimal API):
  - If/when endpoints introduce input validation, ensure validation failures populate `error.details` with multiple issues.

**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api/Contracts/ApiErrorResponse.cs | Add a shared error response contract `{ error: { code, message, details } }` (e.g., record types) |
| CREATE | Server/ClinicalIntelligence.Api/Results/ApiErrorResults.cs | Add helper(s) to generate standardized error `IResult` responses with stable codes and details array |
| CREATE | Server/ClinicalIntelligence.Api/Middleware/ApiExceptionMiddleware.cs | Catch unhandled exceptions and return standardized 500 response without leaking sensitive details |
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Register/use exception middleware; replace unsupported-version `Results.Problem(...)` with standardized error result |
| MODIFY | contracts/api/v1/openapi.yaml | Add `components/schemas` for standardized error and attach common error responses to endpoints |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/aspnet/core/fundamentals/error-handling
- https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis
- https://owasp.org/www-project-api-security/

## Build Commands
- dotnet build Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj
- python scripts/validate_contracts.py

## Implementation Validation Strategy
- Verify standardized error shape for unsupported version:
  - Call `/api/v2/ping` and assert response body matches `{ "error": { "code", "message", "details" } }`.
  - Assert `error.details` is an array.
- Verify standardized error shape for unexpected exception:
  - Temporarily add (or use an existing) route that throws and confirm 500 response matches standardized shape.
  - Confirm response does not contain stack trace lines, connection strings, or environment variable values.
- Verify contract alignment:
  - Run `python scripts/validate_contracts.py` and ensure it passes.
  - Ensure `contracts/api/v1/openapi.yaml` includes standardized error schema and references in response sections.

## Implementation Checklist
- [ ] Add `ApiErrorResponse` contract type with `code`, `message`, and `details` array
- [ ] Add `ApiErrorResults` helper to create consistent error `IResult`s
- [ ] Implement `ApiExceptionMiddleware` and register it early in the pipeline
- [ ] Update unsupported-version response to use standardized error payload
- [ ] Update OpenAPI contract with standardized error schema and endpoint response references
- [ ] Build backend and run contract validation script
