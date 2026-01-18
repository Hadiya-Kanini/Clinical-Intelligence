"""
Entity extraction response parser and validator.

Parses Gemini raw output into structured entity results and validates
against the entity contract schema.
"""

import json
import logging
import re
from typing import Dict, Any, Optional

from .normalization import normalize_payload
from .category_normalization import normalize_and_validate_categories

logger = logging.getLogger(__name__)


class EntityExtractionError(Exception):
    """Base exception for entity extraction errors."""
    
    def __init__(self, message: str, details: Optional[str] = None):
        super().__init__(message)
        self.details = details


class MalformedResponseError(EntityExtractionError):
    """Raised when the LLM response is not valid JSON."""
    pass


class SchemaValidationError(EntityExtractionError):
    """Raised when the parsed response fails schema validation."""
    pass


def parse_entity_extraction_response(raw_text: str) -> Dict[str, Any]:
    """
    Parse and extract JSON from Gemini entity extraction response.
    
    Handles common LLM output patterns:
    - Pure JSON response
    - JSON wrapped in markdown code blocks
    - JSON with leading/trailing text
    
    Args:
        raw_text: Raw response text from Gemini.
    
    Returns:
        Parsed JSON as a dictionary.
    
    Raises:
        MalformedResponseError: If no valid JSON can be extracted.
    """
    if not raw_text or not raw_text.strip():
        raise MalformedResponseError(
            "Empty response from LLM",
            details="Response text was empty or whitespace only"
        )
    
    text = raw_text.strip()
    
    json_match = re.search(r'```(?:json)?\s*([\s\S]*?)\s*```', text)
    if json_match:
        text = json_match.group(1).strip()
    
    json_obj = _extract_json_object(text)
    if json_obj is not None:
        return json_obj
    
    # If all parsing fails, raise an error
    raise MalformedResponseError(
        "Could not extract valid JSON from response",
        details=f"Raw response: {raw_text[:200]}..."
    )


def _extract_json_object(text: str) -> Optional[Dict[str, Any]]:
    """
    Extract JSON object from text using multiple strategies.
    
    Tries multiple strategies:
    1. Direct parse of entire text
    2. Find first { and last } and parse between
    3. Clean common JSON formatting issues
    4. Handle truncated responses by attempting to fix incomplete JSON
    """
    if not text or not text.strip():
        return None
    
    logger.debug(f"Attempting to parse JSON from: {text[:200]}...")
    
    # Strategy 1: Direct parse
    try:
        result = json.loads(text)
        logger.debug("Strategy 1 (direct parse) succeeded")
        return result
    except json.JSONDecodeError as e:
        logger.debug(f"Strategy 1 failed: {e}")
        pass
    
    # Strategy 2: Extract JSON between first { and last }
    start_idx = text.find('{')
    end_idx = text.rfind('}')
    
    if start_idx != -1 and end_idx != -1 and end_idx > start_idx:
        try:
            json_text = text[start_idx:end_idx + 1]
            logger.debug(f"Strategy 2 attempting: {json_text[:200]}...")
            result = json.loads(json_text)
            logger.debug("Strategy 2 succeeded")
            return result
        except json.JSONDecodeError as e:
            logger.debug(f"Strategy 2 failed: {e}")
            pass
    
    # Strategy 3: Clean common issues and try again
    if start_idx != -1 and end_idx != -1 and end_idx > start_idx:
        try:
            json_text = text[start_idx:end_idx + 1]
            # Remove common formatting issues
            json_text = json_text.replace('\n', ' ').replace('\r', ' ')
            # Remove extra spaces
            while '  ' in json_text:
                json_text = json_text.replace('  ', ' ')
            logger.debug(f"Strategy 3 attempting: {json_text[:200]}...")
            result = json.loads(json_text)
            logger.debug("Strategy 3 succeeded")
            return result
        except json.JSONDecodeError as e:
            logger.debug(f"Strategy 3 failed: {e}")
            pass
    
    # Strategy 4: Handle truncated JSON by attempting to fix it
    if start_idx != -1:
        try:
            json_text = text[start_idx:]
            logger.debug(f"Strategy 4 attempting to fix truncated JSON: {json_text[:200]}...")
            
            # Try to fix common truncation issues
            lines = json_text.split('\n')
            fixed_lines = []
            
            for i, line in enumerate(lines):
                line = line.rstrip()  # Remove trailing whitespace but preserve structure
                if not line:
                    continue
                
                # Track if we're inside a string to handle incomplete strings
                escape_next = False
                in_string = False
                string_char = None
                
                for j, char in enumerate(line):
                    if escape_next:
                        escape_next = False
                        continue
                    
                    if char == '\\':
                        escape_next = True
                        continue
                    
                    if char in ('"', "'") and not escape_next:
                        if not in_string:
                            in_string = True
                            string_char = char
                        elif char == string_char:
                            in_string = False
                            string_char = None
                
                # If line ends abruptly while in a string, try to close it
                if in_string and not line.endswith(string_char):
                    line += string_char  # Close the string
                    in_string = False
                    string_char = None
                
                # If line ends with a comma but the structure seems incomplete, remove the comma
                if line.endswith(',') and i == len(lines) - 1:  # Last line
                    line = line[:-1]  # Remove trailing comma from last element
                
                fixed_lines.append(line)
            
            fixed_text = '\n'.join(fixed_lines)
            
            # Try to find balanced braces and fix incomplete structures
            open_braces = 0
            open_brackets = 0
            last_complete_pos = -1
            in_string = False
            escape_next = False
            string_char = None
            
            for i, char in enumerate(fixed_text):
                if escape_next:
                    escape_next = False
                    continue
                
                if char == '\\':
                    escape_next = True
                    continue
                
                if char in ('"', "'") and not escape_next:
                    if not in_string:
                        in_string = True
                        string_char = char
                    elif char == string_char:
                        in_string = False
                        string_char = None
                elif not in_string:
                    if char == '{':
                        open_braces += 1
                    elif char == '}':
                        open_braces -= 1
                    elif char == '[':
                        open_brackets += 1
                    elif char == ']':
                        open_brackets -= 1
                    
                    # When all braces and brackets are balanced, mark this position
                    if open_braces == 0 and open_brackets == 0:
                        last_complete_pos = i
            
            logger.debug(f"Brace balancing: open_braces={open_braces}, open_brackets={open_brackets}, last_complete_pos={last_complete_pos}")
            
            if last_complete_pos != -1:
                fixed_text = fixed_text[:last_complete_pos + 1]
                # Ensure the JSON ends properly
                if not fixed_text.endswith('}'):
                    fixed_text += '}'
                
                logger.debug(f"Strategy 4 attempting fixed JSON: {fixed_text[:200]}...")
                result = json.loads(fixed_text)
                logger.debug("Strategy 4 succeeded")
                return result
            else:
                logger.debug("Strategy 4: No balanced structure found, trying to close incomplete structures")
                # Try to manually close the incomplete structure
                # If we're in an incomplete string, close it
                if in_string:
                    fixed_text += string_char
                    in_string = False
                    string_char = None
                
                # Close remaining braces and brackets
                while open_braces > 0:
                    fixed_text += '}'
                    open_braces -= 1
                
                while open_brackets > 0:
                    fixed_text += ']'
                    open_brackets -= 1
                
                logger.debug(f"Strategy 4 attempting manually closed JSON: {fixed_text[:200]}...")
                try:
                    result = json.loads(fixed_text)
                    logger.debug("Strategy 4 with manual closing succeeded")
                    return result
                except json.JSONDecodeError as manual_e:
                    logger.debug(f"Strategy 4 manual closing failed: {manual_e}")
                
        except Exception as e:
            logger.debug(f"Strategy 4 failed: {e}")
        
        # Always try fallback strategies regardless of Strategy 4 outcome
        logger.debug("Strategy 4 completed, trying fallback strategies")
        
        # Strategy 4b: Find complete entity objects using regex
        try:
            json_text = text[start_idx:]
            # Find the last complete entity object
            entity_pattern = r'\{[^{}]*"entity_group_name"[^{}]*\}'
            entities = re.findall(entity_pattern, json_text, re.DOTALL)
            
            if entities:
                logger.debug(f"Strategy 4b found {len(entities)} complete entities")
                # Construct a minimal valid response with the entities we could recover
                partial_json = f'''{{
                    "schema_version": "1.0",
                    "document_id": "",
                    "extracted_entities": [{",".join(entities)}]
                }}'''
                result = json.loads(partial_json)
                logger.debug("Strategy 4b succeeded")
                return result
            else:
                logger.debug("Strategy 4b found no complete entities")
        except Exception as fallback_e:
            logger.debug(f"Strategy 4b also failed: {fallback_e}")
        
        # Strategy 4c: Manual entity recovery
        try:
            json_text = text[start_idx:]
            # Find all complete entity objects by looking for the pattern
            # that starts with "entity_group_name" and ends with a complete }
            entity_starts = []
            for match in re.finditer(r'"entity_group_name"', json_text):
                # Find the start of this entity object
                start_pos = match.start()
                # Go backwards to find the opening {
                while start_pos > 0 and json_text[start_pos] != '{':
                    start_pos -= 1
                if json_text[start_pos] == '{':
                    entity_starts.append(start_pos)
            
            recovered_entities = []
            for entity_start in entity_starts:
                # Try to find the end of this entity
                brace_count = 0
                in_string = False
                escape_next = False
                string_char = None
                
                for pos in range(entity_start, len(json_text)):
                    char = json_text[pos]
                    
                    if escape_next:
                        escape_next = False
                        continue
                    
                    if char == '\\':
                        escape_next = True
                        continue
                    
                    if char in ('"', "'") and not escape_next:
                        if not in_string:
                            in_string = True
                            string_char = char
                        elif char == string_char:
                            in_string = False
                            string_char = None
                    elif not in_string:
                        if char == '{':
                            brace_count += 1
                        elif char == '}':
                            brace_count -= 1
                            if brace_count == 0:
                                # Found complete entity
                                entity_text = json_text[entity_start:pos + 1]
                                try:
                                    entity_obj = json.loads(entity_text)
                                    if 'entity_group_name' in entity_obj:
                                        recovered_entities.append(entity_obj)
                                        break
                                except:
                                    pass
                                break
            
            if recovered_entities:
                logger.debug(f"Strategy 4c recovered {len(recovered_entities)} entities")
                partial_json = {
                    "schema_version": "1.0",
                    "document_id": "",
                    "extracted_entities": recovered_entities
                }
                logger.debug("Strategy 4c succeeded")
                return partial_json
            else:
                logger.debug("Strategy 4c recovered no entities")
                
        except Exception as final_e:
            logger.debug(f"Strategy 4c also failed: {final_e}")
    
    # If all parsing fails, return None
    return None


def validate_entity_response(
    parsed_response: Dict[str, Any],
    validate_func=None
) -> Dict[str, Any]:
    """
    Validate parsed entity response against the contract schema.
    
    Args:
        parsed_response: Parsed JSON dictionary.
        validate_func: Optional validation function (defaults to main.validate_entity_payload).
    
    Returns:
        The validated response dictionary.
    
    Raises:
        SchemaValidationError: If validation fails.
    """
    if validate_func is None:
        from worker.main import validate_entity_payload
        validate_func = validate_entity_payload
    
    try:
        validate_func(parsed_response)
        return parsed_response
    except ValueError as e:
        error_msg = str(e)[:200]
        raise SchemaValidationError(
            "Entity response failed schema validation",
            details=error_msg
        )


def parse_and_validate_response(
    raw_text: str,
    validate_func=None,
    apply_normalization: bool = True,
    validate_categories: bool = True
) -> Dict[str, Any]:
    """
    Parse and validate entity extraction response in one step.
    
    Args:
        raw_text: Raw response text from Gemini.
        validate_func: Optional validation function.
        apply_normalization: Whether to apply normalization post-parse.
        validate_categories: Whether to validate and normalize categories.
    
    Returns:
        Validated entity response dictionary.
    
    Raises:
        MalformedResponseError: If JSON parsing fails.
        SchemaValidationError: If schema validation fails.
    """
    parsed = parse_entity_extraction_response(raw_text)
    
    if apply_normalization:
        parsed = normalize_payload(parsed, remove_placeholders=True)
    
    if validate_categories:
        cat_result = normalize_and_validate_categories(parsed)
        if cat_result.has_errors:
            raise SchemaValidationError(
                "Entity category validation failed",
                details=cat_result.error_message
            )
        parsed = cat_result.normalized_payload
    
    return validate_entity_response(parsed, validate_func)


def validate_conflicts(parsed_response: Dict[str, Any]) -> bool:
    """
    Validate that conflicts in the response are properly structured.
    
    Args:
        parsed_response: Parsed entity response.
    
    Returns:
        True if all conflicts are valid, False otherwise.
    """
    entities = parsed_response.get("extracted_entities", [])
    
    for entity in entities:
        conflicts = entity.get("conflicts", [])
        if not isinstance(conflicts, list):
            return False
        
        for conflict in conflicts:
            if not isinstance(conflict, dict):
                return False
            if "conflicting_value" not in conflict:
                return False
    
    return True


def extract_entity_count(parsed_response: Dict[str, Any]) -> int:
    """
    Get the count of extracted entities.
    
    Args:
        parsed_response: Parsed entity response.
    
    Returns:
        Number of extracted entities.
    """
    return len(parsed_response.get("extracted_entities", []))


def extract_conflict_count(parsed_response: Dict[str, Any]) -> int:
    """
    Get the total count of conflicts across all entities.
    
    Args:
        parsed_response: Parsed entity response.
    
    Returns:
        Total number of conflicts.
    """
    total = 0
    for entity in parsed_response.get("extracted_entities", []):
        total += len(entity.get("conflicts", []))
    return total
