---
description: Entry point for comprehensive codebase analysis. Validates input and delegates complex analysis to the specialized code-analyzer agent for architectural insights and strategic recommendations.
auto_execution_mode: 1
---

# Codebase Analysis Command

## Purpose
Entry point for comprehensive codebase analysis.

## Input Parameters: $ARGUMENTS (Optional)
**Accepts:** Repository URL | Folder Path | Root path | Technology stack | Business domain | Analysis depth

**Default Parameters:**
- `repo_url`: Current directory or provided Git URL
- 'Folder_path': Source code directory
- `root_path`: "." (current directory)
- `primary_stack`: Auto-detected from codebase
- `business_domain`: "General business application"
- `analysis_depth`: "comprehensive" (or "standard", "quick")
- `time_budget_minutes`: 60
- `priority_areas`: "architecture, security, performance, data model, integrations"

## Output
- Artifact generation: `.propel/context/docs/codeanalysis.md`
- Print the following: 
  - List of rules used by the workflow in bulleted format
  - Evaluation Scores in tabular format with average score. 
  - Evaluation summary (less than 100 words).
  **Do not save as file.**

**Note:**
- If the output file is already available make the necessary changes to applicable sections. Do not overwrite the whole file.
- Always create the output file in manageable smaller chunks to manage memory and processing constraints.
- Always generate a single unified document.
- Generate the output using the .propel/templates/codebase-analysis-template.md template.

## Core Principles
- **Comprehensive Discovery**: Document all architectural patterns, anti-patterns, and system topology
- **Evidence-Based Analysis**: Every finding must reference specific files, patterns, or metrics
- **Actionable Recommendations**: Provide prioritized improvements with clear remediation steps

## Execution Flow

### 1. Parameter Validation
- Validate repository access and permissions
- Set default values for missing parameters
- Verify basic repository accessibility

### 2. Core Responsibilities

#### Deep Analysis (use sequential-thinking MCP)
- Perform stepwise reasoning through complex architectural decisions and patterns
- Trace system flows and business logic transformations methodically
- Build layered understanding of technology choices and their implications
- Develop causal chains linking architectural patterns to system behaviors and risks

**Primary Approach:**
```
mcp__sequential-thinking__sequentialthinking for step-by-step analysis:
- Architectural Patterns: Layered, Microservices, Event-Driven, Hexagonal, CQRS, MVC, MVP, MVVM
- Design Patterns: Singleton, Factory, Observer, Strategy, Repository, Unit of Work
- Anti-Patterns Detection: God objects, circular dependencies, tight coupling
- System Topology: Entry points, communication protocols, external integrations
- Use Case Discovery: Actors, goals, success scenarios from routes and business logic
```

**Fallback Strategy (if Sequential-thinking MCP fails):**
```
Manual structured analysis:

**Phase A: Architecture Pattern Discovery**
- Step 1: Identify directory structure and module organization
- Step 2: Map component relationships and dependencies  
- Step 3: Classify architectural style (monolith/microservices/etc.)

**Phase B: Design Pattern Recognition** 
- Step 1: Scan for common creational patterns (Factory, Builder, Singleton)
- Step 2: Identify behavioral patterns (Observer, Strategy, Command)
- Step 3: Find structural patterns (Adapter, Decorator, Repository)

**Phase C: Quality & Risk Assessment**
- Step 1: Calculate complexity metrics and identify God objects
- Step 2: Map circular dependencies and coupling issues
- Step 3: Document system entry points and data flow
```

**Before writing analysis findings, list all findings:**
| Category | Finding | Severity |
|----------|---------|----------|
| Architecture | ... | ... |
| Security | ... | ... |
| Performance | ... | ... |
**Now expand each finding listed above.**

**Business Logic Analysis**
- **Core Business Logic Discovery**: Identify classes/modules solving business problems
- **Method Analysis**: Document key methods and their business purpose
- **Business Rules Extraction**: Document enforced business rules
- **Process Flow Mapping**: Step-by-step business flow in plain English
- **Dependencies Mapping**: Track critical dependencies for business logic
- **User Persona Journey**: Identify the user persona journey for validation
- **Use Case Diagrams**: Generate PlantUML diagram for each discovered use case

**Quality Assessment & Risk Analysis**
- **Code Quality Metrics**: Cyclomatic complexity, code duplication, technical debt
- **OWASP Top 10 Compliance Assessment**:
  - A01:2021 - Broken Access Control
  - A02:2021 - Cryptographic Failures
  - A03:2021 - Injection
  - A04:2021 - Insecure Design
  - A05:2021 - Security Misconfiguration
  - A06:2021 - Vulnerable and Outdated Components
  - A07:2021 - Identification and Authentication Failures
  - A08:2021 - Software and Data Integrity Failures
  - A09:2021 - Security Logging and Monitoring Failures
  - A10:2021 - Server-Side Request Forgery (SSRF)


#### Technical Research (use Context7 MCP)
- Gather technology stack documentation and framework best practices simultaneously using Context7
- Map architectural patterns, anti-patterns, and structural metrics concurrently
- Understand PlantUML syntax for use case diagram generation 

**Primary Approach:**
```
For each detected technology/framework:
1. mcp__context7__resolve-library-id(libraryName: "technology-name")
2. mcp__context7__get-library-docs(context7CompatibleLibraryID: "resolved-id")
```

**Fallback Strategy (if Context7 MCP fails):**
```
WebSearch: "[technology] architecture best practices documentation"
WebSearch: "[framework] security vulnerabilities CVE"
Read: package.json (detect technology stack and versions)
Read: README.md (understand project structure and technologies)
Grep: "import.*|require.*|using.*" (identify framework usage patterns)
```

**Parallel Research Tasks:**
- **Package Managers**: package.json, pom.xml, build.gradle, go.mod, requirements.txt, Gemfile, *.csproj
- **Frameworks**: React, Angular, Vue, .NET, Spring, Django, Rails, Express
- **Build Tools**: Webpack, Vite, Maven, Gradle, MSBuild, Make
- **Container/Orchestration**: Dockerfile, docker-compose.yml, k8s/*, helm/*
- **IaC**: terraform/*, bicep/*, arm/*, cloudformation/*
- **CI/CD**: .github/workflows, azure-pipelines.yml, .gitlab-ci.yml, Jenkinsfile
- **Monorepo Detection**: nx.json, lerna.json, rush.json, turbo.json, pnpm-workspace.yaml

**Security & Performance Scanning**
```bash
# Parallel vulnerability scanning
npm audit --audit-level=moderate || true
dotnet list package --vulnerable --include-transitive || true
pip-audit || safety check || true

# Secret scanning
trufflehog filesystem . --json || true
gitleaks detect --source . || true
rg -n "(?i)(api[_-]?key|secret|password|token|private[_-]?key)" --glob "!*.lock" || true

# Performance metrics
npm run build -- --stats || webpack-bundle-analyzer || true
npx vite-bundle-visualizer || true
```

**Documentation & Test Coverage Extraction**
```bash
# Coverage analysis
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover || true
npm test -- --coverage --coverageReporters=json-summary text lcov || true
pytest --cov=. --cov-report=json --cov-report=term || true
```

### 3. Analysis Generation
- Read template from `.propel/templates/codebase-analysis-template.md`
- Populate template with the findings
- Use Write tool to create an artifact `.propel/context/docs/codeanalysis.md`
- Ensure all template sections are populated with real data

### 4. Summary Presentation
- Present executive summary to user
- Highlight critical findings and recommendations
- Provide link to detailed report in `.propel/context/docs/codeanalysis.md`
- Present the Quality Assessment metrics

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
- `rules/uml-text-code-standards.md`: PlantUML/Mermaid notation standards

**>>> MANDATORY: Execute Quality Evaluation and Detection Rules. IF any gate fails, execute Self-Healing. <<<**

**Execution Steps:**
1. Score each dimension in the Quality Assessment table below
2. Apply Detection Rules to validate scores and calculate penalties
3. IF any MUST gate failed OR any score is below its threshold: Execute Self-Healing Protocol (Retry Flow)
4. Print the completed evaluation table with final scores
5. Print the Overall Score and Evaluation Summary

## Quality Evaluation

Once the output is generated, Score the analysis generated to evaluate its quality against the following metrics, providing a percentage score (1-100%) for each.

### Codebase Analysis Quality Assessment

| # | Evaluation Dimension | Score | Gate | Assessment Criteria |
|---|---------------------|-------|------|---------------------|
| 1 | Template Adherence | [0-100%] | MUST=100% | All sections from codebase-analysis-template.md present; no extra sections |
| 2 | Architecture Coverage | [0-100%] | >=80% | Architectural patterns, anti-patterns, and system topology documented |
| 3 | Technical Accuracy | [0-100%] | >=80% | Findings verifiable against codebase; no hallucinated components |
| 4 | Security Assessment | [0-100%] | >=80% | OWASP vulnerabilities checked; risk levels assigned |
| 5 | Integration Mapping | [0-100%] | >=80% | All external dependencies and APIs documented |
| 6 | Business Logic Discovery | [0-100%] | >=80% | Core business rules identified; use cases documented |
| 7 | Code Quality Metrics | [0-100%] | >=80% | Complexity, duplication, technical debt measured |
| 8 | Actionable Recommendations | [0-100%] | >=80% | Each finding has specific remediation guidance |

### Detection Rules

| # | Detection Logic | Penalty |
|---|-----------------|---------|
| 1 | Compare sections vs codebase-analysis-template.md | Missing: -20%; Extra: -15% |
| 2 | Patterns, anti-patterns, topology documented | Missing section: -15%/section |
| 3 | Findings verifiable in codebase | Hallucinated component: -25%/instance |
| 4 | OWASP vulnerabilities checked; risk levels assigned | Missing check: -10%; No risk level: -5% |
| 5 | External dependencies and APIs documented | Undocumented dep: -10%/dependency |
| 6 | Business rules identified with code references | No code ref: -10%/rule |
| 7 | Complexity, duplication, tech debt measured | Missing metric: -10%/metric |
| 8 | Each finding has remediation guidance | No guidance: -15%/finding |

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
| Architecture Coverage | Add missing layer analysis |
| Security Assessment | Add OWASP check results |
| Integration Mapping | Add dependency documentation |
| Business Logic Discovery | Add business rule identification |
| Code Quality Metrics | Add metric measurements |
| Actionable Recommendations | Add remediation guidance |

**Non-Patchable (Skip):** Technical Accuracy (hallucinations), source missing, patch >30% of section, >=3 failing dimensions

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