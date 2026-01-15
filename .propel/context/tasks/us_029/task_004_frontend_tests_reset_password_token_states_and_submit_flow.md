# Task - [TASK_004]

## Requirement Reference
- User Story: [us_029]
- Story Location: [.propel/context/tasks/us_029/us_029.md]
- Acceptance Criteria: 
    - [Given I click the reset link in email, When the page loads, Then the token is validated before displaying the form.]
    - [Given an expired or invalid token, When detected, Then an error message is shown with option to request new reset.]

## Task Overview
Add Playwright UI test coverage for the Reset Password page to validate the new token-prevalidation flow and UX states: validating/loading, invalid/expired token, valid token showing form, and successful submission redirect.

## Dependent Tasks
- [US_029 TASK_003 - Frontend reset password token prevalidation and submit flow]

## Impacted Components
- [CREATE | app/src/__tests__/visual/resetPassword.spec.ts | Visual/UX tests for reset-password states using route mocking for validation/reset endpoints]

## Implementation Plan
- Add a new Playwright spec that:
  - Mocks `GET **/api/v1/auth/reset-password/validate?token=*` to simulate:
    - valid token response
    - invalid/expired token error response
  - Mocks `POST **/api/v1/auth/reset-password` to simulate:
    - success response
    - failure response
- Validate UI behavior:
  - Missing token renders invalid/expired state
  - Invalid token renders error message and "request new reset" navigation option
  - Valid token renders the form after validation completes
  - Successful submission shows success state and redirects to `/login`
- Capture screenshots for key states to protect layout across breakpoints.

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | app/src/__tests__/visual/resetPassword.spec.ts | Playwright tests covering reset-password token validation states and submit/redirect behavior, including screenshot assertions |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://playwright.dev/docs/network

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Automated] Run Playwright visual test suite and confirm the new reset-password tests are stable.

## Implementation Checklist
- [x] Add route mocks for `GET /api/v1/auth/reset-password/validate` (valid + invalid)
- [x] Add route mocks for `POST /api/v1/auth/reset-password` (success + failure)
- [x] Add test for missing token -> invalid/expired state
- [x] Add test for valid token -> form renders after validation
- [x] Add test for invalid token -> error UI + request new reset option
- [x] Add test for success -> redirect to `/login`

---

# Design Reference

## UI Impact Assessment
**Has UI Changes**: [x] Yes [ ] No

## User Story Design Context
**Story ID**: US-[029]
**Story Title**: Implement reset password page with token validation
**UI Impact Type**: Component Update

### Screen-to-Design Mappings
| Screen/Feature | Description | Implementation Priority |
|---------------|-------------|----------------------|
| Reset Password | Screenshot baselines for loading/invalid/valid/success states | Medium |

### Accessibility Requirements
- **WCAG Level**: AA (for tested UI states)
- **Keyboard Navigation**: Ensure form fields remain tabbable in valid-token state
