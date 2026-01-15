# Task - [TASK_003]

## Requirement Reference
- User Story: [us_025]
- Story Location: [.propel/context/tasks/us_025/us_025.md]
- Acceptance Criteria: 
    - [Given a password reset request, When a token is generated, Then it uses cryptographically secure random generation.]
    - [Given a reset token, When created, Then it has a 1-hour expiration time.]
    - [Given a reset token, When stored, Then only the hash is persisted (not the plain token).]
    - [Given multiple reset requests, When a new token is generated, Then previous tokens for that user are invalidated.]

## Task Overview
Add automated test coverage validating that password reset token generation and forgot-password integration meet the security and lifecycle requirements: secure generation, 1-hour expiry, hash-only storage, and invalidation of prior tokens.

## Dependent Tasks
- [US_024 TASK_002 - Backend forgot password endpoint (generic response)]
- [US_025 TASK_001 - Backend password reset token generation and storage]
- [US_025 TASK_002 - Backend forgot password generates token and invalidates previous]

## Impacted Components
- [CREATE/MODIFY | Server/ClinicalIntelligence.Api.Tests/Integration/* | Integration tests for `POST /api/v1/auth/forgot-password` token lifecycle behavior]

## Implementation Plan
- Add integration tests that (when database is available):
  - Create/ensure a test user exists.
  - Call `POST /api/v1/auth/forgot-password` and assert:
    - HTTP 200 for syntactically valid email.
    - A `PasswordResetToken` row is created for that user.
    - `ExpiresAt` is ~1 hour ahead of current UTC time (within a small tolerance).
    - No plain token is stored (only `TokenHash` is present).
  - Call endpoint again and assert:
    - New token row exists.
    - Previous token row is invalidated according to the chosen invalidation strategy.
- Add a non-enumeration test ensuring existing and non-existing emails return indistinguishable 200 responses.

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE/MODIFY | Server/ClinicalIntelligence.Api.Tests/Integration/ForgotPasswordTokenLifecycleTests.cs | Integration tests covering hash-only storage, 1-hour expiry, and invalidation of previous tokens |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Automated] Run test suite and confirm the new tests are deterministic and skippable when the database is unavailable.

## Implementation Checklist
- [x] Add integration test: forgot-password creates a reset token record for existing user
- [x] Assert token expiry is ~1 hour from creation (UTC, with tolerance)
- [x] Assert DB contains only `TokenHash` and never a plain token
- [x] Add integration test: second request invalidates prior token(s)
- [x] Add integration test: existing vs non-existing emails are indistinguishable (no enumeration)
- [x] Ensure tests follow existing skip pattern when DB is unavailable
