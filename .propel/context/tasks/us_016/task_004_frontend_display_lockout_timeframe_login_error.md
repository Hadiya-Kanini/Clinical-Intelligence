# Task - [TASK_004]

## Requirement Reference
- User Story: [us_016]
- Story Location: [.propel/context/tasks/us_016/us_016.md]
- Acceptance Criteria: 
    - [Given an account is locked, When the UI displays the error, Then it shows clear unlock timeframe (UXR-009).]

## Task Overview
Update the login UI to display a clear unlock timeframe when the backend returns `account_locked` with remaining lockout details. Also update the existing Playwright visual test mocks to include the new response details. Estimated effort: ~4-6 hours.

## Dependent Tasks
- [TASK_002 - Backend account locked error with remaining time]

## Impacted Components
- [MODIFY | app/src/pages/LoginPage.tsx | Parse `account_locked` error response details and display an unlock timeframe]
- [MODIFY | app/src/__tests__/visual/login.spec.js | Update locked-account mocked response to include remaining time details]

## Implementation Plan
- Parse locked-account backend response:
  - When `POST /api/v1/auth/login` returns `403` with `error.code === 'account_locked'`, parse `error.details`:
    - Prefer `unlock_at:<timestamp>` if present.
    - If only `remaining_seconds:<int>` is present, compute unlock time relative to `Date.now()`.
- Display UXR-009 messaging:
  - Show a clear message such as: “Your account is locked. Try again at HH:MM (local time).”
  - Ensure message is accessible (use `role="alert"` or `aria-live`).
  - Keep message non-sensitive (no mention of whether the account exists beyond the current behavior).
- Update Playwright visual test mock:
  - In `login.spec.js`, adjust the mocked `account_locked` response to include the new `details` format so screenshots cover the updated UI.

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | app/src/pages/LoginPage.tsx | Add logic to render unlock timeframe when backend returns `account_locked` with `unlock_at`/`remaining_seconds` in `error.details` |
| MODIFY | app/src/__tests__/visual/login.spec.js | Update mock `account_locked` response to include `details` with unlock timeframe |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://react.dev/reference/react/useState
- https://playwright.dev/docs/network

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Manual] Trigger locked account response and verify UI shows unlock time.
- [Automated] Run Playwright login visual tests and confirm locked state renders correctly.

## Implementation Checklist
- [ ] Detect `account_locked` error code on login failure
- [ ] Parse `unlock_at:` and/or `remaining_seconds:` from `error.details`
- [ ] Render user-friendly unlock timeframe text (UXR-009)
- [ ] Ensure error message is accessible and non-sensitive
- [ ] Update Playwright mocked response and verify screenshots
