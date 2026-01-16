---
description: Generates comprehensive unit test plans from user story specifications with test coverage analysis, test case design, and validation strategies
auto_execution_mode: 1
---

# Unit Test Plan Generator
As an expert QA Engineer and Test Automation Specialist, generate comprehensive unit test plans that ensure code quality, coverage, and maintainability. This command focuses specifically on unit test planning with detailed test case design and validation strategies.

## Input Parameters: $ARGUMENTS (Mandatory)
**Accepts:** User story file path | User story ID | User story URL | User story text

### Argument Types:
1. **User Story File**: Path to us_XXX.md file (e.g., .propel/context/tasks/us_001/us_001.md)
2. **User Story ID**: US ID pattern (us_001, us_002, etc.)
3. **User Story URL**: URL pointing to user story specification
4. **User Story Text**: Direct user story content in "As a... I want... so that..." format

### User Story Input Processing
**When $ARGUMENTS contains user story (file, URL, or text):**
1. **Extract US ID**: Parse us_XXX identifier from file path, content, or generate if text input
2. **Create Test Plan Folder**: Ensure `.propel/context/tasks/us_<ID>/unittest/` directory exists
3. **Test Plan File Naming**: Generate plans as `test_plan_<layer>_<descriptive_name>.md` within unittest folder
4. **Parent Story Reference**: Maintain traceability to parent user story in all test plans
5. **Acceptance Mapping**: Map test cases to user story acceptance criteria

### US ID Extraction Algorithm
**File Input**: Extract us_XXX from file path using pattern matching
- Pattern: `/us_(\d{3})/` or `us_(\d{3})\.md`
- Example: `.propel/context/tasks/us_001/us_001.md` -> Extract `us_001`

**URL Input**:
- First attempt: Parse URL path for us_XXX pattern
- If not found: Fetch URL content and search for US ID in content
- Pattern: `US[_-]?(\d{3,4})`

**Text Input**:
- Search for existing US ID in text: `US[_-]?(\d{3,4})`

**Fallback Behavior**:
- If no US ID can be extracted -> Create test plans in `.propel/context/tasks/us_<short_descriptive_name>/unittest/`
- Log clearly where test plans are being created (US folder vs. fallback folder)

## Output
- Artifact generation: `.propel/context/tasks/us_<ID>/unittest/test_plan_*.md`
- Print the following:
    - List of rules used by the workflow in bulleted format
    - Evaluation Scores in tabular format with average score.
    - Evaluation summary (less than 100 words).
    **Do not save as file.**

**Note:**
- If the output file is already available make the necessary changes to applicable sections. Do not overwrite the whole file.
- Always generate a single unified unit test plan document.
- Generate the output using the `.propel/templates/unit-test-template.md` template.

### Output Specifications

#### File Organization Strategy:
**For User Story Unit Test (US ID found in input):**
- **Directory**: `.propel/context/tasks/us_<ID>/unittest/`
- **File Pattern**: `test_plan_<layer>_<descriptive_name>.md` (layer: fe, be, db, infra)
- **Example**: `.propel/context/tasks/us_001/unittest/test_plan_fe_login_ui.md`
- **Trigger**: Valid us_XXX pattern extracted from file path, URL, or text content

**For General Unit Test (no US ID found - fallback):**
- **Directory**: `.propel/context/tasks/us_<short_descriptive_name>/unittest/`
- **File Pattern**: `test_plan_<layer>_<descriptive_name>.md`
- **Example**: `.propel/context/tasks/us_setup_db/unittest/test_plan_be_setup_database.md`
- **Trigger**: No valid US ID extractable from input -> automatic fallback to root folder

#### Unit Test Organization Examples
```
.propel/context/tasks/
+-- us_001/
|   +-- us_001.md
|   +-- task_001_implement_login_ui.md
|   +-- task_002_implement_auth_api.md
|   +-- unittest/
|       +-- test_plan_fe_login_form.md
|       +-- test_plan_be_auth_api.md
|       +-- test_data/
|       +-- mocks/
|       +-- coverage/
+-- us_002/
|   +-- us_002.md
|   +-- task_001_password_reset.md
|   +-- unittest/
|       +-- test_plan_fe_password_reset.md
|       +-- test_plan_be_password_reset.md
+-- us_general_setup/
|   +-- unittest/
|       +-- test_plan_infra_setup.md
```

**Additional Artifacts:**
- **Test Data Files**: `.propel/context/tasks/us_<ID>/unittest/test_data/`
- **Mock Specifications**: `.propel/context/tasks/us_<ID>/unittest/mocks/`
- **Coverage Reports**: `.propel/context/tasks/us_<ID>/unittest/coverage/`

## Core Principles
- **Acceptance Criteria Mapping**: Every test case traces to specific acceptance criteria
- **Test Independence**: Each test must be isolated and independently executable
- **Coverage Completeness**: Include happy path, edge cases, and error scenarios

### Task Decomposition Rule
- **Maximum Checklist Items Per Test Plan**: If checklist items exceed 8 for a single test plan, the plan MUST be decomposed into smaller layer-specific plans
- **Decomposition Strategy**:
  1. Group tests by technology layer (FE, BE, DB, Infra)
  2. Split complex plans into independently testable units
  3. Ensure each decomposed plan remains under 8 hours of effort
  4. Maintain traceability: Test Plan -> User Story -> Epic
- **Validation Gate**: Before finalizing test plan generation, count checklist items. If > 8, trigger decomposition process

## Execution Flow

### 1. User Story Analysis
- **Acceptance Criteria Mapping**: Extract all Given/When/Then scenarios from user story
- **Edge Case Identification**: Enumerate all boundary conditions and error scenarios
- **Business Logic Extraction**: Identify core business rules requiring validation
- **Component Identification**: List all functions, methods, classes, and modules to be tested
- **Dependency Analysis**: Map external dependencies, mocks, and test doubles needed

#### 2. Codebase Intelligence Gathering
- **Implementation Task Review**: Read all test_XXX files in .propel/context/tasks/us_XXX/ folder
- **Pattern Discovery**: Search for existing unit test patterns in codebase
- **Framework Detection**: Identify testing framework used (Jest, pytest, xUnit, JUnit, etc.)
- **Test Structure Analysis**: Review existing test file organization and naming conventions
- **Coverage Strategy**: Determine code coverage tools and targets used in project
- **Mock Strategy**: Identify mocking libraries and patterns (unittest.mock, Jest mocks, Mockito, etc.)

#### 3. Technology Stack Detection
**CRITICAL**: Identify technology stack from implementation tasks and codebase:
- **Frontend Frameworks**: React, Angular, Vue, Svelte (use Jest, Vitest, Karma)
- **Backend Frameworks**: Node.js, Python, .NET, Java, Go (use appropriate test frameworks)
- **Database Layer**: SQL, NoSQL, ORM testing strategies
- **API Layer**: REST, GraphQL, gRPC testing approaches
- **Version Pinning**: Use `mcp__context7__resolve-library-id` to pin exact testing framework versions

### Deep Research Methodology

Optimize for test quality and comprehensive coverage. Use Context7 MCP for version-specific testing framework documentation.

#### 1. Testing Framework Research
- **Framework Detection**: Identify testing framework from package.json, requirements.txt, or project files
- **Version Pinning**: Use Context7 MCP (`mcp__context7__resolve-library-id`) for exact framework version
- **Documentation Retrieval**: Use Context7 MCP (`mcp__context7__get-library-docs`) for testing patterns
- **Best Practices**: Research framework-specific best practices and anti-patterns
- **Mock Library Research**: Identify and document mocking libraries and patterns

#### 2. Existing Test Analysis
- **Test Pattern Discovery**: Search for existing unit test files in codebase
- **Convention Extraction**: Document naming conventions, file structure, test organization
- **Assertion Patterns**: Identify assertion styles and validation approaches
- **Setup/Teardown Patterns**: Document test lifecycle management approaches
- **Coverage Configuration**: Review existing coverage targets and configurations

#### 3. Component Dependency Analysis
- **Dependency Mapping**: Identify all dependencies for components under test
- **Mock Candidates**: Determine which dependencies should be mocked
- **Test Double Strategy**: Define mocks, stubs, spies, fakes for each dependency
- **Integration Points**: Identify boundaries between unit and integration testing

#### MCP Fallback Protocol
**If Context7 MCP unavailable:**
- Use web search: "[testing framework] [version] official documentation [current year]"
- Document: "Fallback: Web search for docs (Context7 unavailable)"

**If both unavailable:**
- Use package.json/requirements.txt for framework version detection
- Search codebase for existing test patterns
- Document: "Fallback: Codebase analysis only (MCP unavailable)"

### Unit Test Planning Methodology

#### Test Plan Structure

##### 1. Test Scope Definition
**User Story Context:**
- **US ID**: [us_XXX]
- **User Story Title**: [Extracted from user story]
- **User Story Description**: [As a... I want... so that...]
- **Implementation Tasks**: [List of task files in us_XXX folder]

**Testing Objectives:**
- **Primary Goal**: [What aspect of functionality is being validated]
- **Coverage Target**: [Percentage or specific components]
- **Quality Gates**: [Pass criteria for test suite]

##### 2. Test Component Identification
**Components Under Test:**

| Component | Type | File Path | Priority | Mock Dependencies |
|-----------|------|-----------|----------|-------------------|
| [ComponentName] | [class/function/service] | [src/path/to/file.ts] | [high/med/low] | [deps to mock] |

##### 3. Test Case Design

**Before writing test cases, list all tests to generate:**
| Test-ID | Summary | Type |
|---------|---------|------|
| ... | ... | ... |
**Now expand each test listed above.**

**Test Case Table Format** (use template structure):

| Test-ID | Type | Description | Given | When | Then | Assertions |
|---------|------|-------------|-------|------|------|------------|
| TC-001 | positive | [Description] | [Precondition] | [Action] | [Expected] | [Assertions] |
| TC-002 | negative | [Description] | [Invalid state] | [Action] | [Error] | [Error check] |
| EC-001 | edge_case | [Boundary] | [Boundary condition] | [Action] | [Behavior] | [Validation] |

#### Test Coverage Analysis

##### 1. Code Coverage Metrics

| Metric | Target | Critical Paths |
|--------|--------|----------------|
| Line Coverage | 80% | 95% |
| Branch Coverage | 75% | - |

### Context Documentation Requirements

**Essential References:**
- **User Story**: `.propel/context/tasks/us_XXX/us_*.md` (Acceptance Criteria, Edge Cases)
- **Implementation Tasks**: `task_*.md` files in us_XXX folder
- **Testing Framework Docs**: Framework documentation (version-pinned via Context7 or web search)
- **Project Conventions**: Existing test patterns, code standards, coverage config

### Implementation Instructions

#### Automated Test Plan Generation Flow
```
1. Parse $ARGUMENTS to determine input type (file/ID/URL/text)
2. Extract user story content based on argument type
3. Parse US ID using pattern matching (us_XXX)
4. Read implementation tasks from .propel/context/tasks/us_XXX/ folder
5. Analyze codebase to identify:
   a. Technology stack and testing framework
   b. Existing test patterns and conventions
   c. Component locations and dependencies
6. Pin testing framework version using Context7
7. Extract acceptance criteria and edge cases from user story
8. Design test cases for each criterion. For each, answer: (1) What is being tested? (2) What inputs? (3) Expected output? (4) Edge cases?
9. Identify mock and stub requirements
10. Define test data strategy
11. Generate test plan document
12. Create directory structure: .propel/context/tasks/us_<ID>/unittest/
13. Write test_plan_<layer>_<descriptive_name>.md file
14. Validate test plan completeness
15. Report test plan generation summary
```

#### Error Handling and Recovery
**Common Error Scenarios:**
- **Missing User Story**: Request user to provide valid user story path or ID
- **Invalid US ID**: Validate US ID format and request correction
- **Inaccessible URL**: Request alternative source or local file
- **Empty/Invalid Content**: Ask for clarification or additional detail
- **No Implementation Tasks**: Generate basic test plan based on user story alone
- **Unknown Technology Stack**: Request user to specify testing framework

#### Output Confirmation
After successful test plan generation:
1. **User Story Summary**: Report which user story was analyzed
2. **Test Coverage Summary**: List total test cases generated by category
3. **Component Coverage**: Report components identified for testing
4. **File Confirmation**: Confirm test plan file created successfully
5. **Next Steps**: Suggest follow-up actions (implement tests, run coverage, etc.)

**Example Output Messages:**
- "Generated unit test plan for US-001 with 15 test cases covering 3 components"
- "Test plan created at .propel/context/tasks/us_001/unittest/test_plan_fe_login.md"
- "Identified Jest v29.7 as testing framework with mocking requirements for 4 dependencies"
- "Expected code coverage: 85% (12 test cases for acceptance criteria, 3 for edge cases)"


### Test Implementation Blueprint

#### Test File Location
- **Location**: `.propel/context/tasks/us_<ID>/unittest/`
- **Naming**: `test_<component_name>.<ext>` (e.g., `test_login_service.spec.ts`)

#### Mocking Strategy

| Dependency | Mock Type | Behavior | Return Value |
|------------|-----------|----------|--------------|
| [ServiceName] | mock/stub/spy | [When method called] | [Return or throw] |

#### Test Data

| Scenario | Input | Expected Output |
|----------|-------|-----------------|
| [Valid case] | `{ field: "value" }` | `{ result: "success" }` |
| [Edge case] | `{ field: "" }` | `Error: "Field required"` |

### Quality Assurance Framework

#### Test Plan Validation Checklist
Before completing test plan generation, validate:

**Coverage Validation:**
- [ ] All acceptance criteria have corresponding test cases
- [ ] All edge cases from user story are covered
- [ ] All error scenarios are tested
- [ ] All public methods/functions have test coverage
- [ ] All business logic branches are tested

**Test Design Validation:**
- [ ] Test cases are independent and isolated
- [ ] Test cases have clear Given/When/Then structure
- [ ] Assertions are specific and measurable
- [ ] Test data is realistic and representative
- [ ] Mocks are properly defined and scoped

**Technology Validation:**
- [ ] Testing framework version is pinned via Context7
- [ ] Test patterns match project conventions
- [ ] Test file locations follow project structure
- [ ] Naming conventions are consistent
- [ ] Framework-specific best practices are followed

**Implementation Validation:**
- [ ] Test plan references implementation tasks
- [ ] Component paths are accurate
- [ ] Dependencies are correctly identified
- [ ] Mock requirements are documented
- [ ] Test execution order is defined

## Guardrails
- `rules/ai-assistant-usage-policy.md`: Explicit commands; minimal output
- `rules/code-anti-patterns.md`: Avoid god objects, circular deps, magic constants
- `rules/dry-principle-guidelines.md`: Single source of truth; delta updates
- `rules/iterative-development-guide.md`: Strict phased workflow
- `rules/language-agnostic-standards.md`: KISS, YAGNI, size limits, clear naming
- `rules/markdown-styleguide.md`: Front matter, heading hierarchy, code fences
- `rules/performance-best-practices.md`: Optimize after measurement
- `rules/security-standards-owasp.md`: OWASP Top 10 alignment
- `rules/software-architecture-patterns.md`: Pattern selection, boundaries
- `rules/unit-testing-standards.md`: Unit test plan scaffold

**>>> MANDATORY: Execute Quality Evaluation and Detection Rules. IF any gate fails, execute Self-Healing. <<<**

**Execution Steps:**
1. Score each dimension in the Quality Assessment table below
2. Apply Detection Rules to validate scores and calculate penalties
3. IF any MUST gate failed OR any score is below its threshold: Execute Self-Healing Protocol (Retry Flow)
4. Print the completed evaluation table with final scores
5. Print the Overall Score and Evaluation Summary

## Quality Evaluation

Once the output is generated, Score the generated test plan to evaluate its quality against the following metrics, providing a percentage score (1-100%) for each.

### Unit Test Plan Quality Assessment

| # | Evaluation Dimension | Score | Gate | Assessment Criteria |
|---|---------------------|-------|------|---------------------|
| 1 | Template Adherence | [0-100%] | MUST=100% | Output follows unit-test-template.md structure; no extra sections |
| 2 | Test Independence | [PASS/FAIL] | MUST PASS | Tests are isolated and can run independently |
| 3 | Coverage Completeness | [0-100%] | >=80% | All acceptance criteria covered with test cases |
| 4 | Assertion Clarity | [0-100%] | >=80% | Assertions are specific and measurable |
| 5 | Mock Appropriateness | [0-100%] | >=80% | Mocking strategy is correct for dependencies |
| 6 | Framework Compliance | [0-100%] | >=80% | Follows framework-specific best practices |
| 7 | Task Decomposition | [0-100%] | >=80% | No plan >8 checklist items; no plan >8 hours effort |
| 8 | Layer Purity | [0-100%] | >=80% | Each plan contains only its layer tests; no cross-layer contamination |

### Detection Rules

| # | Detection Logic | Penalty |
|---|-----------------|---------|
| 1 | Compare sections vs unit-test-template.md | Missing: -20%; Extra: -15% |
| 2 | Tests have no shared state; each can run standalone | Shared state: BLOCKED |
| 3 | Count acceptance criteria; verify test cases cover each | Uncovered criterion: -15%/instance |
| 4 | Assertions have specific expected values; no vague checks | Vague assertion: -10%/instance |
| 5 | Mocks for external deps only; no internal mocking | Inappropriate mock: -10%/instance |
| 6 | Test patterns match framework conventions | Convention violation: -10%/instance |
| 7 | Checklist items <=8; effort <=8 hours | >8 checklist: -10%/extra item; >8 hours: -15%/plan |
| 8 | Scan test cases for cross-layer activities | Cross-layer test: -15%/instance |

**Overall Score**: [Weighted Average]%

**Evaluation Summary** (Top 3 Weaknesses):
1. **[Lowest Dimension]** ([X]%): [Specific reason]
2. **[2nd Lowest]** ([X]%): [Specific reason]
3. **[3rd Lowest]** ([X]%): [Specific reason]

**Critical Failures**: [List any MUST gates that failed, or "None"]

### Self-Healing Protocol

**Retry Limits:** 1 patch attempt per failing dimension | Mode: Edit-in-place only

**Patchable:**
| Dimension | Patch Action |
|-----------|--------------|
| Template Adherence | Append missing section to file |
| Coverage Completeness | Add test cases for uncovered criteria |
| Assertion Clarity | Rewrite vague assertions |
| Mock Appropriateness | Fix mocking strategy |
| Framework Compliance | Fix convention violations |
| Task Decomposition | Split plan into layer-specific files |
| Layer Purity | Move cross-layer tests to appropriate layer plan |

**Non-Patchable (Skip):** Test Independence (structural), source missing, hallucinated content, patch >30% of section, >=3 failing dimensions

**Retry Flow:**
```
IF output file missing: ABORT (no retry)
READ existing output
FOR each failing dimension:
  IF patchable AND patch < 30%: EDIT in-place; RE-EVALUATE once
  ELSE: REPORT "[MANUAL FIX] {dimension}"
IF >=3 manual fixes needed: ABORT all retries
```

---

*This unit test plan generator ensures comprehensive test coverage with detailed test case design, proper mocking strategies, and technology-specific best practices for successful test-driven development.*