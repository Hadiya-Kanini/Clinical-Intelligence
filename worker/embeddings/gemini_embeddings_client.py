"""Minimal client wrapper for calling the Gemini Embeddings API."""

from typing import List, Optional
import google.generativeai as genai


class GeminiEmbeddingsClient:
    """
    Wrapper for the Gemini Embeddings API.
    
    Provides a testable interface for generating embeddings with configurable
    model and output dimensionality.
    """
    
    def __init__(
        self,
        api_key: str,
        model: str = "text-embedding-004",
        output_dimensions: int = 768
    ):
        """
        Initialize the Gemini embeddings client.
        
        Args:
            api_key: Gemini API key.
            model: Embedding model name (default text-embedding-004).
            output_dimensions: Output vector dimensionality (default 768).
        """
        self._model = model
        self._output_dimensions = output_dimensions
        
        genai.configure(api_key=api_key)
    
    @property
    def model(self) -> str:
        """Get the configured model name."""
        return self._model
    
    @property
    def output_dimensions(self) -> int:
        """Get the configured output dimensions."""
        return self._output_dimensions
    
    def embed_content(self, text: str) -> List[float]:
        """
        Generate embedding for a single text.
        
        Args:
            text: Text to embed.
        
        Returns:
            List of floats representing the embedding vector.
        
        Raises:
            Exception: If the API call fails.
        """
        result = genai.embed_content(
            model=f"models/{self._model}",
            content=text,
            output_dimensionality=self._output_dimensions
        )
        
        return result["embedding"]
    
    def embed_batch(self, texts: List[str]) -> List[List[float]]:
        """
        Generate embeddings for multiple texts.
        
        Note: This calls the API once per text. For true batching,
        use the batch API if available.
        
        Args:
            texts: List of texts to embed.
        
        Returns:
            List of embedding vectors.
        """
        embeddings = []
        for text in texts:
            embedding = self.embed_content(text)
            embeddings.append(embedding)
        return embeddings
