---
trigger: glob
globs: "**/unittest/*.md"
---

# Test Strategies by Technology

## Standards
- **Write Minimal Tests During Development:** Do NOT write tests for every change or intermediate step. Focus on completing the feature implementation first, then add strategic tests only at logical completion points
- **Test Only Core User Flows:** Write tests exclusively for critical paths and primary user workflows. Skip writing tests for non-critical utilities and secondary workflows until if/when you're instructed to do so.
- **Test Behavior, Not Implementation:** Focus tests on what the code does, not how it does it, to reduce brittleness
- **Clear Test Names:** Use descriptive names that explain what's being tested and the expected outcome.
- **Fast Execution:** Keep unit tests fast (milliseconds) so developers run them frequently during development

## JavaScript/TypeScript (Jest/Vitest)
- Framework: Jest (pinned version)
- Structure: describe/it with beforeEach/afterEach hooks
- Mocking: jest.mock() for dependencies
- Assertions: expect() statements
- Location: `__tests__/` or `*.test.js/ts` alongside source
- Naming: `ComponentName.test.js/ts`

## Python (pytest/unittest)
- Framework: pytest (pinned version)
- Structure: test_ prefix for functions
- Setup: fixtures for setup/teardown, @pytest.mark decorators
- Assertions: assert statements
- Location: `tests/` directory
- Naming: `test_component_name.py`

## .NET (xUnit/NUnit/MSTest)
- Framework: xUnit (pinned version)
- Structure: [Fact]/[Theory] attributes
- Setup: IDisposable for setup/cleanup
- Mocking: Moq for dependencies
- Assertions: Assert.Equal/True/False
- Location: `ProjectName.Tests/` directory
- Naming: `ComponentNameTests.cs`

## Java (JUnit/TestNG)
- Framework: JUnit 5 (pinned version)
- Structure: @Test annotation with @BeforeEach/@AfterEach
- Mocking: Mockito for dependencies
- Assertions: assertEquals/assertTrue
- Location: `src/test/java/`
- Naming: `ComponentNameTest.java`

## Test Plan Example: React Component (Jest)

**User Story:** us_001 - User Login Form Component

**Components:**
- LoginForm.tsx, useAuthValidation.ts, authService.ts

**Test Cases:**
- Render form with email and password fields
- Validate email format on blur
- Validate password strength
- Submit form with valid credentials
- Display error on invalid credentials
- Handle empty form submission
- Handle network timeout
- Handle special characters in password

**Mocks:**
- authService.login() → mock API call
- useRouter() → mock navigation