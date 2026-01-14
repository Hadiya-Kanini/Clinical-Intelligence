# Task - TASK_002

## Requirement Reference
- User Story: us_006
- Story Location: .propel/context/tasks/us_006/us_006.md
- Acceptance Criteria: 
    - Given I am unauthenticated, When I navigate to the Login page (SCR-001), Then the page renders a clean, professional healthcare-appropriate design and branding (medical color palette, professional typography, clear hierarchy).
    - Given I am unauthenticated, When I view the Login page, Then the page includes:
      - Email input field
      - Password input field
      - Login submit button

## Task Overview
Implement the SCR-001 Login page UI shell using established token-based styling and UI primitives. Deliver a professional healthcare-appropriate layout with clear hierarchy and responsive behavior for narrow viewports/zoom.

## Dependent Tasks
- .propel/context/tasks/us_006/task_001_establish_frontend_design_tokens_and_ui_primitives.md (TASK_001)

## Impacted Components
- app/src/App.jsx
- app/src/

## Implementation Plan
- Establish routing/navigation so the user can “navigate to Login page (SCR-001)” consistently:
  - Add a minimal router configuration and expose a `/login` route.
  - Default app route redirects/renders the login page until authentication is implemented.
- Create a dedicated Login page component and compose it from UI primitives:
  - Email TextField with label and placeholder.
  - Password TextField with label and placeholder.
  - Primary Button labeled “Log in”.
  - Optional branding header (logo if present; fallback to text if not).
- Implement a professional healthcare-appropriate layout:
  - Centered card/container on a neutral background.
  - Clear visual hierarchy: heading, short supportive copy, form fields, primary CTA.
- Implement responsive behavior for edge cases:
  - Ensure the page remains usable at narrow widths and with browser zoom (no clipped content; allow vertical scrolling).
  - Keep inputs full-width within the card; card should adapt via max-width + responsive padding.
- Implement baseline accessibility:
  - Labels associated with inputs.
  - Keyboard reachable submit button.
  - Visible focus states inherited from primitives.
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | app/package.json | Add `react-router-dom` for routing (required to support navigation to SCR-001). |
| CREATE | app/src/routes.jsx | Central route definitions including `/login`. |
| MODIFY | app/src/main.jsx | Wrap app in router provider. |
| MODIFY | app/src/App.jsx | Render routed pages and set default to Login until auth is implemented. |
| CREATE | app/src/pages/LoginPage.jsx | SCR-001 login page UI composed from primitives and token-based styles. |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://reactrouter.com/en/main/start/tutorial

## Build Commands
- npm install
- npm run dev
- npm run build

## Implementation Validation Strategy
- Verify `/login` renders the login page and includes all required fields and CTA.
- Verify layout remains usable on narrow viewports and at increased zoom (no horizontal scroll for typical desktop widths; content does not overflow off-screen).
- Verify branding fallback behavior when image assets are missing (text title still visible).

## Implementation Checklist
- [x] Add routing and a `/login` route
- [x] Create `LoginPage` and compose email/password fields + submit button
- [x] Apply token-based layout (card/container, typography hierarchy)
- [x] Validate responsive behavior (narrow/zoom)
- [x] Validate keyboard navigation + focus states

# Design Reference

## UI Impact Assessment
**Has UI Changes**: [x] Yes [ ] No

## User Story Design Context
**Story ID**: US-006
**Story Title**: Create professional login page UI
**UI Impact Type**: New UI

### Design Source References
- **Design System**: `.propel/context/docs/designsystem.md`
- **Screen Inventory**: `.propel/context/docs/figma_spec.md` (SCR-001)

### Screen-to-Design Mappings
| Screen/Feature | Image File | Image Path | Description | Implementation Priority |
|---------------|------------|------------|-------------|----------------------|
| SCR-001 | N/A | N/A | Login page states: Default, Loading, Error, Validation, Lockout | High |

### Component References
| Component Name | Reference Image | Code Location | UI Changes Required |
|---------------|-----------------|---------------|-------------------|
| TextField | N/A | app/src/components/ui/TextField.jsx | Used for email/password inputs |
| Button | N/A | app/src/components/ui/Button.jsx | Used for primary login CTA |
| Link | N/A | app/src/components/ui/Link.jsx | Reserved for future “Forgot password” navigation (not required by US_006 AC) |
| Alert | N/A | app/src/components/ui/Alert.jsx | Reserved for future error display (not required by US_006 AC) |

### Visual Validation Criteria
- Professional clinical tone (neutral background, restrained palette)
- Clear hierarchy: title -> helper text -> fields -> primary CTA
- Responsive: card/container adapts without overflow

### Accessibility Requirements
- **WCAG Level**: AA
- **Focus States**: Visible focus on inputs and button
- **Screen Reader**: Inputs have labels; button has descriptive text
