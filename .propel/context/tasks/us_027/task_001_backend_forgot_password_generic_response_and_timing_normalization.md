# Task - [TASK_001]

## Requirement Reference
- User Story: [us_027]
- Story Location: [.propel/context/tasks/us_027/us_027.md]
- Acceptance Criteria: 
    - [Given any email is submitted for reset, When processed, Then the same generic "check your email" message is displayed.]
    - [Given an email that exists in the system, When reset is requested, Then the response is identical to non-existent emails.]
    - [Given response timing, When implemented, Then consistent response times prevent timing-based enumeration.]

## Task Overview
Harden the backend password reset request flow against account enumeration by ensuring:
- a consistent, generic HTTP response contract for syntactically valid reset requests, regardless of whether the email exists or whether email delivery succeeds
- response timing is normalized so observable latency does not reveal account existence

This task is specifically about **non-enumeration and timing**. Token generation, SMTP delivery, and link construction remain owned by the dependent user stories.

## Dependent Tasks
- [US_024 TASK_002 - Backend forgot password endpoint (generic response)]
- [US_026 TASK_002 - Backend forgot password sends reset email via SMTP] (if the email delivery flow is already implemented and needs timing normalization)

## Impacted Components
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Ensure `POST /api/v1/auth/forgot-password` returns a generic response for syntactically valid input and normalizes response time]
- [CREATE | Server/ClinicalIntelligence.Api/Configuration/ForgotPasswordResponseTimingOptions.cs | Configurable minimum response time and optional jitter for the forgot-password endpoint]
- [CREATE | Server/ClinicalIntelligence.Api/Services/Auth/IResponseTimingNormalizer.cs | Abstraction to normalize response timing (DIP for testability)]
- [CREATE | Server/ClinicalIntelligence.Api/Services/Auth/ResponseTimingNormalizer.cs | Implementation using elapsed time measurement and async delay]

## Implementation Plan
- Add options:
  - Create `ForgotPasswordResponseTimingOptions` with a minimum response delay for syntactically valid requests (and optional jitter).
  - Bind it from configuration (with safe defaults for production and override-friendly values for tests).
- Add timing normalization service:
  - Implement `IResponseTimingNormalizer` that:
    - starts a timer (e.g., `Stopwatch`) at request start
    - after performing the endpoint logic, delays for the remaining time required to reach the configured minimum
    - uses `Task.Delay` with the request cancellation token
- Apply to forgot-password flow:
  - In `POST /api/v1/auth/forgot-password` (from dependent tasks), wrap processing so that:
    - syntactically valid email requests always return the same status code and response body
    - lookup/email send paths do not leak existence via different status or payload
    - exceptions (e.g., SMTP failure) are logged but still return the same generic 200 response for syntactically valid emails
    - timing normalization is applied consistently for syntactically valid requests
  - Keep validation errors (missing/invalid email) as `400` with the standard error shape (do not apply normalization to invalid input).
- Security notes:
  - Do not include any user existence indicators in the response.
  - Do not log reset tokens or sensitive values.

## Current Project State
- Timing normalization service implemented with configurable minimum delay (default 500ms)
- Generic response returned for all syntactically valid requests regardless of user existence
- SMTP/email failures do not change the response (logged only)
- Invalid input (400) responses are NOT subject to timing normalization

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api/Configuration/ForgotPasswordResponseTimingOptions.cs | Options for minimum response delay (and optional jitter) for forgot-password requests |
| CREATE | Server/ClinicalIntelligence.Api/Services/Auth/IResponseTimingNormalizer.cs | Interface for timing normalization to support deterministic testing |
| CREATE | Server/ClinicalIntelligence.Api/Services/Auth/ResponseTimingNormalizer.cs | Applies minimum response duration using elapsed timing + async delay |
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Apply generic response + timing normalization to `POST /api/v1/auth/forgot-password` |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html
- https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.stopwatch
- https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.delay

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Manual/API] POST a syntactically valid email for an existing and non-existing account and confirm HTTP status and response body are identical.
- [Security] Confirm there is no observable timing delta large enough to reliably distinguish existing vs non-existing accounts.
- [Observability] Confirm logs capture operational failures without any secrets.

## Implementation Checklist
- [x] Add `ForgotPasswordResponseTimingOptions` and bind from configuration
- [x] Add `IResponseTimingNormalizer` + `ResponseTimingNormalizer`
- [x] Register options + service in DI
- [x] Update forgot-password endpoint to return a stable generic 200 response for syntactically valid input regardless of user existence
- [x] Ensure SMTP/email failures do not change the response (log only)
- [x] Apply timing normalization for syntactically valid requests (do not apply to invalid input)
- [x] Verify no sensitive information is logged (no tokens, no existence indicators)
