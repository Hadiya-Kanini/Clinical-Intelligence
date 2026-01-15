# Task - [TASK_001]

## Requirement Reference
- User Story: [us_017] (extracted from input)
- Story Location: [.propel/context/tasks/us_017/us_017.md]
- Acceptance Criteria: 
    - [Given the API returns HTTP 429, When the UI receives the response, Then it displays a message with the retry timeframe (UXR-010).]
    - [Given rate limit or lockout occurs, When the message is displayed, Then it includes contact support option for assistance.]

## Task Overview
Update the login UI to display an actionable, non-sensitive rate limiting message when the backend returns HTTP `429 Too Many Requests`, including a clear retry timeframe derived from `Retry-After` (header) or equivalent backend-provided metadata.

## Dependent Tasks
- [US_015 - Implement login rate limiting]

## Impacted Components
- [MODIFY | app/src/pages/LoginPage.tsx | Detect `429` responses from `POST /api/v1/auth/login` and display a retry timeframe message]
- [MODIFY | app/src/__tests__/visual/login.spec.js | Update mocked `429` response to include `Retry-After` header and/or details so UI screenshot coverage validates the new message]

## Implementation Plan
- Update login error handling logic:
  - When `POST /api/v1/auth/login` returns `429`, extract retry timeframe:
    - Prefer `Retry-After` header.
    - Support both common forms:
      - Integer seconds (e.g., `Retry-After: 60`)
      - HTTP date (e.g., `Retry-After: Wed, 21 Oct 2015 07:28:00 GMT`)
  - Convert retry timeframe into user-friendly messaging:
    - If seconds-based and small, show a relative timeframe (e.g., “Try again in 30 seconds.”)
    - If long (e.g., 30 minutes), show minutes and/or a local-time retry timestamp.
  - Keep messaging safe:
    - Do not reveal whether the email exists.
    - Do not display limiter partition keys or IP.
- Include support/assistance option inside the message:
  - Provide a consistent, non-invasive “Contact support/administrator” link or instruction aligned to existing login page guidance.
- Update Playwright visual test mocks:
  - For the `ratelimited` mock branch, add the expected `Retry-After` header and keep error envelope stable.

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | app/src/pages/LoginPage.tsx | Display a rate limiting message with retry timeframe when receiving HTTP 429 from login endpoint |
| MODIFY | app/src/__tests__/visual/login.spec.js | Add `Retry-After` behavior in the mocked 429 response to validate UI rendering |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Retry-After
- https://developer.mozilla.org/en-US/docs/Web/API/Headers/get

## Build Commands
- npm --prefix .\app run build
- npm --prefix .\app run test
- npm --prefix .\app run test:e2e

## Implementation Validation Strategy
- [Manual] Submit login repeatedly until HTTP 429 is returned and confirm retry timeframe is shown.
- [Automated] Run Playwright login visual tests and confirm screenshots capture the rate limit state.

## Implementation Checklist
- [x] Detect HTTP 429 from `POST /api/v1/auth/login`
- [x] Parse `Retry-After` header (seconds or date format)
- [x] Render a user-friendly retry timeframe (relative and/or local-time)
- [x] Include contact support/administrator option in the message
- [x] Update Playwright mocked response to include `Retry-After` and validate screenshots

## Design Reference

## UI Impact Assessment
**Has UI Changes**: [ ] Yes [ ] No
- If NO: Skip this design reference section entirely
- If YES: Complete all applicable sections below

## User Story Design Context
**Story ID**: US-[017]
**Story Title**: Display rate limit and lockout messages in UI
**UI Impact Type**: UI Enhancement

### Design Source References
- **Design System**: .propel/context/docs/designsystem.md
- **Screen Spec**: .propel/context/docs/figma_spec.md

### Screen-to-Design Mappings
| Screen/Feature | Image File | Image Path | Description | Implementation Priority |
|---------------|------------|------------|-------------|----------------------|
| Login rate limit messaging | N/A | N/A | Actionable rate limit message with retry timeframe and support option (UXR-010) | High |

### Task Design Mapping
```yaml
TASK_001:
  title: "Login - rate limit message with retry timeframe"
  ui_impact: true
  visual_references:
    figma_spec_refs: ["SCR-001 Login", "UXR-010"]
  components_affected:
    - LoginPage
  visual_validation_required: true
```

### Accessibility Requirements
- **WCAG Level**: AA
- **Screen Reader**: Message uses `role="alert"` or `aria-live` to announce retry timeframe
- **Keyboard Navigation**: Support link is keyboard reachable and has clear focus state
