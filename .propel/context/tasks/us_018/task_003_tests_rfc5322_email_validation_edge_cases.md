# Task - [TASK_003]

## Requirement Reference
- User Story: [us_018] (extracted from input)
- Story Location: [.propel/context/tasks/us_018/us_018.md]
- Acceptance Criteria: 
    - [Given a user enters an email address, When validation runs, Then RFC 5322 compliant regex patterns are used.]
    - [Given an invalid email format, When validation fails, Then a clear error message indicates the specific issue.]
    - [Given edge-case valid emails (e.g., user+tag@domain.com), When entered, Then they are accepted as valid.]

## Task Overview
Add automated test coverage to ensure the RFC 5322 email validator behavior remains stable over time across both frontend and backend, with focus on common valid formats, edge cases, and clear invalid-format failures.

## Dependent Tasks
- [TASK_001 - Backend RFC 5322 email validation]
- [TASK_002 - Frontend RFC 5322 email validation shared utility]

## Impacted Components
- [CREATE/MODIFY | app/src/lib/validation/email.test.ts | Unit tests for frontend email validator]
- [CREATE/MODIFY | Server/ClinicalIntelligence.Api.Tests/* | Unit/integration tests validating backend email validator and login invalid email behavior]

## Implementation Plan
- Frontend tests (Vitest):
  - Add a unit test file for the shared email validator.
  - Test cases should include:
    - Valid: `user+tag@domain.com`, `first.last@domain.com`, `user@sub.domain.com`
    - Invalid: missing `@`, missing domain, whitespace, trailing dot, etc.
  - Assert error messages are triggered in UI validators where applicable (lightweight component tests optional; unit tests for validator are mandatory).
- Backend tests (xUnit):
  - Add tests for `EmailValidation` helper:
    - Valid and invalid cases matching frontend set.
  - Add an integration test for `/api/v1/auth/login`:
    - Invalid email format returns `400` with stable `details` (e.g., `email:invalid_format`).
    - Valid format proceeds past validation (may still return invalid credentials, but not `400`).
- Keep tests deterministic:
  - Avoid locale/timezone dependencies.
  - Ensure consistent normalization expectations (trim behavior).

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE/MODIFY | app/src/lib/validation/email.test.ts | Unit tests for RFC 5322 email validation utility |
| CREATE/MODIFY | Server/ClinicalIntelligence.Api.Tests/* | Backend tests for email validator + login invalid email response |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://vitest.dev/
- https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests

## Build Commands
- npm --prefix .\app run test
- dotnet test .\Server\ClinicalIntelligence.Api.Tests\ClinicalIntelligence.Api.Tests.csproj

## Implementation Validation Strategy
- [Automated] CI-style run of frontend unit tests and backend test suite.
- [Automated] Ensure edge-case valid emails are accepted by both validators.

## Implementation Checklist
- [x] Add frontend unit tests for `app/src/lib/validation/email.ts`
- [x] Add backend unit tests for `EmailValidation`
- [x] Add backend integration test for `/api/v1/auth/login` invalid email => `400` + stable details
- [x] Verify `user+tag@domain.com` passes in both layers
- [x] Add at least 5 invalid format cases to prevent regex regressions
