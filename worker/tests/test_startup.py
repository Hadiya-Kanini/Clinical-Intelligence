import os
import subprocess
import sys
import unittest


class WorkerStartupIntegrationTests(unittest.TestCase):
    def test_worker_startup_missing_gemini_api_key_fails_fast(self):
        repo_root = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
        worker_main = os.path.join(repo_root, "worker", "main.py")

        env = dict(os.environ)
        env.pop("GEMINI_API_KEY", None)
        env["PYTHONPATH"] = repo_root

        result = subprocess.run(
            [sys.executable, worker_main],
            cwd=repo_root,
            env=env,
            capture_output=True,
            text=True,
        )

        combined = (result.stdout or "") + "\n" + (result.stderr or "")

        self.assertNotEqual(0, result.returncode)
        self.assertIn("Missing required configuration value 'GEMINI_API_KEY'", combined)


if __name__ == "__main__":
    unittest.main()
