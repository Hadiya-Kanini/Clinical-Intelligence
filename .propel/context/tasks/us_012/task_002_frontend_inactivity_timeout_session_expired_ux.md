# Task - [TASK_012_002]

## Requirement Reference
- User Story: [us_012] (extracted from input)
- Story Location: [.propel/context/tasks/us_012/us_012.md]
- Acceptance Criteria: 
    - [Given a user is authenticated, When 15 minutes pass without any user activity, Then the session is automatically terminated.]
    - [Given a session is terminated due to inactivity, When the user attempts any action, Then they are redirected to the login page with a session expired message.]
    - [Given a user performs any action (API call, navigation), When the action is processed, Then the session's last activity timestamp is updated.]

## Task Overview
Implement client-side inactivity tracking UX for authenticated users. Provide a consistent “session expired” experience (including redirect and messaging) and align with the product’s UX expectation that session timeout behavior is visible and safe for users with in-progress work.

## Dependent Tasks
- [US_011 - Implement JWT authentication with HttpOnly cookies]
- [TASK_012_001 - Backend session tracking and inactivity timeout]

## Impacted Components
- [MODIFY: app/src/components/AppShell.tsx]
- [MODIFY: app/src/pages/LoginPage.tsx]
- [CREATE: app/src/hooks/useInactivityTimeout.ts]

## Implementation Plan
- [Create a reusable hook (`useInactivityTimeout`) that tracks “user activity” events in the browser and triggers a timeout flow after 15 minutes of inactivity.]
- [Define activity sources for resetting the inactivity timer:]
- [DOM events (e.g., `mousemove`, `keydown`, `click`, `scroll`, `touchstart`).]
- [Route changes (React Router navigation).]
- [Successful API activity via the shared API client wrapper (see task `TASK_012_003`).]
- [When the timeout triggers:]
- [Clear local auth indicators (`ci_auth`, `ci_token`, `ci_user_role`).]
- [Navigate to `/login` with router state indicating a session-expired reason and optionally the last route (`from`) to support post-login redirect.]
- [If UX requires, show an in-app modal that informs the user their session expired and prompts re-login (align to Figma spec notes on session timeout and preserving work).]
- [Update `LoginPage` to show a clear “Session expired” message when navigated to from timeout.
]
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | app/src/hooks/useInactivityTimeout.ts | Centralized inactivity tracking hook with configurable idle timeout (default 900 seconds) |
| MODIFY | app/src/components/AppShell.tsx | Use `useInactivityTimeout` for authenticated routes; trigger logout/redirect on expiration; optionally display timeout modal |
| MODIFY | app/src/pages/LoginPage.tsx | Display session-expired messaging when redirected due to inactivity (based on `location.state`) |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://react.dev/reference/react/useEffect

## Build Commands
- npm --prefix .\app run build
- npm --prefix .\app run test
- npm --prefix .\app run test:e2e

## Implementation Validation Strategy
- []

## Implementation Checklist
- [ ] Add `useInactivityTimeout` hook with cleanup-safe event listeners.
- [ ] Define idle timeout constant/config (default 15 minutes) and ensure it is test-configurable.
- [ ] Integrate hook in `AppShell` (authenticated layout) so it applies to all protected screens.
- [ ] Ensure clearing `ci_auth` triggers existing cross-tab logout behavior (`storage` listener).
- [ ] Redirect to `/login` with `{ state: { logout: 'expired', from: location } }` (or equivalent).
- [ ] Show “Session expired” message in `LoginPage` when applicable.
- [ ] Verify timeout behavior does not depend on the client clock beyond relative timers.
- [ ] Confirm UX approach for unsaved work is represented (modal + re-login path, minimal preservation via redirect target).

## Design Reference

## UI Impact Assessment
**Has UI Changes**: [ ] Yes [ ] No
- If NO: Skip this design reference section entirely
- If YES: Complete all applicable sections below

## User Story Design Context
**Story ID**: US-[012]
**Story Title**: Implement session tracking and inactivity timeout
**UI Impact Type**: UI Enhancement

### Design Source References
- **Design System**: .propel/context/docs/designsystem.md
- **Screen Spec**: .propel/context/docs/figma_spec.md

### Screen-to-Design Mappings
| Screen/Feature | Image File | Image Path | Description | Implementation Priority |
|---------------|------------|------------|-------------|----------------------|
| Session timeout (all authenticated screens) | N/A | N/A | Modal with re-login option; preserves unsaved work note | High |

### Task Design Mapping
```yaml
TASK_012_002:
  title: "Frontend inactivity timeout + session expired UX"
  ui_impact: true
  visual_references:
    figma_spec_refs: ["Session timeout - all authenticated"]
  components_affected:
    - AppShell (logout + session expiry flows)
    - LoginPage (session expired messaging)
  visual_validation_required: true
```

### Accessibility Requirements
- **WCAG Level**: AA
- **Focus States**: Ensure modal focus trap and keyboard close/confirm actions are accessible
- **Screen Reader**: Use `role="dialog"` and appropriate `aria-*` attributes for the timeout modal
