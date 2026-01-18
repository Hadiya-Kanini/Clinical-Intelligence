SELECT "Email", "Name", "Role", "Status", "FailedLoginAttempts", "LockedUntil" 
FROM users 
WHERE "Email" = 'test@example.com';
