# Task - [TASK_012_004]

## Requirement Reference
- User Story: [us_012] (extracted from input)
- Story Location: [.propel/context/tasks/us_012/us_012.md]
- Acceptance Criteria: 
    - [Given a user is authenticated, When 15 minutes pass without any user activity, Then the session is automatically terminated.]
    - [Given a session is terminated due to inactivity, When the user attempts any action, Then they are redirected to the login page with a session expired message.]
    - [Given a user performs any action (API call, navigation), When the action is processed, Then the session's last activity timestamp is updated.]
    - [Given a session exists, When the Backend API processes requests, Then it tracks session state server-side for revocation capability.]

## Task Overview
Add automated backend tests to validate server-side session tracking behavior, including session creation, last activity updates, inactivity timeout enforcement, and logout-driven revocation.

## Dependent Tasks
- [TASK_012_001 - Backend session tracking and inactivity timeout]

## Impacted Components
- [CREATE: Server/ClinicalIntelligence.Api.Tests/Integration/SessionInactivityTimeoutTests.cs]
- [MODIFY: Server/ClinicalIntelligence.Api.Tests/Integration/ (existing test infrastructure as needed)]

## Implementation Plan
- [Create integration tests using `WebApplicationFactory<Program>` to exercise real HTTP endpoints.]
- [Login and capture the issued JWT.]
- [Call a protected endpoint (e.g., `/api/v1/ping`) and verify success, and that `sessions.LastActivityAt` is updated in the database.]
- [Simulate inactivity by directly updating the session row in the database to set `LastActivityAt` in the past beyond the timeout threshold, then call a protected endpoint and assert the API returns 401 with a session-expired error code.]
- [Verify `/api/v1/auth/logout` revokes the session (sets `IsRevoked = true`) and subsequent requests fail with 401.]
- [Keep tests skippable when PostgreSQL is not available, following existing integration test patterns in the suite.]
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api.Tests/Integration/SessionInactivityTimeoutTests.cs | Integration coverage for session create/update/expire/revoke scenarios |
| MODIFY | Server/ClinicalIntelligence.Api.Tests/Mocks/* | Extend helpers if required to query/update sessions for test setup |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-8.0

## Build Commands
- dotnet test .\Server\ClinicalIntelligence.Api.Tests\ClinicalIntelligence.Api.Tests.csproj

## Implementation Validation Strategy
- []

## Implementation Checklist
- [ ] Add an integration test: login creates a session record.
- [ ] Add an integration test: authenticated request updates `LastActivityAt`.
- [ ] Add an integration test: session inactivity beyond threshold returns 401 with session-expired code.
- [ ] Add an integration test: logout revokes session and blocks subsequent requests.
- [ ] Ensure tests are skippable when PostgreSQL is not configured.
- [ ] Ensure tests do not log secrets or tokens.
