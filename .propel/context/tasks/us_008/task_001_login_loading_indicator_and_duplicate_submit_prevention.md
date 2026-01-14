# Task - [TASK_001]

## Requirement Reference
- User Story: [us_008] (extracted from input)
- Story Location: [.propel/context/tasks/us_008/us_008.md]
- Acceptance Criteria: 
    - Given I am on the Login page (SCR-001), When I click the Login button with valid-format inputs, Then a loading indicator is displayed during the authentication process.
    - Given authentication is in progress, When the loading indicator is shown, Then the UI blocks duplicate submissions (e.g., disables the submit button) until the request completes.
    - Given authentication completes successfully, When the response is received, Then the loading state clears and the user is redirected to the appropriate landing page.
    - Given authentication fails, When the response is received, Then the loading state clears and an actionable error message is shown.

## Task Overview
Implement a robust login submission loading state that provides immediate progress feedback, prevents duplicate submissions, and ensures correct post-authentication navigation and cleanup. This task focuses on SCR-001 (Login) "Loading" state behavior and edge-case handling (slow network, navigation away mid-request, fast responses/flicker).

## Dependent Tasks
- [US_006] Create professional login page UI

## Impacted Components
- app/src/pages/LoginPage.jsx
- app/src/components/ui/Button.jsx
- app/src/routes.jsx
- app/src/pages/__tests__/LoginPage.test.jsx
- app/src/__tests__/visual/login.spec.js

## Implementation Plan
- Add explicit client-side navigation on successful login response (redirect to the initial authenticated landing route).
- Ensure loading state is visible and duplicate submits are blocked across both:
  - Button disabled state
  - Submit handler guard (no duplicate network requests)
- Add request lifecycle safety to avoid stale updates when:
  - Authentication takes a long time (slow network)
  - The user navigates away during an in-flight request
- Prevent perceived UI flicker for very fast responses by enforcing a minimal visible loading duration (small threshold) while still keeping the UI responsive.
- Update unit tests and Playwright tests to validate:
  - Button becomes disabled / busy during submit
  - Only one request is issued on rapid double-click
  - Loading clears on success and error
  - Redirect occurs on success

## Current Project State
```
app/
├─ src/
│  ├─ pages/
│  │  ├─ LoginPage.jsx (updated with abort-safe submit, redirect, and duplicate-submit guard)
│  │  ├─ DashboardPage.jsx (new minimal landing page)
│  │  └─ __tests__/LoginPage.test.jsx (updated with duplicate-submit + redirect tests)
│  ├─ __tests__/visual/login.spec.js (updated with slow-auth loading test)
│  └─ routes.jsx (added /dashboard route)
└─ package.json (no changes)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | app/src/pages/LoginPage.jsx | Add post-login redirect using React Router, strengthen request lifecycle handling (abort on unmount / navigation away), optional minimal spinner duration, and ensure loading + duplicate submit prevention remains correct. |
| MODIFY | app/src/routes.jsx | Add/confirm an authenticated landing route (e.g., `/dashboard`) for redirect target. |
| CREATE | app/src/pages/DashboardPage.jsx | Minimal placeholder landing page to support redirect behavior until full dashboard is implemented. |
| MODIFY | app/src/pages/__tests__/LoginPage.test.jsx | Add tests for duplicate submission prevention and redirect behavior on success; keep existing error-handling tests passing. |
| MODIFY | app/src/__tests__/visual/login.spec.js | Add a scenario where login response is intentionally delayed and assert loading indicator/disabled behavior is visible during the wait. |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://reactrouter.com/en/main/hooks/use-navigate
- https://developer.mozilla.org/en-US/docs/Web/API/AbortController

## Build Commands
- app: `npm run test`
- app (e2e): `npm run test:e2e`
- app (dev): `npm run dev`

## Implementation Validation Strategy
- Unit tests (Vitest + Testing Library):
  - Duplicate submit: `fetch` called once even if submit invoked twice while `isSubmitting` is true.
  - Success path: loading cleared and navigation called.
  - Error path: loading cleared and error message rendered.
- Playwright:
  - Delay the mocked `/api/v1/auth/login` response and verify the UI indicates in-progress state (spinner/busy/disabled) and blocks repeated clicks.
- Manual:
  - Throttle network in browser dev tools and verify loading state persists and remains accessible (ARIA busy/disabled).

## Implementation Checklist
- [x] Decide and document the initial authenticated landing route (e.g., `/dashboard`) for redirect.
- [x] Update `LoginPage` to navigate on success.
- [x] Ensure `isSubmitting` blocks duplicate submits and button is disabled while loading.
- [x] Add request cancellation / stale update protection for navigation-away mid-request.
- [x] Optionally enforce minimum loading indicator duration to avoid flicker on ultra-fast responses.
- [x] Update `LoginPage.test.jsx` to cover duplicate-submit + redirect.
- [x] Update Playwright `login.spec.js` with a delayed auth response test.

---

# Design Reference

> **Note**: This template should only be used for User Stories and Tasks that have **UI impact**.
> Backend-only, API-only, or data processing tasks do not require design references.

## UI Impact Assessment
**Has UI Changes**: [x] Yes [ ] No
- If NO: Skip this design reference section entirely
- If YES: Complete all applicable sections below

## User Story Design Context
**Story ID**: US-[008]
**Story Title**: Show login loading indicator and prevent duplicate submits
**UI Impact Type**: UI Enhancement

### Design Source References
**Choose applicable reference type:**
- **Design System**: `.propel/context/docs/designsystem.md`
- **Screen Spec**: `.propel/context/docs/figma_spec.md` (SCR-001 includes `Loading` state)

### Screen-to-Design Mappings
**Option B: Design Image References**
| Screen/Feature | Image File | Image Path | Description | Implementation Priority |
|---------------|------------|------------|-------------|----------------------|
| Login (SCR-001) Loading State | N/A | N/A | Show in-progress indicator and block duplicate submits during authentication | High |

### Design Tokens
```yaml
colors:
  primary:
    usage: "Primary CTAs and busy/active indicators"
  neutral:
    usage: "Disabled states and borders"

motion:
  duration:
    usage: "Avoid loading flicker with minimal indicator time"
```

### Component References
**Option B: Image-Based Component References**
| Component Name | Reference Image | Code Location | UI Changes Required |
|---------------|-----------------|---------------|-------------------|
| Button | N/A | app/src/components/ui/Button.jsx | Ensure loading state is visible and disables interactions (spinner + aria-busy). |
| Alert | N/A | app/src/components/ui/Alert.jsx | Ensure auth failures display an actionable message once request completes. |

### Accessibility Requirements
- **WCAG Level**: AA (for changes on SCR-001)
- **Focus States**: Ensure disabled/loading states do not break focus order or keyboard navigation.
- **Screen Reader**: Ensure busy state is communicated (e.g., `aria-busy`) and the button remains correctly disabled.

### Design Review Checklist
- [x] Confirm SCR-001 includes `Loading` state behavior per `.propel/context/docs/figma_spec.md`
- [x] Confirm busy/disabled states match design system intent
- [x] Confirm no layout shift occurs when spinner appears (dimensions preserved)
