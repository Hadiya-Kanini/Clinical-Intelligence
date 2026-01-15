---
trigger: glob
globs: "**/*.ts"
---

# TypeScript Development

> Target TypeScript 5.x / ES2022. Adjust for older runtimes if needed.

## Core Intent
- Respect existing architecture and standards
- Prefer readable over clever solutions
- Extend current abstractions before creating new ones
- Prioritize maintainability: short methods, clean code

## General
- Pure ES modules only (no CommonJS)
- Use native features over polyfills
- Follow project's build, lint, test scripts
- Note design trade-offs when not obvious

## Project Organization
- Follow repository folder structure
- Kebab-case filenames (e.g., `user-session.ts`)
- Keep tests, types, helpers near implementation
- Reuse shared utilities before adding new ones

## Naming & Style
- PascalCase for classes, interfaces, enums, type aliases
- camelCase for everything else
- No interface prefixes (`I`)
- Name by behavior/domain, not implementation

## Formatting
- Run lint/format scripts before submitting
- Match project indentation, quotes, trailing commas
- Extract helpers when logic branches grow
- Favor immutable data and pure functions

## Type System
- Avoid `any`; prefer `unknown` with narrowing
- Discriminated unions for events and state machines
- Centralize shared contracts
- Use utility types (Readonly, Partial, Record)

## Async & Error Handling
- Use `async/await` with try/catch and structured errors
- Guard edge cases early
- Use project's logging/telemetry utilities
- Surface user errors via notification pattern
- Debounce config updates; dispose resources deterministically

## Architecture
- Follow DI or composition pattern; single-purpose modules
- Observe initialization and disposal sequences
- Decouple transport, domain, presentation layers
- Supply lifecycle hooks (initialize, dispose) and tests

## External Integrations
- Instantiate clients outside hot paths; inject for testability
- Load secrets from secure sources (never hardcode)
- Apply retries, backoff, cancellation to network/IO
- Normalize responses; map errors to domain shapes

## Security
- Validate/sanitize external input with schema validators or type guards
- Avoid dynamic code execution and untrusted templates
- Encode untrusted content; use framework escaping or trusted types
- Parameterized queries to block injection
- Secure secret storage with rotation and least-privilege scopes
- Immutable flows and defensive copies for sensitive data
- Vetted crypto libraries only
- Prompt dependency patching

## Configuration
- Use shared config helpers with schema validation
- Secure secret storage; guard undefined and errors
- Document new config keys; update tests

## UI Components
- Sanitize user/external content before rendering
- Keep UI thin; push logic to services/state managers
- Decouple UI from business logic with messaging/events

## Testing
- Unit tests with project framework and naming style
- Integration/E2E tests when crossing modules/APIs
- Run targeted test scripts for quick feedback
- Fake timers over brittle timing assertions

## Performance
- Lazy-load heavy dependencies; dispose when done
- Defer expensive work until needed
- Batch/debounce high-frequency events
- Track resource lifetimes to prevent leaks

## Documentation
- JSDoc for public APIs with `@remarks` or `@example`
- Comment intent only; remove stale notes
- Update design docs for significant patterns
