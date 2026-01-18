@echo off
echo Testing login with curl...

REM First, get a session cookie
curl -c cookies.txt -X GET "http://localhost:5002/api/v1/auth/csrf" -H "Content-Type: application/json"

echo.
echo Attempting login...
curl -b cookies.txt -c cookies.txt -X POST "http://localhost:5002/api/v1/auth/login" ^
  -H "Content-Type: application/json" ^
  -d "{\"email\":\"test@example.com\",\"password\":\"Test123456\"}"

echo.
echo Login response complete.
type cookies.txt
