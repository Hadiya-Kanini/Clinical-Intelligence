"""Entity extraction module for single-call Gemini extraction."""

from .models import ChunkWithProvenance, ExtractionInput
from .prompt_builder import build_entity_extraction_prompt
from .gemini_client import GeminiClient
from .extractor import extract_entities_single_call
from .response_parser import (
    parse_entity_extraction_response,
    EntityExtractionError,
)

__all__ = [
    "ChunkWithProvenance",
    "ExtractionInput",
    "build_entity_extraction_prompt",
    "GeminiClient",
    "extract_entities_single_call",
    "parse_entity_extraction_response",
    "EntityExtractionError",
]
