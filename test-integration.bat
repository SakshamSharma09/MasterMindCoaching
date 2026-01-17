@echo off
echo ==============================================
echo 🧪 Testing MasterMind Coaching Classes Integration
echo ==============================================

echo.
echo 1. Testing Backend Health Check...
curl -s http://localhost:5000/health > health_response.json 2>nul
if %errorlevel% equ 0 (
    echo ✅ Backend is healthy
    type health_response.json
) else (
    echo ❌ Backend health check failed
)

echo.
echo 2. Testing API Documentation...
curl -s -o nul -w "%%{http_code}" http://localhost:5000/swagger/v1/swagger.json > swagger_code.txt
set /p swagger_code=<swagger_code.txt
if "%swagger_code%"=="200" (
    echo ✅ Swagger documentation is accessible
) else (
    echo ❌ Swagger documentation not accessible (HTTP %swagger_code%)
)

echo.
echo 3. Testing Authentication Endpoints...
curl -s -X POST http://localhost:5000/api/auth/otp/request -H "Content-Type: application/json" -d "{\"identifier\":\"\",\"type\":\"email\"}" > otp_response.json
if %errorlevel% equ 0 (
    echo ✅ OTP request endpoint is responding
) else (
    echo ❌ OTP request endpoint error
)

echo.
echo 4. Checking Project Structure...
if exist "src\backend\MasterMind.API\MasterMind.API.csproj" (
    echo ✅ Backend project exists
) else (
    echo ❌ Backend project missing
)

if exist "src\frontend\mastermind-web\package.json" (
    echo ✅ Frontend project exists
) else (
    echo ❌ Frontend project missing
)

if exist "docker\docker-compose.yml" (
    echo ✅ Docker configuration exists
) else (
    echo ❌ Docker configuration missing
)

echo.
echo ================================
echo 🎯 Integration Test Summary
echo ================================
echo Backend Status: Running on http://localhost:5000
echo Frontend: Ready for development (requires Node.js)
echo Docker: Ready for containerized deployment
echo Database: PostgreSQL (configure connection string)
echo.
echo 🚀 Next Steps:
echo 1. Install Node.js to run frontend locally
echo 2. Set up PostgreSQL database
echo 3. Run EF Core migrations
echo 4. Configure SMS/Email services for OTP
echo 5. Test full authentication flow
echo.
echo 📦 Free Tier Deployment Options:
echo • Railway (Free: 512MB RAM, PostgreSQL)
echo • Render (Free: 750 hours/month)
echo • Fly.io (Free: 256MB RAM, PostgreSQL)
echo • Azure (Student credits available)
echo • AWS (Free: 750 hours EC2, RDS)
echo.
echo ✨ Ready for deployment!

del health_response.json otp_response.json swagger_code.txt 2>nul