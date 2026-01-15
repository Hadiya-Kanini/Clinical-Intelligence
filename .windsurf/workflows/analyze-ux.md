---
description: Comprehensive UI/UX analysis workflow leveraging Playwright automation, Context7 framework research, and sequential design reasoning to evaluate visual consistency, accessibility compliance, responsiveness, and user experience quality across multiple viewports and interaction patterns.
auto_execution_mode: 1
---

# Unified User Interface Analysis

As a Senior UX Designer with expertise in accessibility, responsive design, and user-centered design principles, conduct a comprehensive evaluation of the user interface implementation. This analysis combines automated testing via Playwright MCP, framework best practices research through Context7, and systematic design reasoning to ensure the UI meets high standards for visual consistency, accessibility compliance (WCAG 2.1 AA), responsive behavior across devices, intuitive interaction patterns, and overall user experience quality. The evaluation provides evidence-based feedback focused on identifying design issues and their impact on users, supporting continuous improvement of the interface.

## Input Parameters

**Optional Parameters:**
- `figma_design`: Figma URL or local image path to design mockup for pixel-perfect comparison (e.g., 'https://figma.com/file/abc123' or '/path/to/wireframe.png' or '/path/to/wireframe.html')
- `server_url`: URL of the development server (defaults to http://localhost:3000)
- `app_path`: URL path to review (e.g., '/checkout', '/dashboard', '/home'). Defaults to '/' if not specified
- `focus_area`: Aspect to emphasize in review - "full" (default), "accessibility", "responsive", "interactions", or "visual-polish"

### Parameter Validation
- Verify server URL accessibility and response
- Validate Figma URL or image path if provided
- Ensure app_path is properly formatted
- Validate focus_area against allowed values

## Output
- Artifact generation:
  - `.propel/context/ui-reviews/ui-review-${timestamp}.md`
- Print the following:
  - List of rules applied to the workflow in bulleted format
  - Evaluation Scores in tabular format with average score
  - Evaluation summary (less than 100 words)
  **Do not save as file.** (Console output only)

**Note:**
- If the output file is already available make the necessary changes to applicable sections. Do not overwrite the whole file.
- Always create the output file in manageable smaller chunks to manage memory and processing constraints.
- Always generate a single unified document using
   - `.propel/templates/design-analysis-template.md` template

## Core Principles

### Automated Testing with Playwright MCP
- Navigate and interact with the UI programmatically
- Capture screenshots at multiple viewports with descriptive filenames
- Test keyboard navigation and accessibility features
- Monitor browser console for errors and warnings
- Verify responsive behavior across devices
- Capture interactive states (hover, focus, active) when relevant

### Framework Research via Context7 MCP (Use web fetch as a fallback)
- Detect UI frameworks from package.json or import statements
- Retrieve framework-specific design patterns and component guidelines
- Access accessibility best practices and ARIA implementation guides
- Review responsive design patterns and breakpoint strategies
- Gather performance optimization recommendations

### Sequential Design Reasoning (Use web fetch as a fallback)
- Use Sequential-thinking MCP for complex design analysis
- Evaluate component consistency and design token usage
- Assess visual hierarchy and information architecture
- Analyze user experience flow and cognitive load
- Perform step-by-step user journey evaluation

### Retrieve Design Specifications (Primary: figma_spec.md)
- **Primary Source**: Read `.propel/context/docs/figma_spec.md` for:
  - Screen inventory with state requirements (Default, Loading, Empty, Error, Validation)
  - Screen-to-persona mapping and flow definitions
  - Component requirements per screen
  - Export specifications and naming conventions
- **Secondary Source**: Read `.propel/context/docs/designsystem.md` for:
  - Design tokens (colors, typography, spacing, shadows)
  - Component specifications and variants
  - Brand guidelines and visual direction
- **Fallback**: Use Figma MCP to access design files directly if spec files unavailable
- Note any design annotations or developer handoff notes

### Communication Approach
- **Problems Over Prescriptions**: Describe problems and their impact, not technical solutions
- **Evidence-Based**: Always provide screenshots for visual issues with specific examples
- **Constructive Tone**: Maintain objectivity while being constructive

## Execution Flow

### Phase 1: Context7 Framework Research
**UI Framework Documentation Retrieval:**
1. Detect frameworks from codebase (package.json, imports)
2. Use `mcp__context7__resolve-library-id(libraryName: "framework-name")`
3. Fetch docs: `mcp__context7__get-library-docs(context7CompatibleLibraryID: "resolved-id", topic: "design-patterns,accessibility,responsive-design,best-practices")`

**Research Focus:**
- Framework-specific design patterns and component guidelines
- Accessibility best practices and ARIA implementation
- Responsive design patterns and breakpoint strategies
- Animation and interaction design principles
- Performance optimization for UI components

### Phase 2: Sequential Design Reasoning
Use `mcp__sequential-thinking__sequentialthinking` for:

**Design System Evaluation:**
- Analyze component consistency and design token usage
- Evaluate visual hierarchy and information architecture
- Assess brand alignment and design language coherence
- Review user experience flow and cognitive load

**Usability Analysis:**
- Step-by-step user journey evaluation
- Error prevention and recovery pattern analysis
- Accessibility impact assessment for different user groups
- Cross-platform and cross-browser compatibility reasoning

### Phase 3: Preparation & Setup
- Analyze PR description/changes to understand motivation and scope
- Review code diff to understand implementation details
- Set up Playwright browser with `mcp__playwright__browser_install`
- Configure initial viewport (1440x900 for desktop)
- Navigate to the development server URL

### Phase 4: Interaction and User Flow
- Execute primary user flows following testing notes
- Test all interactive states (hover, active, disabled, focus)
- Verify form submissions and validation feedback
- Check destructive action confirmations
- Assess perceived performance and responsiveness
- Document interaction patterns with screenshots

### Phase 5: Responsiveness Testing
- Test desktop viewport (1440px) with screenshot
- Test tablet viewport (768px) - verify layout adaptation
- Test mobile viewport (375px) - ensure touch optimization
- Verify no horizontal scrolling or element overlap
- Check responsive navigation patterns (hamburger menus, etc.)
- Document breakpoint transitions

### Phase 6: Visual Polish
- Assess layout alignment and spacing consistency
- Verify typography hierarchy and legibility
- Check color palette consistency and contrast
- Validate image quality and optimization
- Ensure visual hierarchy guides user attention
- Review animation timing and smoothness

### Phase 7: Accessibility (WCAG 2.1 AA)
- Test complete keyboard navigation (Tab order)
- Verify visible focus states on all interactive elements
- Confirm keyboard operability (Enter/Space activation)
- Validate semantic HTML usage
- Check form labels and ARIA attributes
- Verify image alt text and descriptions
- Test color contrast ratios (4.5:1 minimum for normal text)
- Run automated accessibility audit using `mcp__playwright__browser_evaluate`

### Phase 8: Robustness Testing
- Test form validation with invalid/edge case inputs
- Stress test with content overflow scenarios
- Verify loading, empty, and error states
- Check network failure handling
- Test browser back/forward navigation
- Validate data persistence across refreshes

### Phase 9: Code Health
- Verify component reuse over duplication
- Check for design token usage (no magic numbers)
- Ensure adherence to established patterns
- Review CSS organization and maintainability
- Check for console errors or warnings

### Phase 10: Content and Console
- Review grammar, spelling, and clarity of all text
- Check browser console for errors/warnings
- Verify proper error messaging
- Validate help text and tooltips

### Technical Implementation

**Playwright MCP Tools by Category:**

1. **Setup & Navigation:**
   - `mcp__playwright__browser_install` - Initialize browser
   - `mcp__playwright__browser_navigate` - Load pages
   - `mcp__playwright__browser_tabs` - Manage multiple views

2. **Interaction Testing:**
   - `mcp__playwright__browser_click` - Test clickable elements
   - `mcp__playwright__browser_type` - Input field testing
   - `mcp__playwright__browser_select_option` - Dropdown testing
   - `mcp__playwright__browser_hover` - Hover state validation
   - `mcp__playwright__browser_drag` - Drag and drop testing
   - `mcp__playwright__browser_press_key` - Keyboard navigation

3. **Visual Documentation:**
   - `mcp__playwright__browser_take_screenshot` - Capture evidence
   - `mcp__playwright__browser_resize` - Viewport testing
   - `mcp__playwright__browser_snapshot` - DOM analysis

4. **Quality Checks:**
   - `mcp__playwright__browser_console_messages` - Error monitoring
   - `mcp__playwright__browser_evaluate` - Run audits
   - `mcp__playwright__browser_network_requests` - Performance analysis
   - `mcp__playwright__browser_wait_for` - Loading state verification

### Pixel-Perfect Comparison (Design Specification Driven)

**Primary Reference: figma_spec.md**
1. **Load Screen Specifications from figma_spec.md:**
   - Identify the screen being tested from Screen Inventory
   - Load required states (Default, Loading, Empty, Error, Validation)
   - Reference component requirements for the screen
   - Note which personas this screen serves

2. **Load Design Tokens from designsystem.md:**
   - Extract spacing scale for measurement validation
   - Load typography specifications (font, size, weight, line-height)
   - Reference color tokens for contrast and consistency checks
   - Load component variant specifications

**When `figma_design` parameter is provided (additional reference):**
1. **Extract Visual Details:**
   - For Figma URLs: Use WebFetch to extract visual specifications
   - For images: Use Read tool to analyze mockup
   - Cross-reference with figma_spec.md screen specifications

2. **Side-by-Side Analysis:**
   - Take implementation screenshots at matching viewports
   - Compare layouts against figma_spec.md state requirements
   - Validate design token application from designsystem.md
   - Measure pixel-level discrepancies against specifications

3. **Document Deviations:**
   - List specific measurements that differ from designsystem.md tokens
   - Identify missing screen states from figma_spec.md requirements
   - Prioritize based on visual impact and state coverage
   - Provide recommendations for both strict design adherence and practical implementation approaches

### Edge Cases to Consider
- Browser-specific rendering differences
- Font availability and fallbacks
- Dynamic content that might affect layout
- Animations and transitions not visible in static designs
- Screen state coverage (all 5 states per figma_spec.md)
- Accessibility improvements that might deviate from pure visual design

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

Once the output is generated, Score the summary presentation to evaluate its quality against the following metrics, providing a percentage score (1-100%) for each.

### UX Analysis Quality Assessment

| # | Evaluation Dimension | Score | Gate | Assessment Criteria |
|---|---------------------|-------|------|---------------------|
| 1 | Template Adherence | [0-100%] | MUST=100% | Analysis follows required format; no extra sections |
| 2 | Visual Consistency | [0-100%] | >=80% | Design system adherence; consistent spacing, typography, colors |
| 3 | Accessibility Compliance | [0-100%] | >=80% | WCAG 2.2 AA compliance; keyboard nav; ARIA; contrast ratios |
| 4 | Responsive Design | [0-100%] | >=80% | Layout adaptation; no horizontal scroll; touch optimization |
| 5 | Interaction Quality | [0-100%] | >=80% | Clear hover/focus states; intuitive flows |
| 6 | Performance Impact | [0-100%] | >=80% | Render performance; animation smoothness; bundle impact |
| 7 | Design Specification Alignment | [0-100%] | >=80% | Pixel-perfect match to figma_spec.md; state coverage |
| 8 | Robustness | [0-100%] | >=80% | Error handling; edge cases; loading/empty states |

### Detection Rules

| # | Detection Logic | Penalty |
|---|-----------------|---------|
| 1 | Analysis follows required format | Missing section: -20% |
| 2 | Design system tokens verified (spacing, typography, colors) | Deviation: -10%/type |
| 3 | WCAG 2.2 AA checks documented | Missing check: -15%/criterion |
| 4 | Breakpoint behavior documented; no horizontal scroll | Missing breakpoint: -10%; Scroll issue: -15% |
| 5 | Hover/focus states documented | Missing state: -10%/component |
| 6 | Render performance metrics captured | No metrics: -15% |
| 7 | Screen matches figma_spec.md; states covered | Pixel mismatch: -10%/screen; Missing state: -5% |
| 8 | Error/loading/empty states verified | Missing state type: -10%/type |

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
| Visual Consistency | Add design token deviation notes |
| Accessibility Compliance | Add missing WCAG check results |
| Responsive Design | Add breakpoint documentation |
| Interaction Quality | Document missing states |
| Performance Impact | Add performance metrics |
| Design Specification Alignment | Add pixel comparison notes |
| Robustness | Add state verification results |

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