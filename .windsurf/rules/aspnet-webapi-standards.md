---
trigger: glob
globs: "**/*.cs, **/*.json"
---

# ASP.NET REST API Development

ASP.NET Core 9 REST APIs with Controllers and Minimal API styles.

## API Design
- REST principles: resource-oriented URLs, appropriate HTTP verbs
- Controllers vs Minimal APIs based on project needs
- Status codes, content negotiation, response formatting

## Project Structure
- Feature folders or domain-driven design
- Separation: models, services, data access layers
- Program.cs configuration with environment-specific settings

## Controllers
- Attribute routing for resource naming
- [ApiController] for model binding and validation
- Dependency injection in constructors
- Return types: IActionResult, ActionResult<T>, specific types

## Minimal APIs
- Endpoint routing with route groups
- Parameter binding and validation
- DI in route handlers
- Structure for maintainability

## Data Access
- Entity Framework Core with appropriate providers
- Repository pattern when beneficial
- Database migrations and seeding
- Efficient query patterns (avoid N+1)

## Authentication & Authorization
- JWT Bearer tokens or OAuth 2.0/OpenID Connect
- Role-based and policy-based authorization
- Microsoft Entra ID integration when applicable
- Consistent security for Controllers and Minimal APIs

## Validation & Error Handling
- Model validation with data annotations or FluentValidation
- Global exception handling middleware
- RFC 7807 problem details for standardized responses

## API Versioning & Documentation
- Versioning strategies (URL, header, query string)
- Swagger/OpenAPI with proper documentation
- Document endpoints, parameters, responses, auth

## Logging & Monitoring
- Structured logging (Serilog or alternatives)
- Application Insights for telemetry
- Correlation IDs for request tracking

## Testing
- Unit tests for controllers, endpoints, services
- Integration tests for API endpoints
- Mock dependencies effectively
- Test auth and authorization logic

## Performance
- Caching strategies (in-memory, distributed, response)
- Async/await throughout
- Pagination, filtering, sorting for large datasets
- Compression and benchmarking

## Deployment
- .NET's built-in container support: `dotnet publish --os linux --arch x64 -p:PublishProfile=DefaultContainer`
- Health checks and readiness probes
- Environment-specific configuration per stage
