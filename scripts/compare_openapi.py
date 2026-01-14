import json
import sys
import yaml


def load_json(path: str) -> dict:
    with open(path, "r", encoding="utf-8") as f:
        return json.load(f)


def load_yaml(path: str) -> dict:
    with open(path, "r", encoding="utf-8") as f:
        return yaml.safe_load(f)


def normalize_openapi(doc: dict) -> dict:
    # Remove runtime-generated fields that can vary between runs
    doc = doc.copy()
    for key in ["servers"]:
        doc.pop(key, None)
    # Ensure consistent ordering for simple comparison
    return doc


def main() -> int:
    if len(sys.argv) != 3:
        print("Usage: compare_openapi.py <generated-swagger.json> <committed-openapi.yaml>")
        return 1

    gen_path, committed_path = sys.argv[1], sys.argv[2]

    generated = normalize_openapi(load_json(gen_path))
    committed = normalize_openapi(load_yaml(committed_path))

    if generated == committed:
        print("OpenAPI artifacts match.")
        return 0
    else:
        print("ERROR: Generated OpenAPI does not match committed OpenAPI.")
        # Show a simple diff hint
        print("Differences detected. Please update contracts/api/v1/openapi.yaml to match the backend-generated Swagger.")
        return 1


if __name__ == "__main__":
    sys.exit(main())
