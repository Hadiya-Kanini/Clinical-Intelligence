# Task - [TASK_001]

## Requirement Reference
- User Story: [us_038]
- Story Location: [.propel/context/tasks/us_038/us_038.md]
- Acceptance Criteria: 
    - [Given a user creation request, When email already exists, Then the request is rejected with error.]
    - [Given email uniqueness check, When performed, Then it is case-insensitive (User@Example.com = user@example.com).]
    - [Given duplicate detection, When triggered, Then the error message does not reveal existing user details.]

## Task Overview
Ensure user creation rejects duplicate email addresses in a **case-insensitive** manner.

This story is expected to apply to the **admin-only user creation flow** introduced in `US_037` (`POST /api/v1/admin/users`). The endpoint must:
- normalize incoming email using `EmailValidation.Normalize` (trim + lowercase)
- reject duplicates deterministically with a stable conflict error
- remain safe under race conditions by also mapping DB unique-constraint violations to the same conflict response

## Dependent Tasks
- [US_037 TASK_001 - Backend admin create Standard user endpoint]
- [US_038 TASK_002 - Database case-insensitive unique constraint for email] (DB enforcement; required for race-condition safety and defense-in-depth)

## Impacted Components
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Update `POST /api/v1/admin/users` to normalize email and reject duplicates case-insensitively with safe conflict response]

## Implementation Plan
- Ensure request email handling is centralized and consistent:
  - Validate via `EmailValidation.ValidateWithDetails(request.Email)`.
  - Use `EmailValidation.Normalize(request.Email)` to derive the canonical email used for:
    - querying for duplicates
    - persistence (stored email)
- Implement duplicate checks in the user creation endpoint:
  - Query `dbContext.Users.IgnoreQueryFilters()` for any record where `u.Email == normalizedEmail`.
  - Treat soft-deleted users as **still reserving** the email (do not allow reuse) to preserve account uniqueness and reduce identity confusion.
  - If a record is found, return `ApiErrorResults.Conflict(...)` with:
    - code: `duplicate_email`
    - message: a stable message such as "A user with this email already exists."
    - details: do not include user id, status, role, or any existing account details
- Handle race conditions / concurrent requests:
  - Wrap `SaveChangesAsync` in a try/catch.
  - If a `DbUpdateException` occurs due to the email unique constraint, map to the same `duplicate_email` conflict response.
  - Do not attempt to distinguish "who" owns the email.

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Enforce case-insensitive email uniqueness during user creation, including race-condition-safe conflict handling |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/ef/core/modeling/indexes
- https://learn.microsoft.com/en-us/ef/core/saving/transactions

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Manual/API] Create a user via admin endpoint, then attempt to create another user with the same email in a different case and verify:
  - `409 Conflict` (or your standardized conflict status)
  - stable error code/message
  - no leakage of existing user details
- [Concurrency] Send two near-simultaneous create requests for the same email and verify exactly one succeeds.

## Implementation Checklist
- [x] Normalize request email using `EmailValidation.Normalize` before querying/persisting
- [x] Add case-insensitive duplicate check (normalized equality)
- [x] Decide and implement soft-delete behavior (default: email remains reserved even if soft-deleted)
- [x] Map duplicates to stable `409 Conflict` response (`duplicate_email`)
- [x] Catch unique constraint `DbUpdateException` and map to the same conflict response
- [ ] Validate behavior with manual API calls (including different-case duplicates)
