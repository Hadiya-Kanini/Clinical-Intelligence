# Backend API (.NET)

This directory contains the ASP.NET Core backend API. It handles business logic and data persistence, exposing endpoints for the Web UI and interacting with the AI Worker.

## Database migrations (EF Core)

The API uses EF Core migrations to version and apply database schema changes.

Migrations are stored under `Server/ClinicalIntelligence.Api/Migrations`.

Do not apply schema changes by manually editing the database. All schema changes must be captured as EF Core migrations and committed.

### Create a new migration

Run from the repository root:

`dotnet ef migrations add <MigrationName> --project Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj --startup-project Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj --output-dir Migrations`

### Apply migrations

Run from the repository root:

`dotnet ef database update --project Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj`

In Development, the API also applies pending migrations automatically on startup.

### Configure the connection string

Provide either:

- `ConnectionStrings:DefaultConnection` (recommended)
- `DATABASE_CONNECTION_STRING`

Example (PostgreSQL):
```
DATABASE_CONNECTION_STRING="Host=localhost;Database=TrustFirstPlatform;Username=postgres;Password=your_password_here"
```

In Development, if neither is provided, the API will default to a local SQLite database at `Data Source=clinicalintelligence.db`.

In non-Development environments, the API will fail fast at startup if no connection string is provided.

## Secret rotation

Secrets are loaded at startup from the configured sources (environment variables and, in Development, user-secrets). To rotate a secret:

1. Update the value in the configuration source (e.g., environment variable or user-secrets).
2. Restart the API process.
