"""
Pydantic-based entity payload validation with PHI-safe error normalization.

Validates entity extraction payloads using versioned Pydantic schemas
and produces structured, persistence-friendly error details.
"""

from dataclasses import dataclass, field
from typing import Any, Dict, List, Optional

from pydantic import ValidationError

from worker.entity_schemas.registry import (
    get_entity_schema,
    UnsupportedSchemaVersionError,
)


@dataclass
class ValidationErrorDetail:
    """A single validation error with location and message."""
    
    field_path: str
    error_type: str
    message: str
    
    def to_dict(self) -> Dict[str, str]:
        """Convert to dictionary for JSON serialization."""
        return {
            "field_path": self.field_path,
            "error_type": self.error_type,
            "message": self.message,
        }


@dataclass
class EntityValidationResult:
    """Result of entity payload validation."""
    
    is_valid: bool
    error_message: Optional[str] = None
    error_details: List[ValidationErrorDetail] = field(default_factory=list)
    validated_payload: Optional[Dict[str, Any]] = None
    
    def to_dict(self) -> Dict[str, Any]:
        """Convert to dictionary for JSON serialization."""
        result = {"is_valid": self.is_valid}
        if self.error_message:
            result["error_message"] = self.error_message
        if self.error_details:
            result["error_details"] = [e.to_dict() for e in self.error_details]
        return result


class EntityValidationError(Exception):
    """Exception raised when entity validation fails."""
    
    def __init__(
        self,
        message: str,
        error_details: Optional[List[ValidationErrorDetail]] = None
    ):
        super().__init__(message)
        self.error_message = message
        self.error_details = error_details or []
    
    def to_result(self) -> EntityValidationResult:
        """Convert to EntityValidationResult."""
        return EntityValidationResult(
            is_valid=False,
            error_message=self.error_message,
            error_details=self.error_details,
        )


def normalize_validation_errors(
    validation_error: ValidationError,
    max_message_length: int = 200
) -> List[ValidationErrorDetail]:
    """
    Normalize Pydantic ValidationError into PHI-safe error details.
    
    Args:
        validation_error: The Pydantic ValidationError to normalize.
        max_message_length: Maximum length for error messages (truncates if exceeded).
    
    Returns:
        List of normalized ValidationErrorDetail objects.
    """
    details = []
    
    for error in validation_error.errors():
        field_path = ".".join(str(loc) for loc in error.get("loc", []))
        error_type = error.get("type", "unknown")
        
        message = error.get("msg", "Validation failed")
        if len(message) > max_message_length:
            message = message[:max_message_length - 3] + "..."
        
        details.append(ValidationErrorDetail(
            field_path=field_path,
            error_type=error_type,
            message=message,
        ))
    
    return details


def validate_entity_payload_with_pydantic(
    payload: Dict[str, Any]
) -> EntityValidationResult:
    """
    Validate an entity payload using Pydantic schemas.
    
    Resolves the appropriate schema version from the payload and validates
    against the corresponding Pydantic model.
    
    Args:
        payload: The entity extraction payload to validate.
    
    Returns:
        EntityValidationResult with validation status and any errors.
    
    Note:
        - If any entity fails validation, the entire payload is considered invalid.
        - Error messages are PHI-safe (no raw document text or entity values).
    """
    schema_version = payload.get("schema_version")
    
    if not schema_version:
        return EntityValidationResult(
            is_valid=False,
            error_message="Missing required field 'schema_version'",
            error_details=[
                ValidationErrorDetail(
                    field_path="schema_version",
                    error_type="missing",
                    message="schema_version is required",
                )
            ],
        )
    
    try:
        schema_class = get_entity_schema(schema_version)
    except UnsupportedSchemaVersionError as e:
        return EntityValidationResult(
            is_valid=False,
            error_message=str(e),
            error_details=[
                ValidationErrorDetail(
                    field_path="schema_version",
                    error_type="unsupported_version",
                    message=str(e),
                )
            ],
        )
    
    try:
        validated_model = schema_class.model_validate(payload)
        return EntityValidationResult(
            is_valid=True,
            validated_payload=validated_model.model_dump(),
        )
    except ValidationError as e:
        error_details = normalize_validation_errors(e)
        error_count = len(error_details)
        
        error_message = (
            f"Entity payload validation failed with {error_count} error(s)"
        )
        
        return EntityValidationResult(
            is_valid=False,
            error_message=error_message,
            error_details=error_details,
        )


def validate_or_raise(payload: Dict[str, Any]) -> Dict[str, Any]:
    """
    Validate entity payload and raise EntityValidationError on failure.
    
    Args:
        payload: The entity extraction payload to validate.
    
    Returns:
        The validated payload dictionary.
    
    Raises:
        EntityValidationError: If validation fails.
    """
    result = validate_entity_payload_with_pydantic(payload)
    
    if not result.is_valid:
        raise EntityValidationError(
            message=result.error_message or "Validation failed",
            error_details=result.error_details,
        )
    
    return result.validated_payload
