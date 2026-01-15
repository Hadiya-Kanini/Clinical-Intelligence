# Unit Test Plan - Infrastructure Contract Structure

## Requirement Reference
- **User Story**: us_001
- **Story Location**: .propel/context/tasks/us_001/us_001.md
- **Layer**: Infra (Contract Structure & Validation)
- **Related Test Plans**: 
  - test_plan_be_contract_validation.md
  - test_plan_worker_schema_validation.md
- **Acceptance Criteria Covered**:
  - AC-1: Given a new developer opens the repository, When they inspect the solution structure and integration contracts, Then the Web UI, Backend API, and AI Worker boundaries are clearly defined and each has an explicit contract for how it communicates with the others.
  - AC-2: Given a change is proposed to a contract (API schema or job schema), When a developer updates it, Then the change includes a versioning decision (backward compatible vs breaking) and documented migration notes.

## Test Plan Overview
This test plan validates the infrastructure-level contract structure, versioning compliance, and contract governance rules. Tests ensure that contract files exist in the correct locations, follow semantic versioning, include required migration notes, and maintain structural integrity across API and job schemas.

## Dependent Tasks
- TASK_001: Scaffold service structure
- TASK_002: Establish contracts structure and guardrails
- TASK_005: Define contract change process

## Components Under Test

| Component | Type | File Path | Responsibilities |
|-----------|------|-----------|------------------|
| ContractStructureValidator | utility | scripts/validate_contracts.py | Validates contract directory structure and file presence |
| SchemaVersionValidator | utility | scripts/validate_schema_versions.py | Validates schema version compliance with semver |
| MigrationNoteValidator | utility | scripts/validate_migrations.py | Validates migration notes exist and follow template |
| OpenAPIValidator | utility | scripts/validate_openapi.py | Validates OpenAPI spec structure and versioning |
| JSONSchemaValidator | utility | scripts/validate_json_schemas.py | Validates JSON schemas are well-formed |

## Test Cases

| Test-ID | Type | Description | Given | When | Then | Assertions |
|---------|------|-------------|-------|------|------|------------|
| TC-001 | positive | Contract directory structure exists | Repository cloned | Validate contracts/ directory | Structure is valid | contracts/api/, contracts/jobs/, contracts/migrations/ exist |
| TC-002 | positive | API contract v1 files exist | Repository cloned | Check contracts/api/v1/ | Required files present | openapi.yaml, README.md exist |
| TC-003 | positive | Job contract v1 files exist | Repository cloned | Check contracts/jobs/v1/ | Required files present | job.schema.json, README.md exist |
| TC-004 | positive | Migration notes directory exists | Repository cloned | Check contracts/migrations/ | Directory present | README.md, api_v1.md, jobs_v1.md exist |
| TC-005 | positive | OpenAPI spec is valid YAML | contracts/api/v1/openapi.yaml exists | Parse YAML file | Valid YAML structure | No parse errors, contains required fields |
| TC-006 | positive | OpenAPI spec has correct version | OpenAPI file parsed | Check info.version field | Version matches v1 | info.version == "1.0.0" |
| TC-007 | positive | Job schema is valid JSON | contracts/jobs/v1/job.schema.json exists | Parse JSON file | Valid JSON structure | No parse errors, valid JSON Schema draft-07 |
| TC-008 | positive | Job schema has required fields defined | Job schema parsed | Check required array | All required fields present | ["schema_version", "job_id", "document_id", "status"] |
| TC-009 | positive | Job schema version enum is defined | Job schema parsed | Check schema_version property | Enum contains "1.0" | properties.schema_version.enum includes "1.0" |
| TC-010 | positive | Migration note follows template structure | Migration note file exists | Parse migration note | Contains required sections | Sections: Version, Date, Type, Changes, Impact, Migration Steps |
| TC-011 | negative | Missing contracts directory fails validation | contracts/ deleted | Run structure validator | Validation fails | Error: "contracts/ directory not found" |
| TC-012 | negative | Missing API contract fails validation | contracts/api/v1/openapi.yaml deleted | Run structure validator | Validation fails | Error: "API contract v1 not found" |
| TC-013 | negative | Missing job schema fails validation | contracts/jobs/v1/job.schema.json deleted | Run structure validator | Validation fails | Error: "Job schema v1 not found" |
| TC-014 | negative | Invalid YAML in OpenAPI spec fails | OpenAPI has syntax error | Parse OpenAPI file | Parsing fails | YAMLError raised with line number |
| TC-015 | negative | Invalid JSON in job schema fails | Job schema has syntax error | Parse job schema | Parsing fails | JSONDecodeError raised |
| EC-001 | edge_case | Empty migration notes directory is invalid | migrations/ is empty | Run migration validator | Validation fails | Error: "No migration notes found" |
| EC-002 | edge_case | Contract with future version number is valid | Contract has v2.0.0 | Validate version format | Validation passes | Semver format is valid |
| EC-003 | edge_case | Multiple API versions can coexist | v1 and v2 directories exist | Validate structure | Both versions valid | Both pass validation independently |
| ES-001 | error | Malformed semver version fails | Version is "v1" (not semver) | Validate version | Validation fails | Error: "Invalid semver format" |
| ES-002 | error | Missing README in contract directory fails | contracts/api/v1/README.md missing | Validate structure | Validation fails | Error: "README.md required in contract directory" |

## Expected Changes

| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | scripts/validate_contracts.py | Script to validate contract structure |
| CREATE | scripts/validate_schema_versions.py | Script to validate schema versioning |
| CREATE | scripts/validate_migrations.py | Script to validate migration notes |
| CREATE | scripts/validate_openapi.py | Script to validate OpenAPI specifications |
| CREATE | scripts/validate_json_schemas.py | Script to validate JSON schemas |
| CREATE | scripts/tests/test_contract_validation.py | Unit tests for contract validators |
| CREATE | .github/workflows/contract-validation.yml | CI workflow to run contract validation |

## Mocking Strategy

| Dependency | Mock Type | Mock Behavior | Return Value |
|------------|-----------|---------------|--------------|
| os.path.exists | mock | When checking file existence | Return True/False based on test scenario |
| open (builtin) | mock | When reading contract files | Return mock file content |
| yaml.safe_load | spy | Track YAML parsing | Pass through or raise YAMLError |
| json.load | spy | Track JSON parsing | Pass through or raise JSONDecodeError |

## Test Data

| Scenario | Input Data | Expected Output |
|----------|------------|-----------------|
| Valid OpenAPI structure | `openapi: 3.0.0\ninfo:\n  version: "1.0.0"` | Validation passes |
| Invalid OpenAPI version | `openapi: 2.0\ninfo:\n  version: "1.0.0"` | Error: "OpenAPI 3.0+ required" |
| Valid job schema | `{"$schema": "http://json-schema.org/draft-07/schema#", "required": ["schema_version", "job_id", "document_id", "status"]}` | Validation passes |
| Missing required field in schema | `{"$schema": "...", "required": ["schema_version"]}` | Error: "Missing required field: document_id" |
| Valid migration note | `# Version: 1.0.0\n# Date: 2024-01-14\n# Type: Initial\n## Changes\n...` | Validation passes |
| Missing migration sections | `# Version: 1.0.0\n# Date: 2024-01-14` | Error: "Missing required section: Changes" |

## Test Commands
- **Run Tests**: `pytest scripts/tests/test_contract_validation.py -v`
- **Run with Coverage**: `pytest scripts/tests/ --cov=scripts --cov-report=html --cov-report=term`
- **Run Contract Validation**: `python scripts/validate_contracts.py`
- **Run All Validators**: `python scripts/validate_all.py`

## Coverage Target
- **Line Coverage**: 85%
- **Branch Coverage**: 80%
- **Critical Paths**: 
  - Contract structure validation - 100% coverage
  - Schema version validation - 100% coverage
  - Migration note validation - 100% coverage

## Documentation References
- **Semantic Versioning**: https://semver.org/
- **OpenAPI Specification**: https://swagger.io/specification/
- **JSON Schema**: https://json-schema.org/draft-07/schema
- **Project Contract Docs**: contracts/README.md

## Implementation Checklist
- [x] Create validation script structure per Expected Changes
- [x] Set up test data fixtures per Test Data section
- [x] Configure mocking dependencies per Mocking Strategy
- [x] Implement positive test cases (TC-001 to TC-010)
- [x] Implement negative test cases (TC-011 to TC-015)
- [x] Implement edge case tests (EC-001 to EC-003)
- [x] Implement error scenario tests (ES-001 to ES-002)
- [x] Run test suite and validate coverage meets 85% target
