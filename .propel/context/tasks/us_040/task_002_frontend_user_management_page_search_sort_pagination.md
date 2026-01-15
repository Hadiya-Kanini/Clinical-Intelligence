# Task - [TASK_002]

## Requirement Reference
- User Story: [us_040]
- Story Location: [.propel/context/tasks/us_040/us_040.md]
- Acceptance Criteria: 
    - [Given I am authenticated as Admin, When I navigate to User Management (SCR-014), Then I see a list of all users.]
    - [Given the user list, When displayed, Then it is searchable by name or email (UXR-044).]
    - [Given the user list, When displayed, Then it is sortable by columns (name, email, role, status).]
    - [Given the user list, When there are many users, Then pagination is implemented (TR-017).]

## Task Overview
Update the existing `UserManagementPage` to load user data from the backend and provide the required table behaviors for SCR-014:
- Search input that queries name/email
- Sortable columns (name, email, role, status)
- Pagination controls for large result sets

This task focuses on making the page functional for read-only listing. Full CRUD wiring (create/edit/deactivate) is handled by dependent user stories (e.g., US_037) and remains out of scope.

## Dependent Tasks
- [US_035 TASK_003] (Frontend admin route guard + nav visibility)
- [US_040 TASK_001] (Backend admin list users endpoint)

## Impacted Components
- [MODIFY | app/src/pages/UserManagementPage.tsx | Replace hardcoded users with API-backed list; implement search, sorting, and pagination state]
- [CREATE | app/src/lib/adminUsersApi.ts | Thin API wrapper around `GET /api/v1/admin/users` using existing `apiClient`]

## Implementation Plan
- Add an API wrapper:
  - Create `adminUsersApi` wrapper that calls `api.get` against `GET /api/v1/admin/users`.
  - Keep typing aligned with response contract returned by the backend.
- Page state:
  - Track `query`, `sortBy`, `sortDir`, `page`, `pageSize`.
  - Debounce `query` updates (to avoid issuing requests on every keystroke).
  - Reset `page` to 1 when `query` or sort changes.
- Fetch lifecycle:
  - Use loading + error states (consistent with other pages).
  - Display empty state when `items.length === 0`.
- Sorting UI:
  - Make header cells clickable to toggle sort.
  - Sort order should be visually indicated (text-only indicator is acceptable).
- Pagination UI:
  - Provide Previous/Next controls (and optionally page-size selection).
  - Disable controls appropriately (e.g., Prev disabled on page 1).
- Security/authorization UX:
  - If the API returns 403, display a safe error and rely on route guards to redirect non-admins.

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | app/src/pages/UserManagementPage.tsx | Load users from backend and implement search/sort/pagination for SCR-014 |
| CREATE | app/src/lib/adminUsersApi.ts | API wrapper for admin users list endpoint with typed response |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://react.dev/reference/react/useEffect
- https://reactrouter.com/en/main/start/overview

## Build Commands
- npm --prefix .\\app run build
- npm --prefix .\\app run test
- npm --prefix .\\app run test:e2e

## Implementation Validation Strategy
- [Manual/UI] As Admin, load `/admin/users` and confirm the table renders users from the database.
- [Manual/UI] Search by partial name/email and confirm the table updates.
- [Manual/UI] Click column headers and confirm server-side sorting changes results.
- [Manual/UI] Paginate through results (seed 100+ users) and confirm stable performance.
- [Accessibility] Confirm keyboard access to search input, sort controls, and pagination controls.

## Implementation Checklist
- [ ] Add `adminUsersApi` wrapper for `GET /api/v1/admin/users`
- [ ] Replace hardcoded list in `UserManagementPage` with API-backed data and loading/error states
- [ ] Implement debounced search by name/email
- [ ] Implement sortable columns (name, email, role, status) with state + visual indication
- [ ] Implement pagination controls and page reset behavior on query/sort changes

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
| User Management (SCR-014) | N/A | N/A | Default + Loading + Empty + Error with searchable/sortable/paginated table | High |

### Component References
| Component Name | Reference Image | Code Location | UI Changes Required |
|---------------|-----------------|---------------|-------------------|
| Card | N/A | app/src/pages/UserManagementPage.tsx | Ensure headerRight contains search + actions area consistent with existing pages |
| Table | N/A | app/src/pages/UserManagementPage.tsx | Add sortable headers and stable table layout |
| TextField | N/A | app/src/pages/UserManagementPage.tsx | Use existing field styling for search input (or existing TextField where appropriate) |
| Button | N/A | app/src/pages/UserManagementPage.tsx | Add pagination controls using existing button variants |
| Alert | N/A | app/src/pages/UserManagementPage.tsx | Loading/error/empty state messaging consistent with design system |

### Task Design Mapping
```yaml
TASK_040_002:
  title: "User management table: search/sort/pagination"
  ui_impact: true
  visual_references:
    figma_spec_refs: ["SCR-014", "UXR-044"]
  components_affected:
    - UserManagementPage
  visual_validation_required: false
```

### Accessibility Requirements
- **WCAG Level**: AA
- **Keyboard Navigation**: Search input, sort toggles, and pagination controls reachable via Tab
- **Focus States**: Visible focus for interactive elements
- **Screen Reader**: Sort controls should have clear accessible labels
