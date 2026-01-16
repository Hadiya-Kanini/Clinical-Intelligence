# Task - [TASK_002]

## Requirement Reference
- User Story: [us_038]
- Story Location: [.propel/context/tasks/us_038/us_038.md]
- Acceptance Criteria: 
    - [Given email uniqueness check, When performed, Then it is case-insensitive (User@Example.com = user@example.com).]
    - [Given the database schema, When designed, Then email has a unique constraint.]

## Task Overview
Add database-level enforcement for **case-insensitive** email uniqueness on the `users` table.

This task provides defense-in-depth beyond application-level checks and prevents race conditions (two concurrent creates) from allowing duplicates.

## Dependent Tasks
- [US_119] Baseline schema migration (users table exists)

## Impacted Components
- [CREATE | Server/ClinicalIntelligence.Api/Migrations/XXXXXX_US038_CaseInsensitiveUserEmailUniqueConstraint.cs | EF Core migration to enforce case-insensitive uniqueness]
- [MODIFY | Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs | Update the `User.Email` index configuration if needed to reflect the DB change]
- [MODIFY | Server/ClinicalIntelligence.Api/Migrations/ApplicationDbContextModelSnapshot.cs | Snapshot update reflecting the new email uniqueness strategy]

## Implementation Plan
- Choose a Postgres-compatible case-insensitive uniqueness strategy:
  - Preferred: use `citext` for `users."Email"` and keep a unique index on the column.
  - Alternative: keep `varchar` and create a **functional unique index** on `lower(users."Email")`.
- Implement migration steps:
  - If using `citext`:
    - Ensure extension exists: `CREATE EXTENSION IF NOT EXISTS citext;`
    - Alter column type: `ALTER TABLE users ALTER COLUMN "Email" TYPE citext;`
    - Ensure/replace unique index (keep canonical name `ix_users_email` if possible to preserve existing tests/assumptions).
  - If using functional index:
    - Create unique index on `lower("Email")` (and decide whether to retain/remove the existing `ix_users_email` to avoid duplicate/unused indexes).
- Consider soft-delete rules:
  - Default: uniqueness applies globally (including soft-deleted users), so email remains reserved.
  - If business rules require re-use for soft-deleted users, implement a **partial unique index** excluding deleted rows (requires product decision; not assumed by default for US_038).

**Focus on how to implement**

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api/Migrations/XXXXXX_US038_CaseInsensitiveUserEmailUniqueConstraint.cs | Enforces case-insensitive unique email constraint via citext or functional unique index |
| MODIFY | Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs | Ensure EF model matches the DB uniqueness strategy for `User.Email` |
| MODIFY | Server/ClinicalIntelligence.Api/Migrations/ApplicationDbContextModelSnapshot.cs | Update snapshot to reflect the new schema/index configuration |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://www.postgresql.org/docs/current/citext.html
- https://www.postgresql.org/docs/current/indexes-expressional.html
- https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Automated/Integration] Extend/adjust database schema tests to validate that inserting `User@Example.com` and `user@example.com` results in a uniqueness violation.
- [Migration Safety] Validate migration applies cleanly on a database that already contains users (including the seeded static admin).

## Implementation Checklist
- [x] Decide and document (in code) whether `citext` or a functional unique index will be used
- [x] Implement migration to enforce case-insensitive uniqueness for users email
- [x] Ensure the unique index name remains stable (`ix_users_email`) if referenced by tests
- [ ] Apply migration and validate it works with existing seeded data
- [x] Add/adjust schema validation tests for case-insensitive duplicates
