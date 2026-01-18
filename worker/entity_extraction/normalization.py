"""
Deterministic normalization utilities for entity extraction.

Handles missing categories, partial values, and non-standard formats
without hallucinating data or introducing PHI into error messages.
"""

import re
from datetime import datetime
from typing import Any, Dict, List, Optional, Tuple

from .models import CoreEntityCategories


PLACEHOLDER_VALUES = frozenset([
    "n/a", "na", "none", "unknown", "not available", "not specified",
    "null", "nil", "-", "--", "...", "", " "
])

DATE_PATTERNS = [
    (r"^(\d{1,2})/(\d{1,2})/(\d{4})$", "%m/%d/%Y"),
    (r"^(\d{1,2})-(\d{1,2})-(\d{4})$", "%m-%d-%Y"),
    (r"^(\d{4})/(\d{1,2})/(\d{1,2})$", "%Y/%m/%d"),
    (r"^(\d{4})-(\d{1,2})-(\d{1,2})$", "%Y-%m-%d"),
    (r"^(\d{1,2})-([A-Za-z]{3})-(\d{4})$", "%d-%b-%Y"),
    (r"^(\d{1,2})\s+([A-Za-z]{3,9})\s+(\d{4})$", "%d %B %Y"),
    (r"^([A-Za-z]{3,9})\s+(\d{1,2}),?\s+(\d{4})$", "%B %d %Y"),
]


def is_placeholder_value(value: str) -> bool:
    """
    Check if a value is a placeholder that should be treated as missing.
    
    Args:
        value: The value to check.
    
    Returns:
        True if the value is a placeholder, False otherwise.
    """
    if not value:
        return True
    return value.strip().lower() in PLACEHOLDER_VALUES


def normalize_date(value: str) -> str:
    """
    Attempt to normalize a date string to YYYY-MM-DD format.
    
    Only normalizes when the input clearly matches a known pattern.
    Returns the original value if normalization is not possible.
    
    Args:
        value: The date string to normalize.
    
    Returns:
        Normalized date in YYYY-MM-DD format, or original value if ambiguous.
    """
    if not value or not value.strip():
        return value
    
    cleaned = value.strip()
    
    if re.match(r"^\d{4}-\d{2}-\d{2}$", cleaned):
        return cleaned
    
    for pattern, date_format in DATE_PATTERNS:
        if re.match(pattern, cleaned, re.IGNORECASE):
            try:
                parsed = datetime.strptime(cleaned, date_format)
                return parsed.strftime("%Y-%m-%d")
            except ValueError:
                continue
    
    return value


def normalize_entity_value(
    entity_group_name: str,
    entity_name: str,
    entity_value: str
) -> Tuple[str, bool]:
    """
    Normalize an entity value based on its category and name.
    
    Args:
        entity_group_name: The entity category.
        entity_name: The entity name/label.
        entity_value: The raw entity value.
    
    Returns:
        Tuple of (normalized_value, was_modified).
    """
    if is_placeholder_value(entity_value):
        return entity_value, False
    
    date_fields = {"dob", "date", "start_date", "end_date", "effective_date"}
    if entity_name.lower() in date_fields:
        normalized = normalize_date(entity_value)
        return normalized, normalized != entity_value
    
    return entity_value, False


def filter_placeholder_entities(entities: List[Dict[str, Any]]) -> List[Dict[str, Any]]:
    """
    Remove entities with placeholder values.
    
    Args:
        entities: List of entity dictionaries.
    
    Returns:
        Filtered list with placeholder entities removed.
    """
    result = []
    for entity in entities:
        value = entity.get("entity_value", "")
        if not is_placeholder_value(str(value)):
            result.append(entity)
    return result


def normalize_entities(
    entities: List[Dict[str, Any]],
    remove_placeholders: bool = True
) -> List[Dict[str, Any]]:
    """
    Normalize a list of extracted entities.
    
    Applies:
    - Placeholder value filtering (optional)
    - Date normalization for date fields
    - No content fabrication
    
    Args:
        entities: List of entity dictionaries.
        remove_placeholders: Whether to remove placeholder values.
    
    Returns:
        Normalized list of entities.
    """
    if remove_placeholders:
        entities = filter_placeholder_entities(entities)
    
    result = []
    for entity in entities:
        normalized_entity = entity.copy()
        
        group_name = entity.get("entity_group_name", "")
        entity_name = entity.get("entity_name", "")
        entity_value = entity.get("entity_value", "")
        
        if entity_value:
            normalized_value, _ = normalize_entity_value(
                group_name, entity_name, str(entity_value)
            )
            normalized_entity["entity_value"] = normalized_value
        
        result.append(normalized_entity)
    
    return result


def normalize_payload(
    payload: Dict[str, Any],
    remove_placeholders: bool = True
) -> Dict[str, Any]:
    """
    Normalize an entity extraction payload.
    
    Args:
        payload: The entity extraction payload.
        remove_placeholders: Whether to remove placeholder values.
    
    Returns:
        Normalized payload with entities processed.
    """
    result = payload.copy()
    
    entities = payload.get("extracted_entities", [])
    if entities:
        result["extracted_entities"] = normalize_entities(
            entities, remove_placeholders
        )
    
    return result


def get_categories_in_payload(payload: Dict[str, Any]) -> List[str]:
    """
    Get the unique categories present in a payload.
    
    Args:
        payload: The entity extraction payload.
    
    Returns:
        List of unique entity_group_name values.
    """
    entities = payload.get("extracted_entities", [])
    categories = set()
    for entity in entities:
        group_name = entity.get("entity_group_name")
        if group_name:
            categories.add(group_name)
    return sorted(categories)


def get_missing_core_categories(payload: Dict[str, Any]) -> List[str]:
    """
    Get core categories that are not present in the payload.
    
    Args:
        payload: The entity extraction payload.
    
    Returns:
        List of missing core category IDs.
    """
    present = set(get_categories_in_payload(payload))
    all_core = set(CoreEntityCategories.all_categories())
    return sorted(all_core - present)
