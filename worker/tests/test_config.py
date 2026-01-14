import os
import unittest
from unittest.mock import patch


class WorkerConfigTests(unittest.TestCase):
    def test_load_config_missing_gemini_api_key_raises(self):
        with patch.dict(os.environ, {}, clear=True):
            from worker import config

            with patch.object(config, "_try_load_dotenv", return_value=None):
                with self.assertRaises(RuntimeError) as ctx:
                    config.load_config()

        self.assertIn("Missing required configuration value 'GEMINI_API_KEY'", str(ctx.exception))

    def test_load_config_whitespace_gemini_api_key_raises(self):
        with patch.dict(os.environ, {"GEMINI_API_KEY": "   "}, clear=True):
            from worker import config

            with patch.object(config, "_try_load_dotenv", return_value=None):
                with self.assertRaises(RuntimeError) as ctx:
                    config.load_config()

        self.assertIn("Missing required configuration value 'GEMINI_API_KEY'", str(ctx.exception))

    def test_load_config_valid_gemini_api_key_returns_config(self):
        with patch.dict(os.environ, {"GEMINI_API_KEY": "test-key"}, clear=True):
            from worker import config

            with patch.object(config, "_try_load_dotenv", return_value=None):
                cfg = config.load_config()

        self.assertEqual("test-key", cfg.gemini_api_key)


if __name__ == "__main__":
    unittest.main()
