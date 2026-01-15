# Task - [TASK_002]

## Requirement Reference
- User Story: [us_027]
- Story Location: [.propel/context/tasks/us_027/us_027.md]
- Acceptance Criteria: 
    - [Given any email is submitted for reset, When processed, Then the same generic "check your email" message is displayed.]
    - [Given an email that exists in the system, When reset is requested, Then the response is identical to non-existent emails.]
    - [Given response timing, When implemented, Then consistent response times prevent timing-based enumeration.]

## Task Overview
Add automated backend test coverage ensuring the forgot-password endpoint:
- does not allow account enumeration via response code/payload differences
- enforces a minimum response-time floor for syntactically valid requests to reduce timing-based enumeration

The goal is to validate the security contract while keeping tests deterministic and aligned with existing integration-test conventions.

## Dependent Tasks
- [US_024 TASK_002 - Backend forgot password endpoint (generic response)]
- [US_027 TASK_001 - Backend forgot password generic response + timing normalization]

## Impacted Components
- [CREATE/MODIFY | Server/ClinicalIntelligence.Api.Tests/Integration/ForgotPasswordNonEnumerationAndTimingTests.cs | Integration tests verifying response identity and minimum response time floor for `POST /api/v1/auth/forgot-password`]

## Implementation Plan
- Add a new integration test class using existing `WebApplicationFactory<Program>` patterns (see `LoginRateLimitingTests`, `AccountLockoutTests`).
- Add tests for non-enumeration:
  - Choose a known seeded email (e.g., seeded admin email used by existing auth tests).
  - Generate a random email unlikely to exist.
  - Call `POST /api/v1/auth/forgot-password` for both (syntactically valid) emails.
  - Assert:
    - both return `200 OK`
    - both responses have identical JSON shape and identical fields/values (e.g., `{"status":"ok"}` if that is the contract).
- Add test for timing floor (timing normalization):
  - Configure the app-under-test with a known minimum delay (e.g., 200ms) via configuration override in the test host.
  - Measure elapsed duration for a syntactically valid forgot-password request.
  - Assert elapsed time is >= configured minimum minus a small tolerance.
  - Keep the test scope small (single request or very small sample) to limit overall suite runtime.

## Current Project State
- Integration test class `ForgotPasswordNonEnumerationAndTimingTests.cs` created with 8 tests
- Tests verify response indistinguishability between existing and non-existing emails
- Tests verify minimum response time floor is enforced for syntactically valid requests
- Tests verify invalid input (400) is NOT subject to timing normalization
- All 8 tests passing

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE/MODIFY | Server/ClinicalIntelligence.Api.Tests/Integration/ForgotPasswordNonEnumerationAndTimingTests.cs | Add integration tests for response indistinguishability and minimum response-time floor |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests
- https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.stopwatch

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Automated] Run the test suite and confirm:
  - existing vs non-existing emails produce identical 200 responses
  - forgot-password request duration meets the configured minimum delay

## Implementation Checklist
- [x] Create integration test file `ForgotPasswordNonEnumerationAndTimingTests.cs`
- [x] Add test: existing vs non-existing email return identical 200 + identical payload
- [x] Add test: invalid input continues to return 400 (sanity check; optional if covered elsewhere)
- [x] Add timing-floor test using configuration override (e.g., 200ms minimum)
- [x] Keep assertions tolerant enough to avoid flakiness but strict enough to catch regressions
- [x] Ensure tests do not log/assert on any plain reset token values
