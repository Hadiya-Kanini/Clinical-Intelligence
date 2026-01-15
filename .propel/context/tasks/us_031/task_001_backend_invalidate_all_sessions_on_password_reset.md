# Task - [TASK_001]

## Requirement Reference
- User Story: [us_031]
- Story Location: [.propel/context/tasks/us_031/us_031.md]
- Acceptance Criteria: 
    - [Given a password is successfully reset, When completed, Then all existing sessions for that user are invalidated.]
    - [Given session invalidation, When a previous session token is used, Then the API returns 401 Unauthorized.]
    - [Given session invalidation, When completed, Then the user must re-authenticate with the new password.]
    - [Given the reset process, When sessions are invalidated, Then the action is logged in the audit trail.]

## Task Overview
Extend the password reset completion flow to revoke **all existing sessions** for the user whose password was reset. This ensures that any potentially compromised tokens become unusable immediately after a successful password reset.

The codebase already supports session revocation via the `sessions` table and JWT `sid` claim validation (revocation checks happen in JWT bearer validation and `SessionTrackingMiddleware`). This task wires that capability into the `POST /api/v1/auth/reset-password` endpoint.

## Dependent Tasks
- [US_030] (Password reset confirm flow exists and must be updated)
- [US_029 TASK_002 - Backend reset password confirm endpoint and token consumption]

## Impacted Components
- [CREATE | Server/ClinicalIntelligence.Api/Services/Auth/ISessionInvalidationService.cs | Abstraction for invalidating all sessions for a user (testable, DIP)]
- [CREATE | Server/ClinicalIntelligence.Api/Services/Auth/SessionInvalidationService.cs | EF Core implementation that revokes all active sessions for a user]
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Update `POST /api/v1/auth/reset-password` to revoke all user sessions and write an audit event after successful reset]

## Implementation Plan
- Add a session invalidation abstraction:
  - Create `ISessionInvalidationService` exposing a method like `InvalidateAllSessionsAsync(Guid userId, CancellationToken ct)`.
  - Implement `SessionInvalidationService` using `ApplicationDbContext` to:
    - find sessions for `userId` where `IsRevoked == false` and `ExpiresAt > DateTime.UtcNow` (and/or any non-revoked sessions, depending on desired strictness)
    - set `IsRevoked = true` for each session
    - return count of revoked sessions (to support audit metadata)
- Apply invalidation in reset-password completion:
  - In `POST /api/v1/auth/reset-password` (existing minimal API handler in `Program.cs`):
    - after validating token and updating the password + consuming the token, call `ISessionInvalidationService.InvalidateAllSessionsAsync(user.Id, ct)`
    - persist an `AuditLogEvent` with:
      - `UserId = user.Id`
      - `SessionId = null` (because reset-password is unauthenticated)
      - `ActionType = "PASSWORD_RESET_SESSIONS_INVALIDATED"` (or consistent naming per audit conventions)
      - `IpAddress` and `UserAgent` from the current request
      - `ResourceType = "Session"` (or "Auth")
      - `Metadata` including revoked session count (do not include tokens/passwords)
- Ensure atomicity / failure handling:
  - Wrap password update + token consumption + session revocation + audit insert in a single transaction.
  - If session invalidation fails, fail the request and rollback, so the system never ends up in a state where the password changed but old sessions remain valid.

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api/Services/Auth/ISessionInvalidationService.cs | Interface for invalidating all sessions for a user (used by reset-password flow) |
| CREATE | Server/ClinicalIntelligence.Api/Services/Auth/SessionInvalidationService.cs | EF Core implementation that revokes all active sessions and returns revoked count |
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | In reset-password flow, revoke all sessions for the user and add an audit log event inside a transaction |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://cheatsheetseries.owasp.org/cheatsheets/Forgot_Password_Cheat_Sheet.html
- https://learn.microsoft.com/en-us/ef/core/saving/transactions

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Manual/API] Create two active sessions for the same user (e.g., login from two clients), then perform `POST /api/v1/auth/reset-password`, and verify both sessions are rejected with `401` when calling a protected endpoint.
- [Security] Confirm no reset token, password, or session identifier values are written into logs/audit metadata.
- [Auditability] Confirm an audit event is persisted when sessions are invalidated.

## Implementation Checklist
- [ ] Add `ISessionInvalidationService` abstraction
- [ ] Implement `SessionInvalidationService` to revoke sessions for a user (return revoked count)
- [ ] Register the new service in DI (`Program.cs`)
- [ ] Update `POST /api/v1/auth/reset-password` to revoke all sessions after successful password reset
- [ ] Write an `AuditLogEvent` for the invalidation action (safe metadata only)
- [ ] Ensure password update + token consumption + session invalidation + audit write are atomic (transaction)
- [ ] Validate old JWTs return `401` after reset
