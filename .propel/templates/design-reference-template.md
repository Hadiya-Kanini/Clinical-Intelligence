# Design Reference

> **Note**: This template should only be used for User Stories and Tasks that have **UI impact**.
> Backend-only, API-only, or data processing tasks do not require design references.

## UI Impact Assessment
**Has UI Changes**: [ ] Yes [ ] No
- If NO: Skip this design reference section entirely
- If YES: Complete all applicable sections below

## User Story Design Context
**Story ID**: US-[XXX]
**Story Title**: [User Story Title]
**UI Impact Type**: [New UI | UI Enhancement | UI Redesign | Component Update]

### Design Source References
**Choose applicable reference type:**
- **Figma Project**: [https://figma.com/file/PROJECT_ID/PROJECT_NAME] (if using Figma)
- **Design Images**: [Path to design images/screenshots] (if using static images)
- **Design System**: [Link to design system documentation]
- **Brand Guidelines**: [Link to brand guidelines]

### Screen-to-Design Mappings
**Option A: Figma Frame Mappings**
| Screen/Feature | Figma Frame ID | Direct Link | Description | Implementation Priority |
|---------------|---------------|-------------|-------------|----------------------|
| [Screen Name] | node-id=2:45 | [Figma Link](https://figma.com/file/xxx?node-id=2:45) | [Description] | High/Medium/Low |

**Option B: Design Image References**
| Screen/Feature | Image File | Image Path | Description | Implementation Priority |
|---------------|------------|------------|-------------|----------------------|
| [Login Screen] | login_design.png | .propel/context/Design/images/login_design.png | Login interface design | High/Medium/Low |
| [Dashboard] | dashboard_layout.jpg | .propel/context/Design/images/dashboard_layout.jpg | Dashboard layout | High/Medium/Low |

### Design Tokens
```yaml
# Only include tokens that are used in this specific UI implementation
colors:
  primary:
    value: "#007AFF"
    usage: "Primary CTAs, active states"
    affected_components: ["Button", "Links"]

typography:
  heading1:
    family: "SF Pro Display"
    size: "32px"
    weight: "600"
    line-height: "40px"
    used_in: ["Page Headers", "Modal Titles"]

spacing:
  base: "8px"
  affected_layouts: ["Form spacing", "Card padding"]
```

### Component References
**Option A: Figma Component References**
| Component Name | Figma Component | Code Location | UI Changes Required |
|---------------|-----------------|---------------|-------------------|
| Button | node-id=C:123 | components/Button.tsx | New variant: 'outline' |
| Input Field | node-id=C:456 | components/Input.tsx | Add validation styling |

**Option B: Image-Based Component References**
| Component Name | Reference Image | Code Location | UI Changes Required |
|---------------|-----------------|---------------|-------------------|
| Button | components/button_variants.png | components/Button.tsx | New variant: 'outline' |
| Input Field | components/input_states.png | components/Input.tsx | Add validation styling |

### New Visual Assets
```yaml
screenshots:
  location: ".propel/context/Design/US-[XXX]/"
  files:
    - name: "before_after.png"
      description: "Comparison of current vs new UI"
      source: "figma_frame: node-id=2:45" OR "design_image: original_design.png"

new_assets:
  icons:
    - name: "new_icon.svg"
      source: "figma_export: node-id=I:789" OR "image_file: icon_design.png"
      purpose: "Search functionality"
  images:
    - name: "hero_banner.jpg"
      source: "design_file: hero_mockup.psd" OR "figma_export: node-id=IMG:456"
      requirements: "1920x600px, optimized for web"
```

### Task Design Mapping
```yaml
# Only include tasks that have UI implementation
TASK-001:
  title: "Implement Login UI"
  ui_impact: true
  visual_references:
    # Use EITHER Figma OR image references
    figma_frames: ["node-id=2:45"]  # Login form
    # OR design_images: [".propel/context/Design/login_screen.png"]
  components_affected:
    - Button (new 'primary' variant)
    - Input Field (add focus states)
  visual_validation_required: true

TASK-002:
  title: "Add User Authentication API"
  ui_impact: false
  # No design references needed - backend only

TASK-003:
  title: "Update Dashboard with New Metrics"
  ui_impact: true
  visual_references:
    # Use EITHER Figma OR image references
    figma_frames: ["node-id=3:120"]  # Updated dashboard
    # OR design_images: [".propel/context/Design/dashboard_update.png", ".propel/context/Design/metrics_layout.jpg"]
  components_affected:
    - MetricCard (new layout)
  visual_validation_required: true
```

### Visual Validation Criteria
```typescript
// Only apply visual validation for UI-impacting tasks
const requiresVisualValidation = task.ui_impact === true;

if (requiresVisualValidation) {
  const visualValidation = {
    screenshotComparison: {
      maxDifference: "5%",
      breakpoints: [375, 768, 1440]
    },
    componentValidation: {
      colorAccuracy: true,
      spacingAccuracy: true,
      typographyMatch: true
    }
  };
}
```

### Implementation Scenarios

#### For New UI Components
```yaml
new_components:
  - name: "SearchBar"
    figma_reference: "node-id=C:456"
    file_location: "components/SearchBar.tsx"
    design_specifications:
      width: "100% with max-width: 600px"
      height: "48px"
      border_radius: "8px"
      states: ["default", "focused", "error"]
```

#### For UI Enhancements
```yaml
ui_enhancements:
  existing_component: "Button"
  changes_required:
    - "Add loading state with spinner"
    - "Update hover animation to 200ms"
    - "Add disabled state styling"
  figma_reference: "node-id=C:123-updated"
```

#### For Backend/API Tasks (No Design Needed)
```yaml
backend_task:
  ui_impact: false
  design_references: "Not applicable - API/Backend implementation only"
  validation_type: "Unit tests and API testing"
```

### Accessibility Requirements
- **WCAG Level**: AA (only for UI modifications)
- **Color Contrast**: Validate only new/modified UI elements
- **Focus States**: Only for interactive components being changed
- **Screen Reader**: ARIA labels for new UI elements only

### Design Review Checklist
**Complete only if ui_impact: true**
- [ ] Figma frames reviewed for all UI changes
- [ ] Design tokens extracted for affected components
- [ ] Component specifications documented
- [ ] Visual validation criteria defined
- [ ] Responsive behavior specified
- [ ] Accessibility requirements noted

**Skip if ui_impact: false**
- No design review required for backend/API tasks