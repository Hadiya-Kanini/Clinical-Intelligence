# Task - [TASK_002]

## Requirement Reference
- User Story: [us_045]
- Story Location: [.propel/context/tasks/us_045/us_045.md]
- Acceptance Criteria: 
    - Given a file fails validation, When the error is displayed, Then it specifies the exact issue (size, type, corrupted) (UXR-021).
    - Given validation errors, When displayed, Then each file shows its own specific error inline.
    - Given a validation error, When displayed, Then it includes guidance on how to fix the issue.
    - Given multiple files, When some fail validation, Then valid files can still be uploaded.

## Task Overview
Create comprehensive unit and integration tests for the validation error display functionality. Tests should verify error message specificity, guidance text display, inline error rendering per file, and partial batch upload behavior when some files fail validation.

## Dependent Tasks
- [US_045/task_001] - Frontend validation error display UI

## Impacted Components
- [CREATE | app/src/__tests__/ValidationErrorDisplay.test.tsx | Unit tests for validation error display behavior]

## Implementation Plan

### 1. Test Error Message Specificity
- Verify invalid file type shows "Unsupported file type" message
- Verify oversized file shows "File exceeds 50MB limit" message
- Verify corrupted file shows "File appears to be corrupted" message
- Verify password-protected file shows appropriate message
- Verify empty file shows "File is empty" message

### 2. Test Guidance Text Display
- Verify each error type includes guidance text
- Verify guidance text provides actionable fix suggestion
- Verify guidance text is visually distinct from error message

### 3. Test Inline Error Rendering
- Verify error appears next to the specific file
- Verify error icon is displayed
- Verify error uses correct color styling
- Verify multiple files can have different errors simultaneously

### 4. Test Partial Batch Upload
- Verify valid files remain in queue when some fail
- Verify Upload button works for valid files only
- Verify invalid files can be removed individually
- Verify removing invalid file updates queue correctly

### 5. Test Error Recovery
- Verify Remove button removes invalid file
- Verify queue updates correctly after removal
- Verify valid files can be uploaded after removing invalid ones

### 6. Test Accessibility
- Verify error messages have correct ARIA attributes
- Verify errors are announced to screen readers
- Verify error text meets color contrast requirements

**Focus on how to implement**

## Current Project State
```
app/src/
├── __tests__/
│   └── (existing test files)
├── pages/
│   └── DocumentUploadPage.tsx
├── components/
│   └── ui/
│       └── ValidationError.tsx
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | app/src/__tests__/ValidationErrorDisplay.test.tsx | Unit tests for validation error display behavior |

## External References
- https://testing-library.com/docs/react-testing-library/intro/
- https://vitest.dev/guide/

## Build Commands
- npm --prefix .\app run test

## Implementation Validation Strategy
- [Automated] All tests pass with `npm run test`
- [Automated] Test coverage includes all error types
- [Automated] Test coverage includes partial batch behavior
- [Automated] Accessibility tests verify ARIA attributes

## Implementation Checklist
- [x] Create test file for validation error display
- [x] Write tests for error message specificity
- [x] Write tests for guidance text display
- [x] Write tests for inline error rendering
- [x] Write tests for partial batch upload behavior
- [x] Write tests for error recovery (remove invalid files)
- [x] Write accessibility tests for error announcements
- [x] Verify all tests pass
