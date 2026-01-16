# Task - TASK_056_005

## Requirement Reference
- User Story: us_056
- Story Location: .propel/context/tasks/us_056/us_056.md
- Acceptance Criteria: 
    - Given processing fails, When error occurs, Then error message is captured and displayed to user (FR-028).
    - Given processing metadata, When stored, Then it includes start time, completion time, and duration.
    - Given the document list, When a document has failed, Then the error message is visible.

## Task Overview
Add frontend automated tests to ensure processing error messages and timing metadata remain visible in the document list UI as expected.

This task focuses on unit/integration tests at the React component level (and optionally Playwright coverage if already used for similar UI flows).

## Dependent Tasks
- [TASK_056_003] (Frontend document list shows processing errors and timing)

## Impacted Components
- [CREATE | app/src/__tests__/pages/documentListPage.processingMetadata.test.tsx | Tests for DocumentListPage rendering of error + timing metadata]

## Implementation Plan
- Mock `apiClient` layer:
  - Mock `documentsApi` (preferred) or mock `api.get` to return a deterministic response.
- Test scenarios:
  - Failed document renders `errorMessage` text.
  - Completed document renders timing metadata (duration and/or timestamps).
  - Long error message does not break layout expectations (assert truncated rendering rule if implemented).
  - API error returns an `Alert` with safe message.
- Keep tests aligned with existing test conventions in `app/src/__tests__`.

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | app/src/__tests__/pages/documentListPage.processingMetadata.test.tsx | Validate that error messages and timing metadata are rendered for failed/completed documents |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://testing-library.com/docs/react-testing-library/intro/

## Build Commands
- npm --prefix .\app run test

## Implementation Validation Strategy
- Ensure tests cover both failed and non-failed cases.
- Ensure tests are stable (no timers unless necessary; prefer deterministic rendering).

## Implementation Checklist
- [ ] Create `DocumentListPage` test file under `app/src/__tests__/pages/`
- [ ] Mock API response containing failed + completed documents
- [ ] Assert failed documents display `errorMessage`
- [ ] Assert completed/failed documents display timing metadata
- [ ] Add test for API error state rendering an `Alert`
