# Task - TASK_001

## Requirement Reference
- User Story: us_005
- Story Location: .propel/context/tasks/us_005/us_005.md
- Acceptance Criteria: 
    - Given the Backend API requires a database connection string, When the API is configured for an environment, Then the connection string is supplied via an environment-appropriate secure mechanism (e.g., environment variables, local secrets store for development) and is not committed in plaintext.
    - Given the AI Worker requires external API keys (e.g., Gemini), When the worker is configured for an environment, Then API keys are supplied via secure configuration and are not committed in plaintext.
    - Given a required secret is missing, When the service starts, Then it fails fast with a non-sensitive error message using standardized error conventions (where applicable) and does not log the secret value.
    - Given secrets need rotation, When values are updated in the configuration source, Then services can be restarted to pick up new values without code changes.

## Task Overview
Implement secure, per-environment configuration management for secrets across the Backend API and AI Worker. Remove plaintext secrets from source control, enforce required-secret validation at startup, and standardize non-sensitive failure behavior. Ensure local development supports secure secret storage (e.g., ASP.NET Core user-secrets for the API) while production-like environments rely on environment variables or platform secret stores.

## Dependent Tasks
- .propel/context/tasks/us_001/task_001_scaffold_service_structure.md (TASK_001)

## Impacted Components
- Server/ClinicalIntelligence.Api/Program.cs
- Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj
- worker/main.py
- worker/requirements.txt
- .gitignore
- .env (root)

## Implementation Plan
- Define a consistent secret naming convention per service (e.g., `DATABASE_CONNECTION_STRING`, `GEMINI_API_KEY`) and document required variables.
- Backend API (.NET):
  - Introduce a startup-time configuration validation step that verifies required secret values are present for the current environment.
  - Use environment variables for all environments; in Development allow ASP.NET Core user-secrets as an additional secure source.
  - Fail fast if the required database connection string is missing for non-development environments, using a non-sensitive exception message.
  - Ensure connection strings are never written to logs.
- AI Worker (Python):
  - Centralize worker configuration loading (env-first) and validate required API keys at startup.
  - For local development, support loading `.env` from disk (gitignored) for developer convenience; ensure it is never committed.
  - Fail fast with non-sensitive error messages when keys are missing/malformed and avoid logging secret values.
- Repository hygiene:
  - Remove committed plaintext `.env` from the repo and replace with a non-secret `.env.example` with placeholders.
  - Update `.gitignore` to ignore `.env` explicitly.
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| DELETE | .env | Remove committed plaintext secrets file from source control. Replace with a non-secret template file. |
| CREATE | .env.example | Template with placeholder values for local development (no real secrets). |
| MODIFY | .gitignore | Explicitly ignore `.env` and any other local secret files used for development. |
| MODIFY | Server/ClinicalIntelligence.Api/Program.cs | Enforce secure secret loading/validation and fail-fast behavior without logging secret values. |
| CREATE | Server/ClinicalIntelligence.Api/Configuration/SecretsOptions.cs | Strongly-typed secret/config binding and required-value validation logic. |
| MODIFY | Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj | Add any required configuration/validation package references if needed (prefer built-in options + validation). |
| CREATE | worker/config.py | Centralize worker secret/config loading and required-key validation (non-sensitive errors). |
| MODIFY | worker/main.py | Use centralized config loader; validate required keys on startup. |
| MODIFY | worker/requirements.txt | Add `python-dotenv` for local-only `.env` loading (without committing secrets). |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-8.0
- https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-8.0
- https://learn.microsoft.com/en-us/dotnet/core/extensions/options
- https://github.com/theskumar/python-dotenv

## Build Commands
- dotnet build Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj
- dotnet run --project Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj
- python worker/main.py

## Implementation Validation Strategy
- Verify `.env` is no longer committed and `.env.example` contains no real secret values.
- Backend API:
  - Start the API with required secrets missing and verify it fails fast with a non-sensitive message (no secret value echoed).
  - Start the API with required secrets present and verify normal startup and DB connectivity.
- AI Worker:
  - Start the worker with required keys missing and verify it fails fast with a non-sensitive message.
  - Start the worker with keys present (via env or local `.env`) and verify it runs without printing secret values.
- Rotation:
  - Change env/user-secrets values and confirm a restart picks up updated values without code changes.

## Implementation Checklist
- [x] Add `.env` to `.gitignore` (and/or confirm current patterns cover it)
- [x] Remove committed `.env` and replace with `.env.example` placeholders
- [x] Implement .NET secret/config validation at startup (env + user-secrets in Development)
- [x] Ensure API logs and exception messages do not contain secret values
- [x] Implement Python worker config loader + required-key validation
- [x] Ensure worker logs and exception messages do not contain secret values
- [x] Validate fail-fast behavior for missing/malformed required secrets
- [x] Validate restart-based rotation behavior for updated secret values
