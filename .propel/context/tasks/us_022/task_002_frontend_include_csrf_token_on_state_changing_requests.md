# Task - [TASK_002]

## Requirement Reference
- User Story: [us_022]
- Story Location: [.propel/context/tasks/us_022/us_022.md]
- Acceptance Criteria: 
    - [Given the frontend, When making state-changing requests, Then it includes the CSRF token in headers.]
    - [Given a request without valid CSRF token, When submitted, Then the API returns 403 Forbidden.]

## Task Overview
Update the frontend request layer so state-changing requests automatically include the CSRF token header. Ensure token retrieval is handled safely and works with cookie-based authentication. Estimated effort: ~4-6 hours.

## Dependent Tasks
- [TASK_001 - Backend CSRF token issuance and validation]

## Impacted Components
- [MODIFY | app/src/lib/apiClient.ts | Fetch/cache CSRF token and inject `X-CSRF-TOKEN` header for POST/PUT/PATCH/DELETE]
- [MODIFY | app/src/store/slices/authSlice.ts | Ensure logout uses the centralized client (already does) and receives CSRF header automatically]
- [MODIFY | app/src/pages/LoginPage.tsx | Align login flow with centralized auth handling if required to support CSRF token retrieval post-login]

## Implementation Plan
- Establish CSRF token acquisition flow:
  - After authentication (cookie established), call `GET /api/v1/auth/csrf` to retrieve a CSRF token.
  - Store token in-memory (and/or session-scoped storage if required for refresh scenarios) to avoid long-lived storage.
- Inject CSRF header:
  - In `apiClient.ts`, automatically add `X-CSRF-TOKEN: <token>` for state-changing methods.
  - Ensure header injection does not apply to safe methods (GET/HEAD) and does not break existing JSON headers.
- Handle 403 CSRF failures:
  - If server returns `403` due to invalid/expired CSRF token, refresh token once via `GET /api/v1/auth/csrf` and retry the original request once.
  - Avoid retry loops.
- Normalize usage:
  - Reduce direct `fetch` calls in feature pages that bypass `apiClient.ts` where they make state-changing calls.

## Current Project State
- apiClient.ts updated with CSRF token retrieval and caching
- X-CSRF-TOKEN header automatically injected for POST/PUT/PATCH/DELETE requests
- One-time retry logic implemented for CSRF-related 403 errors
- clearCsrfToken export added for manual token invalidation

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | app/src/lib/apiClient.ts | Add CSRF token retrieval, caching, header injection, and single-retry on CSRF 403 |
| MODIFY | app/src/store/slices/authSlice.ts | Validate logout/login flows work with CSRF-enabled backend (minimal changes expected) |
| MODIFY | app/src/pages/LoginPage.tsx | Ensure state-changing auth calls use shared client or include required CSRF retrieval step after login |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://react.dev/reference/react/useEffect
- https://developer.mozilla.org/en-US/docs/Web/API/Fetch_API

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Manual] Login, then logout; confirm logout succeeds (200) and no 403 CSRF errors occur.
- [Manual] Temporarily force an invalid CSRF token and confirm the client refreshes token and retries once.

## Implementation Checklist
- [x] Add CSRF token retrieval helper (calls `GET /api/v1/auth/csrf` with credentials)
- [x] Cache CSRF token in request layer and inject `X-CSRF-TOKEN` for POST/PUT/PATCH/DELETE
- [x] Implement one-time retry on CSRF-related 403 responses
- [x] Confirm logout flow still clears legacy localStorage keys and navigates correctly
- [x] Confirm any direct state-changing `fetch` calls are migrated to use `apiClient.ts`
