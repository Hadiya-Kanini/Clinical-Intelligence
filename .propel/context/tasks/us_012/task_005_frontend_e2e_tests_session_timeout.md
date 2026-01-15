# Task - [TASK_012_005]

## Requirement Reference
- User Story: [us_012] (extracted from input)
- Story Location: [.propel/context/tasks/us_012/us_012.md]
- Acceptance Criteria: 
    - [Given a user is authenticated, When 15 minutes pass without any user activity, Then the session is automatically terminated.]
    - [Given a session is terminated due to inactivity, When the user attempts any action, Then they are redirected to the login page with a session expired message.]

## Task Overview
Add frontend automated tests validating the session timeout UX and session-expired redirect behavior.

## Dependent Tasks
- [TASK_012_002 - Frontend inactivity timeout + session expired UX]
- [TASK_012_003 - Frontend API session-expired handling]

## Impacted Components
- [CREATE: app/src/test/sessionTimeout.spec.ts (or project-appropriate location)]
- [MODIFY: app/playwright.config.* (if needed)]

## Implementation Plan
- [Implement a Playwright test that exercises authenticated navigation and verifies timeout behavior.]
- [To avoid a 15-minute test duration, ensure the idle timeout is configurable in the app for test environments (e.g., set idle timeout to a few seconds via env var).]
- [Test cases:]
- [User becomes unauthenticated after configured idle duration without events.]
- [User is redirected to `/login` with a visible “session expired” message.]
- [Optional: verify that a user action after expiry triggers the redirect if the app is still on a protected page.]
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | app/src/test/sessionTimeout.spec.ts | Playwright e2e coverage for timeout and session-expired login messaging |
| MODIFY | app/playwright.config.* | Configure baseURL, timeouts, and environment for fast timeout tests |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://playwright.dev/docs/test-intro

## Build Commands
- npm --prefix .\app run test:e2e

## Implementation Validation Strategy
- []

## Implementation Checklist
- [ ] Make inactivity timeout configurable for tests (without changing production default).
- [ ] Add Playwright test for session-expired redirect.
- [ ] Assert login page shows session-expired messaging.
- [ ] Ensure tests are deterministic (no flakiness due to timing).
- [ ] Document any required test env var(s) in Playwright config.

## Design Reference

## UI Impact Assessment
**Has UI Changes**: [ ] Yes [ ] No
- If NO: Skip this design reference section entirely
- If YES: Complete all applicable sections below

## User Story Design Context
**Story ID**: US-[012]
**Story Title**: Implement session tracking and inactivity timeout
**UI Impact Type**: Component Update

### Design Source References
- **Screen Spec**: .propel/context/docs/figma_spec.md

### Task Design Mapping
```yaml
TASK_012_005:
  title: "E2E validate session timeout flow"
  ui_impact: true
  visual_references:
    figma_spec_refs: ["Session timeout - all authenticated"]
  components_affected:
    - LoginPage
    - AppShell
  visual_validation_required: true
```
