# PowerShell script to verify pgvector setup
Write-Host "Verifying pgvector setup..." -ForegroundColor Green

# Set environment variables
$env:PGPASSWORD = "admin"

# Test 1: Check if vector extension exists
Write-Host "`n1. Checking vector extension..." -ForegroundColor Yellow
$result = psql -h localhost -U postgres -d ClinicalIntelligence -c "SELECT extname FROM pg_extension WHERE extname = 'vector';" -t --no-align
if ($result.Trim() -eq "vector") {
    Write-Host "✓ Vector extension is installed" -ForegroundColor Green
} else {
    Write-Host "✗ Vector extension not found" -ForegroundColor Red
}

# Test 2: Check document_chunks table
Write-Host "`n2. Checking document_chunks table..." -ForegroundColor Yellow
$result = psql -h localhost -U postgres -d ClinicalIntelligence -c "SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'document_chunks');" -t --no-align
if ($result.Trim() -eq "t") {
    Write-Host "✓ Document chunks table exists" -ForegroundColor Green
} else {
    Write-Host "✗ Document chunks table not found" -ForegroundColor Red
}

# Test 3: Check embedding column
Write-Host "`n3. Checking embedding column..." -ForegroundColor Yellow
$result = psql -h localhost -U postgres -d ClinicalIntelligence -c "SELECT column_name, data_type FROM information_schema.columns WHERE table_name = 'document_chunks' AND column_name = 'embedding';" -t --no-align
if ($result -match "embedding.*vector") {
    Write-Host "✓ Embedding column exists with vector type" -ForegroundColor Green
} else {
    Write-Host "✗ Embedding column not found or not vector type" -ForegroundColor Red
    Write-Host "Result: $result" -ForegroundColor Yellow
}

# Test 4: Test vector functionality
Write-Host "`n4. Testing vector functionality..." -ForegroundColor Yellow
$result = psql -h localhost -U postgres -d ClinicalIntelligence -c "SELECT '[1,2,3]'::vector as test_vector;" -t --no-align
if ($result -match "\[1,2,3\]") {
    Write-Host "✓ Vector operations working" -ForegroundColor Green
} else {
    Write-Host "✗ Vector operations not working" -ForegroundColor Red
}

Write-Host "`nPgvector verification complete!" -ForegroundColor Green
