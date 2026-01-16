# Task - TASK_072_002

## Requirement Reference
- User Story: us_072
- Story Location: .propel/context/tasks/us_072/us_072.md
- Acceptance Criteria: 
    - Given the split view, When scrolling, Then synchronized scrolling is available.
    - What happens when the PDF viewer fails to load?
    - How does the system handle documents with many pages?

## Task Overview
Add synchronized scrolling support between the left source document viewer and the right extracted-data pane on Patient 360 (SCR-008) to improve verification efficiency.

This task focuses on:
- Providing a user-visible synchronization mode (opt-in) so scrolling either pane scrolls the other
- Ensuring behavior remains stable for long multi-page documents
- Ensuring safe fallback behavior when the PDF viewer fails to load

## Dependent Tasks
- [US_070 TASK_070_002] (SourceDocumentViewer component renders the PDF and provides a scrollable container)
- [US_072 TASK_072_001] (Split view layout exists with stable pane containers)

## Impacted Components
- [MODIFY | app/src/pages/Patient360Page.tsx | Add sync-scroll state/control and wire scroll containers from left viewer and right extracted-data pane]
- [MODIFY | app/src/components/layout/VerificationSplitView.tsx | Expose refs or callbacks to access scroll containers for both panes]

## Implementation Plan
- Define sync-scroll UX:
  - Add a simple toggle control (e.g., "Sync scrolling") in the Patient 360 header area (right pane) so users can enable/disable synchronization.
  - Default to OFF to reduce surprise behavior; persist only for the current session unless there is an existing preference mechanism.
- Implement scroll syncing mechanics:
  - Identify the scroll containers:
    - Left: the scrollable element that contains the rendered PDF pages.
    - Right: the scrollable element containing extracted data sections.
  - Use `ref` access to each scroll container and attach `scroll` listeners.
  - Synchronize scroll positions using normalized ratios:
    - `ratio = scrollTop / (scrollHeight - clientHeight)`
    - Apply ratio to the other container to compute target `scrollTop`.
  - Guard against event feedback loops:
    - Track an "active sync source" flag or use a short-lived lock while programmatically setting the other scroll position.
    - Use `requestAnimationFrame` to batch updates and keep UI responsive.
- Long document handling:
  - Ensure ratio math handles very tall scroll heights (multi-page PDFs) and does not jitter.
  - If the left viewer uses virtualization or lazy rendering, implement a best-effort approach (still sync ratios; accept imperfect alignment).
- PDF viewer failure handling:
  - If the left viewer is in an error state, disable the sync toggle and show a short explanation.
  - Ensure scroll syncing code safely no-ops when either scroll container ref is missing.

**Focus on how to implement**

## Current Project State
- `Patient360Page.tsx` currently renders a left "PDF Viewer Placeholder" div and a right pane with cards/tables.
- US_070 tasks introduce a real `SourceDocumentViewer` and API wiring.

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | app/src/pages/Patient360Page.tsx | Add a sync-scroll toggle and implement scroll synchronization wiring between left and right pane containers |
| MODIFY | app/src/components/layout/VerificationSplitView.tsx | Provide access to left/right scroll containers (refs or callback props) so Patient360 can attach scroll sync logic |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://react.dev/reference/react/useEffect

## Build Commands
- npm --prefix .\app run build
- npm --prefix .\app run test
- npm --prefix .\app run test:e2e

## Implementation Validation Strategy
- [Manual/UI] Enable "Sync scrolling" and confirm scrolling either pane scrolls the other.
- [Manual/UI] Disable "Sync scrolling" and confirm panes scroll independently.
- [Edge Case] With a long document (many pages), confirm sync remains responsive and does not jitter excessively.
- [Edge Case] Simulate viewer failure and confirm sync is disabled and the right pane remains usable.

## Implementation Checklist
- [ ] Add sync-scroll toggle control and local state in Patient 360
- [ ] Expose left/right scroll containers via `VerificationSplitView` so Patient 360 can wire scroll events
- [ ] Implement normalized ratio-based scroll syncing with feedback-loop guards
- [ ] Handle missing refs and viewer error state gracefully (no crashes)
- [ ] Validate behavior with long document scenarios

---

# Design Reference

## UI Impact Assessment
**Has UI Changes**: [x] Yes [ ] No

## User Story Design Context
**Story ID**: US-[072]
**Story Title**: Implement side-by-side verification view
**UI Impact Type**: UI Enhancement

### Design Source References
- **Design System**: `.propel/context/docs/designsystem.md`
- **Screen Spec**: `.propel/context/docs/figma_spec.md` (SCR-008, UXR-023)

### Screen-to-Design Mappings
| Screen/Feature | Image File | Image Path | Description | Implementation Priority |
|---------------|------------|------------|-------------|----------------------|
| Patient 360 View (SCR-008) | N/A | N/A | Provide synchronized scrolling option for split-pane verification view | High |

### Task Design Mapping
```yaml
TASK_072_002:
  title: "Patient 360: synchronized scrolling in verification split view"
  ui_impact: true
  visual_references:
    figma_spec_refs: ["SCR-008", "UXR-023"]
  components_affected:
    - Patient360Page
    - VerificationSplitView
  visual_validation_required: false
```

### Accessibility Requirements
- **WCAG Level**: AA
- **Keyboard Navigation**: Sync toggle reachable via keyboard
- **Screen Reader**: Toggle has an accessible label (e.g., "Synchronized scrolling")
