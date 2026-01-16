@echo off
echo Testing Fixed API Endpoints
echo ===========================

echo.
echo 1. Login with Admin Credentials...
curl -s -w "Status: %%{http_code}" -X POST -H "Content-Type: application/json" -d "{\"email\":\"admin@clinicalintelligence.com\",\"password\":\"Admin@123456\"}" http://localhost:5000/api/v1/auth/login -c cookies.txt
echo.

echo.
echo 2. Testing /auth/me endpoint (should work now)...
curl -s -w "Status: %%{http_code}" -b cookies.txt http://localhost:5000/api/v1/auth/me
echo.

echo.
echo 3. Testing Dashboard Stats (should work now)...
curl -s -w "Status: %%{http_code}" -b cookies.txt http://localhost:5000/api/v1/dashboard/stats
echo.

echo.
echo 4. Testing Documents List (should work now)...
curl -s -w "Status: %%{http_code}" -b cookies.txt http://localhost:5000/api/v1/documents
echo.

echo.
echo 5. Testing Logout (should work without content-type)...
curl -s -w "Status: %%{http_code}" -X POST -b cookies.txt http://localhost:5000/api/v1/auth/logout
echo.

echo.
echo 6. Testing Protected Endpoint after Logout (should return 401)...
curl -s -w "Status: %%{http_code}" -b cookies.txt http://localhost:5000/api/v1/ping
echo.

echo.
echo ===========================
echo Fix Verification Complete
echo ===========================

if exist cookies.txt del cookies.txt