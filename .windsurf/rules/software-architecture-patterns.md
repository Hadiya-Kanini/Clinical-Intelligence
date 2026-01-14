---
trigger: glob
globs: "**/{spec.md,codeanalysis.md,design.md}"
---

# Architecture Pattern Guidelines

Ensure spec.md, design.md, codeanalysis.md apply vetted patterns without duplicating broader guidance.

## Pattern Selection
- Start with problem drivers: domain volatility, team autonomy, scaling, latency, audit
- Choose smallest viable (Layered/Modular Monolith before Microservices)
- Justify: Context → Decision → Benefit (3 bullets)
- Record trade-offs explicitly

## Pattern Decision Matrix
| Need | Prefer | Also Consider |
|------|--------|---------------|
| Simple CRUD, stable domain | Layered | Hexagonal for test isolation |
| High testability/framework independence | Hexagonal | Layered + DI |
| UI feature autonomy | Vertical Slice (Frontend) | Modular Monolith |
| Independent deploy/team scaling | Microservices | Modular Monolith (transition) |
| Real-time event flows | Event-Driven | Streaming/Reactive |
| Audit & temporal queries | CQRS + Event Sourcing | Append-only ledger |
| Variable load | Serverless/FaaS | Container autoscaling |

## Required Sections
- **spec.md**: Architecture overview, pattern rationale, risk/mitigation, NFR mapping
- **design.md**: Component diagram, data flow, failure modes, evolution notes
- **codeanalysis.md**: Detected patterns, boundary violations, coupling hotspots, refactors

## Boundaries
- No lateral skips: presentation → domain/service → data only
- Each module declares responsibility
- One authoritative writer per dataset
- External integrations: explicit retry/backoff/circuit policies

## Events & Async
- Events immutable; version via additive changes
- Idempotent handlers; document idempotency strategy
- Define at-least-once vs exactly-once; compensating actions

## Pattern-Specific

### Microservices
- State boundary with ubiquitous language
- Owned data stores; no cross-service table reads
- Deployment independence test

### Hexagonal
- List ports (inbound/outbound); map adapters
- Core domain zero framework imports

### Vertical Slice (Frontend)
- Scope: UI components + state + data hooks per feature
- Structure: feature-name/ contains components, state, API, tests, styles
- Isolation: Cross-slice imports via exported public API only
- Duplication allowed; extract when ≥3 slices use it

### CQRS + Event Sourcing
- Separate write/read models
- Document projection latency + rebuild strategy

## Quality Gates
- Pattern rationale <12 lines
- No unexplained acronyms
- Every risk has mitigation + detection
- All NFRs map to measurable metrics

## Anti-Patterns to Flag
- Distributed monolith (>3 service chains)
- Shared database (multiple writers)
- Leaky abstractions (domain → transport types)
- Chatty interfaces (>3 sequential remote calls)
