# Task - TASK_003

## Requirement Reference
- User Story: us_011
- Story Location: .propel/context/tasks/us_011/us_011.md
- Acceptance Criteria: 
    - AC-1: Given a user submits valid email and password, When the Backend API validates credentials, Then a JWT access token with 15-minute expiration is generated.
    - AC-2: Given a JWT is generated, When the response is sent, Then the token is stored in an HttpOnly, Secure cookie (not accessible via JavaScript).
    - AC-3: Given a user has a valid JWT cookie, When they make authenticated requests, Then the Backend API validates the token and authorizes the request.
    - AC-4: Given a JWT is invalid or expired, When the user makes a request, Then the API returns 401 Unauthorized.

## Task Overview
Update backend integration tests to validate cookie-based JWT authentication end-to-end, including verifying cookie issuance on login, cookie-based authorization on protected endpoints, and correct 401 behavior for invalid/expired cookies.
Estimated Effort: 6 hours

## Dependent Tasks
- .propel/context/tasks/us_011/task_001_backend_issue_jwt_via_httponly_cookie.md (TASK_001)

## Impacted Components
- Server/ClinicalIntelligence.Api.Tests/SeededAdminAuthenticationTests.cs
- Server/ClinicalIntelligence.Api.Tests/

## Implementation Plan
- Update login success tests to validate cookie issuance:
  - After calling `/api/v1/auth/login`, assert `Set-Cookie` exists with the expected cookie name.
  - Assert cookie attributes include `HttpOnly` and expected `SameSite`/`Secure` behavior.
- Validate cookie-based authentication on protected endpoints:
  - Capture the issued cookie and send it back on a subsequent request to a protected endpoint (e.g., `/api/v1/auth/me`).
  - Assert a 200 OK response and correct user payload.
- Validate missing/invalid cookie behavior:
  - Call protected endpoints without cookies and assert 401.
  - Send a malformed cookie value and assert 401.
- Validate expiration behavior:
  - Decode the JWT from the cookie (if the cookie is not encrypted) and assert token expiry is ~15 minutes.
  - If a short-lived test token configuration is introduced for tests, validate 401 after expiry.

**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api.Tests/SeededAdminAuthenticationTests.cs | Update assertions to validate `Set-Cookie` and to use cookie-based auth for follow-up requests (instead of reading JWT from JSON response) |
| CREATE | Server/ClinicalIntelligence.Api.Tests/CookieAuthenticationTests.cs | Add focused integration tests for cookie-required authorization and malformed cookie scenarios |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/aspnet/core/test/integration-tests
- https://developer.mozilla.org/docs/Web/HTTP/Headers/Set-Cookie

## Build Commands
- dotnet build Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj
- dotnet test Server/ClinicalIntelligence.Api.Tests/ClinicalIntelligence.Api.Tests.csproj

## Implementation Validation Strategy
- Run the test suite with a database available and seeded admin credentials configured:
  - Confirm login produces `Set-Cookie` with `HttpOnly` and correct expiry (AC-1, AC-2).
  - Confirm cookie alone authorizes access to protected endpoint (AC-3).
  - Confirm missing/malformed/expired token leads to 401 (AC-4).

## Implementation Checklist
- [x] Update seeded admin login test to assert `Set-Cookie` and capture cookie value
- [x] Add a test that calls `/api/v1/auth/me` using only the cookie and asserts 200
- [x] Add a test that calls a protected endpoint without cookie and asserts 401
- [x] Add a test for malformed/tampered cookie value returning 401
- [x] Validate token expiry duration is 15 minutes (or configuration equivalent)
- [x] Build and run backend test suite
