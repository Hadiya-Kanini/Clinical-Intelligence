---
trigger: glob
globs: "**/*.{cs,csproj,fs,fsproj,go,py,rb,java,kt,ts,js,jsx,tsx,sql,yml,yaml,json,Dockerfile}"
---

# Language-Agnostic Standards

Cross-language baseline: KISS, YAGNI, fail-fast validation, size limits, naming clarity, deterministic tests.

## Core Principles
- KISS: Straightforward, readable solutions
- YAGNI: Ship only required functionality
- Fail Fast: Validate at boundaries; surface errors early
- Self-Explanatory Code: Good naming over comments; comment WHY not WHAT

## Size Discipline (Refactor Signals)
- File ≤ ~500 LOC (tests ≤ ~800)
- Function ≤ ~50 LOC (tests ≤ ~100)
- Class ≤ ~200 LOC; Interface ≤ ~150 LOC
- Line Length 80–100 chars
- Refactor when exceeding (or justify inline)

## Refactoring
- Split functions: extract validation → transformation → IO → preserve orchestration
- Composition over inheritance (>2 levels discouraged)
- Guard clauses over deep nesting

## Naming
- Variables: descriptive (totalPrice, not tp)
- Functions: verb + target (loadUserProfile)
- Classes: noun (InvoiceAggregator)
- Constants: SCREAMING_SNAKE_CASE (MAX_RETRY_ATTEMPTS)
- Avoid: Helper, Util, Manager

## Error Handling
- Never swallow exceptions; add context or rethrow
- Differentiate recoverable vs fatal with typed errors
- Sanitize user messages; retain technical detail in logs (no secrets)

## Testing
- TDD: Red → Green → Refactor
- AAA structure: Arrange, Act, Assert
- One expectation per test
- No cross-test state leakage
- Deterministic data; seeded randomness
- Edge coverage: negative, boundary, typical paths

## Tooling
- Lint/type checks pass; fix warnings (don't suppress)
- Flag deep nesting (>3 levels) or long functions
- ~80% coverage for core logic

## Review Checklist
- Single clear purpose per function/class
- No copy-paste >5 lines without abstraction
- Descriptive naming; no ambiguous abbreviations
- Tests for new logic (happy + edge paths)
- No empty catch blocks
- No hardcoded secrets (env/config driven)
- Magic values extracted to named constants
- Debug statements removed

## Anti-Patterns
- God object/function
- Deep nesting (>3 levels)
- Speculative abstraction
- Silent failure (empty catch/ignored promise)
- Unbounded growth (collections without cap)

## Documentation
- Public APIs: brief docstring with purpose, params, errors, example
- Complex algorithms: rationale + reference
- Trivial helpers: no comments if name is clear

## Code Generation
- Analyze existing codebase before generating
- Identify boundaries before modifying
- Preserve existing functionality
- Assess dependencies and side effects
- Avoid deprecated libraries