# Task - [TASK_002]

## Requirement Reference
- User Story: [us_028]
- Story Location: [.propel/context/tasks/us_028/us_028.md]
- Acceptance Criteria: 
    - [Given rate limit is exceeded, When HTTP 429 is returned, Then the response includes Retry-After header.]
    - [Given the UI, When rate limit is hit, Then it displays the retry timeframe (UXR-010).]

## Task Overview
Update the Forgot Password UI (`/forgot-password`) to call the backend forgot-password endpoint and handle rate limiting responses.

When the backend returns HTTP `429`, the UI must:
- read the `Retry-After` response header (seconds)
- display a user-friendly message including the retry timeframe per UXR-010

This task should preserve the generic "check email" success behavior for syntactically valid email submissions.

## Dependent Tasks
- [US_024 TASK_001 - Implement forgot password page with email input] (page exists)
- [US_024 TASK_002 - Backend forgot password endpoint (generic response)]
- [US_028 TASK_001 - Backend forgot password rate limiting policy]

## Impacted Components
- [MODIFY | app/src/pages/ForgotPasswordPage.tsx | Call backend endpoint on submit; handle 429 by displaying retry timeframe from Retry-After]
- [MODIFY | app/src/lib/apiClient.ts | Extend API client to surface response headers needed for UX (at minimum `Retry-After`) when requests fail]
- [MODIFY | app/src/components/ui/Alert.tsx | If needed, ensure error variant supports the required UX copy for rate limiting]

## Implementation Plan
- Extend API client to surface relevant headers:
  - Update `ApiResult<T>` (failure branch) to include a minimal `headers` projection (e.g., `{ retryAfterSeconds?: number }`) derived from `Response.headers.get('Retry-After')`.
  - Keep behavior unchanged for existing callers; only add optional fields.
- Update `ForgotPasswordPage` submit behavior:
  - Replace the current client-only `setStatus('success')` with an async request to `POST /api/v1/auth/forgot-password` using the shared `api` client.
  - Maintain existing validation behavior (do not call API if client validation fails).
  - On `success: true`, show the existing success alert text (generic, non-enumerating).
  - On `success: false`:
    - If `status === 429`, read `Retry-After` from the new API client field and display an error alert like:
      - "Too many reset requests. Please try again in X minutes." (derive minutes/seconds from header)
    - For other errors, display a generic failure message without exposing account existence.

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | app/src/lib/apiClient.ts | Extend error results to include `Retry-After` header value for 429 handling |
| MODIFY | app/src/pages/ForgotPasswordPage.tsx | Call `POST /api/v1/auth/forgot-password` and display rate-limit retry timeframe on 429 |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Retry-After

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Manual/UI] Simulate HTTP 429 (mock service worker / dev tools / backend config) and verify the UI displays retry timeframe.
- [Manual/UI] Verify successful submissions still show the generic "check email" message.
- [Regression] Ensure other API calls using `apiClient` are unaffected by the added optional header field.

## Implementation Checklist
- [ ] Extend `apiClient` failure results to include parsed `Retry-After` when present
- [ ] Wire `ForgotPasswordPage` to call `POST /api/v1/auth/forgot-password`
- [ ] Handle 429 by showing a user-friendly retry timeframe message (UXR-010)
- [ ] Keep generic success UX (no enumeration)
- [ ] Manually verify behavior with a simulated/exercised rate limit
