-- Delete existing test user
DELETE FROM users WHERE "Email" = 'test@example.com';

-- Insert new test user
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
VALUES
(
    'a95c8075-e64a-4a2d-a7f8-cf7f1126f0b1',
    'test@example.com',
    '$2b$12$huZSKI5qImwthfOQearwDOvTsmdNJzkuujnPL.xJgho33VJU7EvOC',
    'Test User',
    'Admin',
    'Active',
    0,
    NULL,
    false,
    false,
    NULL,
    NOW(),
    NOW()
);
