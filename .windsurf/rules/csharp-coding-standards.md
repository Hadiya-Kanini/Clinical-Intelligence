---
trigger: glob
globs: "**/*.cs"
---

# C# Development

## General
- High confidence suggestions only when reviewing code
- Document design decisions in comments
- Handle edge cases with clear exception handling

## Naming Conventions
- PascalCase for components, methods, public members
- camelCase for private fields, local variables
- Prefix interfaces with "I" (e.g., IUserService)

## Formatting
- Follow `.editorconfig` style
- File-scoped namespaces, single-line using directives
- Newline before opening braces
- Final return statement on its own line
- Pattern matching and switch expressions over explicit checks
- Use `nameof` instead of string literals
- XML doc comments for public APIs with `<example>` and `<code>` tags

## Project Structure
- Feature folders or domain-driven design organization
- Separate models, services, and data access layers
- Environment-specific configuration in Program.cs

## Nullable Reference Types
- Declare non-nullable; check at entry points only
- Use `is null` or `is not null` (not `==`/`!=`)
- Trust null annotations; avoid redundant checks

## Data Access
- Entity Framework Core for data layer
- Repository pattern when beneficial
- Database migrations and seeding
- Efficient query patterns to avoid N+1 problems

## Authentication & Authorization
- JWT Bearer tokens or OAuth 2.0/OpenID Connect
- Role-based and policy-based authorization
- Microsoft Entra ID integration when applicable
- Consistent security for controller-based and Minimal APIs

## Validation & Error Handling
- Data annotations or FluentValidation
- Global exception handling middleware
- RFC 7807 problem details for standardized responses

## API Documentation
- Swagger/OpenAPI with endpoint, parameter, response docs
- Version APIs (URL, header, or query string)
- Meaningful documentation for consumers

## Logging
- Structured logging (Serilog or alternatives)
- Appropriate log levels
- Application Insights for telemetry
- Correlation IDs for request tracking

## Testing
- Test critical paths only
- No "Act", "Arrange", "Assert" comments
- Match existing test style and naming
- Mock dependencies for isolation

## Performance
- Caching strategies (in-memory, distributed, response)
- Async/await patterns throughout
- Pagination, filtering, sorting for large datasets
- Compression and benchmarking

## Deployment
- Use .NET's built-in container support: `dotnet publish --os linux --arch x64 -p:PublishProfile=DefaultContainer`
- Health checks and readiness probes
- Environment-specific configuration per deployment stage
