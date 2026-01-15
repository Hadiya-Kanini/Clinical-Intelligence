"""
Contract structure validator.
Validates contract directory structure and file presence.
This is the main entry point for basic contract validation.
"""
import json
import os
import sys
import yaml


def repo_root() -> str:
    """Get repository root directory."""
    return os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))


def read_json(path: str) -> dict:
    """Read and parse JSON file."""
    with open(path, "r", encoding="utf-8") as f:
        return json.load(f)


def validate_directory_structure() -> None:
    """Validate that required contract directories exist."""
    root = repo_root()
    required_dirs = [
        os.path.join(root, "contracts"),
        os.path.join(root, "contracts", "api"),
        os.path.join(root, "contracts", "jobs"),
        os.path.join(root, "contracts", "migrations"),
    ]
    
    for dir_path in required_dirs:
        if not os.path.isdir(dir_path):
            raise RuntimeError(f"Required directory not found: {os.path.relpath(dir_path, root)}")


def validate_api_contract_files() -> None:
    """Validate that API contract files exist."""
    root = repo_root()
    api_v1_dir = os.path.join(root, "contracts", "api", "v1")
    
    required_files = [
        os.path.join(api_v1_dir, "openapi.yaml"),
        os.path.join(api_v1_dir, "README.md"),
    ]
    
    for file_path in required_files:
        if not os.path.isfile(file_path):
            raise RuntimeError(f"Required API contract file not found: {os.path.relpath(file_path, root)}")


def validate_job_contract_files() -> None:
    """Validate that job contract files exist."""
    root = repo_root()
    jobs_v1_dir = os.path.join(root, "contracts", "jobs", "v1")
    
    required_files = [
        os.path.join(jobs_v1_dir, "job.schema.json"),
        os.path.join(jobs_v1_dir, "README.md"),
    ]
    
    for file_path in required_files:
        if not os.path.isfile(file_path):
            raise RuntimeError(f"Required job contract file not found: {os.path.relpath(file_path, root)}")


def validate_job_schema() -> None:
    """Validate job schema structure and required fields."""
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
    """Validate entity schema structure and required fields."""
    schema_path = os.path.join(repo_root(), "contracts", "entities", "v1", "entity.schema.json")
    
    # Entity schema is optional, skip if doesn't exist
    if not os.path.exists(schema_path):
        return
    
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
    """Validate OpenAPI specification structure."""
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
    """Validate that migration notes exist and are non-empty."""
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
    """Main entry point for contract validation."""
    try:
        print("Validating contract structure...")
        validate_directory_structure()
        validate_api_contract_files()
        validate_job_contract_files()
        validate_migration_notes()
        validate_job_schema()
        validate_entity_schema()
        validate_openapi_contract()
        print("[PASS] Contract validation passed.")
        return 0
    except RuntimeError as e:
        print(f"[FAIL] Contract validation failed: {e}", file=sys.stderr)
        return 1
    except Exception as e:
        print(f"[ERROR] Unexpected error during validation: {e}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    sys.exit(main())
