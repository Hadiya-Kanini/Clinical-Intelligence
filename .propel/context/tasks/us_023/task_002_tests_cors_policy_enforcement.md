# Task - [TASK_002]

## Requirement Reference
- User Story: [us_023]
- Story Location: [.propel/context/tasks/us_023/us_023.md]
- Acceptance Criteria: 
    - [Given a cross-origin request, When processed, Then only the configured frontend origin is allowed.]
    - [Given CORS configuration, When credentials are needed, Then Access-Control-Allow-Credentials is set to true.]
    - [Given preflight requests (OPTIONS), When received, Then appropriate CORS headers are returned.]
    - [Given an unauthorized origin, When a request is made, Then it is rejected with appropriate CORS error.]

## Task Overview
Add automated integration test coverage for the API CORS policy to verify that only configured origin(s) are allowed, credentialed requests are supported, preflight behavior is correct, and unapproved origins do not receive CORS allow headers. Estimated effort: ~4-6 hours.

## Dependent Tasks
- [TASK_001 - Backend strict CORS policy configuration]

## Impacted Components
- [CREATE/MODIFY | Server/ClinicalIntelligence.Api.Tests/Integration/* | Add integration tests for CORS headers for allowed vs disallowed origins, including preflight OPTIONS requests]

## Implementation Plan
- Add integration test fixture using the existing pattern:
  - Use `WebApplicationFactory<Program>` (already used across integration tests).
  - Create `HttpClient` via `_factory.CreateClient()`.
- Define test inputs:
  - Use a configured allowed origin value (via environment variable used by the new `CorsOptions` config).
  - Define a clearly disallowed origin (e.g., `https://evil.example`).
- Validate allowed-origin behavior (simple request):
  - Send `GET /health` (or another anonymous endpoint) with `Origin: <allowed-origin>`.
  - Assert response includes `Access-Control-Allow-Origin` matching the allowed origin.
  - Assert response includes `Access-Control-Allow-Credentials: true`.
- Validate preflight behavior:
  - Send `OPTIONS /api/v1/auth/me` (or `/api/v1/ping`) with headers:
    - `Origin: <allowed-origin>`
    - `Access-Control-Request-Method: GET`
    - `Access-Control-Request-Headers: content-type`
  - Assert response includes `Access-Control-Allow-Origin` and `Access-Control-Allow-Methods`.
  - Assert response includes `Access-Control-Allow-Headers` when request header is specified.
- Validate disallowed-origin behavior:
  - Send `GET /health` with `Origin: <disallowed-origin>`.
  - Assert `Access-Control-Allow-Origin` is absent.
  - Assert `Access-Control-Allow-Credentials` is absent.
  - Note: server typically still returns `200` for the endpoint; the browser blocks access due to missing CORS headers.
- Keep tests deterministic:
  - Do not rely on database availability (prefer `/health` and anonymous endpoints).
  - Avoid asserting exact status codes for OPTIONS if the hosting pipeline returns `200` vs `204`; focus on presence/absence of CORS headers.

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api.Tests/Integration/CorsPolicyIntegrationTests.cs | Integration tests validating allowed vs disallowed origins, credentials header behavior, and OPTIONS preflight CORS responses |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/security/cors
- https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Automated] Run the API test suite and confirm CORS tests pass with configured allowed origin(s).
- [Manual] Confirm a browser client can call the API with cookies from the configured frontend origin.

## Implementation Checklist
- [ ] Add new integration test file for CORS policy enforcement
- [ ] Add test for allowed origin (GET) returning correct `Access-Control-Allow-Origin`
- [ ] Add test for allowed origin (GET) returning `Access-Control-Allow-Credentials: true`
- [ ] Add test for allowed origin preflight OPTIONS returning appropriate allow headers
- [ ] Add test for disallowed origin returning no CORS allow headers
- [ ] Ensure tests are environment-configurable and do not require DB connectivity
