# Task - [TASK_002]

## Requirement Reference
- User Story: [us_015]
- Story Location: [.propel/context/tasks/us_015/us_015.md]
- Acceptance Criteria: 
    - [Given rate limit is exceeded, When HTTP 429 is returned, Then the response includes a Retry-After header indicating when to retry.]

## Task Overview
Enhance the login rate limiting rejection response to include a `Retry-After` header and a consistent JSON error body using `ApiErrorResults.TooManyRequests`. Estimated effort: ~2-4 hours.

## Dependent Tasks
- [TASK_001 - Backend add login rate limiter policy]

## Impacted Components
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Configure rate limiter rejection callback to set `Retry-After` and write standardized error response]
- [MODIFY | Server/ClinicalIntelligence.Api/Results/ApiErrorResults.cs | Reuse existing `TooManyRequests` helper for standardized 429 body (no new result type expected)]

## Implementation Plan
- Configure a rate limiter rejection handler:
  - In the rate limiter configuration, implement `OnRejected` callback.
  - Extract `RetryAfter` metadata (when available) and set `Retry-After` header.
  - Return a `429` JSON response using `ApiErrorResults.TooManyRequests` with a clear, non-sensitive message.
- Ensure the error payload is stable and does not leak details:
  - Do not include whether the email exists.
  - Do not include internal limiter keys.

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Implement rate limiter `OnRejected` handler to set `Retry-After` and write standardized 429 response body |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Manual] Exceed login rate limit and confirm:
  - Response status is `429`
  - `Retry-After` header exists
  - Response is JSON and follows the API error envelope

## Implementation Checklist
- [ ] Add rate limiter `OnRejected` handler
- [ ] Populate `Retry-After` header from limiter metadata
- [ ] Return standardized JSON error using `ApiErrorResults.TooManyRequests`
- [ ] Validate no sensitive information is present in the error body
