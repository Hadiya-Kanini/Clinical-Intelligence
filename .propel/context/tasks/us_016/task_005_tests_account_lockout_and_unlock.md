# Task - [TASK_005]

## Requirement Reference
- User Story: [us_016]
- Story Location: [.propel/context/tasks/us_016/us_016.md]
- Acceptance Criteria: 
    - [Given a user account, When 5 consecutive failed login attempts occur, Then the account is locked for 30 minutes.]
    - [Given an account is locked, When a login attempt is made, Then the system returns an error indicating the account is locked with remaining lockout time.]
    - [Given an account is locked, When 30 minutes pass, Then the account is automatically unlocked.]
    - [Given an account lockout occurs, When the lockout is triggered, Then an ACCOUNT_LOCKED event is logged in the audit trail.]

## Task Overview
Add automated backend test coverage for account lockout, remaining-time response details, automatic unlock behavior, and audit trail logging. Estimated effort: ~6-8 hours.

## Dependent Tasks
- [TASK_001 - Backend account lockout policy 30 minutes]
- [TASK_002 - Backend account locked error with remaining time]
- [TASK_003 - Backend audit log ACCOUNT_LOCKED event]

## Impacted Components
- [CREATE/MODIFY | Server/ClinicalIntelligence.Api.Tests/* | Integration tests covering lockout threshold, locked response, unlock behavior, and audit logging]

## Implementation Plan
- Add integration test covering lockout threshold:
  - For a known test user (seeded admin or a test fixture user), submit invalid password 5 times.
  - Assert attempts prior to lock return the existing invalid-credentials behavior.
  - On the next attempt (or once lock is active), assert `403` with `code=account_locked`.
- Validate remaining-time details:
  - Assert response `error.details` contains `unlock_at:` (and/or `remaining_seconds:`) and is parseable.
- Validate unlock behavior without waiting 30 minutes:
  - Update the user record in DB to set `LockedUntil` in the past, then attempt login again and assert the locked response is gone.
- Validate audit trail:
  - After triggering lockout, query `audit_log_events` for `ACCOUNT_LOCKED`.

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE/MODIFY | Server/ClinicalIntelligence.Api.Tests/* | Add integration tests for lockout threshold, remaining-time details, unlock behavior, and audit log insertion |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Automated] Run test suite and confirm lockout tests are deterministic and do not sleep 30 minutes.

## Implementation Checklist
- [x] Add test for 5 failed attempts leading to lockout
- [x] Add test asserting `account_locked` response includes remaining time details
- [x] Add test that simulates lock expiry by setting `LockedUntil` in the past
- [x] Add test asserting audit log event `ACCOUNT_LOCKED` is written
- [x] Confirm tests coexist with IP rate limiting (skip/adjust if limiter blocks test)
