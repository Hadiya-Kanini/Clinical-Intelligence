-- Check what data exists in the users table
SELECT 
    "Id", 
    "Email", 
    "Name", 
    "Role", 
    "Status", 
    "FailedLoginAttempts",
    "IsDeleted",
    "CreatedAt",
    "UpdatedAt"
FROM users 
WHERE "Email" LIKE '%pwd-reset-sessions-%' OR "Email" LIKE '%timing-test-user%' OR "Email" LIKE '%ratelimit-test%'
ORDER BY "Email"
LIMIT 10;
