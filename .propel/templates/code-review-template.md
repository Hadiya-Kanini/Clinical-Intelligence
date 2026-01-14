# Code Review Report

## Executive Summary

### Review Scope
- **Files Changed**: {{FILES_CHANGED}}
- **Lines Added**: +{{LINES_ADDED}}
- **Lines Deleted**: -{{LINES_DELETED}}
- **Net Change**: {{NET_CHANGE}} lines

### Technology Stack Detected
{{TECHNOLOGY_STACK}}

### Overall Assessment
**Recommendation**: [Approve / Approve with Changes / Reject]
**Confidence Level**: [High / Medium / Low]

### Review Summary
[Brief 2-3 sentence summary of review findings and recommendation]

---

## Findings Summary

| Severity | Count | Critical Actions Required |
|----------|-------|---------------------------|
| CRITICAL | {{CRITICAL_COUNT}} | {{CRITICAL_ACTIONS}} |
| HIGH | {{HIGH_COUNT}} | {{HIGH_ACTIONS}} |
| MEDIUM | {{MEDIUM_COUNT}} | {{MEDIUM_ACTIONS}} |
| LOW | {{LOW_COUNT}} | {{LOW_ACTIONS}} |
| INFO | {{INFO_COUNT}} | {{INFO_ACTIONS}} |

---

## Critical Issues

{{#CRITICAL_ISSUES}}
### [{{CATEGORY}}] {{ISSUE_TITLE}}

**File**: `{{FILE_PATH}}:{{LINE_NUMBER}}`
**Severity**: CRITICAL

**Issue Description**:
{{ISSUE_DESCRIPTION}}

**Code Snippet**:
```{{LANGUAGE}}
{{CODE_SNIPPET}}
```

**Impact**:
{{IMPACT_DESCRIPTION}}

**Fix Recommendation**:
{{FIX_RECOMMENDATION}}

**Code Example**:
```{{LANGUAGE}}
{{FIX_CODE_EXAMPLE}}
```

**References**:
- {{REFERENCE_LINKS}}

---
{{/CRITICAL_ISSUES}}

## High Severity Issues

{{#HIGH_ISSUES}}
### [{{CATEGORY}}] {{ISSUE_TITLE}}

**File**: `{{FILE_PATH}}:{{LINE_NUMBER}}`
**Severity**: HIGH

**Issue Description**:
{{ISSUE_DESCRIPTION}}

**Code Snippet**:
```{{LANGUAGE}}
{{CODE_SNIPPET}}
```

**Impact**:
{{IMPACT_DESCRIPTION}}

**Fix Recommendation**:
{{FIX_RECOMMENDATION}}

**References**:
- {{REFERENCE_LINKS}}

---
{{/HIGH_ISSUES}}

## Medium Severity Issues

{{#MEDIUM_ISSUES}}
### [{{CATEGORY}}] {{ISSUE_TITLE}}

**File**: `{{FILE_PATH}}:{{LINE_NUMBER}}`
**Severity**: MEDIUM

**Issue Description**:
{{ISSUE_DESCRIPTION}}

**Fix Recommendation**:
{{FIX_RECOMMENDATION}}

---
{{/MEDIUM_ISSUES}}

## Low Severity Issues

{{#LOW_ISSUES}}
### [{{CATEGORY}}] {{ISSUE_TITLE}}

**File**: `{{FILE_PATH}}:{{LINE_NUMBER}}`
**Issue**: {{ISSUE_DESCRIPTION}}
**Fix**: {{FIX_RECOMMENDATION}}

---
{{/LOW_ISSUES}}

## Informational Notes

{{#INFO_ISSUES}}
### [{{CATEGORY}}] {{ISSUE_TITLE}}

**File**: `{{FILE_PATH}}:{{LINE_NUMBER}}`
**Note**: {{ISSUE_DESCRIPTION}}
**Suggestion**: {{FIX_RECOMMENDATION}}

---
{{/INFO_ISSUES}}

## Improvement Suggestions

{{#SUGGESTIONS}}
### {{SUGGESTION_TITLE}}

**Area**: {{AREA}}
**Benefit**: {{BENEFIT_DESCRIPTION}}
**Effort**: [Low / Medium / High]

**Details**:
{{SUGGESTION_DETAILS}}

**Example**:
```{{LANGUAGE}}
{{EXAMPLE_CODE}}
```

---
{{/SUGGESTIONS}}

## Positive Findings

{{#POSITIVE_FINDINGS}}
- **{{POSITIVE_CATEGORY}}**: {{POSITIVE_DESCRIPTION}}
{{/POSITIVE_FINDINGS}}

---

## Static Analysis Results

### Linting Summary
{{#LINTING_RESULTS}}
**Tool**: {{TOOL_NAME}}
**Errors**: {{ERROR_COUNT}}
**Warnings**: {{WARNING_COUNT}}
**Status**: {{STATUS}}

{{TOOL_SUMMARY}}
{{/LINTING_RESULTS}}

### Security Scanning
{{#SECURITY_SCAN}}
**Tool**: {{TOOL_NAME}}
**Vulnerabilities Found**: {{VULNERABILITY_COUNT}}
**Critical**: {{CRITICAL_VULN_COUNT}}
**High**: {{HIGH_VULN_COUNT}}

{{SECURITY_SUMMARY}}
{{/SECURITY_SCAN}}

### Code Coverage
{{#COVERAGE_RESULTS}}
**Coverage**: {{COVERAGE_PERCENTAGE}}%
**Lines Covered**: {{LINES_COVERED}} / {{TOTAL_LINES}}
**Branches Covered**: {{BRANCHES_COVERED}} / {{TOTAL_BRANCHES}}
{{/COVERAGE_RESULTS}}

---

## Licensing and Legal Review

### License Compliance
{{#LICENSE_COMPLIANCE}}
**Status**: [Compliant / Issues Found / Unknown]

**Repository License**: {{REPO_LICENSE}}

**Third-Party Dependencies**:
{{#DEPENDENCIES}}
- **{{PACKAGE_NAME}}** ({{LICENSE_TYPE}}): {{COMPLIANCE_STATUS}}
{{/DEPENDENCIES}}

**GPL/Copyleft Violations**: {{GPL_VIOLATIONS}}
{{/LICENSE_COMPLIANCE}}

### Public Code Attribution
{{#PUBLIC_CODE_ATTRIBUTION}}
**Source**: {{SOURCE_URL}}
**File**: `{{FILE_PATH}}`
**License**: {{LICENSE_TYPE}}
**Attribution Required**: [Yes / No]
**Status**: [Properly Attributed / Missing Attribution / As-Is Copy]

**Note**: {{ATTRIBUTION_NOTE}}
{{/PUBLIC_CODE_ATTRIBUTION}}

---

## Architecture Review

### Design Patterns Observed
{{#DESIGN_PATTERNS}}
- **{{PATTERN_NAME}}**: {{PATTERN_USAGE_DESCRIPTION}}
{{/DESIGN_PATTERNS}}

### SOLID Principles Assessment
| Principle | Status | Notes |
|-----------|--------|-------|
| Single Responsibility | {{SRP_STATUS}} | {{SRP_NOTES}} |
| Open/Closed | {{OCP_STATUS}} | {{OCP_NOTES}} |
| Liskov Substitution | {{LSP_STATUS}} | {{LSP_NOTES}} |
| Interface Segregation | {{ISP_STATUS}} | {{ISP_NOTES}} |
| Dependency Inversion | {{DIP_STATUS}} | {{DIP_NOTES}} |

### Anti-Patterns Detected
{{#ANTI_PATTERNS}}
- **{{ANTI_PATTERN_NAME}}**: {{LOCATION}} - {{DESCRIPTION}}
{{/ANTI_PATTERNS}}

### Architectural Concerns
{{#ARCH_CONCERNS}}
**Concern**: {{CONCERN_TITLE}}
**Impact**: {{IMPACT_LEVEL}}
**Description**: {{CONCERN_DESCRIPTION}}
**Recommendation**: {{RECOMMENDATION}}
{{/ARCH_CONCERNS}}

---

## Technology-Specific Analysis

{{#FRONTEND_ANALYSIS}}
### Frontend (React/Angular/Vue)

#### Component Analysis
{{#COMPONENTS}}
- **{{COMPONENT_NAME}}**: {{COMPONENT_STATUS}}
  - Lines: {{LINES_COUNT}}
  - Responsibilities: {{RESPONSIBILITIES_COUNT}}
  - Issues: {{ISSUES_SUMMARY}}
{{/COMPONENTS}}

#### Accessibility (WCAG 2.1)
{{#ACCESSIBILITY}}
- **Level AA Compliance**: {{WCAG_AA_STATUS}}
- **Issues Found**: {{A11Y_ISSUES_COUNT}}
{{/ACCESSIBILITY}}

#### Performance
{{#PERFORMANCE}}
- **Bundle Size**: {{BUNDLE_SIZE}}
- **Re-render Issues**: {{RERENDER_ISSUES}}
- **Optimization Opportunities**: {{OPTIMIZATION_COUNT}}
{{/PERFORMANCE}}
{{/FRONTEND_ANALYSIS}}

{{#BACKEND_ANALYSIS}}
### Backend (.NET/Java/Node.js/Python)

#### API Design
{{#API_DESIGN}}
- **Endpoints Reviewed**: {{ENDPOINT_COUNT}}
- **REST Compliance**: {{REST_COMPLIANCE_STATUS}}
- **Error Handling**: {{ERROR_HANDLING_STATUS}}
{{/API_DESIGN}}

#### Database Interactions
{{#DATABASE}}
- **Queries Reviewed**: {{QUERY_COUNT}}
- **N+1 Issues**: {{N_PLUS_ONE_COUNT}}
- **Missing Indexes**: {{MISSING_INDEXES_COUNT}}
- **Transaction Safety**: {{TRANSACTION_STATUS}}
{{/DATABASE}}

#### Concurrency & Threading
{{#CONCURRENCY}}
- **Async/Await Patterns**: {{ASYNC_STATUS}}
- **Thread Safety**: {{THREAD_SAFETY_STATUS}}
- **Potential Race Conditions**: {{RACE_CONDITION_COUNT}}
{{/CONCURRENCY}}
{{/BACKEND_ANALYSIS}}

{{#DATABASE_ANALYSIS}}
### Database (SQL/Migrations)

#### Migration Safety
{{#MIGRATIONS}}
- **Data Loss Risk**: {{DATA_LOSS_RISK}}
- **Rollback Support**: {{ROLLBACK_STATUS}}
- **Idempotency**: {{IDEMPOTENCY_STATUS}}
{{/MIGRATIONS}}

#### Query Optimization
{{#QUERIES}}
- **Queries Analyzed**: {{QUERY_COUNT}}
- **Performance Issues**: {{PERFORMANCE_ISSUE_COUNT}}
- **Index Recommendations**: {{INDEX_RECOMMENDATIONS}}
{{/QUERIES}}
{{/DATABASE_ANALYSIS}}

---

## Testing Assessment

### Test Coverage
{{#TEST_COVERAGE}}
- **Unit Tests**: {{UNIT_TEST_COUNT}} (Coverage: {{UNIT_COVERAGE}}%)
- **Integration Tests**: {{INTEGRATION_TEST_COUNT}}
- **E2E Tests**: {{E2E_TEST_COUNT}}
{{/TEST_COVERAGE}}

### Test Quality
{{#TEST_QUALITY}}
- **Deterministic Tests**: {{DETERMINISTIC_STATUS}}
- **Test Isolation**: {{ISOLATION_STATUS}}
- **Mock Usage**: {{MOCK_USAGE_STATUS}}
- **Coverage Gaps**: {{COVERAGE_GAPS}}
{{/TEST_QUALITY}}

### Missing Tests
{{#MISSING_TESTS}}
- **{{TEST_AREA}}**: {{MISSING_TEST_DESCRIPTION}}
{{/MISSING_TESTS}}

---

## Security Assessment (OWASP Top 10)

| OWASP Category | Status | Findings |
|----------------|--------|----------|
| A01: Broken Access Control | {{A01_STATUS}} | {{A01_FINDINGS}} |
| A02: Cryptographic Failures | {{A02_STATUS}} | {{A02_FINDINGS}} |
| A03: Injection | {{A03_STATUS}} | {{A03_FINDINGS}} |
| A04: Insecure Design | {{A04_STATUS}} | {{A04_FINDINGS}} |
| A05: Security Misconfiguration | {{A05_STATUS}} | {{A05_FINDINGS}} |
| A06: Vulnerable Components | {{A06_STATUS}} | {{A06_FINDINGS}} |
| A07: Authentication Failures | {{A07_STATUS}} | {{A07_FINDINGS}} |
| A08: Data Integrity Failures | {{A08_STATUS}} | {{A08_FINDINGS}} |
| A09: Logging Failures | {{A09_STATUS}} | {{A09_FINDINGS}} |
| A10: Server-Side Request Forgery | {{A10_STATUS}} | {{A10_FINDINGS}} |

---

## Next Steps

### Required Actions
{{#REQUIRED_ACTIONS}}
1. **{{ACTION_TITLE}}** ({{SEVERITY}})
   - Files: {{AFFECTED_FILES}}
   - Action: {{ACTION_DESCRIPTION}}
{{/REQUIRED_ACTIONS}}

### Recommended Improvements
{{#RECOMMENDED_IMPROVEMENTS}}
1. **{{IMPROVEMENT_TITLE}}** (Priority: {{PRIORITY}})
   - Benefit: {{BENEFIT}}
   - Effort: {{EFFORT_ESTIMATE}}
{{/RECOMMENDED_IMPROVEMENTS}}

### Follow-Up Items
{{#FOLLOWUP_ITEMS}}
- {{FOLLOWUP_DESCRIPTION}}
{{/FOLLOWUP_ITEMS}}

---

## Review Metadata

**Review Duration**: {{REVIEW_DURATION}}
**Tools Used**: {{TOOLS_LIST}}
**Standards Applied**: {{STANDARDS_LIST}}
**Context7 Libraries Referenced**: {{CONTEXT7_LIBRARIES}}

---

*This review was conducted using automated analysis tools and AI-assisted code review. All findings should be validated by human reviewers before taking action.*
