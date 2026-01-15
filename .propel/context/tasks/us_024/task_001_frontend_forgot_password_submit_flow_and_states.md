# Task - [TASK_001]

## Requirement Reference
- User Story: [us_024]
- Story Location: [.propel/context/tasks/us_024/us_024.md]
- Acceptance Criteria: 
    - [Given I'm on the Forgot Password page, When I enter my email and submit, Then the form validates email format.]
    - [Given a valid email is submitted, When processed, Then a confirmation message is displayed (FR-115b).]
    - [Given the forgot password page, When displayed, Then it includes a link back to the login page (FR-115c).]

## Task Overview
Wire the existing Forgot Password page (SCR-002) to a backend request so submission is actually processed, while preserving non-enumeration behavior (always show a generic confirmation message on success). Add the required user-facing states (loading, success, error) and ensure accessible, consistent validation and messaging.

## Dependent Tasks
- [US_009 - Forgot Password navigation from Login] (route/link should already exist)
- [US_018 TASK_002 - Frontend RFC 5322 email validation shared utility] (if adopted as the canonical email validator)
- [Backend forgot password endpoint availability] (see TASK_002)

## Impacted Components
- [MODIFY | app/src/pages/ForgotPasswordPage.tsx | Submit email to backend, implement loading/error/success states, and keep back-to-login link behavior]
- [MODIFY (optional) | app/src/pages/LoginPage.tsx | Display reset-success message when redirected from reset flow (if not already implemented elsewhere)]

## Implementation Plan
- Update `ForgotPasswordPage` submit handler to:
  - Validate email format prior to sending.
  - Call backend endpoint (proposed): `POST /api/v1/auth/forgot-password` with `{ email }`.
  - Display a generic success message on `200` regardless of whether the email exists (FR-009q).
- Add UI states aligned to SCR-002:
  - Loading: disable submit button, optionally show inline loading state.
  - Success: show confirmation message and keep the form usable (or disable submit to avoid spam).
  - Error: show a non-sensitive error message when network/server errors occur.
  - Rate-limited (future-proof): if API returns `429`, show retry guidance (use `Retry-After` header when present).
- Maintain existing navigation affordances:
  - Keep “Back to login” link and continue passing the current email via `location.state`.
- Accessibility:
  - Ensure success/error banners use an appropriate live region (e.g., `role="status"` for success, `role="alert"` for errors).

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | app/src/pages/ForgotPasswordPage.tsx | Call forgot-password API on submit; add loading/success/error (and 429) states; keep generic messaging and back-to-login link |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://reactrouter.com/en/main/hooks/use-location

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Manual] Submit a valid email and confirm a generic success message is displayed.
- [Manual] Submit invalid email format and confirm field-level error is shown and no request is sent.
- [Manual] Simulate backend failure (network down) and confirm a safe, user-friendly error is shown.
- [Manual] If backend returns 429, confirm retry guidance is shown.

## Implementation Checklist
- [x] Implement submit handler that calls backend `POST /api/v1/auth/forgot-password`
- [x] Add loading state and disable/guard duplicate submits
- [x] Keep generic success message (no user enumeration)
- [x] Add safe error UI for network/server failures
- [x] Ensure back-to-login link remains present and preserves email
- [x] Ensure success/error messaging is accessible (ARIA live region)

---

# Design Reference

## UI Impact Assessment
**Has UI Changes**: [x] Yes [ ] No

## User Story Design Context
**Story ID**: US-[024]
**Story Title**: Implement forgot password page with email input
**UI Impact Type**: UI Enhancement

### Design Source References
- **Design System**: `.propel/context/docs/designsystem.md`
- **Screen Spec**: `.propel/context/docs/figma_spec.md` (SCR-002)

### Screen-to-Design Mappings
| Screen/Feature | Image File | Image Path | Description | Implementation Priority |
|---------------|------------|------------|-------------|----------------------|
| Forgot Password (SCR-002) | N/A | N/A | Default + Loading + Success + Error + (optional) RateLimited states | High |

### Component References
| Component Name | Reference Image | Code Location | UI Changes Required |
|---------------|-----------------|---------------|-------------------|
| TextField | N/A | app/src/pages/ForgotPasswordPage.tsx | Ensure email validation + error styling consistent with design system |
| Button | N/A | app/src/pages/ForgotPasswordPage.tsx | Add disabled/loading state during submit |
| Alert | N/A | app/src/pages/ForgotPasswordPage.tsx | Display success/error feedback banners |
| Link | N/A | app/src/pages/ForgotPasswordPage.tsx | Maintain “Back to login” link and focus states |

### Accessibility Requirements
- **WCAG Level**: AA
- **Keyboard Navigation**: Submit + Back link reachable via Tab
- **Live Regions**: Success and error messages announced appropriately
- **Focus States**: Visible focus for interactive elements

### Design Review Checklist
- [x] Confirm SCR-002 states match figma_spec.md definitions
- [x] Confirm layout remains consistent with login card styling
- [x] Confirm focus styling and message announcements meet accessibility requirements
