# Task - [TASK_001]

## Requirement Reference
- User Story: [us_021]
- Story Location: [.propel/context/tasks/us_021/us_021.md]
- Acceptance Criteria: 
    - [Given a password is created or changed, When it is stored, Then bcrypt hashing with minimum 12 rounds is applied.]
    - [Given a user authenticates, When password is verified, Then bcrypt comparison is used (timing-safe).]
    - [Given password hashing, When implemented, Then plain-text passwords are never logged or stored.]
    - [Given bcrypt configuration, When rounds are set, Then the value is configurable for future security updates.]

## Task Overview
Introduce a centralized, reusable bcrypt password hashing/verification service with a configurable work factor (minimum 12), and update current password verification/hashing call sites to use it to ensure consistent, secure handling.

## Dependent Tasks
- [.propel/context/tasks/us_019/task_001_backend_password_complexity_enforcement.md]

## Impacted Components
- [CREATE | Server/ClinicalIntelligence.Api/Services/Auth/BcryptPasswordHasher.cs | Centralize password hashing + verification using BCrypt with enforced minimum work factor]
- [MODIFY | Server/ClinicalIntelligence.Api/Configuration/SecretsOptions.cs | Add configurable bcrypt work factor (env/config) and validate minimum 12]
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Replace direct `BCrypt.Verify` usage with centralized hasher for login verification]
- [MODIFY | Server/ClinicalIntelligence.Api/Migrations/20260115100000_SeedStaticAdminAccount.cs | Replace hardcoded `BcryptWorkFactor` const with configuration-derived value (enforced minimum 12) and central hashing helper]
- [MODIFY | .env.example | Add `BCRYPT_WORK_FACTOR` (or chosen key) with secure default guidance]

## Implementation Plan
- Add a dedicated password hashing service under `Services/Auth`:
  - Implement `HashPassword(string password, int workFactor)` and `Verify(string password, string passwordHash)`.
  - Enforce minimum work factor of 12 (reject invalid configuration rather than silently lowering security).
  - Ensure plaintext passwords are never written to logs or exception messages.
- Add bcrypt work factor configuration:
  - Extend `SecretsOptions` to include a `BcryptWorkFactor` (default 12).
  - Read from configuration/environment (e.g., `BCRYPT_WORK_FACTOR`) and validate it on startup.
- Update authoritative call sites:
  - Login endpoint in `Program.cs` must use the centralized verifier.
  - Static admin seed migration must hash using the configured work factor (>=12).
- Confirm operational behavior:
  - Validate that increased work factor does not break login under rate limiting / lockout conditions.

## Current Project State
- Created `IBcryptPasswordHasher` interface and `BcryptPasswordHasher` implementation in `Services/Auth/`
- Added `BcryptWorkFactor` property to `SecretsOptions` with `ValidateBcryptConfiguration()` method
- Updated `Program.cs` to register `IBcryptPasswordHasher` and use it in login/reset-password endpoints
- Updated migration to use configurable work factor from `BCRYPT_WORK_FACTOR` environment variable
- Added `BCRYPT_WORK_FACTOR=12` to `.env.example` with documentation

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api/Services/Auth/BcryptPasswordHasher.cs | Central bcrypt hashing + verification with enforced minimum work factor and safe error handling |
| MODIFY | Server/ClinicalIntelligence.Api/Configuration/SecretsOptions.cs | Add `BcryptWorkFactor` config parsing + validation (minimum 12) |
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Use centralized password verification in `/api/v1/auth/login` |
| MODIFY | Server/ClinicalIntelligence.Api/Migrations/20260115100000_SeedStaticAdminAccount.cs | Use centralized hashing helper and configured work factor (>=12) instead of hardcoded constant |
| MODIFY | .env.example | Add/clarify `BCRYPT_WORK_FACTOR` configuration for deployments |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Automated] Run backend unit/integration tests covering login and seeded admin hashing/verification.
- [Manual] Verify login succeeds for existing seeded admin and fails for invalid passwords with consistent error messaging.

## Implementation Checklist
- [x] Implement `BcryptPasswordHasher` with `HashPassword` and `Verify` helpers
- [x] Add `BcryptWorkFactor` configuration to `SecretsOptions` with validation enforcing minimum 12
- [x] Update `/api/v1/auth/login` to use centralized verifier and keep error messages non-sensitive
- [x] Update static admin seed migration to hash with configured work factor (>=12)
- [x] Ensure no plaintext password values are logged (review any log statements touched)
- [x] Update `.env.example` to include the bcrypt work factor configuration
- [x] Sanity check performance expectations for work factor 12 under load (document decision in code behavior, not documentation)
