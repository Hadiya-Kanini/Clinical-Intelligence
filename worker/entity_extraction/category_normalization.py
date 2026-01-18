"""
Entity category normalization and validation utilities.

Normalizes entity_group_name values using the category registry,
resolving aliases and detecting unknown categories.
"""

import logging
from dataclasses import dataclass, field
from typing import Any, Dict, List, Optional, Tuple

from .category_registry import (
    get_category_registry,
    CategoryRegistry,
)

logger = logging.getLogger(__name__)


@dataclass
class CategoryNormalizationWarning:
    """Warning generated during category normalization."""
    
    entity_index: int
    original_category: str
    message: str
    warning_type: str


@dataclass
class CategoryNormalizationResult:
    """Result of category normalization for a payload."""
    
    normalized_payload: Dict[str, Any]
    warnings: List[CategoryNormalizationWarning] = field(default_factory=list)
    unknown_categories: List[str] = field(default_factory=list)
    has_errors: bool = False
    error_message: Optional[str] = None
    
    @property
    def has_warnings(self) -> bool:
        """Check if there are any warnings."""
        return len(self.warnings) > 0


class UnknownCategoryError(Exception):
    """Raised when an unknown category is encountered."""
    
    def __init__(self, category: str, known_categories: List[str]):
        self.category = category
        self.known_categories = known_categories
        super().__init__(
            f"Unknown category: '{category}'. "
            f"Known categories: {', '.join(known_categories[:5])}..."
        )


def normalize_entity_categories(
    payload: Dict[str, Any],
    registry: Optional[CategoryRegistry] = None,
    fail_on_unknown: bool = True
) -> CategoryNormalizationResult:
    """
    Normalize entity_group_name values using the category registry.
    
    Resolves aliases to canonical IDs and validates that all categories
    are known. Does not modify entity_name or entity_value.
    
    Args:
        payload: The entity extraction payload to normalize.
        registry: Optional category registry. Uses default if not provided.
        fail_on_unknown: If True, unknown categories cause an error result.
    
    Returns:
        CategoryNormalizationResult with normalized payload and any warnings.
    """
    if registry is None:
        registry = get_category_registry()
    
    normalized_payload = payload.copy()
    entities = payload.get("extracted_entities", [])
    
    if not entities:
        return CategoryNormalizationResult(normalized_payload=normalized_payload)
    
    normalized_entities = []
    warnings = []
    unknown_categories = []
    
    for idx, entity in enumerate(entities):
        normalized_entity = entity.copy()
        original_category = entity.get("entity_group_name", "")
        
        if not original_category:
            normalized_entities.append(normalized_entity)
            continue
        
        resolved_id = registry.resolve_category_id(original_category)
        
        if resolved_id is None:
            unknown_categories.append(original_category)
            if fail_on_unknown:
                continue
            else:
                normalized_entities.append(normalized_entity)
                continue
        
        if resolved_id != original_category:
            normalized_entity["entity_group_name"] = resolved_id
            logger.debug(
                "Resolved alias '%s' to canonical category '%s'",
                original_category,
                resolved_id
            )
        
        category = registry.get_category(resolved_id)
        if category and category.is_deprecated:
            warnings.append(CategoryNormalizationWarning(
                entity_index=idx,
                original_category=original_category,
                message=f"Category '{resolved_id}' is deprecated",
                warning_type="deprecated_category",
            ))
        
        normalized_entities.append(normalized_entity)
    
    normalized_payload["extracted_entities"] = normalized_entities
    
    if unknown_categories and fail_on_unknown:
        unique_unknown = sorted(set(unknown_categories))
        return CategoryNormalizationResult(
            normalized_payload=normalized_payload,
            warnings=warnings,
            unknown_categories=unique_unknown,
            has_errors=True,
            error_message=f"Unknown categories: {', '.join(unique_unknown)}",
        )
    
    return CategoryNormalizationResult(
        normalized_payload=normalized_payload,
        warnings=warnings,
        unknown_categories=list(set(unknown_categories)),
    )


def validate_entity_categories(
    payload: Dict[str, Any],
    registry: Optional[CategoryRegistry] = None
) -> Tuple[bool, List[str]]:
    """
    Validate that all entity categories are known.
    
    Args:
        payload: The entity extraction payload to validate.
        registry: Optional category registry. Uses default if not provided.
    
    Returns:
        Tuple of (is_valid, list_of_unknown_categories).
    """
    if registry is None:
        registry = get_category_registry()
    
    entities = payload.get("extracted_entities", [])
    unknown = []
    
    for entity in entities:
        category = entity.get("entity_group_name", "")
        if category and not registry.is_known_category(category):
            unknown.append(category)
    
    return (len(unknown) == 0, list(set(unknown)))


def normalize_and_validate_categories(
    payload: Dict[str, Any],
    registry: Optional[CategoryRegistry] = None
) -> CategoryNormalizationResult:
    """
    Normalize and validate entity categories in one step.
    
    This is the main entry point for category normalization in the
    parsing/validation pipeline.
    
    Args:
        payload: The entity extraction payload.
        registry: Optional category registry.
    
    Returns:
        CategoryNormalizationResult with normalized payload and validation status.
    """
    return normalize_entity_categories(
        payload,
        registry=registry,
        fail_on_unknown=True
    )
