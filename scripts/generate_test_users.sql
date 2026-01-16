-- Generate complete test users with all required fields
-- Run this script to populate the users table with proper test data

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
) VALUES 
-- Test users from your original data (with missing fields added)
('05d9f8b5-d6d0-41f1-97ab-21c1d5783a4a', 'pwd-reset-sessions-8e5d16e726de409f9194562a37fbf806@example.com', '$2a$12$TsMxIXtx2uK3AVnwjAFhg.O6XC7PgkqnceEo2.XIwebEPJ2wLCuY2', 'Test User 1', 'Standard', 'Active', 0, NULL, false, false, NULL, NOW(), NOW()),
('0739ce71-051e-4d10-807c-4cda060775bc', 'pwd-reset-sessions-5f4bec40e10e4ee78c325b7d9680b54e@example.com', '$2a$12$3UeDxPm1.rsi76k3ybZvQuTSKig82iWSLbQblC4t4ITgOm.FVsHr2', 'Test User 2', 'Standard', 'Active', 0, NULL, false, false, NULL, NOW(), NOW()),
('0c372f34-3af2-475e-ad5f-11c4be26656d', 'pwd-reset-sessions-acefba7dbcae4743bb4959c9471a187f@example.com', '$2a$12$YZTR2qUngVQOjKy6SuvDkuDOJpQwzJdpPDrWtTVhjMHKtHWj7iBGm', 'Test User 3', 'Standard', 'Active', 0, NULL, false, false, NULL, NOW(), NOW()),
('0fbacd16-d20e-4d0e-8f5e-397f11b72023', 'newuser_0937fe972aa845ee8cce8478d168c689@example.com', '$2a$11$FggClth2CCUnZPsJ8QqWVOrAuCwUvm4W3YuPJsc6lh/jv2HWptZTq', 'New Test User', 'Standard', 'Active', 0, NULL, false, false, NULL, NOW(), NOW()),
('109eac78-a5fe-4eae-80c8-76804834c25e', 'pwd-reset-sessions-f072e202e3e749cea67218ae19121771@example.com', '$2a$12$RPIZXQlbmsyEmmufjkAZJug4Dc7jnp5ZQ3fVoxVsIN/iXEsPUGT9m', 'Test User 4', 'Standard', 'Active', 0, NULL, false, false, NULL, NOW(), NOW()),
('12ac2f78-5903-48ab-95c3-78630622b28b', 'pwd-reset-sessions-51616e6987354edfb53251a7f327c25d@example.com', '$2a$12$QaOW/qs.mwXU5gLX0UbwZeBV5jnRjVY4WAMP1GLUkPVR1d9LYOx1e', 'Test User 5', 'Standard', 'Active', 0, NULL, false, false, NULL, NOW(), NOW()),
('1610504b-8178-4909-8cfe-957b26277460', 'pwd-reset-sessions-92f2138c7c1f4ffcaef75d3f3bd2bfd4@example.com', '$2a$12$THJnQzvRAKDbwB3wBd0g4ODxCy0F9QC3TVR9kQr070679jpPmDTmq', 'Test User 6', 'Standard', 'Active', 0, NULL, false, false, NULL, NOW(), NOW()),
('1ec631a4-7f59-45d7-9af4-efcfeb028c63', 'timing-test-user@example.com', '$2a$11$LkZhx1aOLJOfIZ7QRauJh.GxHNTFfWSOUIiEDDXdhvCAiiOKqCtW6', 'Timing Test User', 'Standard', 'Active', 0, NULL, false, false, NULL, NOW(), NOW()),
('20a10f30-e109-4f04-b473-6b1aa607c2a5', 'pwd-reset-sessions-f274f7997882463a90304ded3a206ad4@example.com', '$2a$11$qJQ2piyYc.8Cmev7SeC5Fuws/oqlIsA/fid23RnCsHrOmSrv8dl02', 'Test User 7', 'Standard', 'Active', 0, NULL, false, false, NULL, NOW(), NOW()),
('21ebc7ea-5385-410e-913e-248a792c24e2', 'password-reset-sessions-test@example.com', '$2a$12$Bml4nQGgZWxqGJWmPmFXqOZdyH5oCd3wB6ioBkvakoUjEU0T2x3KK', 'Password Reset Test', 'Standard', 'Active', 0, NULL, false, false, NULL, NOW(), NOW()),
('240e0f6e-6462-466b-b1c0-561addfd676d', 'pwd-reset-sessions-65648c541cd346198bfb650af7df3133@example.com', '$2a$11$tLdsZe6.FsvH8RcLahAAD.kzQvr/56nqB0FW8LNDG7/FAZacocRx.', 'Test User 8', 'Standard', 'Active', 0, NULL, false, false, NULL, NOW(), NOW()),
('24a84f28-8cea-444c-b360-67490f461bbf', 'pwd-reset-sessions-f6c0cbaf9d9a43d4b6c6c6d1d35140eb@example.com', '$2a$12$3tun63UBg6S19bt3noSN4uwDSvaxHXy4LZOHajzoGpnla7AlcjUgK', 'Test User 9', 'Standard', 'Active', 0, NULL, false, false, NULL, NOW(), NOW()),
('2aca8cd2-cd6f-47fc-a24b-295cd7622efc', 'ratelimit-test@example.com', '$2a$11$nEftaIWfZDu3.fAYWuGRb.EazAsayT8lxpu/ZtCmtYZeZuZyJxh5m', 'Rate Limit Test', 'Standard', 'Active', 0, NULL, false, false, NULL, NOW(), NOW()),
('2ef19eaf-205a-4600-bcfd-a3d39f5ffe59', 'fk-test-4c6b80ec-a353-40b1-9788-5b207e2d42fa@example.com', '$2a$12$somehashplaceholder', 'Foreign Key Test', 'Standard', 'Active', 0, NULL, false, false, NULL, NOW(), NOW());
