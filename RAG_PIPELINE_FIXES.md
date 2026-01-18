# RAG Pipeline Fixes - Complete Solution

## Issues Identified

### 1. Database Constraint Missing ❌
```
Database error persisting chunks (batch rolled back): there is no unique or exclusion constraint matching the ON CONFLICT specification
```

**Cause:** The `document_chunks` table is missing the unique constraint required for the `ON CONFLICT` clause in `DocumentChunkStore`.

### 2. Config Attribute Typo ❌
```
⚠️ Entity extraction failed: 'WorkerConfig' object has no attribute 'entity_modell'
```

**Cause:** Typo in `main.py` line 520 - `config.entity_model` should be `config.extraction_model`.

## Fixes Applied

### Fix 1: Database Constraint Creation

**File:** `create_database_constraints.sql` (NEW)

Run this SQL script in PostgreSQL:

```sql
-- 1. Ensure pgvector extension is installed
CREATE EXTENSION IF NOT EXISTS vector;

-- 2. Create the missing unique constraint for document_chunks table
ALTER TABLE document_chunks 
ADD CONSTRAINT unique_document_chunk 
UNIQUE ("DocumentId", "ChunkHash");

-- 3. Create HNSW index for fast similarity search
CREATE INDEX IF NOT EXISTS idx_document_chunks_embedding 
ON document_chunks USING hnsw ("Embedding" vector_cosine_ops)
WITH (m = 16, ef_construction = 64);
```

### Fix 2: Config Attribute Typo

**File:** `worker/main.py` (Line 520)

**Before:**
```python
model=config.entity_model,  # ❌ Typo
```

**After:**
```python
model=config.extraction_model,  # ✅ Fixed
```

Also fixed timeout attribute:
```python
timeout=config.extraction_timeout  # ✅ Fixed
```

## Complete RAG Pipeline Flow (After Fixes)

```
1. Document Upload
   ↓
2. Text Extraction
   ↓
3. Text Chunking
   ↓
4. Embedding Generation
   ↓
5. ✅ Store Embeddings in pgvector (with constraint)
   ↓
6. ✅ RAG Retrieval (cosine similarity)
   ↓
7. ✅ Entity Extraction (fixed config)
   ↓
8. Store Extracted Entities
   ↓
9. Patient 360 Dashboard
```

## Expected Worker Output (After All Fixes)

```
📚 Created 9 text chunks for RAG processing
🔢 Generating embeddings for chunks...
✅ Generated embeddings for 9 chunks
💾 Stored 9 chunks with embeddings to pgvector database
🔍 Performing RAG retrieval...
🎯 Generated query embedding for: 'clinical entities...'
📋 Retrieved 9 chunks via RAG similarity search
🧠 Using 9 RAG-retrieved chunks for entity extraction
🤖 Performing entity extraction on RAG chunks...
✅ RAG-based entity extraction completed: X entities extracted
```

## How to Apply Fixes

### Step 1: Run Database Script
```bash
# Connect to PostgreSQL and run the script
psql -U postgres -d ClinicalIntelligence -f create_database_constraints.sql
```

### Step 2: Restart Worker Service
```bash
# Stop worker (Ctrl+C) and restart
cd worker
python worker_service.py
```

### Step 3: Test with Document Upload
1. Upload a PDF document through the frontend
2. Monitor worker logs for successful pipeline execution
3. Verify embeddings are stored: `SELECT COUNT(*) FROM document_chunks WHERE "Embedding" IS NOT NULL;`

## Verification Commands

### Check Database Constraints
```sql
SELECT conname, contype 
FROM pg_constraint 
WHERE conrelid = 'document_chunks'::regclass;
```

### Check pgvector Extension
```sql
SELECT extname, extversion 
FROM pg_extension 
WHERE extname = 'vector';
```

### Verify Embeddings Stored
```sql
SELECT "DocumentId", COUNT(*) as chunk_count
FROM document_chunks 
WHERE "Embedding" IS NOT NULL
GROUP BY "DocumentId";
```

### Test Similarity Search
```sql
SELECT "Id", "TextContent", 
       1 - ("Embedding" <=> '[0.1, 0.2, ...]'::vector) as similarity
FROM document_chunks
WHERE "Embedding" IS NOT NULL
ORDER BY "Embedding" <=> '[0.1, 0.2, ...]'::vector
LIMIT 10;
```

## Files Modified

1. **`worker/main.py`**
   - Fixed `config.entity_model` → `config.extraction_model`
   - Fixed `config.entity_extraction_timeout` → `config.extraction_timeout`

2. **`create_database_constraints.sql`** (NEW)
   - Creates missing unique constraint
   - Creates HNSW index for pgvector
   - Verifies pgvector extension

## Root Cause Analysis

1. **Database Schema Issue:** The `DocumentChunkStore` was designed to use `ON CONFLICT ("DocumentId", "ChunkHash")` but the database constraint was never created during initial setup.

2. **Code Typo:** During development, `entity_model` was used instead of the correct `extraction_model` attribute name defined in `WorkerConfig`.

Both issues were blocking the complete RAG pipeline from functioning properly.

## Success Criteria

After applying these fixes:
- ✅ Embeddings are successfully stored in pgvector
- ✅ RAG retrieval returns relevant chunks
- ✅ Entity extraction completes without errors
- ✅ Complete pipeline processes documents end-to-end

## Troubleshooting

If issues persist after fixes:

1. **Constraint Error:** Verify the constraint was created successfully
2. **Connection Error:** Ensure `DATABASE_URL` is properly set in `.env`
3. **Embedding Error:** Check Gemini API key and rate limits
4. **Retrieval Error:** Verify pgvector extension and HNSW index

The RAG pipeline should now be fully functional with proper vector storage and retrieval capabilities.
