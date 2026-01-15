# Task - [TASK_004]

## Requirement Reference
- User Story: [us_015]
- Story Location: [.propel/context/tasks/us_015/us_015.md]
- Acceptance Criteria: 
    - [Given an IP address makes login attempts, When 5 attempts are made within 1 minute, Then subsequent attempts return HTTP 429 Too Many Requests.]
    - [Given rate limit is exceeded, When HTTP 429 is returned, Then the response includes a Retry-After header indicating when to retry.]
    - [Given rate limiting is active, When the rate limit window expires, Then the IP can attempt login again.]
    - [Given a rate limit event occurs, When the limit is exceeded, Then the event is logged in the audit trail (RATE_LIMIT_EXCEEDED).]

## Task Overview
Add automated test coverage for login rate limiting behavior, including `429`, `Retry-After`, reset behavior, and audit logging. Estimated effort: ~6-8 hours.

## Dependent Tasks
- [TASK_001 - Backend add login rate limiter policy]
- [TASK_002 - Backend Retry-After and error body]
- [TASK_003 - Backend audit log RATE_LIMIT_EXCEEDED]

## Impacted Components
- [CREATE/MODIFY | Server/ClinicalIntelligence.Api.Tests/* | Integration tests exercising `/api/v1/auth/login` rate limiting]

## Implementation Plan
- Add an integration test that performs repeated login calls from the same client/IP:
  - Send 6 login attempts within the configured window.
  - Assert attempts 1-5 are not rate-limited and attempt 6 returns `429`.
  - Assert `Retry-After` header exists on the 429 response.
- Make the window testable without waiting 60 seconds:
  - Introduce configuration override for test runs (e.g., env var for window seconds) so tests can use a small window (e.g., 1-2 seconds) while production defaults remain 60 seconds.
  - After the window expires, attempt login again and assert it is no longer rate-limited.
- Add audit trail assertion:
  - When DB is available, query `audit_log_events` for `RATE_LIMIT_EXCEEDED` after triggering 429.

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE/MODIFY | Server/ClinicalIntelligence.Api.Tests/* | Add tests verifying 429 + Retry-After + reset behavior + audit logging |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Automated] Run test suite and confirm rate limit tests are deterministic and do not require waiting 60 seconds.

## Implementation Checklist
- [x] Add integration test: 6 login attempts => 429 on attempt 6
- [x] Assert `Retry-After` header exists for 429
- [x] Add deterministic test window override mechanism for tests
- [x] Add test: after window expires, login is allowed again
- [x] Add test: audit log event exists with `ActionType = RATE_LIMIT_EXCEEDED` (when DB is available)
- [x] Confirm existing authentication tests are not broken by limiter
