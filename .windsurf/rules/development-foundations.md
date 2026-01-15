---
trigger: always_on
---

# Reference Guidelines

## Existing Task Analysis
Before creating new tasks:
1. Review `./.propel/context/tasks` for existing patterns
2. Understand current task structure and naming conventions
3. Build dependency mapping with existing tasks
4. Identify reusable components or patterns

## For New Projects (No Existing Codebase)
1. Determine target language and technology stack
2. Research language-specific best practices and project templates
3. Consider industry standards for chosen stack
4. Plan scaffolding tasks based on complexity and requirements

## Test Script Development
Before creating test scripts:
1. Review `./.propel/context/tests` for existing patterns
2. Understand current testing framework and conventions
3. Consider updating existing scripts for new enhancements
4. Follow established test naming and organization patterns

## Task Dependencies

### Dependency Mapping
- Map dependencies between FE, BE, and DB tasks
- Identify blocking dependencies and critical path items
- Document prerequisite tasks and deliverables
- Consider parallel execution opportunities

### Integration Points
- Define clear handoff points between streams
- Specify interface contracts between components
- Plan integration testing at appropriate milestones

### Task Documentation
- Include clear task descriptions and acceptance criteria
- Document expected inputs and outputs
- Specify testing requirements for each task
- Include relevant context from existing codebase

### Quality Assurance
- Plan testing tasks alongside development tasks
- Include code review tasks where appropriate
- Consider performance and security implications
- Plan documentation updates as separate tasks when needed

