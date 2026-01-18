import bcrypt
import sys

# Generate a proper bcrypt hash for Admin@123456
password = "Admin@123456"
print(f"Generating hash for password: {password}")

# Generate hash with 12 rounds (matching the backend)
salt = bcrypt.gensalt(rounds=12)
hash_bytes = bcrypt.hashpw(password.encode('utf-8'), salt)
hash_string = hash_bytes.decode('utf-8')

print(f"Generated hash: {hash_string}")

# Verify it works
verify_result = bcrypt.checkpw(password.encode('utf-8'), hash_bytes)
print(f"Verification result: {verify_result}")

# Output SQL for creating user
print("\nSQL to create user:")
print(f"""
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
SELECT 
    gen_random_uuid(),
    'ambersharief811@gmail.com',
    '{hash_string}',
    'Amber Sharief',
    'Admin',
    'Active',
    0,
    NULL,
    false,
    false,
    NULL,
    NOW(),
    NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM users WHERE "Email" = 'ambersharief811@gmail.com'
);
""")
