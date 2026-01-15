---
description: Generates comprehensive implementation tasks from feature requirements, user stories, or functional specifications with thorough research, context integration, and quality validation
auto_execution_mode: 1
---

# Unified Task Generator
As a Senior Software Engineer expert in Full Stack development, generate comprehensive implementation tasks based on the provided input. This unified command handles all task generation scenarios with consistent quality and thorough research approach.

## Input Parameter: $ARGUMENTS (Mandatory)
**Accepts:** User story file path | User story URL | User story text | Feature requirements | Functional specification

### Argument Types:
1. **User Story File**: Path to us_XXX.md file (e.g., .propel/context/tasks/us_001/us_001.md)
2. **User Story URL**: URL pointing to user story specification
3. **User Story Text**: Direct user story content in "As a... I want... so that..." format
4. **Feature Requirements**: Path to requirements specification
5. **Functional Specification**: General functional description

**Optional Parameters:**
- `--skip-history`: Skip git history analysis and findings registry lookup for faster execution (default: false)

### User Story Input Processing
**When $ARGUMENTS contains user story (file, URL, or text):**
1. **Extract US ID**: Parse us_XXX identifier from file path, content, or generate if text input
2. **Create Task Folder**: Ensure `/.propel/context/tasks/us_<ID>/` directory exists
3. **Task File Naming**: Generate tasks as `task_<seqnum>_<descriptive_name>.md` within US folder
4. **Parent Story Reference**: Maintain traceability to parent user story in all tasks
5. **Acceptance Mapping**: Map implementation tasks to user story acceptance criteria

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
- If no US ID can be extracted or found -> Create tasks in `/.propel/context/tasks/` folder directly
- Use standard task numbering: `task_XXX_<descriptive_name>.md`
- Log clearly where tasks are being created (US folder vs. root Tasks folder)

## Output
- Artifact generation: `/.propel/context/tasks/us_<ID>/*.md`
- Print the following: 
    - List of rules used by the workflow in bulleted format
    - Evaluation Scores in tabular format with average score. 
    - Evaluation summary (less than 100 words).
    **Do not save as file.**

**Note:**
- If the output file is already available make the necessary changes to applicable sections. Do not overwrite the whole file.
- Generate the output using the .propel/templates/task-template.md and `.propel/templates/design-reference-template.md` (for UI tasks only) templates.

### Output Specifications

#### File Organization Strategy:
**For User Story Tasks (US ID found in input):**
- **Directory**: `/.propel/context/tasks/us_<ID>/`
- **File Pattern**: `task_<seqnum>_<descriptive_name>.md`
- **Example**: `/.propel/context/tasks/us_001/task_001_implement_login_ui.md`
- **Trigger**: Valid us_XXX pattern extracted from file path, URL, or text content

**For General Tasks (no US ID found - fallback):**
- **Directory**: `/.propel/context/tasks/us_<short_descriptive_name>/`
- **File Pattern**: `task_<seqnum>_<descriptive_name>.md`
- **Example**: `/.propel/context/tasks/task_001_setup_database.md`
- **Trigger**: No valid US ID extractable from input

**Sequence Number Logic**:
- Auto-increment based on existing task files in the target directory
- Use zero-padded 3-digit format (001, 002, 003...)
- Separate sequence per us_<ID> folder
- Descriptive name should reflect primary functionality being implemented

### Directory Management

#### US ID Extraction Priority
1. **Parse Input**: Attempt to extract us_XXX from input using patterns defined above
2. **Validate Format**: Check if extracted ID matches us_XXX pattern (3-digit numeric)
3. **Fallback to Root**: If no valid US ID found -> use `/.propel/context/tasks/us_<short_descriptive_name>/` directly

#### Directory Creation Logic
**With Valid US ID Found**:
- **Directory**: `/.propel/context/tasks/us_<ID>/`
- **File Pattern**: `task_<seqnum>_<descriptive_name>.md`
- **Sequence**: Continue from highest existing task number in US folder
- **Example**: `/.propel/context/tasks/us_001/task_001_implement_login_form.md`

**Without Valid US ID (Fallback)**:
- **Directory**: `/.propel/context/tasks/us_<short_descriptive_name>/`
- **File Pattern**: `task_<seqnum>_<descriptive_name>.md`
- **Sequence**: Continue from highest existing task number in root Tasks folder
- **Example**: `/.propel/context/tasks/us_setup_db/task_001_setup_database.md`

#### Task Organization Examples
```
.propel/context/tasks/
+-- us_001/                          # US ID found (us_001)
|   +-- us_001.md
|   +-- task_001_implement_login_form.md
|   +-- task_002_add_validation_logic.md
|   +-- task_003_create_unit_tests.md
+-- us_002/                          # US ID found (us_002)
|   +-- us_002.md
|   +-- task_001_setup_password_reset.md
+-- us_general_setup/                # US ID not found -> fallback directory
|   +-- us_general_setup.md
|   +-- task_001_general_setup.md
+-- us_oauth_integration/            # US ID not found -> fallback directory
|   +-- task_001_oauth_integration.md
+-- us_db_integration/               # US ID not found -> fallback directory
    +-- task_001_database_integration.md

```

#### Logging and Transparency
- **Always log** the decision: "Creating tasks in us_XXX folder" or "No US ID found, creating tasks in root /.propel/context/tasks/us_<short_descriptive_name>/"
- **Document reasoning** for task placement in task files
- **Maintain clear traceability** between input type and output location

## Core Principles
- **Story-Task Mapping**: One task maps to one requirement/story only
- **Size Constraints**: Split tasks exceeding 8 hours into smaller units
- **Technology Separation**: Separate tasks by stack (frontend, backend, database)

## Execution Flow

### Delta Processing Protocol
**Before generating any output:**
1. Check if target output file (`task_<seqnum>_<name>.md`) already exists
2. If EXISTS: Read current content, identify only sections requiring update, EDIT those sections only
3. If NOT EXISTS: Generate complete new file
4. Log: "Mode: [Delta update to X sections | New file creation]"

### 1. Input Processing
- Detect input type and extract US ID using defined patterns
- Create appropriate folder structure based on US ID presence
- Read spec.md, design.md, epics.md, and models.md for context

### 2. Task Decomposition Rule
- **Per-File Limits**: Each task file must have <=8 todos AND <=8 hours estimated effort
- **One Layer Per File**: Each technology layer (FE/BE/DB/Infra) = separate task file
- **Decomposition Strategy**:
  1. Split by technology layer FIRST — each layer becomes its own `task_XXX_<layer>_*.md` file
  2. Within a layer, split further if todos >8 or effort >8 hours
  3. Each file must be independently testable
  4. Maintain traceability: Task File -> User Story -> Epic
- **Validation Gate**: Before finalizing, verify EACH file meets limits. If any file exceeds limits, split into additional files.

### Deep Research Methodology

Optimize for success probability over execution speed. Spawn multiple agents and subagents using batch tools for comprehensive analysis.

#### 1. Codebase Intelligence Gathering (use Sequential-Thinking MCP)
- **Screen Specification Analysis**: Read `.propel/context/docs/figma_spec.md` for screen inventory, states, and flow definitions (UI tasks only)
- **Design Token Analysis**: Read `.propel/context/docs/designsystem.md` for design tokens and component mappings (UI tasks only)
- **User Story Design Mapping**: Identify which user stories this task relates to and extract their screen references
- **Visual Asset Loading**: Load Figma URLs OR design images referenced in design documents for this task's user stories
- **Pattern Discovery**: Search for similar features/implementations in existing codebase
- **Impact Analysis**: Identify existing features requiring modification to maintain functionality
- **Reference Mapping**: Document files and patterns to reference in task design
- **Convention Documentation**: Catalog existing code styles, naming patterns, and architectural approaches
- **Test Pattern Analysis**: Review testing strategies and validation approaches used in the project
- **Best Practices Refresh**: Search for "[technology] best practices [current year]" to find recent guides

#### 2. External Knowledge Acquisition
- **Implementation Research**: Search for similar features/patterns in open-source projects
- **Sample Code**: Use `mcp__context7__get-library-docs` (use Context7 MCP) for technology-specific, version-specific documentation, code examples, design principles and coding standards
- **Version Pinning**: Use `mcp__context7__resolve-library-id` (use Context7 MCP) to pin exact framework/library versions referenced in the task (e.g., .NET 8, EF Core 8, React 19, Python) so subsequent docs/examples are version-accurate. Do not include or cite libraries/examples requiring higher versions than those pinned; they may introduce breaking changes.
- **Design Asset Integration**: Reference Figma links OR design images in task context (only when UI impact exists)
- **UI Implementation Guidance**: Include specific design specifications from visual assets (Figma frames OR image files)
- **Component Mapping**: Map design components to code implementation patterns
- **Visual Validation Criteria**: Define pixel-perfect implementation requirements using available design references
- **Style Guidelines**: Research project-specific or framework coding standards
- **Documentation Deep-Dive**: Library documentation with specific section URLs
- **Example Collection**: GitHub repositories, StackOverflow solutions, technical blog implementations
- **Best Practice Synthesis**: Industry patterns, performance considerations, and common pitfalls

#### 3. Contextual Clarification (When Needed)
- **Pattern Alignment**: Which existing implementations should be mirrored and their locations?
- **Integration Requirements**: How should new features integrate with existing systems?
- **Quality Standards**: What testing, documentation, and validation standards apply?

#### 4. Historical Issue Analysis (Optional: skip with --skip-history)

**Git History Check** (~3 commands):
- `git log --grep="fix\|bug" --oneline -5 -- <affected_files>` - Last 5 bug-related commits only
- `git shortlog -sn -5 -- <affected_files>` - Top 5 contributors only
- If >3 bug commits found, flag file as "High-Risk (frequent fixes)"

**Git Fallback** (if git unavailable or commands fail):
- Skip git history check, proceed with registry lookup only"

**Findings Registry Lookup** (selective read):
- Read only the Index section from `.propel/learnings/findings-registry.md`
- If affected files appear in index, grep only those specific entries
- Do NOT read full registry - use index for O(1) lookup

**Output**: Single warning block if issues found, nothing if clean

#### MCP Fallback Protocol
**If Sequential-Thinking MCP unavailable:**
- Use structured analysis: List (1) dependencies, (2) risks, (3) edge cases explicitly
- Document: "Fallback: Manual structured analysis (MCP unavailable)"

**If Context7 MCP unavailable:**
- Use web search: "[technology] [version] official documentation [current year]"
- Document: "Fallback: Web search for docs (Context7 unavailable)"

### Essential Project Intelligence

#### Reference Materials Analysis
- **Codebase Structure**: Explore existing codebase (`app`, `backend`, `server` folders) for code patterns and architectural decisions
- **Documentation Review**: Study existing README files, API documentation, and inline code comments

#### Epic Context Integration
- **epics.md**: Reference EP-XXX for task grouping and priority
- **models.md**: Use sequence diagrams for workflow understanding
- **Requirement Traceability**: Trace tasks through User Story -> Epic -> Requirements

*** Codebase comprehension is non-negotiable for quality task generation ***

### Task Design Framework

#### Template Foundation
Base all tasks on `.propel/templates/task-template.md` structure for consistency.
Integrate design context from `.propel/templates/design-reference-template.md` for UI tasks only.

#### Critical Context Integration

**Documentation Context**
- Specific URLs with relevant sections highlighted
- API documentation with version-specific considerations
- Framework guides with implementation examples

**Architecture Context**
- System design patterns and their rationale
- Integration points and data flow diagrams  
- Performance and scalability considerations

**Implementation Context**
- Real code snippets from existing codebase
- Configuration examples and environment setup
- Error handling patterns used in the project

**Design Context (UI Impact Only)**
- **Screen Specifications**: From figma_spec.md (derived from epics.md UI impact)
- **Design Tokens**: From designsystem.md
- **Architectural Views**: From models.md (sequence diagrams for UC-XXX)
- **Epic Context**: From epics.md for task prioritization
- **Visual References**: Figma URLs OR design images from design documents
- **Component Specifications**: Component mappings from designsystem.md
- **Flow Definitions**: Prototype flows and interactions from figma_spec.md
- **Responsive Design**: Breakpoints and behaviors from figma_spec.md

**Visual Assets (From figma_spec.md and designsystem.md)**
- **Screen State Requirements**: All 5 states (Default/Loading/Empty/Error/Validation) from figma_spec.md
- **Component Documentation**: Visual specs from designsystem.md component mappings
- **Design-to-Code Mapping**: Guidelines extracted from design documents
- **Pixel-Perfect Requirements**: Implementation criteria from screen specifications

**Example Task Design Reference**:
```yaml
Design Context:
  screen_spec: .propel/context/docs/figma_spec.md#SCR-001
  design_tokens: .propel/context/docs/designsystem.md
  user_story: US-001 - User Login Interface
  visual_references:
    figma_url: https://figma.com/file/xyz?node-id=2:45
    # OR design_images: .propel/context/design/US-001/login_mockup.png
  design_tokens:
    primary_color: "#007AFF"
    typography: "SF Pro Display, 32px, 600"
  validation_criteria:
    screenshot_comparison: true
    pixel_tolerance: 5%
    responsive_breakpoints: [375px, 768px, 1440px]
```

**Critical Knowledge**
- Library-specific quirks and version compatibility issues
- Known limitations and workaround strategies
- Security considerations and compliance requirements

**Pattern Consistency**
- Existing approaches for similar functionality
- Naming conventions and code organization patterns
- Testing and validation strategies already established

#### Implementation Blueprint Architecture

**Pseudocode Foundation**
- Start with high-level algorithmic approach
- Break down complex operations into manageable steps
- Reference real files demonstrating similar patterns

**Error Resilience Strategy**
- Identify potential failure points and mitigation approaches
- Document rollback procedures for destructive operations
- Plan validation checkpoints throughout implementation

**Before writing tasks, list all tasks to generate:**
| Task-ID | Summary | Technology Layer |
|---------|---------|------------------|
| ... | ... | ... |
**Now expand each task listed above.**

**Sequential Task Breakdown**
- List implementation tasks in logical dependency order
- Include mandatory unit test creation/updates for each component
- Define clear completion criteria and validation gates

**Versioned Doc Excerpts** 
- Include excerpts/links retrieved via `mcp__context7__get-library-docs` (use Context7 MCP) (tied to IDs from `resolve-library-id`) for patterns the task expects (validation, transactions, async I/O, HTTP semantics).


### Context Documentation Requirements

### Essential References Package
- **Technical Documentation**: All URLs, guides, and examples needed for implementation
- **Implementation Examples**: Complex code snippets with explanations and context
- **Integration Points**: How the feature connects with existing systems
- **Quality Gates**: Testing requirements, performance benchmarks, security validations


### Quality Assurance Framework

#### Pre-Delivery Checklist
- [ ] **User Story Validation**: Parent user story identified and referenced correctly
- [ ] **Folder Structure**: Tasks created in correct us_XXX folder when applicable
- [ ] **Task Numbering**: Sequential numbering within US folder maintained
- [ ] **Acceptance Mapping**: Tasks map to user story acceptance criteria
- [ ] **Story Traceability**: Each task references parent us_XXX appropriately
- [ ] **Context7 References**: Version-pinned docs (IDs + links) are included for all critical patterns the task requires.
- [ ] **Sequential Plan Checklist**: A traceability-friendly checklist derived via `mcp__sequential_thinking__plan` is embedded for execution/analysis.
- [ ] **Screen Spec Loaded**: figma_spec.md read for screen inventory and state requirements (UI tasks only)
- [ ] **Design Tokens Loaded**: designsystem.md read for colors, typography, spacing (UI tasks only)
- [ ] **User Story Screen Mapping**: Task linked to specific screens (SCR-XXX) from figma_spec.md
- [ ] **Visual Assets Referenced**: Figma URLs OR design images loaded from design documents (UI tasks only)
- [ ] **State Requirements Included**: All 5 states defined for each screen in task (UI tasks only)
- [ ] **Component Mapping**: Design components from designsystem.md mapped to code implementations
- [ ] **Validation Criteria Defined**: Pixel-perfect requirements from figma_spec.md included
- [ ] **Context Completeness**: All necessary implementation context included and accessible
- [ ] **Task Scope Validation**: Tasks are under 8 hours and properly split by technology stack when applicable
- [ ] **Technology Stack Separation**: Frontend, backend, database, and infrastructure tasks are independently manageable
- [ ] **Validation Executability**: All quality gates can be executed by AI without human intervention
- [ ] **Pattern Consistency**: References and follows established project patterns
- [ ] **Implementation Clarity**: Clear, unambiguous path from start to completion
- [ ] **Error Handling**: Comprehensive error scenarios documented with responses
- [ ] **Test Strategy**: Testing approach defined with specific validation criteria

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

Once the output is generated, Score the generated task to evaluate its quality against the following metrics, providing a percentage score (1-100%) for each.

### Task Quality Assessment

| # | Evaluation Dimension | Score | Gate | Assessment Criteria |
|---|---------------------|-------|------|---------------------|
| 1 | Template Adherence | [0-100%] | MUST=100% | Strict conformance to template; no missing sections; no extra content |
| 2 | Task Decomposition | [0-100%] | >=80% | Effort estimated per todo (Trivial/Simple/Moderate/Complex/High); total <=8 hours; <=8 todos; single layer per task file; split into separate files when limits exceeded |
| 3 | Business Value Alignment | [0-100%] | >=80% | Tasks map to user story outcomes and measurable objectives |
| 4 | Acceptance Criteria Coverage | [0-100%] | >=80% | Each task traces to specific criteria with validation gates |
| 5 | Epic Alignment | [0-100%] | >=80% | Tasks map to user stories which map to epics.md EP-XXX |
| 6 | Context Completeness | [0-100%] | >=80% | All required context (docs, patterns, examples) included |
| 7 | Validation Gates | [0-100%] | >=80% | Executable tests and quality gates defined for each task |
| 8 | Architectural Consistency | [0-100%] | >=80% | Tasks align with models.md sequence diagrams |
| 9 | Layer Purity | [0-100%] | >=80% | Each layer task contains only its layer activities (FE: UI/CSS/components; BE: API/services; DB: schema/queries; Infra: config/deploy); no cross-layer contamination |

### Detection Rules

| # | Detection Logic | Penalty |
|---|-----------------|---------|
| 1 | Compare sections vs development-task-template.md | Missing: -20%; Extra: -15% |
| 2 | Count todos per task; estimate effort using complexity classification (Trivial=0.5h, Simple=1-2h, Moderate=2-4h, Complex=4-6h, High=6-8h); verify <=8 todos AND total <=8 hours | >8 todos: -10%/extra todo; >8 hours: -15%/task; missing estimation: -20%/task |
| 3 | Task maps to user story outcome | No mapping: -15%/task |
| 4 | Each task traces to acceptance criteria | Missing trace: -10%/task |
| 5 | Tasks map to epics.md EP-XXX | No epic link: -10%/task |
| 6 | Required context (docs, patterns) included | Missing context: -10%/missing item |
| 7 | Validation gates defined per task | No gate: -15%/task |
| 8 | Tasks align with models.md sequence diagrams | Misalignment: -10%/instance |
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
| Task Decomposition | Split tasks with >8 todos or >8 hours effort |
| Business Value Alignment | Add user story mapping |
| Acceptance Criteria Coverage | Add criteria traceability |
| Epic Alignment | Add EP-XXX references |
| Context Completeness | Add missing context docs |
| Validation Gates | Add test/quality gates |
| Architectural Consistency | Add sequence diagram references |
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