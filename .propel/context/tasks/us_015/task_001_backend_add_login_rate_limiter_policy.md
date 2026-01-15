# Task - [TASK_001]

## Requirement Reference
- User Story: [us_015]
- Story Location: [.propel/context/tasks/us_015/us_015.md]
- Acceptance Criteria: 
    - [Given an IP address makes login attempts, When 5 attempts are made within 1 minute, Then subsequent attempts return HTTP 429 Too Many Requests.]
    - [Given rate limiting is active, When the rate limit window expires, Then the IP can attempt login again.]

## Task Overview
Implement backend login rate limiting for `POST /api/v1/auth/login` at **5 attempts per minute per IP address** using ASP.NET Core rate limiting. Ensure requests over the limit are rejected with HTTP `429`. Estimated effort: ~4-6 hours.

## Dependent Tasks
- [US_011 - Implement JWT authentication with HttpOnly cookies]

## Impacted Components
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Register and enable ASP.NET Core rate limiter middleware and apply a policy to `/api/v1/auth/login`]
- [CREATE | Server/ClinicalIntelligence.Api/Configuration/RateLimitingOptions.cs | Strongly typed configuration for login rate limit values (permit limit + window)]

## Implementation Plan
- Add a dedicated rate limiting configuration model (defaults aligned to story):
  - Permit limit: `5`
  - Window: `60 seconds`
  - Partition key: client IP address
- Register rate limiter services in `Program.cs`:
  - Use `builder.Services.AddRateLimiter(...)` with a fixed window or sliding window limiter.
  - Use `PartitionedRateLimiter` keyed by the request IP for the login endpoint.
- Enable middleware:
  - Call `app.UseRateLimiter()` early enough to protect the endpoint.
  - Apply the policy only to `POST /api/v1/auth/login` (avoid globally rate-limiting unrelated endpoints).
- Confirm rate limiter behavior meets edge cases:
  - NAT/shared IP behavior is expected; document as a known tradeoff.
  - Ensure the limiter window resets naturally so the IP can retry after the configured window.

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Configure and enable rate limiting; apply login-specific policy to `/api/v1/auth/login` |
| CREATE | Server/ClinicalIntelligence.Api/Configuration/RateLimitingOptions.cs | Define rate limiting configuration for login policy (permit limit + window seconds) |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Manual] Perform 6 rapid login attempts from the same IP and confirm attempt 6 returns `429`.
- [Manual] Wait for the configured window and confirm login attempts succeed again.

## Implementation Checklist
- [x] Add `RateLimitingOptions` config model with sensible defaults
- [x] Register `AddRateLimiter` services with an IP-partitioned limiter for login
- [x] Add `UseRateLimiter` middleware
- [x] Apply policy only to `POST /api/v1/auth/login`
- [x] Validate limiter resets after the configured window
- [x] Document known NAT/shared IP tradeoff
