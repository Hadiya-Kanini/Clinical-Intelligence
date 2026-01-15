# Implementation Analysis -- <task_file_path>

## Verdict
**Status:** Pass | Conditional Pass | Fail  
**Summary:** <one-paragraph executive summary>

## Traceability Matrix
| Requirement / Acceptance Criterion | Evidence (file:fn/line) | Result |
|---|---|---|
| R1: ... | /path/File.cs: DoThing() L123 | Pass/Gap/Fail |
| R2: ... | ... | ... |

## Logical & Design Findings
- **Business Logic:** <misinterpretations, edge cases, invariants not enforced>
- **Security:** <authZ gaps, input validation, secrets/keys handling>
- **Error Handling:** <missing try/catch, error codes, retries/idempotency>
- **Data Access:** <transaction scope, N+1, missing indexes, async/await issues>
- **Frontend:** <state mgmt, error states, accessibility, API contract mismatches>
- **Performance:** <hot path issues, pagination, caching>
- **Patterns & Standards:** <deviation from layered pattern, naming, SOLID>

## Test Review
- **Existing Tests:** <list and coverage comments>  
- **Missing Tests (must add):**
  - [ ] Unit: ...
  - [ ] Integration: ...
  - [ ] Negative/Edge: ...

## Validation Results
- **Commands Executed:** <from task file>  
- **Outcomes:** <pass/fail logs summary>  

## Fix Plan (Prioritized)
1. <Fix> -- <files/functions> -- ETA <h> -- Risk: <L/M/H>
2. ...

## Appendix
- **Context7 References:** <bulleted doc endpoints used>  
- **Search Evidence:** <key grep patterns, paths>