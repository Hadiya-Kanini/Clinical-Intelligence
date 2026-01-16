# Task - TASK_071_001

## Requirement Reference
- User Story: us_071
- Story Location: .propel/context/tasks/us_071/us_071.md
- Acceptance Criteria: 
    - Given an entity, When I hover over it, Then extraction rationale is displayed in a tooltip (FR-055).
    - Given hover interaction, When triggered, Then response is within 200ms (UXR-017).

## Task Overview
Introduce a reusable, accessible tooltip UI primitive for the web app that can be used to display extraction rationale and cited source text on hover/focus, with a touch-friendly fallback (tap to toggle).

This task focuses on the shared UI component only (not Patient 360 page wiring).

## Dependent Tasks
- [US_070 TASK_001] (Patient 360 renders citation metadata in the UI)

## Impacted Components
- [CREATE | app/src/components/ui/Tooltip.tsx | Accessible tooltip primitive supporting hover/focus and touch fallback]

## Implementation Plan
- Implement a minimal tooltip component API suitable for Patient 360 usage:
  - Accept `content` (tooltip body) and `children` (trigger element).
  - Support interaction triggers:
    - Desktop: show on `mouseenter`/`mouseleave`
    - Keyboard: show on focus, hide on blur
    - Touch: toggle on click/tap (and close on outside click)
- Accessibility:
  - Ensure the trigger remains a real interactive element (do not wrap in non-semantic elements that break focus).
  - Use appropriate ARIA attributes (`aria-describedby`) and stable ids.
  - Provide an escape hatch to disable tooltip for cases where content is empty.
- Performance:
  - Tooltip must not perform any network calls.
  - Keep tooltip rendering lightweight (no heavy layout thrashing on hover).

**Focus on how to implement**

## Current Project State
- App has UI primitives under `app/src/components/ui/` but no dedicated Tooltip component.

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | app/src/components/ui/Tooltip.tsx | Shared tooltip component used by Patient 360 to show rationale + cited source text |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://www.w3.org/WAI/ARIA/apg/patterns/tooltip/

## Build Commands
- npm --prefix .\app run build
- npm --prefix .\app run test
- npm --prefix .\app run test:e2e

## Implementation Validation Strategy
- [Manual/UI] Verify tooltip shows on hover and focus, and hides on mouseleave/blur.
- [Manual/UI] On a touch device/emulation, verify tap toggles tooltip visibility and tapping outside closes it.
- [Accessibility] Verify trigger remains keyboard reachable and tooltip content is announced via `aria-describedby`.

## Implementation Checklist
- [ ] Create `Tooltip` component with hover/focus behaviors and touch toggle fallback
- [ ] Ensure ARIA wiring (`aria-describedby`) and stable ids
- [ ] Add outside-click handling for touch toggle mode
- [ ] Ensure component behaves safely when tooltip content is empty/undefined
- [ ] Validate behavior manually across mouse, keyboard, and touch interactions

---

# Design Reference

## UI Impact Assessment
**Has UI Changes**: [x] Yes [ ] No

## User Story Design Context
**Story ID**: US-[071]
**Story Title**: Show extraction rationale on hover
**UI Impact Type**: Component Update

### Design Source References
- **Design System**: `.propel/context/docs/designsystem.md`
- **Screen Spec**: `.propel/context/docs/figma_spec.md` (SCR-008, UXR-024, UXR-017)

### Screen-to-Design Mappings
| Screen/Feature | Image File | Image Path | Description | Implementation Priority |
|---------------|------------|------------|-------------|----------------------|
| Tooltip (Rationale on hover) | N/A | N/A | Tooltip used for explanation text and cited source text | High |

### Component References
| Component Name | Reference Image | Code Location | UI Changes Required |
|---------------|-----------------|---------------|-------------------|
| Tooltip | N/A | app/src/components/ui/Tooltip.tsx | New shared UI primitive with hover/focus/touch behaviors |

### Task Design Mapping
```yaml
TASK_071_001:
  title: "UI primitive: accessible Tooltip"
  ui_impact: true
  visual_references:
    figma_spec_refs: ["SCR-008", "UXR-017", "UXR-024", "FR-055"]
  components_affected:
    - Tooltip
  visual_validation_required: false
```

### Accessibility Requirements
- **WCAG Level**: AA
- **Keyboard Navigation**: Tooltip appears on focus and does not trap focus
- **Focus States**: Trigger maintains visible focus state
- **Screen Reader**: Tooltip content associated via `aria-describedby`
