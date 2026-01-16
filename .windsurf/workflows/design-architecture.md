---
description: Generates comprehensive design documents including NFR, TR, DR requirements, architecture patterns, technology stack validation, and cross-cutting concerns.
auto_execution_mode: 1
---

# Design Architecture Generator

As an expert Software Architect, generate comprehensive design documents including Non-Functional Requirements (NFR-XXX), Technical Requirements (TR-XXX), and Data Requirements (DR-XXX). This workflow focuses on architectural decisions, technology stack validation, and cross-cutting concerns.

## Input Parameter (Feature file): $ARGUMENTS (Optional)
Read the feature file first to understand what needs to be created, how the examples provided help, and any other considerations.

**Note:** Refer to '.propel/context/docs/spec.md' or '.propel/context/docs/codeanalysis.md' file if argument is not passed

### Input Processing Instructions
**CRITICAL**: Before proceeding with design generation, you MUST determine input type and process accordingly:

#### Input Type Detection
1. **File Path Detection**: Check if `$ARGUMENTS` contains a file path (contains file extensions .md, .txt)
2. **Default Fallback**: If `$ARGUMENTS` is empty, refer to `.propel/context/docs/spec.md` or `.propel/context/docs/codeanalysis.md`

#### File Input Processing
If `$ARGUMENTS` is a file path:
1. Verify file exists and read contents (supports .md, .txt)
2. Validate content contains relevant requirements

#### Default Input Processing
If `$ARGUMENTS` is not provided:
1. Look for `.propel/context/docs/spec.md` first
2. Fallback to `.propel/context/docs/codeanalysis.md` if spec.md doesn't exist

#### Fallback Handling
- If file cannot be read: Request alternative file path
- If no requirements found: Request specification file or text

## Output
- Artifact generation: `.propel/context/docs/design.md`
- Print the following:
  - List of rules used by the workflow in bulleted format
  - List of unclear technical understanding in bulleted format
  - Evaluation Scores in tabular format with average score.
  - Evaluation summary (less than 100 words).
  **Do not save as file.** (console output only)

**Note:**
- If the output file is already available make the necessary changes to applicable sections. Do not overwrite the whole file.
- Always create the output file in manageable smaller chunks to manage memory and processing constraints.
- Always generate a single unified document.
- Generate the output using the `.propel/templates/design-specification-template.md` template.
- design.md contains: Project Overview, Architecture Goals, Technology Stack (with validation and decision matrix), NFR-XXX, TR-XXX, DR-XXX, Core Entities, Technical Constraints, Development Workflow

## Essential Project Intelligence

### Reference Materials Integration
- **Existing Codebase**: Analyze `app`, `client`, `backend`, `server`, custom folders for current implementation patterns
- **Documentation Standards**: Follow existing documentation patterns and conventions
- **Design Specifications**: Reference `.propel/context/docs/spec.md` for requirements alignment

*** Comprehensive understanding of requirements and technical constraints is non-negotiable ***

### References Package
```yaml
- url: [Framework/library documentation URL]
  section: [Architecture constraints and capabilities]
  critical: [Key limitations affecting design]

- file: [existing/feature/path]
  why: [Current implementation to maintain compatibility]
```

## Execution Flow

### 1. Core Principles
- Understanding the requirements, is mandatory
- Understanding the existing codebase, if available, is mandatory.
- Analyze deeply and systematically plan your approach

### 2. Research Process
During the research process, deeper research we do here the better the design will be. we optimize for chance of success and not for speed.

1. **Codebase Analysis** (if provided)
   - Search for similar features/patterns in the codebase 
   - Identify the existing features and understand what changes are required to ensure existing features work as-is
   - Identify files to reference in design
   - Note existing conventions to follow
   - Check test patterns for validation approach

2. **External Research (via Sequential-thinking MCP)**

   **Fallback Mechanism:** If the sequential-thinking MCP tool fails or is unavailable, automatically fall back to standard iterative analysis approach using Web fetch:
   - Perform systematic step-by-step design analysis
   - Use structured thinking with explicit validation checkpoints
   - Apply the same comprehensive methodology without the sequential-thinking tool
   - Ensure no degradation in analysis quality or completeness

   **Research Areas:**
   - Use Context7 MCP to pull official docs (GitHub, frameworks, libraries)
   - Search the web for recent articles, guides, and community discussions
   - Identify and analyze well-regarded open source projects that demonstrate the practices
   - Look for style guides, conventions, and standards from respected organizations
   - Prioritize official documentation and widely-adopted standards
   - Consider the recency of information (prefer current practices over outdated ones)
   - Look for popular repositories on GitHub that exemplify good practices

3. **Requirements Generation** (Moved from create-spec workflow)

   Generate the following requirement types that inform architectural decisions:

   **Before writing requirements, list all NFR/TR/DR to generate:**
   | Req-ID | Type | Summary |
   |--------|------|---------|
   | NFR-001 | Performance | ... |
   | TR-001 | Technology | ... |
   | DR-001 | Data | ... |
   **Now expand each requirement listed above.**

   **Non-Functional Requirements (NFR-XXX)**
   Generate requirements for:
   - **Performance**: Response times, throughput, resource usage (NFR-001, NFR-002...)
   - **Security**: Encryption, authentication, authorization, data protection
   - **Availability**: Uptime targets, disaster recovery, failover
   - **Scalability**: Concurrent users, data volume, growth projections
   - **Reliability**: Error rates, mean time between failures
   - **Maintainability**: Code quality, documentation, upgrade paths

   **Technical Requirements (TR-XXX)**
   Generate requirements for:
   - **Technology Choices**: Database, frameworks, runtime environments (TR-001, TR-002...)
   - **Architecture Patterns**: RESTful APIs, microservices, event-driven
   - **Platform Requirements**: Containerization, cloud deployment, on-premise
   - **Integration Requirements**: OAuth, third-party APIs, message queues
   - **Development Standards**: Code style, testing frameworks, CI/CD

   **Data Requirements (DR-XXX)**
   Generate requirements for:
   - **Data Structures**: Entity definitions, relationships, unique identifiers (DR-001, DR-002...)
   - **Data Integrity**: Referential integrity, validation rules, constraints
   - **Data Retention**: Audit logs, archival policies, compliance periods
   - **Data Backup**: Backup frequency, point-in-time recovery, disaster recovery
   - **Data Migration**: Schema versioning, zero-downtime migrations

   **Requirement Format:**
   ```markdown
   - NFR-001: System MUST [performance requirement, e.g., "respond to user requests within 2 seconds"]
   - TR-001: System MUST [technology choice, e.g., "use PostgreSQL 14+ as the primary database"]
   - DR-001: System MUST [data structure, e.g., "store user profiles with email as unique identifier"]
   ```

   **Note:** Mark unclear requirements with [UNCLEAR] tag for later clarification.

4. **User Clarification** (if needed)
   - Specific patterns to mirror and where to find them?
   - Integration requirements and where to find them?

### 3. Technical Design Considerations
- Architectural patterns to follow
- Technology stack considerations
   - Multi-Tier Requirements
      - **Frontend Requirements**: UI/UX, client-side functionality, responsive design
      - **Backend Requirements**: Business logic, API design, data processing
      - **Database Requirements**: Data models, queries, performance optimization
      - **Infrastructure Requirements**: Deployment, monitoring, scaling strategies
   - Cross-Cutting Concerns
      - **Security Requirements**: Authentication, authorization, data protection
      - **Performance Requirements**: Response times, throughput, resource usage
      - **Monitoring Requirements**: Logging, metrics, alerting, observability
      - **Compliance Requirements**: Regulatory standards, audit trails, data governance

### 4. Design Document Structure
**Document in design.md** (per design-specification-template.md):
- Project Overview with purpose, target users, capabilities
- Architecture Goals with numbered descriptions
- Technology Stack in tabular format with versions
- Alternate Technology Stack considerations
- Technology Stack Validation against project scope
- Decision Matrix comparing primary vs alternate stack (5 metrics)
- Non-Functional Requirements (NFR-XXX)
- Technical Requirements (TR-XXX)
- Data Requirements (DR-XXX)
- Core Entities (what they represent, key attributes, relationships)
- Technical Constraints and Assumptions
- Development Workflow steps

### 5. Design Generation
- Read template from `.propel/templates/design-specification-template.md`
- Populate template with the findings
- Use Write tool to create an artifact `.propel/context/docs/design.md`
- Ensure all template sections are populated with real data

### 6. Summary Presentation
- Present executive summary to user
- Highlight critical findings and recommendations
- Provide link to detailed report in `.propel/context/docs/design.md`
- Present the Quality Assessment metrics

**Design Validation (use sequential thinking MCP if available):**
- Validate research completeness before design generation
- Create and verify design hypothesis
- Ensure architectural decisions align with requirements
- **Fallback**: Create explicit validation checklist and document decision rationale

### Critical Context to Include
- **Documentation**: URLs with specific sections
- **Design**: Architecture and design considerations
- **Code Examples**: Real snippets from codebase
- **Guidelines**: Library quirks, version issues
- **Patterns**: Existing approaches to follow

### Implementation Blueprint
- Start with pseudocode showing approach
- Reference real files for patterns
- Include error handling strategy

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

Score the design quality against the following metrics:

### Design Quality Assessment

| # | Evaluation Dimension | Score | Gate | Assessment Criteria |
|---|---------------------|-------|------|---------------------|
| 1 | Template Adherence | [0-100%] | MUST=100% | All sections from design-specification-template.md present; no extra sections |
| 2 | NFR Coverage | [0-100%] | >=80% | All quality attributes have NFR-XXX; measurable acceptance criteria |
| 3 | TR Coverage | [0-100%] | >=80% | All technical constraints have TR-XXX; technology decisions justified |
| 4 | DR Coverage | [0-100%] | >=80% | All data entities have DR-XXX; schemas and relationships defined |
| 5 | Tech Stack Validation | [0-100%] | >=80% | Decision matrix present; alternatives evaluated with rationale |
| 6 | Spec Alignment | [0-100%] | >=80% | NFR/TR/DR trace to FR/UXR from spec.md; no orphaned requirements |
| 7 | Ambiguity Handling | [0-100%] | >=80% | Unclear technical decisions tagged with [UNCLEAR]; assumptions explicitly documented |

### Detection Rules

| # | Detection Logic | Penalty |
|---|-----------------|---------|
| 1 | Compare sections vs design-specification-template.md | Missing: -20%; Extra: -15% |
| 2 | Count NFR-XXX; verify measurable acceptance criteria | (covered_attributes/total) x 50% + (measurable/NFR_count) x 50% |
| 3 | Count TR-XXX; verify technology justification present | Missing justification: -15%/instance |
| 4 | Count DR-XXX; verify schema + relationships defined | Missing schema: -15%; Missing relationships: -10% |
| 5 | Decision matrix present; >=2 alternatives evaluated | No matrix: -30%; <2 alternatives: -15% |
| 6 | Cross-reference NFR/TR/DR to spec.md FR-XXX | Orphan requirement: -10%/instance |
| 7 | [UNCLEAR] tags; untagged technical assumptions | Untagged: -15%; Hallucinated: -20% |

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
| NFR Coverage | Add missing NFR-XXX to existing section |
| TR Coverage | Add missing TR-XXX with justification |
| DR Coverage | Insert schema/relationships for missing DR |
| Tech Stack Validation | Add decision matrix or alternatives |
| Spec Alignment | Add cross-references to FR-XXX |
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

*This design architecture generator ensures comprehensive NFR/TR/DR requirements with technology stack validation and architectural decisions for successful implementation.*