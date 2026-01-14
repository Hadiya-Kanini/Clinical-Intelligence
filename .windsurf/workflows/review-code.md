---
description: Comprehensive code review workflow analyzing local changes across frontend, backend, and database code with technology-specific best practices, automated static analysis, architectural assessment, and structured feedback generation.
---

# Code Review Workflow

As a Senior Software Engineer with expertise in code quality, security, performance optimization, and architectural design, conduct comprehensive code reviews of local changes with technology-specific analysis, automated validation, and actionable feedback. This workflow combines git-based change detection, technology stack identification, Context7-powered best practice retrieval, static analysis integration, architectural pattern assessment via sequential thinking, and structured report generation to ensure high-quality, secure, and maintainable code across frontend, backend, and database layers.

## Input Parameters

**Input Scope:**
- **If file path provided**: Review only the specified file(s)
- **If no file specified**: Review all local changes (git diff against base branch)

**Optional Parameters:**
- `--file=PATH`: Specific file path to review (can be used multiple times for multiple files)
- `--base=BRANCH`: Base branch for comparison (default: auto-detect main/master/develop)
- `--files=PATTERN`: File pattern to review (e.g., `src/**/*.ts`, `*.cs`) (default: all changed files)
- `--severity=LEVEL`: Minimum severity level to report - "info", "warning", "error", "critical" (default: "info")
- `--auto-fix`: Attempt automatic fixes for linting/formatting issues (default: false)
- `--include-suggestions`: Include code improvement suggestions beyond issues (default: true)
- `--skip-static-analysis`: Skip automated linting/static analysis tools (default: false)
- `--focus=AREA`: Focus review on specific area - "security", "performance", "architecture", "testing", "licensing", "all" (default: "all")
- `--depth=LEVEL`: Analysis depth - "quick", "standard", "deep" (default: "standard")
- `--skip-history`: Skip git history analysis and findings registry lookup for faster execution (default: false)

### Parameter Validation
- If file parameter provided, verify file exists and is readable
- Validate base branch exists in repository
- Verify file patterns match existing files
- Check severity level is valid enum value
- Ensure focus area is recognized category
- Validate depth level selection

## Output

**Console Output:**
- Review progress updates during analysis
- Summary of findings by severity level
- Path to generated review report file

**File Output:**
- **Template**: `.propel/templates/code-review-template.md`
- **Output File**: `.propel/reviews/review_<timestamp>.md`
- **Format**: Markdown using template structure
- **Content**: Complete review report with all findings, analysis, and recommendations

**Console Summary Display:**
```
===========================================================
                CODE REVIEW COMPLETED
===========================================================

Repository: owner/repo
Reviewing: feature/branch-name vs main

Files Reviewed: 12
Findings:
  CRITICAL: 2
  HIGH: 5
  MEDIUM: 8
  LOW: 12
  INFO: 6

Recommendation: [Approve with Changes]

Full Report: .propel/reviews/review_20251211_143022.md

===========================================================
```

**Console Evaluation Output:**
- List of rules applied to the workflow in bulleted format
- Evaluation Scores in tabular format with average score
- Evaluation summary (less than 100 words)
**Not saved to file** (console output only)

## Core Principles

### Comprehensive Analysis
- Review all changed files across technology stacks
- Apply language-specific and framework-specific standards
- Leverage automated tools for objective validation
- Use human judgment for architectural and design concerns

### Technology-Aware
- Detect technology stack from file extensions and imports
- Fetch relevant best practices using Context7 for detected technologies
- Apply language-specific linters and static analyzers
- Adapt review criteria to frontend, backend, or database context

### Evidence-Based
- Reference specific line numbers and code snippets
- Link to official documentation and standards (OWASP, SOLID, etc.)
- Include metrics from static analysis tools
- Provide concrete examples of fixes

### Actionable Feedback
- Prioritize findings by severity and impact
- Provide specific fix recommendations with code examples
- Link to relevant documentation and resources
- Balance criticism with recognition of good practices

### Security-First
- Always check for OWASP Top 10 vulnerabilities
- Validate input sanitization and output encoding
- Review authentication and authorization logic
- Check for sensitive data exposure

### Performance-Conscious
- Identify algorithmic complexity issues
- Detect database query inefficiencies
- Review caching strategies
- Analyze bundle sizes and render performance (frontend)

## Execution Flow

### Phase 1: Change Detection and Context Gathering

**Review Scope Determination:**

**If file path(s) provided (`--file` parameter):**
1. Verify each file exists and is readable
2. Read file contents directly
3. Detect file language and technology from extension and content
4. Set review scope to specified files only
5. Skip git change detection (review entire file content)

**If no file specified (default: local changes):**
1. Verify git repository: `git rev-parse --git-dir`
2. Determine base branch (parameter or auto-detect): `git symbolic-ref refs/remotes/origin/HEAD`
3. Identify changed files: `git diff --name-status <base>..HEAD`
4. Get file-level statistics: `git diff --stat <base>..HEAD`
5. Calculate line changes: `git diff --shortstat <base>..HEAD`
6. Filter by file pattern if `--files` parameter provided
7. Set review scope to changed lines/sections only

**File Categorization (Both Modes):**
- Frontend files: `**/*.{tsx,ts,jsx,js,vue,svelte,html,css,scss}`
- Backend files: `**/*.{cs,java,py,go,rb,kt,php,rs}`
- Database files: `**/*.{sql,prisma,migration}`, EF migrations, schema files
- Configuration: `**/*.{json,yaml,yml,xml,config,env}`
- Tests: `**/*.{test,spec}.{ts,js,cs,py,java}`

### Phase 1b: Historical Review Analysis (Optional: skip with --skip-history)

**Registry Index Lookup**:
1. Read ONLY the Index section from `.propel/learnings/findings-registry.md`
2. If changed files appear in index, grep only those specific entries
3. Limit to 3 most recent entries per file

**Git History**:
- `git log --grep="fix" --oneline -5 -- <changed_files>` - Last 5 fixes only
- If >2 found, add "High-Risk File" to review header

**Git Fallback** (if git unavailable or commands fail):
- Skip git history analysis, continue with registry lookup only"

**Output**: Compact summary in review report, NOT full history dump

### Phase 2: Technology Stack Detection

**Identify Technologies:**
1. Scan package.json, *.csproj, requirements.txt, go.mod, pom.xml
2. Analyze import/using statements in source files
3. Detect frameworks from file patterns and conventions
4. Identify database type from connection strings and migrations

**Technology Mapping:**
- **Frontend**: React, Angular, Vue, Svelte, Next.js, TypeScript, JavaScript
- **Backend**: .NET/C#, Java/Spring Boot, Node.js, Python, Go, Ruby, PHP
- **Database**: SQL Server, PostgreSQL, MySQL, MongoDB, Entity Framework, Prisma, JPA/Hibernate
- **Testing**: Jest, xUnit, NUnit, pytest, JUnit, Jasmine, Karma

### Phase 3: Best Practices Retrieval

**Context7 Integration:**
For each detected technology, fetch relevant best practices:

1. **Frontend Technologies:**
   - React: Use `mcp_context7_get-library-docs` for React best practices
   - Angular: Fetch Angular coding standards, component patterns, RxJS best practices
   - TypeScript: Fetch TypeScript coding standards
   - State Management: Redux, Zustand, Jotai, NgRx patterns
   - CSS/Styling: Tailwind, CSS Modules, Styled Components, Angular Material

2. **Backend Technologies:**
   - .NET: ASP.NET Core, Entity Framework Core patterns
   - Java: Spring Boot, Spring Framework, JPA/Hibernate patterns
   - Node.js: Express, NestJS, Fastify conventions
   - Python: FastAPI, Django, Flask best practices
   - API Design: REST, GraphQL, gRPC standards

3. **Database Technologies:**
   - ORM Patterns: EF Core, Prisma, TypeORM
   - Query Optimization: Indexing, N+1 prevention
   - Migration Strategies: Versioning, rollback safety

4. **Testing Frameworks:**
   - Unit Testing: Jest, xUnit, pytest patterns
   - Integration Testing: Best practices per stack
   - Test Coverage: Minimum thresholds and critical paths

**Fallback Strategy:**
- If Context7 unavailable, use built-in instruction files
- Reference local best practice documentation
- Apply language-agnostic standards from templates

### Phase 4: Static Analysis Execution

**Conditional Tool Execution (unless `--skip-static-analysis`):**

**Frontend Analysis:**
- **TypeScript/JavaScript:**
  - Run ESLint: `npx eslint --format json <files>`
  - Run TypeScript compiler: `npx tsc --noEmit --pretty false`
  - Check Prettier formatting: `npx prettier --check <files>`
  
- **React Specific:**
  - React Hooks linting (eslint-plugin-react-hooks)
  - Accessibility checks (eslint-plugin-jsx-a11y)
  - Bundle size analysis (webpack-bundle-analyzer if available)

- **Angular Specific:**
  - Angular ESLint: `npx ng lint --format json`
  - Template type checking: `npx ng build --configuration production --aot`
  - Angular-specific rules (eslint-plugin-angular)
  - RxJS linting (eslint-plugin-rxjs)

**Backend Analysis:**
- **.NET/C#:**
  - Run Roslyn analyzers: `dotnet build /p:RunAnalyzers=true /p:TreatWarningsAsErrors=false`
  - StyleCop analysis for code style
  - Security analyzers (SecurityCodeScan)
  - Code metrics: `dotnet build /p:CodeAnalysisTreatWarningsAsErrors=false`

- **Java:**
  - Maven/Gradle compile with warnings: `mvn compile` or `gradle build`
  - Checkstyle: `mvn checkstyle:check` or `gradle checkstyleMain`
  - PMD: `mvn pmd:check` or `gradle pmdMain`
  - SpotBugs: `mvn spotbugs:check` or `gradle spotbugsMain`
  - SonarQube if available: `mvn sonar:sonar`

- **Python:**
  - Pylint: `pylint --output-format=json <files>`
  - Mypy type checking: `mypy --strict <files>`
  - Bandit security: `bandit -r -f json <files>`

- **Node.js:**
  - ESLint with Node.js rules
  - npm audit for dependency vulnerabilities

**Database Analysis:**
- **SQL Scripts:**
  - SQL linting (sqlfluff, TSQLLint)
  - Validate syntax and common anti-patterns
  - Check for missing indexes on foreign keys

- **Migrations:**
  - Validate migration safety (no data loss)
  - Check for proper rollback scripts
  - Verify idempotency

**Security Scanning:**
- Dependency vulnerability scanning: `npm audit`, `dotnet list package --vulnerable`
- Secret detection in code: scan for API keys, passwords, tokens
- OWASP ZAP if available for API endpoints

**Parse and Aggregate Results:**
- Collect all tool outputs
- Normalize severity levels across tools
- Deduplicate similar findings
- Map to file locations and line numbers

### Phase 5: Manual Code Review

**Architecture Analysis (Use Sequential Thinking for Complex Changes):**

When changes involve multiple files or architectural decisions:
1. Activate sequential thinking MCP: `mcp_sequential-th_sequentialthinking`
2. Analyze architectural patterns and design decisions
3. Evaluate component/service boundaries and responsibilities
4. Assess data flow and state management approaches
5. Review error handling and resilience patterns
6. Identify potential refactoring opportunities

**Review Checklist Application:**

1. **Implementation Review:**
   - Does code accomplish intended purpose?
   - Can solution be simplified?
   - Are abstractions appropriate?
   - Is code modular and reusable?
   - Are design patterns used correctly?
   - SOLID principles adherence?

2. **Logic and Bugs:**
   - Edge cases handled?
   - Input validation comprehensive?
   - Boundary conditions checked?
   - Error paths tested?

3. **Error Handling:**
   - Exceptions caught appropriately?
   - Error messages user-friendly and informative?
   - Logging levels appropriate?
   - Debugging information sufficient?

4. **Dependencies:**
   - Necessary dependencies only?
   - Version constraints appropriate?
   - Circular dependencies avoided?
   - Impact on other modules assessed?

5. **Security and Privacy (OWASP Top 10):**
   - A01: Broken Access Control - proper authorization checks?
   - A02: Cryptographic Failures - sensitive data encrypted?
   - A03: Injection - inputs sanitized, parameterized queries?
   - A04: Insecure Design - security by design?
   - A05: Security Misconfiguration - secure defaults?
   - A06: Vulnerable Components - dependencies patched?
   - A07: Authentication Failures - secure auth implementation?
   - A08: Data Integrity Failures - serialization safe?
   - A09: Logging Failures - security events logged?
   - A10: SSRF - external requests validated?

6. **Performance:**
   - Algorithmic complexity acceptable (O(n) analysis)?
   - Database queries optimized (N+1 avoided)?
   - Caching used appropriately?
   - Memory leaks prevented?
   - Frontend: Unnecessary re-renders avoided?
   - Backend: Thread safety ensured?

7. **Testing and Testability:**
   - Unit tests added/updated?
   - Test coverage adequate for critical paths?
   - Integration tests for cross-component interactions?
   - Mocking/stubbing done correctly?
   - Tests are deterministic and isolated?

8. **Readability and Maintainability:**
   - Code self-documenting with clear naming?
   - Comments explain "why" not "what"?
   - Complex logic broken down?
   - Consistent code style?
   - Dead code removed?

9. **Accessibility (Frontend Only):**
   - WCAG 2.1 Level AA compliance?
   - Semantic HTML used?
   - ARIA attributes where needed?
   - Keyboard navigation supported?
   - Screen reader friendly?

10. **Ethics and Inclusivity:**
    - Privacy-respecting data usage?
    - No discriminatory algorithms or bias?
    - Inclusive language and UX?
    - Harassment prevention measures?

11. **Licensing and Legal Compliance:**
    - Repository has appropriate license file?
    - Third-party dependencies licenses compatible?
    - GPL/AGPL dependencies identified (copyleft risk)?
    - Public code properly attributed (as-is copies)?
    - License headers in source files if required?
    - No license violations or conflicts?
    - Commercial use restrictions respected?

**Technology-Specific Checks:**

**Frontend (React/TypeScript):**
- Component composition and reusability
- Hook usage and dependencies
- State management patterns
- Memoization (useMemo, useCallback, React.memo)
- Effect cleanup and memory leaks
- Race condition prevention (stale closure detection, AbortController usage)
- Prop types and TypeScript interfaces
- Accessibility attributes

**Frontend (Angular/TypeScript):**
- Component lifecycle hooks usage
- Dependency injection patterns
- RxJS observables and subscriptions cleanup
- OnPush change detection strategy
- Smart vs presentational components
- Module organization and lazy loading
- Template type safety and strict mode
- NgRx/state management patterns

**Backend (.NET/C#):**
- Dependency injection usage
- Async/await patterns and ConfigureAwait
- IDisposable and resource cleanup
- Exception handling and logging
- API versioning and contracts
- Repository/service layer separation

**Backend (Java/Spring Boot):**
- Dependency injection (@Autowired, constructor injection)
- Exception handling and @ControllerAdvice
- Transaction management (@Transactional)
- JPA entity relationships and lazy loading
- Stream API usage and performance
- Optional usage for null safety
- Resource management (try-with-resources)
- Thread safety and concurrency

**Database:**
- Index coverage for queries
- Foreign key constraints
- Transaction boundaries
- Data type selection
- Migration reversibility
- Stored procedure security (parameterization)

### Phase 6: Findings Categorization and Prioritization

**Severity Classification:**

- **CRITICAL**: Security vulnerabilities, data loss risks, production blockers
  - SQL injection, XSS, authentication bypass
  - Data corruption or loss scenarios
  - Crashes or system failures

- **HIGH**: Significant bugs, major performance issues, architectural violations
  - N+1 queries, memory leaks
  - God objects, tight coupling
  - Missing error handling in critical paths

- **MEDIUM**: Code quality issues, minor performance problems, maintainability concerns
  - Code duplication
  - Missing tests for new functionality
  - Suboptimal algorithms

- **LOW**: Style violations, minor refactoring opportunities, suggestions
  - Inconsistent naming
  - Minor code smells
  - Documentation gaps

- **INFO**: Best practice suggestions, educational notes
  - Alternative approaches
  - Framework updates available
  - Performance optimization opportunities

**Category Organization:**
- Group by severity level
- Within severity, group by category (Security, Performance, Architecture, etc.)
- Within category, order by file and line number
- Include related findings together

### Phase 7: Report Generation

**Template Loading:**
1. Load template from `.propel/templates/code-review-template.md`
2. Parse template structure and placeholders
3. Prepare data for template population

**Data Population:**
1. Generate timestamp: `YYYYMMDD_HHMMSS` format
2. Populate metadata: repository, branch, reviewer, timestamp
3. Fill executive summary with findings count and recommendation
4. Populate technology stack detected
5. Add all findings by severity level with full details
6. Include static analysis results
7. Add licensing and legal compliance section
8. Populate architecture review findings
9. Add technology-specific analysis sections
10. Include testing assessment
11. Populate security assessment (OWASP)
12. Add next steps and recommendations

**File Generation:**
1. Ensure `.propel/reviews/` directory exists: Create if missing
2. Generate filename: `review_<timestamp>.md`
3. Write populated template to `.propel/reviews/review_<timestamp>.md`
4. Verify file written successfully

**Console Output:**
1. Display review progress during analysis
2. Show summary statistics
3. Print path to generated report
4. Display recommendation (Approve/Approve with Changes/Reject)

**Formatting Guidelines:**
- Use text severity indicators: CRITICAL, HIGH, MEDIUM, LOW, INFO
- Include file paths with line numbers
- Syntax highlight code snippets
- Keep descriptions concise (1-3 sentences)
- Provide actionable fix recommendations with code examples

**Findings Codification** (CRITICAL/HIGH only):

**Before adding new findings:**

1. **Check if registry exists**:
   - If NOT exists: Create `.propel/learnings/findings-registry.md` from `.propel/templates/findings-registry-template.md`
   - If exists: Continue

2. **Archival check** (run BEFORE adding new entries):
   ```bash
   grep -c "^- id: F" .propel/learnings/findings-registry.md
   ```
   - If count >= 40: Archive oldest 10 entries (lowest IDs) to `.propel/learnings/findings-archive.md`

3. **Add new finding(s)** to `.propel/learnings/findings-registry.md`:
   ```yaml
   - id: F<next_seq>
     file: <path>
     cat: <security|performance|architecture|quality>
     issue: <max 10 words>
     cause: <root cause - max 20 words>
   ```

4. **Update Index section** with new file path → ID mappings

### Phase 8: Auto-Fix Execution (Optional)

**If `--auto-fix` parameter provided:**

1. **Safe Auto-Fixes Only:**
   - Formatting issues (Prettier, StyleCop)
   - Import organization
   - Unused variable removal
   - Simple linting fixes

2. **Create Backup:**
   - Stash current changes: `git stash push -m "Pre-auto-fix backup"`

3. **Execute Fixes:**
   - Run formatters: `npx prettier --write`, `dotnet format`
   - Apply linter auto-fixes: `npx eslint --fix`

4. **Verify Changes:**
   - Run tests: `npm test`, `dotnet test`
   - Ensure build succeeds
   - Display diff of auto-fixes

5. **Prompt for Acceptance:**
   - Show what was changed
   - Ask user to accept or reject auto-fixes
   - If rejected, restore stash

### Technical Implementation

**Git Commands by Category:**

1. **Change Detection:**
   - `git diff --name-status <base>..HEAD` - List changed files with status
   - `git diff --stat <base>..HEAD` - Get file-level statistics
   - `git diff --shortstat <base>..HEAD` - Get total line changes
   - `git diff <base>..HEAD -- <file>` - Get specific file diff

2. **Repository Information:**
   - `git rev-parse --git-dir` - Verify git repository
   - `git branch --show-current` - Get current branch
   - `git symbolic-ref refs/remotes/origin/HEAD` - Get default branch
   - `git config --get remote.origin.url` - Get repository URL

3. **File Analysis:**
   - `git show <commit>:<file>` - Get file content at commit
   - `git ls-files --others --exclude-standard` - List untracked files
   - `git check-attr linguist-language -- <file>` - Detect file language

**Static Analysis Tools:**

**Frontend:**
- ESLint: `npx eslint --format json --no-eslintrc -c .eslintrc.js <files>`
- TypeScript: `npx tsc --noEmit --pretty false 2>&1`
- Prettier: `npx prettier --check --list-different <files>`

**Backend (.NET):**
- Roslyn Analyzers: `dotnet build /p:RunAnalyzers=true /clp:NoSummary /v:quiet`
- StyleCop: Integrated with Roslyn analyzers
- Security: `dotnet list package --vulnerable --include-transitive`

**Backend (Java):**
- Checkstyle: `mvn checkstyle:checkstyle -Dcheckstyle.output.format=xml`
- PMD: `mvn pmd:pmd -Dformat=xml`
- SpotBugs: `mvn spotbugs:spotbugs -Dspotbugs.xmlOutput=true`
- Dependency Check: `mvn dependency-check:check`

**Backend (Python):**
- Pylint: `pylint --output-format=json <files>`
- Mypy: `mypy --strict --json-report <files>`
- Bandit: `bandit -r -f json <files>`

**Database:**
- sqlfluff (SQL): `sqlfluff lint --format json <files>`
- EF Core: Check for pending migrations, validate models
- JPA/Hibernate: Validate entity mappings, check for N+1 queries

**MCP Tools:**

**Context7 for Best Practices:**
- `mcp_context7_resolve-library-id` - Find library ID for detected technology
- `mcp_context7_get-library-docs` - Fetch best practices and patterns

**Sequential Thinking for Architecture:**
- `mcp_sequential-th_sequentialthinking` - Complex architectural analysis
  - Use for multi-file changes affecting architecture
  - Analyze design patterns and trade-offs
  - Evaluate SOLID principle adherence

**Code Search:**
- `semantic_search` - Find similar patterns in codebase
- `grep_search` - Find specific code patterns or anti-patterns
- `list_code_usages` - Analyze impact of changes on other code

**Problem Detection:**
- `get_errors` - Retrieve compiler/linter errors from VS Code
- `test_failure` - Get test failure information if available

## Guardrails
- `rules/ai-assistant-usage-policy.md`: Explicit commands; minimal output
- `rules/code-anti-patterns.md`: Avoid god objects, circular deps, magic constants
- `rules/dry-principle-guidelines.md`: Single source of truth; delta updates
- `rules/language-agnostic-standards.md`: KISS, YAGNI, size limits, clear naming
- `rules/markdown-styleguide.md`: Front matter, heading hierarchy, code fences
- `rules/performance-best-practices.md`: Optimize after measurement
- `rules/security-standards-owasp.md`: OWASP Top 10 alignment
- `rules/software-architecture-patterns.md`: Pattern selection, boundaries

### Extended Standards (apply based on detected technology)
- `rules/code-documentation-standards.md`: Comment WHY, not WHAT
- `rules/react-development-standards.md`: React component patterns
- `rules/angular-development-standards.md`: Angular component patterns
- `rules/typescript-styleguide.md`: TypeScript typing & consistency
- `rules/web-accessibility-standards.md`: WCAG 2.2 AA validation
- `rules/backend-development-standards.md`: Service/controller patterns
- `rules/frontend-development-standards.md`: Frontend patterns
- `rules/ui-ux-design-standards.md`: Layout/interaction standards
- `rules/database-standards.md`: Schema/migration standards
- `rules/stored-procedure-standards.md`: Stored procedure patterns
- `rules/dotnet-architecture-standards.md`: .NET architecture patterns
- `rules/aspnet-webapi-standards.md`: ASP.NET Web API patterns
- `rules/csharp-coding-standards.md`: C# coding standards
- `rules/unit-testing-standards.md`: Unit testing patterns

**Selection**: Detect technology from file extensions/imports. Most specific overrides general. Security/accessibility always on.

**>>> MANDATORY: Execute Quality Evaluation and Detection Rules. IF any gate fails, execute Self-Healing. <<<**

**Execution Steps:**
1. Score each dimension in the Quality Assessment table below
2. Apply Detection Rules to validate scores and calculate penalties
3. IF any MUST gate failed OR any score is below its threshold: Execute Self-Healing Protocol (Retry Flow)
4. Print the completed evaluation table with final scores
5. Print the Overall Score and Evaluation Summary

## Quality Evaluation

Once the review is complete, score the code review process to evaluate its quality against the following metrics, providing a percentage score (1-100%) for each.

### Code Review Quality Assessment

| # | Evaluation Dimension | Score | Gate | Assessment Criteria |
|---|---------------------|-------|------|---------------------|
| 1 | Security Coverage | [PASS/FAIL] | MUST PASS | OWASP Top 10 assessed; no critical vulnerabilities missed |
| 2 | Comprehensiveness | [0-100%] | >=80% | All changed files analyzed; full checklist coverage |
| 3 | Evidence Quality | [0-100%] | >=80% | Specific line numbers cited; code snippets included |
| 4 | Severity Accuracy | [0-100%] | >=80% | Findings correctly prioritized; critical issues flagged |
| 5 | Technology-Specific Analysis | [0-100%] | >=80% | Framework best practices applied; stack-specific patterns checked |
| 6 | Architecture Assessment | [0-100%] | >=80% | SOLID adherence; design patterns evaluated; coupling analyzed |
| 7 | Actionability | [0-100%] | >=80% | Clear fix recommendations; code examples provided |
| 8 | Testing Quality | [0-100%] | >=80% | Test coverage evaluated; missing tests identified |

### Detection Rules

| # | Detection Logic | Penalty |
|---|-----------------|---------|
| 1 | OWASP Top 10 checklist completed | Missing check: BLOCKED |
| 2 | All changed files analyzed | Missed file: -15%/file |
| 3 | Findings cite file:line; code snippets included | No line ref: -10%/finding; No snippet: -5% |
| 4 | Severity levels (Critical/High/Medium/Low) assigned | Missing severity: -10%/finding |
| 5 | Framework-specific patterns checked | Missed pattern: -10%/pattern |
| 6 | SOLID principles evaluated; coupling analyzed | Missing SOLID check: -10%/principle |
| 7 | Fix recommendations with code examples | No fix: -15%/finding; No example: -5% |
| 8 | Test coverage evaluated; missing tests identified | No coverage eval: -15% |

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
| Comprehensiveness | Add analysis for missed files |
| Evidence Quality | Add file:line references |
| Severity Accuracy | Adjust severity levels |
| Technology-Specific Analysis | Add framework patterns |
| Architecture Assessment | Add SOLID evaluation |
| Actionability | Add fix recommendations |
| Testing Quality | Add coverage evaluation |

**Non-Patchable (Skip):** Security Coverage (OWASP requires code access), source missing, hallucinated content, patch >30% of section, >=3 failing dimensions

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