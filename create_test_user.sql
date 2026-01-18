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
    '$2b$12$pDSGp/kPaOV4k2cNBinDQ.5SDQK65fNgtwszGj/V0bXuYEOqIXdHy',
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
