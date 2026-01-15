# Task - [TASK_002]

## Requirement Reference
- User Story: [us_021]
- Story Location: [.propel/context/tasks/us_021/us_021.md]
- Acceptance Criteria: 
    - [Given a password is created or changed, When it is stored, Then bcrypt hashing with minimum 12 rounds is applied.]
    - [Given a user authenticates, When password is verified, Then bcrypt comparison is used (timing-safe).]
    - [Given bcrypt configuration, When rounds are set, Then the value is configurable for future security updates.]

## Task Overview
Add automated test coverage to ensure bcrypt hashing is always performed with a minimum work factor of 12, the work factor configuration is validated, and login verification uses the centralized bcrypt verifier.

## Dependent Tasks
- [.propel/context/tasks/us_021/task_001_backend_configurable_bcrypt_password_hashing.md]

## Impacted Components
- [CREATE | Server/ClinicalIntelligence.Api.Tests/Services/BcryptPasswordHasherTests.cs | Unit tests validating hashing/verification behavior and minimum work factor enforcement]
- [MODIFY | Server/ClinicalIntelligence.Api.Tests/SecretsOptionsTests.cs | Add tests for bcrypt work factor parsing and validation]
- [MODIFY | Server/ClinicalIntelligence.Api.Tests/StaticAdminSeedMigrationTests.cs | Align tests to assert configured work factor behavior (>=12) and verification works]
- [MODIFY | Server/ClinicalIntelligence.Api.Tests/SeededAdminAuthenticationTests.cs | Ensure login path still verifies passwords correctly via centralized verifier]

## Implementation Plan
- Add unit tests for the password hasher:
  - Hash + verify round-trip works.
  - Invalid hashes fail verification safely.
  - Configured work factor below 12 is rejected.
- Add configuration tests:
  - `SecretsOptions` reads `BCRYPT_WORK_FACTOR` (or chosen key) correctly.
  - Startup validation fails if configured work factor is <12.
- Align existing tests:
  - Update any tests that directly call `BCrypt.HashPassword(password, 12)` to use the configured work factor or centralized helper.

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api.Tests/Services/BcryptPasswordHasherTests.cs | Unit tests covering bcrypt hashing/verification and minimum-work-factor enforcement |
| MODIFY | Server/ClinicalIntelligence.Api.Tests/SecretsOptionsTests.cs | Add tests for bcrypt work factor config parsing/validation |
| MODIFY | Server/ClinicalIntelligence.Api.Tests/StaticAdminSeedMigrationTests.cs | Ensure seeded admin hashing/verification tests align with configurable work factor (>=12) |
| MODIFY | Server/ClinicalIntelligence.Api.Tests/SeededAdminAuthenticationTests.cs | Keep login tests passing and validate password verification remains correct |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Automated] Run `ClinicalIntelligence.Api.Tests` suite; ensure new tests cover: work factor >=12, config parsing, hash/verify correctness.

## Implementation Checklist
- [ ] Add `BcryptPasswordHasherTests` to cover hash/verify and minimum work factor constraints
- [ ] Extend `SecretsOptionsTests` for `BCRYPT_WORK_FACTOR` parsing and validation behavior
- [ ] Update `StaticAdminSeedMigrationTests` assertions to reflect configurable work factor (>=12)
- [ ] Update `SeededAdminAuthenticationTests` if needed to align with centralized verifier
- [ ] Ensure tests do not output plaintext passwords in failure messages
