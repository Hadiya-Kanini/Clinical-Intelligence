-- Clear all sessions to fix session_invalidated error
DELETE FROM sessions;

-- Verify sessions are cleared
SELECT COUNT(*) as session_count FROM sessions;
