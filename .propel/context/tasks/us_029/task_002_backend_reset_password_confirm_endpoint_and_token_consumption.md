# Task - [TASK_002]

## Requirement Reference
- User Story: [us_029]
- Story Location: [.propel/context/tasks/us_029/us_029.md]
- Acceptance Criteria: 
    - [Given a valid token, When the form is displayed, Then it includes new password and confirm password fields (FR-116b).]
    - [Given an expired or invalid token, When detected, Then an error message is shown with option to request new reset.]

## Task Overview
Implement the backend endpoint that accepts a password reset token and a new password, validates the token, updates the user password securely, and marks the token as used to enforce single-use behavior.

This task focuses on the reset completion step and requires atomic updates to prevent token reuse.

## Dependent Tasks
- [US_025 TASK_001 - Backend password reset token generation and storage]
- [US_029 TASK_001 - Backend reset password token validation endpoint]

## Impacted Components
- [CREATE | Server/ClinicalIntelligence.Api/Contracts/Auth/ResetPasswordRequest.cs | Request contract containing token + new password]
- [CREATE | Server/ClinicalIntelligence.Api/Services/Auth/IPasswordResetService.cs | Abstraction for applying password reset and consuming token]
- [CREATE | Server/ClinicalIntelligence.Api/Services/Auth/PasswordResetService.cs | Implementation that updates `User.PasswordHash` and sets `PasswordResetToken.UsedAt`]
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Add `POST /api/v1/auth/reset-password` endpoint and register reset service in DI]

## Implementation Plan
- Add request contract for reset completion:
  - include the reset token and new password
  - endpoint should not accept `confirmPassword` (frontend-only UX)
- Add application service (`IPasswordResetService`) responsible for:
  - validating token using the validator (or shared internal method)
  - loading the associated user via `UserId`
  - hashing the new password using bcrypt (consistent with login verification)
  - persisting updates in a transaction:
    - update `User.PasswordHash`
    - set `PasswordResetToken.UsedAt = DateTime.UtcNow`
- Add endpoint:
  - Route: `POST /api/v1/auth/reset-password`
  - On success: return `200` with a simple success payload
  - On invalid/expired/used token: return standardized error response using `ApiErrorResults`
- Security notes:
  - never log the token or the password
  - validate password policy server-side (minimum length; align with frontend policy)
  - ensure transaction prevents token reuse under concurrency

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api/Contracts/Auth/ResetPasswordRequest.cs | DTO for `POST /api/v1/auth/reset-password` containing token and new password |
| CREATE | Server/ClinicalIntelligence.Api/Services/Auth/IPasswordResetService.cs | Interface for applying password reset and consuming the reset token atomically |
| CREATE | Server/ClinicalIntelligence.Api/Services/Auth/PasswordResetService.cs | Implementation that updates password hash (bcrypt) and marks token used within a transaction |
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Add reset-password endpoint and DI registration |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis
- https://cheatsheetseries.owasp.org/cheatsheets/Forgot_Password_Cheat_Sheet.html

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Manual/API] Validate behavior:
  - valid token + valid password -> `200` and token becomes used
  - same token reused -> error response
  - expired token -> error response
  - malformed token -> `400`
- [Security] Confirm password/token are not logged and bcrypt hashing is used for `User.PasswordHash`.

## Implementation Checklist
- [x] Add `ResetPasswordRequest` contract
- [x] Add `IPasswordResetService` abstraction
- [x] Implement `PasswordResetService` with transactional update (password + token consumption)
- [x] Add `POST /api/v1/auth/reset-password` endpoint in `Program.cs`
- [x] Return standardized error responses for invalid/expired/used token
- [x] Ensure token and password are never logged
