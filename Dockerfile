# Multi-stage build for MasterMind Coaching on Railway
FROM node:20-alpine AS frontend-build

# Build frontend
WORKDIR /app/frontend
COPY src/frontend/mastermind-web/package*.json ./
RUN npm ci --legacy-peer-deps
COPY src/frontend/mastermind-web/ .
RUN npm run build

# Build backend
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS backend-build
WORKDIR /src

# Copy and restore backend dependencies
COPY ["src/backend/MasterMind.API/MasterMind.API.csproj", "MasterMind.API/"]
RUN dotnet restore "MasterMind.API/MasterMind.API.csproj"

# Copy backend source and build
COPY src/backend/MasterMind.API/ MasterMind.API/
WORKDIR "/src/MasterMind.API"
RUN dotnet build "MasterMind.API.csproj" -c Release -o /app/build

# Publish backend
FROM backend-build AS backend-publish
RUN dotnet publish "MasterMind.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Install nginx and curl
RUN apt-get update && apt-get install -y nginx curl && rm -rf /var/lib/apt/lists/*

# Copy published backend
COPY --from=backend-publish /app/publish ./backend

# Copy built frontend
COPY --from=frontend-build /app/frontend/dist ./frontend

# Copy nginx configuration
COPY docker/nginx.conf /etc/nginx/nginx.conf

# Create startup script with health checks
RUN echo '#!/bin/bash\n\
set -e\n\
\n\
# Function to wait for API to be ready\n\
wait_for_api() {\n\
    echo "Waiting for API to be ready..."\n\
    for i in {1..60}; do\n\
        echo "Attempt $i/60: Checking API health..."\n\
        if curl -f http://localhost:5000/api/status > /dev/null 2>&1; then\n\
            echo "API is ready!"\n\
            return 0\n\
        fi\n\
        # Also try the basic health endpoint\n\
        if curl -f http://localhost:5000/health > /dev/null 2>&1; then\n\
            echo "API health endpoint responding!"\n\
            return 0\n\
        fi\n\
        # Check if the process is running\n\
        if ! ps aux | grep "dotnet MasterMind.API.dll" | grep -v grep > /dev/null; then\n\
            echo "API process is not running!"\n\
            return 1\n\
        fi\n\
        echo "API not ready yet, waiting 2 seconds..."\n\
        sleep 2\n\
    done\n\
    echo "API failed to start within 120 seconds"\n\
    return 1\n\
}\n\
\n\
# Start the .NET API in background\n\
cd /app/backend\n\
echo "Starting .NET API..."\n\
dotnet MasterMind.API.dll &\n\
API_PID=$!\n\
echo "API PID: $API_PID"\n\
\n\
# Give API a moment to start\n\
sleep 5\n\
\n\
# Check if frontend files exist\n\
echo "Checking frontend files..."\n\
if [ -f "/app/frontend/index.html" ]; then\n\
    echo "✅ Frontend index.html found"\n\
    ls -la /app/frontend/ | head -10\n\
else\n\
    echo "❌ Frontend index.html NOT found"\n\
    echo "Contents of /app:"\n\
    ls -la /app/\n\
    echo "Contents of /app/frontend:"\n\
    ls -la /app/frontend/ || echo "Frontend directory does not exist"\n\
fi\n\
\n\
# Wait for API to be ready\n\
if wait_for_api; then\n\
    echo "API is ready, starting nginx..."\n\
    # Start nginx in foreground\n\
    nginx -g "daemon off;" &\n\
    NGINX_PID=$!\n\
    echo "Nginx PID: $NGINX_PID"\n\
    \n\
    # Wait for both processes\n\
    wait $API_PID $NGINX_PID\n\
else\n\
    echo "API failed to start, exiting..."\n\
    exit 1\n\
fi' > /app/start.sh && chmod +x /app/start.sh

# Create non-root user
RUN adduser --disabled-password --gecos '' appuser && \
    chown -R appuser:appuser /app

# Expose ports
EXPOSE 8080 5000

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=10s --retries=3 \
  CMD curl -f http://localhost:5000/health || exit 1

# Switch to non-root user
USER appuser

# Start both services
CMD ["/app/start.sh"]
