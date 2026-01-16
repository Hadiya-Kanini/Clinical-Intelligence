# Task - TASK_070_004

## Requirement Reference
- User Story: us_070
- Story Location: .propel/context/tasks/us_070/us_070.md
- Acceptance Criteria: 
    - Given critical entities (diagnoses, procedures, medications), When displayed, Then clickable reference links navigate to source (FR-054).
    - Given a reference click, When triggered, Then the source document section is highlighted (UXR-024).

## Task Overview
Add frontend validation coverage ensuring citation metadata renders correctly and citation clicks trigger source navigation/highlight behavior in Patient 360.

This task focuses on automated tests only (no feature work):
- Unit/integration tests for `Patient360Page` citation rendering and click behavior
- E2E tests validating the interactive flow at a high level

## Dependent Tasks
- [TASK_070_001] (Citation UI + API wiring)
- [TASK_070_002] (Source viewer navigation and highlight)

## Impacted Components
- [CREATE | app/src/__tests__/patient360.citations.spec.tsx | Component-level tests for citation rendering + click intent]
- [CREATE | app/src/__tests__/e2e/patient360_citations.spec.ts | Playwright test for citation click -> viewer navigation/highlight]

## Implementation Plan
- Component-level tests (Testing Library / Vitest):
  - Render `Patient360Page` with mocked `patient360Api` response containing citations.
  - Assert document name/page/section appear in the DOM.
  - Trigger click on citation link and assert the viewer receives the selected citation intent (e.g., state update / viewer prop).
- E2E tests (Playwright):
  - Login as a standard user.
  - Navigate to `/patients/:patientId`.
  - Click a citation link.
  - Assert that the viewer panel updates (e.g., shows selected document name and indicates navigation/highlight state).
- Keep tests resilient:
  - Avoid brittle pixel assertions; prefer text/role-based selectors.

**Focus on how to implement**

## Current Project State
- Playwright is present (`npm --prefix .\app run test:e2e`).
- Patient 360 is currently a UI stub and has no automated coverage for citation interactions.

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | app/src/__tests__/patient360.citations.spec.tsx | Tests for citation metadata and click intent wiring |
| CREATE | app/src/__tests__/e2e/patient360_citations.spec.ts | Playwright test for citation click -> viewer behavior |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://testing-library.com/docs/react-testing-library/intro/
- https://playwright.dev/docs/test-intro

## Build Commands
- npm --prefix .\app run test
- npm --prefix .\app run test:e2e

## Implementation Validation Strategy
- [Unit/UI] Confirm tests fail when citation metadata is removed and pass when it is present.
- [E2E] Confirm citation click updates viewer panel state deterministically.

## Implementation Checklist
- [ ] Add component-level tests for citation metadata rendering
- [ ] Add component-level test for citation click emitting navigation intent
- [ ] Add Playwright test for citation click driving viewer update/highlight indicator
- [ ] Ensure tests run reliably in CI (no timing flakiness)
