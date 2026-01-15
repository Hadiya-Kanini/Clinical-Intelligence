# Unit Test Plan - US_004: Maintainability Guardrails

## Requirement Reference
- **User Story**: us_004
- **Story Location**: .propel/context/tasks/us_004/us_004.md
- **Layer**: Multi-layer (BE, DB, Infra, Scripts)
- **Related Test Plans**: N/A (Comprehensive multi-layer plan)
- **Acceptance Criteria Covered**:
  - AC-1: Service boundary compliance - functionality placed in correct service without bypassing contracts
  - AC-2: API contract versioning - Swagger/OpenAPI updated with versioning strategy for changes
  - AC-3: Database migration mechanism - schema changes via migrations, not manual edits
  - AC-4: Worker schema versioning - entity schemas versioned for safe processing and retries

## Test Plan Overview
This comprehensive unit test plan validates maintainability guardrails across contract enforcement, database migrations, and schema versioning. It covers CI/CD validation scripts, EF Core migration infrastructure, and worker schema validation mechanisms to ensure architectural boundaries remain intact as the platform scales.

## Dependent Tasks
- task_001_enforce_contract_change_guardrails.md (Contract CI enforcement)
- task_002_scaffold_database_migration_mechanism.md (EF Core DbContext setup)
- task_003_create_baseline_db_migration_and_guardrail.md (Initial migration)
- task_004_version_worker_entity_validation_schemas.md (Worker schema versioning)

## Components Under Test

| Component | Type | File Path | Responsibilities |
|-----------|------|-----------|------------------|
| ContractValidator | Python Script | scripts/validate_contracts.py | Validates contract files exist, migration notes present, schema compliance |
| ContractEnforcementWorkflow | GitHub Action | .github/workflows/contracts.yml | CI enforcement for contract-migration coupling |
| ApplicationDbContext | EF Core DbContext | Server/ClinicalIntelligence.Api/Data/ApplicationDbContext.cs | Database persistence boundary and migration management |
| MigrationService | .NET Service | Server/ClinicalIntelligence.Api/Services/MigrationService.cs | Migration application and validation logic |
| EntitySchemaValidator | Python Module | worker/schema_validator.py | Worker entity schema version loading and validation |
| WorkerSchemaLoader | Python Module | worker/schema_loader.py | Schema version selection and compatibility checking |

## Test Cases

### Contract Validation & Enforcement Tests

| Test-ID | Type | Description | Given | When | Then | Assertions |
|---------|------|-------------|-------|------|------|------------|
| TC-001 | positive | Validate contract files exist | Contract directory structure with api/v1/openapi.yaml and jobs/v1/job.schema.json | validate_contracts.py executed | Script passes validation | Exit code 0, no error messages |
| TC-002 | negative | Detect missing API contract | contracts/api/v1/ directory empty | validate_contracts.py executed | Script fails with clear error | Exit code 1, error message indicates missing openapi.yaml |
| TC-003 | negative | Detect missing job schema | contracts/jobs/v1/ directory empty | validate_contracts.py executed | Script fails with clear error | Exit code 1, error message indicates missing job.schema.json |
| TC-004 | positive | Validate migration notes exist | contracts/migrations/api_v1.md and jobs_v1.md present with content | validate_contracts.py executed | Script passes migration note check | Migration files validated as non-empty |
| TC-005 | negative | Detect empty migration notes | contracts/migrations/api_v1.md exists but empty | validate_contracts.py executed | Script fails validation | Exit code 1, error indicates empty migration note file |
| TC-006 | positive | CI detects API contract change with migration | PR modifies contracts/api/v1/openapi.yaml and contracts/migrations/api_v1.md | GitHub Actions workflow runs | Workflow passes | No CI failure, validation step succeeds |
| TC-007 | negative | CI blocks API contract change without migration | PR modifies contracts/api/v1/openapi.yaml only | GitHub Actions workflow runs | Workflow fails with clear message | CI fails, message requires migration note update |
| TC-008 | negative | CI blocks job schema change without migration | PR modifies contracts/jobs/v1/job.schema.json only | GitHub Actions workflow runs | Workflow fails with clear message | CI fails, message requires migration note update |
| EC-001 | edge_case | Handle multiple contract changes in single PR | PR modifies both API and job contracts with both migration notes | GitHub Actions workflow runs | Workflow validates both changes | Both contract-migration pairs validated |
| EC-002 | edge_case | Validate contract README checklist exists | contracts/README.md present | validate_contracts.py executed | README contains feature change checklist | Checklist section found with service boundary guidance |

### Database Migration Infrastructure Tests

| Test-ID | Type | Description | Given | When | Then | Assertions |
|---------|------|-------------|-------|------|------|------------|
| TC-009 | positive | DbContext instantiation with valid connection | Valid connection string in configuration | ApplicationDbContext instantiated via DI | DbContext created successfully | No exceptions, DbContext instance not null |
| TC-010 | negative | DbContext fails with invalid connection string | Invalid/malformed connection string | ApplicationDbContext instantiation attempted | Throws configuration exception | Exception type matches expected, clear error message |
| TC-011 | positive | DbContext registered in DI container | Program.cs configures DbContext | Application startup | DbContext available via DI | Service provider resolves ApplicationDbContext |
| TC-012 | positive | Migration command generates migration file | DbContext with model changes | dotnet ef migrations add executed | Migration file created in Migrations/ folder | Migration file exists, contains Up/Down methods |
| TC-013 | positive | Migration applies to empty database | Baseline migration exists | dotnet ef database update executed on empty DB | Database schema created | Tables exist, schema matches migration |
| TC-014 | negative | Migration fails on incompatible database | Database with conflicting schema | Migration application attempted | Migration fails with clear error | Exception thrown, error indicates conflict |
| TC-015 | positive | Migration rollback functionality | Applied migration | dotnet ef database update to previous migration | Database reverted to previous state | Schema matches previous migration version |
| EC-003 | edge_case | Handle concurrent migration attempts | Multiple migration processes started | Migrations attempt to apply simultaneously | One succeeds, others fail gracefully | Database consistency maintained, clear error for failed attempts |
| EC-004 | edge_case | Validate migration history tracking | Multiple migrations applied | Query migration history table | All applied migrations recorded | __EFMigrationsHistory table accurate |

### Worker Entity Schema Versioning Tests

| Test-ID | Type | Description | Given | When | Then | Assertions |
|---------|------|-------------|-------|------|------|------------|
| TC-016 | positive | Load schema by valid version | contracts/entities/v1/entity.schema.json exists | Worker loads schema version "v1" | Schema loaded successfully | Schema object not null, contains expected keys |
| TC-017 | negative | Reject unknown schema version | Worker receives payload with version "v99" | Schema loader attempts to load version | Loader rejects with clear error | ValueError raised, message indicates unsupported version |
| TC-018 | positive | Validate payload against schema | Valid entity payload matching v1 schema | Worker validates payload | Validation passes | No validation errors, payload accepted |
| TC-019 | negative | Reject invalid payload structure | Payload missing required fields | Worker validates against schema | Validation fails with field errors | ValidationError raised, lists missing fields |
| TC-020 | negative | Reject payload with wrong data types | Payload with string instead of number | Worker validates against schema | Validation fails with type error | ValidationError raised, indicates type mismatch |
| TC-021 | positive | Schema contract validation in CI | contracts/entities/v1/entity.schema.json present with version field | validate_contracts.py executed | Entity schema validated | Schema contains required keys: $schema, version, properties |
| TC-022 | negative | Detect malformed entity schema | entity.schema.json with invalid JSON | validate_contracts.py executed | Validation fails | Exit code 1, JSON parse error reported |
| TC-023 | positive | Worker handles schema version in job payload | Job payload includes schemaVersion: "v1" | Worker processes job | Correct schema version loaded for validation | Schema version from payload used |
| EC-005 | edge_case | Handle retry with older schema version | Job created with v1, system upgraded to v2 | Worker retries job with v1 schema version | Worker loads v1 schema for validation | Backward compatibility maintained, v1 schema used |
| EC-006 | edge_case | Validate schema version identifier format | Schema with version "1.0.0" vs "v1" | Schema loader normalizes version | Version parsed correctly | Both formats recognized, normalized to consistent format |
| ES-001 | error | Handle missing schema file | Schema file deleted or not deployed | Worker attempts to load schema | Graceful error with clear message | FileNotFoundError caught, error logged with file path |
| ES-002 | error | Handle corrupted schema file | Schema file with corrupted JSON | Worker attempts to load schema | JSON parse error handled | JSONDecodeError caught, error indicates file corruption |

### Integration & Cross-Boundary Tests

| Test-ID | Type | Description | Given | When | Then | Assertions |
|---------|------|-------------|-------|------|------|------------|
| TC-024 | positive | End-to-end contract change workflow | Developer modifies API contract | PR created with contract + migration note | CI validates, migration documented | Workflow passes, changes traceable |
| TC-025 | positive | Database migration in development workflow | Developer adds new entity | Migration created, applied, committed | Schema updated via migration | No manual SQL, migration history clean |
| TC-026 | positive | Worker schema evolution workflow | New entity field added | Schema v2 created, worker updated | Both v1 and v2 supported | Backward compatibility maintained |
| EC-007 | edge_case | Breaking change detection | API contract removes required field | PR validation runs | Breaking change flagged | Migration note must document breaking change |
| EC-008 | edge_case | Dual-version contract support | System running v1, v2 being deployed | Both versions active | Requests routed to correct version | Version negotiation works, no errors |

## Expected Changes

| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | Server/ClinicalIntelligence.Api.Tests/Contracts/ContractValidationTests.cs | Unit tests for contract validation logic |
| CREATE | Server/ClinicalIntelligence.Api.Tests/Data/ApplicationDbContextTests.cs | Unit tests for DbContext configuration and instantiation |
| CREATE | Server/ClinicalIntelligence.Api.Tests/Data/MigrationServiceTests.cs | Unit tests for migration application and validation |
| CREATE | scripts/tests/test_validate_contracts.py | pytest unit tests for contract validation script |
| CREATE | worker/tests/test_schema_validator.py | pytest unit tests for entity schema validation |
| CREATE | worker/tests/test_schema_loader.py | pytest unit tests for schema version loading |
| CREATE | Server/ClinicalIntelligence.Api.Tests/Mocks/MockDbContext.cs | Mock DbContext for testing without database |
| CREATE | scripts/tests/fixtures/contracts/ | Test fixture contracts (valid/invalid samples) |
| CREATE | worker/tests/fixtures/schemas/ | Test fixture entity schemas (v1, v2, invalid) |
| CREATE | worker/tests/fixtures/payloads/ | Test fixture entity payloads (valid/invalid) |

## Mocking Strategy

| Dependency | Mock Type | Mock Behavior | Return Value |
|------------|-----------|---------------|--------------|
| IConfiguration (.NET) | mock | Return test connection strings | Dictionary with test DB connection |
| DbContextOptions | mock | Provide in-memory database options | InMemoryDatabase configured |
| ILogger | mock | Track log calls for validation | Pass through, verify calls |
| File System (Python) | mock | Simulate contract file presence/absence | Mock file contents or FileNotFoundError |
| JSON Schema Validator | spy | Track validation calls | Pass through real validator |
| GitHub Actions Context | mock | Simulate PR file changes | Mock git diff output |

## Test Data

| Scenario | Input Data | Expected Output |
|----------|------------|-----------------|
| Valid API contract | `{"openapi": "3.0.0", "info": {...}}` | Validation passes |
| Invalid API contract | `{"openapi": "2.0"}` | Error: Unsupported OpenAPI version |
| Valid migration note | `# API v1 Migration\n\n## Changes\n- Added endpoint` | Validation passes |
| Empty migration note | `` (empty file) | Error: Migration note empty |
| Valid entity schema v1 | `{"$schema": "...", "version": "v1", "properties": {...}}` | Schema loaded |
| Invalid schema version | `{"version": "v99"}` | Error: Unsupported version v99 |
| Valid entity payload | `{"type": "diagnosis", "code": "E11.9", "schemaVersion": "v1"}` | Validation passes |
| Invalid entity payload | `{"type": "diagnosis"}` (missing required fields) | Error: Missing required field 'code' |
| Valid connection string | `Server=localhost;Database=test;...` | DbContext created |
| Invalid connection string | `InvalidConnectionString` | Error: Connection string malformed |

## Test Commands

### .NET Backend Tests
- **Run All Tests**: `dotnet test Server/ClinicalIntelligence.Api.Tests/ClinicalIntelligence.Api.Tests.csproj`
- **Run Contract Tests**: `dotnet test --filter "FullyQualifiedName~ContractValidationTests"`
- **Run DbContext Tests**: `dotnet test --filter "FullyQualifiedName~ApplicationDbContextTests"`
- **Run with Coverage**: `dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover`

### Python Script Tests
- **Run Contract Validation Tests**: `pytest scripts/tests/test_validate_contracts.py -v`
- **Run Worker Schema Tests**: `pytest worker/tests/test_schema_validator.py worker/tests/test_schema_loader.py -v`
- **Run with Coverage**: `pytest --cov=scripts --cov=worker --cov-report=html`
- **Run Single Test**: `pytest scripts/tests/test_validate_contracts.py::test_valid_contracts_pass -v`

### Frontend Tests (if applicable)
- **Run Unit Tests**: `npm test -- --run`
- **Run with Coverage**: `npm run test:coverage`

## Coverage Target
- **Line Coverage**: 85%
- **Branch Coverage**: 80%
- **Critical Paths**: 
  - Contract validation logic (100% coverage required)
  - Schema version selection logic (100% coverage required)
  - Migration application error handling (100% coverage required)
  - CI enforcement logic (100% coverage required)

## Documentation References
- **xUnit Framework**: https://xunit.net/docs/getting-started/netcore/cmdline
- **Moq Mocking Library**: https://github.com/moq/moq4/wiki/Quickstart
- **EF Core Testing**: https://learn.microsoft.com/ef/core/testing/
- **pytest Documentation**: https://docs.pytest.org/en/stable/
- **JSON Schema Validation**: https://python-jsonschema.readthedocs.io/
- **GitHub Actions Testing**: https://docs.github.com/actions/automating-builds-and-tests

## Implementation Checklist
- [ ] Create test file structure per Expected Changes section
- [ ] Set up test data fixtures (contracts, schemas, payloads)
- [ ] Configure mocking dependencies per Mocking Strategy
- [ ] Implement contract validation tests (TC-001 to TC-008, EC-001, EC-002)
- [ ] Implement database migration tests (TC-009 to TC-015, EC-003, EC-004)
- [ ] Implement worker schema versioning tests (TC-016 to TC-023, EC-005, EC-006, ES-001, ES-002)
- [ ] Implement integration tests (TC-024 to TC-026, EC-007, EC-008)
- [ ] Run test suite and validate coverage meets 85% target
- [ ] Document test execution results and coverage report
- [ ] Verify all edge cases and error scenarios covered
