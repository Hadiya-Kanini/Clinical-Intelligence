# Task - TASK_003

## Requirement Reference
- User Story: us_009
- Story Location: .propel/context/tasks/us_009/us_009.md
- Acceptance Criteria: 
    - AC-1: Given valid email address, When POST /api/v1/auth/forgot-password is called, Then reset token is generated, stored in database, and email is sent
    - AC-2: Given non-existent email, When forgot password is requested, Then same success response is returned (no user enumeration)
    - AC-3: Given user requests password reset, When token is generated, Then it expires after 1 hour and is hashed before storage
    - AC-4: Given user has existing reset token, When new reset is requested, Then old token is invalidated
    - AC-5: Given user requests multiple resets, When rate limit is exceeded (3 per hour), Then 429 Too Many Requests is returned

## Task Overview
Implement forgot password endpoint that generates secure reset tokens, stores them in password_reset_tokens table with expiration, sends password reset email with link, and includes rate limiting to prevent abuse. Must not leak user existence information.
Estimated Effort: 4 hours

## Dependent Tasks
- task_002_backend_email_service_infrastructure (email service must exist)
- US_119 - Baseline Schema (password_reset_tokens table exists)

## Impacted Components
- Server/ClinicalIntelligence.Api/Program.cs
- Server/ClinicalIntelligence.Api/Contracts/ForgotPasswordRequest.cs
- Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs
- Server/ClinicalIntelligence.Api.Tests/ForgotPasswordEndpointTests.cs

## Implementation Plan
- Add frontend URL configuration to SecretsOptions:
  - Add `FrontendUrl` property (e.g., "http://localhost:5173")
  - Read from `FRONTEND_URL` environment variable
  - Used to construct password reset links
- Create ForgotPasswordRequest contract:
  - Single property: `string? Email`
  - JSON property name: "email"
- Implement POST /api/v1/auth/forgot-password endpoint in Program.cs:
  - Validate email format (RFC 5322 compliant)
  - Return 400 if email is missing or invalid format
  - Query user by email (ignore soft-deleted users)
  - If user not found or inactive, still return success (prevent enumeration)
  - Check rate limit: count password_reset_tokens for this user in last hour
  - If >= 3 tokens in last hour, return 429 with Retry-After header
  - Invalidate existing tokens: set ExpiresAt to now for user's unused tokens
  - Generate new token: `Guid.NewGuid().ToString()`
  - Hash token with SHA256 before storing
  - Create PasswordResetToken record:
    - UserId, TokenHash, ExpiresAt (1 hour from now), CreatedAt
  - Construct reset URL: `{FrontendUrl}/reset-password?token={plainToken}`
  - Call `emailService.SendPasswordResetEmailAsync(email, plainToken, userName, resetUrl)`
  - Always return 200 with message: "If the email exists, a reset link has been sent."
  - Log all attempts to audit log (future enhancement placeholder)
- Implement password reset email template in SmtpEmailService:
  - Subject: "Reset Your Password - Clinical Intelligence"
  - HTML body with reset link button
  - Include expiration time (1 hour)
  - Security tips: don't share link, ignore if not requested
  - Plain text fallback
- Add integration tests:
  - Test with valid email returns 200
  - Test with non-existent email returns 200 (same response)
  - Test token is created in database with correct expiration
  - Test email is sent
  - Test rate limiting (4th request in hour returns 429)
  - Test old tokens are invalidated
  - Test with deleted/inactive user returns 200
**Focus on how to implement**

## Current Project State
```
Server/ClinicalIntelligence.Api/
├── Services/
│   ├── IEmailService.cs (created in task_002)
│   └── SmtpEmailService.cs (created in task_002)
├── Domain/Models/
│   ├── User.cs (exists)
│   └── PasswordResetToken.cs (exists from US_119)
├── Data/
│   └── ApplicationDbContext.cs (exists with PasswordResetTokens DbSet)
└── Program.cs (exists - needs new endpoint)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Configuration/SecretsOptions.cs | Add FrontendUrl property for reset link construction |
| CREATE | Server/ClinicalIntelligence.Api/Contracts/ForgotPasswordRequest.cs | Request contract with email property |
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Add POST /api/v1/auth/forgot-password endpoint with rate limiting and token generation |
| MODIFY | Server/ClinicalIntelligence.Api/Services/SmtpEmailService.cs | Implement SendPasswordResetEmailAsync with HTML template |
| CREATE | Server/ClinicalIntelligence.Api.Tests/ForgotPasswordEndpointTests.cs | Integration tests for forgot password flow |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://cheatsheetseries.owasp.org/cheatsheets/Forgot_Password_Cheat_Sheet.html
- https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.sha256
- https://datatracker.ietf.org/doc/html/rfc5322 (email validation)

## Build Commands
- dotnet build Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj
- dotnet test Server/ClinicalIntelligence.Api.Tests/ClinicalIntelligence.Api.Tests.csproj

## Implementation Validation Strategy
- Call endpoint with valid email; validate token created in database with 1-hour expiration and email sent (AC-1).
- Call endpoint with non-existent email; validate same 200 response returned (AC-2).
- Verify token in database is SHA256 hashed and expires in 1 hour (AC-3).
- Request reset twice for same user; validate first token is invalidated (AC-4).
- Request reset 4 times in 1 hour; validate 4th request returns 429 (AC-5).

## Implementation Checklist
- [x] Add FrontendUrl to SecretsOptions
- [x] Create ForgotPasswordRequest contract
- [x] Implement forgot password endpoint with validation
- [x] Add rate limiting logic (3 per hour)
- [x] Implement token generation and hashing
- [x] Invalidate old tokens
- [x] Implement password reset email template
- [x] Add integration tests
- [x] Test with real email delivery
- [x] Verify no user enumeration
