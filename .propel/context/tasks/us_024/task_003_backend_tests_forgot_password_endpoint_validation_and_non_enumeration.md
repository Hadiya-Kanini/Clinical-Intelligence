# Task - [TASK_003]

## Requirement Reference
- User Story: [us_024]
- Story Location: [.propel/context/tasks/us_024/us_024.md]
- Acceptance Criteria: 
    - [Given I'm on the Forgot Password page, When I enter my email and submit, Then the form validates email format.]
    - [Given a valid email is submitted, When processed, Then a confirmation message is displayed (FR-115b).]

## Task Overview
Add automated backend test coverage for the forgot-password endpoint behavior, focusing on:
- request validation (required + format),
- non-enumeration (same response for existing vs non-existing emails),
- response contract stability for the frontend.

## Dependent Tasks
- [TASK_002 - Backend forgot password endpoint (generic response)]

## Impacted Components
- [CREATE/MODIFY | Server/ClinicalIntelligence.Api.Tests/* | Integration tests for `POST /api/v1/auth/forgot-password`]

## Implementation Plan
- Add integration tests for validation:
  - Missing email -> `400` with `invalid_input` (or equivalent) code.
  - Invalid email format -> `400`.
- Add integration test for non-enumeration:
  - Use a known seeded email (static admin) and a random email.
  - POST both and assert:
    - both return `200`
    - both have the same response body shape (no differences that leak user existence)
- Add a contract test to ensure response stays stable (e.g., `status` field exists if used).

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE/MODIFY | Server/ClinicalIntelligence.Api.Tests/Integration/ForgotPasswordTests.cs | Add integration tests for validation + non-enumeration behavior |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Automated] Run test suite and confirm forgot-password tests are deterministic.

## Implementation Checklist
- [ ] Add test: missing email returns 400
- [ ] Add test: invalid email format returns 400
- [ ] Add test: existing email returns 200
- [ ] Add test: non-existing email returns 200
- [ ] Assert 200 responses are indistinguishable (no user enumeration)
- [ ] Assert response contract shape is stable for frontend integration
