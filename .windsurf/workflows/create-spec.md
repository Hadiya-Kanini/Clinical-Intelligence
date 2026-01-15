---
description: Generates functional requirements from feature specifications or direct text input. Creates spec.md with FR-XXX requirements and Use Case analysis with PlantUML diagrams.
auto_execution_mode: 1
---

# Spec Generator
As an expert Business Analyst and Product Manager, generate Functional Requirements (FR-XXX) and Use Case specifications. This workflow focuses on business requirements and user-facing functionality.

## Input Parameter (Project Scope File): $ARGUMENTS (Mandatory)
**Accepts:** Feature specifications | Business requirements | Project scope documents | User needs text
**Supported File Types:** .pdf | .txt | .md | .docx

### Input Processing Instructions
**CRITICAL**: Before proceeding with requirements generation, you MUST determine input type and process accordingly:

#### Input Type Detection
1. **File Path Detection**: Check if `$ARGUMENTS` contains a file path (contains file extensions .pdf, .txt, .md, .docx)
2. **Direct Text Detection**: If `$ARGUMENTS` doesn't contain a file extension, treat it as direct specification text

#### Input Type Handling

This unified command automatically processes various requirement sources:

**Feature Specifications**
- **Source**: Feature request documents or descriptions
- **Focus**: New capability addition with business justification
- **Output**: Comprehensive spec with implementation roadmap

**Business Requirements**
- **Source**: Business case documents or strategic initiatives
- **Focus**: Business-driven functionality with ROI analysis
- **Output**: Business-aligned spec with success metrics

**Project Scope**
- **Source**: Project charter or scope documents
- **Focus**: Complete project requirements with phases
- **Output**: Phased spec with milestone definitions

**User Needs Analysis**
- **Source**: User research, feedback, or journey maps
- **Focus**: User-centric requirements with experience priorities
- **Output**: User-focused spec with usability criteria

#### File Input Processing
If `$ARGUMENTS` is a file path:
1. Verify file exists and read contents (supports .pdf, .txt, .md, .docx)
2. Validate content is readable and contains relevant specification information

#### Direct Text Processing
If `$ARGUMENTS` is direct text: Use it directly as source material for requirements generation.

#### Design Asset Processing (UI Impact Only)
If requirements involve UI changes, extract design references:
- Figma links (figma.com/file/, figma.com/proto/)
- Image assets (.png, .jpg, .svg, .sketch) - copy to `.propel/context/design/`
- Design tokens (colors, typography, spacing)

#### Fallback Handling
- If file cannot be read: Request alternative path or paste content directly
- If no input provided: Request file path or specification text

## Output
- Artifact generation:
  - `.propel/context/docs/spec.md`
  - Print the following:
    - List of rules used by the workflow in bulleted format
    - Evaluation Scores in tabular format with average score.
    - Evaluation summary (less than 100 words).
    **Do not save as file.** (console output only)

**Note:**
- If the output file is already available make the necessary changes to applicable sections. Do not overwrite the whole file.
- Always create the output file in manageable smaller chunks to manage memory and processing constraints.
- Always generate a single unified document using `.propel/templates/requirements-template.md` template for spec
- spec.md contains: Goal, Why, What, FR-XXX, Use Cases with PlantUML, Risks & Mitigations, Constraints & Assumptions
- UXR-XXX (UX Requirements) are NOT generated here - they are generated in create-figma-spec.md only when UI impact exists

## Execution Flow

### Core Principles
- **FIRST**: Process `$ARGUMENTS` input according to Input Processing Instructions above
- **SECOND**: Extract and analyze the specification content. For hidden requirements, identify: (a) Implicit user expectations (b) Assumed system behaviors (c) Integration touchpoints
- **THIRD**: Analyze project scope and business context before requirements generation
- Review existing codebase (if available) to understand current state and constraints
- For each identified requirement, answer: (1) Who needs this? (2) What triggers it? (3) What is the success outcome? (4) What are the failure scenarios?
- Update existing sections incrementally when file exists; avoid complete overwrites
- Split complex requirements by functional areas when applicable
- Ensure requirements are testable, measurable, and aligned with business objectives
- Focus on Functional Requirements (FR-XXX)
- Generate a single unified document at .propel/context/docs/spec.md only

#### Content Processing Workflow
1. **Input Analysis**: Determine if `$ARGUMENTS` is file path or direct text
2. **Content Extraction**: Read file content OR use direct text as source material
3. **Content Parsing**: Extract key business requirements, user needs, and technical constraints
4. **Context Integration**: Combine extracted content with codebase analysis and business context
5. **Requirements Generation**: Create comprehensive spec based on processed specification content

### Deep Requirements Analysis Methodology (use Sequential-Thinking MCP)

Optimize for requirements completeness and implementation success over speed.

**Fallback Mechanism:** If the sequential-thinking MCP tool fails or is unavailable, automatically fall back to standard iterative analysis approach using Web fetch:
- Perform systematic step-by-step requirement analysis
- Use structured thinking with explicit validation checkpoints
- Apply the same comprehensive methodology without the sequential-thinking tool
- Ensure no degradation in analysis quality or completeness

#### 1. Business Context Analysis
For business context, document for each stakeholder: Role, Primary goal, Key constraint. Then proceed:
- **Stakeholder Identification**: Map all stakeholders and their requirements priorities
- **Business Objectives**: Align features with strategic business goals and KPIs
- **User Journey Mapping**: Document end-to-end user flows and interaction points
- **Success Metrics**: Define measurable success criteria and acceptance standards
- **Risk Assessment**: Identify business risks and mitigation strategies

#### 2. Technical Feasibility Assessment
- **Integration Requirements**: Identify system dependencies and integration points
- **Performance Implications**: Analyze scalability and performance requirements
- **Security Considerations**: Document security requirements and compliance needs

#### 3. Design and User Experience Analysis (UI Impact Only)
**Apply only if requirements include user interface changes:**
- **UI Impact Assessment**: Clearly identify which features require UI modifications
- **Visual Design Requirements**: Extract design specifications from wireframes/*
- **Design System Mapping**: Document colors, typography, spacing, components (UI only)
- **UI/UX Patterns**: Identify interaction patterns and micro-animations
- **Responsive Design**: Document breakpoints and adaptive behaviors
- **Accessibility Standards**: WCAG compliance requirements from designs

#### 4. Existing System Analysis (If Applicable)
- **Current State Documentation**: Map existing features and functionality
- **Gap Analysis**: Identify differences between current and desired states
- **Impact Assessment**: Analyze effects on existing features and workflows
- **Migration Requirements**: Document data migration and transition needs
- **Backward Compatibility**: Ensure existing functionality preservation

#### 5. External Research and Standards
- **Industry Best Practices**: Evaluate exemplar open source projects that implement these practices
- **Regulatory Compliance**: Perform targeted web searches to identify applicable regulations and standards
- **Competitive Analysis**: Reference style guides and standards from recognized organizations
- **User Research**: Incorporate user feedback and usability studies.
- **Community Insights**: Perform targeted web searches for up-to-date guides and community insights
- **Evaluate Information**: Consider the recency of information (prefer current practices over outdated ones). Note when practices are controversial or have multiple valid approaches

#### 6. Synthesize Findings
- Organize discoveries into clear categories (e.g., "Must Have", "Recommended", "Optional"). 
- Provide specific examples from real projects when possible
- Explain the reasoning behind each best practice
- Highlight any technology-specific or domain-specific considerations
- Provide links to authoritative sources for deeper exploration

### Essential Project Intelligence

#### Reference Materials Integration
- **Existing Codebase**: Analyze `app`, `client`, `backend`, `server`, custom folders for current implementation patterns
- **Documentation Standards**: Follow existing documentation patterns and conventions

*** Comprehensive understanding of business context and technical constraints is non-negotiable ***

#### References Package
```yaml
- url: [Industry standards documentation]
  why: [Compliance requirements and best practices]
  
- file: [existing/feature/path]
  why: [Current implementation to maintain compatibility]
  
- doc: [Framework/library documentation URL]
  section: [Architecture constraints and capabilities]
  critical: [Key limitations affecting requirements]

- stakeholder: [Stakeholder interview notes/feedback]
  priority: [Critical requirements from key stakeholders]
```

### Spec Generation Framework

#### Critical Context Integration

**Business Context**
- Stakeholder requirements and priorities
- Business objectives and success metrics
- User personas and journey maps
- Market analysis and competitive positioning

**Technical Context**
- Integration points and API requirements
- Performance and scalability considerations

**Requirements Specification**
- Detailed functional requirements with acceptance criteria (FR-XXX)
- Use Case specifications with PlantUML diagrams (UC-XXX)

**Design Context (UI Impact Only)**
- Visual references: Figma URLs OR design images (PNG, JPG, SVG, Sketch files)
- Design system tokens (colors, typography, spacing) for UI components
- Component specifications with visual asset references (Figma frames OR screenshots)
- Interaction patterns and animation requirements
- Responsive design breakpoints and behaviors

**Constraints and Dependencies**
- Technical limitations and workarounds
- External system dependencies
- Regulatory and compliance requirements
- Timeline and resource constraints

**Implementation Considerations**
- Development approach and methodology
- Testing strategy and validation criteria
- Deployment and rollout planning
- Maintenance and support requirements

#### Requirements Structure Architecture

**Before writing FR-XXX requirements, list all requirements to generate:**
| FR-ID | Summary |
|-------|---------|
| ... | ... |
**Now expand each FR listed above with full specification.**

**Functional Requirements (FR-XXX)**
- Feature specifications with detailed behaviors
- Business rules and logic documentation
- System capabilities and constraints
- Clear acceptance criteria for each requirement

**Use Case Analysis (UC-XXX)**
- Actor identification and system boundaries
- Success scenarios and alternative flows
- PlantUML diagrams for each use case
- Preconditions and postconditions

### Stakeholder Analysis Framework

#### Stakeholder Mapping
- **Primary Stakeholders**: Direct users and beneficiaries
- **Secondary Stakeholders**: Indirect users and support teams
- **Technical Stakeholders**: Development and operations teams
- **Business Stakeholders**: Management and decision makers

#### Requirements Prioritization
- **MoSCoW Method**: Must have, Should have, Could have, Won't have
- **Value vs Effort Matrix**: High value/low effort items prioritized
- **Risk Assessment**: Critical path and high-risk items identified
- **Dependencies Mapping**: Sequential requirements ordering

### Workflow Separation

**This Command Generates**: Requirements -> Use Cases
- Functional Requirements (FR-XXX)
- Use Case Analysis with PlantUML diagrams (UC-XXX)

**spec.md Document Structure** (per requirements-template.md):
- Goal statement with current vs desired state
- Why section (business value, integration, problems solved)
- What section (user-visible behavior, success criteria)
- Functional Requirements (FR-XXX) with MUST statements
- Use Case Analysis:
  - Actors & System Boundary
  - Use Case Specifications (UC-XXX) with PlantUML diagrams
- Risks & Mitigations (top 5, scoped to FR)
- Constraints & Assumptions (top 5, scoped to FR)

### Quality Assurance Framework

#### Pre-Delivery Checklist
- [ ] **Business Alignment**: Requirements align with business objectives and KPIs
- [ ] **Stakeholder Coverage**: All stakeholder needs identified and addressed
- [ ] **Testability**: All requirements have clear acceptance criteria
- [ ] **FR Completeness**: Functional requirements (FR-XXX) comprehensive
- [ ] **Clarity**: Requirements are unambiguous and well-documented
- [ ] **Traceability**: Requirements linked to business objectives and user needs
- [ ] **Risk Assessment**: Potential risks identified with mitigation strategies
- [ ] **Use Case Diagrams**: Each use case (UC-XXX) has corresponding PlantUML diagram

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
- `rules/ui-ux-design-standards.md`: Hierarchy, accessibility, consistency
- `rules/web-accessibility-standards.md`: WCAG 2.2 AA compliance
- `rules/uml-text-code-standards.md`: PlantUML/Mermaid notation standards

**>>> MANDATORY: Execute Quality Evaluation and Detection Rules. IF any gate fails, execute Self-Healing. <<<**

**Execution Steps:**
1. Score each dimension in the Quality Assessment table below
2. Apply Detection Rules to validate scores and calculate penalties
3. IF any MUST gate failed OR any score is below its threshold: Execute Self-Healing Protocol (Retry Flow)
4. Print the completed evaluation table with final scores
5. Print the Overall Score and Evaluation Summary

## Quality Evaluation

Score the specification quality against the following metrics:

### Specification Quality Assessment

| # | Evaluation Dimension | Score | Gate | Assessment Criteria |
|---|---------------------|-------|------|---------------------|
| 1 | Template Adherence | [0-100%] | MUST=100% | All sections from requirements-template.md present; no extra sections; no UXR section (moved to figma_spec.md) |
| 2 | FR Completeness | [0-100%] | >=80% | Every feature has FR-XXX; MUST/SHOULD statements measurable |
| 3 | Use Case Coverage | [0-100%] | >=80% | Each UC-XXX has PlantUML diagram; actors and flows defined |
| 4 | Testability | [0-100%] | >=80% | Acceptance criteria specific, measurable, and verifiable |
| 5 | Traceability | [0-100%] | >=80% | Requirements link to business objectives; IDs consistent |
| 6 | Ambiguity Handling | [0-100%] | >=80% | Unclear requirements tagged with [UNCLEAR]; no assumptions made; clarifications documented |

### Detection Rules

| # | Detection Logic | Penalty |
|---|-----------------|---------|
| 1 | Compare sections vs requirements-template.md | Missing: -20%; Extra: -15% |
| 2 | Count FR-XXX; verify MUST/SHOULD + measurable criteria | (covered/total) x 50% + (measurable/FR_count) x 50% |
| 3 | UC-XXX: PlantUML + Actor + Success Scenario | (diagram/total) x 40% + (actor/total) x 30% + (flow/total) x 30% |
| 4 | Acceptance criteria: numeric values, verifiable actions | Vague terms: -10%/instance |
| 5 | FR-XXX links to "Why" section; ID format FR-\d{3} | Orphan: -10%; Bad ID: -5% |
| 6 | [UNCLEAR] tags; untagged assumptions | Untagged: -15%; Hallucinated: -20% |

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
| FR Completeness | Add missing FR-XXX to existing section |
| Use Case Coverage | Insert PlantUML block for missing UC |
| Testability | Replace vague phrases with metrics |
| Traceability | Add FR-XXX cross-references |
| Ambiguity Handling | Insert [UNCLEAR] tags at flagged items |

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

*This spec generator ensures comprehensive, business-aligned requirements with FR-XXX and Use Case (UC-XXX) documentation for successful downstream workflows.*