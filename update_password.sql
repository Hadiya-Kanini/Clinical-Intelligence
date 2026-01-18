-- Update password hash for test user
UPDATE users 
SET "PasswordHash" = '$2b$12$huZSKI5qImwthfOQearwDOvTsmdNJzkuujnPL.xJgho33VJU7EvOC',
    "FailedLoginAttempts" = 0,
    "LockedUntil" = NULL,
    "UpdatedAt" = NOW()
WHERE "Email" = 'test@example.com';

-- Show user status
SELECT "Email", "Name", "Role", "Status", "FailedLoginAttempts", "LockedUntil" 
FROM users 
WHERE "Email" = 'test@example.com';
