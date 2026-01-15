---
description: Performs comprehensive bug triage, root cause analysis, and generates fix implementation tasks with validation criteria
auto_execution_mode: 1
---

# Bug Fix Task Generator with Triage
As a Senior Software Engineer specializing in debugging and system reliability, perform comprehensive bug triage and generate actionable fix tasks based on the provided bug report.

## Input Parameter: $ARGUMENTS (Mandatory)
**Accepts:** Bug report file | Bug URL | Issue description | Error log

**Optional Parameters:**
- `--skip-history`: Skip git history analysis and findings registry lookup for faster execution (default: false)

## Output
- Artifact generation: `/.propel/context/tasks/bug_<ID>/task_*.md`
- Print the following: 
    - List of rules used by the workflow in bulleted format
    - Evaluation Scores in tabular format with average score. 
    - Evaluation summary (less than 100 words).
    **Do not save as file.**

**Note:**
- If the output file is already available make the necessary changes to applicable sections. Do not overwrite the whole file.
- Always generate a single unified bug task document.
- Generate the output using the .propel/templates/issue-triage-template.md template.

### Output Specifications

**Bug with ID Found**:
- **Directory**: `/.propel/context/tasks/bug_<ID>/` 
- **File Pattern**: `task_<seqnum>_fix_<descriptive_name>.md`
- **Example**: `/.propel/context/tasks/bug_042/task_001_fix_login_error.md`

**No Bug ID (Fallback)**:
- **Directory**: `/.propel/context/tasks/bug_<short_descriptive_name>`
- **File Pattern**: `task_<seqnum>_fix_<descriptive_name>.md`
- **Example**: `/.propel/context/tasks/bug_validation_error/task_001_fix_validation_error.md`

**Sequence Number Logic**:
- Auto-increment within target directory (bug folder or root)
- Use zero-padded 3-digit format (001, 002, 003...)
- Continue from highest existing number in directory
- Descriptive name should reflect the bug being fixed

**Compatibility**: Generated tasks must be executable via `/execute-task` command

### Directory Management

**Bug ID Extraction Priority**:
1. **Parse Input**: Extract bug ID using patterns above
2. **Validate Format**: Ensure valid bug ID format
3. **Create Directory**: `/.propel/context/tasks/bug_<ID>/` if ID found
4. **Fallback**: Use `/.propel/context/tasks/bug_<short_descriptive_name>` if no ID extracted

**Supported Bug ID Formats**:
- `bug_XXX` or `BUG-XXX` (3-4 digits)
- `issue_XXX` or `ISSUE-XXX` (3-5 digits)
- `#XXX` (GitHub/GitLab style)
- JIRA format: `PROJECT-XXX`
- Custom patterns from bug tracking systems

**Directory Examples**:
```
.propel/context/tasks/
+-- bug_042/
|   +-- bug_report.md
|   +-- task_001_fix_login_error.md
|   +-- task_002_add_regression_tests.md
+-- bug_general_error/
    +-- task_001_fix_general_error.md
```

## Core Principles
- **Root Cause Focus**: Address underlying cause, not just symptoms
- **Multi-Hypothesis Analysis**: Identify 3+ potential causes before committing
- **Regression Prevention**: Document risks and create tests to prevent recurrence

## Execution Flow

### Delta Processing Protocol
**Before generating any output:**
1. Check if target output file (`task_<seqnum>_fix_<name>.md`) already exists
2. If EXISTS: Read current content, identify only sections requiring update, EDIT those sections only
3. If NOT EXISTS: Generate complete new file
4. Log: "Mode: [Delta update to X sections | New file creation]"

### 1. Bug Triage Protocol

Execute comprehensive triage before creating any bug fixing tasks:

#### 1. Issue Reproduction & Verification
- Parse bug report for reproduction steps and environment details
- Search codebase for error messages using Grep tool
- Examine relevant files with Read tool
- Execute reproduction steps with Bash tool (if runnable commands provided)
- Capture stack traces and error logs
- Verify issue exists in latest codebase version

#### 2. Root Cause Analysis (use Sequential-Thinking MCP)
- Use `mcp__sequential-thinking__sequentialthinking` to trace error through codebase
- For root cause, answer: (1) What is the immediate trigger? (2) What is the underlying cause? (3) Why wasn't this caught earlier?
- Analyze git history for root cause (skip with --skip-history):
  - `git log --oneline -10 -- <affected_file>` - Last 10 changes only
  - `git log --grep="fix" --oneline -5 -- <affected_file>` - Last 5 bug fixes only
  - If >2 "fix" commits found, flag as **Recurring Issue Hotspot**
  - **Git Fallback** (if git unavailable or commands fail): Skip git history, proceed with registry lookup."
- **Registry Lookup** (skip with --skip-history): Grep `.propel/learnings/findings-registry.md` for affected file (selective read)
- Review component dependencies and interactions
- Identify regression source if applicable
- Document root cause with specific file:line references

#### 3. Impact Assessment
- Determine affected features and user workflows
- Evaluate data integrity implications
- Assess security and performance impacts
- Calculate affected user percentage
- Check for similar issues in related components

#### 4. Priority Categorization
- **Critical**: System down, data loss, security breach
- **High**: Core feature broken, major UX degradation
- **Medium**: Minor features affected, workarounds exist
- **Low**: Cosmetic issues, rare edge cases

#### 5. Solution Validation
- Research similar fixes in git history using Bash
- Consider multiple solution approaches before settling on one
- List potential side effects: (a) What other code paths are affected? (b) What regressions are possible?
- Identify edge cases the fix must handle: boundary values, null inputs, concurrent access
- Use Context7 MCP (`mcp__context7__resolve-library-id` and `mcp__context7__get-library-docs`) for framework-specific or technology-specific solutions
- Avoid referencing libraries or patterns incompatible with the project's tech stack
- Validate approach with code examples from trusted sources
- Evaluate multiple solution approaches with deep consideration
- Define rollback procedure: (1) How to detect fix failure? (2) Steps to revert? (3) Data recovery needed?
- Estimate complexity and regression risk
- Specify validation criteria for successful fix

### Fix Task Generation

#### Task Structure Requirements
- Base task structure on `.propel/templates/issue-triage-template.md`
- Include complete triage summary in task description
- Define clear, sequential fix implementation steps
- Specify regression test requirements
- Include rollback procedures and validation checkpoints

#### Context Documentation Requirements

*** 1. Essential Bug Context Package ***
- **Triage Results**: Complete reproduction steps and root cause analysis
- **Technical Investigation**: Error traces, affected components, dependency analysis
- **Fix Strategy**: Solution approach with implementation examples from research
- **Validation Plan**: Testing requirements, regression prevention measures

*** 2. Framework Research Integration ***
- **Version Pinning**: Use `mcp__context7__resolve-library-id` to identify exact library versions
- **Implementation Patterns**: Use `mcp__context7__get-library-docs` for bug fix patterns and best practices
- **Error Handling**: Include framework-specific error handling approaches
- **Testing Strategies**: Framework-appropriate testing and validation methods

#### Quality Assurance Framework

*** 1. Pre-Delivery Checklist ***
- [ ] **Bug ID Extraction**: Attempted to extract bug ID from input
- [ ] **Folder Structure**: Created appropriate bug_XXX folder or used fallback to /.propel/context/tasks/
- [ ] **Root Cause Analysis Complete**: Answered (1) immediate trigger, (2) underlying cause, (3) why not caught earlier
- [ ] **Edge Case Analysis**: Listed all edge cases and related issues
- [ ] **Issue Reproduction**: Bug successfully reproduced and documented
- [ ] **Root Cause Verified**: Technical cause identified with code references
- [ ] **Impact Assessment**: Complete evaluation of affected systems
- [ ] **Priority Assignment**: Correct categorization based on business impact
- [ ] **Solution Validation**: Approach verified through research and analysis
- [ ] **Regression Tests**: Testing strategy defined for preventing recurrence
- [ ] **Task Executability**: Generated task is compatible with execute-task command
- [ ] **Context Completeness**: All necessary implementation context included
- [ ] **Framework Research**: Version-specific documentation and patterns included
- [ ] **Rollback Plan**: Clear reversion procedure documented

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

Once the output is generated, Score the generated bug fix task plan to evaluate its quality against the following metrics, providing a percentage score (1-100%) for each.

### Bug Fix Task Quality Assessment

| # | Evaluation Dimension | Score | Gate | Assessment Criteria |
|---|---------------------|-------|------|---------------------|
| 1 | Template Adherence | [0-100%] | MUST=100% | Output follows issue-triage-template.md structure; no extra sections |
| 2 | Root Cause Accuracy | [0-100%] | >=80% | Root cause identified with code references; depth of analysis |
| 3 | Reproduction Clarity | [0-100%] | >=80% | Bug can be reproduced with documented steps |
| 4 | Solution Correctness | [0-100%] | >=80% | Technical accuracy of proposed fix |
| 5 | Impact Assessment | [0-100%] | >=80% | Affected features, users, and risks documented |
| 6 | Priority Accuracy | [0-100%] | >=80% | Correct severity categorization based on business impact |
| 7 | Test & Rollback Coverage | [0-100%] | >=80% | Regression tests defined; rollback procedure documented |
| 8 | Task Decomposition | [0-100%] | >=80% | No task >8 todos; no task >8 hours effort; layer-based decomposition (FE/BE/DB/infra) |
| 9 | Layer Purity | [0-100%] | >=80% | Each layer task contains only its layer activities (FE: UI/CSS/components; BE: API/services; DB: schema/queries; Infra: config/deploy); no cross-layer contamination |

### Detection Rules

| # | Detection Logic | Penalty |
|---|-----------------|---------|
| 1 | Compare sections vs issue-triage-template.md | Missing: -20%; Extra: -15% |
| 2 | Root cause has file:line references | No code ref: -20%; Vague cause: -15% |
| 3 | Reproduction steps numbered; environment documented | Missing steps: -15%; No env: -10% |
| 4 | Fix addresses root cause (not symptom) | Symptom-only fix: -25% |
| 5 | Affected features, users, risks documented | Missing impact: -15%/item |
| 6 | Priority matches business impact criteria | Mismatched priority: -15% |
| 7 | Regression tests and rollback procedure defined | No tests: -15%; No rollback: -15% |
| 8 | Count todos per task; verify <=8 todos AND <=8 hours effort | >8 todos: -10%/extra todo; >8 hours: -15%/task |
| 9 | Scan task todos for cross-layer activities (FE with API/DB/deploy code; BE with UI/CSS/schema code; DB with UI/API/deploy code; Infra with UI/API/schema code) | Cross-layer activity: -15%/instance |

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
| Root Cause Accuracy | Add file:line references |
| Reproduction Clarity | Add numbered steps and env details |
| Solution Correctness | Refine technical fix details |
| Impact Assessment | Add affected features/users/risks |
| Priority Accuracy | Adjust severity categorization |
| Test & Rollback Coverage | Add test definitions and rollback procedure |
| Task Decomposition | Split tasks with >8 todos or >8 hours effort |
| Layer Purity | Move cross-layer activities to appropriate layer task |

**Non-Patchable (Skip):** Source missing, clarification needed, hallucinated content, patch >30% of section, >=3 failing dimensions

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