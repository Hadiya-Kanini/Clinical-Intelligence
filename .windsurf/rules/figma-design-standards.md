---
trigger: glob
globs: "**/figma_spec_*.md, **/figma_structure.json, **/designsystem.md, **/component_library.md"
---

# Figma Design Standards

## File Structure

### Page Organization (6-Page Standard)
- **00_Cover**: Project metadata, version, stakeholders, last updated
- **01_Foundations**: Design tokens, grids, typography, colors, spacing, elevation
- **02_Components**: Component library with variants (C/<Category>/<Name>)
- **03_Patterns**: Reusable layout patterns built from components
- **04_Screens**: All screens with required states (Default/Loading/Empty/Error/Validation)
- **05_Prototype**: Interactive flows wired end-to-end
- **06_Handoff**: Developer notes, token rules, responsive specs

### Naming Conventions
- Components: `C/<Category>/<Name>` (e.g., C/Button/Primary)
- Screens: `<ScreenName>/<State>` (e.g., SignIn/Default, SignIn/Error)
- Exports: `<AppName>__<Platform>__<ScreenName>__<State>__v1.jpg`

## Auto Layout Standards

### Core Principles
- **Auto Layout First**: All frames MUST use Auto Layout (no absolute positioning except overlays)
- **Hug vs Fill**: Document resizing behavior for every frame
  - Hug: Content determines size
  - Fill: Parent determines size
- **Spacing Consistency**: Use spacing tokens only (4, 8, 12, 16, 20, 24, 32, 40...)
- **Padding Rules**: Use consistent padding from spacing scale
- **Nested Auto Layout**: Limit nesting to 4 levels for performance

### Frame Size by Platform
| Platform | Width | Height | Notes |
|----------|-------|--------|-------|
| Mobile | 390px | 844px | iPhone 14 baseline |
| Tablet | 768px | 1024px | iPad portrait |
| Web | 1440px | 1024px | Desktop baseline |
| Responsive | All | Variable | Include all breakpoints |

## Component Standards

### Variant Properties (Required)
Every component MUST define:
- **Type**: Primary/Secondary/Tertiary/Ghost
- **Size**: Small/Medium/Large (or XS/S/M/L/XL)
- **State**: Default/Hover/Focus/Active/Disabled/Loading
- **Icon**: None/Leading/Trailing/Both (where applicable)

### Component Categories
| Category | Components |
|----------|------------|
| Actions | Button, IconButton, FAB, Link |
| Inputs | TextField, TextArea, Select, Checkbox, Radio, Toggle, Slider |
| Navigation | Header, Sidebar, Tabs, Breadcrumb, Pagination, BottomNav |
| Content | Card, ListItem, Table, Avatar, Badge, Tag, Chip |
| Feedback | Modal, Drawer, Toast, Alert, Tooltip, Popover, Skeleton |
| Layout | Container, Grid, Divider, Spacer |

### State Implementation
| State | Visual Treatment |
|-------|-----------------|
| Default | Base styling per design tokens |
| Hover | Subtle elevation/color shift (150ms ease) |
| Focus | Visible outline (>=3:1 contrast, 2px offset) |
| Active | Pressed/depressed visual |
| Disabled | 40% opacity, no pointer events |
| Loading | Skeleton or spinner, preserve dimensions |

## Design Token Standards

### Token Hierarchy (from designsystem.md)
```
1. Primitive Tokens (raw values)
   color.blue.500: #3B82F6
2. Semantic Tokens (purpose)
   color.primary: {color.blue.500}
3. Component Tokens (specific)
   button.primary.background: {color.primary}
```

### Required Token Categories
- **Color**: Primary, Secondary, Semantic (success/warning/error/info), Neutral (50-900)
- **Typography**: Font families, Size scale (H1-Caption), Weights, Line-heights
- **Spacing**: Base unit (8px), Scale (4, 8, 12, 16, 20, 24, 32, 40, 48, 64)
- **Radius**: Small (4px), Medium (8px), Large (16px), Full (9999px)
- **Elevation**: Levels 1-5 with shadow definitions
- **Motion**: Duration (150ms, 300ms, 500ms), Easing (ease-out, ease-in-out)

### Mode Support
- Light mode (required)
- Dark mode (required for AA compliance)
- High contrast mode (optional, recommended)

## Screen State Standards

### Required States Per Screen
Every screen frame MUST include:

| State | Purpose | Content |
|-------|---------|---------|
| Default | Normal loaded state | Real content, full functionality |
| Loading | Data fetching | Skeleton screens, preserve layout |
| Empty | No data available | Illustration, message, CTA |
| Error | Operation failed | Error message, retry action |
| Validation | Form errors | Inline errors, field highlighting |

### State Naming Convention
`<ScreenName>/<State>` (e.g., `Dashboard/Default`, `Dashboard/Loading`)

## Prototype Standards

### Required Interactions
- Minimum one validation flow (form submit with errors)
- Minimum one empty state flow (trigger -> display -> recovery)
- Minimum one error retry flow (error -> retry -> success)

### Overlay Usage
| Element | Overlay Type | Animation |
|---------|-------------|-----------|
| Modal | Centered overlay | Fade + scale |
| Drawer | Slide from edge | Slide |
| Toast | Top/bottom fixed | Slide + fade |
| Bottom Sheet | Slide from bottom | Slide |
| Dropdown | Below trigger | Fade |

### Flow Documentation
For each prototype flow:
- Entry point (trigger screen/action)
- Step sequence with screen transitions
- Decision points (branches)
- Exit conditions (success/cancel/error)

## Export Standards

### JPG Export Settings
- **Format**: JPG (JPEG)
- **Quality**: High (80-90%)
- **Scale**: 2x for mobile, 1.5x or 2x for web
- **Color Profile**: sRGB

### Export Naming Convention
`<AppName>__<Platform>__<ScreenName>__<State>__v<Version>.jpg`

Examples:
- `MyApp__Mobile__SignIn__Default__v1.jpg`
- `MyApp__Web__Dashboard__Loading__v1.jpg`
- `MyApp__Mobile__Cart__Empty__v1.jpg`

### Export Manifest
Generate `export_manifest.md` listing:
- All exported files with full paths
- Screen name, state, platform
- Export timestamp
- File dimensions

## Accessibility in Figma

### Design-Time Checks
- [ ] Color contrast >=4.5:1 (text) and >=3:1 (UI)
- [ ] Focus states defined for all interactive elements
- [ ] Touch targets >=44x44px (mobile)
- [ ] Text resizable (no text in images)
- [ ] Icons have text labels or tooltips
- [ ] Error states use more than color alone

### Annotation Requirements
- Focus order annotations
- ARIA label suggestions
- Heading hierarchy notes
- Landmark region identification

## Handoff Documentation

### Required Dev Notes (06_Handoff)
1. **Token Usage Rules**: When to use which token
2. **Component Usage**: Props, variants, when to use
3. **Responsive Behavior**: Breakpoint changes, layout shifts
4. **Edge Cases**: Max content, truncation, overflow
5. **Accessibility Notes**: Focus management, ARIA
6. **Assumptions**: Decisions made when specs were unclear

### Code Connect (Optional)
If using Figma MCP `get_code_connect_map`:
- Map Figma node IDs to code component paths
- Document prop mappings
- Include usage examples
