"""
Unit tests for entity category normalization.

Tests cover:
- Alias resolution to canonical IDs
- Unknown category detection and failure
- Deprecated category warnings
- Core category preservation
"""

import pytest

from worker.entity_extraction.category_normalization import (
    normalize_entity_categories,
    validate_entity_categories,
    normalize_and_validate_categories,
    CategoryNormalizationResult,
    UnknownCategoryError,
)
from worker.entity_extraction.category_registry import (
    load_category_registry,
    Category,
    CategoryRegistry,
)


class TestAliasResolution:
    """Tests for alias resolution to canonical IDs."""

    def test_alias_resolves_to_canonical_id(self):
        """Alias is resolved to canonical category ID."""
        payload = {
            "schema_version": "1.0",
            "document_id": "doc-123",
            "extracted_entities": [
                {
                    "entity_group_name": "labs",
                    "entity_name": "test_name",
                    "entity_value": "Hemoglobin"
                }
            ]
        }
        
        result = normalize_entity_categories(payload)
        
        assert result.has_errors is False
        entities = result.normalized_payload["extracted_entities"]
        assert len(entities) == 1
        assert entities[0]["entity_group_name"] == "lab_results"

    def test_vitals_alias_resolves(self):
        """'vitals' alias resolves to 'vital_signs'."""
        payload = {
            "extracted_entities": [
                {"entity_group_name": "vitals", "entity_name": "bp", "entity_value": "120/80"}
            ]
        }
        
        result = normalize_entity_categories(payload)
        
        assert result.normalized_payload["extracted_entities"][0]["entity_group_name"] == "vital_signs"

    def test_canonical_id_unchanged(self):
        """Canonical category ID remains unchanged."""
        payload = {
            "extracted_entities": [
                {"entity_group_name": "medications", "entity_name": "med", "entity_value": "Aspirin"}
            ]
        }
        
        result = normalize_entity_categories(payload)
        
        assert result.normalized_payload["extracted_entities"][0]["entity_group_name"] == "medications"

    def test_entity_name_and_value_preserved(self):
        """entity_name and entity_value are not modified."""
        payload = {
            "extracted_entities": [
                {
                    "entity_group_name": "meds",
                    "entity_name": "medication_name",
                    "entity_value": "Lisinopril 10mg"
                }
            ]
        }
        
        result = normalize_entity_categories(payload)
        
        entity = result.normalized_payload["extracted_entities"][0]
        assert entity["entity_name"] == "medication_name"
        assert entity["entity_value"] == "Lisinopril 10mg"


class TestUnknownCategoryHandling:
    """Tests for unknown category detection."""

    def test_unknown_category_fails_by_default(self):
        """Unknown category causes error result by default."""
        payload = {
            "extracted_entities": [
                {"entity_group_name": "fake_category", "entity_name": "test", "entity_value": "value"}
            ]
        }
        
        result = normalize_entity_categories(payload)
        
        assert result.has_errors is True
        assert "fake_category" in result.unknown_categories
        assert "fake_category" in result.error_message

    def test_unknown_category_with_fail_on_unknown_false(self):
        """Unknown category preserved when fail_on_unknown=False."""
        payload = {
            "extracted_entities": [
                {"entity_group_name": "fake_category", "entity_name": "test", "entity_value": "value"}
            ]
        }
        
        result = normalize_entity_categories(payload, fail_on_unknown=False)
        
        assert result.has_errors is False
        assert "fake_category" in result.unknown_categories
        entities = result.normalized_payload["extracted_entities"]
        assert len(entities) == 1

    def test_multiple_unknown_categories(self):
        """Multiple unknown categories are all reported."""
        payload = {
            "extracted_entities": [
                {"entity_group_name": "unknown1", "entity_name": "a", "entity_value": "1"},
                {"entity_group_name": "unknown2", "entity_name": "b", "entity_value": "2"},
                {"entity_group_name": "medications", "entity_name": "c", "entity_value": "3"},
            ]
        }
        
        result = normalize_entity_categories(payload)
        
        assert result.has_errors is True
        assert "unknown1" in result.unknown_categories
        assert "unknown2" in result.unknown_categories


class TestDeprecatedCategoryHandling:
    """Tests for deprecated category handling."""

    def test_deprecated_category_generates_warning(self):
        """Deprecated category generates a warning but is accepted."""
        categories = [
            Category("active_cat", "Active", "", "active", (), ()),
            Category("deprecated_cat", "Deprecated", "", "deprecated", (), ()),
        ]
        registry = CategoryRegistry(schema_version="1.0", categories=categories)
        
        payload = {
            "extracted_entities": [
                {"entity_group_name": "deprecated_cat", "entity_name": "test", "entity_value": "value"}
            ]
        }
        
        result = normalize_entity_categories(payload, registry=registry)
        
        assert result.has_errors is False
        assert result.has_warnings is True
        assert len(result.warnings) == 1
        assert result.warnings[0].warning_type == "deprecated_category"


class TestValidateEntityCategories:
    """Tests for validate_entity_categories function."""

    def test_valid_categories_pass(self):
        """Valid categories pass validation."""
        payload = {
            "extracted_entities": [
                {"entity_group_name": "medications", "entity_name": "med", "entity_value": "Aspirin"},
                {"entity_group_name": "allergies", "entity_name": "allergen", "entity_value": "Penicillin"},
            ]
        }
        
        is_valid, unknown = validate_entity_categories(payload)
        
        assert is_valid is True
        assert len(unknown) == 0

    def test_unknown_categories_fail(self):
        """Unknown categories fail validation."""
        payload = {
            "extracted_entities": [
                {"entity_group_name": "not_a_category", "entity_name": "test", "entity_value": "value"}
            ]
        }
        
        is_valid, unknown = validate_entity_categories(payload)
        
        assert is_valid is False
        assert "not_a_category" in unknown


class TestCoreCategoryPreservation:
    """Tests for core category preservation."""

    def test_core_categories_unchanged(self):
        """Core categories remain unchanged after normalization."""
        registry = load_category_registry()
        core_categories = registry.get_active_category_ids()
        
        payload = {
            "extracted_entities": [
                {"entity_group_name": cat, "entity_name": "test", "entity_value": "value"}
                for cat in core_categories
            ]
        }
        
        result = normalize_entity_categories(payload)
        
        assert result.has_errors is False
        normalized_categories = [
            e["entity_group_name"] 
            for e in result.normalized_payload["extracted_entities"]
        ]
        assert normalized_categories == core_categories

    def test_adding_new_category_does_not_affect_existing(self):
        """Adding a new category to registry doesn't affect existing categories."""
        categories = [
            Category("existing_cat", "Existing", "", "active", (), ()),
            Category("new_cat", "New", "", "active", (), ()),
        ]
        registry = CategoryRegistry(schema_version="1.0", categories=categories)
        
        payload = {
            "extracted_entities": [
                {"entity_group_name": "existing_cat", "entity_name": "test", "entity_value": "value"}
            ]
        }
        
        result = normalize_entity_categories(payload, registry=registry)
        
        assert result.has_errors is False
        assert result.normalized_payload["extracted_entities"][0]["entity_group_name"] == "existing_cat"


class TestEmptyPayload:
    """Tests for empty payload handling."""

    def test_empty_entities_list(self):
        """Empty entities list is handled correctly."""
        payload = {"extracted_entities": []}
        
        result = normalize_entity_categories(payload)
        
        assert result.has_errors is False
        assert len(result.normalized_payload["extracted_entities"]) == 0

    def test_missing_entities_key(self):
        """Missing extracted_entities key is handled correctly."""
        payload = {"schema_version": "1.0"}
        
        result = normalize_entity_categories(payload)
        
        assert result.has_errors is False
