"""
Entity validation module for Pydantic-based validation of extracted entities.

Provides:
- Pydantic validation integration with schema registry
- PHI-safe error normalization for persistence
- Deterministic validation failure semantics
"""

from worker.entity_validation.entity_validator import (
    validate_entity_payload_with_pydantic,
    normalize_validation_errors,
    EntityValidationResult,
    ValidationErrorDetail,
    EntityValidationError,
)

__all__ = [
    "validate_entity_payload_with_pydantic",
    "normalize_validation_errors",
    "EntityValidationResult",
    "ValidationErrorDetail",
    "EntityValidationError",
]
