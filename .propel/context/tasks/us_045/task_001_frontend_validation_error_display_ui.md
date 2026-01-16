# Task - [TASK_001]

## Requirement Reference
- User Story: [us_045]
- Story Location: [.propel/context/tasks/us_045/us_045.md]
- Acceptance Criteria: 
    - Given a file fails validation, When the error is displayed, Then it specifies the exact issue (size, type, corrupted) (UXR-021).
    - Given validation errors, When displayed, Then each file shows its own specific error inline.
    - Given a validation error, When displayed, Then it includes guidance on how to fix the issue.
    - Given multiple files, When some fail validation, Then valid files can still be uploaded.

## Task Overview
Enhance the Document Upload page (SCR-005) to display clear, actionable validation error messages for each file. Errors must be shown inline per file, specify the exact issue (file too large, wrong type, corrupted), and include guidance on how to fix the issue. Valid files in a batch should remain uploadable even when some files fail validation.

This task builds on the existing `DocumentUploadPage.tsx` which has basic validation. The enhancement focuses on improving error messaging clarity and user guidance.

## Dependent Tasks
- [US_042/task_001] - Document upload drag-and-drop queue UI (completed)
- [US_043/task_001] - Upload progress percentage UI

## Impacted Components
- [MODIFY | app/src/pages/DocumentUploadPage.tsx | Enhance validation error display with specific messages and guidance]
- [CREATE | app/src/components/ui/ValidationError.tsx | Reusable validation error component with guidance]

## Implementation Plan

### 1. Define Validation Error Types
```typescript
type ValidationErrorType = 
  | 'invalid_type'      // Wrong file format
  | 'file_too_large'    // Exceeds 50MB
  | 'file_corrupted'    // Cannot read file
  | 'password_protected' // PDF is password protected
  | 'empty_file';       // File has no content

interface ValidationError {
  type: ValidationErrorType;
  message: string;
  guidance: string;
}
```

### 2. Create Validation Error Messages with Guidance
| Error Type | Message | Guidance |
|------------|---------|----------|
| invalid_type | "Unsupported file type" | "Please upload PDF or DOCX files only" |
| file_too_large | "File exceeds 50MB limit" | "Reduce file size or split into smaller documents" |
| file_corrupted | "File appears to be corrupted" | "Try re-downloading or re-exporting the file" |
| password_protected | "Password-protected files not supported" | "Remove password protection and try again" |
| empty_file | "File is empty" | "Ensure the file contains content before uploading" |

### 3. Inline Error Display per File
- Show error icon (red X) next to file name
- Display error message in error color (error-main: #F44336)
- Show guidance text below error message
- Use Alert component styling for consistency

### 4. Partial Batch Upload
- When some files fail validation, keep valid files in queue
- Show clear visual distinction between valid and invalid files
- Allow user to remove invalid files and proceed with valid ones
- "Upload" button should work for valid files only

### 5. Handle Multiple Errors per File
- If file has multiple issues, show primary error first
- Provide option to see all errors (expandable)

### 6. Error Recovery Actions
- "Remove" button to remove invalid file from queue
- "Retry" option if error might be transient
- Clear error state when file is removed

**Focus on how to implement**

## Current Project State
```
app/src/
├── pages/
│   └── DocumentUploadPage.tsx  # Main upload page with basic validation
├── components/
│   └── ui/
│       ├── Alert.tsx           # Alert component for messages
│       ├── Button.tsx          # Button component
│       └── Card.tsx            # Card component
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | app/src/pages/DocumentUploadPage.tsx | Enhance validation error display with specific messages, guidance, and partial batch support |
| CREATE | app/src/components/ui/ValidationError.tsx | Reusable validation error component with icon, message, and guidance |

## External References
- https://react.dev/learn/conditional-rendering
- https://www.w3.org/WAI/WCAG21/Understanding/error-identification.html

## Build Commands
- npm --prefix .\app run build
- npm --prefix .\app run test

## Implementation Validation Strategy
- [Manual/UI] Invalid file type shows "Unsupported file type" with guidance
- [Manual/UI] Oversized file shows "File exceeds 50MB limit" with guidance
- [Manual/UI] Each file shows its own specific error inline
- [Manual/UI] Valid files in batch can still be uploaded when some fail
- [Manual/UI] Error messages use error color styling
- [Automated] Unit tests verify error message content

## Implementation Checklist
- [x] Define ValidationError types and messages
- [x] Create ValidationError component with icon, message, and guidance
- [x] Update validateFiles function to return specific error types
- [x] Display inline errors per file in the queue list
- [x] Ensure valid files remain uploadable when some fail
- [x] Style errors using design system error colors
- [x] Add Remove button for invalid files
- [x] Test all error types display correctly
- [x] Verify accessibility (error announcements, color contrast)

---

# Design Reference

## UI Impact Assessment
**Has UI Changes**: [x] Yes [ ] No

## User Story Design Context
**Story ID**: US-045
**Story Title**: Display clear file validation errors
**UI Impact Type**: UI Enhancement

### Design Source References
- **Design System**: `.propel/context/docs/designsystem.md`
- **Screen Spec**: `.propel/context/docs/figma_spec.md` (SCR-005, UXR-021)

### Screen-to-Design Mappings
| Screen/Feature | Image File | Image Path | Description | Implementation Priority |
|---------------|------------|------------|-------------|----------------------|
| Document Upload (SCR-005) | N/A | N/A | Inline validation errors with specific messages and guidance | High |

### Design Tokens
```yaml
colors:
  error:
    light: "#FFEBEE"
    main: "#F44336"
    dark: "#D32F2F"
    usage: "Error backgrounds, text, icons"

alert-error:
  background: "error-light"
  border-color: "error-main"
  color: "error-dark"
  border-left: "4px solid"
  padding: "spacing-4"
  border-radius: "radius-md"

typography:
  error-message:
    font: "body"
    color: "error-main"
  guidance-text:
    font: "body-small"
    color: "neutral-600"
```

### Component References
| Component Name | Reference Image | Code Location | UI Changes Required |
|---------------|-----------------|---------------|-------------------|
| Alert | N/A | app/src/components/ui/Alert.tsx | Error variant styling |
| ValidationError | N/A | app/src/components/ui/ValidationError.tsx | New component |

### Task Design Mapping
```yaml
TASK_045_001:
  title: "Validation error display with guidance"
  ui_impact: true
  visual_references:
    figma_spec_refs: ["SCR-005", "UXR-021"]
  components_affected:
    - ValidationError (new)
    - DocumentUploadPage
  visual_validation_required: true
```

### Visual Validation Criteria
```typescript
const visualValidation = {
  screenshotComparison: {
    maxDifference: "5%",
    breakpoints: [1280, 1440, 1920]
  },
  componentValidation: {
    errorIconVisible: true,
    errorMessageVisible: true,
    guidanceTextVisible: true,
    errorColorCorrect: true
  }
};
```

### Accessibility Requirements
- **WCAG Level**: AA
- **Error Identification**: Errors must identify the specific field/file in error
- **Error Suggestion**: Provide guidance on how to fix the error
- **Color Contrast**: Error text must meet 4.5:1 contrast ratio
- **Screen Reader**: Errors announced via `aria-live="assertive"` or `role="alert"`
