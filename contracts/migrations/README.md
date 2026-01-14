# Migration Notes

This directory stores migration notes for all contract changes. Each time a contract is modified, a corresponding migration note must be created to document the change, its impact, and any actions required by consumers.

## Naming Convention

Migration notes are stored as one file per contract major version.

`<contract_name>.md`

-   **contract_name**: The contract name and major version (e.g., `api_v1`, `jobs_v1`).

**Examples:** `api_v1.md`, `jobs_v1.md`

Within the file, changes are recorded under semantic version headings (e.g., `## [1.0.0] - Initial Release`).

## Migration Note Structure

Each migration note must contain the following sections:

-   **Contract**: The name of the contract being changed.
-   **Version**: The new version number.
-   **Change Type**: `Backward-Compatible` or `Breaking Change`.
-   **Description**: A brief summary of the changes.
-   **Impact**: An analysis of how the change affects consumers.
-   **Required Actions**: A list of steps consumers must take to adapt to the new version.
