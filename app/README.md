# Web UI (React)

This directory contains the React-based web user interface. It communicates with the backend API via the defined contracts.

## Development

```bash
# Install dependencies
npm install

# Start development server
npm run dev

# Build for production
npm run build

# Preview production build
npm run preview
```

## Testing

```bash
# Run unit tests
npm test

# Run tests with UI
npm run test:ui

# Run tests with coverage
npm run test:coverage

# Run E2E tests (requires dev server running)
npm run test:e2e

# Run E2E tests with UI
npm run test:e2e:ui
```

## Project Structure

- `src/pages/` - Page components (e.g., LoginPage.jsx)
- `src/components/ui/` - Reusable UI components
- `src/styles/` - CSS tokens and base styles
- `src/__tests__/visual/` - Visual regression tests with Playwright

## Design System

The application uses a token-based design system defined in `src/styles/tokens.css` with:
- Color palette (clinical-appropriate)
- Typography scale
- Spacing system
- Border radius
- Elevation shadows

## Accessibility

All components follow WCAG 2.2 AA guidelines with:
- Semantic HTML
- Proper ARIA attributes
- Keyboard navigation
- Screen reader support
- Focus management

## Manual QA Checklist (SCR-001 Login)

- Narrow viewport / zoom: Verify no clipped content and the email, password, and submit button remain reachable and usable.
- High-contrast mode: Verify labels, inputs, error states, and the submit button remain distinguishable and readable.
- Logo missing: Simulate a broken logo asset and verify the brand title still renders.
- Token-only styling: Verify no newly introduced hard-coded colors (prefer CSS tokens via `var(--color-*)`).
