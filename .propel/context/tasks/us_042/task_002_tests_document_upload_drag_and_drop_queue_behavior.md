# Task - [TASK_002]

## Requirement Reference
- User Story: [us_042]
- Story Location: [.propel/context/tasks/us_042/us_042.md]
- Acceptance Criteria: 
    - [Given I am on the Document Upload page (SCR-005), When I drag files onto the drop zone, Then they are queued for upload.]
    - [Given the drop zone, When files are dragged over it, Then it highlights to indicate it's a valid drop target (UXR-019).]
    - [Given file selection, When I use the file picker, Then I can select multiple files at once (UXR-022).]
    - [Given files are selected, When displayed, Then the file count and names are shown before upload.]

## Task Overview
Add automated test coverage (unit/integration + E2E) for the Document Upload (SCR-005) drag-and-drop and file-picker queueing behavior.

The goal is to lock in UI contract behavior (queueing, highlighting, count + filenames) and prevent regressions.

## Dependent Tasks
- [US_042 TASK_001] (Frontend document upload queue UI)

## Impacted Components
- [CREATE | app/src/__tests__/visual/documentUpload.spec.ts | Playwright E2E tests for Document Upload (SCR-005): drag-over highlight, drop queueing, multi-select picker behavior]
- [CREATE | app/src/__tests__/DocumentUploadPage.test.tsx | Vitest + React Testing Library tests for queueing/validation behavior where feasible]

## Implementation Plan
- Playwright E2E coverage:
  - Add a new spec file aligned with existing patterns under `app/src/__tests__/visual/`.
  - Validate drag-over highlight:
    - Trigger drag enter/over and assert the drop zone reflects the active style/state.
  - Validate drop queueing:
    - Simulate dropping multiple files onto the drop zone and assert:
      - Selected count updates.
      - Filenames appear in the queued list.
  - Validate file picker multi-select:
    - Use Playwright file chooser / setInputFiles to attach multiple files and assert the same queue behavior.
- Vitest + React Testing Library:
  - Render `DocumentUploadPage` and simulate:
    - Selecting multiple files in the hidden input (via `fireEvent.change`).
    - Validation behavior for invalid extensions and oversized files.
    - Queue list rendering (count + filenames).

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | app/src/__tests__/visual/documentUpload.spec.ts | E2E: drag-over highlights, drop queues files, picker multi-select queues files, filenames + count visible |
| CREATE | app/src/__tests__/DocumentUploadPage.test.tsx | Unit/integration: validate queuing and file validation logic (types, max size, max count) |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://playwright.dev/docs/test-intro
- https://testing-library.com/docs/react-testing-library/intro

## Build Commands
- npm --prefix .\\app run test
- npm --prefix .\\app run test:e2e

## Implementation Validation Strategy
- [Automated] `npm --prefix .\app run test` passes with new `DocumentUploadPage` tests.
- [Automated] `npm --prefix .\app run test:e2e` passes with stable drag/drop + file-picker coverage for SCR-005.

## Implementation Checklist
- [x] Add Playwright E2E test file for SCR-005
- [x] Validate drag-over highlight behavior via E2E
- [x] Validate drop queueing behavior via E2E
- [x] Validate file picker multi-select queueing behavior via E2E
- [x] Add Vitest/RTL coverage for queue rendering + validation edge cases
