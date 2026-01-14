import json
import os
try:
    from jsonschema import Draft7Validator
except ModuleNotFoundError as e:
    raise ModuleNotFoundError(
        "Missing dependency 'jsonschema'. Install worker requirements with: pip install -r worker/requirements.txt"
    ) from e


def _repo_root() -> str:
    return os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))


def _load_job_schema() -> dict:
    schema_path = os.path.join(_repo_root(), "contracts", "jobs", "v1", "job.schema.json")
    try:
        with open(schema_path, "r", encoding="utf-8") as f:
            return json.load(f)
    except FileNotFoundError:
        raise FileNotFoundError(f"Job schema file not found at {schema_path}")
    except json.JSONDecodeError as e:
        raise ValueError(f"Invalid JSON in job schema file: {e}")
    except Exception as e:
        raise RuntimeError(f"Unexpected error loading job schema: {e}")


def _load_entity_schema(schema_version: str) -> dict:
    if schema_version == "1.0":
        schema_path = os.path.join(
            _repo_root(), "contracts", "entities", "v1", "entity.schema.json"
        )
    else:
        raise ValueError(f"Unknown entity schema version: {schema_version}")

    try:
        with open(schema_path, "r", encoding="utf-8") as f:
            return json.load(f)
    except FileNotFoundError:
        raise FileNotFoundError(f"Entity schema file not found at {schema_path}")
    except json.JSONDecodeError as e:
        raise ValueError(f"Invalid JSON in entity schema file: {e}")
    except Exception as e:
        raise RuntimeError(f"Unexpected error loading entity schema: {e}")


def validate_job_payload(payload: dict) -> None:
    schema = _load_job_schema()
    validator = Draft7Validator(schema)

    errors = sorted(validator.iter_errors(payload), key=lambda e: e.path)
    if errors:
        messages = [f"{list(e.path)}: {e.message}" for e in errors]
        raise ValueError("Invalid job payload: " + "; ".join(messages))


def validate_entity_payload(payload: dict) -> None:
    schema_version = payload.get("schema_version")
    if not schema_version:
        raise ValueError("Invalid entity payload: missing required field 'schema_version'")

    schema = _load_entity_schema(schema_version)
    validator = Draft7Validator(schema)

    errors = sorted(validator.iter_errors(payload), key=lambda e: e.path)
    if errors:
        messages = [f"{list(e.path)}: {e.message}" for e in errors]
        raise ValueError("Invalid entity payload: " + "; ".join(messages))


if __name__ == "__main__":
    from config import load_config

    load_config()

    example = {
        "schema_version": "1.0",
        "job_id": "00000000-0000-0000-0000-000000000000",
        "document_id": "doc-123",
        "status": "pending",
        "payload": {}
    }

    entity_example = {
        "schema_version": "1.0",
        "document_id": "doc-123",
        "extracted_entities": [
            {
                "entity_group_name": "patient_demographics",
                "entity_name": "name",
                "entity_value": "Jane Doe"
            }
        ]
    }

    validate_job_payload(example)
    validate_entity_payload(entity_example)
    print("Worker scaffold is running; example job payload validated successfully.")
