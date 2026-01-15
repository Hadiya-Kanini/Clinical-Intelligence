# Task - [TASK_001]

## Requirement Reference
- User Story: [us_029]
- Story Location: [.propel/context/tasks/us_029/us_029.md]
- Acceptance Criteria: 
    - [Given I click the reset link in email, When the page loads, Then the token is validated before displaying the form (FR-116a).]
    - [Given an expired or invalid token, When detected, Then an error message is shown with option to request new reset.]

## Task Overview
Implement a backend API capability to validate password reset tokens so the frontend can confirm the token is still valid (not expired, not already used, and well-formed) before showing the reset password form.

This task creates a dedicated validation endpoint and a reusable validation service that encapsulates the rules around token lifecycle checks.

## Dependent Tasks
- [US_025 TASK_001 - Backend password reset token generation and storage]

## Impacted Components
- [CREATE | Server/ClinicalIntelligence.Api/Contracts/Auth/ValidateResetPasswordTokenResponse.cs | Response contract for token validation result]
- [CREATE | Server/ClinicalIntelligence.Api/Services/Auth/IPasswordResetTokenValidator.cs | Abstraction for validating reset tokens (hash compare, expiry, used-state)]
- [CREATE | Server/ClinicalIntelligence.Api/Services/Auth/PasswordResetTokenValidator.cs | Implementation that validates reset tokens against `PasswordResetTokens` table]
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Add `GET /api/v1/auth/reset-password/validate` endpoint and register validator in DI]

## Implementation Plan
- Add a validator abstraction (`IPasswordResetTokenValidator`) responsible for:
  - rejecting missing/malformed token inputs
  - hashing the provided token and performing lookup against `ApplicationDbContext.PasswordResetTokens`
  - determining validity based on:
    - existence of matching token hash
    - `ExpiresAt` in UTC (token must not be expired)
    - `UsedAt == null` (token must be unused)
- Add a minimal API endpoint:
  - Route: `GET /api/v1/auth/reset-password/validate?token=...`
  - On valid token: return `200` with a success response contract indicating validity
  - On invalid/expired/used token: return a standardized error response using `ApiErrorResults` with a stable code
- Security notes:
  - Do not log the raw token
  - Use parameterized EF queries (default) and avoid timing-sensitive branching beyond required checks

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api/Contracts/Auth/ValidateResetPasswordTokenResponse.cs | DTO describing whether the reset token is valid (and optionally the reason when invalid) |
| CREATE | Server/ClinicalIntelligence.Api/Services/Auth/IPasswordResetTokenValidator.cs | Interface for validating reset tokens without exposing persistence details to endpoints |
| CREATE | Server/ClinicalIntelligence.Api/Services/Auth/PasswordResetTokenValidator.cs | Implementation that validates token hash existence, expiry, and used-state |
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Add `GET /api/v1/auth/reset-password/validate` and DI registration for validator |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis
- https://learn.microsoft.com/en-us/ef/core/

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Manual/API] Call the validation endpoint with:
  - missing token -> `400`
  - malformed token -> `400`
  - expired token -> error response
  - used token -> error response
  - valid token -> `200` success response
- [Security] Confirm logs do not contain raw token values.

## Implementation Checklist
- [x] Add response contract for token validation result
- [x] Add `IPasswordResetTokenValidator` abstraction
- [x] Implement validator using `ApplicationDbContext.PasswordResetTokens` and UTC expiry checks
- [x] Add `GET /api/v1/auth/reset-password/validate` endpoint
- [x] Use standardized error responses via `ApiErrorResults`
- [x] Confirm raw tokens are never logged
