# Task - TASK_001

## Requirement Reference
- User Story: us_006
- Story Location: .propel/context/tasks/us_006/us_006.md
- Acceptance Criteria: 
    - Given I am unauthenticated, When I view the Login page, Then the page uses design system tokens (no hard-coded colors) and remains readable and consistent.

## Task Overview
Establish the frontend design token layer and reusable UI primitives needed by SCR-001 (Login) so the page can be implemented using the project design system (colors, typography, spacing, focus states) without hard-coded values.

## Dependent Tasks
- .propel/context/tasks/us_001/task_001_scaffold_service_structure.md (TASK_001)

## Impacted Components
- app/package.json
- app/src/

## Implementation Plan
- Introduce a token source for the Web UI derived from `.propel/context/docs/designsystem.md`.
- Implement token wiring in a way that prevents hard-coded colors in component styling:
  - Prefer CSS variables (e.g., `--color-primary-500`, `--color-neutral-0`) and a small set of semantic aliases (e.g., `--color-bg`, `--color-text`, `--color-border`).
  - Ensure focus ring styles align to the design system (2px outline using primary token + offset).
- Create minimal reusable primitives required by SCR-001:
  - TextField (supports Default, Focus, Error, Disabled)
  - Button (Primary + Disabled + Loading visual state)
  - Link (for “Forgot password”)
  - Alert (for validation/auth errors)
- Ensure primitives follow accessibility basics:
  - Labels are associated with inputs.
  - Focus states are visible.
  - Disabled state uses non-interactive semantics.
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | app/src/styles/tokens.css | Define CSS variables for colors/typography/spacing based on designsystem.md tokens. |
| CREATE | app/src/styles/base.css | Base element styles using tokens (body font, default text colors, focus ring). |
| MODIFY | app/src/main.jsx | Import global styles once at app bootstrap. |
| CREATE | app/src/components/ui/TextField.jsx | Reusable TextField primitive aligned to token rules and required states. |
| CREATE | app/src/components/ui/Button.jsx | Reusable Button primitive aligned to token rules and required states. |
| CREATE | app/src/components/ui/Link.jsx | Reusable Link primitive aligned to token rules and required states. |
| CREATE | app/src/components/ui/Alert.jsx | Reusable Alert primitive aligned to token rules and required states. |
| MODIFY | app/package.json | Add any required UI dependency only if necessary (prefer no additional dependencies for primitives). |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://www.w3.org/WAI/WCAG21/Understanding/focus-visible.html

## Build Commands
- npm install
- npm run dev
- npm run build

## Implementation Validation Strategy
- Verify no hard-coded hex colors are introduced in the Login UI implementation files; styles reference tokens only.
- Verify focus ring visibility on keyboard navigation for inputs, links, and buttons.
- Verify TextField error state can be triggered and is visually distinguishable without relying on color alone (e.g., helper/error text).

## Implementation Checklist
- [x] Create token definitions aligned to `.propel/context/docs/designsystem.md`
- [x] Add base styles that apply typography tokens and default foreground/background tokens
- [x] Implement UI primitives (TextField, Button, Link, Alert)
- [x] Validate keyboard focus visibility across primitives
- [x] Confirm primitives can render in default, disabled, and error (where applicable) states

# Design Reference

## UI Impact Assessment
**Has UI Changes**: [x] Yes [ ] No

## User Story Design Context
**Story ID**: US-006
**Story Title**: Create professional login page UI
**UI Impact Type**: New UI

### Design Source References
- **Design System**: `.propel/context/docs/designsystem.md`
- **Screen Spec**: `.propel/context/docs/figma_spec.md` (SCR-001)

### Screen-to-Design Mappings
| Screen/Feature | Image File | Image Path | Description | Implementation Priority |
|---------------|------------|------------|-------------|----------------------|
| Login (SCR-001) | N/A | N/A | Implement based on figma_spec + designsystem tokens (no static mock provided) | High |

### Design Tokens
```yaml
colors:
  primary:
    value: "designsystem.md: primary-500"
    usage: "Primary CTA (Login), interactive states, focus ring"
  neutral:
    value: "designsystem.md: neutral-0/50/200/800"
    usage: "Page background, card background, borders, primary text"
  error:
    value: "designsystem.md: error-main/error-light"
    usage: "Validation and authentication error states"

typography:
  primary:
    value: "designsystem.md: Inter"
    usage: "All login screen text"

spacing:
  base:
    value: "designsystem.md: 8px"
    usage: "Form spacing and layout padding"

radius:
  md:
    value: "designsystem.md: 8px"
    usage: "Inputs and buttons"
```

### Component References
| Component Name | Reference Image | Code Location | UI Changes Required |
|---------------|-----------------|---------------|-------------------|
| TextField | N/A | app/src/components/ui/TextField.jsx | New primitive aligned to token rules + states |
| Button | N/A | app/src/components/ui/Button.jsx | New primitive aligned to token rules + states |
| Link | N/A | app/src/components/ui/Link.jsx | New primitive aligned to token rules + states |
| Alert | N/A | app/src/components/ui/Alert.jsx | New primitive aligned to token rules + states |

### Visual Validation Criteria
- Tokens-only styling (no hard-coded colors)
- Focus ring visible on all interactive components
- Typography hierarchy matches designsystem type scale (labels/body)

### Accessibility Requirements
- **WCAG Level**: AA
- **Focus States**: Implement token-based outlines for all interactive primitives
- **Screen Reader**: Inputs must support `label`/`aria-describedby` wiring
