"""
Entity category registry loader and validator.

Loads extraction categories from the canonical registry contract and provides
APIs for category resolution, alias handling, and deterministic ordering.
"""

import json
import os
from dataclasses import dataclass, field
from typing import Dict, List, Optional, Set

try:
    from jsonschema import Draft7Validator
except ModuleNotFoundError as e:
    raise ModuleNotFoundError(
        "Missing dependency 'jsonschema'. Install worker requirements."
    ) from e


class CategoryRegistryError(Exception):
    """Base exception for category registry errors."""
    pass


class RegistryLoadError(CategoryRegistryError):
    """Raised when the registry file cannot be loaded."""
    pass


class RegistryValidationError(CategoryRegistryError):
    """Raised when the registry fails schema validation."""
    pass


class CategoryConflictError(CategoryRegistryError):
    """Raised when category IDs or aliases conflict."""
    pass


@dataclass(frozen=True)
class Category:
    """A single category definition from the registry."""
    
    category_id: str
    display_name: str
    description: str
    status: str
    aliases: tuple
    recommended_entity_names: tuple
    
    @property
    def is_active(self) -> bool:
        """Check if the category is active."""
        return self.status == "active"
    
    @property
    def is_deprecated(self) -> bool:
        """Check if the category is deprecated."""
        return self.status == "deprecated"


@dataclass
class CategoryRegistry:
    """
    Loaded and validated category registry.
    
    Provides APIs for category lookup, alias resolution, and ordering.
    """
    
    schema_version: str
    categories: List[Category] = field(default_factory=list)
    _id_to_category: Dict[str, Category] = field(default_factory=dict, repr=False)
    _alias_to_id: Dict[str, str] = field(default_factory=dict, repr=False)
    
    def __post_init__(self):
        """Build lookup indexes after initialization."""
        self._build_indexes()
    
    def _build_indexes(self):
        """Build category ID and alias lookup indexes."""
        self._id_to_category = {}
        self._alias_to_id = {}
        
        all_ids: Set[str] = set()
        all_aliases: Set[str] = set()
        
        for cat in self.categories:
            if cat.category_id in all_ids:
                raise CategoryConflictError(
                    f"Duplicate category_id: '{cat.category_id}'"
                )
            all_ids.add(cat.category_id)
            self._id_to_category[cat.category_id] = cat
        
        for cat in self.categories:
            for alias in cat.aliases:
                if alias in all_ids:
                    raise CategoryConflictError(
                        f"Alias '{alias}' conflicts with category_id"
                    )
                if alias in all_aliases:
                    raise CategoryConflictError(
                        f"Alias '{alias}' is used by multiple categories"
                    )
                all_aliases.add(alias)
                self._alias_to_id[alias] = cat.category_id
    
    def get_active_category_ids(self) -> List[str]:
        """
        Get all active category IDs in deterministic order.
        
        Returns:
            List of active category IDs in registration order.
        """
        return [cat.category_id for cat in self.categories if cat.is_active]
    
    def get_all_category_ids(self) -> List[str]:
        """
        Get all category IDs in deterministic order.
        
        Returns:
            List of all category IDs in registration order.
        """
        return [cat.category_id for cat in self.categories]
    
    def resolve_category_id(self, input_id: str) -> Optional[str]:
        """
        Resolve an input ID to its canonical category ID.
        
        Handles both direct category IDs and aliases.
        
        Args:
            input_id: The category ID or alias to resolve.
        
        Returns:
            The canonical category ID, or None if not found.
        """
        if input_id in self._id_to_category:
            return input_id
        return self._alias_to_id.get(input_id)
    
    def get_category(self, category_id: str) -> Optional[Category]:
        """
        Get a category by its ID.
        
        Args:
            category_id: The category ID to look up.
        
        Returns:
            The Category object, or None if not found.
        """
        return self._id_to_category.get(category_id)
    
    def is_known_category(self, input_id: str) -> bool:
        """
        Check if an input ID is a known category or alias.
        
        Args:
            input_id: The category ID or alias to check.
        
        Returns:
            True if the ID is known, False otherwise.
        """
        return self.resolve_category_id(input_id) is not None
    
    def get_category_for_prompt(self, category_id: str) -> Dict:
        """
        Get category information formatted for prompt building.
        
        Args:
            category_id: The category ID.
        
        Returns:
            Dictionary with category_id, display_name, and recommended_entity_names.
        """
        cat = self._id_to_category.get(category_id)
        if not cat:
            return {}
        return {
            "category_id": cat.category_id,
            "display_name": cat.display_name,
            "recommended_entity_names": list(cat.recommended_entity_names),
        }


def _repo_root() -> str:
    """Get the repository root directory."""
    return os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))


def _default_registry_path() -> str:
    """Get the default path to the category registry."""
    return os.path.join(
        _repo_root(), "contracts", "entities", "v1", "entity_categories.json"
    )


def _default_schema_path() -> str:
    """Get the default path to the category registry schema."""
    return os.path.join(
        _repo_root(), "contracts", "entities", "v1", "entity_categories.schema.json"
    )


def load_category_registry(
    registry_path: Optional[str] = None,
    schema_path: Optional[str] = None,
    validate_schema: bool = True
) -> CategoryRegistry:
    """
    Load and validate the category registry.
    
    Args:
        registry_path: Path to the registry JSON file. Defaults to contract location.
        schema_path: Path to the schema JSON file. Defaults to contract location.
        validate_schema: Whether to validate against the JSON schema.
    
    Returns:
        Loaded and validated CategoryRegistry.
    
    Raises:
        RegistryLoadError: If the registry file cannot be loaded.
        RegistryValidationError: If schema validation fails.
        CategoryConflictError: If category IDs or aliases conflict.
    """
    registry_path = registry_path or _default_registry_path()
    schema_path = schema_path or _default_schema_path()
    
    try:
        with open(registry_path, "r", encoding="utf-8") as f:
            registry_data = json.load(f)
    except FileNotFoundError:
        raise RegistryLoadError(
            f"Category registry file not found: {registry_path}"
        )
    except json.JSONDecodeError as e:
        raise RegistryLoadError(
            f"Invalid JSON in category registry: {e}"
        )
    
    if validate_schema:
        try:
            with open(schema_path, "r", encoding="utf-8") as f:
                schema = json.load(f)
        except FileNotFoundError:
            raise RegistryLoadError(
                f"Category registry schema not found: {schema_path}"
            )
        except json.JSONDecodeError as e:
            raise RegistryLoadError(
                f"Invalid JSON in category registry schema: {e}"
            )
        
        validator = Draft7Validator(schema)
        errors = sorted(validator.iter_errors(registry_data), key=lambda e: e.path)
        if errors:
            messages = [f"{list(e.path)}: {e.message}" for e in errors]
            raise RegistryValidationError(
                "Category registry failed schema validation: " + "; ".join(messages)
            )
    
    categories = []
    for cat_data in registry_data.get("categories", []):
        category = Category(
            category_id=cat_data["category_id"],
            display_name=cat_data["display_name"],
            description=cat_data.get("description", ""),
            status=cat_data["status"],
            aliases=tuple(cat_data.get("aliases", [])),
            recommended_entity_names=tuple(cat_data.get("recommended_entity_names", [])),
        )
        categories.append(category)
    
    return CategoryRegistry(
        schema_version=registry_data.get("schema_version", "1.0"),
        categories=categories,
    )


_cached_registry: Optional[CategoryRegistry] = None


def get_category_registry(force_reload: bool = False) -> CategoryRegistry:
    """
    Get the category registry, loading it if necessary.
    
    Uses a module-level cache to avoid repeated file I/O.
    
    Args:
        force_reload: Force reload from disk even if cached.
    
    Returns:
        The loaded CategoryRegistry.
    """
    global _cached_registry
    
    if _cached_registry is None or force_reload:
        _cached_registry = load_category_registry()
    
    return _cached_registry
