# Task - [TASK_003]

## Requirement Reference
- User Story: [us_035]
- Story Location: [.propel/context/tasks/us_035/us_035.md]
- Acceptance Criteria: 
    - [Given a Standard User, When they navigate to admin UI routes, Then they are redirected to their dashboard.]
    - [Given the navigation menu, When displayed to Standard Users, Then admin menu items are not visible.]
    - [Given authorization checks, When performed, Then they occur on every request (not just initial load).]

## Task Overview
Enforce admin-only restrictions in the React UI by:
- Guarding admin routes (`/admin`, `/admin/users`) so Standard users are redirected to `/dashboard`
- Hiding admin navigation entries for Standard users
- Avoiding stale client-side authorization decisions (do not use legacy `localStorage` role flags)

## Dependent Tasks
- [US_033] (Define Admin and Standard User roles)
- [US_035 TASK_001] (Backend admin endpoint protection)
- [US_035 TASK_002] (Backend role-change mid-session handling)

## Impacted Components
- [MODIFY | app/src/routes.tsx | Ensure `RequireAdmin` uses current authenticated user role (server-derived) and redirects Standard users to `/dashboard`]
- [MODIFY | app/src/components/AppShell.tsx | Hide Admin navigation items for Standard users based on current user role]
- [MODIFY | app/src/store/slices/authSlice.ts | Ensure role is sourced from `/api/v1/auth/me` and drives UI decisions]

## Implementation Plan
- Establish a single UI source of truth for role:
  - Use Redux `authSlice.user.role` (populated by `/api/v1/auth/me`) for route guard and navigation decisions.
  - Do not rely on `ci_user_role` / `ci_auth` localStorage values.
- Route guard enforcement:
  - Update `RequireAdmin` to:
    - redirect unauthenticated users to `/login`
    - redirect authenticated non-admin users to `/dashboard`
  - Ensure these checks are executed on every navigation to protected routes.
- Navigation enforcement:
  - Update `AppShell` so admin links are only rendered when role is Admin.
- Role downgrade and cached UI:
  - Ensure the app re-checks session state (via existing auth initialization flow) so a user who was downgraded does not keep admin UI access.

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | app/src/routes.tsx | Implement admin route guarding based on server-derived role and redirect Standard users to `/dashboard` |
| MODIFY | app/src/components/AppShell.tsx | Show/hide Admin navigation based on current user role |
| MODIFY | app/src/store/slices/authSlice.ts | Ensure role is consistently reflected in store based on `/api/v1/auth/me` |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://reactrouter.com/en/main/start/overview

## Build Commands
- npm --prefix .\\app run build
- npm --prefix .\\app run test
- npm --prefix .\\app run test:e2e

## Implementation Validation Strategy
- [Manual/UI] Login as Standard and confirm Admin navigation is hidden.
- [Manual/UI] As Standard, navigate directly to `/admin` and `/admin/users` and confirm redirect to `/dashboard`.
- [Manual/UI] Login as Admin and confirm admin routes are accessible.

## Implementation Checklist
- [ ] Update `RequireAdmin` to use store-based role (server-derived)
- [ ] Hide admin navigation items in `AppShell` for Standard users
- [ ] Remove reliance on `ci_user_role` / `ci_auth` for authorization decisions
- [ ] Validate redirects and menu visibility for Admin vs Standard

## Design Reference

## UI Impact Assessment
**Has UI Changes**: [ ] Yes [ ] No
- If NO: Skip this design reference section entirely
- If YES: Complete all applicable sections below

## User Story Design Context
**Story ID**: US-[035]
**Story Title**: Restrict admin-only features from Standard Users
**UI Impact Type**: UI Enhancement

### Design Source References
- **Design System**: .propel/context/docs/designsystem.md
- **Screen Spec**: .propel/context/docs/figma_spec.md

### Screen-to-Design Mappings
| Screen/Feature | Image File | Image Path | Description | Implementation Priority |
|---------------|------------|------------|-------------|----------------------|
| Admin routes guard | N/A | N/A | Redirect Standard users from `/admin*` to `/dashboard` | High |
| Left navigation | N/A | N/A | Hide admin menu items for Standard users | High |

### Task Design Mapping
```yaml
TASK_035_003:
  title: "Frontend admin route guard + nav visibility"
  ui_impact: true
  visual_references:
    figma_spec_refs: ["Navigation", "Admin routes"]
  components_affected:
    - AppShell (navigation rendering)
    - Routes (RequireAdmin guard)
  visual_validation_required: false
```

### Accessibility Requirements
- **WCAG Level**: AA
- **Focus States**: Ensure navigation remains keyboard-accessible when admin links are hidden
- **Screen Reader**: Ensure hidden admin items are not announced for Standard users
