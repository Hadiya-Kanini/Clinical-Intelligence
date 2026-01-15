# Task - [TASK_003]

## Requirement Reference
- User Story: [us_038]
- Story Location: [.propel/context/tasks/us_038/us_038.md]
- Acceptance Criteria: 
    - [Given a user creation request, When email already exists, Then the request is rejected with error.]
    - [Given email uniqueness check, When performed, Then it is case-insensitive (User@Example.com = user@example.com).]
    - [Given duplicate detection, When triggered, Then the error message does not reveal existing user details.]

## Task Overview
Add automated backend tests to ensure the user-creation flow rejects duplicate emails **case-insensitively** and responds with a safe, non-enumerating conflict payload.

Primary target endpoint: `POST /api/v1/admin/users` from `US_037`.

## Dependent Tasks
- [US_037 TASK_001 - Backend admin create Standard user endpoint]
- [US_038 TASK_001 - Backend reject duplicate email on user creation]
- [US_038 TASK_002 - Database case-insensitive unique constraint for email]

## Impacted Components
- [MODIFY | Server/ClinicalIntelligence.Api.Tests/AdminCreateUserEndpointTests.cs | Add test cases for case-insensitive duplicate email rejection and response safety]
- [MODIFY | Server/ClinicalIntelligence.Api.Tests/BaselineSchemaMigrationValidationTests.cs | Add a Postgres-only test validating DB uniqueness is case-insensitive]

## Implementation Plan
- API-level (behavior) tests (SQLite-backed test host):
  - Extend `AdminCreateUserEndpointTests.cs` with:
    - Create user with email `User@Example.com`.
    - Attempt to create another user with `user@example.com`.
    - Assert `409 Conflict` (or your standardized conflict status) with:
      - stable error code (e.g., `duplicate_email`)
      - message does not disclose existing user details
      - details array does not include user id, status, role, or any identifying metadata
- DB-level enforcement test (Postgres-only, when available):
  - Extend `BaselineSchemaMigrationValidationTests.cs` with a test similar to the existing uniqueness test, but inserting:
    - user1.Email = `case-test@Example.com`
    - user2.Email = `case-test@example.com`
  - Assert a `DbUpdateException` occurs due to the users email unique constraint.
- Keep tests deterministic and avoid logging secrets:
  - Use generated GUID-based emails for uniqueness.
  - Do not log plaintext passwords.

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api.Tests/AdminCreateUserEndpointTests.cs | Add test cases for case-insensitive duplicate email rejection and response payload safety |
| MODIFY | Server/ClinicalIntelligence.Api.Tests/BaselineSchemaMigrationValidationTests.cs | Validate DB constraint rejects case-insensitive duplicates when Postgres is available |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Automated] Run API test suite and confirm the new test fails if email normalization or conflict mapping regresses.
- [Automated/Postgres] Run the schema tests with a Postgres connection string set and confirm case-insensitive uniqueness is enforced.

## Implementation Checklist
- [ ] Extend admin create-user endpoint tests to cover different-case duplicate emails
- [ ] Assert conflict response does not include any existing user details
- [ ] Add Postgres-only DB constraint test for case-insensitive uniqueness
- [ ] Ensure tests are deterministic and do not log secrets
