-- Reset failed login attempts for test user
UPDATE users 
SET "FailedLoginAttempts" = 0, "LockedUntil" = NULL, "UpdatedAt" = NOW()
WHERE "Email" = 'test@example.com';

-- Show user status
SELECT "Email", "Name", "Role", "Status", "FailedLoginAttempts", "LockedUntil" 
FROM users 
WHERE "Email" = 'test@example.com';
