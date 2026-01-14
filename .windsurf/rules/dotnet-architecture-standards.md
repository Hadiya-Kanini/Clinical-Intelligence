---
trigger: glob
globs: "**/*.cs,**/*.csproj,**/Program.cs,**/*.razor"
---

# .NET Architecture Guidelines

SOLID principles and .NET best practices for robust, maintainable systems.

## Before Implementation
1. Explain SOLID principles applied and affected layers
2. Check SRP adherence and proper dependency inversion
3. Validate interface/class structure and test naming

## SOLID Principles
- **SRP**: One reason to change per class
- **OCP**: Open for extension, closed for modification
- **LSP**: Subtypes substitutable for base types
- **ISP**: No forced dependency on unused methods
- **DIP**: Depend on abstractions, not concretions

## .NET Best Practices
- async/await for I/O operations
- Built-in DI container for loose coupling
- LINQ for data manipulation
- Consistent exception handling and logging
- Modern C# features (records, pattern matching)

## Security & Compliance
- Authorization at aggregate level
- PCI-DSS, SOX, LGPD compliance
- Domain events for audit trails

## Performance
- Async operations with async/await
- Efficient queries and indexing
- Appropriate caching strategies
- Properly sized aggregates

## Layers

### Application
- Orchestrate domain operations
- DTOs for data transfer
- Input validation before business logic
- Constructor injection for dependencies

### Infrastructure
- Repositories for persistence (interfaces in domain layer)
- Event bus for domain events
- ORMs for database mapping
- External service adapters

## Testing
- Naming: `MethodName_Condition_ExpectedResult()`
- Unit tests for domain logic
- Integration tests for boundaries and persistence
- Acceptance tests for user scenarios
- Min 85% coverage for domain/application layers

## Test Categories
- Aggregates: business rules and state
- Value objects: immutability and equality
- Domain services: complex operations
- Events: publishing and handling
- Application services: orchestration and validation

## Financial Domain
- `decimal` for monetary values
- Currency-aware value objects
- Proper rounding per standards
- Saga patterns for distributed transactions
- Immutable audit trails via domain events
- Compensation patterns for rollback
