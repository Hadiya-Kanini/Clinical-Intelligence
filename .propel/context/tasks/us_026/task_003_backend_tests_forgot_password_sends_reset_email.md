# Task - [TASK_003]

## Requirement Reference
- User Story: [us_026]
- Story Location: [.propel/context/tasks/us_026/us_026.md]
- Acceptance Criteria: 
    - [Given a valid reset request, When processed, Then an email with reset link is sent via SMTP.]
    - [Given the reset link, When included in email, Then it contains the secure token and points to the reset page.]
    - [Given email sending, When attempted, Then delivery status is logged for troubleshooting.]

## Task Overview
Add automated backend test coverage validating that the forgot-password flow triggers SMTP email sending for existing accounts, constructs the correct reset link, and handles SMTP failures safely (logging the failure while preserving the generic response contract and without leaking tokens).

## Dependent Tasks
- [US_024 TASK_002 - Backend forgot password endpoint (generic response)]
- [US_025 TASK_001 - Backend password reset token generation and storage]
- [US_026 TASK_001 - Backend SMTP email sender configuration]
- [US_026 TASK_002 - Backend forgot password sends reset email via SMTP]

## Impacted Components
- [CREATE/MODIFY | Server/ClinicalIntelligence.Api.Tests/Integration/ForgotPasswordEmailDeliveryTests.cs | Integration tests for forgot-password email sending behavior]
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Allow injection override for `ISmtpEmailSender` in tests (using existing DI patterns)]

## Implementation Plan
- Add integration tests using `WebApplicationFactory<Program>` consistent with existing integration tests.
- Use a test double (mock) for `ISmtpEmailSender` to avoid sending real emails:
  - Override DI registration in the test host so the endpoint uses the mock sender.
  - Capture the email payload passed to the sender to validate:
    - recipient email
    - presence of a reset link
    - reset link path equals frontend `reset-password`
    - reset link contains `token` query parameter
- Add failure-path test:
  - Configure mock sender to throw.
  - Assert endpoint still returns generic `200` for syntactically valid email.
  - Assert failure is logged (best-effort verification; do not assert on full message content containing token).

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE/MODIFY | Server/ClinicalIntelligence.Api.Tests/Integration/ForgotPasswordEmailDeliveryTests.cs | Tests verifying reset email is attempted for existing user and that reset link points to `/reset-password?token=...` while preserving generic responses |
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | If required, adjust DI registration to be test-friendly for `ISmtpEmailSender` (no behavior change in production) |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests
- https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Automated] Run the test suite and confirm:
  - email sender is invoked for existing user
  - link format is correct
  - failure path does not change response contract

## Implementation Checklist
- [ ] Add test host override to inject mocked `ISmtpEmailSender`
- [ ] Ensure test user exists and call forgot-password endpoint
- [ ] Assert email sender was called and includes reset link to `/reset-password?token=...`
- [ ] Add negative test: non-existing email returns 200 but does not call email sender
- [ ] Add failure test: email sender throws but endpoint still returns generic 200
- [ ] Ensure tests do not log/assert on any plain token value
