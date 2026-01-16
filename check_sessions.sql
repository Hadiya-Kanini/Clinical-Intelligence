SELECT "Id", "UserId", "CreatedAt", "ExpiresAt", "IsRevoked" 
FROM sessions 
WHERE "UserId" = 'bf9dadbd-aa6a-4826-9ffa-a40e366d34d4' 
ORDER BY "CreatedAt" DESC 
LIMIT 5;
