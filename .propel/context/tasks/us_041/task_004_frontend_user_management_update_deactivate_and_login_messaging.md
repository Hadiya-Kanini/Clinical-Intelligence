# Task - [TASK_004]

## Requirement Reference
- User Story: [us_041]
- Story Location: [.propel/context/tasks/us_041/us_041.md]
- Acceptance Criteria: 
    - [Given user operations, When completed, Then confirmation messages are displayed (UXR-011).]
    - [Given a deactivated user, When they attempt to login, Then they are denied access with appropriate message.]

## Task Overview
Ensure the frontend provides correct UX feedback for admin update/deactivation operations in User Management (SCR-014) and displays an appropriate login denial message for deactivated accounts.

This task focuses on client-side wiring, UI messaging, and status labeling consistency (Active vs Deactivated), building on the backend endpoints introduced in US_041.

## Dependent Tasks
- [US_041 TASK_001] (Backend update endpoint)
- [US_041 TASK_002] (Backend toggle-status/deactivate endpoint)
- [US_041 TASK_003] (Backend login denial response for inactive users)

## Impacted Components
- [MODIFY | app/src/pages/UserManagementPage.tsx | Align status labeling (inactive -> deactivated), and ensure success/error confirmation messages are displayed]
- [MODIFY | app/src/lib/adminUsersApi.ts | Ensure API wrapper is aligned with backend routes and response shapes]
- [MODIFY | app/src/pages/LoginPage.tsx | Ensure deactivated-account denial message is displayed clearly to the user]

## Implementation Plan
- User Management (SCR-014):
  - Confirm `adminUsersApi.updateUser` targets `PUT /api/v1/admin/users/{id}` (backend).
  - Confirm `adminUsersApi.toggleUserStatus` targets `PATCH /api/v1/admin/users/{id}/toggle-status` (backend).
  - Status label normalization:
    - Treat backend `status: "inactive"` as the UI concept “deactivated” for display.
    - Keep the backend value unchanged for API calls; only change the presentation label (badge text and action button label).
  - Confirmation messages:
    - Ensure successful update shows a confirmation message.
    - Ensure successful deactivation/reactivation shows a confirmation message.
    - Ensure backend errors (including self-deactivation/static-admin protection errors) are surfaced to the admin as safe user-facing messages.
- Login denial messaging:
  - When the backend returns `403` with an “account inactive/deactivated” message, display it on the login screen (existing `error` display is acceptable).

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | app/src/pages/UserManagementPage.tsx | Normalize displayed status (“inactive” -> “deactivated”), preserve backend state, and ensure confirmation messaging for update/deactivate flows |
| MODIFY | app/src/lib/adminUsersApi.ts | Verify/update API wrapper routes and typings for update + toggle-status |
| MODIFY | app/src/pages/LoginPage.tsx | Display clear message when backend denies login for deactivated accounts |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://react.dev/reference/react/useState
- https://react.dev/reference/react/useEffect

## Build Commands
- npm --prefix .\\app run build
- npm --prefix .\\app run test
- npm --prefix .\\app run test:e2e

## Implementation Validation Strategy
- [Manual/UI] Admin can edit a user; after save, a confirmation message is displayed and table reflects updated values.
- [Manual/UI] Admin can deactivate a user; after action, a confirmation message is displayed and UI reflects “deactivated”.
- [Manual/UI] Attempt to deactivate self/static admin and confirm message is displayed and action is blocked.
- [Manual/UI] Attempt login as a deactivated user and confirm an appropriate denial message is shown.

## Implementation Checklist
- [x] Ensure `adminUsersApi.updateUser` and `adminUsersApi.toggleUserStatus` match backend routes
- [x] Normalize displayed status text for inactive users to "deactivated" in SCR-014
- [x] Ensure update + deactivate operations show confirmation messages (UXR-011)
- [x] Surface backend error messages for prohibited operations (self/static admin) safely
- [x] Ensure deactivated login denial message is displayed in Login UI

---

# Design Reference

## UI Impact Assessment
**Has UI Changes**: [x] Yes [ ] No

## User Story Design Context
**Story ID**: US-[041]
**Story Title**: Implement user update and deactivation operations
**UI Impact Type**: UI Enhancement

### Design Source References
- **Design System**: `.propel/context/docs/designsystem.md`
- **Screen Spec**: `.propel/context/docs/figma_spec.md` (SCR-014, UXR-011)

### Screen-to-Design Mappings
| Screen/Feature | Image File | Image Path | Description | Implementation Priority |
|---------------|------------|------------|-------------|----------------------|
| User Management (SCR-014) | N/A | N/A | Edit + deactivate flows with clear success/error feedback (UXR-011) | High |
| Login (SCR-001) | N/A | N/A | Display deactivated-account denial messaging | Medium |

### Component References
| Component Name | Reference Image | Code Location | UI Changes Required |
|---------------|-----------------|---------------|-------------------|
| Modal | N/A | app/src/pages/UserManagementPage.tsx | Ensure edit flow uses existing modal patterns and displays feedback consistently |
| Alert | N/A | app/src/pages/UserManagementPage.tsx, app/src/pages/LoginPage.tsx | Display success/error messaging consistent with design system |
| Badge | N/A | app/src/pages/UserManagementPage.tsx | Status label consistency for “deactivated” |

### Task Design Mapping
```yaml
TASK_041_004:
  title: "User management update/deactivate UX + login messaging"
  ui_impact: true
  visual_references:
    figma_spec_refs: ["SCR-014", "SCR-001", "UXR-011"]
  components_affected:
    - UserManagementPage
    - LoginPage
  visual_validation_required: false
```

### Accessibility Requirements
- **WCAG Level**: AA
- **Keyboard Navigation**: Edit/deactivate actions operable via keyboard
- **Screen Reader**: Status and confirmation messages should be announced (role=alert/status where appropriate)
