# Task - TASK_001

## Requirement Reference
- User Story: us_117
- Story Location: .propel/context/tasks/us_117/us_117.md
- Acceptance Criteria: 
    - AC-1: Given the deployment environment, When PostgreSQL is installed, Then version 15 or higher is running and accessible on the configured port

## Task Overview
Establish a repeatable PostgreSQL 15+ installation and base configuration workflow (service setup, port access, database/user creation, and pre-flight validation) for local development and the target deployment environment, so downstream features can rely on a consistent Postgres foundation.
Estimated Effort: 6 hours

## Dependent Tasks
- N/A

## Impacted Components
- scripts/
- Server/README.md
- .env.example

## Implementation Plan
- Define a supported installation approach for the repository (local installation on Windows + environment-variable driven configuration).
- Implement a pre-flight validation script that:
  - Detects PostgreSQL version and fails if < 15
  - Verifies service is running and listening on the expected port
  - Validates credentials by opening a basic connection and running `SELECT 1`
  - Performs a disk space check (minimum 10GB)
- Implement an installation/bootstrap script that:
  - Creates the application database and a least-privilege application user
  - Outputs non-sensitive troubleshooting guidance when startup fails
- Document the installation workflow and expected configuration variables.
**Focus on how to implement**

## Current Project State
- [Project structure in tree format. Refer to 'app' folder for the Frontend and 'Server' for the backend code. Shall remain as placeholder to be updated during the execution of the task based on the completion of the dependent task.]

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | scripts/db/install_postgresql.ps1 | Installs/configures PostgreSQL 15+ (or validates existing install) and provisions initial database/user for the app |
| CREATE | scripts/db/validate_postgresql_prereqs.ps1 | Pre-flight checks: version >= 15, service running, port reachable, disk space >= 10GB |
| MODIFY | Server/README.md | Add documented PostgreSQL installation + configuration steps and expected environment variables |
| MODIFY | .env.example | Ensure PostgreSQL connection string example aligns with the documented workflow |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- https://www.postgresql.org/docs/
- https://www.postgresql.org/docs/current/runtime-config.html

## Build Commands
- dotnet build Server/ClinicalIntelligence.Api/ClinicalIntelligence.Api.csproj

## Implementation Validation Strategy
- Verify `validate_postgresql_prereqs.ps1` fails fast with clear messaging when PostgreSQL version is below 15.
- Verify PostgreSQL is reachable using the configured host/port/credentials and returns `SELECT 1` successfully.
- Verify the application database and application user are created as expected and the user can connect.

## Implementation Checklist
- [x] Define the supported PostgreSQL installation/configuration approach for local + deployment environments
- [x] Implement pre-flight validation (version, service running, port, disk space)
- [x] Implement database/user bootstrap script with non-sensitive error output
- [x] Update `Server/README.md` with repeatable setup instructions
- [x] Update `.env.example` connection string example to match
- [x] Manually validate on a clean machine profile (or fresh Postgres instance)
