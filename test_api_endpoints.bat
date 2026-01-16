@echo off
echo Testing Clinical Intelligence API Endpoints
echo =============================================

echo.
echo 1. Testing Health Check...
curl -s -w "Status: %%{http_code}" http://localhost:5000/health
echo.

echo.
echo 2. Testing Swagger Documentation...
curl -s -w "Status: %%{http_code}" -o nul http://localhost:5000/swagger/v1/swagger.json
echo.

echo.
echo 3. Testing Login Endpoint (empty credentials - should return 400)...
curl -s -w "Status: %%{http_code}" -X POST -H "Content-Type: application/json" -d "{\"email\":\"\",\"password\":\"\"}" http://localhost:5000/api/v1/auth/login
echo.

echo.
echo 4. Testing Login Endpoint (invalid email format - should return 400)...
curl -s -w "Status: %%{http_code}" -X POST -H "Content-Type: application/json" -d "{\"email\":\"invalid-email\",\"password\":\"test123\"}" http://localhost:5000/api/v1/auth/login
echo.

echo.
echo 5. Testing Protected Endpoint without auth (should return 401)...
curl -s -w "Status: %%{http_code}" http://localhost:5000/api/v1/ping
echo.

echo.
echo 6. Testing Admin Endpoint without auth (should return 401)...
curl -s -w "Status: %%{http_code}" http://localhost:5000/api/v1/admin/users
echo.

echo.
echo 7. Testing Database Health Check without auth (should return 401)...
curl -s -w "Status: %%{http_code}" http://localhost:5000/health/db
echo.

echo.
echo 8. Testing API Version Validation (v2 should return 400)...
curl -s -w "Status: %%{http_code}" http://localhost:5000/api/v2/ping
echo.

echo.
echo 9. Testing Login with Admin Credentials...
curl -s -w "Status: %%{http_code}" -X POST -H "Content-Type: application/json" -d "{\"email\":\"admin@clinicalintelligence.com\",\"password\":\"Admin@123456\"}" http://localhost:5000/api/v1/auth/login -c cookies.txt
echo.

echo.
echo 10. Testing Protected Endpoint with Auth Cookie...
curl -s -w "Status: %%{http_code}" -b cookies.txt http://localhost:5000/api/v1/ping
echo.

echo.
echo 11. Testing Current User Endpoint with Auth...
curl -s -w "Status: %%{http_code}" -b cookies.txt http://localhost:5000/api/v1/auth/me
echo.

echo.
echo 12. Testing Admin Users List with Auth...
curl -s -w "Status: %%{http_code}" -b cookies.txt http://localhost:5000/api/v1/admin/users
echo.

echo.
echo 13. Testing Database Health Check with Admin Auth...
curl -s -w "Status: %%{http_code}" -b cookies.txt http://localhost:5000/health/db
echo.

echo.
echo 14. Testing CSRF Token Endpoint...
curl -s -w "Status: %%{http_code}" -b cookies.txt http://localhost:5000/api/v1/auth/csrf
echo.

echo.
echo 15. Testing Dashboard Stats...
curl -s -w "Status: %%{http_code}" -b cookies.txt http://localhost:5000/api/v1/dashboard/stats
echo.

echo.
echo 16. Testing Documents List...
curl -s -w "Status: %%{http_code}" -b cookies.txt http://localhost:5000/api/v1/documents
echo.

echo.
echo 17. Testing Logout...
curl -s -w "Status: %%{http_code}" -X POST -b cookies.txt http://localhost:5000/api/v1/auth/logout
echo.

echo.
echo 18. Testing Protected Endpoint after Logout (should return 401)...
curl -s -w "Status: %%{http_code}" -b cookies.txt http://localhost:5000/api/v1/ping
echo.

echo.
echo =============================================
echo API Endpoint Testing Complete
echo =============================================

if exist cookies.txt del cookies.txt