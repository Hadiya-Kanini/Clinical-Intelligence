# Unit Test Plan - Worker Schema Validation

## Requirement Reference
- **User Story**: us_001
- **Story Location**: .propel/context/tasks/us_001/us_001.md
- **Layer**: Worker (Python AI Worker)
- **Related Test Plans**: 
  - test_plan_be_contract_validation.md
  - test_plan_infra_contract_structure.md
- **Acceptance Criteria Covered**:
  - AC-1: Given the system uses asynchronous processing, When the Backend API enqueues work for the AI Worker, Then the job message schema is defined and versioned (fields, required/optional, status transitions) and is the only supported integration for background processing.
  - Edge Case: What happens when the AI Worker receives a job payload missing required fields (e.g., document_id)?
  - Edge Case: How does the Backend API handle an unknown/unsupported job schema version?

## Test Plan Overview
This test plan validates the AI Worker's ability to consume and validate job messages from RabbitMQ according to the versioned job schema contract. Tests ensure proper handling of valid payloads, missing required fields, unsupported schema versions, and graceful error handling with appropriate status transitions.

## Dependent Tasks
- TASK_003: Define backend to worker job schema

## Components Under Test

| Component | Type | File Path | Responsibilities |
|-----------|------|-----------|------------------|
| validate_job_payload | function | worker/main.py | Validates incoming job payloads against JSON schema |
| _load_job_schema | function | worker/main.py | Loads and parses job schema from contracts directory |
| validate_entity_payload | function | worker/main.py | Validates entity extraction results against schema |
| _load_entity_schema | function | worker/main.py | Loads versioned entity schema files |
| JobProcessor | class | worker/processor.py | Processes validated job payloads |

## Test Cases

| Test-ID | Type | Description | Given | When | Then | Assertions |
|---------|------|-------------|-------|------|------|------------|
| TC-001 | positive | Valid job payload passes validation | Worker initialized with schema | validate_job_payload called with valid payload | No exception raised | Function completes successfully |
| TC-002 | positive | Job payload with optional payload field null succeeds | Worker initialized | validate_job_payload with payload: null | No exception raised | Validation passes |
| TC-003 | positive | Job payload with all valid status values succeeds | Worker initialized | validate_job_payload with each valid status | No exception raised | All status enums accepted |
| TC-004 | negative | Job payload missing schema_version fails | Worker initialized | validate_job_payload without schema_version | Raises ValueError | Error message: "Invalid job payload: missing required field 'schema_version'" |
| TC-005 | negative | Job payload missing document_id fails | Worker initialized | validate_job_payload without document_id | Raises ValueError | Error message: "Invalid job payload: missing required field 'document_id'" |
| TC-006 | negative | Job payload missing job_id fails | Worker initialized | validate_job_payload without job_id | Raises ValueError | Error message: "Invalid job payload: missing required field 'job_id'" |
| TC-007 | negative | Job payload missing status fails | Worker initialized | validate_job_payload without status | Raises ValueError | Error message: "Invalid job payload: missing required field 'status'" |
| TC-008 | negative | Job payload with invalid status value fails | Worker initialized | validate_job_payload with status: "invalid_status" | Raises ValueError | Error message: "Invalid job payload" contains "status" |
| TC-009 | negative | Job payload with unsupported schema version fails | Worker initialized | validate_job_payload with schema_version: "2.0" | Raises ValueError | Error message: "Unknown job schema version: 2.0" |
| TC-010 | negative | Job payload with malformed UUID fails | Worker initialized | validate_job_payload with job_id: "not-a-uuid" | Raises ValueError | Error message contains "invalid UUID format" |
| EC-001 | edge_case | Job payload with empty string document_id fails | Worker initialized | validate_job_payload with document_id: "" | Raises ValueError | Error message: "document_id cannot be empty" |
| EC-002 | edge_case | Job payload with extra unknown fields succeeds | Worker initialized | validate_job_payload with extra fields | No exception raised | Validation passes (additionalProperties allowed) |
| EC-003 | edge_case | Job payload with nested payload object succeeds | Worker initialized | validate_job_payload with complex payload | No exception raised | Validation passes |
| ES-001 | error | Schema file not found raises clear error | Schema file missing | _load_job_schema called | Raises FileNotFoundError | Error message: "Job schema file not found at {path}" |
| ES-002 | error | Invalid JSON in schema file raises clear error | Schema file has malformed JSON | _load_job_schema called | Raises ValueError | Error message: "Invalid JSON in job schema file" |
| ES-003 | error | Entity schema with unknown version fails | Worker initialized | _load_entity_schema("2.0") called | Raises ValueError | Error message: "Unknown entity schema version: 2.0" |

## Expected Changes

| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | worker/tests/test_job_validation.py | Unit tests for job payload validation |
| CREATE | worker/tests/test_schema_loading.py | Unit tests for schema loading functions |
| CREATE | worker/tests/test_entity_validation.py | Unit tests for entity payload validation |
| CREATE | worker/tests/fixtures/job_payloads.py | Test data fixtures for job payloads |
| CREATE | worker/tests/fixtures/schemas.py | Mock schema fixtures |
| MODIFY | worker/tests/test_validation.py | Enhance existing validation tests |

## Mocking Strategy

| Dependency | Mock Type | Mock Behavior | Return Value |
|------------|-----------|---------------|--------------|
| open (builtin) | mock | When reading schema files | Return mock schema JSON string |
| os.path.join | mock | When constructing schema paths | Return test schema path |
| jsonschema.validate | spy | Track validation calls | Pass through or raise ValidationError |
| logging.Logger | mock | When logging errors | Verify error logging calls |

## Test Data

| Scenario | Input Data | Expected Output |
|----------|------------|-----------------|
| Valid job payload | `{"schema_version": "1.0", "job_id": "00000000-0000-0000-0000-000000000000", "document_id": "doc-123", "status": "pending", "payload": {}}` | No exception |
| Missing schema_version | `{"job_id": "00000000-0000-0000-0000-000000000000", "document_id": "doc-123", "status": "pending"}` | ValueError: "missing required field 'schema_version'" |
| Missing document_id | `{"schema_version": "1.0", "job_id": "00000000-0000-0000-0000-000000000000", "status": "pending"}` | ValueError: "missing required field 'document_id'" |
| Invalid status | `{"schema_version": "1.0", "job_id": "00000000-0000-0000-0000-000000000000", "document_id": "doc-123", "status": "invalid"}` | ValueError: "Invalid job payload" |
| Unsupported version | `{"schema_version": "2.0", "job_id": "00000000-0000-0000-0000-000000000000", "document_id": "doc-123", "status": "pending"}` | ValueError: "Unknown job schema version: 2.0" |
| Valid entity payload | `{"schema_version": "1.0", "document_id": "doc-123", "extracted_entities": [{"entity_group_name": "patient_demographics", "entity_name": "name", "entity_value": "Jane Doe"}]}` | No exception |

## Test Commands
- **Run Tests**: `pytest worker/tests/test_job_validation.py -v`
- **Run with Coverage**: `pytest worker/tests/ --cov=worker --cov-report=html --cov-report=term`
- **Run Single Test**: `pytest worker/tests/test_job_validation.py::TestJobValidation::test_validate_job_payload_valid_payload_succeeds -v`
- **Run All Worker Tests**: `pytest worker/tests/ -v`

## Coverage Target
- **Line Coverage**: 95%
- **Branch Coverage**: 90%
- **Critical Paths**: 
  - validate_job_payload() - 100% coverage
  - _load_job_schema() - 100% coverage
  - All error handling paths for missing fields - 100% coverage
  - Schema version validation - 100% coverage

## Documentation References
- **Framework Docs**: https://docs.pytest.org/en/stable/
- **Project Test Patterns**: worker/tests/test_validation.py
- **Mocking Guide**: https://docs.python.org/3/library/unittest.mock.html
- **JSON Schema Validation**: https://python-jsonschema.readthedocs.io/

## Implementation Checklist
- [ ] Create test file structure per Expected Changes
- [ ] Set up test data fixtures per Test Data section (job_payloads.py, schemas.py)
- [ ] Configure mocking dependencies per Mocking Strategy
- [ ] Implement positive test cases (TC-001 to TC-003)
- [ ] Implement negative test cases (TC-004 to TC-010)
- [ ] Implement edge case tests (EC-001 to EC-003)
- [ ] Implement error scenario tests (ES-001 to ES-003)
- [ ] Run test suite and validate coverage meets 95% target
