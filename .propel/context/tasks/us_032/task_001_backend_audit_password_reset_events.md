# Task - [TASK_001]

## Requirement Reference
- User Story: [us_032] (extracted from input)
- Story Location: [.propel/context/tasks/us_032/us_032.md]
- Acceptance Criteria: 
    - [Given a password reset is requested, When processed, Then PASSWORD_RESET_REQUESTED event is logged with email, IP, timestamp, token ID.]
    - [Given a password reset is completed, When successful, Then PASSWORD_RESET_COMPLETED event is logged with user ID, IP, timestamp, token ID.]
    - [Given a reset attempt with invalid token, When detected, Then the failed attempt is logged.]
    - [Given audit logging, When events are recorded, Then they include sufficient metadata for investigation.]

## Task Overview
Add security audit trail events for the password reset flow:
- Log `PASSWORD_RESET_REQUESTED` when a reset request is successfully processed.
- Log `PASSWORD_RESET_COMPLETED` when a reset is successfully completed.
- Log a failure event when an invalid/expired/used token reset attempt is detected.

Audit logging must be **best-effort** and must **not** change the externally observable behavior of the password reset endpoints (responses must remain consistent with existing non-enumeration behavior).

## Dependent Tasks
- [US_025 - Password reset token generation and storage]
- [US_030 - Password reset flow dependency]

## Impacted Components
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Add audit logging for forgot-password and reset-password flows (success + failure paths) without changing response behavior]
- [CREATE | Server/ClinicalIntelligence.Api/Services/Security/IAuditLogWriter.cs | Abstraction for best-effort persistence of `AuditLogEvent` entries]
- [CREATE | Server/ClinicalIntelligence.Api/Services/Security/AuditLogWriter.cs | Implementation that writes `AuditLogEvent` records with safe JSON metadata and swallows persistence failures]
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Register `IAuditLogWriter` in DI]

## Implementation Plan
- Introduce a small audit writer abstraction (DIP) to centralize:
  - constructing an `AuditLogEvent`
  - serializing metadata JSON
  - best-effort persistence (`try/catch` + `ILogger` warning)
- Define action types for this story:
  - `PASSWORD_RESET_REQUESTED`
  - `PASSWORD_RESET_COMPLETED`
  - `PASSWORD_RESET_FAILED` (or equivalent) for invalid/expired/used token attempts
- Implement `PASSWORD_RESET_REQUESTED` audit in `POST /api/v1/auth/forgot-password`:
  - Use existing `HttpContext` to capture `IpAddress` and `UserAgent`.
  - Capture the request email in metadata.
  - Ensure **no raw reset token** is logged.
  - Include a stable `tokenId` in metadata when available (recommended: the persisted `PasswordResetToken.Id`).
  - If the email does not match a user, preserve existing generic response behavior; audit logging must not introduce differences in response.
- Implement `PASSWORD_RESET_COMPLETED` audit in `POST /api/v1/auth/reset-password`:
  - After the password reset DB update succeeds, record `PASSWORD_RESET_COMPLETED` with:
    - `UserId`
    - `IpAddress`
    - `Timestamp`
    - `tokenId` (recommended: `PasswordResetToken.Id`)
  - Ensure audit persistence is best-effort and cannot cause the reset to fail.
- Implement invalid token attempt logging in `POST /api/v1/auth/reset-password`:
  - When an invalid/expired/used token is detected and the endpoint returns unauthorized, record a failure audit event with:
    - `IpAddress`
    - `Timestamp`
    - a safe reason code in metadata (e.g., `invalid_token`, `token_expired`)
    - do not log the raw token
- Match existing audit event conventions in `Program.cs`:
  - uppercase `ActionType` strings with underscores
  - `ResourceType` aligned to existing usage (e.g., `Auth`)
  - `Metadata` as JSON via `JsonSerializer.Serialize`
  - best-effort `try/catch` with `ILogger<Program>` warnings on persistence failure

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api/Services/Security/IAuditLogWriter.cs | Interface for best-effort audit event persistence for security events |
| CREATE | Server/ClinicalIntelligence.Api/Services/Security/AuditLogWriter.cs | Concrete implementation that writes to `ApplicationDbContext.AuditLogEvents` with safe metadata and exception shielding |
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Add calls to `IAuditLogWriter` in forgot-password and reset-password endpoints for requested/completed/failed events |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Manual/API] Call:
  - `POST /api/v1/auth/forgot-password` and confirm `PASSWORD_RESET_REQUESTED` is written to `audit_log_events` with required metadata.
  - `POST /api/v1/auth/reset-password` with a valid token and confirm `PASSWORD_RESET_COMPLETED` is written.
  - `POST /api/v1/auth/reset-password` with an invalid token and confirm a failure audit event is written.
- [Security] Validate that audit metadata contains **no raw reset tokens** and does not introduce account enumeration via response changes.
- [Resilience] Simulate audit persistence failure (e.g., DB write exception) and confirm the password reset endpoints still return the same responses as before.

## Implementation Checklist
- [ ] Add `IAuditLogWriter` abstraction for audit event persistence (DIP)
- [ ] Implement `AuditLogWriter` with safe JSON metadata serialization and best-effort persistence
- [ ] Register `IAuditLogWriter` in DI
- [ ] Add `PASSWORD_RESET_REQUESTED` audit event in forgot-password flow with email, IP, timestamp, and token ID (when available)
- [ ] Add `PASSWORD_RESET_COMPLETED` audit event in reset-password success flow with user ID, IP, timestamp, token ID
- [ ] Add failed reset attempt audit event for invalid/expired/used tokens (no raw token)
- [ ] Confirm audit failures do not break or alter reset flows
