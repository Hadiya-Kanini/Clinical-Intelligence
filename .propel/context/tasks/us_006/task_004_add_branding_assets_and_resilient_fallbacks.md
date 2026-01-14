# Task - TASK_004

## Requirement Reference
- User Story: us_006
- Story Location: .propel/context/tasks/us_006/us_006.md
- Acceptance Criteria: 
    - Given I am unauthenticated, When I navigate to the Login page (SCR-001), Then the page renders a clean, professional healthcare-appropriate design and branding (medical color palette, professional typography, clear hierarchy).

## Task Overview
Add basic branding assets for the Login page and ensure the UI degrades gracefully when assets are unavailable (logo missing/broken), maintaining a professional appearance and clear hierarchy.

## Dependent Tasks
- .propel/context/tasks/us_006/task_002_implement_login_page_ui_and_responsive_layout.md (TASK_002)

## Impacted Components
- app/src/pages/LoginPage.jsx
- app/src/assets/

## Implementation Plan
- Introduce a lightweight brand header for SCR-001:
  - Prefer an SVG logo asset if available.
  - Always render a text fallback (product name) so the header remains meaningful even if the image fails to load.
- Ensure the header uses token-based typography and spacing.
- Implement resilient fallback behavior:
  - If an image fails to load, hide the broken image and keep the text title visible.
  - Ensure `alt` text is present for accessibility.
- Ensure the login card layout remains stable with or without the logo.
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | app/src/assets/logo.svg | Placeholder logo asset (non-sensitive, not environment-specific). |
| MODIFY | app/src/pages/LoginPage.jsx | Add logo rendering with robust fallback to text title when image is unavailable. |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://developer.mozilla.org/en-US/docs/Web/HTML/Element/img#fallback_content

## Build Commands
- npm run dev
- npm run build

## Implementation Validation Strategy
- Verify the Login page shows a coherent brand header when the logo loads.
- Verify the Login page still looks professional and readable when the logo is intentionally missing/broken (text fallback remains and layout stays stable).

## Implementation Checklist
- [x] Add a placeholder logo asset in `app/src/assets/`
- [x] Render logo with text fallback in Login page header
- [x] Validate layout stability for both logo present and missing scenarios

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
- Brand header maintains hierarchy (logo/title) without visual clutter
- No layout shift or awkward spacing when logo is missing
- Token-only styling for spacing, typography, and colors

### Accessibility Requirements
- **WCAG Level**: AA
- **Screen Reader**: Provide meaningful `alt` text for logo and a readable text title fallback
