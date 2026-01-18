"""
Versioned Pydantic entity schemas for entity extraction validation.

This package provides:
- Pydantic models aligned to contracts/entities/v1/entity.schema.json
- Schema version registry for resolving validators by version
- Backward compatibility support for future schema versions
"""

from worker.entity_schemas.v1 import (
    EntityExtractionResultV1,
    ExtractedEntityV1,
    DocumentLocationV1,
    ConflictV1,
    CoordinatesV1,
)
from worker.entity_schemas.registry import (
    get_entity_schema,
    get_supported_versions,
    UnsupportedSchemaVersionError,
)

__all__ = [
    "EntityExtractionResultV1",
    "ExtractedEntityV1",
    "DocumentLocationV1",
    "ConflictV1",
    "CoordinatesV1",
    "get_entity_schema",
    "get_supported_versions",
    "UnsupportedSchemaVersionError",
]
