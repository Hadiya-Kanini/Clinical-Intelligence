# Task - TASK_001

## Requirement Reference
- User Story: us_007
- Story Location: .propel/context/tasks/us_007/us_007.md
- Acceptance Criteria: 
    - Given I am on the Login page (SCR-001), When I submit with an invalid email format, Then I see a field-level validation error indicating the email format is invalid.
    - Given I am on the Login page, When I submit with missing required fields (email or password), Then I see clear field-level validation errors and the form is not submitted.
    - Given I am on the Login page, When I submit valid-format inputs but credentials are incorrect, Then I see an actionable authentication error message.
    - Given the login attempt results in an auth-related restriction, When the system determines the account is locked, Then the UI shows a lockout message with guidance for next steps.
    - Given the login attempt fails, When the error message is displayed, Then it is presented in an accessible manner (e.g., alert region) and does not expose sensitive details.

## Task Overview
Implement actionable and accessible authentication failure messaging on SCR-001 (Login) that cleanly separates:
- client-side validation errors (missing/invalid format)
- server-side authentication errors (invalid credentials, locked account, rate limit)
- network/unexpected failures (timeouts/unreachable)

This task aligns frontend behavior with the backend standardized error contract `{ error: { code, message, details } }` so the UI can map stable error codes to user-safe, non-enumerating guidance.

## Dependent Tasks
- .propel/context/tasks/us_006/task_002_implement_login_page_ui_and_responsive_layout.md (TASK_002)
- .propel/context/tasks/us_006/task_003_add_login_form_validation_states_and_accessibility_hardening.md (TASK_003)
- .propel/context/tasks/us_003/task_001_standardize_api_error_response_format.md (TASK_001)

## Impacted Components
- app/src/pages/LoginPage.jsx
- app/src/pages/__tests__/LoginPage.test.jsx
- app/src/__tests__/visual/login.spec.js
- Server/ClinicalIntelligence.Api/Program.cs
- Server/ClinicalIntelligence.Api/Contracts/

## Implementation Plan
- Backend: make the login endpoint compatible with frontend form submission and standardized errors:
  - Update `/api/v1/auth/login` to accept a JSON request body (DTO/record) rather than primitive parameters.
  - Ensure missing/invalid inputs return `ApiErrorResults.BadRequest(...)` using stable codes (e.g., `invalid_input`).
  - Ensure invalid credentials return `ApiErrorResults.Unauthorized(code: "invalid_credentials", message: "Invalid email or password.")`.
  - Add a placeholder/contract for lockout and rate limiting errors (if not yet enforced in auth logic):
    - lockout -> `ApiErrorResults.Forbidden(code: "account_locked", message: "Account temporarily locked.")`
    - rate limit -> `ApiErrorResults.TooManyRequests(code: "rate_limited", message: "Too many login attempts.")`
  - Ensure responses do not leak whether an email exists and do not include stack traces or sensitive configuration.
- Frontend: wire Login form submission and error mapping:
  - Add an async submit handler calling `POST /api/v1/auth/login` with JSON body.
  - Add `loading` state and disable submit while submitting.
  - Maintain entered email on any failure; do not clear the form on server/network errors.
  - Parse backend standardized errors and map to actionable UI messages:
    - `invalid_credentials` -> actionable but generic (no account enumeration).
    - `account_locked` -> lockout guidance (next steps, retry later/support).
    - `rate_limited` -> retry guidance.
    - unknown codes / network errors -> recoverable generic error (try again) while keeping email.
  - Present server/network errors in an accessible alert region (reuse `Alert` component) and ensure focus behavior is reasonable (e.g., focus alert on submit error or keep existing focus-first-error behavior for field errors).
  - Ensure multiple field validation errors can appear simultaneously (email + password) and are associated via `aria-describedby`.
- Tests:
  - Unit tests (Vitest + Testing Library):
    - Verify invalid input still blocks submission and shows field-level errors.
    - Mock `fetch` to simulate standardized error responses for each error code and assert the correct UI message is displayed.
    - Mock `fetch` network failure and confirm a recoverable message is shown without losing entered email.
  - E2E/visual smoke:
    - Extend Playwright login visual test to cover at least one auth error state screenshot (e.g., invalid credentials) without breaking layout at mobile width.

**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Update `/api/v1/auth/login` to bind JSON request body and return standardized error payloads (use `ApiErrorResults`). |
| CREATE | Server/ClinicalIntelligence.Api/Contracts/LoginRequest.cs | Add a request DTO/record for login request payload (e.g., `email/username`, `password`). |
| MODIFY | app/vite.config.js | Add a dev-server proxy for `/api/*` to avoid CORS issues during local development (configurable via `VITE_BACKEND_URL`). |
| MODIFY | app/src/pages/LoginPage.jsx | Implement API-backed submit, loading state, and map backend error codes to actionable, accessible messages without leaking sensitive details. |
| MODIFY | app/src/pages/__tests__/LoginPage.test.jsx | Add tests for auth error mapping (`invalid_credentials`, `account_locked`, `rate_limited`) and network failure (email preserved). |
| MODIFY | app/src/__tests__/visual/login.spec.js | Add at least one auth error/lockout/rate-limit screenshot assertion at mobile width (layout safety). |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://www.w3.org/WAI/ARIA/apg/patterns/alert/
- https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/401
- https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/429

## Build Commands
- Frontend:
  - npm install
  - npm run dev
  - npm run build
  - npm run test
  - npm run test:e2e
- Backend:
  - dotnet build Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj

## Implementation Validation Strategy
- Confirm client-side validation still prevents submission on missing/invalid fields.
- Confirm backend returns standardized error JSON for failed login attempts and that frontend maps `error.code` to correct UI message.
- Confirm UI does not expose sensitive details (no stack traces, no “email not found” messaging).
- Confirm accessibility semantics:
  - field errors: `aria-invalid` + `aria-describedby`
  - submit/auth errors: alert region announced (`role=alert` via `Alert`)
- Confirm email input value is preserved for network/auth failures.

## Implementation Checklist
- [x] Update backend login endpoint to accept JSON body and return standardized error payloads
- [x] Implement frontend login submit (POST) with loading + disabled submit
- [x] Add stable frontend mapping for `error.code` -> user-safe actionable messages
- [x] Add unit tests for server error mapping + network failure behaviors
- [x] Add Playwright screenshot coverage for one auth error state

# Design Reference

## UI Impact Assessment
**Has UI Changes**: [x] Yes [ ] No

## User Story Design Context
**Story ID**: US-007
**Story Title**: Show actionable login validation and auth errors
**UI Impact Type**: UI Enhancement

### Design Source References
- **Design System**: `.propel/context/docs/designsystem.md`
- **Screen Spec**: `.propel/context/docs/figma_spec.md` (SCR-001 states; UXR-002, UXR-009, UXR-010)

### Screen-to-Design Mappings
| Screen/Feature | Image File | Image Path | Description | Implementation Priority |
|---------------|------------|------------|-------------|----------------------|
| SCR-001 / Error + Lockout + Rate Limited | N/A | N/A | Actionable auth errors, lockout guidance, and retry messaging; does not leak sensitive details | High |

### Visual Validation Criteria
- Error and lockout messages are readable and actionable
- Error states do not break layout at mobile width and when error text wraps
- Token-only styling is preserved for error states

### Accessibility Requirements
- **WCAG Level**: AA
- **Screen Reader**: Auth errors announced via alert region; field errors connected via `aria-describedby`
- **Keyboard**: No focus traps; submit remains reachable; focus behavior remains predictable on error
