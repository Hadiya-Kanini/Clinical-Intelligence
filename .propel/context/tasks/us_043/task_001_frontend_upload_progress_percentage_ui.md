# Task - [TASK_001]

## Requirement Reference
- User Story: [us_043]
- Story Location: [.propel/context/tasks/us_043/us_043.md]
- Acceptance Criteria: 
    - Given files are uploading, When progress updates, Then each file shows a progress bar with percentage (UXR-020).
    - Given upload progress, When displayed, Then it updates in real-time (NFR-016).
    - Given multiple files, When uploading, Then each file has its own independent progress indicator.
    - Given upload in progress, When displayed, Then a cancel option is available for each file.

## Task Overview
Enhance the Document Upload page (SCR-005) to display real-time per-file upload progress with percentage indicators. Each file in the upload queue must show its own independent progress bar with percentage text, updating in real-time during upload. Users must be able to cancel individual file uploads while in progress.

This task builds on the existing `DocumentUploadPage.tsx` which already has basic progress tracking. The enhancement focuses on improving the visual feedback and adding cancel functionality per file.

## Dependent Tasks
- [US_042/task_001] - Document upload drag-and-drop queue UI (completed)

## Impacted Components
- [MODIFY | app/src/pages/DocumentUploadPage.tsx | Enhance progress display with percentage, add per-file cancel button during upload]
- [MODIFY | app/src/components/ui/ProgressBar.tsx | Ensure component supports percentage label display]

## Implementation Plan

### 1. Enhance ProgressBar Component
- Verify `ProgressBar` component accepts and displays percentage label
- Ensure smooth animation for progress transitions (300ms ease per designsystem.md)
- Support variant styling for different states (uploading, success, error)

### 2. Update Upload Item Display
- Display percentage text prominently (e.g., "45%") alongside progress bar
- Show estimated time remaining when possible (optional enhancement)
- Ensure progress updates are smooth and don't cause layout shifts

### 3. Add Per-File Cancel Functionality
- Add "Cancel" button visible during `uploading` status
- Implement `AbortController` pattern for cancellable uploads
- On cancel: update item status to `cancelled`, stop progress updates
- Ensure cancelled files can be retried

### 4. Handle Edge Cases
- **Stalled uploads**: Show visual indicator if no progress for >10 seconds
- **Very small files**: Handle instant uploads gracefully (show brief success state)
- **Backgrounded tab**: Ensure progress continues when tab regains focus
- **Network interruption**: Show appropriate error state with retry option

### 5. Real-Time Updates
- Use `XMLHttpRequest.upload.onprogress` or equivalent for actual progress
- For simulation (current implementation): ensure smooth incremental updates
- Progress should update at minimum every 500ms during active upload

**Focus on how to implement**

## Current Project State
```
app/src/
├── pages/
│   └── DocumentUploadPage.tsx  # Main upload page with basic progress
├── components/
│   └── ui/
│       ├── ProgressBar.tsx     # Progress bar component
│       ├── Button.tsx          # Button component
│       ├── Card.tsx            # Card component
│       └── Alert.tsx           # Alert component
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | app/src/pages/DocumentUploadPage.tsx | Add cancel button per file during upload, enhance progress display with percentage, handle edge cases |
| MODIFY | app/src/components/ui/ProgressBar.tsx | Ensure percentage label support and smooth transitions |

## External References
- https://developer.mozilla.org/en-US/docs/Web/API/XMLHttpRequest/upload
- https://developer.mozilla.org/en-US/docs/Web/API/AbortController
- https://react.dev/learn/synchronizing-with-effects

## Build Commands
- npm --prefix .\app run build
- npm --prefix .\app run test

## Implementation Validation Strategy
- [Manual/UI] Each uploading file shows progress bar with percentage (e.g., "45%")
- [Manual/UI] Progress updates smoothly in real-time during upload
- [Manual/UI] Cancel button appears for files with status "uploading"
- [Manual/UI] Clicking Cancel stops the upload and shows cancelled state
- [Manual/UI] Cancelled files can be retried
- [Manual/UI] Very small files show brief success animation
- [Automated] Unit tests verify progress state transitions

## Implementation Checklist
- [x] Verify ProgressBar component displays percentage label correctly
- [x] Add Cancel button to file items during uploading status
- [x] Implement AbortController pattern for cancellable uploads
- [x] Update progress display to show percentage prominently
- [x] Handle stalled upload visual indicator (>10s no progress)
- [x] Handle instant upload for very small files
- [x] Ensure smooth progress animation (300ms transition)
- [x] Test cancel and retry flow

---

# Design Reference

## UI Impact Assessment
**Has UI Changes**: [x] Yes [ ] No

## User Story Design Context
**Story ID**: US-043
**Story Title**: Display per-file upload progress with percentage
**UI Impact Type**: UI Enhancement

### Design Source References
- **Design System**: `.propel/context/docs/designsystem.md`
- **Screen Spec**: `.propel/context/docs/figma_spec.md` (SCR-005, UXR-020, NFR-016)

### Screen-to-Design Mappings
| Screen/Feature | Image File | Image Path | Description | Implementation Priority |
|---------------|------------|------------|-------------|----------------------|
| Document Upload (SCR-005) | N/A | N/A | Per-file progress bars with percentage and cancel option | High |

### Design Tokens
```yaml
colors:
  primary:
    value: "#2196F3"
    usage: "Progress bar fill, active states"
  success:
    value: "#4CAF50"
    usage: "Completed upload indicator"
  error:
    value: "#F44336"
    usage: "Failed upload indicator"

progress-bar:
  height: "8px"
  background: "neutral-200"
  fill: "primary-500"
  border-radius: "radius-full"
  transition: "width 0.3s ease"
```

### Component References
| Component Name | Reference Image | Code Location | UI Changes Required |
|---------------|-----------------|---------------|-------------------|
| ProgressBar | N/A | app/src/components/ui/ProgressBar.tsx | Ensure percentage label display |
| Button | N/A | app/src/components/ui/Button.tsx | Cancel button variant |

### Task Design Mapping
```yaml
TASK_043_001:
  title: "Per-file upload progress with percentage"
  ui_impact: true
  visual_references:
    figma_spec_refs: ["SCR-005", "UXR-020"]
  components_affected:
    - ProgressBar
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
    progressBarVisible: true,
    percentageTextVisible: true,
    cancelButtonVisible: true
  }
};
```

### Accessibility Requirements
- **WCAG Level**: AA
- **Progress Announcement**: Use `aria-valuenow`, `aria-valuemin`, `aria-valuemax` on progress bar
- **Cancel Button**: Accessible via keyboard, clear focus state
- **Screen Reader**: Progress updates announced via `aria-live="polite"` region
