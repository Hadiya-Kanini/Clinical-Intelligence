# Task - [TASK_001]

## Requirement Reference
- User Story: [us_009] (extracted from input)
- Story Location: [.propel/context/tasks/us_009/us_009.md]
- Acceptance Criteria: 
    - Given I am on the Login page (SCR-001), When I view the page, Then a "Forgot Password" link is prominently displayed without requiring scrolling.
    - Given I am on the Login page, When I activate the "Forgot Password" link, Then I am navigated to the Forgot Password page (SCR-002).
    - Given I am using keyboard navigation, When I tab through interactive elements, Then the "Forgot Password" link is reachable and actionable via keyboard.

## Task Overview
Add a prominent, accessible "Forgot Password" link to the Login page and wire it to a new client route for the Forgot Password page. This task establishes the SCR-001 -> SCR-002 navigation path and adds guardrails for keyboard accessibility and stable navigation behavior.

## Dependent Tasks
- [US_006] Create professional login page UI

## Impacted Components
- app/src/pages/LoginPage.jsx
- app/src/routes.jsx
- app/src/pages/ForgotPasswordPage.jsx
- app/src/pages/__tests__/LoginPage.test.jsx
- app/src/__tests__/visual/login.spec.js

## Implementation Plan
- Add a prominent "Forgot Password" link to `LoginPage` positioned within the form layout so it is visible without scrolling.
- Implement navigation to a new route (e.g., `/forgot-password`) using React Router.
- Create a minimal `ForgotPasswordPage` (SCR-002 placeholder) so routing is real and testable.
- Ensure the link is reachable and actionable via keyboard:
  - Use a semantic link element (React Router `Link`) or an equivalent accessible button+navigation pattern.
  - Ensure focus styling is visible (design system focus indicator).
- Address edge cases:
  - Repeated activation should not produce unstable routing behavior (avoid duplicate state updates / navigation loops).
  - When navigating from Login to Forgot Password, pass the current email value (when present) so it can be pre-filled on SCR-002.
  - When navigating back to Login from Forgot Password, preserve the email value where appropriate.
- Update unit tests and Playwright tests to validate:
  - Link renders.
  - Link triggers navigation.
  - Link is reachable via Tab navigation.

## Current Project State
```
app/
├─ src/
│  ├─ pages/
│  │  ├─ LoginPage.jsx
│  │  ├─ DashboardPage.jsx
│  │  └─ __tests__/LoginPage.test.jsx
│  ├─ __tests__/visual/login.spec.js
│  └─ routes.jsx
└─ package.json
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | app/src/pages/LoginPage.jsx | Add a prominent "Forgot Password" link that navigates to `/forgot-password`; ensure keyboard accessibility and stable click behavior; pass current email value into navigation state/query to support prefill. |
| MODIFY | app/src/routes.jsx | Add a `forgot-password` route mapping to the new `ForgotPasswordPage`. |
| CREATE | app/src/pages/ForgotPasswordPage.jsx | Minimal SCR-002 page placeholder with an email input prefilled (when available) and a "Back to login" navigation affordance that preserves email where appropriate. |
| MODIFY | app/src/pages/__tests__/LoginPage.test.jsx | Add/adjust tests verifying "Forgot Password" link presence, navigation activation (mocked `navigate` or router), and keyboard reachability. |
| MODIFY | app/src/__tests__/visual/login.spec.js | Extend keyboard navigation coverage to ensure the "Forgot Password" link is reachable via Tab and remains clickable; optionally add a screenshot assertion for the updated layout. |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://reactrouter.com/en/main/components/link
- https://reactrouter.com/en/main/hooks/use-navigate
- https://reactrouter.com/en/main/hooks/use-location

## Build Commands
- app: `npm run test`
- app (e2e): `npm run test:e2e`
- app (dev): `npm run dev`

## Implementation Validation Strategy
- Unit tests (Vitest + Testing Library):
  - Assert a "Forgot Password" link is rendered on the Login page.
  - Assert activating the link triggers navigation to `/forgot-password`.
  - Assert the link is reachable via keyboard navigation (Tab order includes the link).
- Playwright:
  - Validate the link is focusable via Tab and activation navigates to the Forgot Password route.
  - Ensure layout remains stable across desktop/tablet/mobile snapshots where applicable.
- Manual:
  - Verify the link is visible without scrolling on common desktop viewports.
  - Verify focus ring is visible when tabbing.
  - Verify email value can be carried forward into Forgot Password when provided.

## Implementation Checklist
- [ ] Add "Forgot Password" link to `LoginPage` in a prominent location (SCR-001).
- [ ] Add `/forgot-password` route entry in `routes.jsx`.
- [ ] Create `ForgotPasswordPage` with minimal layout and navigation back to login.
- [ ] Carry forward email value from Login to Forgot Password and back where appropriate.
- [ ] Update `LoginPage.test.jsx` with link + keyboard navigation coverage.
- [ ] Update `login.spec.js` to include the link in keyboard navigation assertions.

---

# Design Reference

## UI Impact Assessment
**Has UI Changes**: [x] Yes [ ] No

## User Story Design Context
**Story ID**: US-[009]
**Story Title**: Provide Forgot Password navigation from Login
**UI Impact Type**: UI Enhancement

### Design Source References
- **Design System**: `.propel/context/docs/designsystem.md`
- **Screen Spec**: `.propel/context/docs/figma_spec.md` (SCR-001 requires "Forgot Password" link; SCR-002 is the Forgot Password page)

### Screen-to-Design Mappings
**Option B: Design Image References**
| Screen/Feature | Image File | Image Path | Description | Implementation Priority |
|---------------|------------|------------|-------------|----------------------|
| Login (SCR-001) | N/A | N/A | Add a prominent, accessible "Forgot Password" link without scrolling | High |
| Forgot Password (SCR-002) | N/A | N/A | Provide a reachable destination page for navigation (minimal placeholder acceptable) | High |

### Design Tokens
```yaml
colors:
  primary:
    usage: "Link color, focus indicators"
  neutral:
    usage: "Surfaces, borders, muted text"

typography:
  body:
    usage: "Link text styling consistent with form body"

motion:
  transitions:
    usage: "Hover/focus transitions (fast)"
```

### Component References
**Option B: Image-Based Component References**
| Component Name | Reference Image | Code Location | UI Changes Required |
|---------------|-----------------|---------------|-------------------|
| Link | N/A | app/src/pages/LoginPage.jsx | Add "Forgot Password" link with hover/focus states per design system. |
| TextField | N/A | app/src/pages/ForgotPasswordPage.jsx | Provide email input consistent with SCR-002 component requirements. |
| Button | N/A | app/src/pages/ForgotPasswordPage.jsx | Provide primary action (if included in placeholder) consistent with button states. |

### Accessibility Requirements
- **WCAG Level**: AA
- **Keyboard Navigation**: Link must be reachable via Tab and activatable via Enter.
- **Focus States**: Focus indicator must be visible for the link (no outline suppression).
- **Touch Targets**: Ensure sufficient clickable area for the link.

### Design Review Checklist
- [ ] Confirm link placement does not require scrolling on SCR-001 at desktop baseline.
- [ ] Confirm focus styling for link meets design system guidance.
- [ ] Confirm SCR-002 route exists and navigation is functional.
