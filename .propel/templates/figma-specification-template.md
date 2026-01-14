# Figma Design Specification - [AppName]

## 1. Figma Specification
**Platform**: [Web/iOS/Android/Responsive]

---

## 2. Source References

### Primary Source
| Document | Path | Purpose |
|----------|------|---------|
| Requirements | `.propel/context/docs/spec.md` | Personas, use cases, epics with UI impact flags |

### Optional Sources
| Document | Path | Purpose |
|----------|------|---------|
| Wireframes | `.propel/context/wireframes/` | Entity understanding, content structure |
| Design Assets | `.propel/context/Design/` | Visual references from spec.md epics |

### Related Documents
| Document | Path | Purpose |
|----------|------|---------|
| Design System | `.propel/context/docs/designsystem.md` | Tokens, branding, component specifications |

---

## 3. UX Requirements

*Generated based on use cases with UI impact. These requirements apply to screen implementations and are only created when UI impact exists.*

### UXR Requirements Table
| UXR-ID | Category | Requirement | Acceptance Criteria | Screens Affected |
|--------|----------|-------------|---------------------|------------------|
| UXR-XXX | [Category] | System MUST [requirement] | [Measurable criteria] | SCR-XXX |

*[Add UXR for each UX requirement derived from use cases. Mark unclear requirements with `[UNCLEAR]` tag.]*

### UXR Categories
- **Usability**: Navigation, discoverability, efficiency (max 3 clicks, clear hierarchy)
- **Accessibility**: WCAG 2.2 AA compliance, assistive technology support
- **Responsiveness**: Breakpoint behavior, viewport adaptation
- **Visual Design**: Design system adherence, consistency
- **Interaction**: Feedback, loading states, animations (response within 200ms)
- **Error Handling**: Error messages, recovery paths

### UXR Derivation Logic
- **Usability UXR**: Derived from UC-XXX success paths (navigation depth, discoverability)
- **Accessibility UXR**: Derived from WCAG 2.2 AA standards + designsystem.md constraints
- **Responsiveness UXR**: Derived from platform targets + breakpoint definitions
- **Visual Design UXR**: Derived from designsystem.md token requirements
- **Interaction UXR**: Derived from flow complexity + state transitions
- **Error Handling UXR**: Derived from UC-XXX alternative/exception paths

### UXR Numbering Convention
- UXR-001 to UXR-099: Project-wide requirements
- UXR-1XX: Usability requirements
- UXR-2XX: Accessibility requirements
- UXR-3XX: Responsiveness requirements
- UXR-4XX: Visual design requirements
- UXR-5XX: Interaction requirements
- UXR-6XX: Error handling requirements

---

## 4. Personas Summary

*Derived from spec.md - Reference only, do not duplicate full persona details*

| Persona | Role | Primary Goals | Key Screens |
|---------|------|---------------|-------------|
| [Name] | [Role] | [Goals summary] | [Screen list] |
| [Name] | [Role] | [Goals summary] | [Screen list] |

---

## 5. Information Architecture

### Site Map
```
[AppName]
+-- [Primary Section 1]
|   +-- [Screen A]
|   +-- [Screen B]
+-- [Primary Section 2]
|   +-- [Screen C]
|   +-- [Screen D]
+-- [Primary Section 3]
    +-- [Screen E]
```

### Navigation Patterns
| Pattern | Type | Platform Behavior |
|---------|------|-------------------|
| Primary Nav | [Sidebar/Header/Bottom] | [Desktop: Sidebar / Mobile: Bottom Nav] |
| Secondary Nav | [Tabs/Breadcrumb] | [Description] |
| Utility Nav | [User menu/Settings] | [Description] |

---

## 6. Screen Inventory

*All screens derived from use cases in spec.md*

### Screen List
| Screen ID | Screen Name | Derived From | Personas Covered | Priority | States Required |
|-----------|-------------|--------------|------------------|----------|-----------------|
| SCR-001 | [Screen 1] | UC-001 | [Persona 1, Persona 2] | P0 | Default, Loading, Empty, Error, Validation |
| SCR-002 | [Screen 2] | UC-002, UC-003 | [All] | P0 | Default, Loading, Empty, Error, Validation |
| SCR-003 | [Screen 3] | UC-004 | [Persona 1] | P1 | Default, Loading, Empty, Error, Validation |
| SCR-004 | [Screen 4] | UC-005 | [Persona 2] | P2 | Default, Loading, Empty, Error, Validation |

### Priority Legend
- **P0**: Critical path (must-have for MVP)
- **P1**: Core functionality (high priority)
- **P2**: Important features (medium priority)
- **P3**: Nice-to-have (low priority)

### Screen-to-Persona Coverage Matrix
| Screen | Persona 1 | Persona 2 | Persona N | Notes |
|--------|-----------|-----------|-----------|-------|
| [Screen 1] | Primary | Secondary | - | Entry point for all users |
| [Screen 2] | Primary | Primary | - | Shared dashboard |
| [Screen 3] | Primary | - | - | Persona 1 specific |

### Modal/Overlay Inventory
| Name | Type | Trigger | Parent Screen(s) | Priority |
|------|------|---------|-----------------|----------|
| [Login Modal] | Modal | Click "Login" | Header, Protected pages | P0 |
| [Confirm Delete] | Dialog | Delete action | Multiple screens | P0 |
| [Filter Drawer] | Drawer | Click "Filters" (mobile) | List screens | P1 |

---

## 7. Content & Tone

### Voice & Tone
- **Overall Tone**: [Professional / Friendly / Technical / Casual]
- **Error Messages**: [Helpful, non-blaming, actionable]
- **Empty States**: [Encouraging, guiding, with clear CTA]
- **Success Messages**: [Brief, celebratory, next-action oriented]

### Content Guidelines
- **Headings**: [Title case / Sentence case]
- **CTAs**: [Action-oriented, specific verbs]
- **Labels**: [Concise, descriptive]
- **Placeholder Text**: [Helpful examples, not "Lorem ipsum" in final]

---

## 8. Data & Edge Cases

### Data Scenarios
| Scenario | Description | Handling |
|----------|-------------|----------|
| No Data | User has no items/records | Empty state with CTA |
| First Use | New user, no history | Onboarding flow |
| Large Data | 1000+ items | Pagination/virtualization |
| Slow Connection | >3s load time | Skeleton screens |
| Offline | No network | Offline state, cached data |

### Edge Cases
| Case | Screen(s) Affected | Solution |
|------|-------------------|----------|
| Long text | All with text content | Truncation with tooltip |
| Missing image | Cards, profiles | Fallback placeholder |
| Form validation | All forms | Inline error messages |
| Session timeout | All authenticated | Modal with re-login |

---

## 9. Branding & Visual Direction

*See `designsystem.md` for all design tokens (colors, typography, spacing, shadows, etc.)*

### Branding Assets
- **Logo**: [Path or description]
- **Icon Style**: [Outlined / Filled / Duotone]
- **Illustration Style**: [Flat / Isometric / Hand-drawn / None]
- **Photography Style**: [If applicable]

---

## 10. Component Specifications

*Component specifications defined in designsystem.md. Requirements per screen listed below.*

### Component Library Reference
**Source**: `.propel/context/docs/designsystem.md` (Component Specifications section)

### Required Components per Screen
| Screen ID | Components Required | Notes |
|-----------|---------------------|-------|
| SCR-001 | TextField (2), Button (2), Link (1) | Login form |
| SCR-002 | Card (N), Table (1), Header (1) | Dashboard, dynamic card count |
| SCR-003 | TextField (5), Select (2), Button (2) | Profile edit form |

### Component Summary
| Category | Components | Variants |
|----------|------------|----------|
| Actions | Button | Primary, Secondary, Tertiary, Ghost x S/M/L x States |
| Inputs | TextField, Select, Checkbox, Radio, Toggle | States + Sizes |
| Navigation | Header, Sidebar, Tabs, BottomNav | Platform variants |
| Content | Card, ListItem, Table | Content variants |
| Feedback | Modal, Drawer, Toast, Alert, Skeleton | Types + States |

### Component Constraints
- Use only components from designsystem.md
- No custom components without approval
- All components must support all defined states (Default, Hover, Focus, Active, Disabled, Loading)
- Follow naming convention: `C/<Category>/<Name>`

---

## 11. Prototype Flows

*Flows derived from use cases in spec.md. Each flow notes which personas it covers.*

### Flow: FL-XXX - [Flow Name]
**Flow ID**: FL-XXX
**Derived From**: UC-XXX
**Personas Covered**: [Personas]
**Description**: [Brief flow purpose]

#### Flow Sequence
```
1. Entry: [Screen Name] / Default
   - Trigger: [User action or system event]
   |
   v
2. Step: [Screen Name] / [State]
   - Action: [What happens]
   |
   v
3. Decision Point:
   +-- Success -> [Screen Name] / Default
   +-- Error -> [Screen Name] / Error
```

#### Required Interactions
- [Interaction]: [Description]

*[Repeat FL-XXX structure for each flow derived from UC-XXX in spec.md]*

---

## 12. Export Requirements

### JPG Export Settings
| Setting | Value |
|---------|-------|
| Format | JPG |
| Quality | High (85%) |
| Scale - Mobile | 2x |
| Scale - Web | 2x |
| Color Profile | sRGB |

### Export Naming Convention
`<AppName>__<Platform>__<ScreenName>__<State>__v<Version>.jpg`

### Export Manifest
| Screen | State | Platform | Filename |
|--------|-------|----------|----------|
| [Screen 1] | Default | Mobile | [AppName]__Mobile__[Screen1]__Default__v1.jpg |
| [Screen 1] | Loading | Mobile | [AppName]__Mobile__[Screen1]__Loading__v1.jpg |
| [Screen 1] | Empty | Mobile | [AppName]__Mobile__[Screen1]__Empty__v1.jpg |
| [Screen 1] | Error | Mobile | [AppName]__Mobile__[Screen1]__Error__v1.jpg |
| [Screen 2] | Default | Web | [AppName]__Web__[Screen2]__Default__v1.jpg |

### Total Export Count
- **Screens**: [X]
- **States per screen**: [5 average]
- **Total JPGs**: [X * 5 = Y]

---

## 13. Figma File Structure

### Page Organization
```
[AppName] Figma File
+-- 00_Cover
|   +-- Project info, version, stakeholders
+-- 01_Foundations
|   +-- Color tokens (Light + Dark)
|   +-- Typography scale
|   +-- Spacing scale
|   +-- Radius tokens
|   +-- Elevation/shadows
|   +-- Grid definitions
+-- 02_Components
|   +-- C/Actions/[Button, IconButton, Link, FAB]
|   +-- C/Inputs/[TextField, Select, Checkbox, ...]
|   +-- C/Navigation/[Header, Sidebar, Tabs, ...]
|   +-- C/Content/[Card, ListItem, Table, ...]
|   +-- C/Feedback/[Modal, Drawer, Toast, ...]
+-- 03_Patterns
|   +-- Auth form pattern
|   +-- Search + filter pattern
|   +-- Detail page pattern
|   +-- Error/Empty/Loading patterns
+-- 04_Screens
|   +-- [Screen1]/Default
|   +-- [Screen1]/Loading
|   +-- [Screen1]/Empty
|   +-- [Screen1]/Error
|   +-- [Screen1]/Validation
|   +-- [... all screens with states]
+-- 05_Prototype
|   +-- Flow 1: [Name]
|   +-- Flow 2: [Name]
|   +-- Flow 3: [Name]
+-- 06_Handoff
    +-- Token usage rules
    +-- Component guidelines
    +-- Responsive specs
    +-- Edge cases
    +-- Accessibility notes
```

---

## 14. Quality Checklist

### Pre-Export Validation
- [ ] All screens have required states (Default/Loading/Empty/Error/Validation)
- [ ] All components use design tokens (no hard-coded values)
- [ ] Color contrast meets WCAG AA (>=4.5:1 text, >=3:1 UI)
- [ ] Focus states defined for all interactive elements
- [ ] Touch targets >= 44x44px (mobile)
- [ ] Prototype flows wired and functional
- [ ] Naming conventions followed
- [ ] Export manifest complete

### Post-Generation
- [ ] designsystem.md updated with Figma references
- [ ] Export manifest generated
- [ ] JPG files named correctly
- [ ] Handoff documentation complete
