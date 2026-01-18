-- Enable Row-Level Security for document_chunks table (DR-005)
-- This script is idempotent and can be run multiple times safely.

-- Enable RLS on the table
ALTER TABLE document_chunks ENABLE ROW LEVEL SECURITY;

-- Force RLS for table owner as well (important for security)
ALTER TABLE document_chunks FORCE ROW LEVEL SECURITY;

-- Drop existing policies if they exist (for idempotency)
DROP POLICY IF EXISTS document_chunks_select_policy ON document_chunks;
DROP POLICY IF EXISTS document_chunks_insert_policy ON document_chunks;
DROP POLICY IF EXISTS document_chunks_update_policy ON document_chunks;
DROP POLICY IF EXISTS document_chunks_delete_policy ON document_chunks;

-- SELECT policy: Users can only read chunks from documents they uploaded, or Admin can read all
CREATE POLICY document_chunks_select_policy ON document_chunks
    FOR SELECT
    USING (
        EXISTS (
            SELECT 1 FROM documents d
            WHERE d."Id" = document_chunks."DocumentId"
            AND (
                d."UploadedByUserId" = current_setting('app.user_id', true)::uuid
                OR current_setting('app.user_role', true) = 'Admin'
            )
        )
    );

-- INSERT policy: Users can only insert chunks for documents they uploaded
CREATE POLICY document_chunks_insert_policy ON document_chunks
    FOR INSERT
    WITH CHECK (
        EXISTS (
            SELECT 1 FROM documents d
            WHERE d."Id" = document_chunks."DocumentId"
            AND (
                d."UploadedByUserId" = current_setting('app.user_id', true)::uuid
                OR current_setting('app.user_role', true) = 'Admin'
            )
        )
    );

-- UPDATE policy: Users can only update chunks for documents they uploaded
CREATE POLICY document_chunks_update_policy ON document_chunks
    FOR UPDATE
    USING (
        EXISTS (
            SELECT 1 FROM documents d
            WHERE d."Id" = document_chunks."DocumentId"
            AND (
                d."UploadedByUserId" = current_setting('app.user_id', true)::uuid
                OR current_setting('app.user_role', true) = 'Admin'
            )
        )
    );

-- DELETE policy: Users can only delete chunks for documents they uploaded
CREATE POLICY document_chunks_delete_policy ON document_chunks
    FOR DELETE
    USING (
        EXISTS (
            SELECT 1 FROM documents d
            WHERE d."Id" = document_chunks."DocumentId"
            AND (
                d."UploadedByUserId" = current_setting('app.user_id', true)::uuid
                OR current_setting('app.user_role', true) = 'Admin'
            )
        )
    );

-- Verify policies are created
SELECT schemaname, tablename, policyname, permissive, roles, cmd, qual
FROM pg_policies
WHERE tablename = 'document_chunks';
