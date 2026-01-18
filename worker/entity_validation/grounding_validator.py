"""
Grounding validator for entity extraction payloads.

Enforces "100% grounding required" beyond JSON Schema validation,
ensuring every extracted entity includes valid source citations.
Error messages are PHI-safe (no raw text/value inclusion).
"""

from dataclasses import dataclass, field
from typing import Any, Dict, List, Optional


@dataclass
class GroundingError:
    """A single grounding validation error with PHI-safe details."""
    
    entity_index: int
    field_path: str
    error_type: str
    message: str
    
    def to_dict(self) -> Dict[str, Any]:
        """Convert to dictionary for JSON serialization."""
        return {
            "entity_index": self.entity_index,
            "field_path": self.field_path,
            "error_type": self.error_type,
            "message": self.message,
        }
    
    def __str__(self) -> str:
        """PHI-safe string representation."""
        return f"extracted_entities[{self.entity_index}].{self.field_path}: {self.message}"


@dataclass
class GroundingValidationResult:
    """Result of grounding validation."""
    
    is_valid: bool
    errors: List[GroundingError] = field(default_factory=list)
    
    def to_dict(self) -> Dict[str, Any]:
        """Convert to dictionary for JSON serialization."""
        return {
            "is_valid": self.is_valid,
            "errors": [e.to_dict() for e in self.errors],
            "error_count": len(self.errors),
        }


class GroundingValidationError(Exception):
    """
    Exception raised when grounding validation fails.
    
    Error messages are PHI-safe and do not include raw entity values or source text.
    """
    
    def __init__(self, errors: List[GroundingError]):
        self.errors = errors
        # Build PHI-safe summary message
        error_summary = "; ".join(str(e) for e in errors[:5])
        if len(errors) > 5:
            error_summary += f" ... and {len(errors) - 5} more errors"
        super().__init__(f"Grounding validation failed with {len(errors)} error(s): {error_summary}")
    
    def to_result(self) -> GroundingValidationResult:
        """Convert to GroundingValidationResult."""
        return GroundingValidationResult(is_valid=False, errors=self.errors)


def _validate_source_text(entity: Dict[str, Any], index: int) -> Optional[GroundingError]:
    """Validate source_text field is present and non-empty."""
    source_text = entity.get("source_text")
    
    if source_text is None:
        return GroundingError(
            entity_index=index,
            field_path="source_text",
            error_type="missing",
            message="source_text is required for grounded entities"
        )
    
    if not isinstance(source_text, str):
        return GroundingError(
            entity_index=index,
            field_path="source_text",
            error_type="invalid_type",
            message="source_text must be a string"
        )
    
    if len(source_text.strip()) == 0:
        return GroundingError(
            entity_index=index,
            field_path="source_text",
            error_type="empty",
            message="source_text must not be empty"
        )
    
    return None


def _validate_document_location(entity: Dict[str, Any], index: int) -> List[GroundingError]:
    """Validate document_location field and its required subfields."""
    errors = []
    doc_location = entity.get("document_location")
    
    if doc_location is None:
        errors.append(GroundingError(
            entity_index=index,
            field_path="document_location",
            error_type="missing",
            message="document_location is required for grounded entities"
        ))
        return errors
    
    if not isinstance(doc_location, dict):
        errors.append(GroundingError(
            entity_index=index,
            field_path="document_location",
            error_type="invalid_type",
            message="document_location must be an object"
        ))
        return errors
    
    # Validate page
    page = doc_location.get("page")
    if page is None:
        errors.append(GroundingError(
            entity_index=index,
            field_path="document_location.page",
            error_type="missing",
            message="document_location.page is required"
        ))
    elif not isinstance(page, (int, float)) or page < 1:
        errors.append(GroundingError(
            entity_index=index,
            field_path="document_location.page",
            error_type="invalid_value",
            message="document_location.page must be an integer >= 1"
        ))
    
    # Validate section
    section = doc_location.get("section")
    if section is None:
        errors.append(GroundingError(
            entity_index=index,
            field_path="document_location.section",
            error_type="missing",
            message="document_location.section is required"
        ))
    elif not isinstance(section, str) or len(section.strip()) == 0:
        errors.append(GroundingError(
            entity_index=index,
            field_path="document_location.section",
            error_type="invalid_value",
            message="document_location.section must be a non-empty string"
        ))
    
    # Validate coordinates
    coordinates = doc_location.get("coordinates")
    if coordinates is None:
        errors.append(GroundingError(
            entity_index=index,
            field_path="document_location.coordinates",
            error_type="missing",
            message="document_location.coordinates is required"
        ))
    elif not isinstance(coordinates, dict):
        errors.append(GroundingError(
            entity_index=index,
            field_path="document_location.coordinates",
            error_type="invalid_type",
            message="document_location.coordinates must be an object"
        ))
    else:
        # Validate coordinate fields
        for coord_field in ["x", "y", "width", "height"]:
            coord_value = coordinates.get(coord_field)
            if coord_value is None:
                errors.append(GroundingError(
                    entity_index=index,
                    field_path=f"document_location.coordinates.{coord_field}",
                    error_type="missing",
                    message=f"document_location.coordinates.{coord_field} is required"
                ))
            elif not isinstance(coord_value, (int, float)):
                errors.append(GroundingError(
                    entity_index=index,
                    field_path=f"document_location.coordinates.{coord_field}",
                    error_type="invalid_type",
                    message=f"document_location.coordinates.{coord_field} must be a number"
                ))
    
    return errors


def validate_grounding(payload: Dict[str, Any]) -> GroundingValidationResult:
    """
    Validate that every extracted entity in the payload has valid source citations.
    
    This validation is applied after JSON Schema validation succeeds and enforces
    the "100% grounding required" rule for schema_version 1.1.
    
    Args:
        payload: The entity extraction payload to validate.
    
    Returns:
        GroundingValidationResult with validation status and any errors.
    
    Note:
        - Error messages are PHI-safe (no raw text/value inclusion)
        - Only entity index and field paths are included in errors
    """
    errors: List[GroundingError] = []
    
    # Check schema version - only enforce for 1.1
    schema_version = payload.get("schema_version")
    if schema_version != "1.1":
        # Grounding not enforced for other versions
        return GroundingValidationResult(is_valid=True)
    
    extracted_entities = payload.get("extracted_entities", [])
    
    if not isinstance(extracted_entities, list):
        errors.append(GroundingError(
            entity_index=-1,
            field_path="extracted_entities",
            error_type="invalid_type",
            message="extracted_entities must be an array"
        ))
        return GroundingValidationResult(is_valid=False, errors=errors)
    
    for index, entity in enumerate(extracted_entities):
        if not isinstance(entity, dict):
            errors.append(GroundingError(
                entity_index=index,
                field_path="",
                error_type="invalid_type",
                message="entity must be an object"
            ))
            continue
        
        # Validate source_text
        source_text_error = _validate_source_text(entity, index)
        if source_text_error:
            errors.append(source_text_error)
        
        # Validate document_location
        location_errors = _validate_document_location(entity, index)
        errors.extend(location_errors)
    
    return GroundingValidationResult(
        is_valid=len(errors) == 0,
        errors=errors
    )


def validate_grounding_or_raise(payload: Dict[str, Any]) -> None:
    """
    Validate grounding and raise GroundingValidationError on failure.
    
    Args:
        payload: The entity extraction payload to validate.
    
    Raises:
        GroundingValidationError: If any entity lacks required citation fields.
    """
    result = validate_grounding(payload)
    
    if not result.is_valid:
        raise GroundingValidationError(result.errors)
