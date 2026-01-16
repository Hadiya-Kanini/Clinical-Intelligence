# Task - TASK_072_001

## Requirement Reference
- User Story: us_072
- Story Location: .propel/context/tasks/us_072/us_072.md
- Acceptance Criteria: 
    - Given Patient 360 view (SCR-008), When displayed, Then source PDF is on left and extracted data on right (UXR-023).
    - Given data categories, When displayed, Then they have clear visual hierarchy with grouped sections (UXR-027).
    - Given the layout, When on desktop, Then it adapts to viewport width (1280px+).

## Task Overview
Implement the baseline side-by-side verification layout for Patient 360 (SCR-008) so clinicians can review the source document and extracted data in a single split view.

This task focuses on:
- Establishing a robust two-pane layout (PDF left, extracted data right)
- Ensuring the right pane renders extracted data in grouped sections with clear hierarchy
- Handling wide/narrow desktop viewports (1280px+ target) without horizontal overflow

## Dependent Tasks
- [US_070 TASK_070_002] (Frontend source viewer component used in left pane)
- [US_070 TASK_070_001] (Patient 360 API wiring and citation rendering patterns)

## Impacted Components
- [MODIFY | app/src/pages/Patient360Page.tsx | Replace the current fixed 2-column grid with a dedicated split-view layout; ensure left pane hosts the source viewer and right pane hosts grouped extracted data sections]
- [CREATE | app/src/components/layout/VerificationSplitView.tsx | Layout component responsible for left/right panes, min widths, and responsive behavior]

## Implementation Plan
- Define the split view contract:
  - Left pane: source document area (use `SourceDocumentViewer` from US_070 once available; keep a safe fallback placeholder if viewer errors).
  - Right pane: extracted data + review UI grouped into sections.
- Implement layout behavior:
  - Desktop baseline uses a two-column layout with a stable gap and min widths.
  - Ensure the PDF pane has a bounded height and does not force horizontal scroll.
  - For narrower widths (still within desktop), allow the right pane to shrink first; enforce minimum width constraints to avoid unreadable content.
- Implement data hierarchy grouping on the right pane:
  - Convert the current "Review" area into a grouped structure (e.g., separate Cards/sections for patient summary, conflicts, codes).
  - Ensure headings and spacing align to existing design tokens and maintain consistent typography.
- Error handling:
  - If the viewer cannot load, show an `Alert` in the left pane and keep the right pane usable.

**Focus on how to implement**

## Current Project State
- `Patient360Page.tsx` currently renders a simple 1fr/1fr grid with a static "PDF Viewer Placeholder".
- The right pane mixes profile and review content but is not structured specifically for verification grouping.

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | app/src/components/layout/VerificationSplitView.tsx | Split-view layout wrapper for Patient 360 verification (left PDF pane, right extracted data pane) with responsive min-width behavior |
| MODIFY | app/src/pages/Patient360Page.tsx | Adopt `VerificationSplitView`; ensure extracted data renders as grouped sections on the right |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://react.dev/learn

## Build Commands
- npm --prefix .\app run build
- npm --prefix .\app run test
- npm --prefix .\app run test:e2e

## Implementation Validation Strategy
- [Manual/UI] Load `/patients/:patientId` and confirm the page displays a split view with the source document on the left and extracted data on the right.
- [Manual/UI] Resize viewport across 1280px, 1440px, and 1920px widths and confirm the layout adapts without horizontal scroll.
- [Edge Case] Simulate viewer failure and confirm an error state appears in the left pane while the right pane remains usable.

## Implementation Checklist
- [ ] Create `VerificationSplitView` layout component and define left/right pane structure
- [ ] Update `Patient360Page` to use `VerificationSplitView` and keep existing content functional
- [ ] Refactor right pane into grouped sections with clear headings and spacing
- [ ] Ensure responsive behavior at 1280px+ (no overflow, readable min widths)
- [ ] Add safe left-pane error state when viewer cannot load

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
- **Screen Spec**: `.propel/context/docs/figma_spec.md` (SCR-008, UXR-023, UXR-027)

### Screen-to-Design Mappings
| Screen/Feature | Image File | Image Path | Description | Implementation Priority |
|---------------|------------|------------|-------------|----------------------|
| Patient 360 View (SCR-008) | N/A | N/A | Side-by-side verification view layout with PDF left and extracted data right | High |

### Component References
| Component Name | Reference Image | Code Location | UI Changes Required |
|---------------|-----------------|---------------|-------------------|
| Card | N/A | app/src/pages/Patient360Page.tsx | Use cards/sections to create clear hierarchy of extracted data |
| Alert | N/A | app/src/components/ui/Alert | Display safe errors when viewer fails |

### Task Design Mapping
```yaml
TASK_072_001:
  title: "Patient 360: split view layout (PDF left, extracted data right)"
  ui_impact: true
  visual_references:
    figma_spec_refs: ["SCR-008", "UXR-023", "UXR-027"]
  components_affected:
    - Patient360Page
    - VerificationSplitView
  visual_validation_required: false
```

### Accessibility Requirements
- **WCAG Level**: AA
- **Keyboard Navigation**: All interactive controls on right pane remain reachable via Tab
- **Focus States**: Visible focus for interactive controls
