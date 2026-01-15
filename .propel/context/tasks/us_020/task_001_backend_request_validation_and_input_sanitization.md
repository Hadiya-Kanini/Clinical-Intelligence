# Task - [TASK_001]

## Requirement Reference
- User Story: [us_020]
- Story Location: [.propel/context/tasks/us_020/us_020.md]
- Acceptance Criteria: 
    - [Given any user input, When processed by the backend, Then it is sanitized against SQL injection attacks.]
    - [Given suspicious input patterns, When detected, Then the request is rejected with appropriate error (FR-009g).]

## Task Overview
Harden backend request input handling to reduce injection risk by applying consistent validation/sanitization rules at the API boundary (middleware + endpoint-level normalization), while avoiding false positives for legitimate clinical free-text inputs.

## Dependent Tasks
- [N/A]

## Impacted Components
- [MODIFY | Server/ClinicalIntelligence.Api/Middleware/RequestValidationMiddleware.cs | Centralize and strengthen request validation (query + headers + scoped body checks) and ensure consistent `invalid_input` error responses]
- [CREATE | Server/ClinicalIntelligence.Api/Services/Security/InputValidationPolicy.cs | Reusable input validation helpers (e.g., suspicious pattern detection, length constraints, normalization helpers) to keep middleware SRP-focused]
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Register any new security validation service(s) with DI and keep pipeline order correct]

## Implementation Plan
- Confirm current request validation behavior and gaps:
  - Query parameter scanning is present in `RequestValidationMiddleware`.
  - POST/PUT JSON Content-Type and body size limits are enforced.
- Introduce a dedicated validation policy/service:
  - Implement a small `InputValidationPolicy` helper that contains the pattern checks and any normalization rules.
  - Keep it deterministic and easy to test.
- Apply validation in the correct places:
  - Continue validating query parameters (primary injection surface for filtering/search endpoints).
  - Validate selected headers only if relevant (e.g., avoid blocking common User-Agent strings).
  - For JSON bodies, avoid blanket scanning of all string fields to prevent false positives (password fields and clinical note content may legitimately contain suspicious substrings).
  - Prefer endpoint-level validation/normalization for known structured fields (e.g., trim/lowercase email already done in `/auth/login`).
- Standardize rejection behavior (FR-009g):
  - Ensure the middleware rejects suspicious requests using `ApiErrorResults.BadRequest("invalid_input", ...)` with a safe, non-leaky message.
  - Add logging that records the key name and request path, but does not persist raw sensitive values.

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api/Services/Security/InputValidationPolicy.cs | Implement reusable validation helpers (pattern detection + safe normalization utilities) used by middleware/endpoints |
| MODIFY | Server/ClinicalIntelligence.Api/Middleware/RequestValidationMiddleware.cs | Refactor to use `InputValidationPolicy` and clarify what is validated (query/body/headers) to balance security and false positives |
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Wire new validation service(s) into DI if required |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://owasp.org/www-project-top-ten/

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Manual] Submit requests containing suspicious query patterns (e.g., `?q=union%20select`) and confirm `400 invalid_input`.
- [Manual] Confirm legitimate inputs (email/password for login, free text fields when introduced) are not rejected due to naive substring checks.
- [Security] Confirm logs do not contain raw user input values for rejected requests.

## Implementation Checklist
- [ ] Implement `InputValidationPolicy` for suspicious pattern detection and safe normalization helpers
- [ ] Refactor `RequestValidationMiddleware` to use the policy and clearly scope validation to reduce false positives
- [ ] Ensure middleware returns standardized `invalid_input` error for detected suspicious patterns (FR-009g)
- [ ] Ensure logging is informative (path + key) but avoids leaking raw input values
- [ ] Validate behavior for `/auth/login` (do not block passwords containing special characters or substrings)
- [ ] Confirm existing JSON Content-Type and request size protections remain intact
