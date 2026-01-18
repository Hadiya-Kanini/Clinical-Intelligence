"""
Unit tests for entity category registry loader and validator.

Tests cover:
- Registry loading and schema validation
- Deterministic ordering of categories
- Alias resolution
- Conflict detection (duplicate IDs, alias conflicts)
- Active/deprecated category filtering
"""

import json
import os
import tempfile
import pytest

from worker.entity_extraction.category_registry import (
    load_category_registry,
    get_category_registry,
    Category,
    CategoryRegistry,
    RegistryLoadError,
    RegistryValidationError,
    CategoryConflictError,
)


class TestCategoryRegistryLoading:
    """Tests for registry loading functionality."""

    def test_load_default_registry_succeeds(self):
        """Default registry loads successfully."""
        registry = load_category_registry()
        
        assert registry is not None
        assert registry.schema_version == "1.0"
        assert len(registry.categories) >= 10

    def test_load_registry_contains_core_categories(self):
        """Registry contains all 10 core categories."""
        registry = load_category_registry()
        
        core_categories = [
            "patient_demographics",
            "allergies",
            "medications",
            "diagnoses",
            "procedures",
            "lab_results",
            "vital_signs",
            "social_history",
            "clinical_notes",
            "document_metadata",
        ]
        
        category_ids = registry.get_all_category_ids()
        for core_cat in core_categories:
            assert core_cat in category_ids, f"Missing core category: {core_cat}"

    def test_load_missing_file_raises_error(self):
        """Missing registry file raises RegistryLoadError."""
        with pytest.raises(RegistryLoadError, match="not found"):
            load_category_registry(registry_path="/nonexistent/path.json")

    def test_load_invalid_json_raises_error(self):
        """Invalid JSON raises RegistryLoadError."""
        with tempfile.NamedTemporaryFile(mode="w", suffix=".json", delete=False) as f:
            f.write("{ invalid json }")
            temp_path = f.name
        
        try:
            with pytest.raises(RegistryLoadError, match="Invalid JSON"):
                load_category_registry(registry_path=temp_path, validate_schema=False)
        finally:
            os.unlink(temp_path)


class TestCategoryRegistryValidation:
    """Tests for registry schema validation."""

    def test_valid_registry_passes_validation(self):
        """Valid registry passes schema validation."""
        valid_registry = {
            "schema_version": "1.0",
            "categories": [
                {
                    "category_id": "test_category",
                    "display_name": "Test Category",
                    "status": "active"
                }
            ]
        }
        
        with tempfile.NamedTemporaryFile(mode="w", suffix=".json", delete=False) as f:
            json.dump(valid_registry, f)
            temp_path = f.name
        
        try:
            registry = load_category_registry(registry_path=temp_path)
            assert len(registry.categories) == 1
        finally:
            os.unlink(temp_path)

    def test_missing_required_field_fails_validation(self):
        """Missing required field fails schema validation."""
        invalid_registry = {
            "schema_version": "1.0",
            "categories": [
                {
                    "category_id": "test_category",
                    "display_name": "Test Category"
                }
            ]
        }
        
        with tempfile.NamedTemporaryFile(mode="w", suffix=".json", delete=False) as f:
            json.dump(invalid_registry, f)
            temp_path = f.name
        
        try:
            with pytest.raises(RegistryValidationError, match="failed schema validation"):
                load_category_registry(registry_path=temp_path)
        finally:
            os.unlink(temp_path)


class TestCategoryConflictDetection:
    """Tests for conflict detection in category registry."""

    def test_duplicate_category_id_raises_error(self):
        """Duplicate category_id raises CategoryConflictError."""
        categories = [
            Category("test", "Test 1", "", "active", (), ()),
            Category("test", "Test 2", "", "active", (), ()),
        ]
        
        with pytest.raises(CategoryConflictError, match="Duplicate category_id"):
            CategoryRegistry(schema_version="1.0", categories=categories)

    def test_alias_conflicts_with_category_id_raises_error(self):
        """Alias that equals another category_id raises error."""
        categories = [
            Category("category_a", "Category A", "", "active", (), ()),
            Category("category_b", "Category B", "", "active", ("category_a",), ()),
        ]
        
        with pytest.raises(CategoryConflictError, match="conflicts with category_id"):
            CategoryRegistry(schema_version="1.0", categories=categories)

    def test_alias_used_by_multiple_categories_raises_error(self):
        """Alias used by multiple categories raises error."""
        categories = [
            Category("category_a", "Category A", "", "active", ("shared_alias",), ()),
            Category("category_b", "Category B", "", "active", ("shared_alias",), ()),
        ]
        
        with pytest.raises(CategoryConflictError, match="used by multiple categories"):
            CategoryRegistry(schema_version="1.0", categories=categories)


class TestCategoryResolution:
    """Tests for category ID and alias resolution."""

    def test_resolve_direct_category_id(self):
        """Direct category ID resolves to itself."""
        registry = load_category_registry()
        
        resolved = registry.resolve_category_id("patient_demographics")
        assert resolved == "patient_demographics"

    def test_resolve_alias_to_canonical_id(self):
        """Alias resolves to canonical category ID."""
        registry = load_category_registry()
        
        resolved = registry.resolve_category_id("labs")
        assert resolved == "lab_results"
        
        resolved = registry.resolve_category_id("vitals")
        assert resolved == "vital_signs"

    def test_resolve_unknown_returns_none(self):
        """Unknown category returns None."""
        registry = load_category_registry()
        
        resolved = registry.resolve_category_id("unknown_category")
        assert resolved is None

    def test_is_known_category_for_valid_id(self):
        """is_known_category returns True for valid ID."""
        registry = load_category_registry()
        
        assert registry.is_known_category("medications") is True
        assert registry.is_known_category("meds") is True

    def test_is_known_category_for_unknown_id(self):
        """is_known_category returns False for unknown ID."""
        registry = load_category_registry()
        
        assert registry.is_known_category("fake_category") is False


class TestCategoryOrdering:
    """Tests for deterministic category ordering."""

    def test_get_active_category_ids_is_deterministic(self):
        """Active category IDs are returned in deterministic order."""
        registry = load_category_registry()
        
        ids1 = registry.get_active_category_ids()
        ids2 = registry.get_active_category_ids()
        
        assert ids1 == ids2

    def test_get_all_category_ids_preserves_order(self):
        """All category IDs preserve registration order."""
        categories = [
            Category("first", "First", "", "active", (), ()),
            Category("second", "Second", "", "active", (), ()),
            Category("third", "Third", "", "deprecated", (), ()),
        ]
        
        registry = CategoryRegistry(schema_version="1.0", categories=categories)
        
        all_ids = registry.get_all_category_ids()
        assert all_ids == ["first", "second", "third"]
        
        active_ids = registry.get_active_category_ids()
        assert active_ids == ["first", "second"]


class TestCategoryProperties:
    """Tests for Category dataclass properties."""

    def test_category_is_active(self):
        """Category.is_active returns correct value."""
        active = Category("test", "Test", "", "active", (), ())
        deprecated = Category("test2", "Test2", "", "deprecated", (), ())
        
        assert active.is_active is True
        assert active.is_deprecated is False
        assert deprecated.is_active is False
        assert deprecated.is_deprecated is True

    def test_get_category_returns_category_object(self):
        """get_category returns the Category object."""
        registry = load_category_registry()
        
        cat = registry.get_category("medications")
        
        assert cat is not None
        assert cat.category_id == "medications"
        assert cat.display_name == "Medications"
        assert cat.is_active is True

    def test_get_category_for_prompt(self):
        """get_category_for_prompt returns formatted dict."""
        registry = load_category_registry()
        
        prompt_data = registry.get_category_for_prompt("medications")
        
        assert prompt_data["category_id"] == "medications"
        assert prompt_data["display_name"] == "Medications"
        assert "medication_name" in prompt_data["recommended_entity_names"]


class TestCachedRegistry:
    """Tests for cached registry access."""

    def test_get_category_registry_returns_same_instance(self):
        """get_category_registry returns cached instance."""
        registry1 = get_category_registry()
        registry2 = get_category_registry()
        
        assert registry1 is registry2

    def test_force_reload_returns_new_instance(self):
        """force_reload=True returns new instance."""
        registry1 = get_category_registry()
        registry2 = get_category_registry(force_reload=True)
        
        assert registry1 is not registry2
