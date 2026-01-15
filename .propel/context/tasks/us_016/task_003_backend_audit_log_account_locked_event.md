# Task - [TASK_003]

## Requirement Reference
- User Story: [us_016]
- Story Location: [.propel/context/tasks/us_016/us_016.md]
- Acceptance Criteria: 
    - [Given an account lockout occurs, When the lockout is triggered, Then an ACCOUNT_LOCKED event is logged in the audit trail.]

## Task Overview
Persist an audit trail entry when an account lockout is triggered due to failed login attempts, using the existing `audit_log_events` table. Estimated effort: ~3-5 hours.

## Dependent Tasks
- [TASK_001 - Backend account lockout policy 30 minutes]

## Impacted Components
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Insert `AuditLogEvent` when lockout is triggered]
- [MODIFY | Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs | Use existing `AuditLogEvents` DbSet to persist the event]
- [MODIFY | Server/ClinicalIntelligence.Api/Domain/Models/AuditLogEvent.cs | Reuse existing model; ensure appropriate metadata is captured]

## Implementation Plan
- Detect lockout trigger point:
  - When failed attempts reach the threshold and `LockedUntil` is newly set (lockout transition), insert an `AuditLogEvent`.
- Populate the audit event:
  - `ActionType = ACCOUNT_LOCKED`
  - `UserId = user.Id`
  - `SessionId = null` (not authenticated)
  - `IpAddress` and `UserAgent` from the request
  - `Metadata` minimal JSON including `unlock_at` timestamp and threshold count
- Ensure best-effort logging:
  - If audit write fails, do not break the login endpoint flow; log via `ILogger`.

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Insert `AuditLogEvent` with `ActionType = ACCOUNT_LOCKED` when lockout is triggered |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/ef/core/

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Manual] Trigger lockout and verify `audit_log_events` contains an `ACCOUNT_LOCKED` entry.
- [Security] Ensure metadata does not include passwords or tokens.

## Implementation Checklist
- [ ] Define `ActionType` value: `ACCOUNT_LOCKED`
- [ ] Detect lockout transition (avoid logging on every locked retry)
- [ ] Capture request IP + user agent
- [ ] Store minimal metadata including `unlock_at`
- [ ] Persist event best-effort without impacting response flow
