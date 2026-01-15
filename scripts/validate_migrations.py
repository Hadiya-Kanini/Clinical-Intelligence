"""
Migration notes validator for contract changes.
Validates that migration notes exist and follow the required template structure.
"""
import os
import re
from typing import Dict, List, Tuple


REQUIRED_SECTIONS = [
    "Version",
    "Date",
    "Type",
    "Changes",
    "Impact",
    "Migration Steps"
]


def validate_migration_note_structure(migration_path: str) -> Tuple[bool, List[str]]:
    """
    Validate that a migration note follows the required template structure.
    
    Args:
        migration_path: Path to migration note file
        
    Returns:
        Tuple of (is_valid, error_messages)
    """
    errors = []
    
    if not os.path.exists(migration_path):
        errors.append(f"Migration note file not found: {migration_path}")
        return False, errors
    
    try:
        with open(migration_path, 'r', encoding='utf-8') as f:
            content = f.read()
    except Exception as e:
        errors.append(f"Error reading migration note: {e}")
        return False, errors
    
    if not content.strip():
        errors.append("Migration note file is empty")
        return False, errors
    
    # Check for required sections
    missing_sections = []
    for section in REQUIRED_SECTIONS:
        # Look for section headers in various formats
        patterns = [
            rf'^#+ {re.escape(section)}',  # Markdown header
            rf'^{re.escape(section)}:',     # Colon format
            rf'^## {re.escape(section)}'    # Level 2 header
        ]
        
        found = False
        for pattern in patterns:
            if re.search(pattern, content, re.MULTILINE | re.IGNORECASE):
                found = True
                break
        
        if not found:
            missing_sections.append(section)
    
    if missing_sections:
        errors.append(f"Missing required sections: {', '.join(missing_sections)}")
    
    return len(errors) == 0, errors


def validate_migration_directory(migrations_dir: str) -> Tuple[bool, Dict[str, List[str]]]:
    """
    Validate all migration notes in the migrations directory.
    
    Args:
        migrations_dir: Path to migrations directory
        
    Returns:
        Tuple of (all_valid, errors_by_file)
    """
    errors_by_file = {}
    all_valid = True
    
    if not os.path.exists(migrations_dir):
        errors_by_file['migrations/'] = ["Migrations directory not found"]
        return False, errors_by_file
    
    # Check for README.md
    readme_path = os.path.join(migrations_dir, "README.md")
    if not os.path.exists(readme_path):
        errors_by_file['README.md'] = ["README.md required in migrations directory"]
        all_valid = False
    
    # Find all migration note files (excluding README and .gitkeep)
    migration_files = []
    for filename in os.listdir(migrations_dir):
        if filename.endswith('.md') and filename != 'README.md':
            migration_files.append(filename)
    
    if not migration_files:
        errors_by_file['migrations/'] = ["No migration notes found"]
        all_valid = False
        return all_valid, errors_by_file
    
    # Validate each migration note
    for filename in migration_files:
        filepath = os.path.join(migrations_dir, filename)
        is_valid, errors = validate_migration_note_structure(filepath)
        if not is_valid:
            errors_by_file[filename] = errors
            all_valid = False
    
    return all_valid, errors_by_file


def validate_all_migrations(repo_root: str) -> Tuple[bool, Dict[str, List[str]]]:
    """
    Validate all migration notes in the repository.
    
    Args:
        repo_root: Root directory of the repository
        
    Returns:
        Tuple of (all_valid, errors_by_file)
    """
    migrations_dir = os.path.join(repo_root, "contracts", "migrations")
    return validate_migration_directory(migrations_dir)


def main():
    """Main entry point for migration notes validation."""
    import sys
    
    repo_root = os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))
    all_valid, errors_by_file = validate_all_migrations(repo_root)
    
    if not all_valid:
        print("Migration notes validation FAILED:")
        for filename, errors in errors_by_file.items():
            print(f"\n{filename}:")
            for error in errors:
                print(f"  - {error}")
        sys.exit(1)
    else:
        print("Migration notes validation PASSED")
        sys.exit(0)


if __name__ == "__main__":
    main()
