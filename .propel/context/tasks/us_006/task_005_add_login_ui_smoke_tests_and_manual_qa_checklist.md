# Task - TASK_005

## Requirement Reference
- User Story: us_006
- Story Location: .propel/context/tasks/us_006/us_006.md
- Acceptance Criteria: 
    - Given I am unauthenticated, When I navigate to the Login page (SCR-001), Then I see a professional healthcare-appropriate design with clear hierarchy, and the page includes email + password fields and a login button.
    - Given the Login page is implemented, When UI styles are applied, Then the page uses design system tokens (no hard-coded colors) and remains readable and consistent.

## Task Overview
Add lightweight automated smoke coverage plus a manual QA checklist for SCR-001 to ensure required UI elements exist, the layout remains usable under edge conditions, and token-only styling rules are followed.

## Dependent Tasks
- .propel/context/tasks/us_006/task_003_add_login_form_validation_states_and_accessibility_hardening.md (TASK_003)
- .propel/context/tasks/us_006/task_004_add_branding_assets_and_resilient_fallbacks.md (TASK_004)

## Impacted Components
- app/package.json
- app/

## Implementation Plan
- Automated smoke validation (minimal):
  - Add a UI test runner suitable for the current Vite + React setup.
  - Implement a smoke test that asserts the Login page renders:
    - Email input
    - Password input
    - Login button
  - Add a check that validation UI appears when submitting empty fields.
- Manual QA checklist (focused on US_006 edge cases):
  - Narrow viewport/zoom: no clipped content; form remains usable.
  - High-contrast mode: critical elements remain distinguishable (labels, inputs, button).
  - Logo missing: fallback title visible.
  - Token-only styling: no newly introduced hard-coded colors.
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | app/package.json | Verify a UI testing dependency and test script (e.g., Vitest + Testing Library) appropriate for Vite are present. |
| MODIFY | app/src/pages/__tests__/LoginPage.test.jsx | Smoke tests for required elements and basic validation behavior. |
| MODIFY | app/README.md | Add a short manual QA checklist for SCR-001 login screen edge cases (narrow/zoom/high-contrast/logo missing). |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://vitest.dev/
- https://testing-library.com/docs/react-testing-library/intro/

## Build Commands
- npm install
- npm run dev
- npm run build
- npm run test

## Implementation Validation Strategy
- Automated: `npm run test` passes and confirms the presence of required login UI elements.
- Manual: Run through the checklist and confirm edge cases do not degrade usability.

## Implementation Checklist
- [x] Add test runner + scripts for Vite React
- [x] Implement smoke test for SCR-001 required fields and CTA
- [x] Implement validation behavior test for empty submit
- [x] Document manual QA checklist for SCR-001 edge cases

# Design Reference

## UI Impact Assessment
**Has UI Changes**: [x] Yes [ ] No

## User Story Design Context
**Story ID**: US-006
**Story Title**: Create professional login page UI
**UI Impact Type**: UI Enhancement

### Design Source References
- **Design System**: `.propel/context/docs/designsystem.md`
- **Screen Spec**: `.propel/context/docs/figma_spec.md` (SCR-001)

### Visual Validation Criteria
- Required form controls exist and are visible
- Validation is readable and does not break layout
- Token-only styling adherence confirmed by review

### Accessibility Requirements
- **WCAG Level**: AA
- **Keyboard**: Tab navigation reaches email, password, and submit
- **Screen Reader**: Labels and validation semantics are present
