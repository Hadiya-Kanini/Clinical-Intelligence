# Task - TASK_070_001

## Requirement Reference
- User Story: us_070
- Story Location: .propel/context/tasks/us_070/us_070.md
- Acceptance Criteria: 
    - Given an entity, When displayed, Then source document name, page number, and section are shown (FR-053).
    - Given critical entities (diagnoses, procedures, medications), When displayed, Then clickable reference links navigate to source (FR-054).

## Task Overview
Update the Patient 360 UI (SCR-008) so that source metadata is consistently shown alongside displayed clinical items and code suggestions, and citations are rendered as clickable references.

This task focuses on the UI layer behavior and wiring:
- Render `documentName`, `pageNumber`, and `section` for each displayed citation
- Make document references clickable and route the click event into the in-page source viewer (implemented in dependent task) so users can navigate to the cited location.

## Dependent Tasks
- [US_069 TASK_004] (Backend Patient 360 API includes grounded entities + citations)
- [TASK_070_002] (Frontend source document viewer supports navigation/highlight)

## Impacted Components
- [MODIFY | app/src/pages/Patient360Page.tsx | Render citation metadata for critical items and make citation references clickable]
- [CREATE | app/src/lib/patient360Api.ts | Thin API wrapper around `GET /api/v1/patients/{patientId}/360` using existing `apiClient`]

## Implementation Plan
- Define and standardize citation UI presentation:
  - Display `documentName` as the primary clickable element.
  - Display secondary metadata: `Page <n>` and `section` when present.
  - Ensure long `sourceText` is truncated consistently (existing behavior in codes table can be reused).
- Introduce a click contract between the citations UI and the document viewer:
  - Add a `handleCitationClick(citation)` handler in `Patient360Page`.
  - Store selected citation in state (e.g., `activeCitation`).
  - Pass `activeCitation` to the document viewer component (implemented in TASK_070_002).
- Replace stubbed `conflicts` / `codes` data with API-backed state:
  - Add `patient360Api.getPatient360(patientId)` returning conflicts/codes/entities including citations.
  - Add loading/error states consistent with other pages.
- Accessibility and UX:
  - Ensure clickable citation uses a keyboard-accessible element (`<button>` or `<a>`).
  - Provide safe messaging if the viewer cannot navigate (e.g., missing page/coordinates).

**Focus on how to implement**

## Current Project State
- `Patient360Page.tsx` contains stubbed in-memory citations and a PDF viewer placeholder.
- `apiClient` exists but Patient 360 API wiring is not present in the UI.

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | app/src/lib/patient360Api.ts | API wrapper and types for Patient 360 payload including citations used by the UI |
| MODIFY | app/src/pages/Patient360Page.tsx | Replace stubbed data with API-backed model; render citation metadata and clickable links |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://react.dev/reference/react/useEffect
- https://reactrouter.com/en/main/hooks/use-params

## Build Commands
- npm --prefix .\app run build
- npm --prefix .\app run test
- npm --prefix .\app run test:e2e

## Implementation Validation Strategy
- [Manual/UI] Load Patient 360 page and confirm citation metadata (document name, page number, section) is visible for critical items.
- [Manual/UI] Click a document reference and confirm the UI triggers navigation intent (state updates / viewer receives citation).
- [Accessibility] Confirm citation links are reachable via Tab and triggerable via Enter/Space.

## Implementation Checklist
- [ ] Create `patient360Api` wrapper for `GET /api/v1/patients/{patientId}/360` with typed response
- [ ] Replace stubbed `conflicts` / `codes` state with API-backed data and loading/error handling
- [ ] Render citation metadata (document name, page number, section) for critical entities
- [ ] Make citation references clickable and emit `activeCitation` state for the viewer
- [ ] Handle missing citation fields gracefully (no section/coordinates)

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
| Patient 360 View (SCR-008) | N/A | N/A | Show citation metadata and clickable references for critical items | High |

### Component References
| Component Name | Reference Image | Code Location | UI Changes Required |
|---------------|-----------------|---------------|-------------------|
| Card | N/A | app/src/pages/Patient360Page.tsx | Keep split layout; ensure citation metadata fits without crowding |
| Table | N/A | app/src/pages/Patient360Page.tsx | Make citation references clickable without breaking table layout |
| Modal | N/A | app/src/pages/Patient360Page.tsx | Ensure conflict resolution modal shows citation metadata consistently |
| Badge | N/A | app/src/pages/Patient360Page.tsx | Maintain status indicators; avoid new colors outside tokens |

### Task Design Mapping
```yaml
TASK_070_001:
  title: "Patient 360: show source metadata and clickable references"
  ui_impact: true
  visual_references:
    figma_spec_refs: ["SCR-008", "UXR-024", "FR-053", "FR-054"]
  components_affected:
    - Patient360Page
  visual_validation_required: false
```

### Accessibility Requirements
- **WCAG Level**: AA
- **Keyboard Navigation**: Citation links reachable via Tab; conflict modal controls remain accessible
- **Screen Reader**: Citation link text includes document name and page context
