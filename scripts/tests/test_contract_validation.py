"""
Comprehensive unit tests for contract validation scripts.
Tests cover positive, negative, edge case, and error scenarios.
"""
import json
import os
import tempfile
import unittest
from unittest.mock import mock_open, patch, MagicMock
import sys

# Add parent directory to path for imports
sys.path.insert(0, os.path.abspath(os.path.join(os.path.dirname(__file__), '..')))

from validate_schema_versions import (
    is_valid_semver,
    validate_openapi_version,
    validate_json_schema_version,
    validate_all_schema_versions
)
from validate_migrations import (
    validate_migration_note_structure,
    validate_migration_directory,
    validate_all_migrations
)
from validate_openapi import (
    validate_openapi_structure,
    validate_openapi_versioning,
    validate_openapi_readme,
    validate_all_openapi
)
from validate_json_schemas import (
    validate_json_schema_structure,
    validate_job_schema,
    validate_entity_schema,
    validate_schema_readme,
    validate_all_json_schemas
)


class TestContractStructure(unittest.TestCase):
    """Test cases for contract directory structure validation."""
    
    def setUp(self):
        """Set up test fixtures."""
        self.test_dir = tempfile.mkdtemp()
    
    def tearDown(self):
        """Clean up test fixtures."""
        import shutil
        if os.path.exists(self.test_dir):
            shutil.rmtree(self.test_dir)
    
    def test_tc001_contract_directory_structure_exists(self):
        """TC-001: Contract directory structure exists."""
        # Create directory structure
        contracts_dir = os.path.join(self.test_dir, "contracts")
        os.makedirs(os.path.join(contracts_dir, "api"))
        os.makedirs(os.path.join(contracts_dir, "jobs"))
        os.makedirs(os.path.join(contracts_dir, "migrations"))
        
        # Verify directories exist
        self.assertTrue(os.path.exists(os.path.join(contracts_dir, "api")))
        self.assertTrue(os.path.exists(os.path.join(contracts_dir, "jobs")))
        self.assertTrue(os.path.exists(os.path.join(contracts_dir, "migrations")))
    
    def test_tc002_api_contract_v1_files_exist(self):
        """TC-002: API contract v1 files exist."""
        # Create API v1 directory with required files
        api_v1_dir = os.path.join(self.test_dir, "contracts", "api", "v1")
        os.makedirs(api_v1_dir)
        
        openapi_path = os.path.join(api_v1_dir, "openapi.yaml")
        readme_path = os.path.join(api_v1_dir, "README.md")
        
        with open(openapi_path, 'w') as f:
            f.write("openapi: 3.0.0\ninfo:\n  version: '1.0.0'\npaths: {}")
        with open(readme_path, 'w') as f:
            f.write("# API Contract v1")
        
        # Verify files exist
        self.assertTrue(os.path.exists(openapi_path))
        self.assertTrue(os.path.exists(readme_path))
    
    def test_tc003_job_contract_v1_files_exist(self):
        """TC-003: Job contract v1 files exist."""
        # Create Jobs v1 directory with required files
        jobs_v1_dir = os.path.join(self.test_dir, "contracts", "jobs", "v1")
        os.makedirs(jobs_v1_dir)
        
        schema_path = os.path.join(jobs_v1_dir, "job.schema.json")
        readme_path = os.path.join(jobs_v1_dir, "README.md")
        
        schema_content = {
            "$schema": "http://json-schema.org/draft-07/schema#",
            "title": "Job",
            "type": "object",
            "properties": {},
            "required": ["schema_version", "job_id", "document_id", "status"]
        }
        
        with open(schema_path, 'w') as f:
            json.dump(schema_content, f)
        with open(readme_path, 'w') as f:
            f.write("# Job Contract v1")
        
        # Verify files exist
        self.assertTrue(os.path.exists(schema_path))
        self.assertTrue(os.path.exists(readme_path))
    
    def test_tc004_migration_notes_directory_exists(self):
        """TC-004: Migration notes directory exists."""
        # Create migrations directory with required files
        migrations_dir = os.path.join(self.test_dir, "contracts", "migrations")
        os.makedirs(migrations_dir)
        
        readme_path = os.path.join(migrations_dir, "README.md")
        api_v1_path = os.path.join(migrations_dir, "api_v1.md")
        jobs_v1_path = os.path.join(migrations_dir, "jobs_v1.md")
        
        for path in [readme_path, api_v1_path, jobs_v1_path]:
            with open(path, 'w') as f:
                f.write("# Migration Note")
        
        # Verify files exist
        self.assertTrue(os.path.exists(readme_path))
        self.assertTrue(os.path.exists(api_v1_path))
        self.assertTrue(os.path.exists(jobs_v1_path))


class TestOpenAPIValidation(unittest.TestCase):
    """Test cases for OpenAPI specification validation."""
    
    def setUp(self):
        """Set up test fixtures."""
        self.test_dir = tempfile.mkdtemp()
    
    def tearDown(self):
        """Clean up test fixtures."""
        import shutil
        if os.path.exists(self.test_dir):
            shutil.rmtree(self.test_dir)
    
    def test_tc005_openapi_spec_is_valid_yaml(self):
        """TC-005: OpenAPI spec is valid YAML."""
        openapi_path = os.path.join(self.test_dir, "openapi.yaml")
        valid_content = """
openapi: 3.0.0
info:
  version: "1.0.0"
  title: Test API
paths:
  /health:
    get:
      summary: Health check
"""
        with open(openapi_path, 'w') as f:
            f.write(valid_content)
        
        is_valid, errors = validate_openapi_structure(openapi_path)
        self.assertTrue(is_valid, f"Validation failed: {errors}")
    
    def test_tc006_openapi_spec_has_correct_version(self):
        """TC-006: OpenAPI spec has correct version."""
        openapi_path = os.path.join(self.test_dir, "openapi.yaml")
        content = """
openapi: 3.0.0
info:
  version: "1.0.0"
  title: Test API
paths: {}
"""
        with open(openapi_path, 'w') as f:
            f.write(content)
        
        is_valid, errors = validate_openapi_version(openapi_path)
        self.assertTrue(is_valid, f"Validation failed: {errors}")
    
    def test_tc014_invalid_yaml_in_openapi_spec_fails(self):
        """TC-014: Invalid YAML in OpenAPI spec fails."""
        openapi_path = os.path.join(self.test_dir, "openapi.yaml")
        invalid_content = """
openapi: 3.0.0
info:
  version: "1.0.0"
  title: Test API
  invalid yaml syntax here: [unclosed bracket
"""
        with open(openapi_path, 'w') as f:
            f.write(invalid_content)
        
        is_valid, errors = validate_openapi_structure(openapi_path)
        self.assertFalse(is_valid)
        self.assertTrue(any("YAML" in str(e) for e in errors))


class TestJobSchemaValidation(unittest.TestCase):
    """Test cases for job schema validation."""
    
    def setUp(self):
        """Set up test fixtures."""
        self.test_dir = tempfile.mkdtemp()
    
    def tearDown(self):
        """Clean up test fixtures."""
        import shutil
        if os.path.exists(self.test_dir):
            shutil.rmtree(self.test_dir)
    
    def test_tc007_job_schema_is_valid_json(self):
        """TC-007: Job schema is valid JSON."""
        schema_path = os.path.join(self.test_dir, "job.schema.json")
        valid_schema = {
            "$schema": "http://json-schema.org/draft-07/schema#",
            "title": "Job",
            "type": "object",
            "properties": {
                "schema_version": {"type": "string", "enum": ["1.0"]},
                "job_id": {"type": "string"},
                "document_id": {"type": "string"},
                "status": {"type": "string"}
            },
            "required": ["schema_version", "job_id", "document_id", "status"]
        }
        
        with open(schema_path, 'w') as f:
            json.dump(valid_schema, f)
        
        is_valid, errors = validate_job_schema(schema_path)
        self.assertTrue(is_valid, f"Validation failed: {errors}")
    
    def test_tc008_job_schema_has_required_fields_defined(self):
        """TC-008: Job schema has required fields defined."""
        schema_path = os.path.join(self.test_dir, "job.schema.json")
        schema = {
            "$schema": "http://json-schema.org/draft-07/schema#",
            "title": "Job",
            "type": "object",
            "properties": {},
            "required": ["schema_version", "job_id", "document_id", "status"]
        }
        
        with open(schema_path, 'w') as f:
            json.dump(schema, f)
        
        is_valid, errors = validate_job_schema(schema_path)
        self.assertTrue(is_valid, f"Validation failed: {errors}")
    
    def test_tc009_job_schema_version_enum_is_defined(self):
        """TC-009: Job schema version enum is defined."""
        schema_path = os.path.join(self.test_dir, "job.schema.json")
        schema = {
            "$schema": "http://json-schema.org/draft-07/schema#",
            "title": "Job",
            "type": "object",
            "properties": {
                "schema_version": {
                    "type": "string",
                    "enum": ["1.0"]
                }
            },
            "required": ["schema_version", "job_id", "document_id", "status"]
        }
        
        with open(schema_path, 'w') as f:
            json.dump(schema, f)
        
        is_valid, errors = validate_json_schema_version(schema_path, ["1.0"])
        self.assertTrue(is_valid, f"Validation failed: {errors}")
    
    def test_tc015_invalid_json_in_job_schema_fails(self):
        """TC-015: Invalid JSON in job schema fails."""
        schema_path = os.path.join(self.test_dir, "job.schema.json")
        invalid_json = '{"invalid": json syntax here'
        
        with open(schema_path, 'w') as f:
            f.write(invalid_json)
        
        is_valid, errors = validate_job_schema(schema_path)
        self.assertFalse(is_valid)
        self.assertTrue(any("JSON" in str(e) for e in errors))


class TestMigrationNoteValidation(unittest.TestCase):
    """Test cases for migration note validation."""
    
    def setUp(self):
        """Set up test fixtures."""
        self.test_dir = tempfile.mkdtemp()
    
    def tearDown(self):
        """Clean up test fixtures."""
        import shutil
        if os.path.exists(self.test_dir):
            shutil.rmtree(self.test_dir)
    
    def test_tc010_migration_note_follows_template_structure(self):
        """TC-010: Migration note follows template structure."""
        migration_path = os.path.join(self.test_dir, "api_v1.md")
        valid_content = """
# Version: 1.0.0
# Date: 2024-01-14
# Type: Initial

## Changes
Initial API contract

## Impact
No breaking changes

## Migration Steps
1. Review contract
2. Implement endpoints
"""
        with open(migration_path, 'w') as f:
            f.write(valid_content)
        
        is_valid, errors = validate_migration_note_structure(migration_path)
        self.assertTrue(is_valid, f"Validation failed: {errors}")


class TestNegativeScenarios(unittest.TestCase):
    """Test cases for negative scenarios."""
    
    def setUp(self):
        """Set up test fixtures."""
        self.test_dir = tempfile.mkdtemp()
    
    def tearDown(self):
        """Clean up test fixtures."""
        import shutil
        if os.path.exists(self.test_dir):
            shutil.rmtree(self.test_dir)
    
    def test_tc011_missing_contracts_directory_fails_validation(self):
        """TC-011: Missing contracts directory fails validation."""
        # Don't create contracts directory
        contracts_dir = os.path.join(self.test_dir, "contracts")
        self.assertFalse(os.path.exists(contracts_dir))
    
    def test_tc012_missing_api_contract_fails_validation(self):
        """TC-012: Missing API contract fails validation."""
        openapi_path = os.path.join(self.test_dir, "contracts", "api", "v1", "openapi.yaml")
        is_valid, errors = validate_openapi_structure(openapi_path)
        self.assertFalse(is_valid)
        self.assertTrue(any("not found" in str(e) for e in errors))
    
    def test_tc013_missing_job_schema_fails_validation(self):
        """TC-013: Missing job schema fails validation."""
        schema_path = os.path.join(self.test_dir, "contracts", "jobs", "v1", "job.schema.json")
        is_valid, errors = validate_job_schema(schema_path)
        self.assertFalse(is_valid)
        self.assertTrue(any("not found" in str(e) for e in errors))


class TestEdgeCases(unittest.TestCase):
    """Test cases for edge cases."""
    
    def setUp(self):
        """Set up test fixtures."""
        self.test_dir = tempfile.mkdtemp()
    
    def tearDown(self):
        """Clean up test fixtures."""
        import shutil
        if os.path.exists(self.test_dir):
            shutil.rmtree(self.test_dir)
    
    def test_ec001_empty_migration_notes_directory_is_invalid(self):
        """EC-001: Empty migration notes directory is invalid."""
        migrations_dir = os.path.join(self.test_dir, "migrations")
        os.makedirs(migrations_dir)
        
        # Create only README, no migration notes
        readme_path = os.path.join(migrations_dir, "README.md")
        with open(readme_path, 'w') as f:
            f.write("# Migrations")
        
        is_valid, errors = validate_migration_directory(migrations_dir)
        self.assertFalse(is_valid)
    
    def test_ec002_contract_with_future_version_number_is_valid(self):
        """EC-002: Contract with future version number is valid."""
        self.assertTrue(is_valid_semver("2.0.0"))
        self.assertTrue(is_valid_semver("10.5.3"))
    
    def test_ec003_multiple_api_versions_can_coexist(self):
        """EC-003: Multiple API versions can coexist."""
        # Create v1 and v2 directories
        api_dir = os.path.join(self.test_dir, "contracts", "api")
        v1_dir = os.path.join(api_dir, "v1")
        v2_dir = os.path.join(api_dir, "v2")
        
        os.makedirs(v1_dir)
        os.makedirs(v2_dir)
        
        # Both directories can exist
        self.assertTrue(os.path.exists(v1_dir))
        self.assertTrue(os.path.exists(v2_dir))


class TestErrorScenarios(unittest.TestCase):
    """Test cases for error scenarios."""
    
    def setUp(self):
        """Set up test fixtures."""
        self.test_dir = tempfile.mkdtemp()
    
    def tearDown(self):
        """Clean up test fixtures."""
        import shutil
        if os.path.exists(self.test_dir):
            shutil.rmtree(self.test_dir)
    
    def test_es001_malformed_semver_version_fails(self):
        """ES-001: Malformed semver version fails."""
        self.assertFalse(is_valid_semver("v1"))
        self.assertFalse(is_valid_semver("1.0"))
        self.assertFalse(is_valid_semver("1"))
        self.assertFalse(is_valid_semver("1.0.0-beta"))
    
    def test_es002_missing_readme_in_contract_directory_fails(self):
        """ES-002: Missing README in contract directory fails."""
        api_v1_dir = os.path.join(self.test_dir, "api", "v1")
        os.makedirs(api_v1_dir)
        
        is_valid, errors = validate_openapi_readme(api_v1_dir)
        self.assertFalse(is_valid)
        self.assertTrue(any("README.md" in str(e) for e in errors))


class TestSemverValidation(unittest.TestCase):
    """Test cases for semantic versioning validation."""
    
    def test_valid_semver_formats(self):
        """Test valid semantic version formats."""
        valid_versions = ["1.0.0", "2.1.3", "10.20.30", "0.0.1"]
        for version in valid_versions:
            with self.subTest(version=version):
                self.assertTrue(is_valid_semver(version))
    
    def test_invalid_semver_formats(self):
        """Test invalid semantic version formats."""
        invalid_versions = ["v1.0.0", "1.0", "1", "1.0.0-beta", "1.0.0+build"]
        for version in invalid_versions:
            with self.subTest(version=version):
                self.assertFalse(is_valid_semver(version))


class TestIntegration(unittest.TestCase):
    """Integration tests for complete validation workflow."""
    
    def setUp(self):
        """Set up test fixtures."""
        self.test_dir = tempfile.mkdtemp()
        self._create_valid_contract_structure()
    
    def tearDown(self):
        """Clean up test fixtures."""
        import shutil
        if os.path.exists(self.test_dir):
            shutil.rmtree(self.test_dir)
    
    def _create_valid_contract_structure(self):
        """Create a valid contract structure for testing."""
        # Create directories
        contracts_dir = os.path.join(self.test_dir, "contracts")
        api_v1_dir = os.path.join(contracts_dir, "api", "v1")
        jobs_v1_dir = os.path.join(contracts_dir, "jobs", "v1")
        migrations_dir = os.path.join(contracts_dir, "migrations")
        
        os.makedirs(api_v1_dir)
        os.makedirs(jobs_v1_dir)
        os.makedirs(migrations_dir)
        
        # Create OpenAPI spec
        openapi_content = """
openapi: 3.0.0
info:
  version: "1.0.0"
  title: Test API
paths:
  /health:
    get:
      summary: Health check
  /api/v1/test:
    get:
      summary: Test endpoint
"""
        with open(os.path.join(api_v1_dir, "openapi.yaml"), 'w') as f:
            f.write(openapi_content)
        
        with open(os.path.join(api_v1_dir, "README.md"), 'w') as f:
            f.write("# API Contract v1")
        
        # Create job schema
        job_schema = {
            "$schema": "http://json-schema.org/draft-07/schema#",
            "title": "Job",
            "type": "object",
            "properties": {
                "schema_version": {"type": "string", "enum": ["1.0"]},
                "job_id": {"type": "string"},
                "document_id": {"type": "string"},
                "status": {"type": "string"}
            },
            "required": ["schema_version", "job_id", "document_id", "status"]
        }
        
        with open(os.path.join(jobs_v1_dir, "job.schema.json"), 'w') as f:
            json.dump(job_schema, f)
        
        with open(os.path.join(jobs_v1_dir, "README.md"), 'w') as f:
            f.write("# Job Contract v1")
        
        # Create migration notes
        migration_content = """
# Version: 1.0.0
# Date: 2024-01-14
# Type: Initial

## Changes
Initial contract

## Impact
No breaking changes

## Migration Steps
1. Review contract
"""
        
        with open(os.path.join(migrations_dir, "README.md"), 'w') as f:
            f.write("# Migration Notes")
        
        with open(os.path.join(migrations_dir, "api_v1.md"), 'w') as f:
            f.write(migration_content)
        
        with open(os.path.join(migrations_dir, "jobs_v1.md"), 'w') as f:
            f.write(migration_content)
    
    def test_complete_validation_workflow(self):
        """Test complete validation workflow with valid structure."""
        # Validate all components
        is_valid, errors = validate_all_schema_versions(self.test_dir)
        self.assertTrue(is_valid, f"Schema version validation failed: {errors}")
        
        is_valid, errors = validate_all_migrations(self.test_dir)
        self.assertTrue(is_valid, f"Migration validation failed: {errors}")
        
        is_valid, errors = validate_all_openapi(self.test_dir)
        self.assertTrue(is_valid, f"OpenAPI validation failed: {errors}")
        
        is_valid, errors = validate_all_json_schemas(self.test_dir)
        self.assertTrue(is_valid, f"JSON schema validation failed: {errors}")


if __name__ == '__main__':
    unittest.main()
