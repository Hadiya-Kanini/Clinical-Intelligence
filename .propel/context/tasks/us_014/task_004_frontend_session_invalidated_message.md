# Task - [TASK_004]

## Requirement Reference
- User Story: [us_014]
- Story Location: [.propel/context/tasks/us_014/us_014.md]
- Acceptance Criteria: 
    - [Given a session is invalidated due to new login, When the original session makes a request, Then the API returns 401 with a "session invalidated" message.]

## Task Overview
Improve UX for the “session invalidated” scenario by redirecting the user to the login page and showing a clear, non-sensitive message when the backend returns `401` due to session replacement. Estimated effort: ~4-6 hours.

## Dependent Tasks
- [TASK_002 - Backend enforce session revocation and 401]
- [TASK_003 - Frontend logout confirmation message] (pattern reference)

## Impacted Components
- [MODIFY | app/src/main.tsx | Add a centralized axios response handler (interceptor) to detect `401` session-invalidated responses]
- [MODIFY | app/src/pages/LoginPage.tsx | Display a session-invalidated message when navigated with router state (similar to logout success message)]

## Implementation Plan
- Add a centralized `401` handler:
  - Configure an axios response interceptor in `main.tsx`.
  - When a response is `401` and the error payload indicates `session invalidated`, clear any legacy localStorage keys and redirect to `/login` with router navigation state (e.g., `state: { auth: 'session_invalidated' }`).
- Update `LoginPage` to:
  - Read `location.state` and show an accessible banner/message (e.g., `aria-live="polite"`).
  - Keep the message generic (e.g., “Your session was ended because you signed in on another device.”).
  - Clear the message on subsequent login attempts.

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | app/src/main.tsx | Add axios response interceptor to handle session-invalidated `401` and redirect to login |
| MODIFY | app/src/pages/LoginPage.tsx | Render session-invalidated message when redirected with state flag |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://axios-http.com/docs/interceptors
- https://reactrouter.com/en/main/hooks/use-location

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Manual] Login in Browser A, login again in Browser B, then attempt any action in Browser A and confirm redirect + message.
- [Regression] Existing logout confirmation message still works.

## Implementation Checklist
- [x] Add axios response interceptor for `401` with session invalidated payload
- [x] Clear legacy localStorage keys before redirect
- [x] Redirect to `/login` with navigation state flag
- [x] Update `LoginPage` to show a session invalidated message
- [x] Ensure message is accessible and non-sensitive
- [x] Verify message clears on new login attempt
