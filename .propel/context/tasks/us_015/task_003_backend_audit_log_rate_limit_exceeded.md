# Task - [TASK_003]

## Requirement Reference
- User Story: [us_015]
- Story Location: [.propel/context/tasks/us_015/us_015.md]
- Acceptance Criteria: 
    - [Given a rate limit event occurs, When the limit is exceeded, Then the event is logged in the audit trail (RATE_LIMIT_EXCEEDED).]

## Task Overview
Log rate limit exceedance events for login attempts into the existing `audit_log_events` table using `AuditLogEvent` with `ActionType = RATE_LIMIT_EXCEEDED`. Estimated effort: ~3-5 hours.

## Dependent Tasks
- [TASK_001 - Backend add login rate limiter policy]
- [TASK_002 - Backend Retry-After and error body]

## Impacted Components
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Add audit log write when rate limiter rejects a login request]
- [MODIFY | Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs | Use `AuditLogEvents` DbSet to persist rate limit event]
- [MODIFY | Server/ClinicalIntelligence.Api/Domain/Models/AuditLogEvent.cs | Reuse existing model; ensure fields captured are appropriate (UserId may be null)]

## Implementation Plan
- On rate limit rejection for `/api/v1/auth/login`:
  - Create an `AuditLogEvent` with:
    - `UserId = null` (unauthenticated)
    - `SessionId = null`
    - `ActionType = RATE_LIMIT_EXCEEDED`
    - `IpAddress` and `UserAgent` captured from request
    - `ResourceType = "Auth"` (or similar stable value)
    - `Metadata` minimal JSON (e.g., endpoint path, limiter window/limit, retry-after seconds)
  - Persist the event via `ApplicationDbContext.AuditLogEvents`.
- Ensure this logging is best-effort:
  - If DB is unavailable, do not throw from the rejection handler; log via `ILogger` instead.

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Write `AuditLogEvent` when login rate limiter rejects a request |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/ef/core/

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Manual] Exceed the login rate limit and verify an `audit_log_events` row is created with `ActionType = RATE_LIMIT_EXCEEDED`.
- [Resilience] Simulate DB failure and confirm rate limiting still returns `429` (audit logging falls back to application logs).

## Implementation Checklist
- [x] Define `ActionType` value: `RATE_LIMIT_EXCEEDED`
- [x] Capture IP + user agent in audit event
- [x] Store minimal non-sensitive metadata
- [x] Persist audit event best-effort (no failures affecting 429 response)
- [x] Validate audit event creation for rate limit exceedance
