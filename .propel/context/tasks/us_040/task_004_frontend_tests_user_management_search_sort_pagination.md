# Task - [TASK_004]

## Requirement Reference
- User Story: [us_040]
- Story Location: [.propel/context/tasks/us_040/us_040.md]
- Acceptance Criteria: 
    - [Given I am authenticated as Admin, When I navigate to User Management (SCR-014), Then I see a list of all users.]
    - [Given the user list, When displayed, Then it is searchable by name or email (UXR-044).]
    - [Given the user list, When displayed, Then it is sortable by columns (name, email, role, status).]
    - [Given the user list, When there are many users, Then pagination is implemented (TR-017).]

## Task Overview
Add frontend automated tests to ensure the User Management page behaviors are stable:
- Renders user rows from backend
- Search triggers backend calls and updates UI
- Sorting controls change ordering
- Pagination controls change pages

## Dependent Tasks
- [US_040 TASK_002] (Frontend user management page search/sort/pagination)

## Impacted Components
- [CREATE | app/e2e/user-management.spec.ts | E2E coverage for SCR-014 behaviors]

## Implementation Plan
- Create an end-to-end test that:
  - Logs in as Admin (using existing login flow).
  - Navigates to `/admin/users`.
  - Validates that at least one row is rendered.
  - Enters a search query and asserts table results update.
  - Clicks a sortable header and asserts ordering changes.
  - Uses pagination controls and asserts page changes.
- Make tests deterministic:
  - Prefer seeding users in test environment (or use an API seed helper if existing).
  - Avoid brittle selectors; use role-based selectors / stable text.

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | app/e2e/user-management.spec.ts | E2E test covering SCR-014 list/search/sort/pagination |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://playwright.dev/docs/test-intro

## Build Commands
- npm --prefix .\\app run test:e2e

## Implementation Validation Strategy
- [Automated] Run E2E suite and confirm stability across multiple runs.
- [Manual] Spot-check selectors and accessibility flows (Tab navigation).

## Implementation Checklist
- [ ] Add E2E spec for `/admin/users` basic rendering
- [ ] Cover search behavior and result changes
- [ ] Cover sorting behavior on at least one column
- [ ] Cover pagination behavior (next/prev)

---

# Design Reference

## UI Impact Assessment
**Has UI Changes**: [x] Yes [ ] No

## User Story Design Context
**Story ID**: US-[040]
**Story Title**: Implement user management page for admins
**UI Impact Type**: UI Enhancement

### Design Source References
- **Design System**: `.propel/context/docs/designsystem.md`
- **Screen Spec**: `.propel/context/docs/figma_spec.md` (SCR-014)

### Screen-to-Design Mappings
| Screen/Feature | Image File | Image Path | Description | Implementation Priority |
|---------------|------------|------------|-------------|----------------------|
| User Management (SCR-014) | N/A | N/A | Validate Default interactions for search/sort/pagination | High |
