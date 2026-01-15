# Task - [TASK_001]

## Requirement Reference
- User Story: [us_028]
- Story Location: [.propel/context/tasks/us_028/us_028.md]
- Acceptance Criteria: 
    - [Given an IP makes password reset requests, When 3 requests are made within 1 hour, Then subsequent requests return HTTP 429.]
    - [Given rate limit is exceeded, When HTTP 429 is returned, Then the response includes Retry-After header.]
    - [Given rate limiting, When triggered, Then the event is logged (RATE_LIMIT_EXCEEDED).]

## Task Overview
Implement per-IP rate limiting for the forgot-password endpoint (`POST /api/v1/auth/forgot-password`) with a limit of 3 requests per 1-hour window.

This task must:
- enforce HTTP 429 for requests beyond the limit
- include a `Retry-After` header that reflects the remaining window time
- preserve best-effort audit logging of `RATE_LIMIT_EXCEEDED` without altering the 429 response behavior

## Dependent Tasks
- [US_024 TASK_002 - Backend forgot password endpoint (generic response)]
- [US_027 TASK_001 - Backend forgot password generic response and timing normalization] (if timing normalization is already required for non-enumeration)

## Impacted Components
- [MODIFY | Server/ClinicalIntelligence.Api/Configuration/RateLimitingOptions.cs | Extend options to include forgot-password policy configuration (permit limit + window seconds + policy name)]
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Add a dedicated rate limiting policy for forgot-password and apply it to `POST /api/v1/auth/forgot-password`]
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Ensure 429 rejection handling writes a correct message for forgot-password and continues to log RATE_LIMIT_EXCEEDED]

## Implementation Plan
- Extend `RateLimitingOptions`:
  - Add constants for a forgot-password policy name (e.g., `ForgotPasswordPolicyName`).
  - Add configuration properties for forgot-password rate limiting (e.g., `ForgotPasswordPermitLimit`, `ForgotPasswordWindowSeconds`, and a `TimeSpan` convenience property).
- Configure rate limiter policies in `Program.cs`:
  - Keep the existing login policy unchanged.
  - Add a new fixed-window limiter policy keyed by `RemoteIpAddress` for forgot-password using 3/hour defaults.
- Rejection handling:
  - Ensure the global rejection handler (or policy-specific handling) emits:
    - status code `429`
    - `Retry-After` header derived from `MetadataName.RetryAfter`
    - standardized JSON error shape compatible with existing login limiter tests
    - message appropriate to the endpoint (avoid “Too many login attempts” for forgot-password).
- Audit logging:
  - Ensure the existing best-effort DB insert for `RATE_LIMIT_EXCEEDED` continues to run.
  - Ensure audit metadata includes the endpoint path and the configured permit/window.

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Configuration/RateLimitingOptions.cs | Add forgot-password rate limiting configuration fields and policy name constant |
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Add a forgot-password rate limiting policy (3/hour/IP) and apply it to `POST /api/v1/auth/forgot-password` |
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Update 429 rejection response message selection so forgot-password returns a correct message while keeping Retry-After + audit logging |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit
- https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.ratelimiting
- https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Manual/API] Call `POST /api/v1/auth/forgot-password` 3 times from the same IP and verify the 4th returns `429`.
- [Manual/API] Verify `Retry-After` header exists and is a positive integer.
- [Manual/DB] Verify an `AUDIT_LOG_EVENT` record is written with `ActionType = RATE_LIMIT_EXCEEDED` and includes the endpoint path.

## Implementation Checklist
- [ ] Extend `RateLimitingOptions` to include forgot-password policy settings
- [ ] Register forgot-password rate limit policy in `Program.cs` (3/hour/IP)
- [ ] Apply `.RequireRateLimiting(...)` to `POST /api/v1/auth/forgot-password`
- [ ] Ensure rejection handler returns standardized JSON + `Retry-After`
- [ ] Ensure 429 message is correct for forgot-password requests
- [ ] Confirm audit logging still records `RATE_LIMIT_EXCEEDED` for this endpoint
- [ ] Validate behavior manually with repeated requests from the same IP
