# Task - [TASK_001]

## Requirement Reference
- User Story: [us_019]
- Story Location: [.propel/context/tasks/us_019/us_019.md]
- Acceptance Criteria: 
    - [Given a user sets or changes a password, When complexity is validated, Then it must have minimum 8 characters.]
    - [Given password complexity validation, When checked, Then it must contain at least one uppercase letter, one lowercase letter, one number, and one special character.]

## Task Overview
Introduce a single, reusable backend password complexity policy (FR-009c) and apply it to server-side password setting paths (starting with the seeded static admin password validation) so weak passwords are rejected consistently and securely.

## Dependent Tasks
- [N/A]

## Impacted Components
- [CREATE | Server/ClinicalIntelligence.Api/Services/Auth/PasswordPolicy.cs | Central password complexity validation (length + character class rules) with a structured way to return missing requirements]
- [MODIFY | Server/ClinicalIntelligence.Api/Migrations/20260115100000_SeedStaticAdminAccount.cs | Replace duplicated regex checks with the centralized policy to ensure consistent enforcement]
- [MODIFY | Server/ClinicalIntelligence.Api.Tests/StaticAdminSeedMigrationTests.cs | Extend/align tests to validate the policy and ensure migration enforcement matches FR-009c]

## Implementation Plan
- Define an explicit password policy contract:
  - Minimum length: 8
  - Maximum length: decide and enforce (e.g., 128) to prevent pathological inputs
  - Required categories: lowercase, uppercase, digit, special character
  - Special character definition: align with frontend behavior (confirm whether to treat any non-letter/digit as "special", including unicode)
- Implement `PasswordPolicy` in `Services/Auth`:
  - Provide boolean validation and a helper that returns which requirements are missing for consistent error reporting.
  - Ensure policy evaluation is deterministic and unit-testable.
- Apply policy to existing password enforcement points:
  - Update `SeedStaticAdminAccount` migration to use `PasswordPolicy`.
  - Align `StaticAdminSeedMigrationTests` to assert the policy behavior for representative cases.
- Document edge-case decision in code behavior (not documentation):
  - Existing users with legacy weak passwords should not be forcibly logged out; enforce only on password set/change operations.

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api/Services/Auth/PasswordPolicy.cs | Implement reusable password complexity validation helpers (min/max length, required categories, missing requirements) |
| MODIFY | Server/ClinicalIntelligence.Api/Migrations/20260115100000_SeedStaticAdminAccount.cs | Use `PasswordPolicy` to validate `ADMIN_PASSWORD` complexity before hashing/inserting |
| MODIFY | Server/ClinicalIntelligence.Api.Tests/StaticAdminSeedMigrationTests.cs | Add/adjust tests to validate complexity enforcement and ensure edge cases are covered |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://pages.nist.gov/800-63-3/sp800-63b.html

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Automated] Run backend test suite and ensure password policy tests cover each required rule and common failure combinations.
- [Manual] Validate that invalid `ADMIN_PASSWORD` values fail migration with a clear, non-sensitive error message.

## Implementation Checklist
- [x] Implement `PasswordPolicy` with min/max length and required categories (lower/upper/digit/special)
- [x] Decide and implement special character detection consistent with frontend (including unicode handling)
- [x] Add helper API to return missing requirements (for consistent error handling)
- [x] Update `SeedStaticAdminAccount` migration to call `PasswordPolicy` (remove duplicated regex)
- [x] Extend `StaticAdminSeedMigrationTests` to cover valid/invalid examples (per requirement categories)
- [x] Confirm error messages do not leak sensitive values and remain consistent with OWASP guidance
- [x] Validate behavior for existing users: enforce policy only on password set/change operations
