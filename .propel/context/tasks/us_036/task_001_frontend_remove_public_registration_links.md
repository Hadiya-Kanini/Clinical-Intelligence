# Task - [TASK_001]

## Requirement Reference
- User Story: [us_036]
- Story Location: [.propel/context/tasks/us_036/us_036.md]
- Acceptance Criteria: 
    - [Given the public pages (login, forgot password), When displayed, Then no "Sign Up" or "Register" links are visible (UXR-005).]
    - [Given the login page, When displayed, Then it only shows login and forgot password options.]

## Task Overview
Ensure the frontend public surfaces do not expose any account creation entry points. This task validates that the login and password reset flows contain no registration-related CTAs, links, routes, or navigation. If any are present (including legacy/demo UI), remove them and ensure the only public options are login and forgot password.

## Dependent Tasks
- [N/A] (Can be implemented independently)

## Impacted Components
- [MODIFY | app/src/pages/LoginPage.tsx | Ensure the login page contains only login + forgot password actions and no registration-related CTAs/links]
- [MODIFY | app/src/pages/ForgotPasswordPage.tsx | Ensure the forgot password page contains no registration-related CTAs/links]
- [MODIFY | app/src/pages/ResetPasswordPage.tsx | Ensure the reset password page contains no registration-related CTAs/links]
- [MODIFY | app/src/routes.tsx | Ensure there is no public registration route and routing does not expose a registration page]

## Implementation Plan
- Review the public pages:
  - `LoginPage` (`/login`)
  - `ForgotPasswordPage` (`/forgot-password`)
  - `ResetPasswordPage` (`/reset-password`)
- Remove any UI elements that imply public registration:
  - "Sign Up", "Register", "Create account", "New user" links
  - any route navigation pointing to `/register`, `/signup`, or similar
- Confirm the public routing surface does not include a registration page:
  - ensure `routes.tsx` has no route definitions for registration
  - ensure no navigation components used on public pages expose registration
- Validate UX requirement (UXR-005):
  - login page shows only the login form and the forgot-password link
  - forgot/reset pages only link back to login (and to forgot-password from reset error state)

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | app/src/pages/LoginPage.tsx | Remove any registration links/CTAs if present; confirm only login + forgot password options are rendered |
| MODIFY | app/src/pages/ForgotPasswordPage.tsx | Remove any registration links/CTAs if present; keep only navigation back to login |
| MODIFY | app/src/pages/ResetPasswordPage.tsx | Remove any registration links/CTAs if present; keep only navigation back to login/forgot-password as needed |
| MODIFY | app/src/routes.tsx | Ensure there is no `/register` or `/signup` public route exposed |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Manual/UI] Navigate to `/login`, `/forgot-password`, and `/reset-password` and confirm there are no registration-related links or CTAs.
- [Regression] Confirm existing navigation between login <-> forgot password <-> reset password still works.

## Implementation Checklist
- [ ] Audit `LoginPage` for any registration-related UI and remove it if present
- [ ] Audit `ForgotPasswordPage` for any registration-related UI and remove it if present
- [ ] Audit `ResetPasswordPage` for any registration-related UI and remove it if present
- [ ] Verify `routes.tsx` does not expose any registration route
- [ ] Manually validate public pages do not display sign-up/register options

## Design Reference

## UI Impact Assessment
**Has UI Changes**: [x] Yes [ ] No

## User Story Design Context
**Story ID**: US-[036]
**Story Title**: Enforce no public registration policy
**UI Impact Type**: UI Enhancement

### Design Source References
- **Design System**: .propel/context/docs/designsystem.md
- **Screen Spec**: .propel/context/docs/figma_spec.md

### Screen-to-Design Mappings
| Screen/Feature | Image File | Image Path | Description | Implementation Priority |
|---------------|------------|------------|-------------|----------------------|
| Login page public actions | N/A | N/A | Ensure login page has no registration/sign-up options and only shows login + forgot password | High |
| Forgot password public actions | N/A | N/A | Ensure forgot password page has no registration/sign-up options | High |
| Reset password public actions | N/A | N/A | Ensure reset password page has no registration/sign-up options | Medium |

### Task Design Mapping
```yaml
TASK_036_001:
  title: "Frontend remove public registration links"
  ui_impact: true
  visual_references:
    figma_spec_refs: ["Login", "Forgot password", "Reset password"]
  components_affected:
    - LoginPage
    - ForgotPasswordPage
    - ResetPasswordPage
    - Router
  visual_validation_required: false
```

### Accessibility Requirements
- **WCAG Level**: AA
- **Focus States**: Ensure remaining links (e.g., forgot password, back to login) remain keyboard accessible
- **Screen Reader**: Ensure removed registration options are not present in the accessibility tree
