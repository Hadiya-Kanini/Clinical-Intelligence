# Task - [TASK_003]

## Requirement Reference
- User Story: [us_062]
- Story Location: [.propel/context/tasks/us_062/us_062.md]
- Acceptance Criteria: 
    - [Given access controls, When implemented, Then row-level security respects user permissions (DR-005).]

## Task Overview
Implement row-level security (RLS) for `document_chunks` to ensure chunk and vector retrieval is restricted to documents the requesting user is authorized to access. This task establishes the database policy and the backend mechanism required to propagate authenticated user identity into PostgreSQL so the policy can be enforced.

## Dependent Tasks
- [US_062 TASK_001] (Chunk table + pgvector schema exists)

## Impacted Components
- [CREATE | scripts/db/enable_document_chunks_rls.sql | SQL to enable RLS on `document_chunks` and create policies scoped by user identity.]
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Register a mechanism to set per-request PostgreSQL session context (user id/role) so RLS can evaluate.]
- [CREATE | Server/ClinicalIntelligence.Api/Data/NpgsqlUserContextInterceptor.cs | EF Core/Npgsql interceptor that issues `SET` for `app.user_id`/`app.user_role` on connection open.]
- [MODIFY | Server/ClinicalIntelligence.Api.Tests/BaselineSchemaMigrationValidationTests.cs | Add a Postgres-only integration test validating RLS blocks cross-user reads.]

## Implementation Plan
- Define RLS policy scope:
  - Base authorization on `documents.UploadedByUserId` (the current best available ownership signal in the schema).
  - Policy logic:
    - Allow `SELECT` when `documents.UploadedByUserId = current_setting('app.user_id')::uuid`.
    - Allow Admin bypass when `current_setting('app.user_role') = 'Admin'`.
- Create database script (idempotent) to enforce RLS:
  - `ALTER TABLE document_chunks ENABLE ROW LEVEL SECURITY;`
  - Create policies (example structure):
    - `CREATE POLICY ... FOR SELECT USING (EXISTS (SELECT 1 FROM documents d WHERE d."Id" = "DocumentId" AND (d."UploadedByUserId" = current_setting('app.user_id')::uuid OR current_setting('app.user_role') = 'Admin')));`
  - Ensure the policy uses `current_setting(..., true)` to avoid hard failures when context is missing and provide clearer error handling via the app.
- Propagate user context from API into PostgreSQL:
  - Implement an EF Core/Npgsql interceptor that runs on connection open and sets:
    - `SET app.user_id = '<user-guid>'`
    - `SET app.user_role = '<role>'`
  - Read user id and role from JWT claims via `IHttpContextAccessor`.
  - Ensure the values are always parameterized / safely quoted to avoid injection.
  - Confirm connection pooling behavior resets session state when returned to pool (do not rely on connection-level state persisting across requests).
- Add enforcement validation:
  - Add an integration test that:
    - Inserts two `documents` with different `UploadedByUserId` and associated `document_chunks`.
    - Sets `app.user_id` to user A and asserts user A cannot read chunks for user B.
    - Sets `app.user_role = 'Admin'` and asserts Admin can read all chunks.

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | scripts/db/enable_document_chunks_rls.sql | Enable RLS and create `document_chunks` policies based on `documents.UploadedByUserId` and `app.user_*` session variables |
| CREATE | Server/ClinicalIntelligence.Api/Data/NpgsqlUserContextInterceptor.cs | Set Postgres session variables (`app.user_id`, `app.user_role`) for RLS evaluation |
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Register interceptor + `IHttpContextAccessor` for request-scoped identity propagation |
| MODIFY | Server/ClinicalIntelligence.Api.Tests/BaselineSchemaMigrationValidationTests.cs | Add Postgres-only test validating RLS behavior |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://www.postgresql.org/docs/current/ddl-rowsecurity.html
- https://www.npgsql.org/efcore/

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [DB] Apply RLS script and verify `pg_policies` contains the expected policies for `document_chunks`.
- [API] Confirm requests set `app.user_id`/`app.user_role` and RLS filters results (no cross-user chunk access).
- [Security] Ensure queries remain parameterized and that missing `app.user_id` fails closed (no unintended access).

## Implementation Checklist
- [ ] Create `scripts/db/enable_document_chunks_rls.sql` with idempotent RLS enable + policy creation
- [ ] Implement API-to-Postgres identity propagation (`app.user_id`, `app.user_role`) via interceptor
- [ ] Register required services (`IHttpContextAccessor`, interceptor wiring)
- [ ] Add Postgres-only integration test validating cross-user reads are blocked
- [ ] Add Admin bypass test path (role-based)
- [ ] Document operational ordering: enable pgvector + migrations, then apply RLS script
