# Task - [TASK_003]

## Requirement Reference
- User Story: [us_014]
- Story Location: [.propel/context/tasks/us_014/us_014.md]
- Acceptance Criteria: 
    - [Given concurrent session prevention is active, When a user logs in, Then the system logs the session replacement event in the audit trail.]

## Task Overview
Record an audit trail entry whenever a user login revokes one or more existing active sessions (session replacement). This supports security monitoring and credential sharing deterrence. Estimated effort: ~4-6 hours.

## Dependent Tasks
- [US_011 - Implement JWT authentication with HttpOnly cookies]
- [US_012 - Implement session tracking and inactivity timeout]
- [TASK_001 - Backend single active session on login]

## Impacted Components
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Add audit log write during login when previous sessions are revoked]
- [MODIFY | Server/ClinicalIntelligence.Api/Domain/Models/AuditLogEvent.cs | Confirm fields support session replacement metadata and integrity hash strategy (if required)]
- [MODIFY | Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs | Ensure `AuditLogEvents` persistence is used for session replacement events]
- [CREATE/MODIFY | Server/ClinicalIntelligence.Api.Tests/* | Add integration test that asserts audit event is written when session replacement occurs]

## Implementation Plan
- During `/api/v1/auth/login` (after authentication succeeds):
  - Detect whether any active sessions were revoked as part of the login.
  - If yes, insert an `AuditLogEvent` with:
    - `UserId` set to the logged-in user
    - `SessionId` set to the newly created session id
    - `ActionType` set to a consistent value (e.g., `SESSION_REPLACED`)
    - `IpAddress` and `UserAgent` set from the request
    - `Metadata` containing minimally necessary context (e.g., count of revoked sessions; optionally list of revoked session ids)
  - Persist as part of the same transaction as session revocation/creation to avoid partial updates.
- Define test strategy:
  - Add an integration test that logs in twice as the same user and asserts an audit log event exists for the second login.

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Insert `AuditLogEvent` when a login revokes existing sessions for the same user |
| CREATE/MODIFY | Server/ClinicalIntelligence.Api.Tests/* | Integration test verifying audit event is written for session replacement |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/ef/core/

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Integration test] Login twice and assert `audit_log_events` contains a `SESSION_REPLACED` entry for the second login.
- [Security] Ensure metadata does not include sensitive information (no tokens, no passwords).

## Implementation Checklist
- [ ] Define the audit `ActionType` constant for session replacement
- [ ] Capture request IP + user agent for the audit event
- [ ] Persist `AuditLogEvent` when session replacement occurs
- [ ] Store minimal metadata (count and/or ids)
- [ ] Ensure audit write happens transactionally with session updates
- [ ] Add integration test for audit trail insertion
