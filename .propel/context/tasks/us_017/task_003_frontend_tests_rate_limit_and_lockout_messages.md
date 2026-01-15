# Task - [TASK_003]

## Requirement Reference
- User Story: [us_017] (extracted from input)
- Story Location: [.propel/context/tasks/us_017/us_017.md]
- Acceptance Criteria: 
    - [Given the API returns HTTP 429, When the UI receives the response, Then it displays a message with the retry timeframe (UXR-010).]
    - [Given an account is locked, When the login attempt fails with lockout error, Then the UI displays the remaining lockout time and reason (UXR-009).]
    - [Given a lockout message is displayed, When the lockout period expires, Then the UI allows retry without requiring page refresh.]
    - [Given rate limit or lockout occurs, When the message is displayed, Then it includes contact support option for assistance.]

## Task Overview
Add and/or update frontend automated tests to validate the rate limit and lockout UX on the login page, including retry timeframe rendering, lockout countdown behavior, and the auto-retry transition once lockout expires.

## Dependent Tasks
- [TASK_001 - Frontend display rate limit retry timeframe]
- [TASK_002 - Frontend display lockout remaining time, reason, and auto retry]

## Impacted Components
- [MODIFY | app/src/__tests__/visual/login.spec.js | Expand mocks and assertions for 429 + lockout states, ensuring stable screenshots]
- [CREATE/MODIFY | app/src/test/* | Add Playwright e2e coverage for lockout expiry enabling retry without refresh (project-appropriate location)]

## Implementation Plan
- Visual validation (Playwright screenshot-based):
  - Ensure the mocked `429` branch includes `Retry-After` header and verify the UI message is visible.
  - Ensure the mocked `account_locked` branch includes deterministic `details` so the unlock time display is stable for screenshots.
- Behavioral validation (Playwright E2E):
  - Add a test that simulates a short lockout window to avoid long waits:
    - Mock lockout response with `remaining_seconds:2` (or similar) and ensure the countdown is shown.
    - Wait for expiry and assert the UI transitions back to a retry-enabled state without a reload.
  - Add a test that validates the support option exists in both error modes.
- Keep tests deterministic:
  - Avoid relying on real time zones when asserting timestamps; prefer relative countdown assertions when possible.

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | app/src/__tests__/visual/login.spec.js | Update mocks for 429 + lockout to include required metadata and validate UI states via screenshots |
| CREATE/MODIFY | app/src/test/* | Add Playwright e2e coverage for lockout expiry and retry enablement |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://playwright.dev/docs/network
- https://playwright.dev/docs/test-assertions

## Build Commands
- npm --prefix .\app run test:e2e

## Implementation Validation Strategy
- [Automated] Run Playwright suite and confirm:
  - Rate limit state renders with retry timeframe
  - Lockout state renders with remaining time + reason
  - UI transitions to retry-enabled state after lockout expiry without refresh

## Implementation Checklist
- [x] Update Playwright mock: 429 includes `Retry-After`
- [x] Update Playwright mock: account locked includes deterministic timeframe details
- [x] Add test for lockout expiry enabling retry without refresh
- [x] Add assertion for presence of support option in both modes
- [x] Ensure tests are stable across time zones and machines

## Design Reference

## UI Impact Assessment
**Has UI Changes**: [ ] Yes [ ] No
- If NO: Skip this design reference section entirely
- If YES: Complete all applicable sections below

## User Story Design Context
**Story ID**: US-[017]
**Story Title**: Display rate limit and lockout messages in UI
**UI Impact Type**: Component Update

### Design Source References
- **Screen Spec**: .propel/context/docs/figma_spec.md

### Task Design Mapping
```yaml
TASK_003:
  title: "Tests - validate rate limit and lockout messaging"
  ui_impact: true
  visual_references:
    figma_spec_refs: ["SCR-001 Login", "UXR-009", "UXR-010"]
  components_affected:
    - LoginPage
  visual_validation_required: true
```
