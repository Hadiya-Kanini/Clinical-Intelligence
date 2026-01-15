"""
Orchestrator script to run all contract validation checks.
Provides a single entry point for comprehensive contract validation.
"""
import os
import sys
from typing import Dict, List, Tuple

# Import all validators
from validate_schema_versions import validate_all_schema_versions
from validate_migrations import validate_all_migrations
from validate_openapi import validate_all_openapi
from validate_json_schemas import validate_all_json_schemas


class ValidationResult:
    """Container for validation results."""
    
    def __init__(self, name: str, passed: bool, errors: Dict[str, List[str]]):
        self.name = name
        self.passed = passed
        self.errors = errors
    
    def __repr__(self):
        status = "PASSED" if self.passed else "FAILED"
        return f"ValidationResult(name='{self.name}', status={status})"


def run_all_validations(repo_root: str) -> Tuple[bool, List[ValidationResult]]:
    """
    Run all contract validation checks.
    
    Args:
        repo_root: Root directory of the repository
        
    Returns:
        Tuple of (all_passed, validation_results)
    """
    results = []
    all_passed = True
    
    print("=" * 80)
    print("CONTRACT VALIDATION SUITE")
    print("=" * 80)
    print()
    
    # 1. Validate schema versions
    print("1. Validating schema versions...")
    is_valid, errors = validate_all_schema_versions(repo_root)
    results.append(ValidationResult("Schema Versions", is_valid, errors))
    if is_valid:
        print("   [PASS] Schema versions validation PASSED")
    else:
        print("   [FAIL] Schema versions validation FAILED")
        all_passed = False
    print()
    
    # 2. Validate migration notes
    print("2. Validating migration notes...")
    is_valid, errors = validate_all_migrations(repo_root)
    results.append(ValidationResult("Migration Notes", is_valid, errors))
    if is_valid:
        print("   [PASS] Migration notes validation PASSED")
    else:
        print("   [FAIL] Migration notes validation FAILED")
        all_passed = False
    print()
    
    # 3. Validate OpenAPI specifications
    print("3. Validating OpenAPI specifications...")
    is_valid, errors = validate_all_openapi(repo_root)
    results.append(ValidationResult("OpenAPI Specifications", is_valid, errors))
    if is_valid:
        print("   [PASS] OpenAPI validation PASSED")
    else:
        print("   [FAIL] OpenAPI validation FAILED")
        all_passed = False
    print()
    
    # 4. Validate JSON schemas
    print("4. Validating JSON schemas...")
    is_valid, errors = validate_all_json_schemas(repo_root)
    results.append(ValidationResult("JSON Schemas", is_valid, errors))
    if is_valid:
        print("   [PASS] JSON schemas validation PASSED")
    else:
        print("   [FAIL] JSON schemas validation FAILED")
        all_passed = False
    print()
    
    return all_passed, results


def print_summary(all_passed: bool, results: List[ValidationResult]):
    """
    Print validation summary.
    
    Args:
        all_passed: Whether all validations passed
        results: List of validation results
    """
    print("=" * 80)
    print("VALIDATION SUMMARY")
    print("=" * 80)
    print()
    
    passed_count = sum(1 for r in results if r.passed)
    failed_count = len(results) - passed_count
    
    print(f"Total checks: {len(results)}")
    print(f"Passed: {passed_count}")
    print(f"Failed: {failed_count}")
    print()
    
    if not all_passed:
        print("FAILED VALIDATIONS:")
        print("-" * 80)
        for result in results:
            if not result.passed:
                print(f"\n{result.name}:")
                for filename, errors in result.errors.items():
                    print(f"  {filename}:")
                    for error in errors:
                        print(f"    - {error}")
        print()
    
    print("=" * 80)
    if all_passed:
        print("[PASS] ALL CONTRACT VALIDATIONS PASSED")
    else:
        print("[FAIL] CONTRACT VALIDATION FAILED")
    print("=" * 80)


def main():
    """Main entry point for all contract validations."""
    repo_root = os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))
    
    all_passed, results = run_all_validations(repo_root)
    print_summary(all_passed, results)
    
    sys.exit(0 if all_passed else 1)


if __name__ == "__main__":
    main()
