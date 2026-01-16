# Task - TASK_057_002

## Requirement Reference
- User Story: us_057
- Story Location: .propel/context/tasks/us_057/us_057.md
- Acceptance Criteria: 
    - Given the UI, When real-time updates are active, Then a manual refresh button is also available.
    - Edge Case: What happens when WebSocket/polling connection is lost?
    - Edge Case: What happens when the user navigates away and returns?

## Task Overview
Harden the Document List real-time refresh UX by adding:
- A manual refresh control (always available)
- Clear UI feedback for last refresh time and refresh-in-progress
- Resilience behaviors when polling fails (network/API errors), including a safe retry path

Estimated Effort: 5 hours

## Dependent Tasks
- [TASK_057_001] (Polling-based real-time status updates)

## Impacted Components
- [MODIFY | app/src/pages/DocumentListPage.tsx | Add manual refresh button + last updated indicator + safe error handling for polling failures]
- [MODIFY | app/src/hooks/useDocumentListPolling.ts | Expose `refreshNow()` and `lastUpdatedAt` (and optional `isRefreshing`) to support manual refresh UX]

## Implementation Plan
- Manual refresh UX:
  - Add a "Refresh" button in `DocumentListPage` header actions (near search/clear)
  - Button behavior:
    - Calls `refreshNow()` to force an immediate fetch
    - Disabled while a request is in-flight
- Lost connection / error UX:
  - When the polling hook encounters a request failure:
    - Surface a safe error message via existing `Alert` component
    - Keep the last successfully loaded data rendered (do not blank the table)
    - Continue polling with a backoff strategy (e.g., pause auto-polling until the user clicks Refresh) to avoid repeated failures
- Navigation away / return:
  - Ensure polling restarts cleanly when the page remounts
  - Ensure intervals are always cleaned up on unmount

**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | app/src/hooks/useDocumentListPolling.ts | Add `refreshNow()` and `lastUpdatedAt` (and optional `isRefreshing`) so UI can trigger manual refresh and display freshness |
| MODIFY | app/src/pages/DocumentListPage.tsx | Add Refresh button + last updated indicator and show safe error state when polling fails |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://react.dev/reference/react/useEffect

## Build Commands
- npm --prefix .\app run build
- npm --prefix .\app run test

## Implementation Validation Strategy
- [Manual/UI] Confirm Refresh button triggers immediate data refresh.
- [Manual/UI] Simulate network failure and confirm:
  - Existing data remains visible
  - A safe alert is shown
  - User can recover via Refresh
- [Manual/UI] Navigate away from /documents and return; confirm polling resumes and no duplicate intervals are created.

## Implementation Checklist
- [ ] Add `refreshNow()` API to polling hook
- [ ] Add `lastUpdatedAt` and `isRefreshing` state to polling hook
- [ ] Add Refresh button to Document List header actions
- [ ] Display "Last updated" timestamp (secondary text) in the header
- [ ] On polling error, show safe `Alert` without clearing table
- [ ] Confirm polling cleanup on unmount to prevent duplicate intervals

---

# Design Reference

## UI Impact Assessment
**Has UI Changes**: [x] Yes [ ] No

## User Story Design Context
**Story ID**: US-[057]
**Story Title**: Provide real-time status updates during processing
**UI Impact Type**: UI Enhancement

### Design Source References
- **Design System**: `.propel/context/docs/designsystem.md`
- **Screen Spec**: `.propel/context/docs/figma_spec.md` (SCR-006, UXR-043)

### Screen-to-Design Mappings
| Screen/Feature | Image File | Image Path | Description | Implementation Priority |
|---------------|------------|------------|-------------|----------------------|
| Document List | N/A | N/A | Provide manual Refresh control and clear feedback for freshness / error recovery | High |

### Component References
| Component Name | Reference Image | Code Location | UI Changes Required |
|---------------|-----------------|---------------|-------------------|
| Button | N/A | app/src/pages/DocumentListPage.tsx | Add a Refresh action (secondary variant) and keep spacing consistent with existing headerRight controls |
| Alert | N/A | app/src/pages/DocumentListPage.tsx | Display safe error messaging when refresh fails |

### Accessibility Requirements
- **WCAG Level**: AA
- **Keyboard Navigation**: Refresh button must be reachable via tab and indicate disabled state via standard semantics
