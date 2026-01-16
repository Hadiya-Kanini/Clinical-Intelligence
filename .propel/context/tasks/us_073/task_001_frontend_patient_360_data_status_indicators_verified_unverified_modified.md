# Task - TASK_073_001

## Requirement Reference
- User Story: us_073
- Story Location: .propel/context/tasks/us_073/us_073.md
- Acceptance Criteria: 
    - Given extracted data, When displayed, Then verified data shows green badge/indicator (UXR-025).
    - Given unverified data, When displayed, Then it shows yellow badge/indicator.
    - Given modified data, When displayed, Then it shows blue badge/indicator.
    - Given status indicators, When used, Then they are consistent with the design system.

## Task Overview
Implement a consistent, accessible visual indicator for extracted-data status in Patient 360 (SCR-008) so clinicians can quickly understand which fields are verified, unverified, or modified.

This task focuses on the UI layer only:
- A small reusable status indicator component that maps the three status states to design-system compliant badge variants.
- Wiring the status indicator into the extracted-data display on Patient 360 once API-backed entities are available.

## Dependent Tasks
- [US_070 TASK_070_001] (Patient 360 API wiring + UI patterns for entities/citations)
- [US_072 TASK_072_001] (Patient 360 verification split view layout)
- [US_074] (Inline editing provides/sets the “modified” status semantics)

## Impacted Components
- [CREATE | app/src/components/ui/DataStatusBadge.tsx | Reusable badge wrapper that renders Verified/Unverified/Modified using design-system aligned colors and accessible labels]
- [MODIFY | app/src/pages/Patient360Page.tsx | Render a status indicator next to extracted data values (where entities/fields are displayed)]
- [MODIFY | app/src/lib/patient360Api.ts | Align Patient 360 response types to include the status flag used by the UI (e.g., `dataStatus` or equivalent)]

## Implementation Plan
- Define a small status model:
  - Add a `DataStatus` union type with three allowed values: `verified`, `unverified`, `modified`.
  - Define a single mapping from `DataStatus` -> `Badge` variant:
    - `verified` -> `success` (green)
    - `unverified` -> `warning` (yellow)
    - `modified` -> `info` (blue)
- Implement `DataStatusBadge`:
  - Render human-readable label text (e.g., “Verified”, “Unverified”, “Modified”) so meaning is not conveyed by color alone.
  - Ensure the element is accessible:
    - Provide an `aria-label` and/or ensure the label text is present.
    - Use `title` to provide a simple hover hint.
- Wire into Patient 360 extracted-data UI:
  - Identify the extracted-data value UI surface (cards/rows/fields) and add the badge next to the value (or field label) without crowding.
  - Define precedence rules for edge cases where multiple flags exist:
    - Default: `modified` takes precedence over `verified` (since it needs re-review).
    - `unverified` is the fallback when no explicit status is provided.
- Handle status changes during viewing:
  - Ensure the displayed status badge is derived from current in-memory state so it updates immediately when upstream state changes (e.g., after verify action or inline edit).

**Focus on how to implement**

## Current Project State
- `app/src/components/ui/Badge.tsx` exists with variants `success`, `warning`, `info`, etc., and CSS tokens defined in `app/src/styles/base.css`.
- `Patient360Page.tsx` is present but still largely stubbed and does not yet render API-backed extracted entities.

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | app/src/components/ui/DataStatusBadge.tsx | UI component that standardizes verified/unverified/modified indicators using existing `Badge` variants |
| MODIFY | app/src/pages/Patient360Page.tsx | Display `DataStatusBadge` alongside extracted data values once entities are rendered |
| MODIFY | app/src/lib/patient360Api.ts | Extend Patient 360 response typing to expose a status flag usable by `DataStatusBadge` |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://www.w3.org/WAI/WCAG21/Understanding/use-of-color.html

## Build Commands
- npm --prefix .\app run build
- npm --prefix .\app run test

## Implementation Validation Strategy
- [Manual/UI] Load Patient 360 (SCR-008) and confirm:
  - Verified fields display a green indicator with a “Verified” label.
  - Unverified fields display a yellow indicator with an “Unverified” label.
  - Modified fields display a blue indicator with a “Modified” label.
- [Accessibility] Confirm the status meaning is available without relying on color alone (label text present) and is readable by screen readers.
- [Edge Case] When an item transitions to “modified” (via inline edit), confirm the indicator updates immediately.

## Implementation Checklist
- [ ] Define `DataStatus` type and mapping to existing badge variants (success/warning/info)
- [ ] Create `DataStatusBadge` component with accessible label text
- [ ] Align Patient 360 response typing to include data status flag
- [ ] Render `DataStatusBadge` in Patient 360 extracted data section(s)
- [ ] Implement precedence rules for multi-flag situations (modified > unverified > verified)
- [ ] Validate color, spacing, and typography align to design tokens
- [ ] Manually verify keyboard/screen reader semantics

---

# Design Reference

## UI Impact Assessment
**Has UI Changes**: [x] Yes [ ] No

## User Story Design Context
**Story ID**: US-[073]
**Story Title**: Visually distinguish verified, unverified, and modified data
**UI Impact Type**: UI Enhancement

### Design Source References
- **Design System**: `.propel/context/docs/designsystem.md`
- **Screen Spec**: `.propel/context/docs/figma_spec.md` (SCR-008, UXR-025)

### Screen-to-Design Mappings
| Screen/Feature | Image File | Image Path | Description | Implementation Priority |
|---------------|------------|------------|-------------|----------------------|
| Patient 360 View (SCR-008) | N/A | N/A | Status indicators on extracted data values: green=verified, yellow=unverified, blue=modified | High |

### Component References
| Component Name | Reference Image | Code Location | UI Changes Required |
|---------------|-----------------|---------------|-------------------|
| Badge | N/A | app/src/components/ui/Badge.tsx | Reuse variants for verified/unverified/modified data indicators |
| Patient360Page | N/A | app/src/pages/Patient360Page.tsx | Add data status badges near extracted field values |

### Task Design Mapping
```yaml
TASK_073_001:
  title: "Patient 360: data status indicators (verified/unverified/modified)"
  ui_impact: true
  visual_references:
    figma_spec_refs: ["SCR-008", "UXR-025"]
  components_affected:
    - Patient360Page
    - DataStatusBadge
  visual_validation_required: false
```

### Accessibility Requirements
- **WCAG Level**: AA
- **Color Contrast**: Ensure badge text meets contrast requirements against its background
- **Colorblind Support**: Meaning must not rely on color alone (badge text label required)
- **Screen Reader**: Badge label is announced and unambiguous
