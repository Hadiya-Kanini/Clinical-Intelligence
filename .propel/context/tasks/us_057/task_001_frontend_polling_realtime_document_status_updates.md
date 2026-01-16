# Task - TASK_057_001

## Requirement Reference
- User Story: us_057
- Story Location: .propel/context/tasks/us_057/us_057.md
- Acceptance Criteria: 
    - Given documents are processing, When status changes, Then the UI updates automatically (FR-026).
    - Given real-time updates, When implemented, Then they refresh at least every 5 seconds (UXR-043).
    - Given the document list, When a document completes, Then the status badge updates without page reload.

## Task Overview
Implement real-time (polling-based) status updates on the Document List screen so that in-progress document processing states refresh automatically without requiring the user to reload the page.

This task will add a 5-second polling loop that refreshes document data via the existing backend document list endpoint and updates the status badge in-place.

Estimated Effort: 6 hours

## Dependent Tasks
- [US_051 - Track document processing status] (status persistence and display in list)
- [TASK_056_002] (Backend documents list API includes processing metadata)
- [TASK_056_003] (Frontend document list shows processing errors and timing)

## Impacted Components
- [MODIFY | app/src/pages/DocumentListPage.tsx | Add polling-based refresh (5s) and update rows/status badges without page reload]
- [CREATE | app/src/hooks/useDocumentListPolling.ts | Encapsulate polling loop + cleanup and expose refreshed list + status]

## Implementation Plan
- Add a reusable polling hook:
  - Create `useDocumentListPolling` that:
    - Accepts a `pollIntervalMs` (default 5000) and an `enabled` flag
    - Calls existing `documentsApi` (from US_056) to fetch the latest document list
    - Uses `setInterval` inside `useEffect` with proper cleanup to avoid leaks
    - Avoids overlapping requests (skip tick if prior request is still in flight)
- Wire polling into `DocumentListPage`:
  - Replace any one-time load logic with `useDocumentListPolling`
  - Ensure status badge and row state render from the latest API data
  - Keep the existing search/filter behavior applied to the latest `items`
- Performance guardrails:
  - Only enable polling when there is at least one document in `Pending` or `Processing` status
  - Stop polling automatically when all documents are terminal (`Completed`/`Failed`)

**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | app/src/hooks/useDocumentListPolling.ts | Hook that polls the documents endpoint every 5 seconds (with cleanup + no-overlap) and returns refreshed items and error state |
| MODIFY | app/src/pages/DocumentListPage.tsx | Use polling hook to refresh document list and update status badges automatically without reload |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://react.dev/reference/react/useEffect
- https://developer.mozilla.org/en-US/docs/Web/API/setInterval

## Build Commands
- npm --prefix .\app run build
- npm --prefix .\app run test

## Implementation Validation Strategy
- [Manual/UI] Open Document List with at least one `Processing` document and confirm the status badge updates within 5 seconds of the backend status changing.
- [Manual/UI] Confirm polling stops once all documents reach `Completed`/`Failed`.
- [Manual/UI] Confirm filtering/search continues to work while polling is active.

## Implementation Checklist
- [ ] Create `useDocumentListPolling` hook with 5s interval and effect cleanup
- [ ] Ensure polling does not overlap requests (single in-flight request guard)
- [ ] Enable polling only when any document is `Pending`/`Processing`
- [ ] Wire `DocumentListPage` to render from the latest polled API data
- [ ] Verify status badge updates in-place without page reload
- [ ] Verify polling stops when all documents are terminal

---

# Design Reference

## UI Impact Assessment
**Has UI Changes**: [x] Yes [ ] No

## User Story Design Context
**Story ID**: US-[057]
**Story Title**: Provide real-time status updates during processing
**UI Impact Type**: UI Enhancement

### Design Source References
- **Design System**: `.propel/context/docs/designsystem.md`
- **Screen Spec**: `.propel/context/docs/figma_spec.md` (SCR-006, UXR-043)

### Screen-to-Design Mappings
| Screen/Feature | Image File | Image Path | Description | Implementation Priority |
|---------------|------------|------------|-------------|----------------------|
| Document List | N/A | N/A | Auto-refresh status badge every 5 seconds while processing; keep UI stable and readable | High |

### Component References
| Component Name | Reference Image | Code Location | UI Changes Required |
|---------------|-----------------|---------------|-------------------|
| DocumentListPage | N/A | app/src/pages/DocumentListPage.tsx | Add auto-refresh behavior while preserving current layout and status badge variants |
| Badge | N/A | app/src/pages/DocumentListPage.tsx | Ensure status text/variant updates from fresh data without reloading |

### Task Design Mapping
```yaml
TASK_057_001:
  title: "Document list: polling-based real-time status updates"
  ui_impact: true
  visual_references:
    figma_spec_refs: ["SCR-006", "UXR-043"]
  components_affected:
    - DocumentListPage
  visual_validation_required: false
```

### Accessibility Requirements
- **WCAG Level**: AA
- **Screen Reader**: Status text must remain available as plain text within the table cell
- **Keyboard Navigation**: Polling must not steal focus or interrupt keyboard interaction
