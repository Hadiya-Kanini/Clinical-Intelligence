import json
import os
import sys
import yaml


def repo_root() -> str:
    return os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))


def read_json(path: str) -> dict:
    with open(path, "r", encoding="utf-8") as f:
        return json.load(f)


def validate_job_schema() -> None:
    schema_path = os.path.join(repo_root(), "contracts", "jobs", "v1", "job.schema.json")
    schema = read_json(schema_path)

    required_top_level_keys = ["$schema", "title", "type", "properties", "required"]
    for key in required_top_level_keys:
        if key not in schema:
            raise RuntimeError(f"job.schema.json missing required key: {key}")

    required_fields = set(schema.get("required", []))
    for field in ["schema_version", "job_id", "document_id", "status"]:
        if field not in required_fields:
            raise RuntimeError(f"job.schema.json must require field: {field}")


def validate_entity_schema() -> None:
    schema_path = os.path.join(repo_root(), "contracts", "entities", "v1", "entity.schema.json")
    schema = read_json(schema_path)

    required_top_level_keys = ["$schema", "title", "type", "properties", "required"]
    for key in required_top_level_keys:
        if key not in schema:
            raise RuntimeError(f"entity.schema.json missing required key: {key}")

    required_fields = set(schema.get("required", []))
    for field in ["schema_version", "document_id", "extracted_entities"]:
        if field not in required_fields:
            raise RuntimeError(f"entity.schema.json must require field: {field}")

    schema_version = schema.get("properties", {}).get("schema_version", {})
    if schema_version.get("type") != "string":
        raise RuntimeError("entity.schema.json schema_version.type must be 'string'")
    if "enum" not in schema_version or len(schema_version.get("enum", [])) == 0:
        raise RuntimeError("entity.schema.json schema_version.enum must be a non-empty list")


def validate_openapi_contract() -> None:
    openapi_path = os.path.join(repo_root(), "contracts", "api", "v1", "openapi.yaml")
    with open(openapi_path, "r", encoding="utf-8") as f:
        doc = yaml.safe_load(f)

    if not doc.get("openapi"):
        raise RuntimeError("openapi.yaml missing 'openapi:' header")

    paths = doc.get("paths", {})
    if "/health" not in paths:
        raise RuntimeError("openapi.yaml missing /health path")

    # Enforce that all paths are versioned (e.g., start with /api/v1 or similar)
    versioned_prefix = "/api/v1"
    for path in paths:
        if not path.startswith("/health") and not path.startswith(versioned_prefix):
            raise RuntimeError(f"Path '{path}' does not start with versioned prefix '{versioned_prefix}'")


def validate_migration_notes() -> None:
    migration_paths = [
        os.path.join(repo_root(), "contracts", "migrations", "api_v1.md"),
        os.path.join(repo_root(), "contracts", "migrations", "jobs_v1.md"),
    ]

    for path in migration_paths:
        if not os.path.isfile(path):
            raise RuntimeError(f"Missing required migration-note file: {os.path.relpath(path, repo_root())}")

        with open(path, "r", encoding="utf-8") as f:
            contents = f.read()

        if len(contents.strip()) == 0:
            raise RuntimeError(
                f"Migration-note file must be non-empty: {os.path.relpath(path, repo_root())}"
            )


def main() -> int:
    validate_migration_notes()
    validate_job_schema()
    validate_entity_schema()
    validate_openapi_contract()
    print("Contract validation passed.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
