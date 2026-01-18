-- Delete existing test user if exists
DELETE FROM users WHERE "Email" = 'test@example.com';

-- Create new test user with known password
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
    gen_random_uuid(),
    'test@example.com',
    '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6ukx.LFvO6', -- password: Test123456
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
