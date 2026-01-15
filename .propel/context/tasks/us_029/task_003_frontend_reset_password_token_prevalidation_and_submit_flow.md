# Task - [TASK_003]

## Requirement Reference
- User Story: [us_029]
- Story Location: [.propel/context/tasks/us_029/us_029.md]
- Acceptance Criteria: 
    - [Given I click the reset link in email, When the page loads, Then the token is validated before displaying the form (FR-116a).]
    - [Given a valid token, When the form is displayed, Then it includes new password and confirm password fields (FR-116b).]
    - [Given password input, When typing, Then a password strength indicator is displayed (FR-116c, UXR-006).]
    - [Given an expired or invalid token, When detected, Then an error message is shown with option to request new reset.]

## Task Overview
Update the existing Reset Password page to pre-validate the reset token with the backend before showing the password form. When the token is valid, allow the user to set a new password and submit it to the backend reset endpoint. When the token is invalid/expired/used, show a safe error state and provide a direct navigation option to request a new reset.

## Dependent Tasks
- [US_029 TASK_001 - Backend reset password token validation endpoint]
- [US_029 TASK_002 - Backend reset password confirm endpoint and token consumption]
- [US_019 TASK_002 - Frontend password complexity validation UI] (if adopting the shared policy helper as canonical)

## Impacted Components
- [MODIFY | app/src/pages/ResetPasswordPage.tsx | Add token pre-validation on load, add loading/error states, wire submit to `POST /api/v1/auth/reset-password`, and provide "Request new reset" path]

## Implementation Plan
- Add token pre-validation:
  - Read `token` from `useSearchParams()`.
  - If token is missing, immediately render invalid/expired state.
  - Otherwise, call `GET /api/v1/auth/reset-password/validate?token=...`.
  - While the request is pending, show a safe loading state and do not show the reset form.
  - If the token is valid, render the form.
  - If the token is invalid/expired/used, render an error banner and provide an option to navigate to `/forgot-password`.
- Wire reset submit to backend:
  - On submit, validate locally (reuse existing password + confirm validation and password strength UI).
  - Call `POST /api/v1/auth/reset-password` with `{ token, password }`.
  - On success, show success message and navigate to `/login` (existing behavior).
  - On error, surface a safe error message without exposing token state.
- UX and accessibility:
  - Ensure loading/error/success messages are accessible (use existing `Alert` component).
  - Ensure form is disabled during submission to prevent duplicate submits.

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | app/src/pages/ResetPasswordPage.tsx | Pre-validate token on page load via `GET /api/v1/auth/reset-password/validate`, block form until valid, submit new password via `POST /api/v1/auth/reset-password`, and render invalid/expired token state with link to `/forgot-password` |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://reactrouter.com/en/main/hooks/use-search-params

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Manual] Visit `/reset-password?token=<missing>` and confirm invalid/expired state is shown.
- [Manual] Visit `/reset-password?token=<invalid>` (with API mocked or real) and confirm invalid/expired state is shown and form is not displayed.
- [Manual] Visit `/reset-password?token=<valid>` and confirm token is validated first and then form is displayed.
- [Manual] Submit valid password + confirm password and confirm success message and redirect to login.
- [Manual] Simulate API failure and confirm safe error messaging.

## Implementation Checklist
- [x] Add token pre-validation request on page load and a loading state
- [x] Block form render until token is validated successfully
- [x] Add invalid/expired token UI state with navigation to `/forgot-password`
- [x] Wire submit to `POST /api/v1/auth/reset-password` with `{ token, password }`
- [x] Disable submit while submitting to prevent duplicate requests
- [x] Preserve password strength indicator and validation UI

---

# Design Reference

## UI Impact Assessment
**Has UI Changes**: [x] Yes [ ] No

## User Story Design Context
**Story ID**: US-[029]
**Story Title**: Implement reset password page with token validation
**UI Impact Type**: UI Enhancement

### Design Source References
- **Design System**: `.propel/context/docs/designsystem.md`
- **Screen Spec**: `.propel/context/docs/figma_spec.md` (Reset Password screen)

### Screen-to-Design Mappings
| Screen/Feature | Image File | Image Path | Description | Implementation Priority |
|---------------|------------|------------|-------------|----------------------|
| Reset Password | N/A | N/A | Loading (validating token), Valid (form), Invalid/Expired (error + request new), Success (redirect) | High |

### Component References
| Component Name | Reference Image | Code Location | UI Changes Required |
|---------------|-----------------|---------------|-------------------|
| Alert | N/A | app/src/pages/ResetPasswordPage.tsx | Add token-validation loading/error/success banners |
| TextField | N/A | app/src/pages/ResetPasswordPage.tsx | Ensure error styling and accessible error text for password/confirm |
| Button | N/A | app/src/pages/ResetPasswordPage.tsx | Add disabled/loading state during validation and submit |
| PasswordStrength | N/A | app/src/pages/ResetPasswordPage.tsx | Keep visible during valid-token state |
| Link | N/A | app/src/pages/ResetPasswordPage.tsx | Add/ensure "Request new reset" link to forgot-password |

### Accessibility Requirements
- **WCAG Level**: AA
- **Live Regions**: Error and success messages announced appropriately
- **Keyboard Navigation**: All fields and links reachable via Tab
- **Focus States**: Visible focus for interactive elements
