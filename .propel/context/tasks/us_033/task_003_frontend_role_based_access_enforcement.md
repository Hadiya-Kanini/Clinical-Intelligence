# Task - [TASK_003]

## Requirement Reference
- User Story: [us_033]
- Story Location: [.propel/context/tasks/us_033/us_033.md]
- Acceptance Criteria: 
    - [Given the Admin role, When assigned, Then the user can access User Management and System Health functions (FR-011).]
    - [Given the Standard User role, When assigned, Then the user can access Patient Dashboard, Document Upload, and Profile functions (FR-012).]
    - [Given role definitions, When implemented, Then they are enforced at both API and UI levels.]

## Task Overview
Enforce role-based access control at the UI layer by:
- Using the server-provided role from `/api/v1/auth/me` (cookie-based auth)
- Guarding routes for Admin-only pages
- Rendering navigation based on authenticated user role

This task also removes reliance on legacy `localStorage` role flags (e.g., `ci_user_role`) to avoid stale client-side authorization decisions.

## Dependent Tasks
- [US_011] (JWT authentication with HttpOnly cookies)
- [US_033 TASK_001] (Backend RBAC policies)
- [US_033 TASK_002] (Role-change mid-session handling) (recommended so UI reacts correctly)

## Impacted Components
- [MODIFY | app/src/routes.tsx | Replace localStorage-based auth/role checks with state derived from API auth state]
- [MODIFY | app/src/components/AppShell.tsx | Render Admin navigation based on current authenticated user role]
- [MODIFY | app/src/store/slices/authSlice.ts | Ensure role is sourced from `/api/v1/auth/me` and propagated through app]
- [MODIFY | app/src/main.tsx | Ensure 401 handling continues to clear any legacy auth indicators]

## Implementation Plan
- Establish a single UI source of truth for auth state:
  - Prefer Redux `authSlice` and `checkAuthAsync` (already present) to retrieve current user and role from `/api/v1/auth/me`.
  - Remove/stop using `ci_auth` and `ci_user_role` for authorization decisions.
- Route enforcement:
  - Update `RequireAuth` and `RequireAdmin` in `routes.tsx` to rely on current auth state and user role.
  - Ensure Admin-only paths remain:
    - `/admin`
    - `/admin/users`
  - Ensure Standard-user accessible paths remain:
    - `/dashboard`
    - `/documents/upload`
    - `/documents`
    - `/patients/:patientId`
    - `/export`
- Navigation enforcement:
  - Update `AppShell` so admin navigation is shown only when the current user role is Admin.
- Mid-session role changes:
  - Ensure UI behavior is consistent when backend returns `401` due to session invalidation.

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | app/src/routes.tsx | Use authenticated user role (server-derived) for `RequireAdmin` and `RequireAuth` guards |
| MODIFY | app/src/components/AppShell.tsx | Show/hide Admin navigation based on current user role (not localStorage) |
| MODIFY | app/src/store/slices/authSlice.ts | Ensure role is consistently reflected in store based on `/api/v1/auth/me` |
| MODIFY | app/src/main.tsx | Confirm 401 handling clears any legacy localStorage indicators and redirects to login |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://reactrouter.com/en/main/start/overview

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Manual/UI] Login as Admin and confirm `Admin dashboard` and `User management` are visible and accessible.
- [Manual/UI] Login as Standard and confirm Admin navigation is hidden and direct navigation to `/admin/*` routes redirects to `/dashboard`.
- [Manual/UI] Simulate backend session invalidation and confirm user is redirected to login and cannot access protected routes.

## Implementation Checklist
- [ ] Update route guards to use authenticated user role from app state
- [ ] Update AppShell navigation rendering to use app state role
- [ ] Remove reliance on `ci_user_role` / `ci_auth` for role decisions
- [ ] Validate access/redirect behavior for Admin and Standard users
- [ ] Validate UI behavior on session invalidation (401)
