---
description: Generates detailed user stories from epic URLs, feature text, or scope files. Creates individual user story files with proper effort estimation and breakdown.
auto_execution_mode: 1
---

# User Story Generator
As an expert Product Owner and Business Analyst, generate comprehensive user stories that follow INVEST principles and deliver clear business value. This command focuses specifically on user story creation with detailed acceptance criteria and proper effort estimation.

**Example Usage:**
- `/create-user-stories scope_document.md EP-001` (specific epic within scope file)
- `/create-user-stories scope_document.md` (all epics in scope file)
- `/create-user-stories EP-001` (specific epic ID from epics.md)
- `/create-user-stories https://docs.company.com/epic-auth` (epic URL)
- `/create-user-stories "Build user authentication with OAuth2, password reset, and role-based access"` (direct text)
- `/create-user-stories` (all epics from existing epics.md)

## Input Parameters: $ARGUMENTS (Optional)
**Accepts:** Scope file path [Epic ID] | Epic ID | Feature text | Epic URL
**Default Behavior:** Uses .propel/context/docs/epics.md for epic definitions if no arguments provided

### Context Sources (Always Read)
- `.propel/context/docs/epics.md` - **PRIMARY** - Epic definitions with requirement mappings (EP-XXX)
- `.propel/context/docs/spec.md` - **REQUIRED** - FR-XXX, UC-XXX details (UXR-XXX NOT in spec.md)
- `.propel/context/docs/design.md` - **REQUIRED** - NFR-XXX, TR-XXX, DR-XXX details
- `.propel/context/docs/models.md` - **REQUIRED** - Sequence diagrams per UC-XXX (impacts acceptance criteria and edge cases)
- `.propel/context/docs/figma_spec.md` - **CONDITIONAL** - UXR-XXX details (only if UI-impacting story)

### Argument Combinations:
1. **Scope file + Epic ID**: `scope_file.md EP-001` - Generate stories for specific epic within scope file
2. **Scope file only**: `scope_file.md` - Generate stories for all epics in scope file
3. **Direct Text**: Generate stories from direct feature text
4. **Epic ID only**: `EP-001` - Find epic in epics.md and generate stories
5. **No arguments**: Use .propel/context/docs/epics.md and generate stories for all epics. If the file doesn't exist, prompt user to run generate-requirements first.

### Input Processing Instructions
**CRITICAL**: Before proceeding with user story generation, determine input type and process accordingly:

#### Input Type Detection
**Parse $ARGUMENTS to identify the combination:**

1. **Scope File + Epic ID**: Two arguments where first is file path (.md, .pdf, .txt, .docx) and second is Epic ID (EP-XXX)
2. **Scope File Only**: Single argument that is a file path with extensions (.pdf, .txt, .md, .docx)
3. **Epic ID Only**: Single argument matching Epic ID pattern (EP-001, EP-002, etc.)
4. **Epic URL**: Single argument containing URLs pointing to epic specifications or documents
5. **Direct Text**: Single argument that doesn't match file path or Epic ID patterns
6. **No Arguments**: If `$ARGUMENTS` is empty, use .propel/context/docs/spec.md as source

#### Scope File + Epic ID Processing (Two Arguments)
If `$ARGUMENTS` has two parts: scope file + Epic ID:
1. **File Validation**: Verify the scope file exists and is readable
2. **Content Extraction**: Read scope file content (supports .pdf, .txt, .md, .docx)
3. **Epic Filtering**: Parse file to find only the specified Epic ID (EP-XXX)
4. **Epic Analysis**: Extract epic details, mapped requirements, and scope for the specified epic only
5. **Story Generation**: Generate user stories for the single identified epic within the scope file

#### Scope File Only Processing (Single File Argument)
If `$ARGUMENTS` is a single file path:
1. **File Existence Check**: Verify the file exists using appropriate tools
2. **Read File Contents**: Use the Read tool to extract content
   - For .pdf files: Read and extract text content
   - For .txt files: Read plain text content
   - For .md files: Read markdown content
   - For .docx files: Read and extract document content
3. **Epic Discovery**: Parse file to identify all epics within the scope file
4. **Multi-Epic Processing**: Generate user stories for ALL epics found in the scope file

#### Epic ID Only Processing (Single Epic Argument)
If `$ARGUMENTS` contains single Epic ID (EP-001, EP-002, etc.):
1. **epics.md Lookup**: Read .propel/context/docs/epics.md to find the specified epic
2. **Requirement Extraction**: Get mapped requirement IDs from epic table
3. **Cross-Reference**: Read spec.md for FR details, design.md for NFR/TR/DR details
4. **UXR Integration**: IF UI-impacting epic, read figma_spec.md for UXR-XXX details; ELSE skip UXR
5. **Sequence Diagram Analysis**: Read models.md for UC-XXX sequence diagrams to inform acceptance criteria and edge cases
6. **Story Generation**: Generate user stories covering all mapped requirements

#### Epic URL Processing (Single URL Argument)
If `$ARGUMENTS` contains URLs:
1. **URL Validation**: Verify URLs are accessible and contain epic/feature specifications
2. **Content Extraction**: Use WebFetch to retrieve epic content from URLs
3. **Epic Parsing**: Extract epic requirements, scope, and business context
4. **Story Generation**: Generate user stories based on epic content

#### Direct Text Processing
If `$ARGUMENTS` is direct text specification:
1. **Text Validation**: Ensure the provided text contains meaningful feature/epic content
2. **Content Processing**: Use the text directly as source material for story generation
3. **Epic Creation**: Create implicit epic structure from text content
4. **Story Generation**: Generate user stories from analyzed text

#### Default Processing (No Arguments)
If no `$ARGUMENTS` provided:
1. **epics.md Check**: Verify .propel/context/docs/epics.md exists and contains epic definitions
2. **All Epics Discovery**: Parse all epics (EP-XXX) from the epics.md file
3. **Context Loading**: Read spec.md, design.md, and models.md for requirement details and sequence diagrams
4. **UXR Loading**: IF figma_spec.md exists, read UXR-XXX for UI-impacting stories; ELSE skip UXR
5. **Multi-Epic Processing**: Generate user stories for ALL epics found in epics.md

#### Fallback Handling
- If epic/file cannot be read: Request user to provide alternative input or check accessibility
- If text is too brief: Request additional specification details
- If epics.md doesn't exist: Request user to run create-epics workflow first or provide direct input
- If models.md doesn't exist: Proceed but note acceptance criteria may be less precise without sequence diagram context
- If figma_spec.md doesn't exist: Skip UXR-XXX integration (non-UI stories don't need UXR)

#### MCP Fallback Protocol
**If Sequential-Thinking MCP unavailable:**
- Use the structured questions defined in this workflow (e.g., "What user problem? What is the acceptance test?")
- Document: "Fallback: Manual structured analysis (MCP unavailable)"

**If Context7 MCP unavailable:**
- Use web search: "[technology] [version] official documentation [current year]"
- Document: "Fallback: Web search for docs (Context7 unavailable)"

## Output
- Artifact generation: `/.propel/context/tasks/us_<ID>/us_<ID>.md`
- - Print the following: 
    - List of rules used by the workflow in bulleted format
    - Evaluation Scores in tabular format with average score. 
    - Evaluation summary (less than 100 words).
    **Do not save as file.** (Console output only)

**Note:**
- If the output file is already available make the necessary changes to applicable sections. Do not overwrite the whole file.
- Always generate a unified file for each of the user story.
- Generate the output using the `.propel/templates/user-story-template.md` template.

### Output Specifications

#### File Organization Strategy:
**US ID found in input**
- **Directory**: `/.propel/context/tasks/us_<ID>/`
- **File Pattern**: `us_<ID>.md`
- **Example**: `/.propel/context/tasks/us_001/us_001.md`
- **Trigger**: Valid us_XXX pattern extracted from file path, URL, or text content

**No US ID found - fallback**
- **Directory**: `/.propel/context/tasks/us_<short_descriptive_name>/`
- **File Pattern**: `us_<short_descriptive_name>.md`
- **Example**: `/.propel/context/tasks/us_setup_database/us_setup_database.md`
- **Trigger**: No valid US ID extractable from input

#### US Story Organization Examples
```
.propel/context/tasks/
+-- us_001/
|   +-- us_001.md
+-- us_002/
|   +-- us_002.md
+-- us_general_setup/
|   +-- us_general_setup.md
+-- us_oauth_integration/
|   +-- us_oauth_integration.md
+-- us_database_integration/
    +-- us_database_integration.md
```

## Core Principles
- **INVEST Compliance**: Stories must be Independent, Negotiable, Valuable, Estimable, Small, Testable
- **Epic Traceability**: Every story maps to a parent epic with clear linkage
- **Business Value Focus**: Each story delivers tangible user or business value

## Execution Flow

### Delta Processing Protocol
**Before generating any output:**
1. Check if target output file (`us_<ID>.md`) already exists
2. If EXISTS: Read current content, identify only sections requiring update, EDIT those sections only
3. If NOT EXISTS: Generate complete new file
4. Log: "Mode: [Delta update to X sections | New file creation]"

### 1. Content Processing Workflow
1. **Input Analysis**: Determine input type (URL, ID, file, text, or default)
2. **Content Extraction**: Retrieve epic/feature specification from source
3. **Epic Parsing**: Extract requirements, business context, and technical constraints
4. **Story Decomposition**: Break down epic into manageable, testable user stories. For each story, answer: (1) What user problem does it solve? (2) What is the acceptance test?
5. **Story Generation**: Create individual story files following template structure

### User Story Breakdown Methodology

#### Story Sizing and Breakdown Rules
- **Maximum Story Size**: 5 story points per story
- **Story Point Calculation**: 1 story point = 8 hours of effort
- **Effort Threshold**: Stories requiring >40 hours must be broken down into smaller stories
- **Story Independence**: Each story must be testable independently and deliver business value
- **INVEST Principles**: Stories should be Independent, Negotiable, Valuable, Estimable, Small, and Testable

### Story Generation Strategy

#### 1. Epic Analysis and Requirements Mapping (Use Sequential-Thinking MCP)
- **Requirement Extraction**: Identify all functional and non-functional requirements within epic scope
- **Business Value Assessment**: Determine user value and business impact for each requirement
- **Technical Complexity Analysis**: Assess implementation complexity and dependencies. Identify: (a) Dependencies on other stories (b) Technical risks (c) Edge cases
- **Story Decomposition**: Break requirements into logical, deliverable user stories. For each, document: User role, Goal, Benefit

#### Sequence Diagram Integration for Story Quality
When generating acceptance criteria and edge cases:
- **Read models.md**: Extract sequence diagrams for each UC-XXX mapped to the epic
- **Flow Analysis**: Use sequence diagram flows to identify:
  - Success path steps -> acceptance criteria
  - Alternative paths -> edge cases
  - Error handling flows -> error scenario criteria
- **Actor Interactions**: Map participant interactions to user story steps
- **Data Flow Tracing**: Identify data transformations and validations from sequence flows

#### 2. Story Template Compliance
All generated stories MUST follow the exact structure from `.propel/templates/user-story-template.md`

**ID Management:**
- **Sequential Numbering**: Continue from highest existing us_XXX ID
- **Zero-Padded Format**: us_001, us_002, ..., us_999
- **Cross-Epic Continuity**: Maintain sequence across different epics

#### 3. Effort Estimation and Breakdown
**Story Point Estimation Process:**
1. **Complexity Assessment**: Evaluate technical complexity, unknowns, and dependencies
2. **Effort Calculation**: Estimate development time in hours
3. **Point Assignment**: Convert hours to story points (8 hours = 1 point)
4. **Breakdown Logic**: If story >5 points, decompose into smaller, focused stories
5. **Validation**: Ensure each broken-down story still delivers independent value

**Before writing user stories, list all stories to generate:**
| US-ID | Summary | Parent Epic |
|-------|---------|-------------|
| ... | ... | ... |
**Now expand each story listed above.**

#### Template Population Process
For each generated story:

1. **Read userstory-base.md**: Load the exact template structure
2. **Create Directory**: Generate .propel/context/tasks/us_XXX/ directory
3. **Generate Story File**: Create us_XXX.md within the directory
4. **Populate Sections**: Fill all template sections with generated content
5. **Validate Structure**: Ensure template compliance and completeness

### Story Generation Examples

#### Example 1: Authentication Epic (EP-001)
**Input:** EP-001 with requirements FR-001, FR-002, FR-003, NFR-002, TR-004 (+ UXR-001 from figma_spec.md if UI-impacting)

**Generated Stories:**
```
us_001: User Account Registration
- Description: As a new user, I want to create an account with email validation, so that I can access the platform securely
- Epic: EP-001
- Requirements: FR-001, UXR-001
- Effort: 3 story points

us_002: User Login Authentication
- Description: As a registered user, I want to log in with my credentials, so that I can access my account
- Epic: EP-001
- Requirements: FR-002, NFR-002
- Effort: 2 story points

us_003: Password Reset Functionality
- Description: As a user who forgot their password, I want to reset it via email, so that I can regain account access
- Epic: EP-001
- Requirements: FR-002, UXR-001
- Effort: 4 story points
```

#### Example 2: Large Story Breakdown
**Original Story (8 story points):**
"As an admin, I want to manage the complete user lifecycle, so that I can control platform access"

**Broken Down Stories:**
```
us_XXX: Admin User Creation (3 points)
- As an admin, I want to create new user accounts, so that I can onboard users efficiently

us_XXX: Admin User Role Management (2 points)
- As an admin, I want to assign and modify user roles, so that I can control access permissions

us_XXX: Admin User Deactivation (3 points)
- As an admin, I want to deactivate user accounts, so that I can revoke access when needed
```

### Technical Story Handling

#### New Project Scenarios
**EP-TECH stories are pre-defined in epics.md for green-field projects.**
When generating stories for EP-TECH:
- Reference epics.md for EP-TECH mapped requirements (TR-XXX, DR-XXX)
- Generate foundational technical stories from those requirements
- Cross-reference design.md for NFR-XXX requirements affecting technical setup

**Example EP-TECH derived stories:**
```
us_XXX: Project Structure Setup
- As a developer, I want a properly configured project structure, so that development can begin efficiently
- Derived from: TR-XXX (framework), DR-XXX (database setup)

us_XXX: Development Environment Configuration
- As a developer, I want a standardized development environment, so that all team members can work consistently
- Derived from: TR-XXX (containers), NFR-XXX (development standards)

us_XXX: CI/CD Pipeline Implementation
- As a development team, I want automated build and deployment pipelines, so that releases are reliable and efficient
- Derived from: TR-XXX (CI/CD requirements), NFR-XXX (deployment standards)
```

#### Technology Stack Integration
Based on identified technology stack in epic/specification:
- **Framework Stories**: Setup and configuration for chosen frameworks
- **Database Stories**: Schema design and connection setup
- **Integration Stories**: API setup and third-party service connections
- **Testing Stories**: Test framework setup and initial test suites

### Implementation Instructions

#### Automated Story Generation Flow
```
1. Parse $ARGUMENTS to determine combination type (scope+epic, scope only, epic only, etc.)
2. Extract content based on argument combination:
   - Scope file + Epic ID: Read scope file, filter for specific epic
   - Scope file only: Read scope file, extract all epics
   - Epic ID only: Read spec.md, find specific epic
   - Direct Text: Use text as source
   - No arguments: Read spec.md, extract all epics
3. Parse requirements and business context for identified epic(s)
4. If present, read `spec.md`, `codeanalysis.md`, and `design.md` to extract requirements, system insights, and design guidance.
5. Identify existing us_XXX IDs to determine starting sequence
6. For each epic in scope:
   a. Analyze epic requirements and complexity. For each: (1) What is the core need? (2) What are the constraints?
   b. Decompose into user-focused stories
   c. Estimate effort and apply breakdown rules
   d. Generate story files with sequential IDs
   e. Populate userstory-base.md template
7. Validate all generated stories for completeness
8. Confirm file structure and content accuracy
9. Report epic processing summary (single epic vs. multiple epics)
```
**Command Execution Flow:**
```
1. Process $ARGUMENTS
2. Extract epic content
3. Generate user stories
4. Write story files
   v
5. MANDATORY EVALUATION CHECKPOINT
   a. Read all generated stories
   b. Calculate all 8 metrics
   c. Generate evaluation report
   d. Output report to user
   e. Provide go/no-go recommendation
   v
6. Complete execution with evaluation results
```

#### Error Handling and Recovery
**Common Error Scenarios:**
- **Missing Source**: Provide clear guidance on valid input options
- **Invalid Epic ID**: List available epics from spec.md
- **Inaccessible URL**: Request alternative source or local file
- **Empty/Invalid Content**: Ask for clarification or additional detail
- **Template Errors**: Validate against userstory-base.md and regenerate

#### Output Confirmation
After successful story generation:
1. **Epic Processing Summary**: Report which epic(s) were processed (specific epic vs. all epics in scope)
2. **Story Generation Report**: List all generated stories with IDs, titles, and parent epic mapping
3. **File Confirmation**: Confirm all directories and files created successfully
4. **Validation Results**: Report on quality validation outcomes
5. **Next Steps**: Suggest follow-up actions (task generation, estimation refinement, etc.)

**Example Output Messages:**
- "Generated 5 user stories for Epic EP-001 from scope_document.md"
- "Generated 23 user stories for 4 epics from scope_document.md"
- "Generated 8 user stories for Epic EP-002 from .propel/context/docs/spec.md"
- "Generated 45 user stories for all 7 epics from .propel/context/docs/spec.md"

### Quality Assurance Framework

#### Story Validation Checklist
Before completing story generation, validate each story:

**Structure Validation:**
- [ ] Follows userstory-base.md template exactly
- [ ] Contains all required sections (ID, Title, Description, Acceptance Criteria, Edge Cases, Traceability, Tags)
- [ ] Uses proper formatting and structure

**Content Validation:**
- [ ] Description follows "As a... I want... so that..." format
- [ ] Acceptance criteria use Given/When/Then format
- [ ] Edge cases cover boundary conditions and error scenarios
- [ ] Traceability maps to correct parent epic
- [ ] Tags are appropriate and complete

**Sizing Validation:**
- [ ] Story effort <= 5 story points (40 hours)
- [ ] Large stories properly broken down
- [ ] Each story delivers independent business value
- [ ] Stories are testable independently

**Business Value Validation:**
- [ ] Clear business value articulated in description
- [ ] User perspective properly represented
- [ ] Functionality scope is focused and achievable
- [ ] Acceptance criteria are specific and measurable

#### File Organization Validation
After generating all stories:
- [ ] All directories created properly (.propel/context/tasks/us_XXX/)
- [ ] All story files created with correct names (us_XXX.md)
- [ ] Sequential ID numbering is correct and continuous
- [ ] No duplicate IDs or missing sequences
- [ ] File structure matches template requirements

List of the applicable of rules
- [ ] `rules/ai-assistant-usage-policy.md`: Prioritize explicit user commands; minimal, surgical output only.
- [ ] `rules/code-anti-patterns.md`: Detect/avoid god objects, circular dependencies, magic constants, silent error swallowing.
- [ ] `rules/dry-principle-guidelines.md`: Enforce single source of truth; apply delta-only updates; prevent redundant regeneration.
- [ ] `rules/iterative-development-guide.md`: Follow strict phased workflow; no phase merging; no unsolicited narration.
- [ ] `rules/language-agnostic-standards.md`: Apply KISS, YAGNI; enforce size limits, clear naming, robust error handling, deterministic tests.
- [ ] `rules/markdown-styleguide.md`: Conform front matter, heading hierarchy, list syntax, code fence formatting.
- `rules/performance-best-practices.md`: Optimize only after measurement; cover frontend, backend, database hotspots.
- [ ] `rules/security-standards-owasp.md`: Align with OWASP Top 10 (access control, crypto, injection, config, components, SSRF, etc.).
- [ ] `rules/software-architecture-patterns.md`: Apply pattern selection matrix; define boundaries; event flow clarity; CQRS/microservices guidance.

**>>> MANDATORY: Execute Quality Evaluation and Detection Rules. IF any gate fails, execute Self-Healing. <<<**

**Execution Steps:**
1. Score each dimension in the Quality Assessment table below
2. Apply Detection Rules to validate scores and calculate penalties
3. IF any MUST gate failed OR any score is below its threshold: Execute Self-Healing Protocol (Retry Flow)
4. Print the completed evaluation table with final scores
5. Print the Overall Score and Evaluation Summary

## Quality Evaluation

Once the output is generated, Score the generated user story to evaluate its quality against the following metrics, providing a percentage score (1-100%) for each.

### User Story Quality Assessment

| # | Evaluation Dimension | Score | Gate | Assessment Criteria |
|---|---------------------|-------|------|---------------------|
| 1 | Template Adherence | [0-100%] | MUST=100% | Strict conformance to user-story-template.md; no extra content |
| 2 | Single Story File | [PASS/FAIL] | MUST PASS | One story per file; unified file structure |
| 3 | Requirements Coverage | [0-100%] | >=80% | All requirements mapped; no orphans |
| 4 | Story Breakdown | [0-100%] | >=80% | <=5 story points; INVEST compliance |
| 5 | Acceptance Criteria Quality | [0-100%] | >=80% | Given/When/Then format; measurable criteria |
| 6 | Epic Coverage | [0-100%] | >=80% | All epics from epics.md have stories generated |

### Detection Rules

| # | Detection Logic | Penalty |
|---|-----------------|---------|
| 1 | Compare story sections vs user-story-template.md | Missing: -20%/section; Extra: -15%/section |
| 2 | Count files per story; verify us_XXX.md naming | Multiple files: BLOCKED; Bad naming: -10% |
| 3 | Cross-reference FR/NFR/TR/DR IDs in story vs epics.md | Orphan requirement: -15%/instance; Missing mapping: -10% |
| 4 | Calculate story points; verify <=5; check INVEST criteria | >5 points: -20%/story; INVEST violation: -10%/criterion |
| 5 | Parse acceptance criteria for Given/When/Then; check measurability | Missing format: -10%/criterion; Vague criteria: -10%/instance |
| 6 | Count epics in epics.md; verify each has >=1 story | Uncovered epic: -15%/epic |

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
| Requirements Coverage | Add missing requirement mappings |
| Story Breakdown | Split oversized stories |
| Acceptance Criteria Quality | Reformat to Given/When/Then |
| Epic Coverage | Generate stories for uncovered epics |

**Non-Patchable (Skip):** Single Story File (structural), source missing, hallucinated content, patch >30% of section, >=3 failing dimensions

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

*This user story generator ensures comprehensive, business-aligned user stories with proper effort estimation, clear acceptance criteria, complete traceability, and MANDATORY quality evaluation for successful agile development.*