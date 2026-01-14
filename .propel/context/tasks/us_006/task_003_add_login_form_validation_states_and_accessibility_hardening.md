# Task - TASK_003

## Requirement Reference
- User Story: us_006
- Story Location: .propel/context/tasks/us_006/us_006.md
- Acceptance Criteria: 
    - Given I am unauthenticated, When I navigate to the Login page (SCR-001), Then I see a professional healthcare-appropriate design with clear hierarchy, and the page includes email + password fields and a login button.
    - Given the Login page is implemented, When UI styles are applied, Then the page uses design system tokens (no hard-coded colors) and remains readable and consistent.

## Task Overview
Add client-side validation states and accessibility hardening to the Login form UI so the page remains readable and operable under edge conditions (narrow viewports, zoom, high-contrast settings) and provides clear validation feedback.

## Dependent Tasks
- .propel/context/tasks/us_006/task_002_implement_login_page_ui_and_responsive_layout.md (TASK_002)

## Impacted Components
- app/src/pages/LoginPage.jsx
- app/src/components/ui/TextField.jsx
- app/src/components/ui/Alert.jsx

## Implementation Plan
- Implement client-side validation for login form fields:
  - Email: required + basic email format validation.
  - Password: required.
- Add validation UI states using existing primitives:
  - Inline error message using TextField helper/error text area.
  - Optional page-level Alert for submit-level errors.
- Accessibility hardening:
  - Ensure error text is connected to inputs via `aria-describedby`.
  - Set `aria-invalid` on fields with errors.
  - Ensure errors are not conveyed by color alone (include text).
- Edge case handling:
  - Confirm visible focus and readable contrast in typical high-contrast scenarios.
  - Confirm layout remains usable when error messages expand content height.
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | app/src/pages/LoginPage.jsx | Add form state, validation logic, and validation rendering. |
| MODIFY | app/src/components/ui/TextField.jsx | Support `aria-invalid`, `aria-describedby`, and consistent error rendering using tokens. |
| MODIFY | app/src/components/ui/Alert.jsx | Ensure alert supports accessibility semantics (e.g., `role=alert`) and token-based styling. |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://developer.mozilla.org/en-US/docs/Web/Accessibility/ARIA/Attributes/aria-invalid
- https://www.w3.org/WAI/ARIA/apg/patterns/alert/

## Build Commands
- npm install
- npm run dev
- npm run build

## Implementation Validation Strategy
- Verify validation triggers on submit and displays field-level errors for missing/invalid values.
- Verify screen reader semantics for errors: `aria-invalid` and `aria-describedby` present when errors render.
- Verify narrow/zoom scenarios still allow completion of the form without content clipping.

## Implementation Checklist
- [x] Add login form state (email/password) and validation rules
- [x] Render inline field errors and (optional) top-level alert
- [x] Wire ARIA attributes for validation states
- [x] Validate token-only styling for error states

# Design Reference

## UI Impact Assessment
**Has UI Changes**: [x] Yes [ ] No

## User Story Design Context
**Story ID**: US-006
**Story Title**: Create professional login page UI
**UI Impact Type**: UI Enhancement

### Design Source References
- **Design System**: `.propel/context/docs/designsystem.md`
- **UX Requirements**: `.propel/context/docs/figma_spec.md` (UXR-002, UXR-014, UXR-016; SCR-001 states)

### Screen-to-Design Mappings
| Screen/Feature | Image File | Image Path | Description | Implementation Priority |
|---------------|------------|------------|-------------|----------------------|
| SCR-001 / Validation | N/A | N/A | Inline validation errors, readable and accessible | High |

### Visual Validation Criteria
- Error messaging is clear and readable
- Error state uses design system error tokens
- Validation does not break layout at small widths or zoom

### Accessibility Requirements
- **WCAG Level**: AA
- **Color Contrast**: Ensure error text/indicators meet AA thresholds
- **Screen Reader**: Error association via `aria-describedby` and `role=alert` where applicable
