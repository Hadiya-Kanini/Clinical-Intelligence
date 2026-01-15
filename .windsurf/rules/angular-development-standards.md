---
applyTo: '**/*.jsx, **/*.tsx, **/*.js, **/*.ts, **/*.css, **/*.scss'
---

# Angular Development

Modern Angular patterns following https://angular.dev/style-guide

## Project Context
- TypeScript with strict mode enabled
- Standalone components as primary pattern (Angular 14+)
- Modern build tools (Angular CLI, esbuild)
- Component composition and reusability
- Signals for reactive state management (Angular 16+)

## Architecture
- Standalone components with signals as primary pattern
- Component composition over inheritance
- Feature/domain-based organization
- Separate smart (container) and dumb (presentational) components
- Services for reusable business logic
- Clear data flow hierarchies
- Dependency injection for loose coupling

## TypeScript
- Strict mode enabled in `tsconfig.json`
- Interfaces for data models and contracts
- Proper types for all variables, parameters, and return values
- Angular's built-in types (OnInit, AfterViewInit, etc.)
- Union types for variants and states
- Avoid `any` type; use `unknown` when type is truly unknown
- Use type-safe strings (union types) instead of plain strings

## Component Design
- Single responsibility principle
- Descriptive, consistent naming (PascalCase for components, kebab-case for files)
- File naming: `feature.type.ts` (e.g., `user-profile.component.ts`)
- TypeScript interfaces for inputs and outputs
- Limit files to 400 lines; functions to 75 lines
- Small, focused, testable components
- Use `@Component` decorator with inline templates for simple components
- Separate template files for complex components

## File Naming & Structure
- Use hyphens to separate words: `user-profile.component.ts`
- Match file names to primary TypeScript identifier
- Same base name for component's TS, HTML, and CSS files
- Test files: `user-profile.component.spec.ts`
- One component/service/directive per file
- Organize by feature areas, not by type
- Use `index.ts` barrel exports for cleaner imports

## State Management
- Signals for reactive state (Angular 16+)
- Services with BehaviorSubject for shared state
- NgRx, NGXS, or Akita for complex applications
- RxJS for async operations and event streams
- OnPush change detection for performance

## Signals & Reactivity
- Use signals for reactive state management
- `signal()` for writable signals
- `computed()` for derived values
- `effect()` for side effects
- Prefer signals over Observables for simple state
- Combine signals with RxJS when needed using `toSignal()` and `toObservable()`

## Dependency Injection
- Prefer `inject()` function over constructor parameter injection
- `inject()` offers better readability and type inference
- Use `providedIn: 'root'` for singleton services
- Use `InjectionToken` for non-class dependencies
- Provide services at component level for component-specific instances
- Use `@Optional()` and `@SkipSelf()` decorators when appropriate

## Component Properties & Methods
- Group Angular-specific properties at top:
  1. Inputs
  2. Outputs
  3. ViewChild/ViewChildren
  4. ContentChild/ContentChildren
  5. Private/protected properties
- Constructor or inject() calls
- Lifecycle hooks (in execution order)
- Public methods
- Protected methods (for template use)
- Private methods
- Use `protected` for template-only members
- Use `readonly` for inputs, outputs, and queries

## Lifecycle Hooks
- Implement lifecycle hook interfaces (OnInit, OnDestroy, etc.)
- Order hooks by execution sequence
- Keep lifecycle methods simple; delegate to well-named methods
- Always implement `OnDestroy` for cleanup
- Use `takeUntil()` or `takeUntilDestroyed()` for Observable cleanup

## Templates & Data Binding
- Use Angular template syntax (interpolation, property binding, event binding)
- Avoid complex logic in templates; move to component
- Use `async` pipe for Observables (automatic subscription management)
- Wrap pipes in parentheses: `(movies$ | async)`
- Use `trackBy` with `*ngFor` for performance
- Safe navigation operator `?.` for nullable properties
- Structural directives only on `<ng-container>` elements
- HTML attribute ordering:
  1. Structural directives (`*ngIf`, `*ngFor`)
  2. Animation triggers (`@fadeIn`)
  3. Template variables (`#element`)
  4. Static attributes (`id`, `class`, `type`)
  5. Dynamic properties (`[property]`, `[attr.aria-label]`)
  6. Events (`(click)`, `(input)`)
  7. Two-way binding (`[(ngModel)]`)

## Control Flow Syntax
- Use `@if`, `@for`, `@switch` (Angular 17+) over `*ngIf`, `*ngFor`, `*ngSwitch`
- Built-in control flow offers better type narrowing and performance

## Forms
- Prefer Reactive Forms over Template-driven Forms
- Type-safe forms with typed FormGroup, FormControl
- Form validation in component, not template
- Custom validators as separate functions
- Proper error handling and display
- Accessibility (labels, ARIA attributes)

## RxJS & Observables
- Prefer Observables over Promises for consistency
- Use RxJS operators for transformation and composition
- Avoid nested subscriptions; use operators (`switchMap`, `mergeMap`, `combineLatest`)
- Subscribe in template with `async` pipe when possible
- Unsubscribe in `ngOnDestroy` (use `takeUntil()` pattern)
- Use `shareReplay()` for caching API responses
- Do not expose Subjects; expose Observables via `asObservable()`
- Handle errors with `catchError` operator

## Change Detection
- Use `OnPush` change detection strategy for performance
- Immutable data patterns
- Mark component for check when needed with `ChangeDetectorRef`
- Avoid direct DOM manipulation
- Use `ChangeDetectorRef.detectChanges()` sparingly

## Styling
- Component-scoped styles (View Encapsulation)
- Do not remove view encapsulation unless absolutely necessary
- Angular Material theming
- Prefer `[class.active]` and `[style.color]` over `[ngClass]` and `[ngStyle]`

## Directives
- Attribute directives for behavior
- Structural directives for DOM manipulation
- Use application-specific prefix (e.g., `app`, `mr`)
- CamelCase selector for attribute directives: `[appTooltip]`
- Avoid changing DOM directly; use `Renderer2`

## Services
- `@Injectable({ providedIn: 'root' })` for app-level services
- Component-level providers for component-specific services
- Services for business logic, data fetching, state management
- Pure functions for utility operations
- Isolate API-specific logic in services
- Cache API calls when appropriate

## Routing
- Lazy loading for feature modules
- Route guards for access control (functional guards preferred)
- Resolver for pre-fetching data (functional resolvers preferred)
- Route parameters and query parameters
- Child routes for nested views
- Preloading strategies for optimization
- `RouterLink` directive for navigation
- `Router` service for programmatic navigation

## HTTP & Data Fetching
- Use `HttpClient` service
- Type API responses with interfaces
- Error handling with `catchError`
- Loading, error, success states
- Interceptors for auth, logging, error handling
- Cancel in-flight requests on navigation

## Performance
- Lazy loading modules and components
- `OnPush` change detection strategy
- `trackBy` functions with `*ngFor`
- Virtual scrolling for large lists (CDK)
- Avoid memory leaks (unsubscribe, destroy refs)
- Pure pipes for transformations
- Tree-shakable providers
- AOT compilation for production

## Testing
- Jasmine and Karma for unit tests (or Jest)
- Test behavior, not implementation
- Use `TestBed` for component testing
- Mock dependencies and HTTP calls with `HttpTestingController`
- Write meaningful tests; avoid empty spec files
- Test with `DebugElement` and `fixture.detectChanges()`

## Security
- Sanitize user inputs (DomSanitizer)
- Avoid direct DOM access; use templates
- Avoid `bypassSecurityTrust*` methods unless necessary
- Validate and escape before rendering
- Use Angular's built-in XSS protection

## Accessibility
- Semantic HTML elements
- ARIA attributes and roles in templates
- Keyboard navigation support
- Angular CDK A11y module for focus management
- Test with screen readers
- Angular Material components have built-in accessibility

## Error Handling
- RxJS error handling with `catchError`
- Global error handler with `ErrorHandler`
- Meaningful error messages
- User-friendly error UI
- Log errors for monitoring
- Handle async errors in effects

## Code Quality
- Use Angular CLI for generation (`ng generate`)
- ESLint for linting (TSLint deprecated)
- Prettier for formatting
- Follow Angular coding style guide
- Remove unused code and imports
- Use meaningful variable and function names

## Common Patterns
- Smart (container) and Dumb (presentational) components
- Service facade pattern
- Observable data services
- Singleton services
- Base component classes for shared logic
- Custom pipes for transformations
- Directives for DOM behavior
- Guards for route protection

## Angular DevTools
- Use Angular DevTools for debugging
- Inspect component tree
- Profile change detection
- Debug dependency injection
- Monitor component state

## Environment Configuration
- Use `environment.ts` for environment-specific values
- Development and production environments
- Add custom environments as needed
- Never commit secrets; use environment variables

## Module Organization (Legacy)
- Core module for singleton services (if using NgModules)
- Shared module for common components/directives/pipes
- Feature modules for features
- Lazy load feature modules
- `forRoot()` pattern for module configuration
- Prevent re-importing core modules in feature modules

## Standalone Components (Modern)
- Default to standalone components (Angular 14+)
- Import dependencies directly in component
- Use `bootstrapApplication()` for app bootstrap
- Functional guards and resolvers
- Functional interceptors
- Tree-shakable by default

## Best Practices Summary
- Single responsibility principle
- Follow official Angular style guide
- Keep components focused on presentation
- Use services for business logic
- Immutable data patterns
- OnPush change detection
- Async pipe for subscriptions
- Type safety everywhere
- Test meaningfully
- Optimize for performance
- Accessibility first
- Security by design
