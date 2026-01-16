-- Check if ambersharief811@gmail.com exists and add it if not
SELECT 'Checking for ambersharief811@gmail.com:' as info;
SELECT "Id", "Email", "Name", "Role", "IsStaticAdmin" 
FROM users 
WHERE "Email" = 'ambersharief811@gmail.com';

-- Add ambersharief811@gmail.com if it doesn't exist
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
    '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj6ukx.LFvO.', -- password: Admin@123456
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

-- Verify the user was added
SELECT 'Users after adding ambersharief811@gmail.com:' as info;
SELECT "Id", "Email", "Name", "Role", "IsStaticAdmin" 
FROM users 
ORDER BY "IsStaticAdmin" DESC, "Email";
