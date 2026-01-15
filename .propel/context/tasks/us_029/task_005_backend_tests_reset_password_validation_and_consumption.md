# Task - [TASK_005]

## Requirement Reference
- User Story: [us_029]
- Story Location: [.propel/context/tasks/us_029/us_029.md]
- Acceptance Criteria: 
    - [Given I click the reset link, When the page loads, Then my token is validated before showing the form.]
    - [Given an expired or invalid token, When detected, Then an error message is shown with option to request new reset.]

## Task Overview
Add backend integration test coverage for the reset-password token validation endpoint and the reset-password confirm endpoint to ensure token lifecycle security: invalid/expired/used tokens are rejected, valid tokens are accepted, and tokens are consumed on successful password reset.

## Dependent Tasks
- [US_029 TASK_001 - Backend reset password token validation endpoint]
- [US_029 TASK_002 - Backend reset password confirm endpoint and token consumption]
- [US_025 TASK_001 - Backend password reset token generation and storage] (for generating realistic token records)

## Impacted Components
- [CREATE | Server/ClinicalIntelligence.Api.Tests/Integration/ResetPasswordFlowTests.cs | Integration tests for token validation and password reset completion]

## Implementation Plan
- Add integration tests (via `WebApplicationFactory<Program>`) that validate:
  - `GET /api/v1/auth/reset-password/validate`:
    - missing token -> `400`
    - invalid token -> standardized error response
    - expired token -> standardized error response
    - used token -> standardized error response
    - valid token -> `200`
  - `POST /api/v1/auth/reset-password`:
    - valid token + valid password -> `200`
    - token is marked used (cannot be reused)
    - user password hash changes (bcrypt format/verification)
- Ensure tests follow existing patterns:
  - use standardized error response assertions (see `ErrorResponseIntegrationTests`)
  - avoid leaking secrets in assertions/log output

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api.Tests/Integration/ResetPasswordFlowTests.cs | Integration tests for reset-password token validation and reset completion with token consumption |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Automated] Run backend test suite and confirm new tests pass consistently.

## Implementation Checklist
- [x] Add tests for `GET /api/v1/auth/reset-password/validate` missing/invalid/expired/used/valid scenarios
- [x] Add tests for `POST /api/v1/auth/reset-password` success and error scenarios
- [x] Assert used tokens cannot be reused
- [x] Assert password hash changes and remains verifiable using bcrypt
- [x] Reuse standardized error response assertions (shape and stable error codes)
