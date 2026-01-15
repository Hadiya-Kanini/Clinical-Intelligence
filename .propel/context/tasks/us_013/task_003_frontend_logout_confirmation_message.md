# Task - [TASK_003]

## Requirement Reference
- User Story: [us_013]
- Story Location: [.propel/context/tasks/us_013/us_013.md]
- Acceptance Criteria: 
    - [Given logout is successful, When the user is redirected, Then they see a logout confirmation message (FR-003a).]

## Task Overview
Implement a logout confirmation message after redirecting the user to the login page. Ensure the message is displayed reliably after logout and is accessible, without leaking sensitive information.

## Dependent Tasks
- [TASK_001 - Backend secure logout token invalidation]

## Impacted Components
- [MODIFY | app/src/pages/LoginPage.tsx | Display logout success message when navigated with `location.state.logout === 'success'`]
- [MODIFY | app/src/components/AppShell.tsx | Ensure redirect includes logout success state (already present) and remains consistent]

## Implementation Plan
- Confirm the logout redirect behavior:
  - `AppShell` navigates to `/login` with `state: { logout: 'success' }`.
  - Cross-tab logout already emits `storage` event to navigate other tabs.
- Update `LoginPage` to detect `location.state?.logout === 'success'` and render a user-visible confirmation message.
- Ensure the message is:
  - Accessible (e.g., `role="status"` or `aria-live="polite"`)
  - Non-persistent unless intentionally designed (should clear when user starts typing or on navigation)
  - Not dependent on localStorage contents (to work in multi-tab cases)

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | app/src/pages/LoginPage.tsx | Add logout confirmation banner/message when redirected from logout |
| MODIFY | app/src/components/AppShell.tsx | Verify logout redirect state remains `logout: 'success'` (no functional change unless needed) |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://reactrouter.com/en/main/hooks/use-location

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Manual test] Click "Log out" and confirm login page shows confirmation message.
- [Multi-tab test] Open 2 tabs, logout from one, ensure other tab redirects and also shows message.

## Implementation Checklist
- [x] Add a logout success UI state to `LoginPage`
- [x] Render an accessible confirmation message when `location.state.logout === 'success'`
- [x] Ensure the message clears appropriately on new login attempts
- [x] Validate behavior for direct visits to `/login` (no message shown)
