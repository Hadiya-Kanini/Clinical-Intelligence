---
description: Generates comprehensive E2E / feature specific test plan requirements covering functional, NFR, technical, and data validations
auto_execution_mode: 1
---

# E2E Test Plan Generator

As an expert Senior QA Engineer and Test Strategist, generate comprehensive E2E test plans that ensure complete coverage of functional requirements, non-functional requirements, technical integrations, and data validations. This workflow produces ISTQB-aligned test plans with full traceability.

## Input Parameters

### $ARGUMENTS (Optional)
**Accepts:** spec.md path | design.md path | Feature name | Use Case ID (UC-XXX)
**Default:** `.propel/context/docs/spec.md` and `.propel/context/docs/design.md`

### --scope (Optional)
**Accepts:** `full` | `critical` | `regression` | `feature:<name>`
**Default:** `full`

| Value | Behavior |
|-------|----------|
| `full` | Complete system test plan covering all requirements |
| `critical` | P0/P1 critical paths only - focused test plan |
| `regression` | Stable features for regression testing |
| `feature:<name>` | Single feature/module test plan (e.g., `feature:login`) |

### Input Processing Instructions

**Step 1: Determine Sources**
| Input | Action |
|-------|--------|
| Empty | Use `.propel/context/docs/spec.md` + `.propel/context/docs/design.md` |
| File path | Read specified file(s) |
| `UC-001` | Find UC in spec.md, related NFR/TR/DR in design.md |
| `Login` | Search spec.md + design.md for matching requirements |

**Step 2: Validate Input**
- If file path: Verify files exist
- If UC ID: Verify UC-XXX exists in spec.md
- If feature name: Find matching Use Cases and requirements

## Output

- **File**: `.propel/context/test/test_plan_[feature]_[YYYYMMDD].md`
- **Template**: `.propel/templates/test-plan-template.md`
- **Print** (console only):
  - Rules used (bulleted list)
  - Evaluation Scores (table)
  - Summary (<100 words)

**Note:** If output file exists, update applicable sections. Do not overwrite.

## Core Principles

- **Traceability First**: Every test case links to FR-XXX, UC-XXX, NFR-XXX, TR-XXX, or DR-XXX
- **Risk-Based Prioritization**: P0/P1 focus on high-impact, high-likelihood scenarios
- **Test Pyramid Compliance**: E2E limited to 5-10% covering critical journeys only
- **Full Given/When/Then**: Every scenario includes complete steps with test data
- **NFR Coverage**: Performance, security, and scalability specifications included

## Execution Flow

### Deep Research Methodology

**MCP Tools Required:**
- `mcp__sequential-thinking__sequentialthinking` - Requirement analysis, risk assessment
- `mcp__context7__resolve-library-id` - Pin Playwright/testing framework versions
- `mcp__context7__get-library-docs` - Fetch E2E best practices

**Fallback:** If MCP unavailable, use web search for "E2E test plan best practices 2026"

### Phase 1: Requirements Analysis
**Use:** `mcp__sequential-thinking__sequentialthinking`

1. Read `.propel/context/docs/spec.md`
   - Extract all Functional Requirements (FR-XXX)
   - Extract all Use Cases (UC-XXX) with success scenarios and alternatives
   - Identify actors, preconditions, postconditions

2. Read `.propel/context/docs/design.md`
   - Extract Non-Functional Requirements (NFR-XXX): performance, security, scalability
   - Extract Technical Requirements (TR-XXX): integrations, APIs, platform
   - Extract Data Requirements (DR-XXX): integrity, retention, migrations

3. Apply --scope filter:
   | Scope | Action |
   |-------|--------|
   | `full` | Include all requirements |
   | `critical` | Filter to P0/P1 priority only |
   | `regression` | Filter to stable, baselined features |
   | `feature:<name>` | Filter to matching feature requirements |

### Phase 2: Risk Assessment
**Use:** `mcp__sequential-thinking__sequentialthinking`

1. Identify high-risk areas based on:
   - Business criticality (user-facing, revenue-impacting)
   - Technical complexity (integrations, new technology)
   - Historical defect density (if `.propel/learnings/findings-registry.md` exists)
   - Security sensitivity (authentication, authorization, data handling)

2. Assign risk scores:
   | Impact | Likelihood | Priority |
   |--------|------------|----------|
   | High | High | P0 |
   | High | Medium | P0 |
   | Medium | High | P1 |
   | Medium | Medium | P1 |
   | Low | Any | P2 |
   | Any | Low | P2 |

3. Document risks in Risk Assessment section

### Phase 3: Test Strategy Definition
**Use:** `mcp__context7__get-library-docs`

1. Define test pyramid allocation:
   - E2E: 5-10% (critical user journeys)
   - Integration: 20-30% (API contracts, service boundaries)
   - Unit: 60-70% (business logic, edge cases)

2. Identify E2E approach:
   - **Horizontal**: UI-driven user flows
   - **Vertical**: API → DB validation for data integrity

3. Define environment strategy:
   - DEV: Smoke tests with mocked/seeded data
   - QA: Full regression with snapshot data
   - Staging: Pre-prod validation with prod-like data

4. Pin testing framework versions using `mcp__context7__resolve-library-id`

### Phase 4: Scenario Decomposition
**Use:** `mcp__sequential-thinking__sequentialthinking`

**Before writing test cases, list all scenarios to generate:**
| TC-ID | Requirement | Scenario Summary | Type | Priority |
|-------|-------------|------------------|------|----------|
| ... | ... | ... | ... | ... |

**Now expand each scenario listed above.**

For each requirement type, generate test scenarios:

**4.1 Functional Requirements (FR-XXX)**
For each FR, generate:
| Type | ID Pattern | Focus |
|------|------------|-------|
| Happy Path | TC-FR-XXX-HP | Normal successful flow |
| Edge Case | TC-FR-XXX-EC | Boundary conditions |
| Error Case | TC-FR-XXX-ER | Invalid inputs/failures |

**4.2 Use Cases (UC-XXX)**
For each UC, generate:
- Happy path covering main success scenario
- Alternative flows from extensions
- Error handling from exception paths

**4.3 Non-Functional Requirements (NFR-XXX)**
For each NFR category:
| Category | Test Type | Tool |
|----------|-----------|------|
| Performance | Load test | k6/JMeter/Artillery |
| Security | Vulnerability scan | OWASP ZAP/Burp Suite |
| Scalability | Stress test | k6/Locust |

**4.4 Technical Requirements (TR-XXX)**
For each TR:
- Contract compliance tests
- Integration validation
- Error handling verification

**4.5 Data Requirements (DR-XXX)**
For each DR:
- Data integrity tests
- Referential integrity verification
- Audit trail validation

### Phase 5: E2E Journey Identification
**Use:** `mcp__sequential-thinking__sequentialthinking`

1. Identify critical user journeys spanning multiple Use Cases
2. Map journey: UC-001 → UC-002 → UC-003
3. Define session requirements (auth, state persistence)
4. Identify shared test data across journey phases
5. Define checkpoints for intermediate state validation

**E2E Journey Pattern:**
| Journey-ID | Name | UC Chain | Business Value | Priority |
|------------|------|----------|----------------|----------|
| E2E-001 | [Journey Name] | UC-001 → UC-002 | [Critical value] | P0 |

### Phase 6: Traceability Matrix Generation

Create bidirectional traceability:
- Requirement → Test Cases
- Test Case → Requirements
- E2E Journey → Use Cases

Validate coverage:
- All FR-XXX have at least one test case
- All UC-XXX have happy path + edge case coverage
- All NFR-XXX have validation approach defined
- All TR-XXX have integration tests
- All DR-XXX have data validation tests

### Phase 7: Template Population

1. Read `.propel/templates/test-plan-template.md`
2. Populate all sections with generated content
3. Include full Given/When/Then for each scenario
4. Include YAML test data specifications
5. Write to `.propel/context/test/test_plan_[feature]_[YYYYMMDD].md`

## Guardrails

- `rules/playwright-testing-guide.md`: Test independence, wait strategies
- `rules/playwright-standards.md`: Locator priority, anti-patterns
- `rules/playwright-typescript-guide.md`: Code quality, assertions
- `rules/unit-testing-standards.md`: Test patterns, coverage
- `rules/language-agnostic-standards.md`: KISS, YAGNI
- `rules/security-standards-owasp.md`: OWASP Top 10 alignment
- `rules/markdown-styleguide.md`: Front matter, heading hierarchy

**>>> MANDATORY: Execute Quality Evaluation and Detection Rules. IF any gate fails, execute Self-Healing. <<<**

**Execution Steps:**
1. Score each dimension in the Quality Assessment table below
2. Apply Detection Rules to validate scores and calculate penalties
3. IF any MUST gate failed OR any score is below its threshold: Execute Self-Healing Protocol
4. Print the completed evaluation table with final scores
5. Print the Overall Score and Evaluation Summary

## Quality Evaluation

### Test Plan Quality Assessment

| # | Dimension | Score | Gate | Criteria |
|---|-----------|-------|------|----------|
| 1 | Template Adherence | [0-100%] | MUST=100% | Follows test-plan-template.md structure |
| 2 | FR Coverage | [0-100%] | >=80% | All FR-XXX have test scenarios |
| 3 | UC Coverage | [0-100%] | >=80% | All UC-XXX have HP+EC tests |
| 4 | NFR Coverage | [0-100%] | >=70% | NFR-XXX have validation specs |
| 5 | TR Coverage | [0-100%] | >=70% | TR-XXX have integration tests |
| 6 | DR Coverage | [0-100%] | >=70% | DR-XXX have data validation tests |
| 7 | Risk Alignment | [0-100%] | >=80% | High-risk areas prioritized P0/P1 |
| 8 | Traceability | [0-100%] | >=90% | All TC-XXX link to requirements |
| 9 | E2E Journey Coverage | [0-100%] | >=80% | Critical journeys have E2E tests |
| 10 | GWT Completeness | [0-100%] | >=90% | All scenarios have Given/When/Then |

### Detection Rules

| # | Detection Logic | Penalty |
|---|-----------------|---------|
| 1 | Compare vs test-plan-template.md sections | Missing: -20%; Extra: -15% |
| 2 | Count FR in spec vs TC-FR coverage | Uncovered FR: -15%/instance |
| 3 | Each UC has HP+EC tests | Missing type: -10%/instance |
| 4 | NFR has performance/security specs | Missing spec: -15%/instance |
| 5 | TR has integration test defined | Missing: -10%/instance |
| 6 | DR has data validation test | Missing: -10%/instance |
| 7 | High-risk items are P0/P1 | Misaligned priority: -10%/instance |
| 8 | TC-XXX references requirement | Orphan TC: -10%/instance |
| 9 | Critical UC chains have E2E | Missing journey: -15%/instance |
| 10 | Scenarios have Given/When/Then | Incomplete: -10%/instance |

**Overall Score**: [Weighted Average]%

**Evaluation Summary** (Top 3 Weaknesses):
1. **[Lowest]** ([X]%): [Reason]
2. **[2nd Lowest]** ([X]%): [Reason]
3. **[3rd Lowest]** ([X]%): [Reason]

**Critical Failures**: [List MUST gates that failed, or "None"]

### Self-Healing Protocol

**Retry Limits:** 1 patch per dimension | Mode: Edit-in-place

**Patchable:**
| Dimension | Patch Action |
|-----------|--------------|
| Template Adherence | Append missing section |
| FR Coverage | Add test scenarios for uncovered FR |
| UC Coverage | Add HP/EC tests for uncovered UC |
| NFR Coverage | Add performance/security specs |
| TR Coverage | Add integration test definitions |
| DR Coverage | Add data validation tests |
| Risk Alignment | Adjust priorities for high-risk items |
| Traceability | Add requirement links to TC |
| E2E Journey Coverage | Add E2E journeys for critical flows |
| GWT Completeness | Add Given/When/Then to incomplete scenarios |

**Non-Patchable:** Source spec/design missing, patch >30% of section, >=3 failing dimensions

**Retry Flow:**
```
IF output file missing: ABORT
READ existing output
FOR each failing dimension:
  IF patchable AND patch < 30%: EDIT in-place; RE-EVALUATE once
  ELSE: REPORT "[MANUAL FIX] {dimension}"
IF >=3 manual fixes needed: ABORT all retries
```

---

*Generates comprehensive E2E test plans with full requirement traceability and NFR validation specifications.*
