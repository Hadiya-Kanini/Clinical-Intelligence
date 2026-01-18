# Job Message Contract v1

This document defines version 1 of the job message schema for asynchronous processing between the Backend API and the AI Worker.

## Schema Location

The canonical JSON schema is located at `contracts/jobs/v1/job.schema.json`.

## Fields

| Field | Type | Required | Description |
|---|---|---|---|
| `schema_version` | string | Yes | The version of the job schema. Must be `"1.0"` or `"1.1"`. |
| `job_id` | string | Yes | A unique identifier (UUID) for the job. |
| `document_id` | string | Yes | The identifier for the document to be processed. |
| `status` | string | Yes | The current status of the job. |
| `payload` | object | No | An arbitrary JSON object containing data needed for the job. |

## Payload Fields (v1.1)

The following optional payload fields were added in v1.1:

### Text Extraction Fields

| Field | Type | Description |
|---|---|---|
| `payload.storage_path` | string | Path to the document in storage for extraction. |
| `payload.mime_type` | string | MIME type of the document (e.g., `application/pdf`). |

### Patient Merge Fields

| Field | Type | Description |
|---|---|---|
| `payload.patient_id` | string (UUID) | Patient identifier for multi-document merge operations. |
| `payload.document_ids` | array of strings | Document IDs to merge for the patient. |
| `payload.patient_identifiers` | object | Alternative patient matching inputs (mrn, name, dob). |

### Patient Identifiers Object

| Field | Type | Description |
|---|---|---|
| `mrn` | string | Medical Record Number for patient matching. |
| `name` | string | Patient name for matching (used with dob). |
| `dob` | string (date) | Patient date of birth (ISO 8601 format). |

## Worker Behavior

- When `payload.storage_path` and `payload.mime_type` are present, the worker performs text extraction.
- When `payload.document_ids` is present, the worker performs multi-document merge for the patient.
- `payload.patient_id` takes precedence over `payload.patient_identifiers` for patient identification.

## Status Transitions

The `status` field follows a defined lifecycle. The allowed transitions are:

- `pending` -> `processing`
- `processing` -> `completed`
- `processing` -> `failed`
- `processing` -> `validation_failed`

## Error Handling

### Consumer (AI Worker)

- **Missing Required Fields**: If a received message is missing one or more of `schema_version`, `job_id`, `document_id`, or `status`, the message is considered invalid. It will be rejected and sent to a Dead Letter Exchange (DLX) for inspection.
- **Unknown Schema Version**: If the `schema_version` is not `"1.0"` or `"1.1"`, the worker will reject the message and send it to a DLX.

### Producer (Backend API)

- The Backend API is responsible for ensuring all enqueued job messages conform to this schema and contain a valid `schema_version`.
