# Task - [TASK_002]

## Requirement Reference
- User Story: [us_025]
- Story Location: [.propel/context/tasks/us_025/us_025.md]
- Acceptance Criteria: 
    - [Given multiple reset requests, When a new token is generated, Then previous tokens for that user are invalidated.]
    - [Given a password reset request, When a token is generated, Then it uses cryptographically secure random generation.]

## Task Overview
Integrate password reset token generation into the forgot-password backend flow so that a new reset token is generated for an existing account while previous reset tokens for that user are invalidated. The endpoint response must remain non-enumerating and stable for the frontend.

## Dependent Tasks
- [US_024 TASK_002 - Backend forgot password endpoint (generic response)]
- [US_025 TASK_001 - Backend password reset token generation and storage]

## Impacted Components
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Enhance `POST /api/v1/auth/forgot-password` to generate a token for existing user and invalidate previous tokens]
- [MODIFY | Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs | Query/update password reset tokens to invalidate previous entries (if helper configuration is needed)]

## Implementation Plan
- Update the forgot-password endpoint implementation to:
  - Validate request input (reuse existing validation behavior).
  - Lookup the user by email.
  - If user exists and is eligible for reset:
    - Invalidate all previously active reset tokens for the user.
    - Generate and persist a new reset token via `IPasswordResetTokenService`.
    - Leave a placeholder integration point for email delivery (do not change response shape).
  - If user does not exist:
    - Do not create any token.
  - Always return the same generic `200` response for syntactically valid emails to prevent user enumeration.
- Token invalidation approach should ensure older tokens cannot be used (e.g., mark `UsedAt` or set `ExpiresAt` to `DateTime.UtcNow`).

## Current Project State
- [Placeholder to be updated during execution]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Update `POST /api/v1/auth/forgot-password` to call token service for existing users and invalidate previous tokens while keeping generic response |
| MODIFY | Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs | Update EF mapping or query filters only if required for invalidation queries |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis

## Build Commands
- [Refer to applicable technology stack specific build commands](../.propel/build/)

## Implementation Validation Strategy
- [Manual/API] POST a valid email for an existing user twice and confirm only the newest token remains valid per DB state (previous token invalidated).
- [Security] POST existing vs non-existing emails and confirm responses are indistinguishable.

## Implementation Checklist
- [ ] Update forgot-password endpoint to look up user by email
- [ ] If user exists, invalidate previous reset tokens for that user
- [ ] If user exists, generate + persist a new token via `IPasswordResetTokenService`
- [ ] Ensure no token is generated for non-existing email
- [ ] Keep response generic and unchanged for valid emails (no user enumeration)
- [ ] Ensure invalidation logic is atomic enough to prevent multiple active tokens
