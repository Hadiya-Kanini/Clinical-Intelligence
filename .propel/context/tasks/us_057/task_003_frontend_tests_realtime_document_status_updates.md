# Task - TASK_057_003

## Requirement Reference
- User Story: us_057
- Story Location: .propel/context/tasks/us_057/us_057.md
- Acceptance Criteria: 
    - Given documents are processing, When status changes, Then the UI updates automatically (FR-026).
    - Given real-time updates, When implemented, Then they refresh at least every 5 seconds (UXR-043).
    - Given the UI, When real-time updates are active, Then a manual refresh button is also available.

## Task Overview
Add frontend automated tests to ensure the Document List real-time refresh behavior remains correct:
- Polling triggers refreshes while processing is active
- Manual refresh triggers an immediate refresh
- Errors are handled safely without clearing existing data

Estimated Effort: 4 hours

## Dependent Tasks
- [TASK_057_001] (Polling-based real-time status updates)
- [TASK_057_002] (Connection resilience and manual refresh)

## Impacted Components
- [CREATE | app/src/__tests__/hooks/useDocumentListPolling.test.tsx | Hook-level tests using fake timers to validate polling interval, cleanup, and error handling]
- [CREATE | app/src/__tests__/pages/documentListPage.realtimeUpdates.test.tsx | Page-level tests validating status updates and manual refresh integration]

## Implementation Plan
- Hook tests (`useDocumentListPolling`):
  - Use fake timers to assert the 5s interval triggers repeated refresh calls
  - Assert no overlap: if a request is in-flight, a tick does not trigger a second request
  - Assert cleanup clears interval on unmount
  - Assert error case preserves last successful data and exposes error state
- Page tests (`DocumentListPage`):
  - Mock `documentsApi` (preferred) or `apiClient` to return deterministic document lists
  - Validate status badge changes after a polling tick
  - Validate Refresh button invokes immediate fetch

**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | app/src/__tests__/hooks/useDocumentListPolling.test.tsx | Validate polling interval, cleanup, non-overlap behavior, and safe error state |
| CREATE | app/src/__tests__/pages/documentListPage.realtimeUpdates.test.tsx | Validate DocumentListPage integrates polling and manual refresh correctly |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://testing-library.com/docs/react-testing-library/intro/
- https://jestjs.io/docs/timer-mocks

## Build Commands
- npm --prefix .\app run test

## Implementation Validation Strategy
- Ensure tests are deterministic (fake timers, no real network).
- Ensure interval cleanup is asserted to prevent memory leaks.
- Ensure the 5-second requirement is encoded in tests to prevent regression.

## Implementation Checklist
- [ ] Add hook tests for 5s polling interval and cleanup
- [ ] Add hook tests for non-overlapping requests
- [ ] Add hook tests for error handling preserving last data
- [ ] Add page tests for status badge updating after a polling tick
- [ ] Add page tests for manual Refresh button
- [ ] Run `npm --prefix .\app run test` and confirm green
