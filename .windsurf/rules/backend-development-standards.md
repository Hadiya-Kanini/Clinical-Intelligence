---
trigger: glob
globs: "**/*.{cs,csproj,fs,fsproj,go,py,rb,java,kt,ts,js,yml,yaml,json,Dockerfile}"
---

# Backend Development Standards

Higher-level system design: correctness under failure, evolvability, data integrity, observability, production readiness.

## Architecture & Boundaries
- Explicit bounded contexts; each owns its language
- Expose only stable contracts (DTOs, events)
- Domain core has zero outward infrastructure dependencies
- Single business rule change affects one module + tests

## API & Contracts
- Contract-first (OpenAPI/JSON Schema/Protobuf)
- Backward compatibility; additive changes preferred
- Idempotency for mutating endpoints
- Cursor-based pagination for unbounded collections
- Consistent error envelope (traceId, code, message, details[], timestamp)

## Data Ownership
- Single source of truth per service
- No shared database reads; use events or APIs
- Read models only when justified by bottlenecks
- Referential integrity within bounded context
- Forward-compatible migrations (expand → deploy → contract)

## Transactions & Consistency
- Local ACID within service boundary
- Outbox pattern for cross-service events
- Document eventual states explicitly
- Idempotent consumers (tolerate duplicates)

## Caching
- Specify purpose and invalidation rules per cache
- Define TTL staleness budget
- Negative caching (15–60s) for miss amplification
- Design invalidation before adding cache

## Resilience
- Explicit timeouts on all outbound calls
- Circuit breakers for failure burst risk
- Bulkheads for high-latency/3rd-party calls
- Exponential backoff + jitter; retry idempotent only
- Dead letter queue with metadata

## Observability
- Propagate trace/span IDs (traceId, spanId, userId, tenantId)
- Structured logging with cause, latency, outcome
- Control cardinality; bucket unbounded labels
- RED metrics (Rate, Errors, Duration) per endpoint
- Health: Liveness (process viable), Readiness (dependencies reachable), Startup (warm-up)

## Background Jobs
- Define SLA/SLO per job class
- At-least-once semantics with deduplication
- Deterministic sharding to prevent hotspots
- Visibility timeout > worst-case processing + buffer
