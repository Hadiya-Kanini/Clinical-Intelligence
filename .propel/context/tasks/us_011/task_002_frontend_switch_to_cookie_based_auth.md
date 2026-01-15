# Task - TASK_002

## Requirement Reference
- User Story: us_011
- Story Location: .propel/context/tasks/us_011/us_011.md
- Acceptance Criteria: 
    - AC-2: Given a JWT is generated, When the response is sent, Then the token is stored in an HttpOnly, Secure cookie (not accessible via JavaScript).
    - AC-3: Given a user has a valid JWT cookie, When they make authenticated requests, Then the Backend API validates the token and authorizes the request.
    - AC-4: Given a JWT is invalid or expired, When the user makes a request, Then the API returns 401 Unauthorized.

## Task Overview
Update the React frontend authentication flow to stop storing the JWT in `localStorage` and instead rely on cookie-based authentication. Ensure all authenticated API calls work by using credentialed requests and the existing `/api/v1/auth/me` endpoint as the source of truth for the current session.
Estimated Effort: 6 hours

## Dependent Tasks
- .propel/context/tasks/us_011/task_001_backend_issue_jwt_via_httponly_cookie.md (TASK_001)

## Impacted Components
- app/src/store/slices/authSlice.ts
- app/src/main.tsx (or existing app bootstrap where axios is configured, if present)

## Implementation Plan
- Remove token persistence in browser storage:
  - Delete the logic that writes `ci_token` and related auth flags to `localStorage`.
  - Ensure logout removes any legacy `localStorage` keys for backward compatibility.
- Switch API calls to cookie-based auth:
  - Configure axios requests to include cookies (`withCredentials: true`) for calls to the backend.
  - Update authenticated calls (e.g., `checkAuthAsync`) to call `/api/v1/auth/me` without `Authorization: Bearer`.
- Update login flow:
  - On successful login, treat authentication success as a session cookie being set server-side.
  - Use the login response data (if any) only for UI state (e.g., `user`), not for storing tokens.
- Update logout flow:
  - Ensure logout calls `/api/v1/auth/logout` and clears client-side auth state.
- Handle expired/missing cookie cases:
  - Ensure `checkAuthAsync` and any protected-route logic handles 401 by clearing auth state and redirecting to login if applicable.

**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | app/src/store/slices/authSlice.ts | Remove JWT storage in `localStorage`, remove `Authorization` header usage, and make auth state rely on cookie-backed `/api/v1/auth/me` |
| MODIFY | app/src/main.tsx | Configure axios default `withCredentials` (or establish a shared API client module if one already exists) |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://axios-http.com/docs/config_defaults
- https://developer.mozilla.org/docs/Web/HTTP/Cookies

## Build Commands
- npm --prefix app run build
- npm --prefix app run test

## Implementation Validation Strategy
- Login from the UI and confirm:
  - No JWT is written to `localStorage`.
  - The backend session works via cookies (subsequent `/api/v1/auth/me` returns 200).
- Refresh the page and confirm session still resolves via `/api/v1/auth/me` without relying on `localStorage`.
- Confirm that when the cookie is expired/cleared, the UI transitions to unauthenticated state and protected calls fail gracefully (401).

## Implementation Checklist
- [x] Remove token persistence (`ci_token`) from `localStorage` and clear legacy keys on logout
- [x] Configure axios to send credentialed requests (`withCredentials: true`)
- [x] Update `checkAuthAsync` to use `/api/v1/auth/me` without `Authorization` header
- [x] Update `loginAsync` to not store token and to set authenticated state based on response/session
- [x] Update `logoutAsync` to call backend logout and clear client-side auth state
- [x] Run frontend unit tests and build
