---
trigger: glob
globs: "**/*.spec.ts, **/*.test.ts"
---

# Playwright Testing Guide

Augments existing guidance. Focuses on stability, flakiness avoidance, and suite sustainability.

## Core Principles
- Each test independent—no side effects from prior tests
- Prefer user-observable outcomes over implementation details
- Eliminate non-determinism (timing races, shared state, environment leakage)
- Fast feedback: lean tests; push combinatorics to lower layers
- Fail fast with actionable diagnostics

## Critical Anti-Patterns (MUST Avoid)
| Category | Anti-Pattern | Safer Alternative |
|----------|--------------|-------------------|
| Timing | `waitForTimeout()` as sync | Web-first assertions / explicit state waits |
| Selectors | Styling / dynamic ID / nth-child | Role, accessible name, test id (if role not feasible) |
| State | Test B depends on Test A login | Each test performs (or reuses stored) auth setup |
| Assertions | Existence-only (`toBeTruthy()`) | User-facing state & text, a11y tree, URL, visibility |
| Network | Live external API | Route interception + mocked responses |
| Page Objects | Embedding test logic/assertions | Interaction + lightweight state queries only |
| Data | Hard-coded credentials/literals | Centralized fixtures + env-driven secrets |
| Debugging | Leftover `page.pause()`, console spam | Targeted trace/screenshot on failure only |
| Performance | Repeating expensive setup | Reuse authenticated storage state / worker fixtures |
| Security | Plain-text secrets committed | Environment variables + secret store |

## Test Independence
- Never rely on execution order; tests pass when run alone (`--grep`)
- Storage state snapshots when safe; refresh if auth-scoped data mutates
- Clear persisted state (localStorage, sessionStorage, IndexedDB) in `beforeEach` or fixture
- Random data: seed values for reproducibility

## Synchronization & Timing
- Prefer assertion-driven waiting (auto-wait) over manual waits
- Explicitly wait for: navigation completion, network idle (business-critical), component readiness
- Use `page.waitForResponse()` only when asserting request side-effect
- Sequence: trigger → assert intermediate state → proceed

## Assertion Quality
- Express user intent ("success banner visible", "URL includes /dashboard")
- Precise failure messages (custom wrapper when ambiguous)
- Avoid probing private globals or framework internals
- Negative assertions for critical absence (spinner removed) with timeout

## Network & API Control
- Intercept external endpoints with stable fixtures: 3rd-party services, volatile datasets, failure paths
- Model error scenarios (4xx/5xx) explicitly
- Coalesce reused mock responses into helper factories (`buildUser()`)

## Page Object Discipline
**SHOULD:**
- Expose clear intent methods (`login(email, password)`)
- Provide lightweight state accessors (`isLoggedIn()`)

**MUST NOT:**
- Contain test assertions beyond basic state queries
- Embed arbitrary sleeps or environment mutation
- Chain multiple business flows (keep composable)

## Test Data & Fixtures
- Centralize canonical test users, roles, boundary values
- Generate fresh entities via deterministic helpers
- Parameterized tests for input matrices
- Mask/redact sensitive data in logs

## Configuration & Performance
- Modest global timeout; rely on per-assertion stability
- Parallel by default; serial only for non-isolatable side effects (document why)
- Reuse browser contexts judiciously
- Capture traces/screenshots ONLY on failure

## Debugging & Diagnostics
- `page.pause()` only locally; never commit
- Enable trace viewer on flaky investigation branches; remove before merge
- Attach context metadata (test id, seed, user role) to failure logs

## Security & Secrets
- Read credentials from environment (never literal in repo)
- Rotate test secrets separately from production; least privilege
- Scrub secrets from traces if shared externally

## Stability Checklist
Before marking stable:
- [ ] No direct `waitForTimeout` calls
- [ ] All selectors are role / label / test id (no brittle chains)
- [ ] No shared mutable cross-test state
- [ ] Network dependencies mocked where non-deterministic
- [ ] Assertions reflect user-visible outcomes
- [ ] Page objects free of test logic & sleeps
- [ ] Secrets sourced via environment
- [ ] Parallel-safe (passes when isolated & in random order)

## Maintenance
- Refactor duplicated locator fragments when repeated >2 times
- Document root cause + mitigation when fixing flakiness
- Periodically audit slowest 10 tests; optimize or re-scope
