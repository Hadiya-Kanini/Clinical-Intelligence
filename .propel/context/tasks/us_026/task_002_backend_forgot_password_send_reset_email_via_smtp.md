# Task - [TASK_002]

## Requirement Reference
- User Story: [us_026]
- Story Location: [.propel/context/tasks/us_026/us_026.md]
- Acceptance Criteria: 
    - [Given a valid reset request, When processed, Then an email with the reset link is sent via SMTP.]
    - [Given the reset link, When included in email, Then it contains the secure token and points to the reset page.]
    - [Given email sending, When attempted, Then delivery status is logged for troubleshooting.]

## Task Overview
Implement the forgot-password backend flow to generate a new password reset token and send a password reset email via SMTP. The email must include the reset link pointing to the frontend reset page and must preserve non-enumeration behavior (same success response for existing vs non-existing emails).

This task focuses on orchestration: user lookup, token generation, reset link creation, SMTP send invocation, and safe logging.

## Dependent Tasks
- [US_024 TASK_002 - Backend forgot password endpoint (generic response)]
- [US_025 TASK_001 - Backend password reset token generation and storage]
- [US_026 TASK_001 - Backend SMTP email sender configuration]

## Impacted Components
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Add/extend `POST /api/v1/auth/forgot-password` to generate token and send SMTP email while keeping generic response]
- [CREATE | Server/ClinicalIntelligence.Api/Configuration/FrontendUrlsOptions.cs | Configuration for base URL used to build reset link (e.g., `https://app/...`)]
- [CREATE | Server/ClinicalIntelligence.Api/Services/Auth/IPasswordResetLinkBuilder.cs | Abstraction for building reset URLs from token (keeps URL logic out of endpoint)]
- [CREATE | Server/ClinicalIntelligence.Api/Services/Auth/PasswordResetLinkBuilder.cs | Builds reset URL pointing to `/reset-password?token=...`]
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Register options + link builder in DI]

## Implementation Plan
- Ensure `POST /api/v1/auth/forgot-password` exists under `var v1 = app.MapGroup("/api/v1");` (from US_024). If missing, implement it using the same validation and response contract planned in US_024.
- For syntactically valid email requests:
  - Lookup user by email.
  - If user exists and is eligible for reset:
    - Generate and persist a new reset token using `IPasswordResetTokenService` (from US_025).
    - Build the reset link using `IPasswordResetLinkBuilder` targeting the frontend route `reset-password` with query parameter `token`.
    - Send an email via `ISmtpEmailSender`.
    - Log success/failure of email sending (do not log tokens or SMTP credentials).
  - If user does not exist:
    - Do not send an email.
- Always return a generic `200` response for syntactically valid emails regardless of user existence (no enumeration).
- Error handling:
  - If SMTP send fails, log failure and still return the same generic success response.

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api/Configuration/FrontendUrlsOptions.cs | Provides a configured base URL to build password reset links |
| CREATE | Server/ClinicalIntelligence.Api/Services/Auth/IPasswordResetLinkBuilder.cs | Interface for building reset links from token |
| CREATE | Server/ClinicalIntelligence.Api/Services/Auth/PasswordResetLinkBuilder.cs | Implementation that builds `/reset-password?token=...` links |
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Add/extend forgot-password endpoint to generate token and send reset email via SMTP; register link builder + options |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Manual/API] POST a syntactically valid email for an existing user and verify:
  - generic `200` response
  - SMTP email is attempted
  - reset link points to the frontend route `reset-password` and includes `token` query param
- [Security] POST existing vs non-existing emails and confirm responses are indistinguishable.
- [Observability] Confirm logs show send attempt outcome without logging reset token.

## Implementation Checklist
- [ ] Ensure forgot-password endpoint exists (or implement it) with validation + generic response per US_024
- [ ] Add `FrontendUrlsOptions` and bind it from configuration
- [ ] Add `IPasswordResetLinkBuilder` + implementation to build `/reset-password?token=...` URLs
- [ ] If user exists, generate reset token via `IPasswordResetTokenService`
- [ ] Send reset email via `ISmtpEmailSender` and include reset link
- [ ] Log send outcomes without secrets (no token, no credentials)
- [ ] Keep non-enumeration behavior regardless of user existence
