"""
Entity extraction orchestration.

Coordinates prompt building and Gemini API calls for single-call extraction.
"""

import json
import logging
import os
from datetime import datetime
from typing import List, Optional

from .models import ChunkWithProvenance, ExtractionInput
from .prompt_builder import (
    build_entity_extraction_prompt,
    get_system_instruction,
)
from .gemini_client import GeminiClient


logger = logging.getLogger(__name__)


def extract_entities_single_call(
    extraction_input: ExtractionInput,
    gemini_client: GeminiClient
) -> str:
    """
    Perform single-call entity extraction using Gemini.
    
    This function orchestrates:
    1. Building the extraction prompt from chunks
    2. Making exactly one Gemini API call
    3. Returning the raw response for parsing/validation
    
    Args:
        extraction_input: Input containing document_id and chunks with provenance.
        gemini_client: Configured Gemini client instance.
    
    Returns:
        Raw response text from Gemini (to be parsed/validated by caller).
    
    Raises:
        ValueError: If extraction_input is invalid.
        GeminiClientError: If API call fails after retries.
    """
    if not extraction_input:
        raise ValueError("extraction_input is required")
    if not extraction_input.document_id:
        raise ValueError("document_id is required")
    if not extraction_input.chunks:
        raise ValueError("At least one chunk is required for extraction")
    if not gemini_client:
        raise ValueError("gemini_client is required")
    
    logger.info(
        "Starting entity extraction for document %s with %d chunks",
        extraction_input.document_id,
        len(extraction_input.chunks)
    )
    
    prompt = build_entity_extraction_prompt(
        document_id=extraction_input.document_id,
        chunks=extraction_input.chunks,
        patient_id=extraction_input.patient_id
    )
    
    system_instruction = get_system_instruction()
    
    logger.debug(
        "Built extraction prompt (%d chars) for document %s",
        len(prompt),
        extraction_input.document_id
    )
    
    response = gemini_client.generate_content(
        prompt=prompt,
        system_instruction=system_instruction
    )
    
    # Store Gemini output to JSON file for debugging/auditing
    store_gemini_output(extraction_input.document_id, response, prompt, system_instruction)
    
    logger.info(
        "Entity extraction completed for document %s, response length: %d",
        extraction_input.document_id,
        len(response)
    )
    
    return response


def create_extraction_input(
    document_id: str,
    chunks: List[ChunkWithProvenance],
    patient_id: Optional[str] = None
) -> ExtractionInput:
    """
    Create an ExtractionInput from components.
    
    Args:
        document_id: The document ID being processed.
        chunks: List of chunks with provenance.
        patient_id: Optional patient ID.
    
    Returns:
        Configured ExtractionInput instance.
    """
    return ExtractionInput(
        document_id=document_id,
        chunks=chunks,
        patient_id=patient_id
    )


def store_gemini_output(
    document_id: str,
    response: str,
    prompt: str,
    system_instruction: str,
    output_dir: Optional[str] = None
) -> None:
    """
    Store Gemini output to JSON file for debugging and auditing.
    
    Args:
        document_id: The document ID being processed.
        response: The raw response from Gemini.
        prompt: The prompt sent to Gemini.
        system_instruction: The system instruction sent to Gemini.
        output_dir: Optional output directory (defaults to 'gemini_outputs').
    """
    try:
        # Create output directory if not specified
        if output_dir is None:
            # Get worker root directory
            worker_dir = os.path.dirname(os.path.dirname(__file__))
            output_dir = os.path.join(worker_dir, "gemini_outputs")
        
        os.makedirs(output_dir, exist_ok=True)
        
        # Generate filename with timestamp
        timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        safe_document_id = document_id.replace("/", "_").replace("\\", "_")[:50]
        filename = f"gemini_output_{safe_document_id}_{timestamp}.json"
        filepath = os.path.join(output_dir, filename)
        
        # Prepare output data
        output_data = {
            "metadata": {
                "document_id": document_id,
                "timestamp": datetime.now().isoformat(),
                "response_length": len(response),
                "prompt_length": len(prompt),
                "system_instruction_length": len(system_instruction)
            },
            "input": {
                "system_instruction": system_instruction,
                "prompt": prompt
            },
            "output": {
                "raw_response": response
            }
        }
        
        # Write to file
        with open(filepath, 'w', encoding='utf-8') as f:
            json.dump(output_data, f, indent=2, ensure_ascii=False)
        
        logger.info(
            "Gemini output stored to %s (size: %d bytes)",
            filepath,
            os.path.getsize(filepath)
        )
        
        # Keep only last 50 files to prevent disk space issues
        try:
            files = sorted(
                [f for f in os.listdir(output_dir) if f.startswith("gemini_output_")],
                key=lambda x: os.path.getmtime(os.path.join(output_dir, x)),
                reverse=True
            )
            
            if len(files) > 50:
                for old_file in files[50:]:
                    old_filepath = os.path.join(output_dir, old_file)
                    os.remove(old_filepath)
                    logger.debug("Removed old Gemini output file: %s", old_filepath)
                    
        except Exception as cleanup_error:
            logger.warning("Failed to cleanup old Gemini output files: %s", cleanup_error)
            
    except Exception as e:
        logger.error("Failed to store Gemini output: %s", e)
        # Don't raise - this is a non-critical operation
