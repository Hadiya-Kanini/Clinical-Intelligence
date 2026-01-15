# Task - [TASK_001]

## Requirement Reference
- User Story: [us_022]
- Story Location: [.propel/context/tasks/us_022/us_022.md]
- Acceptance Criteria: 
    - [Given CSRF protection, When implemented, Then tokens are generated per-session and validated server-side.]
    - [Given a state-changing request (POST, PUT, DELETE), When processed, Then CSRF token validation is required.]
    - [Given a request without valid CSRF token, When submitted, Then the API returns 403 Forbidden.]

## Task Overview
Implement server-side CSRF protection for cookie-authenticated sessions by generating a per-session CSRF token, persisting it server-side, exposing a safe token retrieval endpoint, and enforcing validation on state-changing API requests. Estimated effort: ~6-8 hours.

## Dependent Tasks
- [US_011 - Implement JWT authentication with HttpOnly cookies]

## Impacted Components
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Register CSRF services, add CSRF middleware, and enforce CSRF validation for state-changing endpoints]
- [MODIFY | Server/ClinicalIntelligence.Api/Domain/Models/Session.cs | Persist CSRF token metadata at the session level (per-session token)]
- [MODIFY | Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs | Persist new session CSRF fields and update EF model configuration as needed]
- [CREATE | Server/ClinicalIntelligence.Api/Middleware/CsrfProtectionMiddleware.cs | Validate CSRF header token for state-changing requests and return standardized 403 error on failure]
- [CREATE | Server/ClinicalIntelligence.Api/Contracts/CsrfTokenResponse.cs | Response contract for CSRF token retrieval endpoint]
- [CREATE/MODIFY | Server/ClinicalIntelligence.Api/Migrations/* | Add EF Core migration for new session CSRF fields]

## Implementation Plan
- Define CSRF token strategy (cookie-auth scenario):
  - Generate a cryptographically strong per-session token on successful login (or on first token request).
  - Persist a server-side representation (e.g., hash) tied to the `Session.Id`.
  - Define token expiration behavior aligned to session expiry (edge case: token expires mid-session).
- Add CSRF token retrieval endpoint:
  - Add `GET /api/v1/auth/csrf` (requires authorization) that returns a token value suitable for sending in a header.
  - Ensure it never sets/returns sensitive auth state beyond the CSRF token.
- Enforce CSRF validation:
  - Add middleware to enforce CSRF for state-changing methods (`POST`, `PUT`, `PATCH`, `DELETE`) when cookie-based auth is in use.
  - Validate `X-CSRF-TOKEN` (or equivalent header) against the persisted per-session value.
  - On missing/invalid/expired token, return `403` with a standardized error payload.
- Handle multi-tab behavior:
  - Ensure CSRF token remains stable per-session (so multiple tabs can share it) or provide deterministic refresh semantics.
- Update OpenAPI documentation:
  - Document required header for state-changing endpoints in Swagger (at least for `/api/v1/auth/logout` currently).

## Current Project State
- Session model updated with CsrfTokenHash field
- CSRF middleware created for state-changing request validation
- CSRF token endpoint implemented at GET /api/v1/auth/csrf
- EF Core migration added for CsrfTokenHash column
- Login endpoint generates CSRF token on session creation

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Register and wire CSRF token generation + enforcement for state-changing endpoints |
| MODIFY | Server/ClinicalIntelligence.Api/Domain/Models/Session.cs | Add per-session CSRF token storage fields (e.g., hash + timestamps) |
| MODIFY | Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs | Map new CSRF session fields and ensure migrations snapshot updates |
| CREATE | Server/ClinicalIntelligence.Api/Middleware/CsrfProtectionMiddleware.cs | Validate CSRF header for state-changing requests and return 403 on failure |
| CREATE | Server/ClinicalIntelligence.Api/Contracts/CsrfTokenResponse.cs | Define response shape for CSRF token retrieval |
| CREATE/MODIFY | Server/ClinicalIntelligence.Api/Migrations/* | EF Core migration(s) for CSRF fields on session |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://cheatsheetseries.owasp.org/cheatsheets/Cross-Site_Request_Forgery_Prevention_Cheat_Sheet.html
- https://learn.microsoft.com/en-us/aspnet/core/security/anti-request-forgery

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Manual] Login, retrieve CSRF token, call `POST /api/v1/auth/logout` with and without the CSRF header, confirm 200 vs 403.
- [Manual] Open two tabs in same session; confirm both can use the same CSRF token successfully.

## Implementation Checklist
- [x] Define CSRF token format, header name, and expiration strategy
- [x] Persist per-session CSRF token server-side (hash) and update schema via migration
- [x] Implement `GET /api/v1/auth/csrf` token retrieval endpoint
- [x] Implement CSRF enforcement middleware for state-changing requests
- [x] Ensure invalid/missing token returns standardized 403 response
- [x] Update Swagger/OpenAPI to document CSRF header requirement
- [x] Validate multi-tab and token-expiry behaviors
