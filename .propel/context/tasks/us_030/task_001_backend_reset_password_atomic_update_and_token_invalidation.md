# Task - [TASK_001]

## Requirement Reference
- User Story: [us_030]
- Story Location: [.propel/context/tasks/us_030/us_030.md]
- Acceptance Criteria: 
    - [Given I submit a valid new password, When processed, Then my password hash is updated in the database.]
    - [Given successful password update, When completed, Then the reset token is immediately invalidated (FR-009r).]
    - [Given token invalidation, When the same token is used again, Then it is rejected as invalid.]

## Task Overview
Harden the backend `POST /api/v1/auth/reset-password` flow to guarantee that password update and reset-token invalidation happen atomically and remain correct under concurrency.

This task focuses on ensuring the token cannot be reused (even with concurrent requests) and that the token is not consumed if the password update fails after token validation.

## Dependent Tasks
- [US_029 TASK_002 - Backend reset password confirm endpoint and token consumption]

## Impacted Components
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Make reset-password processing atomic and concurrency-safe (single-use enforcement) while preserving existing response contracts]

## Implementation Plan
- Confirm current implementation behavior:
  - token lookup uses `TokenHash` and rejects `UsedAt != null` tokens
  - password update and setting `UsedAt` are saved together
- Add explicit atomicity and concurrency protection for token consumption:
  - Wrap the reset-password processing in an explicit database transaction.
  - Ensure token consumption is performed as an atomic operation at the database level (e.g., conditional update where `UsedAt == null` and token is still valid).
  - If the token cannot be consumed (affected rows = 0), return the existing unauthorized error (`invalid_token`) without changing the password.
- Ensure rollback safety:
  - If any persistence error occurs after token validation (DB unavailable / save fails), roll back the transaction so the token is not left in a consumed state without the password update.
- Keep security invariants:
  - Do not log reset token or password.
  - Preserve existing error codes and payload shapes used by the frontend (`invalid_token`, `token_expired`, `password_requirements_not_met`).

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Update `POST /api/v1/auth/reset-password` to perform password update + token invalidation atomically and ensure single-use behavior holds under concurrent requests |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://cheatsheetseries.owasp.org/cheatsheets/Forgot_Password_Cheat_Sheet.html
- https://learn.microsoft.com/en-us/ef/core/saving/transactions

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Manual/API] Reset password with a valid token:
  - returns `200`
  - password hash updated
  - token becomes invalid immediately
- [Security] Attempt to reuse the same token:
  - returns `401` (`invalid_token`)
- [Concurrency] Send two reset requests concurrently with the same token:
  - only one request succeeds
  - the other is rejected as invalid
- [Reliability] Simulate DB failure during reset and confirm token is not consumed without a successful password update.

## Implementation Checklist
- [x] Wrap reset-password persistence operations in an explicit transaction
- [x] Implement atomic token consumption (conditional update where `UsedAt == null` and token is valid)
- [x] Ensure password update and token invalidation commit together (rollback on failure)
- [x] Preserve existing error codes/response shapes consumed by the frontend
- [x] Ensure secrets are not logged (no token, no password)
- [x] Validate the flow under concurrent reset attempts using the same token
