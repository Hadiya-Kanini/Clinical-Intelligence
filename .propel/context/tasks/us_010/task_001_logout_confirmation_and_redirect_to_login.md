# Task - [TASK_001]

## Requirement Reference
- User Story: [us_010] (extracted from input)
- Story Location: [.propel/context/tasks/us_010/us_010.md]
- Acceptance Criteria: 
    - Given I am authenticated, When I trigger logout, Then the UI displays a logout confirmation message.
    - Given logout is successful, When the confirmation is shown, Then the user is redirected to the Login page (SCR-001) within 2 seconds.
    - Given logout is successful, When the user is redirected, Then the Login page is presented in its default state and no authenticated-only content is accessible.

## Task Overview
Implement an end-to-end logout experience that:
- provides clear user confirmation that the session ended,
- redirects to Login (SCR-001) within 2 seconds, and
- prevents access to authenticated-only routes after logout (including browser Back and multi-tab scenarios).

This includes wiring a backend logout endpoint, adding a logout trigger in the authenticated UI surface, and implementing minimal route protection so authenticated content is not accessible once logged out.

## Dependent Tasks
- [US_006] Login UI foundation and auth-related UX patterns

## Impacted Components
- app/src/pages/DashboardPage.jsx
- app/src/pages/LoginPage.jsx
- app/src/routes.jsx
- app/src/pages/__tests__/LoginPage.test.jsx
- app/src/__tests__/visual/login.spec.js
- Server/ClinicalIntelligence.Api/Program.cs

## Implementation Plan
- Add a Logout trigger to the authenticated surface:
  - Add a "Log out" button to `DashboardPage` (top/right aligned).
  - On click, call the backend logout endpoint (`POST /api/v1/auth/logout`).
- Implement logout success UX + redirect:
  - On successful logout response:
    - Navigate to `/login` with `replace: true` to reduce back-navigation exposure.
    - Pass a navigation state flag (e.g., `state: { logout: 'success' }`) so Login can show a success confirmation.
  - Ensure redirect occurs within 2 seconds (no artificial delays; the 2 seconds window is an upper bound).
- Implement logout error UX:
  - If logout fails due to network/API issues, keep the user on the current page and display a clear, actionable error message (retry guidance).
- Add minimal route protection so authenticated-only content is not accessible:
  - Introduce a minimal client-side auth state indicator (token presence or authenticated flag) and ensure:
    - `/dashboard` redirects to `/login` if not authenticated.
    - After logout, `/dashboard` cannot be reached via Back.
  - Note: This is a UX guardrail, not a substitute for backend authorization.
- Backend:
  - Add `POST /api/v1/auth/logout` under `/api/v1`:
    - Return a success status code even if the server is currently using development-only auth.
    - Prepare the endpoint for future session invalidation semantics.
- Testing:
  - Unit tests:
    - Cover logout success path (redirect + confirmation state on Login).
    - Cover logout failure path (error message shown, no redirect).
    - Cover route protection (Dashboard redirects to Login when unauthenticated).
  - Playwright:
    - Add coverage for:
      - Clicking "Log out" redirects to `/login` and shows confirmation.
      - Using Back after logout does not reveal authenticated content.

## Current Project State
```
app/
├─ src/
│  ├─ pages/
│  │  ├─ LoginPage.jsx
│  │  ├─ ForgotPasswordPage.jsx
│  │  ├─ DashboardPage.jsx
│  │  └─ __tests__/LoginPage.test.jsx
│  ├─ __tests__/visual/login.spec.js
│  └─ routes.jsx
└─ package.json

Server/
└─ ClinicalIntelligence.Api/
   └─ Program.cs
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | app/src/pages/DashboardPage.jsx | Add a "Log out" action that calls `POST /api/v1/auth/logout`, handles success/failure states, and navigates to `/login` with `replace: true`. |
| MODIFY | app/src/pages/LoginPage.jsx | Display a logout success confirmation when navigated from a successful logout (via router state). Ensure the Login page renders in default state after redirect (no residual auth-only content). |
| MODIFY | app/src/routes.jsx | Add minimal protection for authenticated-only routes (at minimum `/dashboard`) so unauthenticated users are redirected to `/login`. |
| MODIFY | app/src/pages/__tests__/LoginPage.test.jsx | Add coverage for logout-success message rendering on the Login page when `location.state` indicates logout success. |
| MODIFY | app/src/__tests__/visual/login.spec.js | Extend Playwright coverage to validate logout confirmation state on Login and verify Back navigation does not return to authenticated content. |
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Add `POST /api/v1/auth/logout` endpoint returning a success response and ready for future session invalidation logic. |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://reactrouter.com/en/main/hooks/use-navigate
- https://reactrouter.com/en/main/components/navigate
- https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt-bearer?view=aspnetcore-8.0

## Build Commands
- app: `npm run test`
- app (e2e): `npm run test:e2e`
- api: `dotnet test Server/ClinicalIntelligence.Api.Tests/ClinicalIntelligence.Api.Tests.csproj`
- api (run): `dotnet run --project Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj`

## Implementation Validation Strategy
- Unit tests:
  - Assert logout success triggers navigation to `/login` and Login shows confirmation.
  - Assert logout failure shows error and keeps user on the current screen.
  - Assert protected routes (at minimum `/dashboard`) redirect to `/login` when unauthenticated.
- Playwright:
  - Validate the logout flow end-to-end (button -> API -> redirect -> confirmation).
  - Validate browser Back does not expose authenticated UI after logout.
- Manual:
  - Validate multi-tab behavior: log out in one tab and verify other tabs converge to unauthenticated state on next interaction.
  - Validate slow network conditions show appropriate feedback.

## Implementation Checklist
- [ ] Add "Log out" action in `DashboardPage` and implement success/failure UX.
- [ ] Implement `/api/v1/auth/logout` backend endpoint.
- [ ] Add minimal route protection to prevent unauthenticated access to `/dashboard`.
- [ ] Show logout confirmation message on Login after redirect.
- [ ] Add/extend unit tests and Playwright coverage for logout + back-navigation.

---

# Design Reference

## UI Impact Assessment
**Has UI Changes**: [x] Yes [ ] No

## User Story Design Context
**Story ID**: US-[010]
**Story Title**: Show logout confirmation and redirect to Login
**UI Impact Type**: UI Enhancement

### Design Source References
- **Design System**: `.propel/context/docs/designsystem.md`
- **Screen Spec**: `.propel/context/docs/figma_spec.md` (UXR-008; Logout Confirmation Modal inventory; SCR-001 Login)

### Screen-to-Design Mappings
**Option B: Design Image References**
| Screen/Feature | Image File | Image Path | Description | Implementation Priority |
|---------------|------------|------------|-------------|----------------------|
| Authenticated surfaces (e.g., Dashboard SCR-004) | N/A | N/A | Provide a visible "Log out" action in the header/utility area | High |
| Login (SCR-001) | N/A | N/A | Display a clear logout confirmation message when redirected after logout | High |

### Design Tokens
```yaml
colors:
  success:
    usage: "Logout success confirmation state"
  error:
    usage: "Logout failure message"
  primary:
    usage: "Interactive elements and focus indicators"

typography:
  body:
    usage: "Confirmation message text"

motion:
  transitions:
    usage: "Button hover/focus transitions (fast)"
```

### Component References
**Option B: Image-Based Component References**
| Component Name | Reference Image | Code Location | UI Changes Required |
|---------------|-----------------|---------------|-------------------|
| Button | N/A | app/src/pages/DashboardPage.jsx | Add "Log out" action with loading/disabled handling where applicable. |
| Alert / Confirmation Message | N/A | app/src/pages/LoginPage.jsx | Show logout success and logout failure messaging in a consistent feedback component. |

### Accessibility Requirements
- **WCAG Level**: AA
- **Keyboard Navigation**: Logout action must be reachable via Tab and activatable via Enter/Space.
- **Focus States**: Visible focus indicator for the logout action and any confirmation UI.
- **ARIA Live**: Confirmation feedback should be announced to screen readers (where the chosen component supports it).

### Design Review Checklist
- [ ] Logout action placement is discoverable and consistent with utility nav patterns.
- [ ] Confirmation message is short, confirmatory, and professional (clinical tone).
- [ ] Redirect timing meets the ≤2s requirement.
- [ ] Keyboard-only flow works end-to-end.
