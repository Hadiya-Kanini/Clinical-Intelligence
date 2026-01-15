"""
OpenAPI specification validator.
Validates OpenAPI spec structure, versioning, and compliance.
"""
import os
from typing import Dict, List, Tuple


def validate_openapi_structure(openapi_path: str) -> Tuple[bool, List[str]]:
    """
    Validate OpenAPI specification structure and required fields.
    
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
        errors.append(f"YAMLError: Invalid YAML syntax at {e}")
        return False, errors
    except Exception as e:
        errors.append(f"Error reading OpenAPI file: {e}")
        return False, errors
    
    # Validate OpenAPI version
    if 'openapi' not in spec:
        errors.append("OpenAPI spec missing 'openapi' field")
        return False, errors
    
    openapi_version = spec['openapi']
    if not openapi_version.startswith('3.'):
        errors.append(f"OpenAPI 3.0+ required, found: {openapi_version}")
    
    # Validate info section
    if 'info' not in spec:
        errors.append("OpenAPI spec missing 'info' section")
    else:
        info = spec['info']
        if 'title' not in info:
            errors.append("OpenAPI spec missing 'info.title'")
        if 'version' not in info:
            errors.append("OpenAPI spec missing 'info.version'")
        else:
            # Check version format
            version = info['version']
            if version != "1.0.0":
                errors.append(f"Expected info.version to be '1.0.0', found: '{version}'")
    
    # Validate paths section
    if 'paths' not in spec:
        errors.append("OpenAPI spec missing 'paths' section")
    else:
        paths = spec['paths']
        if not paths:
            errors.append("OpenAPI spec has empty 'paths' section")
    
    return len(errors) == 0, errors


def validate_openapi_versioning(openapi_path: str, expected_prefix: str = "/api/v1") -> Tuple[bool, List[str]]:
    """
    Validate that all API paths follow versioning conventions.
    
    Args:
        openapi_path: Path to openapi.yaml file
        expected_prefix: Expected version prefix for API paths
        
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
    except Exception as e:
        errors.append(f"Error reading OpenAPI file: {e}")
        return False, errors
    
    paths = spec.get('paths', {})
    
    for path in paths:
        # Skip health check endpoints
        if path.startswith('/health'):
            continue
        
        # Check versioning prefix
        if not path.startswith(expected_prefix):
            errors.append(f"Path '{path}' does not start with versioned prefix '{expected_prefix}'")
    
    return len(errors) == 0, errors


def validate_openapi_readme(api_dir: str) -> Tuple[bool, List[str]]:
    """
    Validate that README.md exists in the API contract directory.
    
    Args:
        api_dir: Path to API contract directory (e.g., contracts/api/v1)
        
    Returns:
        Tuple of (is_valid, error_messages)
    """
    errors = []
    
    readme_path = os.path.join(api_dir, "README.md")
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


def validate_all_openapi(repo_root: str) -> Tuple[bool, Dict[str, List[str]]]:
    """
    Validate all OpenAPI specifications in the repository.
    
    Args:
        repo_root: Root directory of the repository
        
    Returns:
        Tuple of (all_valid, errors_by_check)
    """
    errors_by_check = {}
    all_valid = True
    
    api_v1_dir = os.path.join(repo_root, "contracts", "api", "v1")
    openapi_path = os.path.join(api_v1_dir, "openapi.yaml")
    
    # Validate structure
    is_valid, errors = validate_openapi_structure(openapi_path)
    if not is_valid:
        errors_by_check['structure'] = errors
        all_valid = False
    
    # Validate versioning
    is_valid, errors = validate_openapi_versioning(openapi_path)
    if not is_valid:
        errors_by_check['versioning'] = errors
        all_valid = False
    
    # Validate README
    is_valid, errors = validate_openapi_readme(api_v1_dir)
    if not is_valid:
        errors_by_check['readme'] = errors
        all_valid = False
    
    return all_valid, errors_by_check


def main():
    """Main entry point for OpenAPI validation."""
    import sys
    
    repo_root = os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))
    all_valid, errors_by_check = validate_all_openapi(repo_root)
    
    if not all_valid:
        print("OpenAPI validation FAILED:")
        for check_name, errors in errors_by_check.items():
            print(f"\n{check_name}:")
            for error in errors:
                print(f"  - {error}")
        sys.exit(1)
    else:
        print("OpenAPI validation PASSED")
        sys.exit(0)


if __name__ == "__main__":
    main()
