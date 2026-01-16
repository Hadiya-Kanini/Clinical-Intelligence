# Task - TASK_073_003

## Requirement Reference
- User Story: us_073
- Story Location: .propel/context/tasks/us_073/us_073.md
- Acceptance Criteria: 
    - Given extracted data, When displayed, Then verified, unverified, and modified states are visually distinct.
    - Given status indicators, When used, Then they are consistent with the design system.

## Task Overview
Add automated frontend coverage to ensure the Patient 360 UI renders verification status indicators consistently and accessibly.

Because the current UI test setup focuses on security/visual specs, this task uses unit/integration style tests (React Testing Library) to validate:
- The correct label text is rendered for each status.
- The correct badge variant class is applied (design system alignment).
- Status meaning is available without relying on color alone.

## Dependent Tasks
- [TASK_073_001] (Frontend data status indicators implemented)

## Impacted Components
- [CREATE | app/src/__tests__/patient360/dataStatusIndicators.test.tsx | Tests for DataStatusBadge rendering and Patient360 integration]

## Implementation Plan
- Add a focused test file under `app/src/__tests__/patient360/`:
  - Render `DataStatusBadge` directly for each status state and assert:
    - Label text (Verified/Unverified/Modified)
    - Variant class mapping (`ui-badge--success`, `ui-badge--warning`, `ui-badge--info`)
  - Add a lightweight integration test (if feasible) that renders the Patient 360 extracted data section and asserts status indicator presence for a sample entity list.
- Accessibility assertions:
  - Confirm label text exists (so meaning is not color-only).
  - If `aria-label` is used, assert it is present and accurate.

**Focus on how to implement**

## Current Project State
- UI tests exist under `app/src/__tests__/` (security + visual).
- No Patient 360 specific tests are present yet.

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | app/src/__tests__/patient360/dataStatusIndicators.test.tsx | Validate status badge label + variant mapping for verified/unverified/modified |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://testing-library.com/docs/react-testing-library/intro/

## Build Commands
- npm --prefix .\app run test

## Implementation Validation Strategy
- [Automated] Run `npm --prefix .\app run test` and confirm tests pass.
- [Regression] Ensure tests do not rely on hard-coded colors; assert against variant classes and label text.

## Implementation Checklist
- [ ] Create Patient 360 test folder and add `dataStatusIndicators.test.tsx`
- [ ] Add unit tests for `DataStatusBadge` for all status values
- [ ] Add optional integration-level assertion for Patient 360 extracted-data UI once available
- [ ] Ensure tests assert label text (not color-only)
