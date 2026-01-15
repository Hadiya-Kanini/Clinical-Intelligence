# Task - [TASK_002]

## Requirement Reference
- User Story: [us_017] (extracted from input)
- Story Location: [.propel/context/tasks/us_017/us_017.md]
- Acceptance Criteria: 
    - [Given an account is locked, When the login attempt fails with lockout error, Then the UI displays the remaining lockout time and reason (UXR-009).]
    - [Given a lockout message is displayed, When the lockout period expires, Then the UI allows retry without requiring page refresh.]
    - [Given rate limit or lockout occurs, When the message is displayed, Then it includes contact support option for assistance.]

## Task Overview
Enhance the login UI lockout experience so the user sees a clear remaining lockout timeframe and a concise reason, and the UI automatically transitions back to an enabled login state when the lockout expires (without requiring a page refresh).

## Dependent Tasks
- [US_016 - Implement account lockout after failed attempts]
- [TASK_004 (us_016) - Frontend display lockout timeframe login error]

## Impacted Components
- [MODIFY | app/src/pages/LoginPage.tsx | When lockout error is returned, show remaining time + reason and manage an auto-expiry timer]
- [MODIFY | app/src/__tests__/visual/login.spec.js | Update locked-account mocked response details (unlock timeframe + reason metadata if applicable) for UI coverage]

## Implementation Plan
- Align lockout parsing with backend contract:
  - When `POST /api/v1/auth/login` returns `403` with `error.code === 'account_locked'`, parse `error.details`:
    - Prefer `unlock_at:<ISO-8601 UTC timestamp>`.
    - If `remaining_seconds:<int>` is present, compute a local countdown value.
- Display UXR-009 messaging:
  - Show a clear lockout message with:
    - Remaining lockout time (countdown and/or local time display)
    - Reason text (e.g., “Too many failed login attempts”) derived from the known lockout policy without revealing sensitive account existence details beyond current behavior
    - Contact support/administrator option
  - Ensure accessible announcement (`role="alert"` or `aria-live`) and keep text concise.
- Implement auto-retry enablement:
  - Store unlock timestamp in component state.
  - Create an interval timer that updates the countdown (and clears itself on unmount).
  - When the unlock time is reached:
    - Clear the lockout message state
    - Re-enable the login submit path without requiring refresh
    - Optionally surface an informational “You can try again now” message.
- Update visual test mocks:
  - Ensure the `locked` mock branch includes `details` with a deterministic unlock time for screenshots.

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | app/src/pages/LoginPage.tsx | Implement lockout countdown messaging and auto-expiry state transition for `account_locked` errors |
| MODIFY | app/src/__tests__/visual/login.spec.js | Mock `account_locked` response with unlock timeframe details for UI validation |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://react.dev/reference/react/useEffect
- https://developer.mozilla.org/en-US/docs/Web/API/setInterval

## Build Commands
- npm --prefix .\app run build
- npm --prefix .\app run test
- npm --prefix .\app run test:e2e

## Implementation Validation Strategy
- [Manual] Trigger a locked account response and confirm:
  - Lockout reason + remaining time is displayed
  - Countdown updates
  - Once expired, the UI allows retry without refresh
- [Automated] Run Playwright login visual tests and confirm locked state renders correctly.

## Implementation Checklist
- [x] Detect `account_locked` error on login failure
- [x] Parse `unlock_at:` and/or `remaining_seconds:` from `error.details`
- [x] Render lockout reason + remaining timeframe (UXR-009)
- [x] Implement countdown timer with cleanup-safe `useEffect`
- [x] Auto-clear lockout UI state when unlock time is reached (no refresh)
- [x] Include contact support/administrator option in the lockout message
- [x] Update Playwright mocked response and validate screenshots

## Design Reference

## UI Impact Assessment
**Has UI Changes**: [ ] Yes [ ] No
- If NO: Skip this design reference section entirely
- If YES: Complete all applicable sections below

## User Story Design Context
**Story ID**: US-[017]
**Story Title**: Display rate limit and lockout messages in UI
**UI Impact Type**: UI Enhancement

### Design Source References
- **Design System**: .propel/context/docs/designsystem.md
- **Screen Spec**: .propel/context/docs/figma_spec.md

### Screen-to-Design Mappings
| Screen/Feature | Image File | Image Path | Description | Implementation Priority |
|---------------|------------|------------|-------------|----------------------|
| Login lockout messaging | N/A | N/A | Lockout message with remaining time, reason, support option, and auto-retry enablement (UXR-009) | High |

### Task Design Mapping
```yaml
TASK_002:
  title: "Login - lockout remaining time + reason + auto retry"
  ui_impact: true
  visual_references:
    figma_spec_refs: ["SCR-001 Login", "UXR-009"]
  components_affected:
    - LoginPage
  visual_validation_required: true
```

### Accessibility Requirements
- **WCAG Level**: AA
- **Screen Reader**: Lockout message announced; countdown changes should not be overly verbose
- **Focus States**: Support link must have visible focus
