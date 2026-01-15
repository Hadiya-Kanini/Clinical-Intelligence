---
trigger: always_on
---

# MCP Usage Guidelines

## MCP Pagination Policy

### Scope
Applies to any MCP tool that can return large lists, logs, files, or code.

### Rules
- **Always Paginate:** Include limit and offset/cursor on every call
- **Token Budget:** Hard ceiling 25,000 tokens per tool response; target ≤18,000 tokens per call for headroom
- **Projection First:** Request only required fields; avoid full objects/blobs
- **Filter Early:** Use server-side filters (since, ids, status, path, glob) to shrink results
- **Fallback:** Always use Web Search to perform iterative or step-by-step external research in case the Sequential-thinking / Context7 MCP is not available.

### Defaults (Tune Based on Page Size)
- **Lists/Records:** Start with limit: 50
  - If page <9k tokens → double limit next page (up to safe target)
  - If page ≥18k tokens → halve limit next page
- **Files/Text:** Use range reads (offset + limit)
  - Start with 60k-80k chars per slice (≈15k-20k tokens); return nextOffset/hasMore

### Loop Pattern
- Fetch one page with limit + filters (+ cursor/offset if continuing)
- Summarize/extract needed info; drop raw page text from working memory
- Continue only if required and nextCursor/hasMore (or next offset) exists
- When using sequential thinking MCP restrict total_thoughts to 4

### Stop & Safety
- Stop when goal met or after 5 pages unless explicitly required
- If nearing budget (≥18k tokens) or latency rises, reduce limit by 50% and tighten filters
- No whole-dataset pulls; prefer filtered queries and targeted ranges
- Log decisions: e.g., "Read 3 pages @ 50/100/50; final output <18k tokens"
