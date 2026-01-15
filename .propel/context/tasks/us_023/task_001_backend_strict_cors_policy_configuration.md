# Task - [TASK_001]

## Requirement Reference
- User Story: [us_023]
- Story Location: [.propel/context/tasks/us_023/us_023.md]
- Acceptance Criteria: 
    - [Given a cross-origin request, When processed, Then only the configured frontend origin is allowed.]
    - [Given CORS configuration, When credentials are needed, Then Access-Control-Allow-Credentials is set to true.]
    - [Given preflight requests (OPTIONS), When received, Then appropriate CORS headers are returned.]
    - [Given an unauthorized origin, When a request is made, Then it is rejected with appropriate CORS error.]

## Task Overview
Implement a strict, configuration-driven CORS policy for the API that allows only the approved frontend origin(s) and supports credentialed requests (cookies). Replace the current hardcoded localhost origins in the existing CORS policy with configuration-based allowed origins and add startup validation to prevent insecure defaults. Estimated effort: ~4-6 hours.

## Dependent Tasks
- [US_011 - Implement JWT authentication with HttpOnly cookies]

## Impacted Components
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Replace hardcoded CORS origins with config-driven allowed origins; ensure credentials are enabled; keep middleware ordering correct]
- [CREATE | Server/ClinicalIntelligence.Api/Configuration/CorsOptions.cs | Strongly-typed configuration model for CORS allowed origin(s) and policy name constants]
- [MODIFY | .env.example | Add environment variable template entry for approved frontend origin(s) used by CORS configuration]

## Implementation Plan
- Define a configuration model for CORS:
  - Add a `CorsOptions` class (similar to existing configuration patterns like `RateLimitingOptions`).
  - Bind allowed origin(s) from configuration (environment variables supported via `builder.Configuration.AddEnvironmentVariables()`).
  - Decide on supported shapes:
    - Single origin (e.g., `CORS_FRONTEND_ORIGIN`) or
    - Multiple origins (e.g., `CORS_ALLOWED_ORIGINS` as a delimited string) to support staging vs production.
- Update API startup to use strict CORS configuration:
  - Replace the current `policy.WithOrigins("http://localhost:5173", ...)` list with the configured origin list.
  - Ensure `AllowCredentials()` remains enabled (required for cookie auth).
  - Ensure `AllowAnyHeader()` and `AllowAnyMethod()` remain enabled unless policy hardening is explicitly required elsewhere.
- Add validation and secure defaults:
  - Fail fast (startup exception) if no allowed origin is configured for non-development environments.
  - Ensure `AllowAnyOrigin()` is not used (incompatible with `AllowCredentials()` and insecure).
- Middleware ordering verification:
  - Confirm `app.UseCors(...)` remains early enough in the pipeline to handle preflight and attach headers.
  - Confirm authentication/authorization behavior is not affected.

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Replace hardcoded frontend origins with configuration-driven list; validate presence for non-dev; keep `AllowCredentials()` enabled; keep `UseCors("AllowFrontend")` policy wiring | 
| CREATE | Server/ClinicalIntelligence.Api/Configuration/CorsOptions.cs | Provide configuration binding model and constants for the CORS policy name and allowed origins key(s) |
| MODIFY | .env.example | Add template value(s) for CORS allowed origin(s) (e.g., `CORS_FRONTEND_ORIGIN=` or `CORS_ALLOWED_ORIGINS=`) |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/security/cors
- https://cheatsheetseries.owasp.org/cheatsheets/Cross-Site_Request_Forgery_Prevention_Cheat_Sheet.html

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Manual] Send a request with `Origin: <approved-origin>` and verify `Access-Control-Allow-Origin: <approved-origin>` and `Access-Control-Allow-Credentials: true` are present.
- [Manual] Send a request with `Origin: <unapproved-origin>` and verify CORS headers are not returned.
- [Manual] Send an `OPTIONS` preflight request with `Origin` and `Access-Control-Request-Method` headers and verify CORS preflight headers are returned for approved origin.

## Implementation Checklist
- [ ] Add strongly-typed CORS configuration model (`CorsOptions`) and decide how origin(s) are provided via env/config
- [ ] Replace hardcoded origins in `Program.cs` with configured origin(s)
- [ ] Ensure credentials support remains enabled (`AllowCredentials()`)
- [ ] Implement startup validation to avoid empty/unsafe configuration in non-development environments
- [ ] Verify middleware ordering and preflight behavior remains correct
- [ ] Update `.env.example` to document required CORS origin configuration
