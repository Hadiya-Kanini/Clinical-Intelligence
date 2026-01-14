# Backend API Contract v1

This directory contains the OpenAPI v1 specification for the Backend API.

## Consumption

The Web UI and other clients **must** only consume versioned endpoints documented in the `openapi.yaml` file. The Backend API is configured to generate and expose this contract via a Swagger UI endpoint.

## Versioning

Changes to this contract must follow semantic versioning principles. Any breaking changes require a new version of the API contract. Non-breaking changes can be applied to the current version. All changes must be documented in the corresponding migration notes.
