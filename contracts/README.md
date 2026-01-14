# Integration Contracts

This directory contains the canonical integration contracts for the Clinical Intelligence solution. These contracts are the source of truth for how components communicate with each other.

## Contract Boundaries and Ownership

- **Backend API**: `contracts/api/`
  - **Owner**: Backend Team
  - **Consumer**: Web UI (`app/`)
  - **Description**: Defines the RESTful API for all frontend-to-backend communication using the OpenAPI specification.

- **AI Worker Jobs**: `contracts/jobs/`
  - **Owner**: Backend Team
  - **Consumer**: AI Worker (`worker/`)
  - **Description**: Defines the message schema for jobs produced by the Backend API and consumed by the AI Worker.

## Contract Change Process

All changes to contracts, regardless of size, must follow this process to ensure consistency, traceability, and clear communication to all consuming components.

## Feature Change Checklist (Cross-Service)

Use this checklist any time a feature touches more than one service (Web UI, Backend API, AI Worker).

- Put new behavior in the correct service (do not bypass contracts).
- If the Web UI and Backend API integration changes, update `contracts/api/`.
- If the Backend API and AI Worker job payload changes, update `contracts/jobs/`.
- For any contract change, update the corresponding migration note in `contracts/migrations/`.

**1. Identify the Change:**
   - Locate the contract file to be modified under `contracts/api/` or `contracts/jobs/`.

**2. Classify the Change and Determine New Version:**
   - Determine if the change is a `Breaking Change` or `Backward-Compatible`.
   - Based on the classification, determine the new semantic version number for the contract.

**3. Create a Migration Note:**
   - A migration note is required for **every** change.
   - Create a new file in the `contracts/migrations/` directory.
   - Follow the naming convention and structure defined in `contracts/migrations/README.md`.

**4. Update the Contract:**
   - Apply the required modifications to the contract file (e.g., the OpenAPI spec or job schema).
   - Ensure the version number within the contract artifact is updated to the new version.

## Versioning and Change Classification

All contracts follow [Semantic Versioning 2.0.0](https://semver.org/). Each contract is versioned independently.

- **MAJOR** version (e.g., `v1.x` to `v2.0`): For backward-incompatible (breaking) changes.
- **MINOR** version (e.g., `v1.1` to `v1.2`): For adding functionality in a backward-compatible manner.
- **PATCH** version (e.g., `v1.1.1` to `v1.1.2`): For backward-compatible bug fixes.

### Change Types

- **Breaking Change**: Any change that requires a consumer to update its code. Examples include:
  - Removing an API endpoint or a field from a schema.
  - Renaming a field.
  - Changing a field's data type.
  - Adding a new required field.
  - Changing an existing validation rule (e.g., making an optional field required).

- **Backward-Compatible Change**: A change that will not break existing consumers. Examples include:
  - Adding a new optional field.
  - Adding a new API endpoint.
  - Adding a new, optional query parameter.

## Migration Notes

All contract changes must be documented in a migration note to provide a clear audit trail and impact assessment for consumers.

For detailed instructions on migration note structure and naming conventions, see the `contracts/migrations/README.md` file.

## No Out-of-Contract Rule

Direct component-to-component integrations that bypass these contracts are strictly prohibited. All communication must adhere to the schemas and endpoints defined in this repository.

- **Source of Truth**: The contract artifacts (OpenAPI specs, JSON schemas) are the single source of truth. Code generation or validation against these artifacts is enforced where possible.
- **Enforcement**: Automated checks will fail any pull request that attempts to introduce communication patterns not defined by a contract.
