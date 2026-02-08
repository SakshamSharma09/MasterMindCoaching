@echo off
echo 🚀 Starting MasterMind Coaching Application...
echo.

REM Check if Node.js is installed
node --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ Node.js is not installed. Please install Node.js first.
    echo 📥 Download from: https://nodejs.org/
    pause
    exit /b 1
)

REM Check if dependencies are installed
if not exist node_modules (
    echo 📦 Installing dependencies...
    npm install
    if %errorlevel% neq 0 (
        echo ❌ Failed to install dependencies.
        pause
        exit /b 1
    )
)

REM Test database connection
echo 🔍 Testing database connection...
npm run test-db
echo.

REM Start the application
echo 🌐 Starting application server...
echo 📱 Application will be available at: http://localhost:3000
echo 🛑 Press Ctrl+C to stop the server
echo.

npm start

pause
