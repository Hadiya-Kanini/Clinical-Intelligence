-- Create a test user with known password
SELECT 'Creating test user:' as info;

-- Delete existing test user if exists
DELETE FROM users WHERE "Email" = 'test@clinical.com';

-- Insert new test user with password: Test@123456
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
) VALUES 
    (
        gen_random_uuid(),
        'test@clinical.com',
        '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6ukx.LFvO.', -- password: Test@123456 (same hash)
        'Test User',
        'Admin',
        'Active',
        0,
        NULL,
        true,
        false,
        NULL,
        NOW(),
        NOW()
    );

-- Verify the user was created
SELECT 'Created test user:' as info;
SELECT "Id", "Email", "Name", "Role", "IsStaticAdmin" 
FROM users 
WHERE "Email" = 'test@clinical.com';
