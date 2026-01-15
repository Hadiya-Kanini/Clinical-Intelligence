# Task - [TASK_012_003]

## Requirement Reference
- User Story: [us_012] (extracted from input)
- Story Location: [.propel/context/tasks/us_012/us_012.md]
- Acceptance Criteria: 
    - [Given a session is terminated due to inactivity, When the user attempts any action, Then they are redirected to the login page with a session expired message.]
    - [Given a user performs any action (API call, navigation), When the action is processed, Then the session's last activity timestamp is updated.]

## Task Overview
Implement a consistent frontend mechanism for authenticated API calls that:
- Adds the auth token to requests
- Detects session-expired responses from the backend
- Clears local auth state and triggers the existing logout redirect flow

This ensures the app responds correctly when the backend expires a session due to inactivity.

## Dependent Tasks
- [TASK_012_001 - Backend session tracking and inactivity timeout]

## Impacted Components
- [CREATE: app/src/lib/apiClient.ts]
- [MODIFY: app/src/store/slices/authSlice.ts]
- [MODIFY: app/src/pages/DashboardPage.tsx]
- [MODIFY: app/src/pages/DocumentUploadPage.tsx]
- [MODIFY: app/src/pages/DocumentListPage.tsx]
- [MODIFY: app/src/pages/Patient360Page.tsx]
- [MODIFY: app/src/pages/AdminDashboardPage.tsx]
- [MODIFY: app/src/pages/UserManagementPage.tsx]

## Implementation Plan
- [Create `apiClient.ts` as the single place to perform HTTP calls to `/api/v1/*`.]
- [In the API client:]
- [Attach `Authorization: Bearer <ci_token>` header when present.]
- [On HTTP 401 responses, inspect the error payload for a session-expired code (e.g., `session_expired`) and perform a forced logout (clear local storage keys and trigger a storage event by updating `ci_auth`).]
- [Update pages/features that call the backend to use the API client (or establish the API client for future use if those pages are not yet wired to APIs).]
- [Optionally align Redux `authSlice` thunks to use the API client instead of raw `axios` to reduce duplication and keep session expiry behavior consistent.]
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | app/src/lib/apiClient.ts | Wrapper around `fetch` that adds auth headers, normalizes errors, and handles session-expired forced logout |
| MODIFY | app/src/store/slices/authSlice.ts | Use `apiClient` for login/logout/me calls or centralize error handling for `session_expired` |
| MODIFY | app/src/pages/* | Adopt API client for any backend calls, ensuring consistent 401/session-expired behavior |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://developer.mozilla.org/en-US/docs/Web/API/Fetch_API

## Build Commands
- npm --prefix .\app run build
- npm --prefix .\app run test

## Implementation Validation Strategy
- []

## Implementation Checklist
- [ ] Implement `apiClient` with token header injection.
- [ ] Standardize parsing of backend error response shape.
- [ ] Detect session-expired condition and clear local auth keys.
- [ ] Ensure forced logout triggers `AppShell` storage listener and navigates to `/login`.
- [ ] Update Redux auth thunks to avoid duplicated auth logic.
- [ ] Verify requests continue to work when no token is present (public endpoints).
- [ ] Confirm no sensitive tokens are logged to console.
