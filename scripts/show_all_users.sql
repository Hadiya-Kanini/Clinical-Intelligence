-- Show all current users
SELECT "Id", "Email", "Name", "Role", "IsStaticAdmin", "Status" 
FROM users 
ORDER BY "IsStaticAdmin" DESC, "Email";
