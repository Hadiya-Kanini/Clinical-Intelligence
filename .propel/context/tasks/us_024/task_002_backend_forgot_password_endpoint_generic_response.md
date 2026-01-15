# Task - [TASK_002]

## Requirement Reference
- User Story: [us_024]
- Story Location: [.propel/context/tasks/us_024/us_024.md]
- Acceptance Criteria: 
    - [Given a valid email is submitted, When processed, Then a confirmation message is displayed (FR-115b).]

## Task Overview
Add a backend endpoint to accept forgot-password requests so the frontend submission is actually processed. This task focuses on:
- validating request shape (email format),
- returning a non-enumerating response (generic success),
- establishing a stable contract for later tasks that will implement token generation and email delivery.

This intentionally does NOT implement full token generation / email sending; those belong to follow-up password reset stories (e.g., US_025 and related).

## Dependent Tasks
- [N/A] (Can be implemented independently)

## Impacted Components
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Add `POST /api/v1/auth/forgot-password` anonymous endpoint]
- [CREATE | Server/ClinicalIntelligence.Api/Contracts/Auth/ForgotPasswordRequest.cs | Request DTO `{ email }` for endpoint]

## Implementation Plan
- Add `POST /api/v1/auth/forgot-password` endpoint under the existing `var v1 = app.MapGroup("/api/v1");` group.
- Accept a request body containing email.
- Validate:
  - required email value (non-empty)
  - email format (use consistent validation approach aligned to FR-009b)
- Security/non-enumeration:
  - If email is syntactically valid, return HTTP `200` with a generic success payload regardless of whether the account exists (FR-009q).
  - Avoid returning details that confirm user existence.
- Contract stability:
  - Return a minimal success response to support UI messaging (e.g., `{ status: "ok" }`).
  - Leave hooks for future enhancement to generate tokens and send emails.

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Add anonymous `POST /api/v1/auth/forgot-password` endpoint with validation + generic response |
| CREATE | Server/ClinicalIntelligence.Api/Contracts/Auth/ForgotPasswordRequest.cs | DTO used by the new endpoint |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Manual/API] POST valid email and confirm 200.
- [Manual/API] POST invalid email and confirm 400 with a standardized error shape.
- [Security] POST non-existing vs existing email and confirm responses are indistinguishable.

## Implementation Checklist
- [ ] Create `ForgotPasswordRequest` contract DTO
- [ ] Add `POST /api/v1/auth/forgot-password` minimal API endpoint
- [ ] Validate required email and email format
- [ ] Return generic 200 response for syntactically valid emails (no enumeration)
- [ ] Ensure endpoint is anonymous (no auth required)
- [ ] Confirm standardized error response shape on validation failure
