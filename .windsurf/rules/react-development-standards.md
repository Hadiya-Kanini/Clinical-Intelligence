---
trigger: glob
globs: "**/*.jsx, **/*.tsx, **/*.js, **/*.ts, **/*.css, **/*.scss"
---

# ReactJS Development

Modern React patterns following https://react.dev

## Project Context
- TypeScript for type safety
- Functional components with hooks
- Modern build tools (Vite preferred)
- Component composition and reusability

## Architecture
- Functional components with hooks as primary pattern
- Component composition over inheritance
- Feature/domain-based organization
- Separate presentational and container components
- Custom hooks for reusable stateful logic
- Clear data flow hierarchies

## TypeScript
- Interfaces for props, state, components
- Proper types for event handlers and refs
- Generic components where appropriate
- Strict mode in `tsconfig.json`
- React's built-in types (React.FC, React.ComponentProps)
- Union types for variants and states

## Component Design
- Single responsibility principle
- Descriptive, consistent naming (PascalCase for components)
- TypeScript prop validation
- Testable and reusable
- Small, focused components
- Composition patterns (render props, children as functions)

## State Management
- `useState` for local state
- `useReducer` for complex state logic
- `useContext` for shared state across trees
- Redux Toolkit or Zustand for complex apps
- React Query or SWR for server state

## Hooks & Effects
- `useEffect` with proper dependency arrays
- Cleanup functions to prevent memory leaks
- `useMemo`/`useCallback` for performance optimization
- Custom hooks for reusable logic
- Follow rules of hooks (top-level only)
- `useRef` for DOM access and mutable values

## Styling
- CSS Modules, Styled Components, or modern CSS-in-JS
- Mobile-first responsive design
- BEM methodology or similar naming
- CSS custom properties for theming
- Consistent spacing, typography, color systems
- Semantic HTML with proper ARIA attributes

## Performance
- `React.memo` for expensive components
- Code splitting with `React.lazy` and `Suspense`
- Tree shaking and dynamic imports
- Virtual scrolling for large lists
- Profile with React DevTools

## Data Fetching
- React Query, SWR, or Apollo Client
- Loading, error, success states
- Race condition and cancellation handling
- Optimistic updates
- Caching strategies
- Offline and network error handling

## Error Handling
- Error Boundaries for component errors
- Error states in data fetching
- Fallback UI for errors
- Meaningful error messages
- Async error handling in effects/handlers

## Forms & Validation
- Controlled components
- Formik or React Hook Form
- Proper error states
- Accessibility (labels, ARIA attributes)
- Debounced validation
- File upload handling

## Routing
- React Router for client-side routing
- Nested routes and route protection
- Route parameters and query strings
- Lazy loading for route-based code splitting
- Navigation state management

## Testing
- React Testing Library for unit tests
- Test behavior, not implementation
- Jest for test runner
- Integration tests for complex interactions
- Mock dependencies and API calls
- Test accessibility and keyboard navigation

## Security
- Sanitize user inputs (prevent XSS)
- Validate/escape before rendering
- HTTPS for API calls
- Proper auth patterns
- Avoid sensitive data in localStorage/sessionStorage
- Content Security Policy headers

## Accessibility
- Semantic HTML elements
- Proper ARIA attributes and roles
- Keyboard navigation for interactive elements
- Alt text for images, descriptive text for icons
- Proper color contrast ratios
- Test with screen readers

## Common Patterns
- HOCs for cross-cutting concerns
- Render props for composition
- Compound components for related functionality
- Provider pattern for context-based state
- Container/Presentational separation
- Custom hooks for logic extraction
