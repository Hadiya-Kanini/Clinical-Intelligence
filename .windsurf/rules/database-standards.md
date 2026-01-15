---
trigger: glob
globs: "**/*.{cs,csproj,fs,fsproj,go,py,rb,java,kt,ts,js,sql,yml,yaml,json,Dockerfile}"
---

# Database Standards

Higher-order data architecture & operational quality. Excludes foundational SQL style, injection prevention, basic indexing (covered elsewhere).

## Data Modeling
- **Intent-Revealing Names:** Tables & columns articulate business meaning (avoid `Data`, `Value`)
- **Narrow Tables First:** Decompose wide tables (>40 columns) into cohesive sub-entities
- **Immutable Facts vs Mutable State:** Separate append-only event/fact tables from current-state snapshots
- **Enumerations:** Centralize (lookup tables or constrained enum types); no magic strings across services
- **Temporal Modeling:** Bitemporal or effective dating (valid_from, valid_to) instead of overwriting

## Keys & Identity
- **Natural Key When Stable:** Use natural composite keys where business guarantees stability; surrogate when volatility risk exists
- **Surrogate + Natural Hybrid:** Surrogate primary key + unique natural key constraint for business validation
- **UUID vs INT:** Monotonic (ULID / sequential UUID) for write hot-spot & index fragmentation concerns
- **Foreign Key Optionality:** Nullable only for true business optionality—use staging tables for incomplete ingestion

## Normalization vs Duplication
- Normalize for integrity until proven read amplification; denormalize with measured latency evidence & documented invalidation
- **Precompute/Materialize:** Derived projections for expensive aggregations (document refresh cadence & staleness SLA)
- No partial denormalization—fully own projection or not at all

## Query Shape & Access Layer
- **Explicit Repository/Gateway Boundaries:** Application depends on abstraction; raw ORM/query objects internal
- **Deterministic Loading Plans:** Pre-plan eager vs selective column projection per use case
- **N+1 Prevention:** Detect via automated test (query count threshold) in integration suite
- **Result Set Contracts:** Guarantee ordering, filtering semantics, pagination determinism

## Concurrency & Consistency
- **Optimistic Concurrency Tokens:** Row version / etag columns; reject conflicting writes with explicit domain error
- **Lost Update Safeguards:** Never read-modify-write without version predicate
- **Write Contention Mitigation:** Shard hot aggregates logically (per tenant/bucket) before distributed locks
- **Multi-Step Consistency:** Multi-entity updates prefer single transaction; spanning contexts use saga/orchestration with documented compensations

## Transactions & Isolation
- **Default Isolation:** READ COMMITTED; elevate (REPEATABLE READ/SERIALIZABLE) only for correctness-critical sections
- **Idempotent Retry Blocks:** Wrap transient-failure retry only around idempotent transactional units
- **Savepoints:** Partial rollback in complex multi-operation batches

## Migration Safety
- **Expand → Migrate Data → Switch Usage → Contract:** Never drop/rename in same deploy as replacement logic
- **Reversible Migrations:** Provide down path until proven stable in production
- **Zero-Downtime Additions:** Add nullable columns first; backfill in batches; enforce NOT NULL after completeness verified
- **Large Table Changes:** Online schema change tools / chunked backfills with progress logging & cancellation hooks
- **Migration Observability:** Emit metrics (name, batch rate, rows processed, error count)

## Index Strategy
- **Hypothesis Driven:** Each index has recorded rationale (workload, query pattern) & review date
- **Covering Indexes:** Eliminate bookmark lookups on hottest paths; keep width disciplined
- **Composite Order:** Most selective & equality-filtered columns first; defer range/ordering columns
- **Index Bloat Monitoring:** Track dead tuple %, fragmentation, stale statistics—automate alerts
- **Cleanup Cadence:** Periodic review to drop unused indexes (via usage DMVs / pg_stat / sys.dm_db_index_usage_stats)

## Caching & Persistence
- **Multi-Layer Awareness:** Cache invalidation inside same transaction boundary or via outbox event dispatch
- **Write-Through vs Write-Behind:** Write-behind only with durability assurances & replay for crash recovery
- **Cache Stampede Prevention:** Request coalescing / per-key mutex + probabilistic early refresh (jittered TTL)

## Observability & Diagnostics
- **Structured Query Logging:** Slow queries above threshold with: normalized statement, duration, rows, plan hash, correlation id
- **Plan Regression Detection:** Track plan hash drift for top N queries; alert on sudden change + latency increase
- **Cardinality Risk Flags:** Identify queries with high variance row estimates vs actual
- **Data Quality Metrics:** Freshness (lag), completeness (% non-null), duplication rate on unique keys

## Security & Governance
- **Least Privilege DB Roles:** Separate migration, app read/write, read-only analytics; no shared superuser
- **Restricted Dynamic SQL:** Allow only in vetted utility modules with input sanitization & parameterization wrappers
- **PII Tagging:** Classify sensitive columns; enforce encryption/masking at rest & in logs

## Multi-Tenancy & Sharding
- **Strategy Declaration:** Document model (shared schema, schema-per-tenant, database-per-tenant, key-based shard) with routing logic
- **Hotspot Mitigation:** Hash-based distribution for sequential tenant identifiers
- **Cross-Tenant Queries:** Prohibit unless routed through analytics pipeline with anonymization
- **Tenant Data Lifecycle:** Explicit archival + purge workflows respecting retention policies

## Event Sourcing & CDC
- **Immutable Event Store:** Append-only; corrections via compensating events
- **Snapshotting Policy:** Periodic snapshot per aggregate after threshold events to bound replay latency
- **CDC Consumption:** Deduplicate on primary key + LSN/sequence; treat gaps as backpressure signals & alert

## Testing & Verification
- **Migration Dry Run:** Simulate against masked production snapshot; compare row counts & constraint violations pre/post
- **Query Budget Tests:** Assert max queries per high-level operation (fails if N+1 introduced)
- **Data Drift Tests:** Validate projections vs authoritative sources (checksum or row count parity)
- **Fixture Realism:** Seed datasets representing edge cardinalities (empty, typical, high-volume)

## Quality Checklist
Before merging:
- [ ] Migration follows expand → migrate → switch → contract
- [ ] New/changed queries covered by N+1 prevention test
- [ ] Added indexes have documented rationale & monitored usage plan
- [ ] Long-running data task includes progress & cancellation
- [ ] Caching layer invalidation path defined & tested
- [ ] Concurrency control (version token/predicate) applied to mutable aggregates
- [ ] Sensitive columns classified & protected per policy
- [ ] Multi-tenant isolation preserved
- [ ] Event/CDC consumers idempotent & duplicate-safe
- [ ] Performance baseline compared (no significant regression without justification)
