"""
Schema version validator for contract schemas.
Validates that schema versions follow semantic versioning conventions.
"""
import json
import os
import re
from typing import Dict, List, Tuple


def is_valid_semver(version: str) -> bool:
    """
    Validate if a version string follows semantic versioning (X.Y.Z).
    
    Args:
        version: Version string to validate
        
    Returns:
        True if valid semver format, False otherwise
    """
    semver_pattern = r'^\d+\.\d+\.\d+$'
    return bool(re.match(semver_pattern, version))


def validate_openapi_version(openapi_path: str) -> Tuple[bool, List[str]]:
    """
    Validate OpenAPI specification version compliance.
    
    Args:
        openapi_path: Path to openapi.yaml file
        
    Returns:
        Tuple of (is_valid, error_messages)
    """
    import yaml
    
    errors = []
    
    if not os.path.exists(openapi_path):
        errors.append(f"OpenAPI file not found: {openapi_path}")
        return False, errors
    
    try:
        with open(openapi_path, 'r', encoding='utf-8') as f:
            spec = yaml.safe_load(f)
    except yaml.YAMLError as e:
        errors.append(f"Invalid YAML in OpenAPI spec: {e}")
        return False, errors
    
    # Check info.version exists
    if 'info' not in spec:
        errors.append("OpenAPI spec missing 'info' section")
        return False, errors
    
    if 'version' not in spec['info']:
        errors.append("OpenAPI spec missing 'info.version' field")
        return False, errors
    
    version = spec['info']['version']
    
    # Validate semver format
    if not is_valid_semver(version):
        errors.append(f"Invalid semver format in OpenAPI version: '{version}'. Expected format: X.Y.Z")
        return False, errors
    
    return True, []


def validate_json_schema_version(schema_path: str, expected_version_enum: List[str] = None) -> Tuple[bool, List[str]]:
    """
    Validate JSON schema version compliance.
    
    Args:
        schema_path: Path to JSON schema file
        expected_version_enum: Optional list of expected version values in enum
        
    Returns:
        Tuple of (is_valid, error_messages)
    """
    errors = []
    
    if not os.path.exists(schema_path):
        errors.append(f"JSON schema file not found: {schema_path}")
        return False, errors
    
    try:
        with open(schema_path, 'r', encoding='utf-8') as f:
            schema = json.load(f)
    except json.JSONDecodeError as e:
        errors.append(f"Invalid JSON in schema: {e}")
        return False, errors
    
    # Check schema_version property exists
    if 'properties' not in schema:
        errors.append("JSON schema missing 'properties' section")
        return False, errors
    
    if 'schema_version' not in schema['properties']:
        errors.append("JSON schema missing 'schema_version' property")
        return False, errors
    
    schema_version_prop = schema['properties']['schema_version']
    
    # Check enum exists
    if 'enum' not in schema_version_prop:
        errors.append("schema_version property missing 'enum' field")
        return False, errors
    
    enum_values = schema_version_prop['enum']
    
    if not enum_values or len(enum_values) == 0:
        errors.append("schema_version enum must contain at least one version")
        return False, errors
    
    # Validate expected version if provided
    if expected_version_enum:
        for expected_ver in expected_version_enum:
            if expected_ver not in enum_values:
                errors.append(f"Expected version '{expected_ver}' not found in schema_version enum")
    
    return len(errors) == 0, errors


def validate_all_schema_versions(repo_root: str) -> Tuple[bool, Dict[str, List[str]]]:
    """
    Validate all contract schema versions in the repository.
    
    Args:
        repo_root: Root directory of the repository
        
    Returns:
        Tuple of (all_valid, errors_by_file)
    """
    errors_by_file = {}
    all_valid = True
    
    # Validate OpenAPI version
    openapi_path = os.path.join(repo_root, "contracts", "api", "v1", "openapi.yaml")
    is_valid, errors = validate_openapi_version(openapi_path)
    if not is_valid:
        errors_by_file['openapi.yaml'] = errors
        all_valid = False
    
    # Validate job schema version
    job_schema_path = os.path.join(repo_root, "contracts", "jobs", "v1", "job.schema.json")
    is_valid, errors = validate_json_schema_version(job_schema_path, expected_version_enum=["1.0"])
    if not is_valid:
        errors_by_file['job.schema.json'] = errors
        all_valid = False
    
    # Validate entity schema version if exists
    entity_schema_path = os.path.join(repo_root, "contracts", "entities", "v1", "entity.schema.json")
    if os.path.exists(entity_schema_path):
        is_valid, errors = validate_json_schema_version(entity_schema_path, expected_version_enum=["1.0"])
        if not is_valid:
            errors_by_file['entity.schema.json'] = errors
            all_valid = False
    
    return all_valid, errors_by_file


def main():
    """Main entry point for schema version validation."""
    import sys
    
    repo_root = os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))
    all_valid, errors_by_file = validate_all_schema_versions(repo_root)
    
    if not all_valid:
        print("Schema version validation FAILED:")
        for filename, errors in errors_by_file.items():
            print(f"\n{filename}:")
            for error in errors:
                print(f"  - {error}")
        sys.exit(1)
    else:
        print("Schema version validation PASSED")
        sys.exit(0)


if __name__ == "__main__":
    main()
