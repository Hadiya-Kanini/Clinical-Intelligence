# E2E Test Plan: [PROJECT_NAME | FEATURE_NAME]

## 1. Test Objectives
- [Objective 1: Quality attribute being validated]
- [Objective 2: User journeys covered]
- [Objective 3: Risks being mitigated]

## 2. Scope

### In Scope
| Category | Items | Requirement IDs |
|----------|-------|-----------------|
| Functional | [Features] | FR-001, FR-002 |
| User Journeys | [Critical flows] | UC-001, UC-002 |
| Non-Functional | [Performance, Security] | NFR-001, NFR-002 |
| Technical | [Integrations, APIs] | TR-001 |
| Data | [Validation, Integrity] | DR-001 |

### Out of Scope
- [Items explicitly excluded with rationale]

## 3. Test Strategy

### Test Pyramid Allocation
| Level | Coverage Target | Focus |
|-------|-----------------|-------|
| E2E | 5-10% | Critical user journeys only |
| Integration | 20-30% | API contracts, service boundaries |
| Unit | 60-70% | Business logic, edge cases |

### E2E Approach
- **Horizontal**: UI-driven user flows (login, checkout, etc.)
- **Vertical**: API -> DB validation for data integrity

### Environment Strategy
| Environment | Purpose | Data Strategy |
|-------------|---------|---------------|
| DEV | Smoke tests | Mocked/Seeded |
| QA | Full regression | Snapshot data |
| Staging | Pre-prod validation | Prod-like data |

## 4. Test Cases

### 4.1 Functional Test Cases

#### TC-FR-[REQ_ID]-[SEQ]: [Test Case Name]
| Field | Value |
|-------|-------|
| Requirement | FR-[REQ_ID] |
| Use Case | UC-[UC_ID] |
| Type | [happy_path/edge_case/error] |

**Preconditions:**
- [System state before test]

**Test Steps:**
| Step | Given | When | Then |
|------|-------|------|------|
| 1 | [Initial state/context] | [Action performed] | [Expected outcome] |
| 2 | [State after step 1] | [Next action] | [Expected result] |
| 3 | [State after step 2] | [Final action] | [Final expected state] |

**Test Data:**
| Field | Valid Value | Invalid Value | Boundary Value |
|-------|-------------|---------------|----------------|
| [field_1] | [valid] | [invalid] | [boundary] |
| [field_2] | [valid] | [invalid] | [boundary] |

**Expected Results:**
- [ ] [Assertion 1]
- [ ] [Assertion 2]

**Postconditions:**
- [System state after test]

---

### 4.2 NFR Test Cases

#### TC-NFR-[REQ_ID]-PERF: Performance Validation
| Field | Value |
|-------|-------|
| Requirement | NFR-[REQ_ID] |
| Category | Performance |

**Preconditions:**
- System deployed in [environment]
- Baseline load established

**Test Steps:**
| Step | Given | When | Then |
|------|-------|------|------|
| 1 | System at baseline load | [X] concurrent users execute [journey] | Response time P95 < [threshold] |
| 2 | System under load | Monitor error rates | Error rate < [threshold]% |
| 3 | Peak load reached | Measure throughput | Throughput >= [X] req/sec |

**Acceptance Criteria:**
- [ ] Response time P95 < [X]s
- [ ] Error rate < [X]%
- [ ] Throughput >= [X] requests/second
- [ ] No memory leaks detected
- [ ] CPU utilization < [X]%

---

#### TC-NFR-[REQ_ID]-SEC: Security Validation
| Field | Value |
|-------|-------|
| Requirement | NFR-[REQ_ID] |
| Category | Security |

**Preconditions:**
- Application endpoints accessible
- Test credentials available

**Test Steps:**
| Step | Given | When | Then |
|------|-------|------|------|
| 1 | Authentication endpoints exposed | Attempt unauthorized access | Access denied with proper error |
| 2 | Input fields available | Submit injection payloads | Input sanitized, no injection |
| 3 | Session established | Attempt session hijacking | Session protected |

**Acceptance Criteria:**
- [ ] No Critical/High vulnerabilities
- [ ] OWASP Top 10 mitigated
- [ ] Authentication enforced on protected endpoints
- [ ] Authorization rules validated
- [ ] Sensitive data encrypted

---

#### TC-NFR-[REQ_ID]-SCALE: Scalability Validation
| Field | Value |
|-------|-------|
| Requirement | NFR-[REQ_ID] |
| Category | Scalability |

**Test Steps:**
| Step | Given | When | Then |
|------|-------|------|------|
| 1 | System at baseline [X] users | Ramp to [Y] users | System scales without degradation |
| 2 | Peak load [Y] users | Sustain for [duration] | Performance within thresholds |
| 3 | Load reduced | Scale down | Resources released properly |

**Acceptance Criteria:**
- [ ] System scales to [X] concurrent users
- [ ] Auto-scaling triggers at [threshold]
- [ ] No degradation beyond [acceptable limit]

---

### 4.3 Technical Requirement Test Cases

#### TC-TR-[REQ_ID]: [Integration/API Name]
| Field | Value |
|-------|-------|
| Requirement | TR-[REQ_ID] |
| Category | [Integration/API/Platform] |

**Preconditions:**
- [External system/API available]
- [Authentication configured]

**Test Steps:**
| Step | Given | When | Then |
|------|-------|------|------|
| 1 | [Integration state] | [API call/action] | [Expected response] |
| 2 | [Valid request] | [Submit request] | [Success response with correct format] |
| 3 | [Invalid request] | [Submit malformed request] | [Proper error handling] |

**Validation Points:**
- [ ] Contract compliance verified
- [ ] Response format matches specification
- [ ] Error codes returned correctly
- [ ] Timeout handling works

---

### 4.4 Data Requirement Test Cases

#### TC-DR-[REQ_ID]: [Data Validation Name]
| Field | Value |
|-------|-------|
| Requirement | DR-[REQ_ID] |
| Category | [Integrity/Retention/Migration] |

**Preconditions:**
- [Database state]
- [Test data seeded]

**Test Steps:**
| Step | Given | When | Then |
|------|-------|------|------|
| 1 | [Initial data state] | [Data operation] | [Expected data state] |
| 2 | [Related records exist] | [Cascading operation] | [Referential integrity maintained] |
| 3 | [Audit logging enabled] | [Data modification] | [Audit trail created] |

**Validation Points:**
- [ ] Data integrity preserved
- [ ] Referential integrity maintained
- [ ] Audit trail complete
- [ ] Data retention policy enforced

---

### 4.5 E2E Journey Test Cases

#### E2E-[JOURNEY_ID]: [Journey Name]
| Field | Value |
|-------|-------|
| UC Chain | UC-[ID1] -> UC-[ID2] -> UC-[ID3] |
| Session | [Auth required/Guest] |

**Preconditions:**
- [User role/permissions]
- [System state]
- [Test data available]

**Journey Flow:**
| Phase | Use Case | Action | Expected State | Checkpoint |
|-------|----------|--------|----------------|------------|
| 1 | UC-[ID1] | [User action] | [System state after action] | Y |
| 2 | UC-[ID2] | [Next action] | [Next state] | N |
| 3 | UC-[ID3] | [Final action] | [Journey complete state] | Y |

**Detailed Test Steps:**

**Phase 1: UC-[ID1] - [Name]**
| Step | Given | When | Then |
|------|-------|------|------|
| 1.1 | [Initial journey state] | [First action] | [Expected outcome] |
| 1.2 | [State after 1.1] | [Checkpoint validation] | [Phase 1 complete] |

**Phase 2: UC-[ID2] - [Name]**
| Step | Given | When | Then |
|------|-------|------|------|
| 2.1 | [State from Phase 1] | [Continue action] | [Expected outcome] |
| 2.2 | [State after 2.1] | [Next action] | [Phase 2 complete] |

**Phase 3: UC-[ID3] - [Name]**
| Step | Given | When | Then |
|------|-------|------|------|
| 3.1 | [State from Phase 2] | [Final action] | [Expected outcome] |
| 3.2 | [Journey end state] | [Verify completion] | [Journey success] |

**Test Data:**
| Entity | Field | Value |
|--------|-------|-------|
| User | Role | [user_role] |
| User | Credentials | [test_user] |
| [Entity] | [field] | [value] |

**Expected Results:**
- [ ] All phases complete without errors
- [ ] Session state maintained across phases
- [ ] Checkpoints validate intermediate states
- [ ] Final state matches success criteria

---

## 5. Entry & Exit Criteria

### Entry Criteria
- [ ] All FR-XXX requirements approved and baselined
- [ ] Test environment provisioned and accessible
- [ ] Test data seeded or available
- [ ] Test cases reviewed and approved
- [ ] Relevant NFR/TR/DR requirements reviewed

### Exit Criteria
- [ ] 100% P0 test cases executed
- [ ] >=95% P0 test cases passed
- [ ] >=90% P1 test cases passed
- [ ] No open Critical/High severity defects
- [ ] NFR thresholds validated
- [ ] All E2E journeys pass end-to-end

## 6. Risk Assessment

| Risk-ID | Risk Description | Impact | Likelihood | Mitigation |
|---------|------------------|--------|------------|------------|
| R-001 | [Risk description] | [High/Medium/Low] | [High/Medium/Low] | [Mitigation strategy] |
| R-002 | [Risk description] | [High/Medium/Low] | [High/Medium/Low] | [Mitigation strategy] |

### Risk-Based Test Prioritization
| Priority | Criteria | Test Focus |
|----------|----------|------------|
| P0 (Must Test) | Impact=High AND Likelihood>=Medium | Critical paths, security, data integrity |
| P1 (Should Test) | Impact=Medium OR Likelihood=High | Feature completeness, edge cases |
| P2 (Could Test) | Remaining scenarios | Nice-to-have validations |

## 7. Traceability Matrix

| Requirement | Type | Priority | Test Cases | E2E Journey | Status |
|-------------|------|----------|------------|-------------|--------|
| FR-001 | Functional | P0 | TC-FR-001-01, TC-FR-001-02 | E2E-001 | [Planned/In Progress/Complete] |
| UC-001 | Use Case | P0 | TC-UC-001-HP, TC-UC-001-EC | E2E-001 | [Planned/In Progress/Complete] |
| NFR-001 | Non-Functional | P0 | TC-NFR-001-PERF | - | [Planned/In Progress/Complete] |
| NFR-002 | Non-Functional | P0 | TC-NFR-002-SEC | - | [Planned/In Progress/Complete] |
| TR-001 | Technical | P1 | TC-TR-001 | E2E-002 | [Planned/In Progress/Complete] |
| DR-001 | Data | P1 | TC-DR-001 | - | [Planned/In Progress/Complete] |

**Note:** Priority derived from Risk Assessment (Section 6) based on Impact × Likelihood.

## 8. Test Data Requirements

| Scenario Type | Data Description | Source | Isolation |
|---------------|------------------|--------|-----------|
| Happy Path | Valid data for success flows | Seeded fixtures | Test-specific |
| Edge Cases | Boundary values | Generated | Shared read-only |
| Error Cases | Invalid/malformed data | Static fixtures | Shared read-only |
| E2E Journeys | Complete user datasets | Journey-specific | Journey-isolated |

### Sensitive Data Handling
- [ ] Production data masked/anonymized
- [ ] PII replaced with synthetic data
- [ ] Credentials stored securely

## 9. Defect Management

| Severity | Definition | SLA | Action |
|----------|------------|-----|--------|
| Critical | System unusable, data loss, security breach | Immediate | Block release |
| High | Major feature broken, no workaround | Before release | Must fix |
| Medium | Feature impacted, workaround exists | Next sprint | Should fix |
| Low | Minor issue, cosmetic | Backlog | Could fix |

---
*Template: test-plan-template.md | Output: .propel/context/test/test_plan_[feature]_[YYYYMMDD].md*
