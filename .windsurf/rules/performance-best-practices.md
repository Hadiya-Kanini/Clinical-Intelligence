---
trigger: model_decision
description: Performance optimization best practices for frontend, backend, and database layers.
---

# Performance Optimization

## General Principles
- Measure first, optimize second (profile and benchmark)
- Optimize for common case; defer edge case optimization
- Avoid premature optimization; write clear code first
- Minimize resource usage (memory, CPU, network, disk)
- Prefer simplicity; simple algorithms often perform best
- Document performance-critical code
- Understand platform characteristics
- Automate performance testing in CI/CD
- Set performance budgets with automated checks

## Frontend

### Rendering & DOM
- Batch DOM updates; avoid updating in loops
- Use React.memo, useMemo, useCallback to avoid re-renders
- Stable keys in lists (not array indices)
- CSS classes over inline styles
- CSS animations over JavaScript for GPU acceleration
- Defer non-critical work with requestIdleCallback

### Assets
- Compress images (WebP, AVIF); SVGs for icons
- Minify and bundle JS/CSS; enable tree-shaking
- Long-lived cache headers with cache busting
- Lazy load images (`loading="lazy"`) and dynamic imports
- Subset fonts; use `font-display: swap`

### Network
- Reduce HTTP requests; inline critical CSS
- Enable HTTP/2 or HTTP/3
- Service Workers, IndexedDB, localStorage for caching
- CDN for static assets
- `defer`/`async` for non-critical scripts
- Preload/prefetch critical resources

### JavaScript
- Web Workers for heavy computation
- Debounce/throttle scroll, resize, input events
- Clean up listeners, intervals, DOM refs (avoid memory leaks)
- Maps/Sets for lookups; TypedArrays for numeric data
- Avoid globals and deep cloning

### React
- React.memo, useMemo, useCallback
- Code splitting (React.lazy, Suspense)
- Avoid anonymous functions in render
- Profile with React DevTools Profiler

### Angular
- OnPush change detection
- Simple template expressions
- trackBy in ngFor
- Lazy load with Angular Router

### Troubleshooting
- Chrome DevTools Performance tab, Lighthouse
- WebPageTest for real-world testing
- Monitor Core Web Vitals (LCP, FID, CLS)

## Backend

### Algorithms & Data Structures
- Choose appropriate data structures (arrays, hash maps, trees)
- Efficient algorithms (binary search, quicksort, hash-based)
- Avoid O(n²) or worse; profile nested loops
- Batch processing for overhead reduction
- Stream large datasets to avoid memory loading

### Concurrency
- Async I/O (async/await, callbacks, event loops)
- Thread/worker pools for resource management
- Locks, semaphores for race condition prevention
- Batch network/database calls
- Backpressure in queues and pipelines

### Caching
- In-memory caches (Redis, Memcached) for hot data
- Cache invalidation (TTL, event-based, manual)
- Distributed caching with consistency awareness
- Cache stampede protection (locks, request coalescing)
- Don't cache volatile or sensitive data

### API & Network
- Minimize payloads; compress (gzip, Brotli)
- Paginate large results; cursors for real-time data
- Rate limiting for protection
- Connection pooling
- HTTP/2, gRPC, WebSockets for high-throughput

### Logging & Monitoring
- Minimize logging in hot paths
- Structured logging (JSON, key-value)
- Monitor latency, throughput, errors, resources
- Alerts for regressions and exhaustion

### Node.js
- Async APIs; never block event loop
- Clustering/worker threads for CPU-bound tasks
- Streams for large data
- Profile with clinic.js, node --inspect

### Python
- Built-in data structures (dict, set, deque)
- Profile with cProfile, Py-Spy
- multiprocessing/asyncio for parallelism
- lru_cache for memoization

### .NET
- async/await for I/O operations
- Span<T>, Memory<T> for memory efficiency
- Object/connection pooling
- IAsyncEnumerable<T> for streaming

### Troubleshooting
- Flame graphs for CPU visualization
- Distributed tracing (OpenTelemetry, Jaeger, Zipkin)
- Heap dumps and memory profilers
- Log slow queries and API calls

## Database

### Query Optimization
- Index frequently queried, filtered, joined columns
- Select only needed columns (not SELECT *)
- Parameterized queries for security and plan caching
- Analyze query plans with EXPLAIN
- Avoid N+1 queries; use joins or batch queries
- LIMIT/OFFSET or cursors for large result sets

### Schema Design
- Normalize for redundancy; denormalize for read-heavy workloads
- Efficient data types with appropriate constraints
- Partition large tables
- Archive or purge old data regularly
- Foreign keys for integrity (consider write performance)

### Transactions
- Short transactions to reduce lock contention
- Lowest isolation level meeting consistency needs
- Avoid long-running transactions

### Caching & Replication
- Read replicas for scaling reads; monitor lag
- Cache query results (Redis, Memcached)
- Choose write-through or write-behind strategy
- Sharding for horizontal scalability

### NoSQL
- Model for access patterns
- Distribute writes/reads to avoid hot partitions
- Watch unbounded arrays or documents
- Understand eventual vs strong consistency

### Troubleshooting
- Slow query logs for bottlenecks
- EXPLAIN for query plan analysis
- Monitor cache hit/miss ratios
- Database-specific tools (pg_stat_statements, MySQL Performance Schema)

## Performance Review Checklist
- Algorithmic inefficiencies (O(n²) or worse)?
- Appropriate data structures?
- Unnecessary computations or repeated work?
- Caching used correctly with proper invalidation?
- Optimized, indexed queries without N+1 issues?
- Large payloads paginated, streamed, or chunked?
- Memory leaks or unbounded resource usage?
- Network requests minimized and batched?
- Assets optimized and compressed?
- Blocking operations in hot paths?
- Logging minimized in hot paths?
- Performance-critical code documented?
- Automated performance tests?
- Alerts for regressions? 
