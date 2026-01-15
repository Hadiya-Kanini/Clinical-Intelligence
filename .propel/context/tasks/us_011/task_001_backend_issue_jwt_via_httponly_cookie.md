# Task - TASK_001

## Requirement Reference
- User Story: us_011
- Story Location: .propel/context/tasks/us_011/us_011.md
- Acceptance Criteria: 
    - AC-1: Given a user submits valid email and password, When the Backend API validates credentials, Then a JWT access token with 15-minute expiration is generated.
    - AC-2: Given a JWT is generated, When the response is sent, Then the token is stored in an HttpOnly, Secure cookie (not accessible via JavaScript).
    - AC-3: Given a user has a valid JWT cookie, When they make authenticated requests, Then the Backend API validates the token and authorizes the request.
    - AC-4: Given a JWT is invalid or expired, When the user makes a request, Then the API returns 401 Unauthorized.

## Task Overview
Update the backend authentication implementation to issue the JWT access token via an HttpOnly cookie and to accept JWTs from that cookie for all authenticated endpoints. Align JWT expiration to 15 minutes, implement cookie security attributes, and ensure the existing minimal API endpoints (`/api/v1/auth/login`, `/api/v1/auth/logout`, `/api/v1/auth/me`) support the cookie-based session model.
Estimated Effort: 8 hours

## Dependent Tasks
- .propel/context/tasks/us_123/task_002_backend_authenticate_seeded_admin_and_issue_jwt_role_claim.md (TASK_002)

## Impacted Components
- Server/ClinicalIntelligence.Api/Program.cs
- Server/ClinicalIntelligence.Api/Configuration/SecretsOptions.cs
- Server/ClinicalIntelligence.Api/Contracts/LoginRequest.cs

## Implementation Plan
- Define cookie-based access token storage for the login flow:
  - Choose a single cookie name (e.g., `ci_access_token`) and apply it consistently for set, read, and clear.
  - On successful login, set the cookie using `HttpContext.Response.Cookies.Append(...)`.
  - Configure cookie attributes:
    - `HttpOnly = true`
    - `Secure = true` (production); define explicit development behavior if HTTPS is not available locally
    - `SameSite` policy chosen to support the app’s frontend-backend hosting model
    - Set `MaxAge`/`Expires` aligned to the JWT expiry
    - Set `Path = "/"` (or `"/api"` if restricting is desired)
- Align access token expiration to 15 minutes:
  - Update `SecretsOptions.JwtExpirationMinutes` default to `15` and/or introduce an explicit configuration override for token lifetime.
  - Ensure the issued `exp` claim matches the configured expiration.
- Configure JWT bearer authentication to read the token from cookie:
  - Update the `AddJwtBearer(...)` configuration to read the JWT from the cookie in `JwtBearerEvents.OnMessageReceived`.
  - Keep standard `TokenValidationParameters` (issuer/audience/signing key/lifetime).
  - Ensure the authentication scheme continues to work for non-browser clients if required (e.g., allow `Authorization: Bearer` fallback).
- Update `/api/v1/auth/logout` to clear the cookie:
  - Use `HttpContext.Response.Cookies.Delete(...)` with matching cookie options so deletion works reliably.
- Ensure protected endpoints use cookie auth:
  - Verify `/api/v1/auth/me` and other `.RequireAuthorization()` endpoints succeed with cookie-only authentication.
  - Verify invalid/expired token behavior results in 401.
- Consider required CORS / credential support for the frontend:
  - If the UI is hosted on a different origin, add explicit CORS policy enabling credentials (`AllowCredentials`) and a constrained origin allowlist.

**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Configuration/SecretsOptions.cs | Update default token expiration to 15 minutes (or introduce explicit configuration override) to match US_011 requirements |
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Update JWT generation to use 15-minute expiry, set JWT cookie on `/api/v1/auth/login`, clear cookie on `/api/v1/auth/logout`, and configure `JwtBearerEvents.OnMessageReceived` to read token from cookie |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/aspnet/core/security/authentication/jwt-bearer
- https://learn.microsoft.com/aspnet/core/security/cors
- https://learn.microsoft.com/aspnet/core/fundamentals/app-state#cookies
- https://developer.mozilla.org/docs/Web/HTTP/Cookies

## Build Commands
- dotnet build Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj
- dotnet test Server/ClinicalIntelligence.Api.Tests/ClinicalIntelligence.Api.Tests.csproj

## Implementation Validation Strategy
- Call `/api/v1/auth/login` with valid credentials and confirm:
  - A cookie is set with `HttpOnly` and appropriate `Secure`/`SameSite` attributes.
  - The JWT `exp` is ~15 minutes from issuance (AC-1, AC-2).
- Call a protected endpoint (e.g., `/api/v1/auth/me` or `/api/v1/ping`) without `Authorization` header but with the cookie, and confirm 200 OK (AC-3).
- Call the same protected endpoint with an expired/invalid/tampered cookie value and confirm 401 Unauthorized (AC-4).
- Call `/api/v1/auth/logout` and confirm the cookie is removed and subsequent protected calls return 401.

## Implementation Checklist
- [x] Update JWT expiration to 15 minutes and confirm token `exp` aligns to config
- [x] Set JWT access token into an HttpOnly cookie on successful login
- [x] Configure JWT bearer authentication to read token from cookie (and optionally accept Authorization header fallback)
- [x] Clear JWT cookie on logout
- [x] Ensure `/api/v1/auth/me` and other protected endpoints authorize via cookie-based JWT
- [x] Add/adjust CORS policy if the frontend runs on a different origin and needs credentialed requests
- [x] Build and run backend tests
