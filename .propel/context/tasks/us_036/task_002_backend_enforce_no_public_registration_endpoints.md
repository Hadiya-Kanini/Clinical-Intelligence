# Task - [TASK_002]

## Requirement Reference
- User Story: [us_036]
- Story Location: [.propel/context/tasks/us_036/us_036.md]
- Acceptance Criteria: 
    - [Given the API, When no public registration endpoint exists, Then attempts to register return 404.]
    - [Given user creation, When attempted, Then it is only possible through admin-authenticated endpoints.]

## Task Overview
Harden the backend API surface to ensure there is no public self-service registration capability. This task focuses on:
- verifying that common public registration endpoints do not exist (and return `404`)
- ensuring that user creation remains strictly admin-only via authenticated admin endpoints (owned by the admin user creation feature), with explicit negative tests to prevent accidental introduction of a public registration endpoint

## Dependent Tasks
- [US_035] (Admin-only endpoint protection and correct `401/403` semantics)
- [US_037] (Admin-only user creation endpoint exists; this task adds negative tests around public registration)

## Impacted Components
- [CREATE | Server/ClinicalIntelligence.Api.Tests/NoPublicRegistrationEndpointTests.cs | Integration tests asserting common registration endpoints return 404 and that user creation is not available without admin auth]

## Implementation Plan
- Add backend integration tests that assert `404 Not Found` for public registration attempts, covering common patterns:
  - `POST /api/v1/auth/register`
  - `POST /api/v1/auth/signup`
  - `POST /api/v1/register`
  - `POST /api/v1/signup`
  - `POST /api/v1/users` (if not part of admin surface)
- Add negative authorization tests (where the admin endpoint exists) to ensure user creation cannot be performed without admin authentication:
  - unauthenticated -> `401`
  - authenticated Standard -> `403`
  - authenticated Admin -> success (validated as part of US_037)
- Keep the behavior aligned with deny-by-default API principles:
  - do not introduce placeholder registration endpoints
  - rely on explicit admin-only endpoints for user provisioning

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api.Tests/NoPublicRegistrationEndpointTests.cs | Add integration tests to ensure public registration endpoints do not exist and return 404; add negative checks to confirm user creation remains admin-only when applicable |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html
- https://cheatsheetseries.owasp.org/cheatsheets/Authorization_Cheat_Sheet.html

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Tests] Run backend integration test suite and verify `NoPublicRegistrationEndpointTests` passes.
- [Security] Confirm there is no endpoint that allows unauthenticated user creation.

## Implementation Checklist
- [x] Add `NoPublicRegistrationEndpointTests` covering common registration endpoint paths and asserting `404`
- [x] If an admin user creation endpoint exists, add negative tests for unauthenticated and Standard user access (`401/403`)
- [x] Ensure tests are stable and do not rely on side effects (no user creation in negative-path tests)
- [x] Confirm no changes introduce a public registration route or handler
