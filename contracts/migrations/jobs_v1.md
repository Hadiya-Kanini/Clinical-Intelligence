# Job Contract Migration Notes - v1

## Version 1.0

**Release Date:** 2026-01-13

### Summary

This is the initial version of the job message contract. It establishes the foundational schema for asynchronous communication between the Backend API and the AI Worker.

### Changes

- **Initial Schema**: Introduced the first versioned job schema (`1.0`).
- **Required Fields**: Defined `schema_version`, `job_id`, `document_id`, and `status` as mandatory fields.
- **Status Lifecycle**: Established the initial set of status values: `pending`, `processing`, `completed`, `failed`, `validation_failed`.

### Migration Guidance

As this is the first version, no migration is necessary. All new job messages produced by the Backend API must conform to this v1 schema.

### Future Changes

Any future modifications to the job schema must follow these guidelines:

1.  **Backward-Compatible Change**: If a change does not break existing consumers (e.g., adding a new optional field), the minor version of the schema should be incremented (e.g., `1.1`).
2.  **Breaking Change**: If a change will break existing consumers (e.g., removing a required field, changing a data type), the major version must be incremented (e.g., `2.0`).
3.  **Documentation**: All changes must be documented in a new migration file (e.g., `jobs_v2.md`).
