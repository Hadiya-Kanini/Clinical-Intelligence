---
description: Generates epic decomposition from spec.md/codeanalysis.md, design.md, and models.md. Creates epics.md with EP-XXX epics, requirement mappings, priorities, and business value. Includes EP-TECH for green-field projects.
auto_execution_mode: 1
---

# Epic Generator

As an expert Product Manager and Business Analyst, generate comprehensive epic decomposition that maps all requirements to actionable development epics. This workflow creates epics.md by analyzing functional, non-functional, technical, data, and UX requirements from multiple source documents.

## Input Parameter: $ARGUMENTS (Optional)
**Accepts:** File path to requirements document | Free text epic scope
**Supported File Types:** .md | .txt | free text

### Input Processing Instructions
**CRITICAL**: Before proceeding with epic generation, you MUST determine input type and process accordingly:

#### Input Type Detection
1. **File Path Detection**: Check if `$ARGUMENTS` contains a file path (contains file extensions .md, .txt)
2. **Free Text Detection**: If `$ARGUMENTS` is text without file extension, treat as epic scope description
3. **Default Fallback**: If `$ARGUMENTS` is empty, use standard input files

#### File Input Processing
If `$ARGUMENTS` is a file path:
1. **File Existence Check**: Verify the file exists using appropriate tools
2. **Read File Contents**: Extract content from the provided file
3. **Content Validation**: Ensure file contains requirements for epic mapping

#### Free Text Processing
If `$ARGUMENTS` is free text:
1. **Text Validation**: Ensure text describes epic scope or feature set
2. **Context Integration**: Combine with default input files if available
3. **Epic Generation**: Create epics based on text scope

#### Default Input Processing
If `$ARGUMENTS` is not provided:
1. **Read spec.md**: Extract FR-XXX requirements and use cases (UXR-XXX NOT in spec.md - generated in create-figma-spec.md)
2. **Read codeanalysis.md**: Alternative to spec.md for brown-field projects
3. **Read design.md**: Extract NFR-XXX, TR-XXX, DR-XXX requirements
4. **Read models.md**: Reference for architectural context (optional) especially sequence diagrams for each of the use case
5. **Content Validation**: Ensure sufficient requirements exist for epic generation

#### Fallback Handling
- If spec.md/codeanalysis.md cannot be read: Request user to provide requirements source
- If design.md doesn't exist: Proceed with FR only, note missing technical requirements
- If no requirements found: Request user to run create-spec and design-architecture workflows first
- Note: UXR-XXX requirements are generated in create-figma-spec.md workflow, not in spec.md

## Output
- Artifact generation: `.propel/context/docs/epics.md`
- Print the following:
  - List of rules used by the workflow in bulleted format
  - Epic summary table (EP-XXX with mapped requirements count)
  - Evaluation Scores in tabular format with average score.
  - Evaluation summary (less than 100 words).
  **Do not save as file.** (console output only)

**Note:**
- If the output file is already available make the necessary changes to applicable sections. Do not overwrite the whole file.
- Always create the output file in manageable smaller chunks to manage memory and processing constraints.
- Always generate a single unified document.
- Generate the output using the `.propel/templates/epics-template.md` template.

## Essential Project Intelligence

### Reference Materials Integration
- **spec.md/codeanalysis.md**: Source for FR, UC requirements (UXR generated in create-figma-spec.md)
- **design.md**: Source for NFR, TR, DR requirements
- **models.md**: Architectural context for epic scoping
- **Existing Codebase**: Project type detection and existing feature analysis

*** Comprehensive requirement mapping with zero orphaned requirements is non-negotiable ***

### References Package
```yaml
- file: .propel/context/docs/spec.md
  why: FR and Use Case requirements (UXR generated in create-figma-spec.md)

- file: .propel/context/docs/codeanalysis.md
  why: Alternative requirements source for brown-field projects

- file: .propel/context/docs/design.md
  why: NFR, TR, DR requirements for technical epics

- file: .propel/context/docs/models.md
  why: Architectural context for epic scoping

- file: .propel/context/docs/figma_spec.md
  why: UXR-XXX requirements (only when UI impact exists)
```

## Core Principles
- **Zero Orphaned Requirements**: Every requirement maps to exactly one epic
- **Business Value Ordering**: Prioritize epics by business impact, then dependency
- **Manageable Scope**: Limit epics to ~12 requirements; split if exceeded

## Execution Flow

### 1. Requirement Analysis
- Detect project type (green-field vs brown-field)
- Read all source documents to extract requirements
- Group requirements into logical epics by business outcome
- Generate epic table with requirement mappings
- Create detailed epic descriptions with priorities
- For green-field projects, always include EP-TECH as first epic

### 2. Project Type Detection

#### Green-field Detection
Check for green-field (new) project indicators:
- No existing codebase in `app`, `client`, `backend`, `server` folders
- No `.propel/context/docs/codeanalysis.md` file
- Requirements indicate new system creation
- No migration or backward compatibility requirements

**If Green-field Detected:**
- Create EP-TECH epic as first epic (highest priority)
- EP-TECH includes: Project scaffolding, CI/CD setup, development environment, base architecture

#### Brown-field Detection
Check for brown-field (existing) project indicators:
- Existing codebase present
- `.propel/context/docs/codeanalysis.md` exists
- Requirements reference existing features
- Migration or integration requirements present

**If Brown-field Detected:**
- Do NOT create EP-TECH epic
- Focus epics on feature enhancement and integration
- Consider backward compatibility in epic priorities

### 3. Research Process

#### Requirements Gathering (use Sequential-Thinking MCP)

**Fallback Mechanism:** If the sequential-thinking MCP tool fails or is unavailable, automatically fall back to standard iterative analysis approach using Web fetch:
- Perform systematic step-by-step requirements analysis
- Use structured thinking with explicit validation checkpoints
- Ensure no degradation in analysis quality or completeness

**From spec.md/codeanalysis.md:**
- Extract all FR-XXX (Functional Requirements)
- Extract all UC-XXX (Use Cases)
- Note any [UNCLEAR] tagged requirements
- Note: UXR-XXX are NOT in spec.md - they are generated in create-figma-spec.md when UI impact exists

**From figma_spec.md (if exists - for UI-impacting epics):**
- Extract all UXR-XXX (UX Requirements) - ONLY available after create-figma-spec.md runs
- Map UXR-XXX to UI-impacting epics

**From design.md:**
- Extract all NFR-XXX (Non-Functional Requirements)
- Extract all TR-XXX (Technical Requirements)
- Extract all DR-XXX (Data Requirements)
- Extract Core Entities for data-centric epics

#### Requirements Analysis
- Total requirements count by category
- Identify requirement dependencies
- Group by functional area/outcome
- Detect cross-cutting concerns (security, performance)

#### Epic Grouping Strategy
**Grouping Principles:**
- Group by business outcome, not technical layer
- Keep related FR together (user-facing features); UXR mapping happens after create-figma-spec.md
- Group NFR + TR for infrastructure/platform epics
- Separate security/compliance into dedicated epic if >8 requirements
- Create data-focused epic for DR requirements if complex
- Note: UXR-XXX mapping to epics occurs AFTER figma_spec.md is generated (for UI-impacting epics only)

### 4. Epic Generation Framework

**Before writing epics, list all epics to generate:**
| EP-ID | Epic Title | Requirement Count |
|-------|------------|-------------------|
| EP-TECH | (if green-field) | ... |
| EP-001 | ... | ... |
| ... | ... | ... |
**Now expand each epic listed above.**

#### EP-TECH Epic (Green-field Only)

**Generate First if Green-field Detected:**
```markdown
| Epic ID | Epic Title | Mapped Requirement IDs |
|---------|------------|------------------------|
| EP-TECH | Project Infrastructure & Scaffolding | TR-001 (framework), TR-002 (API spec), TR-003 (containers), DR-004 (backups) |
```

**EP-TECH Description:**
```markdown
### EP-TECH: Project Infrastructure & Scaffolding
**Priority**: Critical (Blocking)
**Business Value**: Enables all subsequent development by establishing project foundation
**Description**: Set up project scaffolding, development environment, CI/CD pipelines, and base architecture patterns required for feature development.

**Key Deliverables**:
- Project structure and folder organization
- Development environment configuration
- CI/CD pipeline setup
- Base architecture implementation
- Initial database schema and migrations
- Authentication/authorization foundation (if applicable)
```

#### Feature Epics

**Generate Based on Requirements:**
- Map FR + UXR by functional area
- Create one epic per major user outcome
- Limit to ~12 requirements per epic

**Epic Table Format:**
```markdown
| Epic ID | Epic Title | Mapped Requirement IDs |
|---------|------------|------------------------|
| EP-001 | [Action-Oriented Title] | FR-001, FR-002, UXR-001, NFR-002 |
| EP-002 | [Action-Oriented Title] | FR-003, FR-004, UXR-002, TR-001 |
```

#### Epic Descriptions

**For Each Epic Generate:**
```markdown
### EP-XXX: [Epic Title]
**Priority**: [Critical/High/Medium/Low]
**Business Value**: [1-2 sentence business impact]
**Description**: [What this epic delivers and why it matters]

**Key Deliverables**:
- [Specific deliverable 1]
- [Specific deliverable 2]
- [Specific deliverable 3]
```

### 5. Design Generation
- Read template from `.propel/templates/epics-template.md`
- Populate epic table with all requirement mappings
- Generate detailed epic descriptions
- Use Write tool to create artifact `.propel/context/docs/epics.md`
- Ensure all template sections are populated with real data

### 6. Summary Presentation
- Present executive summary to user
- Display epic summary table with requirement counts
- Highlight EP-TECH if green-field project
- Provide link to detailed epics in `.propel/context/docs/epics.md`
- Present the Quality Assessment metrics

**Epic Validation (use sequential thinking MCP if available):**
- Validate all requirements are mapped to exactly one epic
- Ensure no orphaned requirements
- Verify epic sizing (5-12 requirements per epic)
- **Fallback**: Create explicit validation checklist and document decision rationale

### Quality Assurance Framework

#### Pre-Delivery Checklist
- [ ] **Project Type Detected**: Green-field or brown-field correctly identified
- [ ] **EP-TECH Included**: If green-field, EP-TECH is first epic
- [ ] **Requirement Coverage**: All FR, UXR, NFR, TR, DR mapped to exactly one epic
- [ ] **No Orphans**: Zero requirements without epic assignment
- [ ] **No Duplicates**: Each requirement appears in only one epic (unless explicitly shared)
- [ ] **Epic Sizing**: No epic exceeds ~12 requirements
- [ ] **UNCLEAR Handling**: [UNCLEAR] tagged requirements in backlog refinement list
- [ ] **Priority Ordering**: Epics ordered by business value then dependencies
- [ ] **Template Adherence**: Output follows epics-template.md structure

## Guardrails
- `rules/ai-assistant-usage-policy.md`: Explicit commands; minimal output
- `rules/dry-principle-guidelines.md`: Single source of truth; delta updates
- `rules/iterative-development-guide.md`: Strict phased workflow
- `rules/markdown-styleguide.md`: Front matter, heading hierarchy, code fences
- `rules/software-architecture-patterns.md`: Pattern selection, boundaries

**>>> MANDATORY: Execute Quality Evaluation and Detection Rules. IF any gate fails, execute Self-Healing. <<<**

**Execution Steps:**
1. Score each dimension in the Quality Assessment table below
2. Apply Detection Rules to validate scores and calculate penalties
3. IF any MUST gate failed OR any score is below its threshold: Execute Self-Healing Protocol (Retry Flow)
4. Print the completed evaluation table with final scores
5. Print the Overall Score and Evaluation Summary

## Quality Evaluation

Once the output is generated, Score the epics generated to evaluate their quality against the following metrics, providing a percentage score (1-100%) for each.

### Epic Quality Assessment

| # | Evaluation Dimension | Score | Gate | Assessment Criteria |
|---|---------------------|-------|------|---------------------|
| 1 | Template Adherence | [0-100%] | MUST=100% | Output conforms strictly to epics-template.md structure; no extra sections |
| 2 | Requirement Coverage | [0-100%] | >=80% | All FR, NFR, TR, DR requirements mapped to epics (UXR mapped after figma_spec.md) |
| 3 | Epic Sizing | [0-100%] | >=80% | Each epic has <13 requirements; no oversized epics |
| 4 | Business Value Clarity | [0-100%] | >=80% | Each epic has clear, measurable business value statement |
| 5 | Traceability | [0-100%] | >=80% | Each requirement traceable to exactly one epic; no orphans |
| 6 | Project Type Accuracy | [PASS/FAIL] | PASS | Green-field/brown-field correctly detected; EP-TECH included for green-field only |

### Detection Rules

| # | Detection Logic | Penalty |
|---|-----------------|---------|
| 1 | Compare sections vs epics-template.md | Missing: -20%; Extra: -15% |
| 2 | Count FR/NFR/TR/DR mapped to EP-XXX | Unmapped req: -10%/instance |
| 3 | Count requirements per epic; verify <13 | Oversized epic: -15%/epic |
| 4 | Each epic has business value statement | No value: -15%/epic |
| 5 | Each requirement traces to exactly one epic | Orphan: -10%; Multi-mapped: -10% |
| 6 | Green-field has EP-TECH; brown-field does not | Wrong detection: BLOCKED |

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
| Requirement Coverage | Add unmapped requirements to epics |
| Epic Sizing | Split oversized epics |
| Business Value Clarity | Add value statements |
| Traceability | Add requirement tracing |

**Non-Patchable (Skip):** Project Type Accuracy (structural), source missing, hallucinated content, patch >30% of section, >=3 failing dimensions

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

*This epic generator ensures comprehensive requirement-to-epic mapping with proper business value alignment, priority ordering, and complete traceability.*
