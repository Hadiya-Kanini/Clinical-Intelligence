---
trigger: glob
description: WCAG 2.2 Level AA accessibility guidance. Built with accessibility in mind but may still have issues.
globs: "**/*.{jsx,tsx,js,ts,css,scss,html}"
---

# Web Accessibility Standards

Generate accessible code conforming to [WCAG 2.2 Level AA](https://www.w3.org/TR/WCAG22/). After generating, review against WCAG and iterate. Inform user to test with tools like [Accessibility Insights](https://accessibilityinsights.io/).

## Inclusive Language
- People-first language (e.g., "person using screen reader")
- Avoid stereotypes or assumptions about ability
- Include reasoning/references to standards
- Neutral, helpful, respectful tone

## Cognitive
- Plain language; consistent structure and navigation order
- Clean, simple interface; reduce distractions

## Keyboard
- All interactive elements keyboard navigable with visible focus
- Tab order follows reading order; static elements not in tab order
- Hidden elements not focusable
- Skip links for repeated blocks (e.g., "Skip to main")
- Focus not trapped; Escape closes surfaces
- Composite components (grids, menus): single tab stop with arrow key navigation
- Common keys: Tab (next element), Arrow (within component), Enter (activate), Escape (close)

## Low Vision
- Minimum 4.5:1 contrast for normal text; 3:1 for large text (18pt+ or 14pt+ bold)
- Text resizable to 200% without loss of content
- No information by color alone

## Screen Reader
- Semantic HTML (e.g., `<button>`, `<nav>`, `<main>`)
- ARIA labels when semantic HTML insufficient
- Meaningful link text (not "click here")
- Live regions for dynamic content updates
- Form labels programmatically associated

## Voice Access
- Visible labels match accessible names
- Large clickable targets (min 44x44px)

## Forms
- Each input has associated label (via `<label for>` or `aria-labelledby`)
- Required fields marked with `aria-required` or `required`
- Error messages linked to inputs (`aria-describedby`)

## Images & Graphics
- Informative: meaningful alt text
- Decorative: `alt=""` or `aria-hidden="true"`
- Complex: supplemental description via `aria-describedby`
- Correct role: `img`, `graphics-document`, etc.

## Navigation & Menus
- Wrap in `<nav>` with unique `aria-label`
- Current page indicated (`aria-current="page"`)
- Keyboard operable with Escape to close

## Page Title
- Unique, descriptive `<title>` for each page

## Tables & Grids
- Column/row headers programmatically associated (`<th scope>`)
- Simple tables for static info; grids for interactive data
- Use semantic elements (`<table>`, `<thead>`, `<tbody>`, `role="grid"`)