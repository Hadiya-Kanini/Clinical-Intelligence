---
description: AI-assisted Figma specification workflow that generates UX Requirements (UXR-XXX), derives screens from requirements, creates screen specifications with screen inventory and flows, and generates design system tokens. Only runs when UI impact is identified.
auto_execution_mode: 1
---

# Create Figma Specification

As a Senior Product Designer and UX Architect, generate UX Requirements (UXR-XXX) and derive comprehensive screen specifications from requirements. Transform use cases and personas into a complete screen inventory with states, flows, and design system tokens. This workflow is the authoritative source for all UXR-XXX requirements.

## Input Parameters

### Primary Input
- `$ARGUMENTS`: Path to spec.md file (optional, defaults to `.propel/context/docs/spec.md`)

### Context Sources (Always Read)
- `.propel/context/docs/spec.md` - **PRIMARY** - Personas, use cases, FR-XXX
- `.propel/context/docs/epics.md` - **REQUIRED** - Epics with UI impact assessment and requirement mappings
- `.propel/context/docs/design.md` - **OPTIONAL** - NFR, TR, DR for technical context
- `.propel/context/wireframes/` - **OPTIONAL** - Wireframe artifacts for entity understanding

### Input Validation
```
1. Check if spec.md exists
2. Check if epics.md exists (required for epic-to-screen mapping)
3. Check epics.md for UI-impacting epics (contains UI tags or frontend/UI related user stories)
4. IF no UI-impacting epics exist -> EXIT with message "No UI impact detected. Figma spec not required."
5. ELSE -> Proceed with UXR-XXX generation and specification generation
```

## Output

### Artifact Generation
- `.propel/context/docs/figma_spec.md` - Screen inventory, states, flows, component mapping
- `.propel/context/docs/designsystem.md` - Design tokens, branding, component specifications

### Console Output (Do Not Save)
- List of rules used by the workflow in bulleted format
- Evaluation Scores in tabular format with average score
- Evaluation summary (less than 100 words)

**Note:**
- If output files already exist, make necessary changes to applicable sections. Do not overwrite whole files.
- Use `.propel/templates/figma-specification-template.md` for figma_spec.md structure
- Use `.propel/templates/design-reference-template.md` for designsystem.md structure

## Core Principles

### Screen Derivation First
- **NEVER** create arbitrary screens - derive ALL screens from use cases
- Each screen must trace back to one or more use cases (UC-XXX)
- Each screen must serve one or more personas
- Cover ALL use cases with appropriate screens

### Persona Coverage
- Every persona in spec.md must have a complete user journey
- Map screens to personas they serve
- Identify shared screens (serve multiple personas) vs persona-specific screens

### Flow Organization
- Organize prototype flows BY FLOW NAME (not by persona)
- Note which personas are covered by each flow
- Flows derived from use case sequences and epic workflows

### State Completeness
- Every screen requires 5 states: Default, Loading, Empty, Error, Validation
- Document triggers for each state
- Plan fallback content for each state

## Execution Flow

### Phase 0: UI Impact Gate
**Critical First Step - Validate UI Impact Exists**

```
1. Read epics.md for EP-XXX epic definitions
2. Check for UI-impacting epics (frontend work, UI changes, screen requirements)
3. IF no UI-impacting epics exist:
   - Output: "No UI impact detected. Skipping Figma specification."
   - EXIT workflow
4. ELSE:
   - List UI-impacting epics
   - Proceed to Phase 0.5 (UXR Generation)
```

### Phase 0.5: UXR-XXX Generation
**Generate UX Requirements based on UI-impacting Use Cases**

This workflow is the AUTHORITATIVE SOURCE for all UXR-XXX requirements.

**Before writing UXR requirements, list all UXR to generate:**
| UXR-ID | Category | Summary |
|--------|----------|---------|
| UXR-101 | Usability | ... |
| UXR-201 | Accessibility | ... |
| ... | ... | ... |
**Now expand each UXR listed above.**

**UXR Derivation Logic:**
```
FOR each UC-XXX with UI impact (from spec.md via epics.md):
  1. USABILITY: Derive navigation requirements (max clicks, discoverability)
  2. ACCESSIBILITY: Apply WCAG 2.2 AA baseline requirements
  3. RESPONSIVENESS: Define breakpoint behavior from platform targets
  4. VISUAL DESIGN: Reference designsystem.md for consistency requirements
  5. INTERACTION: Define feedback requirements (loading, success, error states)
  6. ERROR HANDLING: Derive from UC-XXX alternative paths

FOR each persona:
  1. Identify accessibility needs (e.g., screen reader users)
  2. Add persona-specific UXR requirements
```

**UXR Categories:**
- **Usability** (UXR-1XX): Navigation, discoverability, efficiency (max 3 clicks, clear hierarchy)
- **Accessibility** (UXR-2XX): WCAG 2.2 AA compliance, assistive technology support
- **Responsiveness** (UXR-3XX): Breakpoint behavior, viewport adaptation
- **Visual Design** (UXR-4XX): Design system adherence, consistency
- **Interaction** (UXR-5XX): Feedback, loading states, animations (response within 200ms)
- **Error Handling** (UXR-6XX): Error messages, recovery paths

**UXR Output Format (for figma_spec.md Section 2):**
```markdown
| UXR-ID | Category | Requirement | Acceptance Criteria | Screens Affected |
|--------|----------|-------------|---------------------|------------------|
| UXR-101 | Usability | System MUST provide navigation to any feature in max 3 clicks | Click count audit passes | All screens |
| UXR-201 | Accessibility | System MUST comply with WCAG 2.2 AA standards | WAVE/axe audit passes | All screens |
| UXR-301 | Responsiveness | System MUST adapt to mobile (320px), tablet (768px), desktop (1024px+) | Responsive audit passes | All screens |
```

**UXR Numbering Convention:**
- UXR-001 to UXR-099: Project-wide requirements (generated first)
- UXR-1XX: Usability requirements
- UXR-2XX: Accessibility requirements
- UXR-3XX: Responsiveness requirements
- UXR-4XX: Visual design requirements
- UXR-5XX: Interaction requirements
- UXR-6XX: Error handling requirements

**Handling Unclear UX Requirements:**
- Mark ambiguous UXR with [UNCLEAR] tag when:
  - Accessibility needs conflict with design aesthetics
  - Responsive behavior not defined in source UC-XXX
  - Persona-specific needs unclear from spec.md
  - Visual design intent ambiguous from available references
- Document what clarification is needed
- Do NOT make assumptions about UX intent

### Phase 1: Spec Analysis (Sequential-Thinking MCP)
**MCP Tool:** `mcp__sequential-thinking__sequentialthinking`

**A. Persona Extraction:**
- Read all personas from spec.md
- Document persona goals, tasks, and pain points
- Identify primary vs secondary personas

**B. Use Case Analysis:**
- Extract all use cases (UC-XXX) from spec.md
- Map use case actors to personas
- Identify use case sequences and dependencies
- Note preconditions and postconditions

**C. Epic-to-Screen Mapping:**
- Read epics.md for EP-XXX definitions
- For each epic containing UXR-XXX requirements:
  - Cross-reference spec.md for use case flows
  - Identify required screens from UC-XXX success scenarios
  - Map screens to personas and use cases
  - Note design references (Figma URLs, images) from epic

**Fallback Strategy (if Sequential-Thinking MCP fails):**
Manual structured analysis with explicit extraction phases.

### Phase 2: Screen Derivation
**Goal:** Derive complete screen inventory from use cases

**Screen Derivation Logic:**
```
FOR each Use Case (UC-XXX):
  1. Identify entry point screen
  2. Identify intermediate screens (steps in flow)
  3. Identify success outcome screen
  4. Identify error/alternative screens
  5. Map to persona(s) who execute this use case

CONSOLIDATE:
  - Merge duplicate screens (same screen from multiple use cases)
  - Identify shared vs unique screens
  - Assign screen IDs (SCR-XXX)
```

**UXR-XXX Requirement Mapping:**
```
FOR each UXR-XXX in spec.md:
  1. Identify which screen(s) address this UX requirement
  2. Document UXR-XXX ID in Screen Inventory table
  3. Flag any UXR-XXX without screen coverage

VALIDATION: All UXR-XXX must map to at least one screen.
IF orphan UXR-XXX found:
  - Create additional screen(s) to address the requirement, OR
  - Document why the UXR-XXX is not applicable to screens
```

**Screen Inventory Table Format:**
```markdown
| Screen ID | Screen Name | Derived From UC | UXR-XXX Mapped | Personas Covered | Priority | States Required |
|-----------|-------------|-----------------|----------------|------------------|----------|-----------------|
| SCR-001 | Login | UC-001 | UXR-001, UXR-002 | All | P0 | 5 |
| SCR-002 | Dashboard | UC-002, UC-003 | UXR-003 | Admin, User | P0 | 5 |
| SCR-003 | Profile | UC-004 | UXR-004, UXR-005 | User | P1 | 5 |
```

### Phase 3: Wireframe Integration (Optional)
**Goal:** Extract entities and content structure from existing wireframes

```
IF .propel/context/wireframes/ EXISTS:
  1. Read information-architecture.md for navigation structure
  2. Read component-inventory.md for UI elements
  3. Extract entity types (forms, lists, cards, tables)
  4. Map entities to derived screens
  5. Incorporate content hierarchy
ELSE:
  - Proceed without wireframe context
  - Note: Screens will need entity definition during generate-figma
```

### Phase 4: Flow Definition
**Goal:** Define prototype flows organized by flow name

**Flow Structure:**
```markdown
## Flow: [Flow Name]
**Flow ID:** FL-XXX
**Derived From:** UC-XXX, UC-XXX
**Personas Covered:** [List of personas this flow serves]
**Description:** [Brief description of the flow purpose]

### Flow Sequence:
1. Entry: [Screen Name] / [State]
   - Trigger: [User action or system event]
2. Step: [Screen Name] / [State]
   - Action: [What happens]
3. Decision Point:
   - Success -> [Screen Name] / [State]
   - Error -> [Screen Name] / [Error State]
4. Exit: [Screen Name] / [Final State]

### Required Interactions:
- [Interaction 1]: [Description]
- [Interaction 2]: [Description]
```

**Minimum Required Flows:**
- At least one authentication flow (if applicable)
- At least one primary task completion flow
- At least one error recovery flow

### Phase 5: Component Identification
**Goal:** Identify required UI components from screens

**Component Categories:**
| Category | Components to Identify |
|----------|----------------------|
| Actions | Buttons, links, FABs |
| Inputs | Text fields, selects, checkboxes, toggles |
| Navigation | Headers, sidebars, tabs, breadcrumbs |
| Content | Cards, lists, tables, avatars |
| Feedback | Modals, drawers, toasts, alerts |

**Component Mapping:**
```markdown
| Screen | Components Required | Notes |
|--------|---------------------|-------|
| Login | TextField (2), Button (2), Link (1) | Email, password fields |
| Dashboard | Card (N), Table (1), Header (1) | Dynamic card count |
```

### Phase 6: Design System Generation
**Goal:** Create designsystem.md with tokens and specifications

**Token Sources (Priority Order):**
1. Design references from spec.md epics (Figma URLs, images)
2. Context7 MCP for framework defaults
3. Platform defaults (Material Design / iOS HIG / Fluent)

**Design System Sections:**
1. **Color Palette**
   - Primary, Secondary, Tertiary
   - Semantic (success, warning, error, info)
   - Neutral scale (50-900)
   - Light + Dark mode variants

2. **Typography**
   - Font families (heading, body, mono)
   - Size scale (H1-H6, Body, Caption)
   - Weights and line-heights

3. **Spacing**
   - Base unit (4px or 8px)
   - Scale (4, 8, 12, 16, 24, 32, 48, 64)

4. **Border Radius**
   - Small (4px), Medium (8px), Large (16px), Full

5. **Elevation/Shadows**
   - Levels 1-5 with definitions

6. **Component Specifications**
   - Reference component inventory from Phase 5
   - Define variants per component

### Phase 7: State Specifications
**Goal:** Document required states for each screen

**State Matrix:**
```markdown
| Screen | Default | Loading | Empty | Error | Validation |
|--------|---------|---------|-------|-------|------------|
| Login | Form ready | Authenticating | N/A | Invalid credentials | Field errors |
| Dashboard | Data loaded | Fetching data | No items | API error | N/A |
| Profile | User data | Loading profile | N/A | Load failed | Form errors |
```

**State Details Per Screen:**
```markdown
### SCR-001: Login

#### Default State
- Form fields visible and enabled
- Submit button active
- "Forgot Password" link visible

#### Loading State
- Submit button shows spinner
- Form fields disabled
- "Authenticating..." message

#### Error State
- Error banner at top
- "Invalid email or password"
- Retry option available

#### Validation State
- Inline field errors
- Red border on invalid fields
- Error messages below fields
```

### Phase 8: Output Generation

**A. Generate figma_spec.md:**
Using `.propel/templates/figma-specification-template.md`:
1. Source References (link to spec.md)
2. **UX Requirements (UXR-XXX)** - GENERATED IN THIS WORKFLOW (Section 2 of template)
3. Personas Summary (from spec.md)
4. Information Architecture / Navigation
5. Screen Inventory (derived, with UXR-XXX mapping and traceability)
6. Content & Tone
7. Data & Edge Cases
8. Branding / Visual Direction
9. Components / Design System Constraints
10. Prototype Flows (organized by flow, personas noted)
11. Export Requirements

**B. Generate designsystem.md:**
Using `.propel/templates/design-reference-template.md`:
1. Design Tokens (colors, typography, spacing)
2. Component Library Reference
3. Brand Guidelines
4. Figma/Image Asset References (from spec.md)

### Phase 9: Validation
**Goal:** Ensure complete coverage

**Validation Checks:**
- [ ] Every use case (UC-XXX) maps to at least one screen
- [ ] Every persona has screens covering their primary goals
- [ ] Every screen has all 5 states defined
- [ ] Every flow has entry, steps, and exit defined
- [ ] Design tokens are complete (colors, typography, spacing, radius, shadows)
- [ ] Component inventory covers all screen requirements

## Guardrails
- `rules/figma-design-standards.md`: File structure, components, tokens, exports
- `rules/ui-ux-design-standards.md`: Design tokens, component states
- `rules/web-accessibility-standards.md`: WCAG 2.2 AA, contrast, focus states
- `rules/ai-assistant-usage-policy.md`: Explicit commands; minimal output
- `rules/dry-principle-guidelines.md`: Single source of truth; delta updates
- `rules/markdown-styleguide.md`: Front matter, heading hierarchy

**>>> MANDATORY: Execute Quality Evaluation and Detection Rules. IF any gate fails, execute Self-Healing. <<<**

**Execution Steps:**
1. Score each dimension in the Quality Assessment table below
2. Apply Detection Rules to validate scores and calculate penalties
3. IF any MUST gate failed OR any score is below its threshold: Execute Self-Healing Protocol (Retry Flow)
4. Print the completed evaluation table with final scores
5. Print the Overall Score and Evaluation Summary

## Quality Evaluation

Score the specification quality against the following metrics:

| # | Evaluation Dimension | Score | Gate | Assessment Criteria |
|---|---------------------|-------|------|---------------------|
| 1 | Template Adherence | [0-100%] | MUST=100% | All sections from figma-specification-template.md present; no extra sections; UXR section populated |
| 2 | UXR-XXX Generation | [0-100%] | >=80% | All 6 UXR categories addressed; each UXR has acceptance criteria; no orphan UXR (all map to screens) |
| 3 | Use Case Coverage | [0-100%] | >=80% | Every UC-XXX with UI impact maps to screens with complete flow |
| 4 | Screen Completeness | [0-100%] | >=80% | All screens have 5 states defined; all screens map to at least one UXR-XXX |
| 5 | Flow Traceability | [0-100%] | >=80% | All flows trace back to UC-XXX with entry, steps, exit documented |
| 6 | Design Token Completeness | [0-100%] | >=80% | All token categories populated (colors, typography, spacing, radius, shadows) |
| 7 | Ambiguity Handling | [0-100%] | >=80% | Unclear UXR tagged with [UNCLEAR]; no UX assumptions made; clarifications documented |

### Detection Rules

| # | Detection Logic | Penalty |
|---|-----------------|---------|
| 1 | Compare sections vs figma-specification-template.md; UXR section populated | Missing: -20%; Empty UXR: BLOCKED |
| 2 | Count UXR categories (6 required); each UXR has acceptance criteria | Missing category: -15%; No criteria: -10%/UXR |
| 3 | UC-XXX with UI impact maps to SCR-XXX | Unmapped UC: -15%/instance |
| 4 | Each SCR-XXX has 5 states; maps to >=1 UXR-XXX | Missing state: -5%/state; No UXR link: -10%/screen |
| 5 | FL-XXX traces to UC-XXX; has entry/steps/exit | Missing trace: -15%; Incomplete flow: -10% |
| 6 | Token categories: colors, typography, spacing, radius, shadows | Missing category: -15%/category |
| 7 | [UNCLEAR] tags on ambiguous UXR | Untagged assumption: -15%; UX guess: -20% |

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
| UXR-XXX Generation | Add missing UXR category to section |
| Use Case Coverage | Add SCR-XXX mapping for unmapped UC |
| Screen Completeness | Add missing states to SCR-XXX |
| Flow Traceability | Add FL-XXX with UC-XXX trace |
| Design Token Completeness | Add missing token category |
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

## Console Output Format

```
===============================================================
         FIGMA SPECIFICATION GENERATION COMPLETE
===============================================================

1) UI Impact Assessment:
   - Total Epics: [X]
   - UI-Impacting Epics: [Y]
   - Proceeding with specification: YES

2) Persona Coverage:
   | Persona | Primary Screens | Flows Covered |
   |---------|-----------------|---------------|
   | [Name]  | [X screens]     | [Y flows]     |

3) Screen Inventory:
   - Total Screens Derived: [X]
   - From Use Cases: [List UC-XXX]

   | Screen | Derived From | Priority |
   |--------|--------------|----------|
   | [Name] | UC-XXX       | P0       |

4) Prototype Flows:
   - Total Flows: [X]

   | Flow Name | Personas | Screens |
   |-----------|----------|---------|
   | [Name]    | [List]   | [Count] |

5) Design System:
   - Color Tokens: [X]
   - Typography Tokens: [X]
   - Spacing Tokens: [X]
   - Components Specified: [X]

6) Output Files:
   - figma_spec.md: CREATED/UPDATED
   - designsystem.md: CREATED/UPDATED

===============================================================

Rules Applied:
- [bulleted list]

Evaluation Scores:
| Dimension | Score |
|-----------|-------|
| [...]     | [%]   |

Average Score: [X]%

Summary: [< 100 words]
===============================================================
```

---

*This workflow ensures comprehensive Figma specifications derived directly from requirements, with complete persona coverage and use case traceability.*
