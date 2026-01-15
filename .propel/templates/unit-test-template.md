# Unit Test Plan - [TASK_XXX]

## Requirement Reference
- **User Story**: [us_XXX]
- **Story Location**: [.propel/context/tasks/us_XXX/us_XXX.md]
- **Layer**: [FE/BE/DB/Infra]
- **Related Test Plans**: [Links to other layer test plans for same US]
- **Acceptance Criteria Covered**:
  - [AC-1: Description from user story]
  - [AC-2: Description from user story]

## Test Plan Overview
[High level description of what this test plan covers - purpose and scope]

## Dependent Tasks
- [List of tasks/test plans that must complete before this one]

## Components Under Test

| Component | Type | File Path | Responsibilities |
|-----------|------|-----------|------------------|
| [ComponentName] | [class/function/service/controller] | [src/path/to/file.ts] | [What this component does] |

## Test Cases

| Test-ID | Type | Description | Given | When | Then | Assertions |
|---------|------|-------------|-------|------|------|------------|
| TC-001 | positive | [Test description] | [Precondition/setup] | [Action/trigger] | [Expected outcome] | [Specific assertions to verify] |
| TC-002 | negative | [Invalid input test] | [Invalid state] | [Action] | [Error handling] | [Error message/code] |
| EC-001 | edge_case | [Boundary test] | [Boundary condition] | [Action] | [Expected behavior] | [Boundary validation] |
| ES-001 | error | [Error scenario] | [Error condition] | [Action] | [Graceful handling] | [Error state assertions] |

## Expected Changes

| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | [tests/unit/componentName.spec.ts] | [Unit tests for ComponentName] |
| CREATE | [tests/mocks/serviceName.mock.ts] | [Mock for ServiceName dependency] |
| CREATE | [tests/fixtures/testData.ts] | [Test data fixtures] |

## Mocking Strategy

| Dependency | Mock Type | Mock Behavior | Return Value |
|------------|-----------|---------------|--------------|
| [ServiceName] | mock | [When method X called] | [Return value or throw error] |
| [APIClient] | stub | [Always return] | [Fixed response data] |
| [Logger] | spy | [Track calls] | [Pass through] |

## Test Data

| Scenario | Input Data | Expected Output |
|----------|------------|-----------------|
| [Valid case] | `{ field: "value" }` | `{ result: "success" }` |
| [Invalid case] | `{ field: null }` | `Error: "Field required"` |
| [Edge case] | `{ field: "" }` | `{ result: "empty" }` |

## Test Commands
- **Run Tests**: [command to run tests, e.g., `npm test -- --testPathPattern=componentName`]
- **Run with Coverage**: [e.g., `npm run test:coverage -- --collectCoverageFrom='src/path/**'`]
- **Run Single Test**: [e.g., `npm test -- -t "test name"`]

## Coverage Target
- **Line Coverage**: [X]%
- **Branch Coverage**: [X]%
- **Critical Paths**: [List specific functions/branches that MUST have 100% coverage]

## Documentation References
- **Framework Docs**: [Link to testing framework documentation]
- **Project Test Patterns**: [Link to existing test examples in codebase]
- **Mocking Guide**: [Link to mocking library documentation]

## Implementation Checklist
- [ ] Create test file structure per Expected Changes
- [ ] Set up test data fixtures per Test Data section
- [ ] Configure mocking dependencies per Mocking Strategy
- [ ] Implement positive test cases (TC-XXX)
- [ ] Implement negative test cases
- [ ] Implement edge case tests (EC-XXX)
- [ ] Implement error scenario tests (ES-XXX)
- [ ] Run test suite and validate coverage meets target
