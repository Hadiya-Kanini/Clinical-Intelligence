# Task - [TASK_001]

## Requirement Reference
- User Story: [us_039]
- Story Location: [.propel/context/tasks/us_039/us_039.md]
- Acceptance Criteria: 
    - [Given the credential email, When sent, Then it includes the user's email and a temporary/initial password.]
    - [Given email sending, When attempted, Then it uses TLS for secure transmission.]

## Task Overview
Extend the backend email abstraction to support sending a **new user credential email** (email + temporary/initial password) via the existing SMTP infrastructure.

This task focuses on **email service capability and templating only** (no admin endpoint wiring). It must:
- generate an email body that safely includes the recipient email and the generated temporary password
- reuse the existing SMTP implementation (`SmtpEmailService`) which already uses `SecureSocketOptions.StartTls` when SSL is enabled
- log send attempts and outcomes without leaking secrets beyond what is explicitly required (the password is required in email content, but must not be logged)

## Dependent Tasks
- [US_026] Backend SMTP email sender configuration (email service must be configured)

## Impacted Components
- [MODIFY | Server/ClinicalIntelligence.Api/Services/IEmailService.cs | Add an API for sending new-user credential emails]
- [MODIFY | Server/ClinicalIntelligence.Api/Services/SmtpEmailService.cs | Implement the new-user credential email method with a dedicated subject + HTML body]

## Implementation Plan
- Extend the email abstraction:
  - Add a new method to `IEmailService` (example signature):
    - `Task<bool> SendNewUserCredentialsEmailAsync(string to, string userName, string temporaryPassword)`
  - Keep method naming and return semantics consistent with existing methods (`true` when sent successfully, `false` otherwise).
- Implement in `SmtpEmailService`:
  - Add a new subject line: e.g. `"Your Clinical Intelligence Account Credentials"`.
  - Add an HTML generator similar to password reset emails:
    - include `userName`
    - include `to` (email)
    - include `temporaryPassword`
    - include a short security note recommending password change on first login (do not implement enforcement in this task)
  - Ensure HTML encoding for interpolated values.
- Security/observability:
  - Log: "Sending new user credentials email" with recipient email.
  - Do NOT log the temporary password.
  - Preserve the existing TLS behavior:
    - `SecureSocketOptions.StartTls` is selected when `_secrets.SmtpEnableSsl` is true.

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Services/IEmailService.cs | Add `SendNewUserCredentialsEmailAsync(...)` method to the email abstraction |
| MODIFY | Server/ClinicalIntelligence.Api/Services/SmtpEmailService.cs | Implement the new method + HTML template generation; ensure no secrets are logged |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/dotnet/api/system.net.webutility.htmlencode
- https://github.com/jstedfast/MailKit

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Manual] With SMTP configured, call the new method directly (or via a small local harness) and verify:
  - email is attempted
  - email body includes the recipient email and the temporary password
  - logs contain send attempt and outcome, but do not contain the temporary password
- [Security] Confirm SMTP connection uses StartTLS when enabled (existing `SmtpEmailService` behavior).

## Implementation Checklist
- [ ] Add `SendNewUserCredentialsEmailAsync(...)` to `IEmailService`
- [ ] Implement method in `SmtpEmailService` using `SendEmailAsync(...)`
- [ ] Add HTML generator function for credential email (encode inputs)
- [ ] Ensure logging excludes temporary password
- [ ] Verify email sends successfully using a dev SMTP provider
