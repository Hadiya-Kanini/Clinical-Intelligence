# Task - TASK_073_002

## Requirement Reference
- User Story: us_073
- Story Location: .propel/context/tasks/us_073/us_073.md
- Acceptance Criteria: 
    - Given extracted data, When displayed, Then verified data shows green badge/indicator (UXR-025).
    - Given unverified data, When displayed, Then it shows yellow badge/indicator.
    - Given modified data, When displayed, Then it shows blue badge/indicator.

## Task Overview
Expose a stable data-status signal in the backend Patient 360 response (SCR-008) so the frontend can render verified/unverified/modified indicators deterministically.

This task intentionally limits scope to the API contract and mapping layer:
- Adds a simple status field on each returned entity/field.
- Derives the status from existing persistence where available (e.g., `ExtractedEntity.IsVerified`).
- Defers “modified” persistence semantics to US_074, but provides the contract surface and wiring hook.

## Dependent Tasks
- [TASK_069_004] (Backend Patient 360 endpoint and DTOs)
- [US_074] (Inline editing persists “modified” status)

## Impacted Components
- [MODIFY | Server/ClinicalIntelligence.Api/Contracts/Patients/Patient360Response.cs | Add a `dataStatus` field (or equivalent) on each entity/field returned to the UI]
- [MODIFY | Server/ClinicalIntelligence.Api/Services/Patients/Patient360Reader.cs | Populate `dataStatus` based on stored fields; ensure deterministic mapping]
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Ensure `GET /api/v1/patients/{id}/360` returns the enriched status fields]
- [MODIFY | Server/ClinicalIntelligence.Api.Tests/Patient360GroundingEnforcementTests.cs | Extend assertions to validate `dataStatus` mapping for verified/unverified cases]

## Implementation Plan
- Define a constrained status shape:
  - Prefer a string enum-like field in the response: `"verified" | "unverified" | "modified"`.
  - Ensure the response remains forward-compatible by treating unknown values as `unverified` on the client.
- Map status for Phase 1:
  - Verified:
    - If an entity source is `ExtractedEntity` and `IsVerified == true`, set `dataStatus = "verified"`.
  - Unverified:
    - Default for extracted entities with `IsVerified == false` OR when no verification field exists.
  - Modified:
    - If/when US_074 introduces a persisted flag/metadata, map it to `dataStatus = "modified"`.
    - Until then, keep mapping to verified/unverified only (but keep the contract field available).
- Edge cases:
  - If multiple flags exist (e.g., verified + modified), set `modified` precedence (needs review).
  - Ensure no PHI is introduced into the status field.

**Focus on how to implement**

## Current Project State
- The Patient 360 backend endpoint is planned in US_069 (TASK_069_004), but is not currently discoverable in `Program.cs`.
- `ExtractedEntity` includes `IsVerified` / `VerifiedAt` / `VerifiedByUserId` which can support verified/unverified.
- “Modified” persistence is planned in US_074.

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Contracts/Patients/Patient360Response.cs | Add `dataStatus` field to each entity/field in the Patient 360 response |
| MODIFY | Server/ClinicalIntelligence.Api/Services/Patients/Patient360Reader.cs | Populate `dataStatus` deterministically from stored verification/modified fields |
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Ensure endpoint returns the status-enriched response |
| MODIFY | Server/ClinicalIntelligence.Api.Tests/Patient360GroundingEnforcementTests.cs | Validate `dataStatus` is present and mapped correctly for verified/unverified test fixtures |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis

## Build Commands
- dotnet build .\Server\ClinicalIntelligence.Api\ClinicalIntelligence.Api.csproj
- dotnet test .\Server\ClinicalIntelligence.Api.Tests\ClinicalIntelligence.Api.Tests.csproj

## Implementation Validation Strategy
- [API/Tests] Seed an extracted entity with `IsVerified=true` and confirm it is returned with `dataStatus="verified"`.
- [API/Tests] Seed an extracted entity with `IsVerified=false` and confirm it is returned with `dataStatus="unverified"`.
- [Edge Case] When modified persistence is available, seed modified entity and confirm `dataStatus="modified"` takes precedence.

## Implementation Checklist
- [ ] Add `dataStatus` field to Patient 360 response DTO at the entity/field level
- [ ] Implement mapping logic in `Patient360Reader`
- [ ] Ensure endpoint returns `dataStatus` for all items
- [ ] Extend Patient 360 endpoint tests to cover verified/unverified mapping
- [ ] Document precedence rule (modified > verified/unverified) in code via shared mapping helper
