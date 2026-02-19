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
    for i in {1..30}; do\n\
        if curl -f http://localhost:5000/api/status > /dev/null 2>&1; then\n\
            echo "API is ready!"\n\
            return 0\n\
        fi\n\
        echo "Attempt $i/30: API not ready, waiting..."\n\
        sleep 2\n\
    done\n\
    echo "API failed to start within 60 seconds"\n\
    exit 1\n\
}\n\
\n\
# Start the .NET API in background\n\
cd /app/backend\n\
dotnet MasterMind.API.dll &\n\
API_PID=$!\n\
\n\
# Wait for API to be ready\n\
wait_for_api\n\
\n\
# Start nginx in foreground\n\
nginx -g "daemon off;" &\n\
NGINX_PID=$!\n\
\n\
# Wait for both processes\n\
wait $API_PID $NGINX_PID' > /app/start.sh && chmod +x /app/start.sh

# Create non-root user
RUN adduser --disabled-password --gecos '' appuser && \
    chown -R appuser:appuser /app

# Expose ports
EXPOSE 3000 5000

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=10s --retries=3 \
  CMD curl -f http://localhost:5000/health || exit 1

# Switch to non-root user
USER appuser

# Start both services
CMD ["/app/start.sh"]
