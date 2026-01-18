"""
Gemini client wrapper for entity extraction.

Provides a single-call extraction interface with bounded retries and safe logging.
"""

import logging
import time
from typing import Optional

try:
    import google.generativeai as genai
    from google.api_core import exceptions as google_exceptions
except ImportError as e:
    raise ImportError(
        "Missing dependency 'google-generativeai'. "
        "Install with: pip install google-generativeai"
    ) from e

from tenacity import (
    retry,
    stop_after_attempt,
    wait_exponential,
    retry_if_exception_type,
)


logger = logging.getLogger(__name__)


class GeminiClientError(Exception):
    """Base exception for Gemini client errors."""
    pass


class GeminiRateLimitError(GeminiClientError):
    """Raised when rate limit (429) is encountered."""
    pass


class GeminiTimeoutError(GeminiClientError):
    """Raised when request times out."""
    pass


class GeminiClient:
    """
    Wrapper for Gemini API calls with retry logic and safe error handling.
    
    Ensures exactly one API call per extraction invocation (before retries).
    """

    def __init__(
        self,
        api_key: str,
        model: str = "gemini-2.5-flash",
        timeout: int = 60,
        max_retries: int = 3
    ):
        """
        Initialize the Gemini client.
        
        Args:
            api_key: Gemini API key.
            model: Model name to use for generation.
            timeout: Request timeout in seconds.
            max_retries: Maximum retry attempts for transient errors.
        
        Raises:
            ValueError: If api_key is empty.
        """
        if not api_key or not api_key.strip():
            raise ValueError("GEMINI_API_KEY is required")
        
        self._api_key = api_key
        self._model_name = model
        self._timeout = timeout
        self._max_retries = max_retries
        
        genai.configure(api_key=api_key)
        self._model = genai.GenerativeModel(model)
        
        logger.info(
            "GeminiClient initialized with model=%s, timeout=%d, max_retries=%d",
            model, timeout, max_retries
        )

    def generate_content(
        self,
        prompt: str,
        system_instruction: Optional[str] = None
    ) -> str:
        """
        Generate content from a prompt using Gemini.
        
        This method makes exactly one API call (plus retries for transient errors).
        
        Args:
            prompt: The prompt to send to Gemini.
            system_instruction: Optional system instruction for the model.
        
        Returns:
            The generated text response.
        
        Raises:
            GeminiRateLimitError: If rate limit is exceeded after retries.
            GeminiTimeoutError: If request times out after retries.
            GeminiClientError: For other API errors.
        """
        if not prompt:
            raise ValueError("Prompt is required")
        
        return self._generate_with_retry(prompt, system_instruction)

    def _generate_with_retry(
        self,
        prompt: str,
        system_instruction: Optional[str]
    ) -> str:
        """
        Internal method that handles retry logic.
        """
        last_exception = None
        
        for attempt in range(1, self._max_retries + 1):
            try:
                logger.debug(
                    "Gemini API call attempt %d/%d",
                    attempt, self._max_retries
                )
                
                generation_config = genai.types.GenerationConfig(
                    temperature=0.1,
                    top_p=0.95,
                    max_output_tokens=16384,
                )
                
                if system_instruction:
                    model = genai.GenerativeModel(
                        self._model_name,
                        system_instruction=system_instruction
                    )
                else:
                    model = self._model
                
                response = model.generate_content(
                    prompt,
                    generation_config=generation_config,
                    request_options={"timeout": self._timeout}
                )
                
                if response.text:
                    logger.debug("Gemini API call successful")
                    return response.text
                else:
                    raise GeminiClientError("Empty response from Gemini API")
                    
            except google_exceptions.ResourceExhausted as e:
                last_exception = GeminiRateLimitError(
                    f"Rate limit exceeded (attempt {attempt})"
                )
                logger.warning(
                    "Rate limit hit on attempt %d/%d, waiting before retry",
                    attempt, self._max_retries
                )
                if attempt < self._max_retries:
                    # Exponential backoff with longer base wait for rate limits
                    wait_time = min(2 ** attempt + 10, 60)  # Start at 12s, max 60s
                    logger.info(f"Waiting {wait_time}s before retry...")
                    time.sleep(wait_time)
                    
            except google_exceptions.DeadlineExceeded as e:
                last_exception = GeminiTimeoutError(
                    f"Request timed out (attempt {attempt})"
                )
                logger.warning(
                    "Timeout on attempt %d/%d",
                    attempt, self._max_retries
                )
                if attempt < self._max_retries:
                    time.sleep(1)
                    
            except google_exceptions.GoogleAPIError as e:
                error_msg = str(e)[:200]
                last_exception = GeminiClientError(
                    f"API error: {error_msg}"
                )
                logger.error(
                    "Gemini API error on attempt %d: %s",
                    attempt, error_msg
                )
                if attempt < self._max_retries:
                    time.sleep(1)
                    
            except Exception as e:
                error_msg = str(e)[:200]
                last_exception = GeminiClientError(
                    f"Unexpected error: {error_msg}"
                )
                logger.error(
                    "Unexpected error on attempt %d: %s",
                    attempt, error_msg
                )
                break
        
        logger.error(
            "All %d retry attempts exhausted",
            self._max_retries
        )
        raise last_exception

    @property
    def model_name(self) -> str:
        """Get the model name."""
        return self._model_name

    @property
    def max_retries(self) -> int:
        """Get the maximum retry count."""
        return self._max_retries
