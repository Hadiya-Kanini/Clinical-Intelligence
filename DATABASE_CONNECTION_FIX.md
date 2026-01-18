# Database Connection String Fix

## Problem
The Python worker service was receiving database connection errors:
```
Database error retrieving chunks: invalid connection option "Host"
```

## Root Cause
The Python worker was attempting to use `DATABASE_CONNECTION_STRING` which contains .NET format connection strings (`Host=localhost;Database=...`), but Python's `psycopg2` and `psycopg` libraries expect PostgreSQL URI format (`postgresql://user:password@host:port/database`).

## Solution
Updated all Python worker files to use `DATABASE_URL` exclusively instead of `DATABASE_CONNECTION_STRING`.

### Files Modified

#### 1. `worker/config.py`
- Changed to use `DATABASE_URL` environment variable
- Added fallback to default connection string for local development
- Added warning when `DATABASE_URL` is not set

#### 2. `worker/worker_service.py`
- Updated `PatientManager` initialization to use `DATABASE_URL`
- Removed fallback to `DATABASE_CONNECTION_STRING`

#### 3. `worker/main.py`
- Updated `DocumentChunkRetriever` initialization to use `DATABASE_URL`
- Ensures RAG retrieval uses correct connection format

### Environment Variables

The `.env` file contains both connection strings for different purposes:

```env
# For .NET API (C# backend)
DATABASE_CONNECTION_STRING="Host=localhost;Database=ClinicalIntelligence;Username=postgres;Password=admin"

# For Python worker (psycopg2/psycopg)
DATABASE_URL="postgresql://postgres:admin@localhost:5432/ClinicalIntelligence"
```

### Connection String Formats

**❌ .NET Format (for C# API only):**
```
Host=localhost;Database=ClinicalIntelligence;Username=postgres;Password=admin
```

**✅ PostgreSQL URI Format (for Python worker):**
```
postgresql://postgres:admin@localhost:5432/ClinicalIntelligence
```

## Testing
After these changes, the worker should:
1. Successfully connect to PostgreSQL database
2. Store document chunks with embeddings
3. Retrieve chunks via RAG similarity search
4. Extract and store entities without database errors

## Files Using Database Connections

### Python Worker Files
- `worker/config.py` - Configuration loader
- `worker/worker_service.py` - Main worker service
- `worker/main.py` - Pipeline orchestration
- `worker/database/patient_manager.py` - Patient CRUD operations (uses psycopg2)
- `worker/storage/document_chunk_store.py` - Chunk storage (uses psycopg)
- `worker/retrieval/document_chunk_retriever.py` - RAG retrieval (uses psycopg)

### C# API Files
- `Server/ClinicalIntelligence.Api/Program.cs` - Uses `DATABASE_CONNECTION_STRING`
- All Entity Framework DbContext classes - Use .NET connection string format

## Important Notes
1. **Never** use `DATABASE_CONNECTION_STRING` in Python code
2. **Always** use `DATABASE_URL` for Python/psycopg connections
3. The two formats are **not interchangeable**
4. Keep both variables in `.env` for compatibility with both .NET API and Python worker
