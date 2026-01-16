# Task - [TASK_001]

## Requirement Reference
- User Story: [us_042]
- Story Location: [.propel/context/tasks/us_042/us_042.md]
- Acceptance Criteria: 
    - [Given I am on the Document Upload page (SCR-005), When I drag files onto the drop zone, Then they are queued for upload.]
    - [Given the drop zone, When files are dragged over it, Then it highlights to indicate it's a valid drop target (UXR-019).]
    - [Given file selection, When I use the file picker, Then I can select multiple files at once (UXR-022).]
    - [Given files are selected, When displayed, Then the file count and names are shown before upload.]

## Task Overview
Implement and harden the drag-and-drop document upload experience on Document Upload (SCR-005) so users can add files via drag/drop or the file picker, see a queued list (count + filenames) prior to upload, and get clear UI feedback during drag-over.

This task is UI-focused and should not introduce backend contract changes.

## Dependent Tasks
- [US_035] (Route protection / standard user access patterns)

## Impacted Components
- [MODIFY | app/src/pages/DocumentUploadPage.tsx | Ensure drag-over highlight behavior, queueing, multi-select file picker, and pre-upload queued list meet UX requirements and handle defined edge cases]

## Implementation Plan
- Confirm SCR-005 routing is present (`/documents/upload`) and uses the authenticated layout.
- Drag-and-drop queueing:
  - Ensure `onDragOver`, `onDragLeave`, and `onDrop` correctly manage a single “drag over dropzone” highlight state.
  - Ensure dropped files are added to an in-memory queue (not auto-uploaded) and the UI reflects the updated count and filenames.
- File picker multi-select:
  - Ensure the file input supports `multiple` and accepts `.pdf,.docx`.
  - Ensure selecting files appends them to the same queue used by drag/drop.
- Edge-case handling:
  - Dropped outside drop zone should not add files and should not break drag state.
  - Very long filenames should remain readable (truncate + provide a way to view full name, e.g., tooltip via `title`).
  - If drag-and-drop events are not supported/blocked, the file picker path should remain fully functional.

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | app/src/pages/DocumentUploadPage.tsx | Implement/adjust drag-over highlight behavior and ensure dropped/selected files are queued and displayed with count + names, including edge-case handling |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://developer.mozilla.org/en-US/docs/Web/API/HTML_Drag_and_Drop_API
- https://react.dev/learn/responding-to-events

## Build Commands
- npm --prefix .\\app run build
- npm --prefix .\\app run test

## Implementation Validation Strategy
- [Manual/UI] On `/documents/upload`, dragging files over the drop zone highlights it and removing drag resets the highlight.
- [Manual/UI] Dropping PDF/DOCX files onto the drop zone adds them to a queue and the “selected” count updates.
- [Manual/UI] Using “Select files” allows multi-select and adds files to the same queue.
- [Manual/UI] Long filenames are truncated but the full name remains accessible (e.g., via hover tooltip).

## Implementation Checklist
- [x] Verify route for SCR-005 is accessible for authenticated users
- [x] Ensure drag-over highlight state is correct and resets reliably
- [x] Ensure dropped files are queued (not auto-uploaded) and displayed
- [x] Ensure file picker supports multi-select and queues files
- [x] Add/confirm UI behavior for long filenames (truncate + full name access)
- [x] Validate dropped-outside-dropzone does not queue files

---

# Design Reference

## UI Impact Assessment
**Has UI Changes**: [x] Yes [ ] No

## User Story Design Context
**Story ID**: US-[042]
**Story Title**: Implement drag-and-drop document upload UI
**UI Impact Type**: UI Enhancement

### Design Source References
- **Design System**: `.propel/context/docs/designsystem.md`
- **Screen Spec**: `.propel/context/docs/figma_spec.md` (SCR-005, UXR-019, UXR-022)

### Screen-to-Design Mappings
| Screen/Feature | Image File | Image Path | Description | Implementation Priority |
|---------------|------------|------------|-------------|----------------------|
| Document Upload (SCR-005) | N/A | N/A | Drag-and-drop zone with highlight state, multi-select picker, and queued list preview | High |

### Component References
| Component Name | Reference Image | Code Location | UI Changes Required |
|---------------|-----------------|---------------|-------------------|
| Card | N/A | app/src/pages/DocumentUploadPage.tsx | Ensure upload section layout follows existing card patterns |
| Button | N/A | app/src/pages/DocumentUploadPage.tsx | Use existing button variants for actions (Select files / Upload / Clear) |
| Alert | N/A | app/src/pages/DocumentUploadPage.tsx | Display validation feedback consistently |

### Task Design Mapping
```yaml
TASK_042_001:
  title: "Drag-and-drop upload UI + queued preview"
  ui_impact: true
  visual_references:
    figma_spec_refs: ["SCR-005", "UXR-019", "UXR-022"]
  components_affected:
    - DocumentUploadPage
  visual_validation_required: false
```

### Accessibility Requirements
- **WCAG Level**: AA
- **Keyboard Navigation**: All actions (Select files, Clear, Upload, Remove item) operable via keyboard
- **Screen Reader**: Validation errors and queued list updates should be announced appropriately (use existing alert patterns)
