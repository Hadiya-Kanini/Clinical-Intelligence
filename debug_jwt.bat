@echo off
echo Testing JWT Authentication Debug
echo ================================

echo.
echo 1. Fresh Login...
curl -s -c test_cookies.txt -X POST -H "Content-Type: application/json" -d "{\"email\":\"admin@clinicalintelligence.com\",\"password\":\"Admin@123456\"}" http://localhost:5000/api/v1/auth/login
echo.

echo.
echo 2. Extract token from cookie...
for /f "tokens=7" %%i in ('findstr "ci_access_token" test_cookies.txt') do set TOKEN=%%i
echo Token length: 
echo %TOKEN% | find /c /v ""

echo.
echo 3. Test with Authorization header...
curl -s -w "Status: %%{http_code}" -H "Authorization: Bearer %TOKEN%" http://localhost:5000/api/v1/auth/me
echo.

echo.
echo 4. Test with cookie...
curl -s -w "Status: %%{http_code}" -b test_cookies.txt http://localhost:5000/api/v1/auth/me
echo.

echo.
echo 5. Test ping endpoint with cookie...
curl -s -w "Status: %%{http_code}" -b test_cookies.txt http://localhost:5000/api/v1/ping
echo.

if exist test_cookies.txt del test_cookies.txt