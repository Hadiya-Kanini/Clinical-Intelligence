-- Clean database: Keep only static admin and ambersharief811@gmail.com, remove all other users
-- This will remove all test/mock data while preserving essential admin accounts

-- First, let's see what users exist before deletion
SELECT 'Current users before cleanup:' as info;
SELECT "Id", "Email", "Name", "Role", "IsStaticAdmin" 
FROM users 
ORDER BY "IsStaticAdmin" DESC, "Email";

-- Delete all users except:
-- 1. Static admin accounts (IsStaticAdmin = true)
-- 2. ambersharief811@gmail.com
DELETE FROM users 
WHERE 
    "IsStaticAdmin" = false 
    AND "Email" != 'ambersharief811@gmail.com';

-- Show remaining users after cleanup
SELECT 'Users remaining after cleanup:' as info;
SELECT "Id", "Email", "Name", "Role", "IsStaticAdmin" 
FROM users 
ORDER BY "IsStaticAdmin" DESC, "Email";

-- Also clean up related test data (optional - uncomment if needed)
/*
-- Clean up sessions for deleted users
DELETE FROM sessions WHERE "UserId" NOT IN (SELECT "Id" FROM users);

-- Clean up password reset tokens for deleted users  
DELETE FROM password_reset_tokens WHERE "UserId" NOT IN (SELECT "Id" FROM users);

-- Clean up document batches for deleted users
DELETE FROM document_batches WHERE "UploadedByUserId" NOT IN (SELECT "Id" FROM users);
*/
