# Task - TASK_072_003

## Requirement Reference
- User Story: us_072
- Story Location: .propel/context/tasks/us_072/us_072.md
- Acceptance Criteria: 
    - Given Patient 360 view, When displayed, Then source PDF is on left and extracted data on right.
    - Given the split view, When scrolling, Then synchronized scrolling is available.
    - What happens when the PDF viewer fails to load?

## Task Overview
Add automated test coverage for the Patient 360 side-by-side verification view (SCR-008), focusing on layout wiring and synchronized scrolling behavior.

This task focuses on tests only (no feature work):
- Component-level tests for split view structure and sync-scroll toggle wiring
- E2E tests validating the high-level user experience of split view and sync-scroll

## Dependent Tasks
- [US_072 TASK_072_001] (Split view layout implemented)
- [US_072 TASK_072_002] (Synchronized scrolling implemented)

## Impacted Components
- [CREATE | app/src/__tests__/patient360.split_view.spec.tsx | Component-level tests for split view layout and sync-scroll control]
- [CREATE | app/src/__tests__/e2e/patient360_split_view_sync_scroll.spec.ts | Playwright E2E tests for split view + sync-scroll behavior]

## Implementation Plan
- Component-level tests (Testing Library / Vitest):
  - Render `Patient360Page` and assert that left and right panes exist.
  - Assert the left pane contains the viewer container (or placeholder/error state depending on mocks).
  - Toggle sync-scroll on/off and assert the UI state changes (e.g., aria-checked or text indicator).
  - Simulate scroll events on the right pane and assert the left pane scroll position updates when sync is enabled (mock scroll containers and use deterministic heights).
  - Add a test case for viewer failure state (mock the viewer component to render an error) and assert the sync toggle is disabled.
- E2E tests (Playwright):
  - Login as a standard user.
  - Navigate to `/patients/:patientId`.
  - Confirm split view is visible (PDF panel + extracted-data panel).
  - Enable sync-scroll and scroll the right pane; confirm the left viewer scroll position changes (use DOM evaluation to compare `scrollTop` values).
  - Disable sync-scroll and confirm the panes scroll independently.
- Keep tests resilient:
  - Prefer role/text selectors.
  - Avoid brittle pixel-perfect assertions; verify behavior and state.

**Focus on how to implement**

## Current Project State
- Test folder exists at `app/src/__tests__`.
- Playwright config exists at `app/playwright.config.js`.

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | app/src/__tests__/patient360.split_view.spec.tsx | Unit/integration tests for split view structure and sync-scroll wiring |
| CREATE | app/src/__tests__/e2e/patient360_split_view_sync_scroll.spec.ts | Playwright tests for end-to-end split view + sync-scroll behavior |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://testing-library.com/docs/react-testing-library/intro/
- https://playwright.dev/docs/test-intro

## Build Commands
- npm --prefix .\app run test
- npm --prefix .\app run test:e2e

## Implementation Validation Strategy
- [Unit/UI] Tests should fail if split view panes are removed or if sync-scroll toggle does not affect scroll synchronization.
- [E2E] Playwright test should confirm deterministic scroll linking behavior when sync is enabled.

## Implementation Checklist
- [ ] Add component-level tests for split view panes and basic rendering
- [ ] Add component-level test verifying sync-scroll toggle enables/disables synchronization
- [ ] Add component-level test for viewer error state disables sync-scroll
- [ ] Add Playwright test for split view presence and sync-scroll behavior
- [ ] Ensure tests are stable (no timing flakiness)
