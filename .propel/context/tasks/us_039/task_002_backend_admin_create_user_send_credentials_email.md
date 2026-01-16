# Task - [TASK_002]

## Requirement Reference
- User Story: [us_039]
- Story Location: [.propel/context/tasks/us_039/us_039.md]
- Acceptance Criteria: 
    - [Given a user is successfully created, When creation completes, Then an email with login credentials is sent to the user.]
    - [Given email delivery, When status is known, Then the admin is informed of success or failure.]
    - [Given email sending, When attempted, Then it uses TLS for secure transmission.]

## Task Overview
Wire the "send credential email" behavior into the **admin user creation flow**, so that when an Admin provisions a new Standard user, the system:
- generates a temporary/initial password (server-side)
- stores only the hashed password
- sends a credential email via `IEmailService`
- returns a response to the Admin indicating whether email delivery was attempted/succeeded

This task focuses on backend endpoint orchestration and response semantics.

## Dependent Tasks
- [US_037] Implement admin-only user creation endpoint
- [US_039 TASK_001] Backend email support for new-user credential emails
- [US_026] SMTP configuration (email delivery capability)

## Impacted Components
- [CREATE | Server/ClinicalIntelligence.Api/Contracts/Admin/CreateUserRequest.cs | Update/introduce contract to align with server-generated temporary password]
- [CREATE | Server/ClinicalIntelligence.Api/Contracts/Admin/CreateUserResponse.cs | Include delivery status fields so Admin is informed of success/failure]
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Extend admin create-user endpoint to generate temporary password and send credential email]

## Implementation Plan
- Align the admin-create-user contract with credential email behavior:
  - Prefer server-generated password for onboarding:
    - `CreateUserRequest`: `name`, `email` (no plaintext password from admin)
  - `CreateUserResponse` should include:
    - created user identifiers (id/email/role)
    - delivery status info, e.g. `credentials_email_sent` boolean
    - optional `credentials_email_error_code` (non-sensitive) when send fails
- Generate a temporary password:
  - Create a random password that meets `PasswordPolicy` requirements.
  - Do not log the plaintext password.
  - Hash with BCrypt (same pattern as other auth flows).
- Send credential email:
  - Call `IEmailService.SendNewUserCredentialsEmailAsync(...)`.
  - Ensure email sending occurs after the user is persisted (so admin gets a stable create result).
  - Decide how failures impact the API response:
    - user creation should succeed even if email fails
    - response should clearly communicate email success/failure
- Security:
  - Validate input email using `EmailValidation.ValidateWithDetails()`.
  - Use the existing admin authorization pattern (role check).
  - Ensure audit logging does not include the temporary password.

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api/Contracts/Admin/CreateUserRequest.cs | Admin create-user request DTO aligned to server-generated credentials |
| CREATE | Server/ClinicalIntelligence.Api/Contracts/Admin/CreateUserResponse.cs | Response DTO including `credentials_email_sent` and safe delivery metadata |
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Extend `POST /api/v1/admin/users` flow: generate temporary password, persist hash, send credentials email, return delivery status |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis
- https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Manual/API] Authenticate as Admin and create a new user:
  - verify user is created and stored with hashed password
  - verify credential email is attempted and delivered (if SMTP configured)
  - verify API response includes `credentials_email_sent` reflecting send result
- [Failure simulation] Temporarily disable SMTP config and verify:
  - user creation still succeeds
  - response sets `credentials_email_sent = false`
  - logs show warning/error without leaking the temporary password

## Implementation Checklist
- [x] Implement a temporary password generator that satisfies `PasswordPolicy`
- [x] Update/create admin create-user contracts under `Server/ClinicalIntelligence.Api/Contracts/Admin/`
- [x] Update `POST /api/v1/admin/users` to generate password + persist hash
- [x] Invoke `SendNewUserCredentialsEmailAsync(...)` and capture success/failure
- [x] Return delivery status to admin (no secrets)
- [x] Ensure audit logging excludes plaintext password
