"""Mock schema fixtures for testing."""

import json
import os


def _repo_root() -> str:
    """Get repository root directory."""
    return os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))


def load_chunking_schema() -> dict:
    """Load the chunking contract schema from contracts/chunking/v1/chunked_text.schema.json."""
    schema_path = os.path.join(
        _repo_root(), "contracts", "chunking", "v1", "chunked_text.schema.json"
    )
    with open(schema_path, "r", encoding="utf-8") as f:
        return json.load(f)


def load_embeddings_schema() -> dict:
    """Load the embeddings contract schema from contracts/embeddings/v1/embedding_result.schema.json."""
    schema_path = os.path.join(
        _repo_root(), "contracts", "embeddings", "v1", "embedding_result.schema.json"
    )
    with open(schema_path, "r", encoding="utf-8") as f:
        return json.load(f)


def load_merged_text_schema() -> dict:
    """Load the merged text contract schema from contracts/text_merge/v1/merged_text.schema.json."""
    schema_path = os.path.join(
        _repo_root(), "contracts", "text_merge", "v1", "merged_text.schema.json"
    )
    with open(schema_path, "r", encoding="utf-8") as f:
        return json.load(f)


MOCK_JOB_SCHEMA = {
    "$schema": "http://json-schema.org/draft-07/schema#",
    "title": "Job",
    "description": "Schema for a job to be processed by the AI worker.",
    "type": "object",
    "properties": {
        "schema_version": {
            "description": "The version of this job schema.",
            "type": "string",
            "enum": ["1.0"]
        },
        "job_id": {
            "description": "Unique identifier for the job (UUID).",
            "type": "string",
            "pattern": "^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$"
        },
        "document_id": {
            "description": "Identifier for the document to be processed.",
            "type": "string"
        },
        "status": {
            "description": "The current status of the job.",
            "type": "string",
            "enum": ["pending", "processing", "completed", "failed", "validation_failed"]
        },
        "payload": {
            "description": "Arbitrary data for the worker to use.",
            "type": "object"
        }
    },
    "required": [
        "schema_version",
        "job_id",
        "document_id",
        "status"
    ]
}

MOCK_ENTITY_SCHEMA = {
    "$schema": "http://json-schema.org/draft-07/schema#",
    "title": "EntityExtractionResult",
    "description": "Schema for entity extraction output produced/validated by the AI worker.",
    "type": "object",
    "properties": {
        "schema_version": {
            "description": "The version of this entity output schema.",
            "type": "string",
            "enum": ["1.0"]
        },
        "document_id": {
            "description": "Identifier for the document processed.",
            "type": "string"
        },
        "extracted_entities": {
            "description": "Extracted entities from the source document.",
            "type": "array",
            "items": {"$ref": "#/definitions/ExtractedEntity"}
        }
    },
    "required": [
        "schema_version",
        "document_id",
        "extracted_entities"
    ],
    "definitions": {
        "ExtractedEntity": {
            "type": "object",
            "properties": {
                "entity_group_name": {"type": "string"},
                "entity_name": {"type": "string"},
                "entity_value": {"type": "string"}
            },
            "required": ["entity_group_name", "entity_name", "entity_value"]
        }
    }
}

INVALID_JSON_SCHEMA = "{ invalid json content"
