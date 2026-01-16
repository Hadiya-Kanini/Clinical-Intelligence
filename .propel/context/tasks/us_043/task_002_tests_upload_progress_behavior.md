# Task - [TASK_002]

## Requirement Reference
- User Story: [us_043]
- Story Location: [.propel/context/tasks/us_043/us_043.md]
- Acceptance Criteria: 
    - Given files are uploading, When progress updates, Then each file shows a progress bar with percentage (UXR-020).
    - Given upload progress, When displayed, Then it updates in real-time (NFR-016).
    - Given multiple files, When uploading, Then each file has its own independent progress indicator.
    - Given upload in progress, When displayed, Then a cancel option is available for each file.

## Task Overview
Create comprehensive unit and integration tests for the per-file upload progress functionality. Tests should verify progress state transitions, percentage display accuracy, cancel functionality, and edge case handling (stalled uploads, instant uploads, retry behavior).

## Dependent Tasks
- [US_043/task_001] - Frontend upload progress percentage UI

## Impacted Components
- [CREATE | app/src/__tests__/DocumentUploadProgress.test.tsx | Unit tests for upload progress behavior]

## Implementation Plan

### 1. Test Progress Display
- Verify progress bar renders with correct percentage value
- Verify percentage text updates as progress changes
- Verify each file has independent progress tracking
- Verify progress bar uses correct ARIA attributes

### 2. Test Cancel Functionality
- Verify Cancel button appears only during "uploading" status
- Verify clicking Cancel stops the upload
- Verify cancelled file shows correct status
- Verify cancelled file can be retried

### 3. Test State Transitions
- Verify queued → uploading → success flow
- Verify queued → uploading → error flow
- Verify queued → uploading → cancelled flow
- Verify error → uploading (retry) flow

### 4. Test Edge Cases
- Verify behavior with very small files (instant upload)
- Verify behavior when multiple files upload simultaneously
- Verify progress continues correctly after tab regains focus
- Verify stalled upload indicator appears after timeout

### 5. Test Accessibility
- Verify progress bar has correct ARIA attributes
- Verify Cancel button is keyboard accessible
- Verify screen reader announcements for progress updates

**Focus on how to implement**

## Current Project State
```
app/src/
├── __tests__/
│   └── (existing test files)
├── pages/
│   └── DocumentUploadPage.tsx
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | app/src/__tests__/DocumentUploadProgress.test.tsx | Unit tests for upload progress behavior |

## External References
- https://testing-library.com/docs/react-testing-library/intro/
- https://vitest.dev/guide/

## Build Commands
- npm --prefix .\app run test

## Implementation Validation Strategy
- [Automated] All tests pass with `npm run test`
- [Automated] Test coverage includes progress display, cancel, state transitions, edge cases
- [Automated] Accessibility tests verify ARIA attributes

## Implementation Checklist
- [x] Create test file for upload progress behavior
- [x] Write tests for progress bar percentage display
- [x] Write tests for Cancel button functionality
- [x] Write tests for state transitions (queued → uploading → success/error/cancelled)
- [x] Write tests for retry flow after cancel/error
- [x] Write tests for edge cases (small files, multiple files)
- [x] Write accessibility tests for ARIA attributes
- [x] Verify all tests pass
