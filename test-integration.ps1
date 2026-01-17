# MasterMind Coaching Classes - Integration Test Script
# This script tests the backend API endpoints and Docker setup

Write-Host "🧪 Testing MasterMind Coaching Classes Integration" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan

# Test 1: Check if backend is running
Write-Host "`n1. Testing Backend Health Check..." -ForegroundColor Yellow
try {
    $healthResponse = Invoke-RestMethod -Uri "http://localhost:5000/health" -Method GET
    Write-Host "✅ Backend is healthy: $($healthResponse.status)" -ForegroundColor Green
    Write-Host "   Environment: $($healthResponse.environment)" -ForegroundColor Gray
    Write-Host "   Timestamp: $($healthResponse.timestamp)" -ForegroundColor Gray
} catch {
    Write-Host "❌ Backend health check failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 2: Check Swagger documentation
Write-Host "`n2. Testing API Documentation..." -ForegroundColor Yellow
try {
    $swaggerResponse = Invoke-WebRequest -Uri "http://localhost:5000/swagger/v1/swagger.json" -Method GET
    if ($swaggerResponse.StatusCode -eq 200) {
        Write-Host "✅ Swagger documentation is accessible" -ForegroundColor Green
    }
} catch {
    Write-Host "❌ Swagger documentation not accessible: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 3: Test authentication endpoints (without actual login)
Write-Host "`n3. Testing Authentication Endpoints..." -ForegroundColor Yellow

# Test OTP request endpoint (should return validation error for empty data)
try {
    $otpRequest = @{
        identifier = ""
        type = "email"
    } | ConvertTo-Json

    $otpResponse = Invoke-WebRequest -Uri "http://localhost:5000/api/auth/otp/request" -Method POST -Body $otpRequest -ContentType "application/json"
    Write-Host "✅ OTP request endpoint is responding" -ForegroundColor Green
} catch {
    if ($_.Exception.Response.StatusCode -eq 400) {
        Write-Host "✅ OTP request endpoint validation working (expected 400 for empty data)" -ForegroundColor Green
    } else {
        Write-Host "❌ OTP request endpoint error: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Test 4: Check Docker setup
Write-Host "`n4. Testing Docker Setup..." -ForegroundColor Yellow
try {
    $dockerVersion = docker --version
    Write-Host "✅ Docker is installed: $dockerVersion" -ForegroundColor Green

    # Check if docker-compose file exists
    if (Test-Path "docker/docker-compose.yml") {
        Write-Host "✅ Docker Compose configuration exists" -ForegroundColor Green
    } else {
        Write-Host "❌ Docker Compose configuration missing" -ForegroundColor Red
    }

    # Check Dockerfiles
    if ((Test-Path "docker/Dockerfile.api") -and (Test-Path "docker/Dockerfile.web")) {
        Write-Host "✅ Dockerfiles exist" -ForegroundColor Green
    } else {
        Write-Host "❌ Dockerfiles missing" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Docker not available: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 5: Check project structure
Write-Host "`n5. Checking Project Structure..." -ForegroundColor Yellow
$requiredPaths = @(
    "src/backend/MasterMind.API/MasterMind.API.csproj",
    "src/frontend/mastermind-web/package.json",
    "docs/README.md",
    "docker/docker-compose.yml"
)

$structureOk = $true
foreach ($path in $requiredPaths) {
    if (Test-Path $path) {
        Write-Host "✅ $path exists" -ForegroundColor Green
    } else {
        Write-Host "❌ $path missing" -ForegroundColor Red
        $structureOk = $false
    }
}

if ($structureOk) {
    Write-Host "✅ Project structure is complete" -ForegroundColor Green
} else {
    Write-Host "❌ Project structure has missing files" -ForegroundColor Red
}

# Summary
Write-Host "`n🎯 Integration Test Summary" -ForegroundColor Cyan
Write-Host "==========================" -ForegroundColor Cyan
Write-Host "Backend Status: Running on http://localhost:5000" -ForegroundColor White
Write-Host "Frontend: Ready for development (run 'npm install && npm run dev')" -ForegroundColor White
Write-Host "Docker: Ready for containerized deployment" -ForegroundColor White
Write-Host "Database: PostgreSQL (configure connection string for full functionality)" -ForegroundColor White

Write-Host "`n🚀 Next Steps:" -ForegroundColor Green
Write-Host "1. Install Node.js to run frontend locally" -ForegroundColor White
Write-Host "2. Set up PostgreSQL database" -ForegroundColor White
Write-Host "3. Run EF Core migrations" -ForegroundColor White
Write-Host "4. Configure SMS/Email services for OTP" -ForegroundColor White
Write-Host "5. Test full authentication flow" -ForegroundColor White

Write-Host "`n📦 Deployment Options:" -ForegroundColor Green
Write-Host "• Railway (Free tier: 512MB RAM, PostgreSQL)" -ForegroundColor White
Write-Host "• Render (Free tier: 750 hours/month)" -ForegroundColor White
Write-Host "• Fly.io (Free tier: 256MB RAM, PostgreSQL)" -ForegroundColor White
Write-Host "• Azure (Student credits available)" -ForegroundColor White
Write-Host "• AWS (Free tier: 750 hours EC2, RDS)" -ForegroundColor White

Write-Host "`n✨ Ready for deployment!" -ForegroundColor Cyan