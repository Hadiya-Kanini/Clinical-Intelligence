# Task - [TASK_002]

## Requirement Reference
- User Story: [us_030]
- Story Location: [.propel/context/tasks/us_030/us_030.md]
- Acceptance Criteria: 
    - [Given I submit a valid new password, When processed, Then my password hash is updated in the database.]
    - [Given successful password update, When completed, Then the reset token is immediately invalidated (FR-009r).]
    - [Given token invalidation, When the same token is used again, Then it is rejected as invalid.]

## Task Overview
Extend backend integration tests to verify the reset-password flow is single-use and atomic:
- a successful reset consumes the token
- token reuse is rejected
- invalid reset attempts do not consume tokens
- concurrency does not allow multiple successes with the same token

## Dependent Tasks
- [US_030 TASK_001 - Backend reset password atomic update and token invalidation]

## Impacted Components
- [MODIFY | Server/ClinicalIntelligence.Api.Tests/ForgotPasswordEndpointTests.cs | Add/extend `ResetPasswordEndpointTests` to cover token invalidation, reuse rejection, and concurrency behavior]

## Implementation Plan
- Add a test to verify token is invalidated after success:
  - call `POST /api/v1/auth/reset-password` with a valid token
  - assert `200`
  - query `PasswordResetTokens` and assert `UsedAt` is set for the token hash
- Add a test to verify token reuse is rejected:
  - perform a successful reset with a token
  - attempt reset again with the same token
  - assert `401` with `invalid_token`
- Add a test to ensure invalid attempts do not consume token:
  - create a valid token
  - call reset with weak password (expect `400`)
  - query token and assert `UsedAt` remains null
- Add a concurrency test for single-use guarantee:
  - create one valid token
  - start two concurrent `POST /api/v1/auth/reset-password` calls with the same token and a valid password
  - assert exactly one `200` and one `401` (`invalid_token`)

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api.Tests/ForgotPasswordEndpointTests.cs | Add reset-password tests that assert token invalidation, reuse rejection, and concurrency single-use behavior |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Automated] Run test suite and confirm new tests pass reliably (no flakiness in concurrency test).
- [Data Validation] Confirm test assertions query the correct token row by `TokenHash`.

## Implementation Checklist
- [ ] Add test for token invalidation after successful reset (`UsedAt` set)
- [ ] Add test for token reuse rejection (`invalid_token`)
- [ ] Add test that weak password reset attempt does not consume token (`UsedAt` remains null)
- [ ] Add concurrency test that exactly one request succeeds for the same token
- [ ] Ensure tests do not rely on timing-sensitive assertions beyond necessary coordination
