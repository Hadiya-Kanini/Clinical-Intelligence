# Task - [TASK_001]

## Requirement Reference
- User Story: [us_016]
- Story Location: [.propel/context/tasks/us_016/us_016.md]
- Acceptance Criteria: 
    - [Given a user account, When 5 consecutive failed login attempts occur, Then the account is locked for 30 minutes.]
    - [Given an account is locked, When 30 minutes pass, Then the account is automatically unlocked.]

## Task Overview
Update backend login lockout behavior to enforce a 30-minute lockout window after 5 consecutive failed login attempts, and ensure the account becomes usable again after the lockout window expires (automatic unlock behavior). Estimated effort: ~4-6 hours.

## Dependent Tasks
- [US_011 - Implement JWT authentication with HttpOnly cookies]
- [US_015 - Implement login rate limiting]

## Impacted Components
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Update lockout duration from 15 to 30 minutes and ensure lockout expiry resets counters appropriately]
- [MODIFY | Server/ClinicalIntelligence.Api/Domain/Models/User.cs | Reuse existing `FailedLoginAttempts` and `LockedUntil` fields (no schema changes expected)]

## Implementation Plan
- Update the lockout duration:
  - In `/api/v1/auth/login` (currently in `Program.cs`), change the lockout duration from 15 to 30 minutes.
- Implement robust automatic unlock behavior:
  - When evaluating a user with a `LockedUntil` value:
    - If `LockedUntil <= DateTime.UtcNow`, treat the lockout as expired.
    - Reset `FailedLoginAttempts` (e.g., set to `0`) and clear `LockedUntil` so the next login attempt behaves normally.
  - Ensure this reset is persisted.
- Consider interaction with IP rate limiting:
  - If `US_015` rate limiting rejects the request earlier (HTTP 429), ensure failed-attempt counters are not incremented (endpoint logic should not run).

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Update lockout duration to 30 minutes and reset counters when lockout window expires |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Manual] Trigger lockout through repeated failed logins, confirm lockout duration is 30 minutes.
- [Manual] Simulate lockout expiry (by manipulating `LockedUntil` in DB) and confirm login attempts are allowed again.

## Implementation Checklist
- [x] Change lockout duration constant/logic to 30 minutes
- [x] Add expired-lockout handling: reset `FailedLoginAttempts` and clear `LockedUntil`
- [x] Persist reset state to DB
- [x] Verify correct behavior when rate limiting returns 429 (no account attempt increments)
- [x] Validate unlock behavior after lockout window expiry
