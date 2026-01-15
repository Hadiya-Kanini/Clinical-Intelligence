---
trigger: glob
description: Self-explanatory code with minimal comments. Comment WHY, not WHAT.
globs: "**/*.{cs,csproj,fs,fsproj,go,py,rb,java,kt,ts,js,jsx,tsx,sql,yml,yaml,json,Dockerfile}"
---

# Code Documentation Standards

**Core Principle:** Write code that speaks for itself. Comment only when necessary to explain WHY, not WHAT.

## ❌ Avoid
- Obvious comments (e.g., `counter++; // Increment counter`)
- Redundant comments repeating the code
- Outdated comments not matching the code
- Dead code comments
- Changelog comments in code
- Decorative divider comments

## ✅ Comment When Needed
- Complex business logic and algorithms
- Non-obvious algorithm choices
- Regex patterns
- API constraints and external limitations
- Configuration and constant rationale
- Public API documentation (JSDoc, XML docs)

## Decision Framework
1. Is the code self-explanatory? → No comment
2. Would better naming eliminate the need? → Refactor instead
3. Does this explain WHY, not WHAT? → Good comment
4. Will this help future maintainers? → Good comment

## Annotations
Use standard tags:
- `TODO:` - Work to be done
- `FIXME:` - Known issues needing investigation
- `HACK:` - Temporary workaround with reason
- `NOTE:` - Important implementation detail
- `WARNING:` - Dangerous behavior or side effects
- `PERF:` - Performance consideration
- `SECURITY:` - Security-related concern
- `DEPRECATED:` - Marked for removal

## Quality Checklist
- Explains WHY, not WHAT
- Grammatically correct
- Will remain accurate
- Adds genuine value
- Placed above code it describes
- Professional language

**Remember:** The best comment is the one you don't need to write.