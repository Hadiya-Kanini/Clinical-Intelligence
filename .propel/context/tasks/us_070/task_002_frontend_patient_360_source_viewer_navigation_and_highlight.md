# Task - TASK_070_002

## Requirement Reference
- User Story: us_070
- Story Location: .propel/context/tasks/us_070/us_070.md
- Acceptance Criteria: 
    - Given a reference click, When triggered, Then the source document section is highlighted (UXR-024).
    - Given source location, When available, Then exact coordinates enable precise highlighting.

## Task Overview
Implement an in-page source document viewer capability for the Patient 360 view so the UI can navigate to a cited location and visually highlight the cited content when a user clicks a reference.

This task focuses on:
- Document rendering (viewing) within the Patient 360 split-pane
- Navigating to a cited page
- Highlighting cited content based on coordinates when available, with a graceful fallback when coordinates are missing

## Dependent Tasks
- [TASK_070_003] (Backend document content endpoint to fetch document bytes securely)
- [TASK_070_001] (Citation UI emits selected citation/navigation intent)

## Impacted Components
- [CREATE | app/src/components/documents/SourceDocumentViewer.tsx | Document viewer component that supports page navigation and highlight overlay]
- [CREATE | app/src/lib/documentsApi.ts | Thin API wrapper to fetch document content from backend]
- [MODIFY | app/src/pages/Patient360Page.tsx | Replace PDF placeholder with `SourceDocumentViewer` and wire `activeCitation`]
- [MODIFY | app/package.json | Add PDF rendering dependency if required by viewer implementation]

## Implementation Plan
- Choose a document rendering approach suitable for the current app stack:
  - Prefer a React-compatible PDF renderer (e.g., PDF.js via a React wrapper) to allow page navigation and an overlay highlight.
  - Keep the implementation scoped to Patient 360 (SCR-008) only.
- API wiring:
  - Add `documentsApi.getDocumentContent(documentId)` to fetch bytes/stream from the backend endpoint introduced in TASK_070_003.
  - Use `Blob` URLs for rendering in the browser.
- Viewer behavior:
  - Render the current document for the selected citation.
  - Navigate to `pageNumber` on citation click (if provided).
  - Highlight:
    - If `coordinates` is present and parseable, render a highlight overlay on the target page.
    - If `coordinates` is missing/unparseable, fall back to a page-level highlight cue (e.g., highlight banner or focusing the page).
- Error handling and edge cases:
  - If document content is unavailable (404/410), show a safe error state in the viewer panel and keep the rest of Patient 360 usable.
  - Ensure object URLs are revoked to avoid memory leaks.

**Focus on how to implement**

## Current Project State
- `Patient360Page` shows a static "PDF Viewer Placeholder" and does not render a real document.
- The frontend currently does not include a PDF rendering library.

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | app/src/components/documents/SourceDocumentViewer.tsx | Viewer that loads document content and supports citation-driven navigation + highlight |
| CREATE | app/src/lib/documentsApi.ts | API wrapper for backend document content retrieval |
| MODIFY | app/src/pages/Patient360Page.tsx | Replace placeholder with viewer and wire `activeCitation` updates |
| MODIFY | app/package.json | Add dependency needed for PDF rendering / highlighting |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://mozilla.github.io/pdf.js/

## Build Commands
- npm --prefix .\app run build
- npm --prefix .\app run test
- npm --prefix .\app run test:e2e

## Implementation Validation Strategy
- [Manual/UI] Click a citation and confirm the viewer opens the correct document and navigates to the cited page.
- [Manual/UI] When coordinates are available, confirm a highlight overlay is shown in the viewer.
- [Manual/UI] When coordinates are missing, confirm the viewer still navigates to the cited page and shows a clear fallback indicator.
- [Performance] Confirm repeated citation clicks do not leak memory (Blob URLs revoked) and the page remains responsive.

## Implementation Checklist
- [ ] Select and add a PDF rendering approach/library compatible with the app
- [ ] Implement `documentsApi` for fetching document content as a Blob
- [ ] Build `SourceDocumentViewer` with page navigation and highlight overlay support
- [ ] Wire `Patient360Page` to pass selected citation into viewer
- [ ] Implement fallback behavior for missing coordinates and missing documents

---

# Design Reference

## UI Impact Assessment
**Has UI Changes**: [x] Yes [ ] No

## User Story Design Context
**Story ID**: US-[070]
**Story Title**: Display source metadata and clickable references
**UI Impact Type**: UI Enhancement

### Design Source References
- **Design System**: `.propel/context/docs/designsystem.md`
- **Screen Spec**: `.propel/context/docs/figma_spec.md` (SCR-008, UXR-024)

### Screen-to-Design Mappings
| Screen/Feature | Image File | Image Path | Description | Implementation Priority |
|---------------|------------|------------|-------------|----------------------|
| Patient 360 View - Source Viewer | N/A | N/A | Left panel renders source document and highlights cited content on click | High |

### Component References
| Component Name | Reference Image | Code Location | UI Changes Required |
|---------------|-----------------|---------------|-------------------|
| Card | N/A | app/src/pages/Patient360Page.tsx | Left panel should remain readable at 1280px+ and not overflow |
| Alert | N/A | app/src/components/documents/SourceDocumentViewer.tsx | Safe error states when document cannot be loaded |

### Task Design Mapping
```yaml
TASK_070_002:
  title: "Patient 360: source viewer navigation and highlight"
  ui_impact: true
  visual_references:
    figma_spec_refs: ["SCR-008", "UXR-024"]
  components_affected:
    - Patient360Page
    - SourceDocumentViewer
  visual_validation_required: false
```

### Accessibility Requirements
- **WCAG Level**: AA
- **Keyboard Navigation**: Viewer controls (if present) are keyboard accessible
- **Focus States**: Visible focus for interactive viewer controls
