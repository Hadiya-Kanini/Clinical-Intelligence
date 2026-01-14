# Findings Registry (Active)

> **Constraints**: 40 entries max (oldest entries archived when limit reached)

## Index
<!-- Maps file paths to finding IDs for O(1) lookup -->
```yaml
# Example: src/auth/login.ts: [F001, F003]
```

## Active Findings
```yaml
# Format:
# - id: F<seq>
#   file: <relative path>
#   cat: security|performance|architecture|quality
#   issue: <max 10 words>
#   cause: <root cause - max 20 words>
```

## Archive Reference
- Archived findings: `.propel/learnings/findings-archive.md`
- Archive is write-only (never read to save tokens)
