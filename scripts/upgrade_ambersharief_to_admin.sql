-- Upgrade ambersharief811@gmail.com to Admin role
UPDATE users 
SET "Role" = 'Admin', "UpdatedAt" = NOW()
WHERE "Email" = 'ambersharief811@gmail.com';

-- Verify the change
SELECT "Id", "Email", "Name", "Role", "IsStaticAdmin", "Status" 
FROM users 
ORDER BY "IsStaticAdmin" DESC, "Email";
