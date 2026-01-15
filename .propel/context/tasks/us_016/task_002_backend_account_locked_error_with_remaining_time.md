# Task - [TASK_002]

## Requirement Reference
- User Story: [us_016]
- Story Location: [.propel/context/tasks/us_016/us_016.md]
- Acceptance Criteria: 
    - [Given an account is locked, When a login attempt is made, Then the system returns an error indicating the account is locked with remaining lockout time.]

## Task Overview
Update the backend locked-account login response to return a clear error and include the **remaining lockout time** in a machine-readable way so the UI can display an accurate unlock timeframe (UXR-009). Estimated effort: ~3-5 hours.

## Dependent Tasks
- [TASK_001 - Backend account lockout policy 30 minutes]

## Impacted Components
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Enhance the `account_locked` response to include remaining lockout time]
- [MODIFY | Server/ClinicalIntelligence.Api/Results/ApiErrorResults.cs | Reuse standardized error envelope; ensure `details` can carry remaining time]
- [MODIFY | Server/ClinicalIntelligence.Api/Contracts/ApiErrorResponse.cs | Reuse current schema (`error.code`, `error.message`, `error.details`); no schema changes expected]

## Implementation Plan
- When `LockedUntil > DateTime.UtcNow` in `/api/v1/auth/login`:
  - Return an error code of `account_locked`.
  - Include remaining lockout time:
    - Prefer a stable detail item such as `unlock_at:<ISO-8601 UTC timestamp>` (e.g., `unlock_at:2026-01-15T12:34:56Z`).
    - Optionally include `remaining_seconds:<int>` for easier UI display.
  - Ensure the top-level `message` is safe and non-sensitive (do not leak whether the email exists beyond the existing behavior).
- Align status code:
  - Keep using `403 Forbidden` if that is the existing contract for locked accounts, but ensure the error details meet the UX requirement.

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Return `account_locked` error including remaining lockout timeframe in `error.details` |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://datatracker.ietf.org/doc/html/rfc3339

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Manual] Attempt login while `LockedUntil` is in the future and verify response includes:
  - `code = account_locked`
  - `details` containing `unlock_at:` and/or `remaining_seconds:`
- [Manual] Confirm that after expiry, this locked response no longer occurs.

## Implementation Checklist
- [x] Define the response detail format (`unlock_at:` and/or `remaining_seconds:`)
- [x] Update locked-account branch to populate details consistently
- [x] Ensure error message remains generic and safe
- [x] Confirm `details` are present even when empty in other cases (schema consistency)
- [x] Validate UI can parse the remaining time data
