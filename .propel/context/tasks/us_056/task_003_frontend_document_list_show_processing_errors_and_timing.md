# Task - TASK_056_003

## Requirement Reference
- User Story: us_056
- Story Location: .propel/context/tasks/us_056/us_056.md
- Acceptance Criteria: 
    - Given processing fails, When error occurs, Then error message is captured and displayed to user (FR-028).
    - Given the document list, When a document has failed, Then the error message is visible.
    - Given processing metadata, When stored, Then it includes start time, completion time, and duration.

## Task Overview
Wire the Document List UI to backend data and surface processing errors and timing information so standard users can understand what happened to their documents.

This task focuses on:
- Replacing the current hardcoded `DocumentListPage` rows with an API-backed list
- Displaying error message for failed documents
- Displaying timing metadata (duration and timestamps) in a readable, non-intrusive way

## Dependent Tasks
- [TASK_056_002] (Backend documents list API includes processing metadata)

## Impacted Components
- [MODIFY | app/src/pages/DocumentListPage.tsx | Replace hardcoded rows with API-backed data; display error + timing metadata]
- [CREATE | app/src/lib/documentsApi.ts | Thin API wrapper around `GET /api/v1/documents` using existing `apiClient`]

## Implementation Plan
- Add an API wrapper:
  - Create `documentsApi` wrapper that calls `api.get` against `GET /api/v1/documents`.
  - Add TypeScript types aligned to the backend response contract (document fields + processing metadata).
- Page state and lifecycle:
  - Track loading/error states.
  - Store `items` array returned from backend.
  - Keep existing local filter (search) functionality, but apply it on the returned items (until server-side search is introduced).
- Rendering requirements:
  - Maintain status badge behavior.
  - For failed documents:
    - Render `errorMessage` inline (e.g., under the document name or in a dedicated “Error” column).
    - Ensure long error messages do not break layout (truncate with a “show more” affordance if needed).
  - For timing metadata:
    - Show `processingTimeMs` (formatted, e.g., seconds) and/or show `startedAt`/`completedAt` timestamps in a secondary line.
- UX and safety:
  - If API returns an error, display a safe message in `Alert`.
  - Do not display raw structured error details (only `errorMessage`).

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | app/src/lib/documentsApi.ts | API wrapper for `GET /api/v1/documents` with typed response including processing metadata |
| MODIFY | app/src/pages/DocumentListPage.tsx | Load documents from backend and display processing error messages + timing metadata |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://react.dev/reference/react/useEffect

## Build Commands
- npm --prefix .\app run build
- npm --prefix .\app run test
- npm --prefix .\app run test:e2e

## Implementation Validation Strategy
- [Manual/UI] Load Document List and confirm rows render from backend response.
- [Manual/UI] For a failed document, confirm the error message is visible on the list.
- [Manual/UI] For completed/failed documents, confirm timing metadata (duration and timestamps) is visible and formatted.
- [Accessibility] Confirm error message/timing text does not interfere with keyboard navigation and table semantics.

## Implementation Checklist
- [ ] Create `documentsApi` wrapper for `GET /api/v1/documents` with typed response
- [ ] Replace hardcoded rows in `DocumentListPage` with API-backed list + loading/error states
- [ ] Display `errorMessage` for failed documents in the list
- [ ] Display `processingTimeMs` and/or `startedAt`/`completedAt` timing metadata in the list
- [ ] Handle long error messages without breaking table layout

---

# Design Reference

## UI Impact Assessment
**Has UI Changes**: [x] Yes [ ] No

## User Story Design Context
**Story ID**: US-[056]
**Story Title**: Track processing metadata and surface errors
**UI Impact Type**: UI Enhancement

### Design Source References
- **Design System**: `.propel/context/docs/designsystem.md`
- **Screen Spec**: `.propel/context/docs/figma_spec.md`

### Screen-to-Design Mappings
| Screen/Feature | Image File | Image Path | Description | Implementation Priority |
|---------------|------------|------------|-------------|----------------------|
| Document List | N/A | N/A | Show status + error message for failed documents + timing metadata in list rows | High |

### Component References
| Component Name | Reference Image | Code Location | UI Changes Required |
|---------------|-----------------|---------------|-------------------|
| Card | N/A | app/src/pages/DocumentListPage.tsx | Keep headerRight search/clear layout consistent with existing pages |
| Table | N/A | app/src/pages/DocumentListPage.tsx | Add metadata/error presentation while keeping table readable |
| Badge | N/A | app/src/pages/DocumentListPage.tsx | Maintain status variant mapping |
| Alert | N/A | app/src/pages/DocumentListPage.tsx | Display API error state safely |

### Task Design Mapping
```yaml
TASK_056_003:
  title: "Document list: show processing errors and timing"
  ui_impact: true
  visual_references:
    figma_spec_refs: []
  components_affected:
    - DocumentListPage
  visual_validation_required: false
```

### Accessibility Requirements
- **WCAG Level**: AA
- **Keyboard Navigation**: Search input and table actions remain keyboard accessible
- **Screen Reader**: Error messages should be included in readable text content (no icon-only states)
