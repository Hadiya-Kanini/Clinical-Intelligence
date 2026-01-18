-- Update the user password with the correct hash
UPDATE users 
SET "PasswordHash" = '$2b$12$Wxmf.aYCjzO3dUVgI1e8/eq0Ul6LfDe7EfC2oEmiIKkJeIRHcqvg2',
    "FailedLoginAttempts" = 0,
    "LockedUntil" = NULL,
    "UpdatedAt" = NOW()
WHERE "Email" = 'ambersharief811@gmail.com';

-- Verify the update
SELECT "Email", "Name", "Role", "Status", "FailedLoginAttempts", "LockedUntil" 
FROM users 
WHERE "Email" = 'ambersharief811@gmail.com';
