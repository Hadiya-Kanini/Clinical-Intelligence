---
trigger: glob
globs: "**/*.{jsx,tsx,js,ts,css,scss,html}"
---

# Frontend Development Standards

Advanced structural and operational frontend. Augments React, performance, accessibility, anti-pattern guidance.

## Styling
- No inline style objects for layout/theme (only dynamic toggles)
- Module domains: global, themes, components
- Theme switch without reload (CSS custom properties)
- Periodic dead style detection (PurgeCSS CI)
- CSS Modules or scoped styles; avoid global cascade

## Component Loading
- Route-level lazy loading mandatory (dynamic imports + suspense)
- Critical fold only loads synchronously
- Heavy components (charts, editors) gated by interaction/intersection
- Preload likely next routes after idle
- Error boundaries wrap lazy segments

## Bundle Governance
- Budgets (gzipped): entry <200KB, chunk <120KB, vendor <350KB
- Vendor split: framework, viz libs, heavy utils (long cache)
- Manual chunking for infrequent co-loads
- Path-specific imports (lodash/debounce, not whole lib)
- Inline assets <4KB only

## Performance Budgets
- Core Web Vitals: LCP <2.5s (p75), CLS <0.1, INP <200ms
- Time to interactive <3s on mid-tier device
- Long tasks >50ms monitored and alerted
- Skeleton placeholders >300ms load (avoid nested spinners)

## State & Data
- Suspense boundaries per data island (prevent whole-page blocking)
- Explicit stale time; no infinite caching unless immutable
- Consolidate adjacent network calls

## Accessibility + Lazy
- Suspense fallbacks communicate purpose (not generic "Loading...")
- Focus first landmark/heading after lazy load
- Respect prefers-reduced-motion

## Error & Resilience
- Segmented boundaries (navigation, content, widgets)
- Lazy import failures auto-retry with backoff
- Offline guard: queue mutations, replay when online

## Assets
- Responsive img srcset or dynamic import for large images
- AVIF/WebP first, JPEG fallback
- font-display: swap; subset fonts; log FOIT >100ms
- Third-party scripts deferred or after idle

## Testing
- Lighthouse CI thresholds (fail on >5% regression)
- Snapshot structural anchors only (ARIA tree)
- Track unused exports (flag if stale 30 days)

## CI Automation
- Bundle diff in commit artifact
- Weekly unused selector report
- Alert if vendor chunk hash changes without dependency updates
