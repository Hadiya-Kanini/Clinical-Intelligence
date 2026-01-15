# Task - TASK_004

## Requirement Reference
- User Story: us_009
- Story Location: .propel/context/tasks/us_009/us_009.md
- Acceptance Criteria: 
    - AC-1: Given valid reset token and new password, When POST /api/v1/auth/reset-password is called, Then password is updated and confirmation email is sent
    - AC-2: Given expired token (>1 hour old), When reset is attempted, Then 401 Unauthorized is returned with message "Reset link has expired"
    - AC-3: Given already-used token, When reset is attempted, Then 401 Unauthorized is returned with message "Reset link has already been used"
    - AC-4: Given new password doesn't meet complexity requirements, When reset is attempted, Then 400 Bad Request is returned with specific requirements
    - AC-5: Given successful password reset, When completed, Then FailedLoginAttempts is reset to 0 and LockedUntil is cleared

## Task Overview
Implement reset password endpoint that validates reset tokens, enforces password complexity requirements (FR-009c), updates user password with bcrypt hashing, marks token as used, resets account lockout state, and sends confirmation email.
Estimated Effort: 3 hours

## Dependent Tasks
- task_003_password_reset_email_flow (forgot password flow must exist)
- US_021 - Bcrypt Password Hashing (password hashing implemented)

## Impacted Components
- Server/ClinicalIntelligence.Api/Program.cs
- Server/ClinicalIntelligence.Api/Contracts/ResetPasswordRequest.cs
- Server/ClinicalIntelligence.Api/Services/SmtpEmailService.cs
- Server/ClinicalIntelligence.Api.Tests/ResetPasswordEndpointTests.cs

## Implementation Plan
- Create ResetPasswordRequest contract:
  - Properties: `string? Token`, `string? NewPassword`
  - JSON property names: "token", "newPassword"
- Implement POST /api/v1/auth/reset-password endpoint in Program.cs:
  - Validate token and newPassword are not null/empty (return 400)
  - Validate token is valid GUID format (return 400)
  - Hash token with SHA256 to lookup in database
  - Query password_reset_tokens by TokenHash where UsedAt is null
  - If not found, return 401 with code "invalid_token", message "Invalid or expired reset link"
  - Check ExpiresAt > DateTime.UtcNow (return 401 if expired)
  - Validate password complexity (FR-009c):
    - Minimum 8 characters
    - At least one uppercase letter
    - At least one lowercase letter
    - At least one digit
    - At least one special character
    - Return 400 with specific missing requirements if invalid
  - Get user from token.UserId
  - If user is null or deleted, return 401
  - Hash new password with BCrypt (12 rounds)
  - Update user:
    - PasswordHash = new hash
    - FailedLoginAttempts = 0
    - LockedUntil = null
    - UpdatedAt = DateTime.UtcNow
  - Mark token as used:
    - UsedAt = DateTime.UtcNow
  - Save changes to database
  - Send confirmation email asynchronously
  - Return 200 with message: "Password reset successful. You can now log in."
- Implement password reset confirmation email template:
  - Subject: "Password Successfully Changed - Clinical Intelligence"
  - HTML body confirming password change
  - Security tips: contact support if not you
  - Plain text fallback
- Add integration tests:
  - Test with valid token and password returns 200
  - Test with expired token returns 401
  - Test with used token returns 401
  - Test with invalid token format returns 400
  - Test with weak password returns 400 with requirements
  - Test user's FailedLoginAttempts and LockedUntil are reset
  - Test confirmation email is sent
  - Test token is marked as used
**Focus on how to implement**

## Current Project State
```
Server/ClinicalIntelligence.Api/
├── Services/
│   ├── IEmailService.cs (exists)
│   └── SmtpEmailService.cs (exists with SendPasswordResetEmailAsync)
├── Domain/Models/
│   ├── User.cs (exists with PasswordHash, FailedLoginAttempts, LockedUntil)
│   └── PasswordResetToken.cs (exists with UsedAt field)
└── Program.cs (exists with forgot-password endpoint)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api/Contracts/ResetPasswordRequest.cs | Request contract with token and newPassword properties |
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Add POST /api/v1/auth/reset-password endpoint with validation and password update |
| MODIFY | Server/ClinicalIntelligence.Api/Services/SmtpEmailService.cs | Implement SendPasswordResetConfirmationAsync with HTML template |
| CREATE | Server/ClinicalIntelligence.Api.Tests/ResetPasswordEndpointTests.cs | Integration tests for reset password flow |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html#password-complexity
- https://github.com/BcryptNet/bcrypt.net
- https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.sha256

## Build Commands
- dotnet build Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj
- dotnet test Server/ClinicalIntelligence.Api.Tests/ClinicalIntelligence.Api.Tests.csproj

## Implementation Validation Strategy
- Create valid reset token, call endpoint with strong password; validate password updated and confirmation email sent (AC-1).
- Create token with ExpiresAt in past, call endpoint; validate 401 returned (AC-2).
- Use token twice; validate second attempt returns 401 (AC-3).
- Call endpoint with weak password; validate 400 with specific requirements (AC-4).
- Reset password for locked account; validate FailedLoginAttempts=0 and LockedUntil=null (AC-5).

## Implementation Checklist
- [x] Create ResetPasswordRequest contract
- [x] Implement reset password endpoint with token validation
- [x] Add password complexity validation
- [x] Implement password update with bcrypt
- [x] Reset FailedLoginAttempts and LockedUntil
- [x] Mark token as used
- [x] Implement confirmation email template
- [x] Add integration tests
- [x] Test with expired tokens
- [x] Test with used tokens
- [x] Verify account unlock after reset
