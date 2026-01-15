# Task - [TASK_003]

## Requirement Reference
- User Story: [us_028]
- Story Location: [.propel/context/tasks/us_028/us_028.md]
- Acceptance Criteria: 
    - [Given an IP makes password reset requests, When 3 requests are made within 1 hour, Then subsequent requests return HTTP 429.]
    - [Given rate limit is exceeded, When HTTP 429 is returned, Then the response includes Retry-After header.]
    - [Given rate limiting, When triggered, Then the event is logged (RATE_LIMIT_EXCEEDED).]

## Task Overview
Add automated backend integration tests validating the forgot-password endpoint rate limiting behavior (3 requests per hour per IP), including:
- HTTP 429 on the request exceeding the permit limit
- presence and validity of `Retry-After` header
- best-effort audit logging of `RATE_LIMIT_EXCEEDED`

Tests should follow the existing pattern used for login rate limiting in `LoginRateLimitingTests.cs`.

## Dependent Tasks
- [US_024 TASK_002 - Backend forgot password endpoint (generic response)]
- [US_028 TASK_001 - Backend forgot password rate limiting policy]

## Impacted Components
- [CREATE | Server/ClinicalIntelligence.Api.Tests/Integration/ForgotPasswordRateLimitingTests.cs | Integration tests for forgot-password rate limiting, Retry-After header, and audit logging]

## Implementation Plan
- Create `ForgotPasswordRateLimitingTests` using `WebApplicationFactory<Program>`.
- Configure rate limiting in the test host with a small window for deterministic tests:
  - Override `RateLimiting:*` options for forgot-password (permit limit = 3, window seconds = 2).
- Execute repeated requests to `POST /api/v1/auth/forgot-password` from the same test client:
  - Assert first 3 requests are not 429.
  - Assert the next request returns 429.
- Validate headers:
  - Confirm `Retry-After` header exists.
  - Confirm header parses as positive integer.
- Validate response shape:
  - Confirm JSON body contains `error.code` (expected `rate_limited`) and non-empty `error.message`.
- Validate audit logging (best-effort, same approach as login limiter tests):
  - If PostgreSQL is available (same `IsPostgreSqlAvailable()` guard pattern), query `AuditLogEvents` for a new `RATE_LIMIT_EXCEEDED` event and assert metadata includes the endpoint path.

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api.Tests/Integration/ForgotPasswordRateLimitingTests.cs | Tests for forgot-password 3/hour/IP rate limiting, `Retry-After` header, standardized JSON error, and `RATE_LIMIT_EXCEEDED` audit event |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests
- https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Automated] Run API test suite and confirm new tests are deterministic (short window override) and do not require sleeping for long durations.

## Implementation Checklist
- [x] Add `ForgotPasswordRateLimitingTests` following the structure of `LoginRateLimitingTests`
- [x] Override forgot-password limiter settings for short deterministic test window
- [x] Assert 429 occurs after 3 requests within the window
- [x] Assert `Retry-After` header exists and parses
- [x] Assert JSON error response shape and code
- [x] Assert `RATE_LIMIT_EXCEEDED` audit record is created (when DB is available)
