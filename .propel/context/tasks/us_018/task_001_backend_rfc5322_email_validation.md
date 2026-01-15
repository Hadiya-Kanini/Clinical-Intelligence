# Task - [TASK_001]

## Requirement Reference
- User Story: [us_018] (extracted from input)
- Story Location: [.propel/context/tasks/us_018/us_018.md]
- Acceptance Criteria: 
    - [Given a user enters an email address, When validation runs, Then RFC 5322 compliant regex patterns are used.]
    - [Given an invalid email format, When validation fails, Then a clear error message indicates the specific issue.]
    - [Given email validation, When implemented, Then it validates on both frontend and backend (security).]
    - [Given edge-case valid emails (e.g., user+tag@domain.com), When entered, Then they are accepted as valid.]

## Task Overview
Implement backend email format validation using an RFC 5322-compliant regex in a centralized helper, then apply it to API entry points that accept user emails (starting with `POST /api/v1/auth/login`). Return a standardized `400 Bad Request` with clear, structured validation details when email format is invalid.

## Dependent Tasks
- [N/A]

## Impacted Components
- [CREATE | Server/ClinicalIntelligence.Api/Validation/EmailValidation.cs | Central RFC 5322 email regex + helper methods (e.g., normalize + validate)]
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Validate login email format before querying DB; return standardized invalid-input response on failure]
- [MODIFY | Server/ClinicalIntelligence.Api/Migrations/20260115100000_SeedStaticAdminAccount.cs | Align ADMIN_EMAIL validation to reuse the same validator (avoid drift between env validation and API validation)]

## Implementation Plan
- Introduce a single source of truth:
  - Create `EmailValidation` utility with:
    - A single RFC 5322-compliant regex (documented in code via constant naming, not comments)
    - A `Normalize(string)` helper (trim + lower-invariant) and a `IsValid(string)` helper
  - Make validation decisions explicit:
    - Support common valid emails such as `user+tag@domain.com`
    - Decide explicitly how to handle internationalized domains (IDN):
      - Prefer accepting ASCII + punycode domains; reject raw unicode domains unless explicitly supported
- Apply validation to login endpoint:
  - After required-field checks, validate `email` format.
  - On invalid format, return `ApiErrorResults.BadRequest`:
    - `code`: `invalid_input`
    - `message`: "Email format is invalid."
    - `details`: include a stable value such as `email:invalid_format`
  - Ensure behavior is safe:
    - Do not leak user existence and do not perform DB query when the email is invalid.
- Align static admin seed validation:
  - Update seeding migration to validate `ADMIN_EMAIL` via the same `EmailValidation` helper (keeps EP-004 validation consistent).

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api/Validation/EmailValidation.cs | Central RFC 5322 email validation helper used by endpoints and seeding |
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Enforce RFC email validation for login before DB lookup; return `400` with stable details |
| MODIFY | Server/ClinicalIntelligence.Api/Migrations/20260115100000_SeedStaticAdminAccount.cs | Reuse the same validator for `ADMIN_EMAIL` validation |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://www.rfc-editor.org/rfc/rfc5322

## Build Commands
- dotnet build .\Server\ClinicalIntelligence.Api\ClinicalIntelligence.Api.csproj
- dotnet test .\Server\ClinicalIntelligence.Api.Tests\ClinicalIntelligence.Api.Tests.csproj

## Implementation Validation Strategy
- [Manual] Submit `POST /api/v1/auth/login` with an invalid email (e.g., `not-an-email`) and confirm `400` includes stable validation details.
- [Manual] Verify `user+tag@domain.com` is accepted as a valid format (format-only; auth may still fail).
- [Automated] Add/adjust backend tests (see TASK_003) to validate edge cases.

## Implementation Checklist
- [x] Create centralized `EmailValidation` helper and RFC 5322 regex
- [x] Validate email format in `/api/v1/auth/login` prior to DB query
- [x] Return standardized `400` response for invalid email format with stable `details`
- [x] Ensure common valid emails (`+tag`) pass
- [x] Decide/document IDN handling behavior (punycode vs unicode)
- [x] Update static admin seeding validation to reuse the same helper
- [x] Verify no sensitive info leakage in validation responses
