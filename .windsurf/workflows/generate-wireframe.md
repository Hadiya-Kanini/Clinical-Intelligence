---
description: AI-assisted wireframe generation workflow that transforms requirements into low, medium, or high-fidelity wireframes with site architecture mapping, component selection, user flow visualization, and direct Figma export capability.
auto_execution_mode: 1
---

# Wireframe Generation Workflow

As a Senior UX/UI Designer with expertise in information architecture, wireframing methodologies, and user-centered design, transform requirements into structured wireframes that serve as blueprints for web and mobile interfaces. This workflow follows industry-standard wireframing practices to map site architecture, identify necessary screens, select appropriate components, and generate wireframes at the specified fidelity level. The process emphasizes functionality and layout over visual aesthetics, providing clear guidance for stakeholders and development teams while supporting iterative refinement and user flow validation.

## Input Parameters

**Required Parameters:**
- `--requirement=TEXT_OR_PATH`: Requirement description (text) or path to requirements file (default: '.propel/context/docs/figma_spec.md')

**Optional Parameters:**
- `--fidelity=LEVEL`: Wireframe fidelity level (default: "high")
  - "low": Grayscale sketches - layout structure only (see rules/ui-ux-design-standards.md#wireframe-fidelity-standards)
  - "high": Production-ready mockups - full design system application (see rules/ui-ux-design-standards.md#wireframe-fidelity-standards)
- `--design-system=PATH`: Design system reference (default: '.propel/context/docs/designsystem.md')
- `--screen-type=TYPE`: Target device type - "web" (desktop), "mobile" (phone), "tablet", or "responsive" (all breakpoints) (default: "web")
- `--viewport-width=PIXELS`: Custom viewport width in pixels (default: 1440 for web, 375 for mobile, 768 for tablet)
- `--components=LIST`: Specific components to include (comma-separated: "header,footer,nav,cards,forms,buttons,gallery,search")
- `--reference-design=URL_OR_PATH`: URL or file path to reference design for style/pattern inspiration (optional)
- `--user-flow=FLOW`: Primary user flow to emphasize (e.g., "signup-to-checkout", "browse-to-purchase")
- `--export-format=FORMAT`: Output format - "hypertext (html)"

### Parameter Validation
- Verify requirement parameter is provided and accessible
- Validate fidelity level is "low" or "high"
- Validate screen-type is valid device type
- Verify viewport width is reasonable (320-2560 pixels)
- Parse and validate component list if provided
- Verify reference design URL/path is accessible if provided
- Validate export format is supported

### Context Sources (Always Read)
- `.propel/context/docs/figma_spec.md` - **PRIMARY** - Screen inventory (SCR-XXX), UXR-XXX, flows (FL-XXX), component mapping
- `.propel/context/docs/designsystem.md` - **REQUIRED FOR HIGH-FIDELITY** - Design tokens, branding, component specs
- `.propel/context/docs/spec.md` - **FALLBACK** - Only if figma_spec.md doesn't exist

### Prerequisite Validation
```
1. Check if figma_spec.md exists
2. IF EXISTS: Use as primary input (screens, flows, components pre-defined)
3. IF NOT EXISTS: Fall back to spec.md and execute full derivation phases (Phase 2-4, 7)
4. FOR HIGH-FIDELITY: Require designsystem.md (tokens, colors, typography)
5. IF designsystem.md missing for high-fidelity: Warn and use platform defaults
```

## Output
- Artifact generation
   - `.propel/context/wireframes/information-architecture.md`
   - `.propel/context/wireframes/component-inventory.md`
   - `.propel/context/wireframes/navigation-map.md` - Cross-screen navigation index
   - HTML wireframes `.propel/context/wireframes/wireframe-SCR-XXX-{screen-name}.html`

**Fidelity-Specific Output:**
- **LOW FIDELITY**: Grayscale wireframes only (no design token documentation)
- **HIGH FIDELITY**: Include design token application summary:
  - `.propel/context/wireframes/design-tokens-applied.md` (tokens from platform defaults or Context7 MCP)

**Component Inventory:**
- List of all UI components with:
  - Component name and type
  - Screen placement locations
  - Interactive states (if high fidelity)
  - Responsive behavior notes

### Bidirectional Integration
- These wireframes can be consumed by `create-figma-spec.md` Phase 3 (Wireframe Integration)
- If figma_spec.md is regenerated after wireframes exist, wireframe entities inform screen specifications

### Wireframe Output Format (console output only)

**Wireframe Generation Summary:**
```
===========================================================
              WIREFRAME GENERATION COMPLETE
===========================================================

Figma Wireframe URL:
   https://figma.com/file/{project-id}/Wireframe-{feature-name}

Specifications:
   - Fidelity: Low/High
   - Screen Type: Web/Mobile/Tablet/Responsive
   - Total Screens: 5
   - Viewport: 1440x900px

Components Included:
   [x] Header with navigation
   [x] Hero section with CTA
   [x] Product card grid
   [x] Footer with links
   [x] Modal dialogs

User Flow:
   Homepage -> Product Listing -> Product Detail -> Cart -> Checkout

===========================================================
```
- Print the following: 
    - List of rules used by the workflow in bulleted format
    - Evaluation Scores in tabular format with average score. 
    - Evaluation summary (less than 100 words).
    **Do not save as file.** (console output only)

**Note:**
- If the output file is already available make the necessary changes to applicable sections. Do not overwrite the whole file.
- Generate the output using:
  - `.propel/templates/wireframe-reference-template.md` template for information-architecture.md
  - `.propel/templates/component-inventory-template.md` template for component-inventory.md

## Core Principles
- **Requirements-Driven**: Every screen derives from use cases and user flows
- **Fidelity-Appropriate**: Match detail level to project phase (low vs high fidelity)
- **Framework-Aware**: Apply framework-specific patterns and component libraries

## Execution Flow

### 1. Framework Research via Context7 MCP
- Detect UI frameworks from project dependencies (package.json, imports)
- Retrieve framework-specific component patterns and wireframe guidelines
- Access design system best practices and component composition strategies

### 2. Sequential Design Reasoning via Sequential-thinking MCP
- Use `mcp__sequential-thinking__sequentialthinking` for complex wireframe planning:
  - Multi-step information architecture analysis
  - Component hierarchy and relationship mapping
  - User flow optimization with cognitive load considerations
  - Screen-to-screen navigation logic evaluation
  - Responsive layout adaptation reasoning across breakpoints

### Functionality Over Aesthetics
- Focus on layout structure and component placement
- Avoid detailed visual styling (colors, fonts, imagery)
- Use placeholders for content (lorem ipsum, image boxes)
- Emphasize information architecture and hierarchy

### Fidelity-Appropriate Detail

**IMPORTANT:** Wireframe fidelity standards are defined in `rules/ui-ux-design-standards.md#wireframe-fidelity-standards`. Follow those specifications precisely.

**Low Fidelity (Grayscale Wireframes):**
- Grayscale ONLY (#000, #666, #CCC, #FFF) - no colors
- System fonts only (Arial/Helvetica) - no design token fonts
- Basic shapes (rectangles, circles, lines) - no styling
- Gray box placeholders for images
- Lorem ipsum text placeholders
- Approximate spacing - no design token precision
- **Focus:** Layout structure and information hierarchy ONLY

**High Fidelity (Production-Ready Mockups):**
- **Apply ALL standards from `rules/ui-ux-design-standards.md`:**
  - Design Tokens (Color, Typography, Spacing, Radius, Dark Mode) - Lines 39-48
  - Component States (default, hover, focus, active, disabled, loading) - Lines 49-53
  - Interaction & Motion (150-300ms easing, prefers-reduced-motion) - Lines 62-65
  - Accessibility (WCAG AA contrast, focus states) - Lines 45, 51-53
- **Branding:** Logos, icons, branded imagery
- **Content:** Realistic text length, proper image dimensions
- **Output:** Pixel-perfect, development-ready mockup

**Note:** See `rules/ui-ux-design-standards.md#wireframe-fidelity-standards` for complete specifications. 

### Component-Based Design
- Utilize reusable UI components from design systems
- Maintain consistency across screens
- Document component variations and states
- Follow established design patterns

### User-Centric Flow
- Map complete user journeys from entry to goal completion
- For each use case in the requirements:
   - Map related screens
   - List required components and key interactions
   - Define entry, main steps, decision points, and exit
   - Note edge cases and error states
- Build navigation paths between screens
- Indicate interactive elements and their actions

### Iterative Refinement
- Start simple and add complexity progressively
- Gather feedback at each fidelity stage
- Validate with stakeholders before high-fidelity investment
- Support rapid iteration and modifications

**HIGH FIDELITY ANTI-PATTERNS:** See `rules/ui-ux-design-standards.md#frontend-aesthetics` (Line 33) for prohibited patterns.
**LOW FIDELITY:** Use Arial/Helvetica system fonts only (grayscale wireframes don't apply design aesthetics).

## Execution Flow

### Phase 0: Framework Research via Context7 MCP
**UI Framework Documentation Retrieval:**
1. Detect UI frameworks from project dependencies:
   - Check `package.json` for React, Vue, Angular, Svelte, etc.
   - Identify CSS frameworks (Tailwind, Bootstrap, Material-UI, Chakra UI)
   - Detect component libraries and design systems
2. Use `mcp__context7__resolve-library-id(libraryName: "framework-name")`
3. Fetch wireframe-relevant documentation:
   ```
   mcp__context7__get-library-docs(
     context7CompatibleLibraryID: "resolved-id",
     topic: "component-patterns,layout-systems,responsive-design,design-tokens,wireframing-guidelines"
   )
   ```

**Research Focus Areas:**
- Framework-specific component composition patterns
- Layout and grid system conventions
- Responsive breakpoint strategies and mobile-first approaches
- Design token structure (spacing, typography, colors)
- Component library documentation and wireframe representations
- Navigation patterns (routing, modals, drawers)
- Form patterns and validation approaches

**Output:**
- Framework-specific wireframing guidelines
- Component library inventory with wireframe equivalents
- Responsive design patterns to apply
- Design token references for spacing and typography

### Phase 1: Sequential Design Reasoning (WIREFRAME-SPECIFIC)
**Visual Layout Planning Using Sequential-thinking MCP:**

Use `mcp__sequential-thinking__sequentialthinking` (Fallback: Web_Search) for wireframe-specific concerns only.

**Note:** Information architecture, screen identification, and user flow mapping are already defined in figma_spec.md (if available). This phase focuses on visual layout execution.

**1. Visual Layout Strategy:**
- Grid system application per screen (12-column, flexbox, CSS grid)
- Content placement within layout grid
- Responsive adaptation rules per breakpoint
- Visual hierarchy through size, weight, and spacing

**2. Component Composition:**
- How components from figma_spec.md Section 10 compose on each screen
- Z-index stacking for overlays and modals
- State transition visualizations (default → hover → active)
- Component spacing and alignment within containers

**3. Fidelity Execution Planning:**
- **HIGH-FIDELITY:** Token application strategy from designsystem.md
  - Color token mapping to UI elements
  - Typography token application (headings, body, captions)
  - Spacing token consistency across screens
- **LOW-FIDELITY:** Placeholder content strategy
  - Gray box dimensions for images
  - Lorem ipsum length for text areas
  - Approximate spacing ratios

**4. Responsive Breakpoint Execution:**
- Desktop (1440px): Multi-column layout execution
- Tablet (768px): Collapsed navigation, 2-column adaptation
- Mobile (375px): Single column, stacked elements, touch targets

**Output:**
- Visual layout grid specifications per screen
- Component composition diagram
- Responsive adaptation rules
- Fidelity-specific styling approach

**Note:** Use Web_Search as a fallback mechanism

### Phase 2: Consume Figma Spec Inputs
**Goal:** Extract wireframe inputs from figma_spec.md (or fall back to spec.md)

**IF figma_spec.md EXISTS (Primary Path):**
1. Read Screen Inventory (Section 6): SCR-XXX list with priorities and states
2. Read Modal/Overlay Inventory: Triggers, parent screens
3. Read Component Requirements per Screen (Section 10)
4. Read Prototype Flows (Section 11): FL-XXX sequences for navigation wiring
5. Read UXR-XXX Requirements (Section 3): Constraints for wireframe generation
6. Read design tokens from designsystem.md (for high-fidelity)

**IF figma_spec.md DOES NOT EXIST (Fallback Path):**
- Fall back to spec.md and execute legacy derivation:
  - Execute Phase 2-FALLBACK: Requirement Analysis (below)
  - Execute Phase 3-FALLBACK: Site Architecture Mapping
  - Execute Phase 4-FALLBACK: Screen Identification
  - Execute Phase 7-FALLBACK: User Flow Mapping

**Token Source for High-Fidelity Wireframes:**
- **LOW FIDELITY**: Grayscale only - no design tokens needed
- **HIGH FIDELITY**: Use designsystem.md tokens (primary) or Context7 MCP research:
  - Read color, typography, spacing tokens from designsystem.md
  - Fallback: Use Context7 MCP to fetch framework design tokens
  - Fallback: Apply Material Design / iOS HIG / Fluent defaults based on platform

**Success Criteria:**
- Screen inventory loaded (SCR-XXX list with priorities)
- Component requirements per screen identified
- Flow sequences available for navigation wiring (FL-XXX)
- Design tokens sourced for high-fidelity

---

### Phase 2-FALLBACK: Requirement Analysis (Only if figma_spec.md missing)
**Extract Wireframe Requirements from spec.md:**
1. Parse requirement input (text or file content)
2. Identify key features and functionalities from requirements
3. Extract user personas and use cases
4. Determine primary user goals and actions
5. List required UI components and interactions
6. Identify content types (text, images, forms, data displays)
7. Note any constraints or technical requirements

**Success Criteria:**
- Clear understanding of what needs to be wireframed
- Identified all user-facing features
- Documented content and interaction requirements

### Phase 3-FALLBACK: Site Architecture Mapping (Only if figma_spec.md missing)
**Structure Information Hierarchy:**
1. Define site map or app screen hierarchy (reference Phase 1 Sequential-thinking output)
2. Identify main navigation categories
3. Map parent-child relationships between screens
4. Determine entry points and primary paths
5. Plan information organization (taxonomies, categories)
6. Document sitemap structure
7. Apply framework-specific navigation patterns (from Phase 0 Context7 research)

**Deliverables:**
- Site map diagram showing page relationships
- Navigation structure outline
- Content hierarchy definition
- Framework-aligned navigation patterns

**Note:** Skip this phase if figma_spec.md exists - information architecture is defined in phase 1.

### Phase 4-FALLBACK: Screen Identification (Only if figma_spec.md missing)
**Determine Required Wireframes:**
1. List all unique screens based on user flows:
   - Homepage/Landing page
   - Category/Listing pages
   - Detail/Item pages
   - Forms (signup, checkout, contact)
   - Account/Dashboard screens
   - Error/Empty states
   - Modal/Dialog overlays
2. Prioritize screens by importance and user flow sequence
3. Identify screen variations (logged in vs. logged out, different user roles)
4. Determine responsive breakpoints if screen-type is "responsive"

**Screen Inventory:**
- Numbered list of screens with brief descriptions
- Logical sequence following user journey
- Notation for critical vs. optional screens

**Modal/Dialog/Overlay Inventory:**
Systematically identify all overlay elements (often missed in screen counts):

**Identification Checklist - Review each category:**
- [ ] **Authentication Modals:** Login, Signup, Password Reset, 2FA verification
- [ ] **Confirmation Dialogs:** Delete confirmation, Logout confirmation, Discard changes, Submit confirmation
- [ ] **Media Overlays:** Image lightbox, Video player, Gallery viewer, Document preview
- [ ] **Filter/Search:** Advanced filter drawer (mobile), Search overlay, Sort options panel
- [ ] **Notifications:** Success messages, Error alerts, Warning prompts, Toast notifications
- [ ] **Forms:** Quick add modal, Inline edit dialog, Share dialog, Contact form popup
- [ ] **Information:** Help tooltips, Onboarding tours, Feature explanations, Cookie consent
- [ ] **Navigation:** Mobile menu drawer, User profile dropdown (if complex), Settings panel

**For Each Modal/Dialog, Document:**
- **Name:** Descriptive name (e.g., "Delete Product Confirmation")
- **Type:** Modal / Dialog / Drawer / Popover / Overlay
- **Trigger Context:** What user action opens it (e.g., "Click Delete button")
- **Parent Screen(s):** Which screens can trigger this modal
- **Dismissal Actions:** Close button, overlay click, ESC key, success action
- **Priority:** P0 (critical) to P3 (optional)
- **Responsive Behavior:** Desktop modal vs. mobile full-screen transformation

**Validation:**
- Each modal counts as a separate wireframe (not just a component state)
- Each modal must have documented trigger and dismissal actions
- Mobile behavior must be specified (modal, drawer, or full-screen)

**Example Output:**
```
Modal Inventory:
1. Login Modal (P0) - Triggered from: Header, Protected pages
2. Delete Confirmation (P0) - Triggered from: Product list, User management
3. Image Lightbox (P2) - Triggered from: Product detail, Gallery
4. Filter Drawer (P1) - Triggered from: Product listing (mobile only)
```

**Note:** Skip this phase if figma_spec.md exists - screen inventory is defined in phase 1.

### Phase 5: Component Selection
**Choose Appropriate UI Components:**
1. **Reference Phase 0 Context7 output**: Use framework-specific components and patterns
2. **Reference Phase 1 Sequential-thinking/web-search output**: Component hierarchy and relationships
3. Review requirement components or select from standard patterns:
   - **Layout**: Headers, footers, sidebars, grids, containers
   - **Navigation**: Menus, tabs, breadcrumbs, pagination
   - **Content**: Cards, lists, tables, galleries, accordions
   - **Interactive**: Buttons, forms, inputs, dropdowns, sliders
   - **Feedback**: Modals, tooltips, alerts, progress indicators
2. Match components to fidelity level:
   - Low: Simple rectangles with labels
   - High: Detailed component structure with realistic content
3. Document component states (default, hover, active, disabled, error)
4. Note responsive behavior for each component

**Component Library:**
- Selected components with wireframe representation style
- Component placement guidelines
- Interaction specifications

### Phase 6: Wireframe Generation
**Create Wireframes Based on Fidelity Level:**

**Low-Fidelity Wireframes:**
Follow specifications in `rules/ui-ux-design-standards.md#wireframe-fidelity-standards` (Low Fidelity section):
1. Grayscale ONLY (#000, #666, #CCC, #FFF)
2. System fonts only (Arial/Helvetica)
3. Basic shapes - no styling
4. Gray box image placeholders
5. Lorem ipsum text
6. Approximate spacing
7. **Focus:** Layout structure and information hierarchy ONLY

**High-Fidelity Wireframes:**
Follow ALL standards in `rules/ui-ux-design-standards.md`:

1. **Design Tokens Application** (Lines 39-48):
   - Apply complete color palette (brand + semantic + neutral grayscale)
   - Use typography modular scale (H1->Caption, proper line-heights)
   - Implement spacing scale (8px base unit with multipliers)
   - Apply radius tokens (small for inputs, medium for cards)
   - Validate WCAG AA contrast (4.5:1 text, 3:1 UI)

2. **Component States** (Lines 49-53):
   - Implement all states: default, hover, focus, active, disabled, loading
   - State feedback <100ms
   - Focus outline contrast >=3:1
   - Differentiate disabled vs readonly

3. **Interaction & Motion** (Lines 62-65):
   - Micro-interactions: 150-300ms easing
   - Loading states: Skeletons >300ms, inline spinners for local actions
   - Honor `prefers-reduced-motion`

4. **Frontend Aesthetics** (Lines 24-37):
   - Avoid anti-patterns: Inter, Roboto, Arial, purple gradients, predictable layouts
   - Use distinctive typography, cohesive color themes, spatial composition
   - Apply backgrounds, textures, shadows, decorative elements per design system

5. **Branding & Content**:
   - Brand logos with proper sizing and placement
   - Icon system (outlined, filled, custom brand icons)
   - Realistic text length and proper image dimensions
   - Branded visualizations

**Output:** Production-ready visual mockup following all `rules/ui-ux-design-standards.md` specifications

**Traceability Requirements:**
- Each wireframe file name MUST include SCR-XXX ID: `wireframe-SCR-001-login.html`
- Component placement must reference figma_spec.md Section 10 component requirements
- UXR-XXX constraints must be applied (from figma_spec.md Section 3)

**Navigation Implementation Requirements:**
- Parse FL-XXX flows from figma_spec.md Section 11 (Prototype Flows)
- For each flow sequence, implement clickable navigation:
  - Buttons/links that trigger screen transitions MUST be hyperlinks to target wireframe
  - Example: "Login" button on SCR-001 links to `wireframe-SCR-002-dashboard.html`
- Navigation mapping comment required in each wireframe:
  ```html
  <!-- Navigation Map
  | Element | Action | Target Screen |
  |---------|--------|---------------|
  | #login-btn | click | SCR-002 (Dashboard) |
  | #forgot-pwd | click | SCR-005 (Password Reset) |
  -->
  ```
- All interactive elements from FL-XXX sequences must be wired
- Dead-end screens (no outbound navigation) must be flagged in output summary

**AI-Assisted Generation:**
- Use AI design tools (if available) to auto-generate wireframes from requirements
- Leverage design system components for consistency
- Apply responsive design patterns automatically
- Generate multiple screen variants for A/B consideration

### Phase 7-FALLBACK: User Flow Mapping (Only if figma_spec.md missing)
**Visualize User Journeys:**
1. **Reference Phase 1 Sequential-thinking output**: Use user flow decision tree and navigation logic
2. Draw connections between wireframe screens
3. Indicate navigation paths with arrows
4. Label actions that trigger screen transitions (e.g., "Click 'Buy Now'", "Submit form")
5. Show decision points and branching paths (from Sequential-thinking analysis)
6. Highlight primary flow vs. alternative paths
7. Annotate user actions and system responses

**User Flow Diagram Elements:**
- Start point (entry screen)
- Screen nodes (wireframe references)
- Action arrows (user interactions)
- Decision diamonds (conditional paths)
- End point (goal completion or exit)

**Note:** Skip this phase if figma_spec.md exists. Navigation is implemented directly in Phase 6 using FL-XXX flow sequences.

### Phase 8: Export and Delivery
**Generate Output in Specified Format:**

**Figma Export:**
1. Create new Figma project: "Wireframe - {Feature Name}"
2. Organize screens into pages (by user flow or screen type)
3. Apply wireframe style:
   - Low-fidelity: Use wireframe kit with sketchy lines
   - High-fidelity: Use detailed components with annotations
4. Add artboards for each screen at specified viewport size
5. Connect screens with flow arrows using Figma prototype feature
6. Add comments/annotations explaining interactions
7. Generate shareable Figma URL
8. Set permissions (view/comment access)

**Image Export (if selected):**
1. Export each wireframe screen as PNG or SVG (default: PNG)
2. Naming convention: `wireframe-{screen-name}-{fidelity}.png`
3. Save to `.propel/output/wireframes/` directory
4. Create aggregate view showing all screens (if multiple)
5. Export user flow diagram as separate image

**HTML Export Styling Requirements:**

**Low-Fidelity HTML:**
```html
<style>
/* Grayscale wireframes per rules/ui-ux-design-standards.md */
:root {
  --wireframe-black: #000000;
  --wireframe-gray-dark: #666666;
  --wireframe-gray-light: #CCCCCC;
  --wireframe-white: #FFFFFF;
}
body { font-family: Arial, sans-serif; color: var(--wireframe-black); }
.component { border: 1px solid var(--wireframe-gray-dark); background: var(--wireframe-white); }
/* NO colors, NO shadows, NO custom fonts */
</style>
```

**High-Fidelity HTML:**
Extract design tokens from platform defaults (Material Design / iOS HIG / Fluent) via Context7 MCP:
```html
<style>
/* Design Tokens from platform defaults (Context7 MCP) */
:root {
  /* Color palette - platform defaults */
  --color-primary: [from Context7 MCP or platform defaults];
  --color-secondary: [from Context7 MCP or platform defaults];
  --color-success: #22c55e;
  --color-neutral-50: #f8fafc;

  /* Typography - platform defaults */
  --font-heading: system-ui, -apple-system, sans-serif;
  --font-body: system-ui, -apple-system, sans-serif;
  --font-size-h1: 2.25rem;
  --line-height-base: 1.5;

  /* Spacing - standard 8px grid */
  --spacing-unit: 8px;
  --spacing-2: 16px;
  --spacing-4: 32px;

  /* Radius - platform defaults */
  --radius-sm: 4px;
  --radius-md: 8px;

  /* Transitions */
  --transition-micro: 200ms ease;
}

/* Component states */
.button-primary {
  background: var(--color-primary);
  transition: all var(--transition-micro);
}

.button-primary:hover { /* Hover state */ }
.button-primary:active { /* Active state */ }
.button-primary:focus { /* Focus with >=3:1 contrast outline */ }
</style>
```

**Delivery Checklist:**
- [x] All screens generated at correct fidelity level
- [x] Screens organized logically (by flow or type)
- [x] User flow connections visible
- [x] Annotations and labels clear
- [x] Export URL or file paths valid
- [x] Component inventory documented
- [x] Responsive variants included (if applicable)

### Phase 9: Wireframe Completion Summary
**Finalize wireframe generation:**

1. Verify all screens generated at correct fidelity
2. Confirm component inventory is complete
3. Validate user flow connections
4. Generate final output summary

**Screen Coverage Validation (if figma_spec.md exists):**
1. Read Screen Inventory from figma_spec.md Section 6
2. List all SCR-XXX IDs with priorities
3. Compare against generated wireframe files
4. Report:
   - Covered: [SCR-XXX list]
   - Missing: [SCR-XXX list] (FAIL if any P0/P1 missing)
   - Extra: [wireframes not in inventory]

**Flow Coverage Validation (if figma_spec.md exists):**
1. Read Prototype Flows from figma_spec.md Section 11
2. For each FL-XXX:
   - Verify all screens in flow sequence have wireframes
   - Verify navigation links between screens are implemented
3. Report:
   - Complete Flows: [FL-XXX list]
   - Broken Flows: [FL-XXX with missing screens or links]

**Output Files Generated:**
- `.propel/context/wireframes/information-architecture.md`
- `.propel/context/wireframes/component-inventory.md`
- `.propel/context/wireframes/navigation-map.md`
- `.propel/context/wireframes/wireframe-SCR-XXX-{screen-name}.html` (for each screen)
- `.propel/context/wireframes/design-tokens-applied.md` (high-fidelity only)

**Note:** Wireframes are standalone artifacts. They do NOT update designsystem.md. The create-figma-spec workflow can optionally read wireframes to understand entities when generating the Figma specification.

### Technical Implementation

**AI Design Tools (Preferred):**
- Use AI-powered design generation if available
- Leverage design APIs for programmatic wireframe creation
- Apply design tokens and component libraries

**Manual Wireframe Creation:**
- Use vector graphics libraries for programmatic drawing
- Apply wireframe styling (grayscale, simple shapes)
- Generate SVG or PNG outputs
- Create Figma files via API if Figma token available

**Component Libraries to Reference:**
- Material Design wireframe components
- Bootstrap wireframe kit
- Custom design system components
- Platform-specific UI kits (iOS, Android, Web)

**Responsive Design Patterns:**
- Desktop (1440px): Multi-column layouts, expanded navigation
- Tablet (768px): Collapsed navigation, 2-column grids
- Mobile (375px): Single column, hamburger menu, stacked elements

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

**>>> MANDATORY: Execute Quality Evaluation and Detection Rules. IF any gate fails, execute Self-Healing. <<<**

**Execution Steps:**
1. Score each dimension in the Quality Assessment table below
2. Apply Detection Rules to validate scores and calculate penalties
3. IF any MUST gate failed OR any score is below its threshold: Execute Self-Healing Protocol (Retry Flow)
4. Print the completed evaluation table with final scores
5. Print the Overall Score and Evaluation Summary

## Quality Evaluation

Once the wireframe is generated, score the output quality against the following metrics, providing a percentage score (1-100%) for each.

### Wireframe Quality Assessment

| # | Evaluation Dimension | Score | Gate | Assessment Criteria |
|---|---------------------|-------|------|---------------------|
| 1 | Template Adherence | [0-100%] | MUST=100% | Output follows wireframe template structure; no extra sections |
| 2 | Traceability | [0-100%] | >=80% | Wireframes map to SCR-XXX from figma_spec.md; filenames include SCR-XXX ID |
| 3 | Screen Coverage | [0-100%] | MUST>=90% | ALL screens from figma_spec.md have wireframes; no missing screens |
| 4 | Flow Coverage | [0-100%] | >=80% | ALL FL-XXX flows from figma_spec.md are navigatable; screens link correctly |
| 5 | Navigation Integrity | [0-100%] | >=80% | All buttons/links per FL-XXX are wired to target screens; no dead-end screens |
| 6 | Interaction States | [0-100%] | >=80% | Default/hover/active/error/loading represented per fidelity level |
| 7 | Accessibility | [0-100%] | >=80% | Semantics, focus order, touch targets, alt placeholders |

### Detection Rules

| # | Detection Logic | Penalty |
|---|-----------------|---------|
| 1 | Compare sections vs wireframe-template.md | Missing: -20%; Extra: -15% |
| 2 | Each wireframe filename contains SCR-XXX | Missing ID: -10%/wireframe |
| 3 | Count SCR-XXX (P0+P1) in figma_spec.md vs wireframe files | Missing P0: -25%/screen; Missing P1: -15%/screen |
| 4 | For each FL-XXX, verify all screens in sequence have wireframes | Missing screen in flow: -15%/screen |
| 5 | For each navigation element in FL-XXX, verify href links to correct wireframe | Broken link: -10%/link; Dead-end P0 screen: -20% |
| 6 | Each interactive element has required states per fidelity | Missing state: -5%/state |
| 7 | Semantic labels, focus order, touch targets >=44px | Violation: -10%/instance |

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
| Traceability | Rename wireframe files to include SCR-XXX ID |
| Screen Coverage | Generate missing wireframes for P0/P1 screens |
| Flow Coverage | Generate wireframes for missing screens in FL-XXX flows |
| Navigation Integrity | Add missing href links to wireframe navigation elements |
| Interaction States | Add missing element states |
| Accessibility | Fix touch targets/labels |

**Non-Patchable (Skip):** Source missing, hallucinated content, patch >30% of section, >=3 failing dimensions

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
