import bcrypt
import uuid

# Generate a proper bcrypt hash for Test123456
password = "Test123456"
print(f"Generating hash for password: {password}")

# Generate hash with 12 rounds (matching the backend)
salt = bcrypt.gensalt(rounds=12)
hash_bytes = bcrypt.hashpw(password.encode('utf-8'), salt)
hash_string = hash_bytes.decode('utf-8')

print(f"Generated hash: {hash_string}")

# Verify it works
verify_result = bcrypt.checkpw(password.encode('utf-8'), hash_bytes)
print(f"Verification result: {verify_result}")

# Generate SQL for creating user
user_id = str(uuid.uuid4())
print(f"\nSQL to create user:")
print(f"""
-- Delete existing test user
DELETE FROM users WHERE "Email" = 'test@example.com';

-- Insert new test user
INSERT INTO users (
    "Id",
    "Email",
    "PasswordHash",
    "Name",
    "Role",
    "Status",
    "FailedLoginAttempts",
    "LockedUntil",
    "IsStaticAdmin",
    "IsDeleted",
    "DeletedAt",
    "CreatedAt",
    "UpdatedAt"
)
VALUES
(
    '{user_id}',
    'test@example.com',
    '{hash_string}',
    'Test User',
    'Admin',
    'Active',
    0,
    NULL,
    false,
    false,
    NULL,
    NOW(),
    NOW()
);
""")
