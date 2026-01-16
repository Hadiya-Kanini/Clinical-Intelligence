---
trigger: glob
globs: "**/*.{cs,csproj,fs,fsproj,go,py,rb,java,kt,ts,js,jsx,tsx,sql,yml,yaml,json,Dockerfile}"
---

# Code Anti-Patterns

Supplements existing guidance. Enforce fast detection and prevention of common mistakes.

## Core Enforcement
- Prefer deletion/refactor over layering fixes
- Fail fast: surface anti-pattern risk in PR
- Never add TODOs for known anti-patterns; fix or justify inline
- Extract duplicated patterns (utility/module) or mark: `// DUPLICATION-INTENTIONAL: reason`

## Must-Block (Reject in PR)
- God objects/functions (multiple unrelated responsibilities)
- Copy-paste >5 lines without abstraction
- Hidden side effects (mutating external state without clear naming)
- Circular dependencies (module cycles)
- Layer violations (UI → persistence, skipping domain/service)
- Silent error swallowing (empty catch, no logging)
- Magic constants (inline numbers/strings without named constant)
- Shared mutable singletons for request-scoped data
- Hard-coded config/credentials/URLs
- Overloaded interfaces mixing concerns (transport + domain + view)

## Require Justification
- Premature optimization (without profiler evidence)
- New abstraction for single usage
- Dependency for trivial stdlib functionality
- Large switch/if chains (better as strategy/map)
- Broad exports (export *, large barrels) increasing coupling

## Architecture Integrity
- Boundary direction: high-level never imports low-level framework directly
- Each module states responsibility in first comment (WHY, not WHAT)
- Cross-module IO calls expose clear return contract (no hidden mutations)

## Event/Async
- Handlers idempotent unless documented: `// NON-IDEMPOTENT: reason`
- No business logic in serialization/mapping/migration helpers
- Avoid chatty interfaces: batch calls; refactor if >3 sequential remote calls

## Testing Anti-Patterns
- Multiple unrelated behaviors per test (split them)
- Mocking internal implementation vs public contract
- Copy-pasted fixtures (centralize or generate)
- Ignoring flaky tests without ticket + root-cause note

## Validation Checklist
- Single responsibility preserved
- No new duplication without justification
- No layering/boundary violations
- Errors handled or rethrown with context
- Magic values extracted or commented
- No direct shared state mutation across concurrency
- Config/secrets externalized
- Added code tested or testability documented

## Auto-Refactor
- Minimize surface change
- Write replacement → switch references → delete obsolete
- Preserve API signatures unless approved

## Documentation
- Link refactors to anti-pattern category
- PR remediation note: Problem → Impact → Fix → Residual Risk

## Escalation
If blocked by systemic issue:
- Create ticket: scope, risk, staged plan
- Don't introduce compensating complexity unless approved