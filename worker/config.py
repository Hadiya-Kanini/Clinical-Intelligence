import os
from dataclasses import dataclass


@dataclass(frozen=True)
class WorkerConfig:
    gemini_api_key: str


def _repo_root() -> str:
    return os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))


def _try_load_dotenv() -> None:
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

    return WorkerConfig(gemini_api_key=gemini_api_key)
