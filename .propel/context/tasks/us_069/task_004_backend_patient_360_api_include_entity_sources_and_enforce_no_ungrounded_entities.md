# Task - TASK_069_004

## Requirement Reference
- User Story: us_069 (extracted from input)
- Story Location: [.propel/context/tasks/us_069/us_069.md]
- Acceptance Criteria: 
    - Given grounding enforcement, When applied, Then 100% of displayed entities have verifiable sources.
    - Given an extracted entity, When stored, Then it must have valid source citations or be rejected.

## Task Overview
Implement backend retrieval semantics for Patient 360 so that the API only returns extracted entities that have verifiable source citations, and includes citation details in the response payload. This enforces the “trust-first” invariant at the API boundary even if upstream components misbehave.

## Dependent Tasks
- [TASK_069_003] (Backend citations persistence + query helpers)

## Impacted Components
- [MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Add/extend `GET /api/v1/patients/{id}/360` endpoint to return entities with citations]
- [CREATE | Server/ClinicalIntelligence.Api/Contracts/Patients/Patient360Response.cs | Response DTO including entities + citations]
- [CREATE | Server/ClinicalIntelligence.Api/Services/Patients/Patient360Reader.cs | Query aggregation for entities + citations]
- [CREATE | Server/ClinicalIntelligence.Api.Tests/Patient360GroundingEnforcementTests.cs | Tests verifying API does not return ungrounded entities]

## Implementation Plan
- Define API response contract:
  - Patient summary fields (existing as needed)
  - `entities[]` each containing:
    - `category`, `name`, `value`
    - `citations[]` with document metadata + location + cited text
- Implement query logic:
  - Load extracted entities for patient
  - Join citations
  - Filter rule:
    - Only return entities with at least one valid citation record
    - If an entity has zero citations, do not return it (or return as `rejected`/`invalid` list if product later requires; for US_069 enforce exclusion)
- Tests:
  - Seed extracted entities with citations => returned
  - Seed extracted entities without citations => not returned

**Focus on how to implement**

## Current Project State
- Patient 360 frontend currently uses stubbed in-memory citations.
- No backend Patient 360 endpoint currently located via grep; may need to introduce first version.

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Add `GET /api/v1/patients/{id}/360` or extend existing endpoint to include grounded entities + citations |
| CREATE | Server/ClinicalIntelligence.Api/Contracts/Patients/Patient360Response.cs | Stable response DTO (entities + citations) |
| CREATE | Server/ClinicalIntelligence.Api/Services/Patients/Patient360Reader.cs | Query aggregator with grounding filter |
| CREATE | Server/ClinicalIntelligence.Api.Tests/Patient360GroundingEnforcementTests.cs | Verify API boundary never returns ungrounded entities |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis

## Build Commands
- dotnet build .\Server\ClinicalIntelligence.Api\ClinicalIntelligence.Api.csproj
- dotnet test .\Server\ClinicalIntelligence.Api.Tests\ClinicalIntelligence.Api.Tests.csproj

## Implementation Validation Strategy
- [API/Unit] Patient 360 response only includes entities with citations.
- [API/Unit] Citation fields in response match persisted values.

## Implementation Checklist
- [ ] Define Patient 360 response contract including entity citations
- [ ] Implement query aggregation and grounding filter in backend
- [ ] Add endpoint wiring and authorization as appropriate
- [ ] Add tests for grounded-only response behavior
