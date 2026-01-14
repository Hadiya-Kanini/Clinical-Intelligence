# Information Architecture - [Application Name]

## 1. Wireframe Specification

**Fidelity Level**: [Low/High]
**Screen Type**: [Web/Mobile/Tablet/Responsive]
**Viewport**: [Width x Height]

## 2. System Overview
[Description of the application, its purpose, and key functionalities]

## 3. Wireframe References

### Generated Wireframes
**Figma Wireframes** (if applicable):
| Screen/Feature | Figma Frame/URL | Description | Fidelity | Date Created |
|---------------|-----------------|-------------|----------|--------------|
| [Screen Name] | [Figma URL or N/A] | [Brief description] | Low/High | [YYYY-MM-DD] |

**HTML/Image Wireframes**:
| Screen/Feature | File Path | Description | Fidelity | Date Created |
|---------------|-----------|-------------|----------|--------------|
| [Screen Name] | [./wireframe-{name}.html] | [Brief description] | Low/High | [YYYY-MM-DD] |

### Component Inventory
**Reference**: See [Component Inventory](./component-inventory.md) for detailed component documentation including:
- Complete component specifications
- Component states and variants
- Responsive behavior details
- Reusability analysis
- Implementation priorities

## 4. User Personas & Flows

### Persona 1: [Name]
- **Role**: [User role/type]
- **Goals**: [Primary goals]
- **Key Screens**: [List of screens this persona interacts with]
- **Primary Flow**: [Entry point] -> [Step 1] -> [Step 2] -> [Goal completion]
- **Wireframe References**: [Links to relevant wireframe files]
- **Decision Points**: [Key decisions this persona makes]

### User Flow Diagrams
- **Primary Flow**: [Flow name - Reference to wireframe file or Figma URL]
- **Secondary Flow**: [Flow name - Reference to wireframe file or Figma URL]

## 5. Screen Hierarchy

### Level 1: [Primary Category - e.g., Main Navigation]
- **Screen A** (P0 - Critical) - [Wireframe: wireframe-screenA.html]
  - Description: [Brief description]
  - User Entry Point: [Yes/No]
  - Key Components: [List from Component Inventory]

- **Screen B** (P1 - High Priority) - [Wireframe: wireframe-screenB.html]
  - Description: [Brief description]
  - Parent Screen: [Screen A]
  - Key Components: [List from Component Inventory]

### Level 2: [Secondary Category - e.g., Account Section]
- **Screen C** (P2 - Medium Priority) - [Wireframe: wireframe-screenC.html]
- **Screen D** (P3 - Low Priority) - [Wireframe: wireframe-screenD.html]

### Screen Priority Legend
- **P0**: Critical path screens (must-have)
- **P1**: High-priority screens (core functionality)
- **P2**: Medium-priority screens (important features)
- **P3**: Low-priority screens (nice-to-have)

### Modal/Dialog/Overlay Inventory
**Purpose:** Systematically document all overlay elements (modals, dialogs, drawers, popovers) that are not full-page screens but require wireframes.

| Modal/Dialog Name | Type | Trigger Context | Parent Screen | Wireframe Reference | Priority |
|------------------|------|----------------|---------------|-------------------|----------|
| [Login Modal] | Modal | Click "Login" button | Homepage, All protected pages | [./wireframe-login-modal.html] | P0 |
| [Confirmation Dialog] | Dialog | Delete/Logout actions | Multiple screens | [./wireframe-confirm-dialog.html] | P0 |
| [Image Lightbox] | Overlay | Click product image | Product Detail, Gallery | [./wireframe-image-lightbox.html] | P2 |
| [Filter Drawer] | Drawer | Click "Filters" (mobile) | Product Listing | [./wireframe-filter-drawer.html] | P1 |

**Modal Behavior Notes:**
- **Responsive Behavior:** Desktop modal vs. mobile full-screen transformation
- **Trigger Actions:** User actions that open each modal/dialog
- **Dismissal Actions:** Close button, overlay click, ESC key, successful action
- **Focus Management:** Tab trap within modal, return focus on close
- **Accessibility:** ARIA role="dialog", aria-labelledby, aria-describedby

**Common Modal Categories:**
- Authentication: Login, Signup, Password Reset, 2FA
- Confirmations: Delete, Logout, Discard Changes, Submit
- Media: Image/Video Lightbox, Gallery Viewer
- Filters/Search: Advanced Filters (mobile drawer), Search Overlay
- Notifications: Success/Error Messages, Cookie Consent
- Forms: Quick Add, Inline Edit, Share Dialog

## 6. Navigation Architecture

```
[Navigation tree/sitemap with wireframe references]

Homepage (wireframe-homepage.html)
+-- Product Listing (wireframe-products.html)
|   +-- Product Detail (wireframe-product-detail.html)
|       +-- Add to Cart -> Cart (wireframe-cart.html)
+-- About (wireframe-about.html)
+-- Account (wireframe-account.html)
|   +-- Profile (wireframe-profile.html)
|   +-- Orders (wireframe-orders.html)
|   +-- Settings (wireframe-settings.html)
+-- Help (wireframe-help.html)
```

### Navigation Patterns
- **Primary Navigation**: [Description and wireframe reference]
- **Secondary Navigation**: [Description and wireframe reference]
- **Mobile Navigation**: [Description of adaptive behavior]

## 7. Interaction Patterns

### Pattern 1: [Pattern Name - e.g., Add to Cart]
- **Trigger**: [User action]
- **Flow**: [Step-by-step interaction]
- **Screens Involved**: [List with wireframe references]
- **Feedback**: [System response]
- **Components Used**: [Reference Component Inventory]

### Pattern 2: [Pattern Name - e.g., Form Validation]
- **Trigger**: [User action]
- **Flow**: [Step-by-step interaction]
- **Screens Involved**: [List with wireframe references]
- **Feedback**: [System response]
- **Components Used**: [Reference Component Inventory]

## 8. Error Handling

### Error Scenario 1: [Error Type - e.g., Network Error]
- **Trigger**: [What causes this error]
- **Error Screen/State**: [Wireframe reference or component state]
- **User Action**: [What user can do]
- **Recovery Flow**: [Steps to recover]

### Error Scenario 2: [Error Type - e.g., Form Validation Error]
- **Trigger**: [What causes this error]
- **Error Screen/State**: [Wireframe reference or component state]
- **User Action**: [What user can do]
- **Recovery Flow**: [Steps to recover]

## 9. Responsive Strategy

| Breakpoint | Width | Layout Changes | Navigation Changes | Component Adaptations |
|-----------|-------|----------------|-------------------|---------------------|
| Mobile | 375px | [Single column, stacked] | [Hamburger menu] | [See Component Inventory] |
| Tablet | 768px | [2-column grid] | [Collapsed nav] | [See Component Inventory] |
| Desktop | 1440px | [Multi-column] | [Expanded nav] | [See Component Inventory] |

### Responsive Wireframe Variants
- Mobile variants: [List wireframe files for mobile views]
- Tablet variants: [List wireframe files for tablet views]
- Desktop variants: [List wireframe files for desktop views]

## 10. Accessibility

### WCAG Compliance
- **Target Level**: AA
- **Color Contrast**: [Validation approach for wireframe-to-implementation]
- **Keyboard Navigation**: [See Interaction Patterns section]
- **Screen Reader Support**: [ARIA labels and announcements - see Component Inventory]

### Accessibility Considerations by Screen
| Screen | Key Accessibility Features | Wireframe Notes |
|--------|---------------------------|----------------|
| [Screen Name] | [Focus management, ARIA labels, etc.] | [Reference] |

### Focus Order
- [Description of tab order strategy across key screens with wireframe references]

## 11. Content Strategy

### Content Hierarchy
- **H1**: [Usage and placement]
- **H2**: [Usage and placement]
- **Body Text**: [Guidelines]
- **Placeholder Content**: [How placeholders are used in wireframes]

### Content Types by Screen
| Screen | Content Types | Wireframe Reference |
|--------|--------------|-------------------|
| [Screen] | [Text, Images, Forms, Data] | [Link] |