# Multi-stage build for MasterMind Coaching on Railway
FROM node:20-alpine AS frontend-build

# Build frontend
WORKDIR /app/frontend
COPY src/frontend/mastermind-web/package*.json ./
RUN npm ci --only=production
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
COPY --from=frontend-build /app/dist ./frontend

# Copy nginx configuration
COPY docker/nginx.conf /etc/nginx/nginx.conf

# Create startup script
RUN echo '#!/bin/bash\n\
# Start the .NET API in background\n\
cd /app/backend\n\
dotnet MasterMind.API.dll &\n\
\n\
# Start nginx\n\
nginx -g "daemon off;"' > /app/start.sh && chmod +x /app/start.sh

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
