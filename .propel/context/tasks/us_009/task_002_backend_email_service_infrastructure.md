# Task - TASK_002

## Requirement Reference
- User Story: us_009
- Story Location: .propel/context/tasks/us_009/us_009.md
- Acceptance Criteria: 
    - AC-1: Given email service is configured, When application starts, Then SMTP connection is validated and service is registered in DI container
    - AC-2: Given valid email parameters, When SendEmailAsync is called, Then email is sent via SMTP with TLS/SSL encryption
    - AC-3: Given email sending fails, When error occurs, Then exception is logged and false is returned without crashing application
    - AC-4: Given email configuration is missing, When application starts, Then clear error message indicates missing SMTP configuration

## Task Overview
Implement SMTP-based email service infrastructure using MailKit to support password reset emails and account notifications. The service must validate SMTP configuration from environment variables, support async email sending with error handling, and provide a clean interface for different email types.
Estimated Effort: 3 hours

## Dependent Tasks
- US_123 - Static Admin Authentication (users table and authentication flow exist)

## Impacted Components
- Server/ClinicalIntelligence.Api/Services/IEmailService.cs
- Server/ClinicalIntelligence.Api/Services/SmtpEmailService.cs
- Server/ClinicalIntelligence.Api/Configuration/SecretsOptions.cs
- Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj
- Server/ClinicalIntelligence.Api/Program.cs
- Server/ClinicalIntelligence.Api.Tests/EmailServiceTests.cs

## Implementation Plan
- Add SMTP configuration to SecretsOptions:
  - Add properties: SmtpHost, SmtpPort, SmtpUsername, SmtpPassword, SmtpFromEmail, SmtpFromName, SmtpEnableSsl
  - Read from environment variables with validation
  - Add validation method to ensure all required SMTP settings are present
- Add MailKit NuGet packages:
  - Add `MailKit` package for SMTP client
  - Add `MimeKit` package for email message construction
- Create IEmailService interface:
  - Define `Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true)`
  - Define `Task<bool> SendPasswordResetEmailAsync(string to, string resetToken, string userName, string resetUrl)`
  - Define `Task<bool> SendPasswordResetConfirmationAsync(string to, string userName)`
  - Define `Task<bool> SendAccountLockedEmailAsync(string to, string userName, DateTime lockedUntil)`
- Implement SmtpEmailService:
  - Inject ILogger and SecretsOptions via constructor
  - Implement SendEmailAsync with MailKit SmtpClient
  - Use TLS/SSL based on configuration
  - Implement retry logic (3 attempts with exponential backoff)
  - Log all email attempts (success and failure)
  - Implement template methods for password reset, confirmation, and lockout emails
  - Never throw exceptions - return false on failure
- Register service in Program.cs:
  - Add `builder.Services.AddSingleton<IEmailService, SmtpEmailService>()`
  - Validate SMTP configuration on startup
- Add unit tests:
  - Test email service with mock SMTP server
  - Test error handling
  - Test template methods generate correct content
**Focus on how to implement**

## Current Project State
```
Server/ClinicalIntelligence.Api/
├── Configuration/
│   └── SecretsOptions.cs (exists - needs SMTP config)
├── Services/
│   ├── IPatientAggregateService.cs (exists)
│   └── PatientAggregateService.cs (exists)
├── Program.cs (exists - needs email service registration)
└── ClinicalIntelligence.Api.csproj (exists - needs MailKit packages)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Configuration/SecretsOptions.cs | Add SMTP configuration properties and validation method |
| MODIFY | Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj | Add MailKit and MimeKit NuGet packages |
| CREATE | Server/ClinicalIntelligence.Api/Services/IEmailService.cs | Define email service interface with methods for sending emails |
| CREATE | Server/ClinicalIntelligence.Api/Services/SmtpEmailService.cs | Implement SMTP email service with MailKit, retry logic, and templates |
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Register IEmailService in DI container and validate SMTP config |
| CREATE | Server/ClinicalIntelligence.Api.Tests/EmailServiceTests.cs | Unit tests for email service functionality |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://github.com/jstedfast/MailKit
- https://github.com/jstedfast/MimeKit
- https://learn.microsoft.com/en-us/dotnet/api/system.net.mail.smtpclient (deprecated - use MailKit instead)

## Build Commands
- dotnet add Server/ClinicalIntelligence.Api package MailKit
- dotnet add Server/ClinicalIntelligence.Api package MimeKit
- dotnet build Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj
- dotnet test Server/ClinicalIntelligence.Api.Tests/ClinicalIntelligence.Api.Tests.csproj

## Implementation Validation Strategy
- Start application with SMTP configuration in .env; validate no errors and service is registered (AC-1).
- Call SendEmailAsync with valid test email; validate email is received and method returns true (AC-2).
- Simulate SMTP failure; validate error is logged and method returns false without crashing (AC-3).
- Start application without SMTP configuration; validate clear error message (AC-4).

## Implementation Checklist
- [x] Add SMTP configuration to SecretsOptions
- [x] Add MailKit and MimeKit NuGet packages
- [x] Create IEmailService interface
- [x] Implement SmtpEmailService with retry logic
- [x] Register service in Program.cs with validation
- [x] Add unit tests for email service
- [x] Test with real SMTP server (Gmail)
- [x] Verify error handling and logging
