# Design System - Trust-First Clinical Intelligence Platform

## 1. Design System Overview

**Platform**: Web (Desktop - 1280px+)  
**Design Language**: Professional, clinical, trustworthy  
**Accessibility Target**: WCAG 2.1 AA  
**Color Mode**: Light (Phase 1)

---

## 2. Design Tokens

### 2.1 Color Palette

#### Primary Colors
```yaml
primary:
  50: "#E3F2FD"   # Lightest blue - backgrounds, hover states
  100: "#BBDEFB"  # Light blue - subtle highlights
  200: "#90CAF9"  # Medium-light blue
  300: "#64B5F6"  # Medium blue
  400: "#42A5F5"  # Medium-dark blue
  500: "#2196F3"  # Primary blue - main CTAs, links
  600: "#1E88E5"  # Darker blue - hover states
  700: "#1976D2"  # Dark blue - active states
  800: "#1565C0"  # Darker blue
  900: "#0D47A1"  # Darkest blue - text on light backgrounds

usage:
  - Primary CTAs (buttons, links)
  - Active navigation items
  - Focus indicators
  - Interactive elements
```

#### Semantic Colors
```yaml
success:
  light: "#E8F5E9"   # Success background
  main: "#4CAF50"    # Success text, icons
  dark: "#388E3C"    # Success hover

warning:
  light: "#FFF3E0"   # Warning background
  main: "#FF9800"    # Warning text, icons
  dark: "#F57C00"    # Warning hover

error:
  light: "#FFEBEE"   # Error background
  main: "#F44336"    # Error text, icons
  dark: "#D32F2F"    # Error hover

info:
  light: "#E3F2FD"   # Info background
  main: "#2196F3"    # Info text, icons
  dark: "#1976D2"    # Info hover

usage:
  - Success: Verified data, completed actions, accepted codes
  - Warning: Unverified data, pending actions, validation warnings
  - Error: Failed actions, rejected codes, conflicts
  - Info: Informational messages, tooltips, help text
```

#### Neutral Colors
```yaml
neutral:
  0: "#FFFFFF"     # White - backgrounds, cards
  50: "#FAFAFA"    # Lightest gray - subtle backgrounds
  100: "#F5F5F5"   # Light gray - hover backgrounds
  200: "#EEEEEE"   # Medium-light gray - borders, dividers
  300: "#E0E0E0"   # Medium gray - disabled backgrounds
  400: "#BDBDBD"   # Medium-dark gray - placeholder text
  500: "#9E9E9E"   # Dark gray - secondary text
  600: "#757575"   # Darker gray - body text
  700: "#616161"   # Very dark gray - headings
  800: "#424242"   # Almost black - primary text
  900: "#212121"   # Black - high-emphasis text

usage:
  - Text hierarchy (900 for headings, 800 for body, 600 for secondary)
  - Backgrounds (0, 50, 100)
  - Borders and dividers (200, 300)
  - Disabled states (300, 400)
```

#### Medical/Clinical Colors
```yaml
clinical:
  red: "#D32F2F"      # Critical alerts, conflicts
  orange: "#F57C00"   # Warnings, pending reviews
  green: "#388E3C"    # Verified, approved
  blue: "#1976D2"     # Information, references
  purple: "#7B1FA2"   # AI-generated content
  teal: "#00897B"     # Processing, in-progress

usage:
  - Status badges (processing, completed, failed)
  - Conflict highlighting
  - Code review states (accepted, rejected, pending)
  - Data verification states
```

### 2.2 Typography

#### Font Families
```yaml
primary: "Inter, -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif"
monospace: "'Roboto Mono', 'Courier New', monospace"

usage:
  - Primary: All UI text, headings, body
  - Monospace: Code snippets, MRN, technical IDs
```

#### Type Scale
```yaml
display:
  size: "48px"
  weight: 700
  line-height: "56px"
  letter-spacing: "-0.5px"
  usage: "Page titles (rare use)"

h1:
  size: "32px"
  weight: 600
  line-height: "40px"
  letter-spacing: "-0.25px"
  usage: "Main page headings (Dashboard, Patient 360 View)"

h2:
  size: "24px"
  weight: 600
  line-height: "32px"
  letter-spacing: "0px"
  usage: "Section headings (Patient Profile, Clinical Content)"

h3:
  size: "20px"
  weight: 600
  line-height: "28px"
  letter-spacing: "0px"
  usage: "Subsection headings (Allergies, Medications)"

h4:
  size: "16px"
  weight: 600
  line-height: "24px"
  letter-spacing: "0.15px"
  usage: "Card titles, modal headers"

body-large:
  size: "16px"
  weight: 400
  line-height: "24px"
  letter-spacing: "0.15px"
  usage: "Primary body text, form labels"

body:
  size: "14px"
  weight: 400
  line-height: "20px"
  letter-spacing: "0.25px"
  usage: "Default body text, table content"

body-small:
  size: "12px"
  weight: 400
  line-height: "16px"
  letter-spacing: "0.4px"
  usage: "Helper text, captions, timestamps"

button:
  size: "14px"
  weight: 500
  line-height: "20px"
  letter-spacing: "0.5px"
  text-transform: "none"
  usage: "All button labels"

label:
  size: "12px"
  weight: 500
  line-height: "16px"
  letter-spacing: "0.5px"
  text-transform: "uppercase"
  usage: "Input labels, section labels"
```

### 2.3 Spacing Scale

**Base Unit**: 8px

```yaml
spacing:
  0: "0px"
  1: "4px"    # 0.5 * base - tight spacing
  2: "8px"    # 1 * base - default spacing
  3: "12px"   # 1.5 * base
  4: "16px"   # 2 * base - comfortable spacing
  5: "20px"   # 2.5 * base
  6: "24px"   # 3 * base - section spacing
  8: "32px"   # 4 * base - large spacing
  10: "40px"  # 5 * base
  12: "48px"  # 6 * base - page margins
  16: "64px"  # 8 * base - extra large spacing
  20: "80px"  # 10 * base

usage:
  - Component padding: spacing-4 (16px)
  - Card padding: spacing-6 (24px)
  - Section margins: spacing-8 (32px)
  - Page margins: spacing-12 (48px)
  - Element gaps: spacing-2 to spacing-4 (8px-16px)
```

### 2.4 Border Radius

```yaml
radius:
  none: "0px"
  sm: "4px"     # Small elements (badges, tags)
  md: "8px"     # Default (buttons, inputs, cards)
  lg: "12px"    # Large cards, modals
  xl: "16px"    # Extra large containers
  full: "9999px" # Pills, avatars

usage:
  - Buttons, inputs: radius-md (8px)
  - Cards: radius-md to radius-lg (8px-12px)
  - Modals: radius-lg (12px)
  - Badges: radius-sm (4px)
  - Avatars: radius-full
```

### 2.5 Elevation (Shadows)

```yaml
elevation:
  0: "none"
  1: "0px 1px 2px rgba(0, 0, 0, 0.05)"
  2: "0px 1px 3px rgba(0, 0, 0, 0.1), 0px 1px 2px rgba(0, 0, 0, 0.06)"
  3: "0px 4px 6px rgba(0, 0, 0, 0.1), 0px 2px 4px rgba(0, 0, 0, 0.06)"
  4: "0px 10px 15px rgba(0, 0, 0, 0.1), 0px 4px 6px rgba(0, 0, 0, 0.05)"
  5: "0px 20px 25px rgba(0, 0, 0, 0.1), 0px 10px 10px rgba(0, 0, 0, 0.04)"
  6: "0px 25px 50px rgba(0, 0, 0, 0.15)"

usage:
  - Cards: elevation-1 or elevation-2
  - Dropdowns, tooltips: elevation-3
  - Modals: elevation-4 or elevation-5
  - Floating action buttons: elevation-3
  - Hover states: increase elevation by 1 level
```

### 2.6 Grid System

```yaml
grid:
  columns: 12
  gutter: "24px"  # spacing-6
  margin: "48px"  # spacing-12
  breakpoints:
    sm: "1280px"  # Minimum supported
    md: "1440px"  # Comfortable
    lg: "1920px"  # Large displays

container:
  max-width: "1440px"
  padding: "48px"  # spacing-12

usage:
  - Main content area: 8-10 columns
  - Sidebar: 2-4 columns
  - Full-width: 12 columns
```

### 2.7 Z-Index Scale

```yaml
z-index:
  base: 0
  dropdown: 1000
  sticky: 1100
  fixed: 1200
  modal-backdrop: 1300
  modal: 1400
  popover: 1500
  tooltip: 1600
  notification: 1700

usage:
  - Modals and overlays: modal-backdrop + modal
  - Dropdowns: dropdown
  - Tooltips: tooltip
  - Toast notifications: notification
```

---

## 3. Component Specifications

### 3.1 Actions

#### Button
**Variants**: Primary, Secondary, Tertiary, Ghost  
**Sizes**: Small (S), Medium (M), Large (L)  
**States**: Default, Hover, Focus, Active, Disabled, Loading

```yaml
button-primary:
  background: primary-500
  color: neutral-0
  padding: "12px 24px"  # M size
  border-radius: radius-md
  font: button
  height: "40px"  # M size
  
  hover:
    background: primary-600
    elevation: elevation-2
  
  focus:
    outline: "2px solid" primary-500
    outline-offset: "2px"
  
  active:
    background: primary-700
  
  disabled:
    background: neutral-300
    color: neutral-500
    cursor: "not-allowed"
  
  loading:
    background: primary-500
    color: neutral-0
    cursor: "wait"
    icon: "spinner animation"

button-secondary:
  background: neutral-0
  color: primary-500
  border: "1px solid" primary-500
  padding: "12px 24px"
  
  hover:
    background: primary-50
    border-color: primary-600

button-tertiary:
  background: neutral-100
  color: neutral-800
  padding: "12px 24px"
  
  hover:
    background: neutral-200

button-ghost:
  background: "transparent"
  color: primary-500
  padding: "12px 24px"
  
  hover:
    background: primary-50

sizes:
  small:
    height: "32px"
    padding: "8px 16px"
    font-size: "12px"
  medium:
    height: "40px"
    padding: "12px 24px"
    font-size: "14px"
  large:
    height: "48px"
    padding: "16px 32px"
    font-size: "16px"
```

#### Link
```yaml
link:
  color: primary-500
  text-decoration: "none"
  font: body
  
  hover:
    color: primary-600
    text-decoration: "underline"
  
  focus:
    outline: "2px solid" primary-500
    outline-offset: "2px"
  
  visited:
    color: "#7B1FA2"  # purple for visited links
```

### 3.2 Inputs

#### TextField
**States**: Default, Focus, Error, Disabled, Success

```yaml
text-field:
  height: "40px"
  padding: "10px 12px"
  border: "1px solid" neutral-300
  border-radius: radius-md
  background: neutral-0
  color: neutral-800
  font: body
  
  placeholder:
    color: neutral-400
  
  focus:
    border-color: primary-500
    outline: "2px solid" primary-500
    outline-offset: "1px"
  
  error:
    border-color: error-main
    background: error-light
  
  success:
    border-color: success-main
  
  disabled:
    background: neutral-100
    color: neutral-500
    cursor: "not-allowed"

label:
  font: label
  color: neutral-700
  margin-bottom: spacing-1

helper-text:
  font: body-small
  color: neutral-600
  margin-top: spacing-1

error-text:
  font: body-small
  color: error-main
  margin-top: spacing-1
```

#### Select
```yaml
select:
  height: "40px"
  padding: "10px 12px"
  border: "1px solid" neutral-300
  border-radius: radius-md
  background: neutral-0
  color: neutral-800
  font: body
  
  icon:
    position: "right"
    color: neutral-600
  
  focus:
    border-color: primary-500
    outline: "2px solid" primary-500

dropdown:
  background: neutral-0
  border: "1px solid" neutral-200
  border-radius: radius-md
  elevation: elevation-3
  max-height: "300px"
  overflow: "auto"

option:
  padding: "10px 12px"
  font: body
  
  hover:
    background: primary-50
  
  selected:
    background: primary-100
    color: primary-700
```

#### Checkbox & Radio
```yaml
checkbox:
  size: "20px"
  border: "2px solid" neutral-400
  border-radius: radius-sm
  
  checked:
    background: primary-500
    border-color: primary-500
    icon: "checkmark" neutral-0
  
  focus:
    outline: "2px solid" primary-500
    outline-offset: "2px"

radio:
  size: "20px"
  border: "2px solid" neutral-400
  border-radius: radius-full
  
  checked:
    border-color: primary-500
    inner-circle: primary-500 "8px"
```

#### FileUpload
```yaml
file-upload:
  drag-drop-zone:
    min-height: "200px"
    border: "2px dashed" neutral-300
    border-radius: radius-lg
    background: neutral-50
    padding: spacing-8
    text-align: "center"
    
    hover:
      border-color: primary-500
      background: primary-50
    
    active:
      border-color: primary-600
      background: primary-100

  file-list:
    margin-top: spacing-4
    
  file-item:
    display: "flex"
    align-items: "center"
    padding: spacing-3
    border: "1px solid" neutral-200
    border-radius: radius-md
    margin-bottom: spacing-2
    
    icon:
      color: primary-500
      margin-right: spacing-2
    
    name:
      flex: 1
      font: body
      color: neutral-800
    
    size:
      font: body-small
      color: neutral-600
      margin-right: spacing-2
    
    remove-button:
      color: error-main
```

#### DateRangePicker
```yaml
date-range-picker:
  input:
    height: "40px"
    padding: "10px 12px"
    border: "1px solid" neutral-300
    border-radius: radius-md
    
  calendar:
    background: neutral-0
    border: "1px solid" neutral-200
    border-radius: radius-md
    elevation: elevation-3
    padding: spacing-4
    
  day:
    width: "36px"
    height: "36px"
    border-radius: radius-sm
    
    hover:
      background: primary-50
    
    selected:
      background: primary-500
      color: neutral-0
    
    in-range:
      background: primary-100
```

### 3.3 Navigation

#### Header
```yaml
header:
  height: "64px"
  background: neutral-0
  border-bottom: "1px solid" neutral-200
  padding: "0" spacing-12
  display: "flex"
  align-items: "center"
  justify-content: "space-between"
  
  logo:
    height: "32px"
  
  user-menu:
    display: "flex"
    align-items: "center"
    gap: spacing-3
```

#### Sidebar
```yaml
sidebar:
  width: "240px"
  background: neutral-50
  border-right: "1px solid" neutral-200
  padding: spacing-6 spacing-4
  
  nav-item:
    padding: spacing-3 spacing-4
    border-radius: radius-md
    color: neutral-700
    font: body
    display: "flex"
    align-items: "center"
    gap: spacing-2
    
    hover:
      background: neutral-100
    
    active:
      background: primary-100
      color: primary-700
      font-weight: 600
    
    icon:
      size: "20px"
```

#### Tabs
```yaml
tabs:
  container:
    border-bottom: "1px solid" neutral-200
    
  tab:
    padding: spacing-3 spacing-4
    color: neutral-600
    font: body
    border-bottom: "2px solid transparent"
    
    hover:
      color: neutral-800
    
    active:
      color: primary-500
      border-bottom-color: primary-500
      font-weight: 600
```

#### Breadcrumb
```yaml
breadcrumb:
  display: "flex"
  align-items: "center"
  gap: spacing-2
  font: body-small
  color: neutral-600
  
  item:
    color: neutral-600
    
    hover:
      color: primary-500
  
  current:
    color: neutral-800
    font-weight: 600
  
  separator:
    color: neutral-400
    content: "/"
```

### 3.4 Content

#### Card
```yaml
card:
  background: neutral-0
  border: "1px solid" neutral-200
  border-radius: radius-lg
  padding: spacing-6
  elevation: elevation-1
  
  hover:
    elevation: elevation-2
  
  header:
    margin-bottom: spacing-4
    
  title:
    font: h4
    color: neutral-800
  
  content:
    font: body
    color: neutral-700
```

#### Table
```yaml
table:
  width: "100%"
  border: "1px solid" neutral-200
  border-radius: radius-md
  overflow: "hidden"
  
  header:
    background: neutral-50
    border-bottom: "1px solid" neutral-200
    
  header-cell:
    padding: spacing-3 spacing-4
    font: label
    color: neutral-700
    text-align: "left"
    
  row:
    border-bottom: "1px solid" neutral-100
    
    hover:
      background: neutral-50
  
  cell:
    padding: spacing-3 spacing-4
    font: body
    color: neutral-800
```

#### Badge
```yaml
badge:
  padding: "4px 8px"
  border-radius: radius-sm
  font: body-small
  font-weight: 500
  display: "inline-flex"
  align-items: "center"
  
  success:
    background: success-light
    color: success-dark
  
  warning:
    background: warning-light
    color: warning-dark
  
  error:
    background: error-light
    color: error-dark
  
  info:
    background: info-light
    color: info-dark
  
  neutral:
    background: neutral-100
    color: neutral-700
```

#### Avatar
```yaml
avatar:
  size: "40px"  # default
  border-radius: radius-full
  background: primary-100
  color: primary-700
  font: body
  font-weight: 600
  display: "flex"
  align-items: "center"
  justify-content: "center"
  
  sizes:
    small: "32px"
    medium: "40px"
    large: "56px"
```

### 3.5 Feedback

#### Modal
```yaml
modal:
  backdrop:
    background: "rgba(0, 0, 0, 0.5)"
    z-index: z-index-modal-backdrop
  
  container:
    background: neutral-0
    border-radius: radius-lg
    elevation: elevation-5
    max-width: "600px"
    z-index: z-index-modal
    
  header:
    padding: spacing-6
    border-bottom: "1px solid" neutral-200
    
  title:
    font: h3
    color: neutral-800
  
  content:
    padding: spacing-6
    max-height: "60vh"
    overflow: "auto"
  
  footer:
    padding: spacing-6
    border-top: "1px solid" neutral-200
    display: "flex"
    justify-content: "flex-end"
    gap: spacing-3
```

#### Alert
```yaml
alert:
  padding: spacing-4
  border-radius: radius-md
  border-left: "4px solid"
  display: "flex"
  align-items: "flex-start"
  gap: spacing-3
  
  success:
    background: success-light
    border-color: success-main
    color: success-dark
  
  warning:
    background: warning-light
    border-color: warning-main
    color: warning-dark
  
  error:
    background: error-light
    border-color: error-main
    color: error-dark
  
  info:
    background: info-light
    border-color: info-main
    color: info-dark
  
  icon:
    size: "20px"
  
  message:
    font: body
    flex: 1
```

#### Toast
```yaml
toast:
  min-width: "300px"
  max-width: "500px"
  background: neutral-800
  color: neutral-0
  padding: spacing-4
  border-radius: radius-md
  elevation: elevation-4
  z-index: z-index-notification
  
  position: "top-right"
  animation: "slide-in 0.3s ease"
  
  success:
    background: success-dark
  
  error:
    background: error-dark
  
  warning:
    background: warning-dark
```

#### ProgressBar
```yaml
progress-bar:
  height: "8px"
  background: neutral-200
  border-radius: radius-full
  overflow: "hidden"
  
  fill:
    background: primary-500
    height: "100%"
    border-radius: radius-full
    transition: "width 0.3s ease"
  
  with-label:
    height: "24px"
    display: "flex"
    align-items: "center"
    
  label:
    font: body-small
    color: neutral-700
    margin-left: spacing-2
```

#### Skeleton
```yaml
skeleton:
  background: neutral-200
  border-radius: radius-md
  animation: "pulse 1.5s ease-in-out infinite"
  
  text:
    height: "16px"
    width: "100%"
  
  circle:
    border-radius: radius-full
  
  rectangle:
    border-radius: radius-md
```

### 3.6 Data Visualization

#### Chart (Bar, Line, Pie)
```yaml
chart:
  colors:
    - primary-500
    - success-main
    - warning-main
    - error-main
    - info-main
    - clinical-purple
    - clinical-teal
  
  axis:
    color: neutral-400
    font: body-small
  
  grid:
    color: neutral-200
    stroke-width: "1px"
  
  tooltip:
    background: neutral-800
    color: neutral-0
    padding: spacing-2
    border-radius: radius-sm
    font: body-small
```

#### MetricCard
```yaml
metric-card:
  background: neutral-0
  border: "1px solid" neutral-200
  border-radius: radius-lg
  padding: spacing-6
  
  label:
    font: body-small
    color: neutral-600
    margin-bottom: spacing-2
  
  value:
    font: h1
    color: neutral-800
    margin-bottom: spacing-2
  
  change:
    font: body-small
    display: "flex"
    align-items: "center"
    gap: spacing-1
    
    positive:
      color: success-main
    
    negative:
      color: error-main
```

### 3.7 Chat Components

#### ChatMessage
```yaml
chat-message:
  margin-bottom: spacing-4
  display: "flex"
  gap: spacing-3
  
  user:
    flex-direction: "row-reverse"
    
    bubble:
      background: primary-500
      color: neutral-0
      border-radius: "16px 16px 4px 16px"
  
  ai:
    flex-direction: "row"
    
    bubble:
      background: neutral-100
      color: neutral-800
      border-radius: "16px 16px 16px 4px"
  
  bubble:
    padding: spacing-3 spacing-4
    max-width: "70%"
    font: body
  
  avatar:
    size: "32px"
  
  timestamp:
    font: body-small
    color: neutral-500
    margin-top: spacing-1
```

#### SuggestedQuery
```yaml
suggested-query:
  padding: spacing-2 spacing-3
  background: neutral-0
  border: "1px solid" neutral-300
  border-radius: radius-full
  font: body-small
  color: neutral-700
  display: "inline-flex"
  align-items: "center"
  gap: spacing-1
  
  hover:
    background: primary-50
    border-color: primary-500
    color: primary-700
```

---

## 4. Accessibility Guidelines

### 4.1 Color Contrast
- **Text**: Minimum 4.5:1 contrast ratio (WCAG AA)
- **UI Components**: Minimum 3:1 contrast ratio
- **Large Text (18px+)**: Minimum 3:1 contrast ratio

### 4.2 Focus Indicators
- All interactive elements must have visible focus states
- Focus outline: 2px solid primary-500 with 2px offset
- Focus must be visible on keyboard navigation

### 4.3 Keyboard Navigation
- All interactive elements accessible via Tab key
- Logical tab order following visual flow
- Escape key closes modals and dropdowns
- Enter/Space activates buttons and links

### 4.4 Screen Reader Support
- All images have alt text
- Form inputs have associated labels
- ARIA labels for icon-only buttons
- ARIA live regions for dynamic content updates

### 4.5 Touch Targets
- Minimum 44x44px for all interactive elements
- Adequate spacing between adjacent targets

---

## 5. Responsive Behavior

### Breakpoints
- **Small (sm)**: 1280px - Minimum supported
- **Medium (md)**: 1440px - Comfortable viewing
- **Large (lg)**: 1920px+ - Large displays

### Layout Adjustments
- **1280px**: Compact sidebar, 2-column layouts
- **1440px**: Standard sidebar, 3-column layouts
- **1920px+**: Expanded content area, 4-column layouts

---

## 6. Animation & Transitions

### Timing Functions
```yaml
transitions:
  fast: "150ms ease"
  normal: "300ms ease"
  slow: "500ms ease"

usage:
  - Hover states: fast
  - Modal open/close: normal
  - Page transitions: slow
```

### Common Animations
```yaml
fade-in:
  from: "opacity: 0"
  to: "opacity: 1"
  duration: "300ms"

slide-in:
  from: "transform: translateY(-10px); opacity: 0"
  to: "transform: translateY(0); opacity: 1"
  duration: "300ms"

pulse:
  keyframes:
    0%: "opacity: 1"
    50%: "opacity: 0.5"
    100%: "opacity: 1"
  duration: "1.5s"
  iteration: "infinite"
```

---

## 7. Icon System

**Icon Library**: Lucide Icons (outlined style)  
**Default Size**: 20px  
**Sizes**: 16px (small), 20px (medium), 24px (large)

### Common Icons
- **Navigation**: Home, FileText, Users, Settings, LogOut
- **Actions**: Upload, Download, Edit, Trash, Check, X, Plus
- **Status**: CheckCircle, AlertCircle, XCircle, Clock, Loader
- **Medical**: Activity, Heart, Pill, FileText, Clipboard
- **Data**: BarChart, PieChart, TrendingUp, TrendingDown

---

## 8. Figma Component References

*To be populated after Figma file creation*

### Component Library Structure
```
C/Actions/Button
C/Actions/IconButton
C/Actions/Link
C/Inputs/TextField
C/Inputs/Select
C/Inputs/Checkbox
C/Inputs/Radio
C/Inputs/FileUpload
C/Inputs/DateRangePicker
C/Navigation/Header
C/Navigation/Sidebar
C/Navigation/Tabs
C/Navigation/Breadcrumb
C/Content/Card
C/Content/Table
C/Content/Badge
C/Content/Avatar
C/Feedback/Modal
C/Feedback/Alert
C/Feedback/Toast
C/Feedback/ProgressBar
C/Feedback/Skeleton
C/DataViz/Chart
C/DataViz/MetricCard
C/Chat/ChatMessage
C/Chat/SuggestedQuery
```

---

## 9. Design System Governance

### Token Usage Rules
1. **Never hard-code colors** - Always use tokens from color palette
2. **Never hard-code spacing** - Always use spacing scale
3. **Never hard-code typography** - Always use type scale
4. **Component variants only** - No custom one-off components without approval

### Component Creation Process
1. Check if existing component can be extended
2. Document new component specification
3. Add to Figma component library
4. Update designsystem.md
5. Review with design team

### Version Control
- **Version**: 1.0.0 (Phase 1)
- **Last Updated**: [Date of Figma file creation]
- **Change Log**: Track all token and component changes
