---
trigger: glob
description: UI design and system guidance: tokens, layout hierarchy, module-specific UI tactics, accessibility alignment, and interaction quality. Excludes foundational a11y, performance, and frontend coding rules already covered elsewhere.
globs: "**/*.{jsx,tsx,js,ts,css,scss,html}, **/{designsystem.md,component-inventory.md,information-architecture.md}"
---

# Design Principles
## Core Philosophy
- Users First: Optimize task completion & reduce cognitive load
- Clarity Over Novelty: Explicit labels & progressive disclosure
- Consistent Language: Shared visual grammar across components
- Accessible by Design: Contrast & focus states at component inception
- Opinionated Defaults: Sensible initial settings minimize decisions

## Design Thinking
Before coding:
- **Purpose:** What problem? Who uses it?
- **Tone:** Pick extreme aesthetic: brutally minimal, maximalist chaos, retro-futuristic, luxury/refined, playful, editorial, brutalist, art deco, soft/pastel, industrial
- **Constraints:** Framework, performance, accessibility requirements
- **Differentiation:** What makes this UNFORGETTABLE?

**CRITICAL:** Choose clear conceptual direction; execute with precision. Bold and minimal both work—key is intentionality.

## Frontend Aesthetics
- **Typography:** Beautiful, unique fonts. Avoid generic (Arial, Inter). Distinctive display + refined body
- **Color & Theme:** Cohesive aesthetic via CSS variables. Dominant colors with sharp accents
- **Motion:** High-impact moments: orchestrated page loads, staggered reveals, scroll-triggering, surprise hover states
- **Spatial Composition:** Unexpected layouts, asymmetry, overlap, diagonal flow, grid-breaking, generous negative space OR controlled density
- **Backgrounds & Visual Details:** Gradient meshes, noise textures, geometric patterns, layered transparencies, dramatic shadows, decorative borders, custom cursors, grain overlays
- **Pairing:** High contrast. Display + monospace, serif + geometric sans, variable font weights
- **Use extremes:** 100/200 vs 800/900 weight, not 400 vs 600. Size jumps 3x+, not 1.5x

**NEVER** use: Inter, Roboto, Arial, system fonts; purple gradients on white; predictable layouts; cookie-cutter design lacking context-specific character.

Vary between light/dark themes, different fonts, different aesthetics. NEVER converge on common choices (Space Grotesk) across generations.

**Match complexity to vision:** Maximalist needs elaborate code; minimalist needs restraint & precision.

## Design Tokens
Single source canonical set:
- **Color:** Brand primary + semantic (success, warning, error, info) + neutral grayscale (5-7 steps)
- **Typography:** Modular scale (H1→Caption) with limited weights; line-height 1.5-1.7
- **Spacing:** Base unit (8px) with multiplicative scale (4,8,12,16,24,32...)
- **Radius:** Small (inputs/buttons), Medium (cards/modals). No ad-hoc pixels
- **Dark Mode Parity:** Accessible dark equivalents (AA+ contrast)

Consume via variables—no hard-coded literals.

## Component States
Explicit states: default, hover, focus (visible outline), active, disabled, loading
- State Feedback <100ms
- Focus Outline Contrast ≥3:1
- Disabled vs ReadOnly clearly differentiated

## Layout & Hierarchy
- **Grid:** Responsive 12-column; mobile-first
- **Visual Hierarchy:** Size/weight & spacing establish priority
- **Whitespace:** Intentional breathing room
- **Alignment:** Left for text, right for numeric
- **Shell:** Persistent sidebar + top bar (optional) + scrollable content

## Interaction & Motion
- **Micro-Interactions:** 150-300ms easing; never obstruct flow
- **Loading:** Skeletons >300ms; inline spinners for local actions
- **Motion Reduction:** Honor `prefers-reduced-motion`
- **Keyboard:** All actionable elements reachable; roving tabindex for composites

## Font Examples
- Code: JetBrains Mono, Fira Code, Space Grotesk
- Editorial: Playfair Display, Crimson Pro
- Technical: IBM Plex, Source Sans 3
- Distinctive: Bricolage Grotesque, Newsreader

## Module-Specific Patterns
### Media Moderation
- Prominent preview with status badge
- Bulk actions: multi-select + contextual toolbar
- Keyboard shortcuts for throughput
- Optional dark theme

### Data Tables
- Left-align text, right-align numbers, bold headers
- Sort indicators, filter controls, global search
- Pagination preferred; virtual scroll only when validated
- Expandable details, inline edits, bulk selection

### Configuration Panels
- Precise labels + concise helper text
- Group via headings or tabs
- Progressive disclosure for advanced options
- Immediate save confirmation; explicit errors
- Reset & defaults; live preview

## RPG Aesthetic
- Fantasy color palettes with dramatic tones
- Ornate borders, decorative frames
- Parchment textures, leather-bound styling, weathered materials
- Epic atmosphere with dramatic lighting
- Medieval serif typography with embellished headers

## Wireframe Fidelity Standards

### Low Fidelity (Grayscale Wireframes)
**Purpose:** Layout structure and information hierarchy validation only

**Visual Constraints:**
- **Colors:** Grayscale ONLY (#000000, #666666, #CCCCCC, #FFFFFF)
- **Typography:** System fonts only (Arial/Helvetica) - no design token fonts
- **Shapes:** Basic rectangles, circles, lines - no decorative elements
- **Images:** Gray boxes with "X" or wavy line placeholders
- **Text:** "Lorem ipsum" or single-line placeholders
- **Spacing:** Approximate spacing - no design token precision
- **Prohibited:** Colors, shadows, gradients, brand logos, custom fonts, visual styling

**Output:** Structural blueprint for content organization and navigation flow

### High Fidelity (Production-Ready Mockups)
**Purpose:** Development-ready visual specifications with complete design system application

**Requirements:**
- **Design Tokens:** Apply ALL tokens (Color, Typography, Spacing, Radius, Dark Mode) - see Design Tokens section above
- **Component States:** Implement all states (default, hover, focus, active, disabled, loading) - see Component States section above
- **Typography:** Use complete type system with modular scale (H1→Caption), proper line-heights (1.5-1.7)
- **Accessibility:** WCAG AA compliance (4.5:1 text contrast, 3:1 UI contrast, focus states ≥3:1)
- **Motion:** Transition specifications per Interaction & Motion section (150-300ms easing, honor prefers-reduced-motion)
- **Branding:** Full brand asset integration (logos, iconography, imagery guidelines per Frontend Aesthetics)
- **Content:** Realistic text length, proper image dimensions, branded visualizations
- **Anti-Patterns:** Avoid prohibited patterns from Frontend Aesthetics section (Line 33)

**Deliverable:** Pixel-perfect mockup indistinguishable from final implementation
