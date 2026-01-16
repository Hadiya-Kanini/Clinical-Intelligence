# Task - [TASK_003]

## Requirement Reference
- User Story: [us_039]
- Story Location: [.propel/context/tasks/us_039/us_039.md]
- Acceptance Criteria: 
    - [Given a user is created, When creation succeeds, Then an email with credentials is sent.]
    - [Given email delivery, When status is known, Then the admin is informed of success or failure.]

## Task Overview
Add automated integration tests to ensure that the admin create-user flow:
- triggers the credential email send operation
- returns a response field that informs the Admin whether the credential email send succeeded

Tests should use the existing `ClinicalIntelligence.Api.Tests` integration testing approach and avoid sending real emails.

## Dependent Tasks
- [US_039 TASK_001] Backend email support for new-user credential emails
- [US_039 TASK_002] Admin create-user wiring to send credential emails

## Impacted Components
- [CREATE | Server/ClinicalIntelligence.Api.Tests/AdminCreateUserCredentialEmailTests.cs | Integration tests covering credential email invocation and admin-visible delivery status]
- [MODIFY | Server/ClinicalIntelligence.Api.Tests/TestWebApplicationFactory.cs | Override `IEmailService` with a fake for deterministic tests (if needed)]

## Implementation Plan
- Introduce a fake/stub `IEmailService` for tests:
  - Implement `IEmailService` in test project with:
    - `IsConfigured = true`
    - `SendNewUserCredentialsEmailAsync(...)` capturing invocation parameters and returning configurable success
    - Other required interface methods returning default values
  - Override DI registration in `TestWebApplicationFactory`:
    - remove existing `IEmailService` registration
    - register the fake as singleton
- Test cases:
  - **Email send success**:
    - login as seeded admin (`admin@example.com`)
    - call `POST /api/v1/admin/users`
    - assert HTTP success
    - assert response contains `credentials_email_sent = true`
    - assert fake email service recorded exactly one send invocation to the created user's email
  - **Email send failure**:
    - configure fake email service to return `false`
    - call the endpoint
    - assert user creation still succeeds
    - assert response contains `credentials_email_sent = false`
- Ensure tests do not log/assert on plaintext password beyond minimal shape checks:
  - do not print password
  - only verify that the method was called and that a non-empty password was provided to the email sender (if captured)

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api.Tests/AdminCreateUserCredentialEmailTests.cs | Integration tests validating credential email send behavior and admin-visible status flag |
| MODIFY | Server/ClinicalIntelligence.Api.Tests/TestWebApplicationFactory.cs | Override `IEmailService` registration with a deterministic fake for tests |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests
- https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Automated] Run `dotnet test` and confirm:
  - tests reliably pass without external SMTP
  - regression in email send invocation or response fields causes test failure

## Implementation Checklist
- [x] Implement a fake `IEmailService` for tests (captures send invocations)
- [x] Override DI in `TestWebApplicationFactory` to use the fake
- [x] Add success test for `credentials_email_sent = true`
- [x] Add failure test for `credentials_email_sent = false`
- [x] Ensure tests avoid leaking plaintext passwords into logs
