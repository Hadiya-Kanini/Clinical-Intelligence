# Task - [TASK_004]

## Requirement Reference
- User Story: [us_035]
- Story Location: [.propel/context/tasks/us_035/us_035.md]
- Acceptance Criteria: 
    - [Given a Standard User, When they attempt to access admin API endpoints, Then they receive 403 Forbidden.]
    - [Given a Standard User, When they navigate to admin UI routes, Then they are redirected to their dashboard.]
    - [Given the navigation menu, When displayed to Standard Users, Then admin menu items are not visible.]

## Task Overview
Add automated coverage validating that Standard users are blocked from admin-only backend endpoints and admin-only UI routes.

This task ensures the access-control behavior remains stable over time and guards against regressions (Broken Access Control).

## Dependent Tasks
- [US_035 TASK_001] (Backend admin endpoint authorization and 403)
- [US_035 TASK_003] (Frontend admin route guard + nav visibility)

## Impacted Components
- [MODIFY | Server/ClinicalIntelligence.Api.Tests/* | Add backend integration tests for 401/403/200 behavior on admin-only endpoints]
- [MODIFY | app/src/__tests__/* | Add/extend frontend tests to validate route guard behavior and navigation visibility]

## Implementation Plan
- Backend tests (API integration):
  - Extend or add a test class to validate:
    - unauthenticated -> `401` for admin-only endpoints
    - authenticated Standard -> `403` for admin-only endpoints
    - authenticated Admin -> `200` for admin-only endpoints
  - Start with concrete endpoints that exist today:
    - `/health/db`
    - `/health/db/pool`
  - Leverage existing cookie-based test patterns (e.g., `CookieAuthenticationTests`) for authenticated calls.
- Frontend tests:
  - Add Playwright E2E test(s) to validate:
    - Standard user cannot access `/admin` and `/admin/users` (redirects to `/dashboard`)
    - Admin navigation links are not present for Standard users

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api.Tests/DatabaseHealthEndpointTests.cs (or new test file) | Add assertions for RBAC: Standard -> 403, Admin -> 200, unauthenticated -> 401 |
| MODIFY | app/src/__tests__/visual/* | Add E2E/visual tests that validate admin route guard redirects and admin nav visibility |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests
- https://playwright.dev/docs/intro

## Build Commands
- dotnet test .\\Server\\ClinicalIntelligence.Api.Tests
- npm --prefix .\\app run test
- npm --prefix .\\app run test:e2e

## Implementation Validation Strategy
- [Automated] Backend test suite passes with new 401/403/200 assertions.
- [Automated] Frontend E2E tests pass verifying redirect and nav visibility behavior.

## Implementation Checklist
- [ ] Add backend integration tests for 401/403/200 across admin-only endpoints
- [ ] Add frontend E2E tests for admin route guarding and redirects
- [ ] Add frontend assertions for admin menu items not being visible to Standard users
- [ ] Ensure tests are deterministic and do not rely on shared state
