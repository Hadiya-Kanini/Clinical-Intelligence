# Job Message Contract v1

This document defines version 1 of the job message schema for asynchronous processing between the Backend API and the AI Worker.

## Schema Location

The canonical JSON schema is located at `contracts/jobs/v1/job.schema.json`.

## Fields

| Field | Type | Required | Description |
|---|---|---|---|
| `schema_version` | string | Yes | The version of the job schema. For v1, this must be `"1.0"`. |
| `job_id` | string | Yes | A unique identifier (UUID) for the job. |
| `document_id` | string | Yes | The identifier for the document to be processed. |
| `status` | string | Yes | The current status of the job. |
| `payload` | object | No | An arbitrary JSON object containing data needed for the job. |

## Status Transitions

The `status` field follows a defined lifecycle. The allowed transitions are:

- `pending` -> `processing`
- `processing` -> `completed`
- `processing` -> `failed`
- `processing` -> `validation_failed`

## Error Handling

### Consumer (AI Worker)

- **Missing Required Fields**: If a received message is missing one or more of `schema_version`, `job_id`, `document_id`, or `status`, the message is considered invalid. It will be rejected and sent to a Dead Letter Exchange (DLX) for inspection.
- **Unknown Schema Version**: If the `schema_version` is not `"1.0"`, the worker will reject the message and send it to a DLX.

### Producer (Backend API)

- The Backend API is responsible for ensuring all enqueued job messages conform to this schema and contain a valid `schema_version`.
