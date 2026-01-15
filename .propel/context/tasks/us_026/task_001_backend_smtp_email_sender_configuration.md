# Task - [TASK_001]

## Requirement Reference
- User Story: [us_026]
- Story Location: [.propel/context/tasks/us_026/us_026.md]
- Acceptance Criteria: 
    - [Given a valid reset request, When processed, Then an email with the reset link is sent via SMTP.]
    - [Given the reset email, When sent, Then it uses TLS 1.2+ for secure transmission.]
    - [Given email sending, When attempted, Then delivery status is logged for troubleshooting.]

## Task Overview
Implement an SMTP-based email sending capability in the backend, exposed via an abstraction that can be injected into the forgot-password flow. The implementation must enforce TLS 1.2+ during SMTP transmission and provide structured logging for send attempts.

This task focuses on the reusable SMTP sender infrastructure (configuration + DI + implementation) and does not wire it into the forgot-password endpoint (handled in TASK_002).

## Dependent Tasks
- [N/A] (Can be implemented independently)

## Impacted Components
- [CREATE | Server/ClinicalIntelligence.Api/Configuration/SmtpOptions.cs | Strongly typed SMTP settings loaded from environment/configuration]
- [CREATE | Server/ClinicalIntelligence.Api/Services/Email/ISmtpEmailSender.cs | Abstraction for sending email via SMTP]
- [CREATE | Server/ClinicalIntelligence.Api/Services/Email/SmtpEmailSender.cs | SMTP implementation that enforces TLS 1.2+ and emits structured logs]
- [MODIFY | Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj | Add SMTP library dependency if needed (e.g., MailKit)]
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Register SMTP options + `ISmtpEmailSender` in DI]

## Implementation Plan
- Introduce an `SmtpOptions` configuration model to capture:
  - SMTP host, port, username, password, from-address
  - security mode (StartTLS/SSL)
  - optional timeout settings
- Load configuration from environment variables / configuration provider consistent with existing `SecretsOptions` usage.
- Implement `ISmtpEmailSender` with async I/O operations.
- Enforce TLS 1.2+ for SMTP connections.
- Add structured logs for:
  - send attempt (without including reset token or other secrets)
  - success/failure (exception type + safe metadata)

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api/Configuration/SmtpOptions.cs | SMTP configuration model for host/port/credentials/from/security settings |
| CREATE | Server/ClinicalIntelligence.Api/Services/Email/ISmtpEmailSender.cs | Interface for sending transactional emails |
| CREATE | Server/ClinicalIntelligence.Api/Services/Email/SmtpEmailSender.cs | SMTP sender enforcing TLS 1.2+ and emitting structured logs |
| MODIFY | Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj | Add required SMTP dependency (if not using built-in SMTP stack) |
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Register SMTP configuration and email sender in DI |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/smtp
- https://learn.microsoft.com/en-us/dotnet/core/extensions/options

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Manual/Debug] Start API locally and validate:
  - configuration is bound correctly (without logging secrets)
  - a test email can be sent in development using a sandbox SMTP provider
- [Security] Confirm TLS 1.2+ is negotiated/enforced by the SMTP client configuration.
- [Observability] Confirm logs contain success/failure status without exposing credentials or tokens.

## Implementation Checklist
- [x] Add `SmtpOptions` configuration model and bind it from configuration
- [x] Create `ISmtpEmailSender` abstraction (DIP)
- [x] Implement `SmtpEmailSender` with TLS 1.2+ enforcement
- [x] Add structured logging for send attempts and outcomes (no secrets)
- [x] Register SMTP sender and options in `Program.cs`
- [x] Validate locally with a sandbox SMTP server (manual)
