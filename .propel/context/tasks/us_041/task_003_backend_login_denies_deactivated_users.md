# Task - [TASK_003]

## Requirement Reference
- User Story: [us_041]
- Story Location: [.propel/context/tasks/us_041/us_041.md]
- Acceptance Criteria: 
    - [Given a deactivated user, When they attempt to login, Then they are denied access with appropriate message.]

## Task Overview
Update the login endpoint behavior so that a user whose account is deactivated (represented by `Status != "Active"`, e.g., `Inactive`) is explicitly denied access with an appropriate message, without weakening credential-security guarantees.

## Dependent Tasks
- [US_041 TASK_002] (Deactivation endpoint establishes the “Inactive” status state)

## Impacted Components
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Update `/api/v1/auth/login` logic to return a clear denial message for deactivated users]

## Implementation Plan
- Current behavior blocks login early when `user.Status != "Active"`, returning `401 invalid_credentials`.
- Adjust flow to keep security properties while enabling a clearer message:
  - Continue to return `401 invalid_credentials` for invalid email and/or invalid password.
  - For a valid email + valid password:
    - If `Status != "Active"`, return `403` (`ApiErrorResults.Forbidden`) with:
      - code: `account_inactive` (or `account_deactivated`)
      - message: a user-friendly message indicating the account is deactivated and to contact an administrator
  - Keep lockout handling in place (locked users still receive the lockout response).

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Update login endpoint to return `403 account_inactive` when credentials are valid but user status is not Active |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/security/authentication/

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Manual/API] Create a user, set `Status = Inactive`, attempt login with correct password, and verify `403` with `account_inactive` message.
- [Security] Attempt login with wrong password and verify response remains `401 invalid_credentials`.
- [Regression] Confirm lockout behavior is unchanged.

## Implementation Checklist
- [x] Refactor login flow to validate password before enforcing non-active status denial
- [x] Return `403` with stable error code and clear message for inactive users
- [x] Ensure wrong-password and unknown-email flows still return `401 invalid_credentials`
- [x] Confirm account-locked path remains unchanged
