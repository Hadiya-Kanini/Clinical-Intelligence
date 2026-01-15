# Task - TASK_006

## Requirement Reference
- User Story: us_016
- Story Location: .propel/context/tasks/us_016/us_016.md
- Acceptance Criteria: 
    - AC-1: Given account is locked after 5 failed attempts, When lockout occurs, Then email notification is sent to user asynchronously
    - AC-2: Given lockout email is sent, When email fails to send, Then error is logged but lockout still occurs (non-blocking)
    - AC-3: Given lockout email, When user receives it, Then email contains lockout duration, timestamp, and security tips
    - AC-4: Given email sending, When called, Then it does not delay the login API response

## Task Overview
Implement email notification when user account is locked due to failed login attempts. Email must be sent asynchronously (fire-and-forget) to avoid delaying login response, include security information, and handle failures gracefully without affecting lockout functionality.
Estimated Effort: 2 hours

## Dependent Tasks
- task_002_backend_email_service_infrastructure (email service must exist)
- US_016 - Account Lockout (lockout logic implemented in login endpoint)

## Impacted Components
- Server/ClinicalIntelligence.Api/Program.cs
- Server/ClinicalIntelligence.Api/Services/IEmailService.cs
- Server/ClinicalIntelligence.Api/Services/SmtpEmailService.cs

## Implementation Plan
- Add SendAccountLockedEmailAsync to IEmailService interface:
  - Method signature: `Task<bool> SendAccountLockedEmailAsync(string to, string userName, DateTime lockedUntil)`
  - Returns bool indicating success/failure
- Implement SendAccountLockedEmailAsync in SmtpEmailService:
  - Create HTML email template with:
    - Subject: "Security Alert: Account Temporarily Locked - Clinical Intelligence"
    - Lockout timestamp
    - Unlock timestamp (lockedUntil)
    - Duration (15 minutes)
    - Reason: "5 consecutive failed login attempts"
    - Security tips:
      - If this was you, wait 15 minutes and try again
      - If this wasn't you, your account may be under attack
      - Consider changing password after unlocking
      - Contact support if needed
    - Support contact information
    - Plain text fallback
  - Log email sending attempts
  - Return false on failure (don't throw)
- Update login endpoint in Program.cs:
  - After setting LockedUntil in database
  - Call email service asynchronously using fire-and-forget pattern:
    ```csharp
    _ = Task.Run(async () => 
    {
        try 
        {
            await emailService.SendAccountLockedEmailAsync(
                user.Email, 
                user.Name, 
                user.LockedUntil.Value
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send account locked email to {Email}", user.Email);
        }
    });
    ```
  - Ensure login response is not delayed by email sending
  - Email failure does not affect lockout functionality
- Add logging:
  - Log when lockout email is queued
  - Log when email sends successfully
  - Log when email fails (with exception details)
  - Never log email content or user details in plain text
**Focus on how to implement**

## Current Project State
```
Server/ClinicalIntelligence.Api/
├── Services/
│   ├── IEmailService.cs (exists with password reset methods)
│   └── SmtpEmailService.cs (exists with email templates)
└── Program.cs (exists with login endpoint that sets LockedUntil)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Services/IEmailService.cs | Add SendAccountLockedEmailAsync method signature |
| MODIFY | Server/ClinicalIntelligence.Api/Services/SmtpEmailService.cs | Implement account locked email template and sending logic |
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Add async email call in login endpoint after account lockout |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/
- https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.run

## Build Commands
- dotnet build Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj
- dotnet test Server/ClinicalIntelligence.Api.Tests/ClinicalIntelligence.Api.Tests.csproj

## Implementation Validation Strategy
- Trigger account lockout (5 failed login attempts); validate email is sent and login response is immediate (AC-1).
- Simulate email service failure; validate lockout still occurs and error is logged (AC-2).
- Check received email; validate it contains lockout duration, timestamp, and security tips (AC-3).
- Measure login response time with email sending; validate no delay (AC-4).

## Implementation Checklist
- [ ] Add SendAccountLockedEmailAsync to IEmailService
- [ ] Implement email template in SmtpEmailService
- [ ] Add async email call to login endpoint
- [ ] Use fire-and-forget pattern (Task.Run)
- [ ] Add error handling and logging
- [ ] Test email delivery
- [ ] Verify login response not delayed
- [ ] Test with email service failure
- [ ] Verify lockout works even if email fails
