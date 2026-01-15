"""
JSON Schema validator for contract schemas.
Validates JSON schema structure and required fields.
"""
import json
import os
from typing import Dict, List, Tuple


def validate_json_schema_structure(schema_path: str, required_fields: List[str]) -> Tuple[bool, List[str]]:
    """
    Validate JSON schema structure and required fields.
    
    Args:
        schema_path: Path to JSON schema file
        required_fields: List of required field names in the schema
        
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
        errors.append(f"JSONDecodeError: Invalid JSON syntax - {e}")
        return False, errors
    except Exception as e:
        errors.append(f"Error reading JSON schema: {e}")
        return False, errors
    
    # Validate required top-level keys
    required_top_level = ["$schema", "title", "type", "properties", "required"]
    for key in required_top_level:
        if key not in schema:
            errors.append(f"JSON schema missing required key: '{key}'")
    
    # Validate $schema is draft-07
    if '$schema' in schema:
        expected_schema = "http://json-schema.org/draft-07/schema#"
        if schema['$schema'] != expected_schema:
            errors.append(f"Expected $schema to be '{expected_schema}', found: '{schema['$schema']}'")
    
    # Validate required fields are present
    if 'required' in schema:
        schema_required = set(schema['required'])
        for field in required_fields:
            if field not in schema_required:
                errors.append(f"Missing required field: '{field}'")
    
    return len(errors) == 0, errors


def validate_job_schema(schema_path: str) -> Tuple[bool, List[str]]:
    """
    Validate job schema specific requirements.
    
    Args:
        schema_path: Path to job.schema.json file
        
    Returns:
        Tuple of (is_valid, error_messages)
    """
    required_fields = ["schema_version", "job_id", "document_id", "status"]
    return validate_json_schema_structure(schema_path, required_fields)


def validate_entity_schema(schema_path: str) -> Tuple[bool, List[str]]:
    """
    Validate entity schema specific requirements.
    
    Args:
        schema_path: Path to entity.schema.json file
        
    Returns:
        Tuple of (is_valid, error_messages)
    """
    required_fields = ["schema_version", "document_id", "extracted_entities"]
    is_valid, errors = validate_json_schema_structure(schema_path, required_fields)
    
    if not is_valid:
        return is_valid, errors
    
    # Additional validation for entity schema
    try:
        with open(schema_path, 'r', encoding='utf-8') as f:
            schema = json.load(f)
        
        # Check schema_version has enum
        if 'properties' in schema and 'schema_version' in schema['properties']:
            schema_version = schema['properties']['schema_version']
            if schema_version.get('type') != 'string':
                errors.append("schema_version.type must be 'string'")
            if 'enum' not in schema_version or len(schema_version.get('enum', [])) == 0:
                errors.append("schema_version.enum must be a non-empty list")
    except Exception as e:
        errors.append(f"Error validating entity schema: {e}")
    
    return len(errors) == 0, errors


def validate_schema_readme(schema_dir: str) -> Tuple[bool, List[str]]:
    """
    Validate that README.md exists in the schema directory.
    
    Args:
        schema_dir: Path to schema directory (e.g., contracts/jobs/v1)
        
    Returns:
        Tuple of (is_valid, error_messages)
    """
    errors = []
    
    readme_path = os.path.join(schema_dir, "README.md")
    if not os.path.exists(readme_path):
        errors.append("README.md required in contract directory")
        return False, errors
    
    try:
        with open(readme_path, 'r', encoding='utf-8') as f:
            content = f.read()
            if not content.strip():
                errors.append("README.md is empty")
    except Exception as e:
        errors.append(f"Error reading README.md: {e}")
    
    return len(errors) == 0, errors


def validate_all_json_schemas(repo_root: str) -> Tuple[bool, Dict[str, List[str]]]:
    """
    Validate all JSON schemas in the repository.
    
    Args:
        repo_root: Root directory of the repository
        
    Returns:
        Tuple of (all_valid, errors_by_file)
    """
    errors_by_file = {}
    all_valid = True
    
    # Validate job schema
    job_schema_dir = os.path.join(repo_root, "contracts", "jobs", "v1")
    job_schema_path = os.path.join(job_schema_dir, "job.schema.json")
    
    is_valid, errors = validate_job_schema(job_schema_path)
    if not is_valid:
        errors_by_file['job.schema.json'] = errors
        all_valid = False
    
    is_valid, errors = validate_schema_readme(job_schema_dir)
    if not is_valid:
        errors_by_file['jobs/v1/README.md'] = errors
        all_valid = False
    
    # Validate entity schema if exists
    entity_schema_dir = os.path.join(repo_root, "contracts", "entities", "v1")
    entity_schema_path = os.path.join(entity_schema_dir, "entity.schema.json")
    
    if os.path.exists(entity_schema_path):
        is_valid, errors = validate_entity_schema(entity_schema_path)
        if not is_valid:
            errors_by_file['entity.schema.json'] = errors
            all_valid = False
        
        is_valid, errors = validate_schema_readme(entity_schema_dir)
        if not is_valid:
            errors_by_file['entities/v1/README.md'] = errors
            all_valid = False
    
    return all_valid, errors_by_file


def main():
    """Main entry point for JSON schema validation."""
    import sys
    
    repo_root = os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))
    all_valid, errors_by_file = validate_all_json_schemas(repo_root)
    
    if not all_valid:
        print("JSON schema validation FAILED:")
        for filename, errors in errors_by_file.items():
            print(f"\n{filename}:")
            for error in errors:
                print(f"  - {error}")
        sys.exit(1)
    else:
        print("JSON schema validation PASSED")
        sys.exit(0)


if __name__ == "__main__":
    main()
