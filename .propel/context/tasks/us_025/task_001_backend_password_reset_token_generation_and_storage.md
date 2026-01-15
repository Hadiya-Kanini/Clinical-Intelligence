# Task - [TASK_001]

## Requirement Reference
- User Story: [us_025]
- Story Location: [.propel/context/tasks/us_025/us_025.md]
- Acceptance Criteria: 
    - [Given a password reset request, When a token is generated, Then it uses cryptographically secure random generation.]
    - [Given a reset token, When created, Then it has a 1-hour expiration time.]
    - [Given a reset token, When stored, Then only the hash is persisted (not the plain token).]

## Task Overview
Implement the backend capability to generate cryptographically secure password reset tokens with a strict 1-hour expiry, and persist only the token hash in the database. This task establishes a reusable service that can be invoked by the forgot-password endpoint (US_024) and future reset-password validation logic.

## Dependent Tasks
- [US_024 TASK_002 - Backend forgot password endpoint (generic response)] (integration will happen in TASK_002)

## Impacted Components
- [CREATE | Server/ClinicalIntelligence.Api/Services/Auth/IPasswordResetTokenService.cs | Interface for generating and persisting password reset tokens]
- [CREATE | Server/ClinicalIntelligence.Api/Services/Auth/PasswordResetTokenService.cs | Implementation that generates secure tokens, stores only hash, and sets 1-hour expiry]
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Register the password reset token service in DI]

## Implementation Plan
- Create an application-level service responsible for token generation and persistence.
- Generate token material using a cryptographically secure RNG.
- Encode the token for safe transport (URL-safe string) and compute a server-side hash for persistence.
- Persist a `PasswordResetToken` row containing:
  - `UserId`
  - `TokenHash`
  - `ExpiresAt = DateTime.UtcNow.AddHours(1)`
  - `UsedAt = null`
- Ensure the service returns the plain token to the caller (for later email delivery), but never persists or logs it.

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api/Services/Auth/IPasswordResetTokenService.cs | Abstraction for reset token generation + persistence API |
| CREATE | Server/ClinicalIntelligence.Api/Services/Auth/PasswordResetTokenService.cs | Secure token generator that stores only token hash and sets 1-hour expiry |
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Register `IPasswordResetTokenService` in the DI container |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.randomnumbergenerator

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Manual/Debug] Invoke service method in a local debug session and confirm:
  - Token is non-empty and URL-safe.
  - A `PasswordResetToken` row is created with `ExpiresAt` approximately 1 hour in the future (UTC).
  - Only `TokenHash` is stored (plain token is not present in DB).

## Implementation Checklist
- [x] Add `IPasswordResetTokenService` interface under `Services/Auth`
- [x] Implement `PasswordResetTokenService` using cryptographically secure randomness
- [x] Implement token hashing and ensure only the hash is persisted
- [x] Set token expiry to 1 hour from creation (UTC)
- [x] Ensure the plain token is returned to caller but not logged or stored
- [x] Register the service in DI (`Program.cs`)
