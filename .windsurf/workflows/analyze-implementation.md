---
description: Review completed code changes against task requirements to verify scope alignment, identify gaps, and provide quality assessment with actionable recommendations.
auto_execution_mode: 1
---

# Implementation analysis command
As a Senior Software Engineer expert in Full Stack development, review the implementation against the task. This unified command handles verification of task requirements, scope alignment with consistent quality to identify the gaps and suggest the actionable recommendations.

## Input Parameter (Task File)s: $ARGUMENTS (Mandatory)
**Accepts:** Task file path

**Required Parameters:**
- `task_file_path`: Path to the task file that defines requirements, acceptance criteria, and validation gates (e.g., `.propel/context/tasks/task_001_signin.md`)

**Optional Parameters:**
- `analysis_depth`: "quick" | "standard" | "comprehensive" (default: "standard") - comprehensive mode triggers full analysis of all implications
- `focus_areas`: Specific areas to emphasize with deep analysis (e.g., "security,testing,performance") - each area will be probed extensively for hidden issues

### Parameter Validation
- Validate task file path and accessibility
- Parse basic task file structure to ensure it's processable
- Verify repository context and related code accessibility

## Output
- Artifact generation: 
  - `.propel/context/task-reviews/task-review-${timestamp}.md`
- Print the following: 
  - List of rules used by the workflow in bulleted format
  - Evaluation Scores in tabular format with average score. 
  - Evaluation summary (less than 100 words).
  **Do not save as file.** (console output only)

**Note:**
- If the output file is already available make the necessary changes to applicable sections. Do not overwrite the whole file.
- Always create the output file in manageable smaller chunks to manage memory and processing constraints.
- Always generate a single unified document using the 
  - `.propel/templates/task-analysis-template.md` template

## Execution Flow

### Core Principles
- Analyze tasks for requirements verification, test coverage analysis, security compliance, and integration validation
- Gather framework documentation and best practices simultaneously using Context7 MCP
- Map task requirements to actual implementation files and functions
- Perform stepwise reasoning (use Sequential-Thinking MCP) throug complex requirement verfication
- Trace task specifications against actual implementation methodically
- Build layered understanding of business logic correctness and completeness
- Develop causal chains linking requirements to code quality and test coverage

### Implementation Workflow
- **Task File Analysis**: Parse requirements, acceptance criteria, validation gates, todos
- **Code Discovery**: Grep for features/endpoints/components referenced by task
- **Framework Documentation**: Fetch targeted API/guide excerpts for versions in use
- **Pattern Analysis**: Map controller -> service -> repository (backend) and view -> state -> API client (frontend)
- **Infrastructure Changes**: Identify migrations/SQL and configuration changes

#### Requirements Alignment Analysis
- Derive detailed checklist from acceptance criteria and non-functional constraints
- Map each requirement to concrete files, functions, and code lines
- Identify missing implementations and scope gaps
- Assess business logic correctness and completeness 

#### Repository Scanning
- Locate implementation files based on task references
- Identify related test files and coverage areas
- Map system integration points and dependencies
- Extract validation commands from task file

#### Gap Analysis
- **Missing Features**: Identify unimplemented requirements
- **Incomplete Logic**: Highlight logical or business-logic errors
- **Test Gaps**: Missing unit tests, integration tests, edge cases
- **Documentation Gaps**: Missing or outdated documentation
- **Security Gaps**: Missing error handling, input validation, security headers

#### Risk Analysis
- Identify high-risk code areas and potential failure points
- Evaluate security vulnerabilities and performance impacts
- Analyze transaction boundaries and data consistency
- Review authentication, authorization, and role-based access

### Summary Presentation
Produce comprehensive task analysis reports including:
    - Executive summary with pass/fail verdict and critical issues
    - Requirements traceability matrix mapping specifications to implementation
    - Quality assessment scorecard across all evaluation dimensions
    - Gap analysis identifying missing features, incomplete logic, and test coverage
    - Risk register with high-priority risks and mitigation strategies
    - Prioritized action plan with effort estimates and file-specific recommendations

**Note:** Do not create any file to store the summary presentation.

### Quality Assurance Framework
- Evaluate code quality, maintainability, and pattern adherence
- Analyze error handling, logging, and security implementation
- Review testing coverage and test quality
- Assess integration impact and backwards compatibility

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

### Extended Standards (apply based on task domain)
- `rules/code-documentation-standards.md`: Comment WHY, not WHAT
- `rules/react-development-standards.md`: React component patterns
- `rules/typescript-styleguide.md`: TypeScript typing & consistency
- `rules/web-accessibility-standards.md`: WCAG 2.2 AA validation
- `rules/backend-development-standards.md`: Service/controller patterns
- `rules/frontend-development-standards.md`: Frontend patterns
- `rules/ui-ux-design-standards.md`: Layout/interaction standards
- `rules/database-standards.md`: Schema/migration standards
- `rules/stored-procedure-standards.md`: Stored procedure patterns
- `rules/dotnet-architecture-standards.md`: .NET architecture patterns

**Selection**: Apply only standards matching task domain. Most specific overrides general.

**>>> MANDATORY: Execute Quality Evaluation and Detection Rules. IF any gate fails, execute Self-Healing. <<<**

**Execution Steps:**
1. Score each dimension in the Quality Assessment table below
2. Apply Detection Rules to validate scores and calculate penalties
3. IF any MUST gate failed OR any score is below its threshold: Execute Self-Healing Protocol (Retry Flow)
4. Print the completed evaluation table with final scores
5. Print the Overall Score and Evaluation Summary

## Quality Evaluation

Once the output is generated, Score the summary presentation to evaluate its quality against the following metrics, providing a percentage score (1-100%) for each.

### Implementation Analysis Quality Assessment

| # | Evaluation Dimension | Score | Gate | Assessment Criteria |
|---|---------------------|-------|------|---------------------|
| 1 | Template Adherence | [0-100%] | MUST=100% | Analysis follows task-analysis-template.md format; no extra sections |
| 2 | Requirements Fulfillment | [0-100%] | >=80% | All acceptance criteria mapped; no orphan requirements |
| 3 | Code Quality | [0-100%] | >=80% | Clean, maintainable code following project patterns |
| 4 | Testing Coverage | [0-100%] | >=80% | Unit tests present with edge case and error path coverage |
| 5 | Security Compliance | [0-100%] | >=80% | OWASP alignment; input validation; no vulnerabilities |
| 6 | Integration Success | [PASS/FAIL] | MUST PASS | No breaking changes; backwards compatibility maintained |
| 7 | Pattern Consistency | [0-100%] | >=80% | Adherence to architectural and coding patterns |
| 8 | Gap Identification | [0-100%] | >=80% | All gaps documented with actionable recommendations |

### Detection Rules

| # | Detection Logic | Penalty |
|---|-----------------|---------|
| 1 | Compare sections vs task-analysis-template.md | Missing: -20%/section; Extra: -15%/section |
| 2 | Cross-reference task acceptance criteria vs implementation | Unimplemented criterion: -15%/criterion; Orphan code: -10% |
| 3 | Static analysis: complexity metrics, code smells, duplication | High complexity: -10%/file; Code smell: -5%/instance |
| 4 | Count test files; verify edge case + error path coverage | Missing tests: -15%/component; No edge cases: -10% |
| 5 | OWASP checklist; input validation scan; vulnerability detection | Missing check: -10%/category; Vulnerability found: -20%/instance |
| 6 | Check API contracts; verify backwards compatibility | Breaking change: BLOCKED; Deprecation without notice: -15% |
| 7 | Pattern matching vs architectural standards | Pattern violation: -10%/instance; Anti-pattern: -15%/instance |
| 8 | Count gaps; verify each has actionable recommendation | Gap without recommendation: -15%/gap; Vague recommendation: -10% |

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
| Requirements Fulfillment | Add missing criteria mapping |
| Code Quality | Flag issues with specific recommendations |
| Testing Coverage | List missing test files/cases |
| Security Compliance | Add OWASP check results |
| Pattern Consistency | Document pattern violations |
| Gap Identification | Add recommendations for gaps |

**Non-Patchable (Skip):** Integration Success (breaking changes), source missing, hallucinated content, patch >30% of section, >=3 failing dimensions

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