---
description: AI-assisted Figma artifact generation workflow that transforms screen specifications into production-ready Figma structures with component libraries, clickable prototypes organized by flow, and JPG exports.
auto_execution_mode: 1
---

# Generate Figma Artifacts

As a Senior Product Designer and Figma Specialist, build production-ready Figma design artifacts from specifications. Your output includes 6-page Figma structure, clickable prototypes organized by flow, and JPG exports.

## Input Parameters

### Primary Input (Required)
- `.propel/context/docs/figma_spec.md` - **PRIMARY** - Screen inventory, states, flows, components
- `.propel/context/docs/designsystem.md` - **READ ONLY** - Design tokens, branding, component specs

### Optional Parameters
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `--app-name` | String | From figma_spec.md | Override application name |
| `--platform` | Enum | From figma_spec.md | `web` / `ios` / `android` / `responsive` |
| `--export-scale` | String | `2x` | JPG export scale |

### Input Validation
```
1. Check if figma_spec.md exists
   - IF NOT EXISTS -> EXIT with "figma_spec.md not found. Run create-figma-spec first."
2. Check if designsystem.md exists
   - IF NOT EXISTS -> EXIT with "designsystem.md not found. Run create-figma-spec first."
3. Validate figma_spec.md has required sections:
   - Screen Inventory
   - Prototype Flows
   - Component Requirements
4. ELSE -> Proceed with generation
```

## Output

### Artifact Generation
- `.propel/context/figma/figma_structure.json` - Figma-compatible JSON structure
- `.propel/context/figma/components/component_library.md` - Component specifications with variants
- `.propel/context/figma/exports/export_manifest.md` - JPG export list with naming

### Console Output (Do Not Save)
- List of rules used by the workflow in bulleted format
- Evaluation Scores in tabular format with average score
- Evaluation summary (less than 100 words)

**Note:**
- This workflow does NOT create or modify figma_spec.md or designsystem.md
- If these files need updates, run create-figma-spec workflow
- If output files already exist, update applicable sections only

## Core Principles

### Read-Only Design System
- **NEVER** create or modify designsystem.md
- Read all tokens from designsystem.md
- Apply tokens consistently across all artifacts
- If tokens are missing, report error (do not generate defaults)

### Spec-Driven Generation
- **NEVER** derive screens - use Screen Inventory from figma_spec.md
- **NEVER** create flows - use Prototype Flows from figma_spec.md
- Build exactly what is specified, nothing more

### Flow-Organized Prototypes
- Organize prototypes BY FLOW NAME (not by persona)
- Include persona coverage as metadata per flow
- Wire flows exactly as defined in figma_spec.md

### Auto Layout First
- All frames MUST use Auto Layout
- No absolute positioning except overlays
- Use spacing tokens from designsystem.md consistently

## Execution Flow

### Phase 0: Input Validation
**Critical First Step - Validate Required Files Exist**

```
1. Read figma_spec.md
   - Extract app name, platform
   - Extract screen inventory
   - Extract prototype flows
   - Extract component requirements

2. Read designsystem.md
   - Extract color tokens
   - Extract typography tokens
   - Extract spacing tokens
   - Extract border radius tokens
   - Extract elevation/shadow tokens
   - Extract component specifications

3. Validate completeness
   - IF missing screens -> ERROR
   - IF missing flows -> ERROR
   - IF missing tokens -> ERROR
```

### Phase 1: Build Figma Structure
**Goal:** Create 6-page Figma file structure

```
00_Cover      - Project metadata from figma_spec.md
01_Foundations - Tokens from designsystem.md
02_Components  - Component library with variants
03_Patterns    - Reusable layout patterns
04_Screens     - All screens with all states (from figma_spec.md)
05_Prototype   - Interactive flows (from figma_spec.md)
06_Handoff     - Developer notes and specs
```

### Phase 2: Foundations (01_Foundations)
**Goal:** Document design tokens from designsystem.md

**Apply directly from designsystem.md:**
- **Colors:** Primary, Secondary, Semantic, Neutral scale
- **Typography:** Font families, size scale, weights, line-heights
- **Spacing:** Base unit, scale values
- **Border Radius:** Small, Medium, Large, Full
- **Elevation:** Levels 1-5 with shadow definitions

**Frame Sizes (from figma_spec.md or defaults):**
| Platform | Width | Height |
|----------|-------|--------|
| Mobile | 390px | 844px |
| Tablet | 768px | 1024px |
| Web | 1440px | 1024px |

### Phase 3: Component Library (02_Components)
**Goal:** Build components specified in figma_spec.md

**Read component requirements from figma_spec.md:**
- Build only specified components
- Apply variants as defined
- Use tokens from designsystem.md

**Naming Convention:** `C/<Category>/<Name>`

**Required States per Component:**
| State | Visual Treatment |
|-------|-----------------|
| Default | Base styling per design tokens |
| Hover | Subtle elevation/color shift |
| Focus | Visible outline (>=3:1 contrast) |
| Active | Pressed/depressed visual |
| Disabled | 40% opacity |
| Loading | Skeleton or spinner |

### Phase 4: Patterns (03_Patterns)
**Goal:** Build reusable patterns from components

Patterns based on screen requirements in figma_spec.md:
- Auth form pattern (if login screens exist)
- Search + filter pattern (if search screens exist)
- List + pagination pattern (if list screens exist)
- Error/Empty/Loading pattern blocks (required for all)

All patterns built from 02_Components using designsystem.md tokens.

### Phase 5: Build Screens (04_Screens)
**Goal:** Create all screens with all states from figma_spec.md

**For each screen in Screen Inventory:**
1. Create frames for ALL 5 states:
   - `<ScreenName>/Default`
   - `<ScreenName>/Loading`
   - `<ScreenName>/Empty`
   - `<ScreenName>/Error`
   - `<ScreenName>/Validation`

2. Apply from 01_Foundations:
   - Grid system
   - Design tokens
   - Spacing scale

3. Build from 02_Components and 03_Patterns

4. Use state specifications from figma_spec.md

### Phase 6: Prototype Flows (05_Prototype)
**Goal:** Wire clickable flows as defined in figma_spec.md

**For each flow in Prototype Flows section:**
1. Read flow definition:
   - Flow name
   - Personas covered (as metadata)
   - Screen sequence
   - Interactions required

2. Wire screens in sequence:
   - Entry point -> Steps -> Exit conditions
   - Decision points with branches
   - Error paths

3. Document flow metadata:
   ```
   Flow: [Flow Name]
   Personas: [List from figma_spec.md]
   Screens: [Count]
   Interactions: [Count]
   ```

**Overlay Usage:**
| Element | Type | Animation |
|---------|------|-----------|
| Modal | Centered | Fade + scale |
| Drawer | Edge | Slide |
| Toast | Fixed | Slide + fade |
| Bottom Sheet | Bottom | Slide |

### Phase 7: Handoff (06_Handoff)
**Goal:** Generate developer documentation

**Documentation Sections:**
1. **Token Usage Rules**: From designsystem.md
2. **Component Usage**: Props, variants, usage guidelines
3. **Responsive Behavior**: Breakpoint changes
4. **Edge Cases**: Max content, truncation, overflow
5. **Accessibility Notes**: Focus order, ARIA requirements
6. **Screen-to-Code Mapping**: Reference figma_spec.md screen IDs

### Phase 8: Export Specifications
**Goal:** Generate export manifest for JPG exports

**Export Settings:**
| Setting | Value |
|---------|-------|
| Format | JPG (JPEG) |
| Quality | High (85%) |
| Scale - Mobile | 2x |
| Scale - Web | 2x |
| Color Profile | sRGB |

**Naming Convention:**
`<AppName>__<Platform>__<ScreenName>__<State>__v1.jpg`

**Generate export_manifest.md:**
- List all screens x states
- Full file paths
- Dimensions
- Export timestamp

## Guardrails
- `rules/figma-design-standards.md`: File structure, Auto Layout, components, exports
- `rules/ui-ux-design-standards.md`: Design tokens, component states
- `rules/web-accessibility-standards.md`: WCAG 2.2 AA, keyboard nav, contrast
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

Score the generation quality against the following metrics:

| # | Evaluation Dimension | Score | Gate | Assessment Criteria |
|---|---------------------|-------|------|---------------------|
| 1 | Spec Compliance | [0-100%] | MUST=100% | All screens from figma_spec.md generated; no missing screens |
| 2 | Token Application | [0-100%] | >=80% | All tokens from designsystem.md correctly applied |
| 3 | Figma Structure | [PASS/FAIL] | MUST PASS | All 6 pages complete with proper hierarchy |
| 4 | Component Coverage | [0-100%] | >=80% | All specified components with all states |
| 5 | Screen State Coverage | [0-100%] | >=80% | All 5 states for each screen (Default/Loading/Empty/Error/Validation) |
| 6 | Flow Implementation | [0-100%] | >=80% | All flows wired as specified in figma_spec.md |

### Detection Rules

| # | Detection Logic | Penalty |
|---|-----------------|---------|
| 1 | Count SCR-XXX in figma_spec.md; verify all generated | Missing screen: -20%/screen |
| 2 | Verify colors, typography, spacing match designsystem.md | Token mismatch: -10%/token |
| 3 | Check 6 Figma pages exist with proper hierarchy | Missing page: BLOCKED |
| 4 | Count components in spec; verify all with all states | Missing component: -10%; Missing state: -5% |
| 5 | Each screen has 5 states (Default/Loading/Empty/Error/Validation) | Missing state: -5%/state |
| 6 | FL-XXX flows wired with correct screen connections | Broken flow: -15%/flow |

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
| Spec Coverage | Add missing screens from spec |
| Component Completeness | Add missing component variants |
| State Coverage | Add missing screen states |
| Design Token Usage | Add token references |

**Non-Patchable (Skip):** Source spec missing, hallucinated content, Figma API errors, patch >30% of section, >=3 failing dimensions

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
              FIGMA ARTIFACT GENERATION COMPLETE
===============================================================

1) Input Validation:
   - figma_spec.md: FOUND
   - designsystem.md: FOUND
   - Screens defined: [X]
   - Flows defined: [Y]

2) Figma Structure:
   [check] 00_Cover
   [check] 01_Foundations
   [check] 02_Components
   [check] 03_Patterns
   [check] 04_Screens
   [check] 05_Prototype
   [check] 06_Handoff

3) Components Built:
   - From designsystem.md: [X components]
   - Variants per component: [Y average]

4) Screens Generated:
   | Screen Name | States | From Spec |
   |-------------|--------|-----------|
   | [Name]      | 5      | SCR-XXX   |

5) Prototype Flows Wired:
   | Flow Name | Personas | Screens | Interactions |
   |-----------|----------|---------|--------------|
   | [Name]    | [List]   | [X]     | [Y]          |

6) Export Manifest:
   - Total JPGs: [Screens x 5 states]
   - Naming: <AppName>__<Platform>__<Screen>__<State>__v1.jpg

===============================================================

Rules Applied:
- rules/figma-design-standards.md
- rules/ui-ux-design-standards.md
- rules/web-accessibility-standards.md

Evaluation Scores:
| Dimension | Score |
|-----------|-------|
| Spec Compliance | [X]% |
| Token Application | [X]% |
| Structure | [X]% |
| Components | [X]% |
| Screen States | [X]% |
| Flow Implementation | [X]% |
| Export Completeness | [X]% |

Average Score: [X]%

Summary: [< 100 words]
===============================================================
```

---

*This workflow generates Figma artifacts from specifications. It does NOT create specifications - run create-figma-spec first.*
