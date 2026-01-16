# Task - TASK_071_002

## Requirement Reference
- User Story: us_071
- Story Location: .propel/context/tasks/us_071/us_071.md
- Acceptance Criteria: 
    - Given an entity, When I hover over it, Then extraction rationale is displayed in a tooltip (FR-055).
    - Given the rationale, When displayed, Then it explains why this value was extracted from the source.
    - Given the tooltip, When shown, Then it includes the cited source text.
    - Given hover interaction, When triggered, Then response is within 200ms (UXR-017).

## Task Overview
Update the Patient 360 UI (SCR-008) to show extraction rationale and cited source text for extracted entities using a tooltip interaction.

This task focuses on:
- Rendering tooltip content from already-loaded Patient 360 data (no additional network calls on hover)
- Handling missing rationale and long rationale text gracefully
- Ensuring touch-device behavior works without hover

## Dependent Tasks
- [TASK_071_001] (Shared Tooltip UI primitive)
- [US_070 TASK_001] (Patient 360 loads entities/citations and renders citation metadata)

## Impacted Components
- [MODIFY | app/src/pages/Patient360Page.tsx | Wrap displayed entity/value UI with Tooltip showing rationale + cited source text]
- [MODIFY | app/src/lib/patient360Api.ts | Ensure Patient 360 response types include `rationale` and `sourceText` fields used by tooltip]

## Implementation Plan
- Identify the Patient 360 UI elements that represent “entities”:
  - For Phase 1, apply tooltip to the primary extracted value UI (e.g., entity value cell / displayed field) rather than to the entire row/card.
- Tooltip trigger behavior:
  - Desktop: show on hover.
  - Keyboard: show on focus (trigger must be focusable).
  - Touch: tap to toggle (provided by Tooltip component).
- Tooltip content composition:
  - Show a short header/label (e.g., “Why extracted”).
  - Body includes:
    - `rationale` (or a safe fallback message when missing)
    - `sourceText` (cited text) rendered in a visually distinct style (e.g., italic / quote), truncated if extremely long.
  - Ensure UI never renders raw HTML from the rationale/source text.
- Performance requirement (UXR-017):
  - Ensure tooltip content is sourced from in-memory state already present on the page.
  - Avoid async work on hover; no API calls in hover handlers.
- Edge cases:
  - Missing rationale: display “Rationale not available.”
  - Very long rationale/source text: truncate with an affordance to view full text (if needed, reuse existing modal pattern or keep tooltip scrollable with max height).

**Focus on how to implement**

## Current Project State
- `Patient360Page.tsx` exists and currently renders conflict/codes tables with citation metadata using stubbed data.
- US_070 tasks introduce API-backed Patient 360 data and citation rendering patterns.

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | app/src/pages/Patient360Page.tsx | Add rationale + source text tooltip behavior to displayed extracted entity/value UI |
| MODIFY | app/src/lib/patient360Api.ts | Extend/align types so tooltip has access to `rationale` + `sourceText` per entity |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://www.w3.org/WAI/ARIA/apg/patterns/tooltip/

## Build Commands
- npm --prefix .\app run build
- npm --prefix .\app run test
- npm --prefix .\app run test:e2e

## Implementation Validation Strategy
- [Manual/UI] Hover over an extracted entity/value and confirm rationale tooltip appears within 200ms.
- [Manual/UI] Confirm tooltip includes cited source text.
- [Edge Case] Confirm missing rationale shows a safe fallback message.
- [Edge Case] Confirm long rationale/source text does not break layout and remains readable.
- [Touch] On touch device/emulation, tap an entity/value to toggle the tooltip.
- [Accessibility] Confirm tooltip can be triggered via keyboard focus and content is associated via ARIA.

## Implementation Checklist
- [ ] Identify the exact entity/value UI elements to use as tooltip triggers in Patient 360
- [ ] Wrap trigger elements with `Tooltip` and render rationale + source text content
- [ ] Ensure no network calls occur during hover/focus (preloaded data only)
- [ ] Implement missing rationale fallback and long-text truncation/scroll behavior
- [ ] Validate mouse, keyboard, and touch behaviors
- [ ] Verify tooltip meets 200ms interaction feedback expectation

---

# Design Reference

## UI Impact Assessment
**Has UI Changes**: [x] Yes [ ] No

## User Story Design Context
**Story ID**: US-[071]
**Story Title**: Show extraction rationale on hover
**UI Impact Type**: UI Enhancement

### Design Source References
- **Design System**: `.propel/context/docs/designsystem.md`
- **Screen Spec**: `.propel/context/docs/figma_spec.md` (SCR-008, UXR-017, UXR-024)

### Screen-to-Design Mappings
| Screen/Feature | Image File | Image Path | Description | Implementation Priority |
|---------------|------------|------------|-------------|----------------------|
| Patient 360 View (SCR-008) | N/A | N/A | Tooltip on entity/value to show rationale + cited source text | High |

### Component References
| Component Name | Reference Image | Code Location | UI Changes Required |
|---------------|-----------------|---------------|-------------------|
| Tooltip | N/A | app/src/components/ui/Tooltip.tsx | Use for rationale/source text display |
| Patient360Page | N/A | app/src/pages/Patient360Page.tsx | Add tooltip triggers on extracted entity/value UI |

### Task Design Mapping
```yaml
TASK_071_002:
  title: "Patient 360: rationale tooltip on entity hover"
  ui_impact: true
  visual_references:
    figma_spec_refs: ["SCR-008", "UXR-017", "UXR-024", "FR-055"]
  components_affected:
    - Patient360Page
    - Tooltip
  visual_validation_required: false
```

### Accessibility Requirements
- **WCAG Level**: AA
- **Keyboard Navigation**: Tooltip triggers are focusable; tooltip can be revealed on focus
- **Focus States**: Visible focus on tooltip trigger
- **Screen Reader**: Tooltip content is announced via `aria-describedby`
