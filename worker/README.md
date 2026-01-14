# AI Worker (Python)

This directory contains the Python-based AI worker. It performs intensive computational tasks and is invoked by the Backend API.

## Configuration

The worker reads required secrets from environment variables. For local development it also supports loading a gitignored root `.env` file.

Required environment variables:

- `GEMINI_API_KEY`

Example:
```
GEMINI_API_KEY=AIzaSyAaZ-AOsa8h0kZPfNv7whqMBumzQ8hlPY4
```

## Secret rotation

Secrets are loaded at startup. To rotate a secret:

1. Update the value in the configuration source (e.g., environment variable or local `.env`).
2. Restart the worker process.
