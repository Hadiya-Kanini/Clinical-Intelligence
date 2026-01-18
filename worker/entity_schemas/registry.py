"""
Schema version registry for entity extraction validation.

Provides a centralized mechanism to resolve Pydantic models by schema version,
supporting backward compatibility for future schema versions.
"""

from typing import Dict, List, Type

from pydantic import BaseModel

from worker.entity_schemas.v1 import EntityExtractionResultV1


class UnsupportedSchemaVersionError(Exception):
    """Raised when an unknown or unsupported schema version is requested."""
    
    def __init__(self, version: str, supported_versions: List[str]):
        self.version = version
        self.supported_versions = supported_versions
        super().__init__(
            f"Unsupported schema version: '{version}'. "
            f"Supported versions: {', '.join(supported_versions)}"
        )


_SCHEMA_REGISTRY: Dict[str, Type[BaseModel]] = {
    "1.0": EntityExtractionResultV1,
}


def get_entity_schema(schema_version: str) -> Type[BaseModel]:
    """
    Resolve the Pydantic model for a given schema version.
    
    Args:
        schema_version: The schema version string (e.g., "1.0").
    
    Returns:
        The Pydantic model class for the specified version.
    
    Raises:
        UnsupportedSchemaVersionError: If the version is not supported.
    """
    if not schema_version:
        raise UnsupportedSchemaVersionError(
            version="<empty>",
            supported_versions=list(_SCHEMA_REGISTRY.keys())
        )
    
    schema_class = _SCHEMA_REGISTRY.get(schema_version)
    if schema_class is None:
        raise UnsupportedSchemaVersionError(
            version=schema_version,
            supported_versions=list(_SCHEMA_REGISTRY.keys())
        )
    
    return schema_class


def get_supported_versions() -> List[str]:
    """
    Get the list of supported schema versions.
    
    Returns:
        List of supported version strings in registration order.
    """
    return list(_SCHEMA_REGISTRY.keys())


def is_version_supported(schema_version: str) -> bool:
    """
    Check if a schema version is supported.
    
    Args:
        schema_version: The schema version to check.
    
    Returns:
        True if the version is supported, False otherwise.
    """
    return schema_version in _SCHEMA_REGISTRY
