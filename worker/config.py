import os
from dataclasses import dataclass
from typing import Optional


DEFAULT_EMBEDDING_MODEL = "text-embedding-004"
DEFAULT_EMBEDDING_DIMENSIONS = 768
DEFAULT_RPM_LIMIT = 15
DEFAULT_MAX_RETRIES = 3
DEFAULT_EXTRACTION_MODEL = "gemini-2.5-flash"
DEFAULT_EXTRACTION_TIMEOUT = 60
DEFAULT_EXTRACTION_MAX_RETRIES = 3
DEFAULT_ENTITY_CATEGORIES_PATH = "contracts/entities/v1/entity_categories.json"


@dataclass(frozen=True)
class WorkerConfig:
    gemini_api_key: str
    embedding_model: str = DEFAULT_EMBEDDING_MODEL
    embedding_dimensions: int = DEFAULT_EMBEDDING_DIMENSIONS
    rpm_limit: int = DEFAULT_RPM_LIMIT
    max_retries: int = DEFAULT_MAX_RETRIES
    database_connection_string: Optional[str] = None
    extraction_model: str = DEFAULT_EXTRACTION_MODEL
    extraction_timeout: int = DEFAULT_EXTRACTION_TIMEOUT
    extraction_max_retries: int = DEFAULT_EXTRACTION_MAX_RETRIES
    entity_categories_path: Optional[str] = None
    worker_api_key: str = "worker-secret-key-2024"


def _repo_root() -> str:
    return os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))


def _try_load_dotenv() -> None:
    # Skip dotenv loading if explicitly disabled (e.g., during testing)
    if os.getenv("SKIP_DOTENV_LOADING"):
        return
        
    try:
        from dotenv import load_dotenv
    except ModuleNotFoundError:
        return

    dotenv_path = os.path.join(_repo_root(), ".env")
    if os.path.isfile(dotenv_path):
        load_dotenv(dotenv_path=dotenv_path, override=False)


def load_config() -> WorkerConfig:
    _try_load_dotenv()

    gemini_api_key = os.getenv("GEMINI_API_KEY")
    if not gemini_api_key or not gemini_api_key.strip():
        raise RuntimeError("Missing required configuration value 'GEMINI_API_KEY'.")

    embedding_model = os.getenv("GEMINI_EMBEDDING_MODEL", DEFAULT_EMBEDDING_MODEL)
    
    embedding_dimensions_str = os.getenv("GEMINI_EMBEDDING_OUTPUT_DIMENSIONS", str(DEFAULT_EMBEDDING_DIMENSIONS))
    try:
        embedding_dimensions = int(embedding_dimensions_str)
    except ValueError:
        embedding_dimensions = DEFAULT_EMBEDDING_DIMENSIONS
    
    rpm_limit_str = os.getenv("GEMINI_RPM_LIMIT", str(DEFAULT_RPM_LIMIT))
    try:
        rpm_limit = int(rpm_limit_str)
    except ValueError:
        rpm_limit = DEFAULT_RPM_LIMIT
    
    max_retries_str = os.getenv("GEMINI_MAX_RETRIES", str(DEFAULT_MAX_RETRIES))
    try:
        max_retries = int(max_retries_str)
    except ValueError:
        max_retries = DEFAULT_MAX_RETRIES

    # Always use DATABASE_URL (Python psycopg2 format) for Python connections
    # DATABASE_CONNECTION_STRING is in .NET format and not compatible with psycopg2
    database_connection_string = os.getenv("DATABASE_URL")
    if not database_connection_string:
        # Provide a default for local development
        database_connection_string = "postgresql://postgres:admin@localhost:5432/ClinicalIntelligence"
        import logging
        logging.warning(
            "DATABASE_URL not set. Using default: %s. "
            "For production, set DATABASE_URL in PostgreSQL URI format: postgresql://user:password@host:port/database",
            database_connection_string
        )
    
    extraction_model = os.getenv("GEMINI_EXTRACTION_MODEL", DEFAULT_EXTRACTION_MODEL)
    
    extraction_timeout_str = os.getenv("GEMINI_EXTRACTION_TIMEOUT", str(DEFAULT_EXTRACTION_TIMEOUT))
    try:
        extraction_timeout = int(extraction_timeout_str)
    except ValueError:
        extraction_timeout = DEFAULT_EXTRACTION_TIMEOUT
    
    extraction_max_retries_str = os.getenv("GEMINI_EXTRACTION_MAX_RETRIES", str(DEFAULT_EXTRACTION_MAX_RETRIES))
    try:
        extraction_max_retries = int(extraction_max_retries_str)
    except ValueError:
        extraction_max_retries = DEFAULT_EXTRACTION_MAX_RETRIES

    entity_categories_path = os.getenv("ENTITY_CATEGORIES_PATH")
    
    # Worker API key for backend authentication
    worker_api_key = os.getenv("WORKER_API_KEY", "worker-secret-key-2024")

    return WorkerConfig(
        gemini_api_key=gemini_api_key,
        embedding_model=embedding_model,
        embedding_dimensions=embedding_dimensions,
        rpm_limit=rpm_limit,
        max_retries=max_retries,
        database_connection_string=database_connection_string,
        extraction_model=extraction_model,
        extraction_timeout=extraction_timeout,
        extraction_max_retries=extraction_max_retries,
        entity_categories_path=entity_categories_path,
        worker_api_key=worker_api_key,
    )
