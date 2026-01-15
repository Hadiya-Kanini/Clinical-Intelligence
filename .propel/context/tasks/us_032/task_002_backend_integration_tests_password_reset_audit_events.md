# Task - [TASK_002]

## Requirement Reference
- User Story: [us_032] (extracted from input)
- Story Location: [.propel/context/tasks/us_032/us_032.md]
- Acceptance Criteria: 
    - [Given a password reset is requested, When processed, Then PASSWORD_RESET_REQUESTED event is logged with email, IP, timestamp, token ID.]
    - [Given a password reset is completed, When successful, Then PASSWORD_RESET_COMPLETED event is logged with user ID, IP, timestamp, token ID.]
    - [Given a reset attempt with invalid token, When detected, Then the failed attempt is logged.]

## Task Overview
Add/extend integration tests to verify that the password reset flow writes audit events into `AuditLogEvents` with the required metadata.

These tests should validate persistence into the test database used by `TestWebApplicationFactory` (SQLite file DB) and should assert that audit logging does not change response contracts.

## Dependent Tasks
- [US_032 TASK_001 - Backend audit password reset events]

## Impacted Components
- [MODIFY | Server/ClinicalIntelligence.Api.Tests/ForgotPasswordEndpointTests.cs | Add assertions that `PASSWORD_RESET_REQUESTED` and `PASSWORD_RESET_COMPLETED` audit events are persisted, and that invalid tokens create a failure audit event]

## Implementation Plan
- Forgot password audit coverage:
  - Reuse existing `ForgotPasswordEndpointTests` request flow.
  - After calling `POST /api/v1/auth/forgot-password` for an existing seeded user (`test@example.com`), query `ApplicationDbContext.AuditLogEvents` and assert:
    - `ActionType == "PASSWORD_RESET_REQUESTED"`
    - metadata JSON contains the normalized email
    - metadata JSON contains a `tokenId` when the request produced a token
- Reset password success audit coverage:
  - Reuse existing `ResetPasswordEndpointTests` flow and helpers within `Server/ClinicalIntelligence.Api.Tests/ForgotPasswordEndpointTests.cs`.
  - After a successful `POST /api/v1/auth/reset-password`, query `AuditLogEvents` and assert:
    - `ActionType == "PASSWORD_RESET_COMPLETED"`
    - metadata JSON contains `userId`
    - metadata JSON contains `tokenId`
- Invalid token audit coverage:
  - Call `POST /api/v1/auth/reset-password` with an invalid token value that reaches the token lookup/unauthorized branch.
  - Assert an audit event exists with:
    - `ActionType` matching the implementation (recommended: `PASSWORD_RESET_FAILED`)
    - metadata JSON includes a safe reason code (e.g., `invalid_token`)
- Validation details:
  - Use `JsonDocument` parsing on `AuditLogEvent.Metadata` to assert required fields.
  - Avoid brittle ordering assertions; filter audit events by action type and latest timestamp.
  - Ensure tests remain deterministic against SQLite.

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api.Tests/ForgotPasswordEndpointTests.cs | Add/extend integration tests to validate audit events for password reset requested/completed/failed scenarios |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Automated] Run the test suite and confirm tests pass locally using the SQLite-backed `TestWebApplicationFactory`.
- [Data] Confirm tests validate:
  - correct `ActionType`
  - required metadata fields
  - no raw reset token is present in `AuditLogEvent.Metadata`

## Implementation Checklist
- [ ] Add integration test assertions for `PASSWORD_RESET_REQUESTED` audit event creation
- [ ] Add integration test assertions for `PASSWORD_RESET_COMPLETED` audit event creation
- [ ] Add integration test for invalid token attempt logging
- [ ] Parse and validate `AuditLogEvent.Metadata` JSON fields (email/userId/tokenId/reason)
- [ ] Ensure tests remain deterministic and do not depend on event ordering
