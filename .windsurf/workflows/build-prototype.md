---
description: Transform business hypotheses into working validation prototypes within 40 hours. Creates minimal functional prototypes covering critical functionality with working code for rapid hypothesis testing and user feedback collection.
auto_execution_mode: 1
---

# Build Prototype Workflow

## Purpose
Transform business ideas into testable prototypes with working code, enabling rapid validation of business hypotheses through functional software.

## Input Parameters: $ARGUMENTS (Optional)
**Accepts:** Business hypothesis | Feature idea | Problem statement | User need description

**Example Inputs:**
- "Test if small business owners will pay for automated invoice tracking"
- "Validate market demand for markdown-to-slides conversion"
- "Build prototype for real-time collaboration on documents"

## Output
**Deliverables in mvp/ folder:**
- `mvp-scope-and-journeys.md` - Validation plan with hypothesis and user journeys
- `src/` - Complete working source code (HTML, JS, CSS, assets)
- `README.md` - Setup and launch instructions
- `deployment-guide.md` - Deployment procedures
- `test-results/` - Automated validation evidence with screenshots
- Print the following: 
  - List of rules used by the workflow in bulleted format
  - Evaluation Scores in tabular format with average score. 
  - Evaluation summary (less than 100 words).
  **Do not save as file.**

## Execution Flow

### Core Principles
- **Code-first delivery**: Working source code is primary output
- **Validation-focused**: Every feature enables hypothesis testing
- **Minimal viable**: Build only what's needed for validation
- **Launch-ready**: Stakeholders can test immediately
- **Modern design**: Clean, accessible, contemporary interface
- **Time-boxed**: Strict 40-hour constraint adherence

### Hypothesis Definition (3 hours)

**Create mvp/ folder structure**
```bash
mkdir -p mvp/src
mkdir -p mvp/test-results/screenshots
```

**Analyze business hypothesis** (use sequential-thinking MCP)
- Extract core business assumption from input
- Identify validation success criteria
- Define primary and secondary user personas for testing
- Map validation user journeys

**Generate validation plan**

**Before writing MVP scope, list all features to build:**
| Feature | Purpose | Validates Hypothesis? |
|---------|---------|----------------------|
| ... | ... | Yes/No |
**Now expand each feature listed above.**

- Create `mvp/mvp-scope-and-journeys.md` with:
  - Business hypothesis statement
  - Validation success criteria
  - Primary and secondary user personas
  - Validation user journeys
  - MVP validation features (minimal set)
  - 40-hour timeline breakdown

**Request user approval**
- Present mvp-scope-and-journeys.md to user
- Wait for explicit approval before proceeding
- **STOP HERE until user confirms approval**

### Prototype Build (32 hours)

**Pre-implementation verification**
- Read approved mvp-scope-and-journeys.md file
- Create TodoWrite plan with all source files
- Verify folder structure exists

**Technology selection** (use sequential-thinking MCP for rapid decisions)
- Choose proven frameworks (Next JS/vanilla JS)
- Select UI components (prioritize shadcn/ui)
- Determine if backend needed (auto-detect from requirements. Prefer for the complex requirements)
- Plan minimal tech stack for validation

**Generate working code** (use context7 MCP for framework guidance)
- Build validation interface using shadcn components
- Implement core hypothesis testing features
- Create feedback collection mechanisms
- Generate backend services if needed (Node.js/Express with mock data)

**File creation with path validation**
- Use `Write('mvp/src/filename')` for ALL files
- Verify each file: `ls -la mvp/src/filename`
- Track creation in TodoWrite
- Minimum required: index.html, app.js, style.css, package.json

**Modern UI implementation**
- Clean, contemporary interface design
- Neutral-first color system with purposeful accents
- Spacious layout with strong visual hierarchy
- Accessible contrast and responsive behavior
- Essential shadcn components only

### Documentation & Testing Setup (2 hours)

**Create documentation**
- `mvp/README.md` - Setup and launch guide
- `mvp/deployment-guide.md` - Deployment instructions
- Document validation procedures

**Prepare for testing**
- Define validation test scenarios
- Set up test environment configuration
- Document expected validation outcomes

### Automated Validation (3 hours)

**Launch prototype** (use Playwright MCP)
```
1. Navigate: mcp__playwright__browser_navigate(url)
2. Capture: mcp__playwright__browser_take_screenshot("mvp-launch.png")
```

**Test user journeys** (use Playwright MCP)
```
For each persona:
1. Snapshot: mcp__playwright__browser_snapshot()
2. Interact: mcp__playwright__browser_click/fill_form/type
3. Validate: mcp__playwright__browser_wait_for(expected_result)
4. Capture: mcp__playwright__browser_take_screenshot("journey-X.png")
```

**Validate feedback mechanisms**
- Test feedback collection features
- Verify submission workflows
- Document functionality

**Generate validation report**
- Create `mvp/test-results/validation-report.md`
- Document test results with screenshot evidence
- Assess hypothesis testing readiness

### Success Criteria
**Technical Success:**
- Prototype builds and runs without errors
- All validation features functional
- Professional UI suitable for user testing

**Business Success:**
- Clear value proposition demonstration
- Hypothesis testing enabled
- User feedback collection functional

**Process Success:**
- User approval obtained before implementation
- Working code delivered within 24 hours
- Complete documentation for stakeholder testing

### Quality Assurance Framework

#### Completion Checklist

**Verify all deliverables exist:**
```bash
ls -d mvp/                              # MVP folder
ls mvp/mvp-scope-and-journeys.md        # Validation plan
ls -la mvp/src/                         # Source code folder
ls mvp/src/index.html                   # Entry point
ls mvp/src/app.js                       # Application logic
ls mvp/src/style.css                    # Styling
ls mvp/README.md                        # Setup guide
ls mvp/deployment-guide.md              # Deployment guide
ls -d mvp/test-results/                 # Test results folder
ls mvp/test-results/validation-report.md # Validation report
ls mvp/test-results/screenshots/        # Visual evidence
tree mvp/                               # Complete structure
```
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

Once the output is generated, Score the prototype built to evaluate its quality against the following metrics, providing a percentage score (1-100%) for each.

### Prototype Quality Assessment

| # | Evaluation Dimension | Score | Gate | Assessment Criteria |
|---|---------------------|-------|------|---------------------|
| 1 | Build Success | [PASS/FAIL] | MUST PASS | Prototype builds and runs without critical errors |
| 2 | Validation Stories | [0-100%] | MUST=100% | All defined validation journeys implemented and testable |
| 3 | Local Launch Ready | [PASS/FAIL] | MUST PASS | Stakeholders can launch locally using provided instructions |
| 4 | Documentation Complete | [0-100%] | >=80% | Setup, testing, and validation procedures included |
| 5 | Business Alignment | [0-100%] | >=80% | Alignment with strategic objectives and stakeholder needs |
| 6 | Timeline Adherence | [0-100%] | >=80% | Delivery within 40-hour constraint with time breakdown |

### Detection Rules

| # | Detection Logic | Penalty |
|---|-----------------|---------|
| 1 | Run build + startup commands; check for errors | Critical error: BLOCKED |
| 2 | Count validation journeys defined; verify each testable | Missing journey: -20%/instance |
| 3 | Execute setup instructions; verify launch success | Launch fails: BLOCKED |
| 4 | Check for README, setup guide, validation procedures | Missing doc: -15%/doc |
| 5 | Map prototype features to business objectives | Unaligned feature: -10%/feature |
| 6 | Sum task hours; verify <=40; breakdown present | Over 40h: -5%/hour; No breakdown: -15% |

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
| User Journey Coverage | Add missing validation journey |
| Documentation Completeness | Add missing docs |
| Business Alignment | Add objective mapping |
| Timeline Adherence | Add time breakdown |

**Non-Patchable (Skip):** Functional Validation, Deployment Readiness (code execution required), source missing, patch >30% of section, >=3 failing dimensions

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

*This workflow creates validation prototypes for hypothesis testing, not complete applications. Focus is on rapid validation through minimal working software.*