# MasterMind Coaching Classes

A full-stack coaching institute management system deployed on Microsoft Azure.

| Layer | Technology |
|-------|------------|
| **Backend** | ASP.NET Core 9.0 Web API |
| **Frontend** | Vue.js 3 + TypeScript + Vite |
| **Database** | Azure SQL Database |
| **Hosting** | Azure App Service (API) + Azure Static Web App (Frontend) |
| **CI/CD** | GitHub Actions |
| **Auth** | JWT + OTP (Fast2SMS + Gmail SMTP) |
| **ORM** | Entity Framework Core 9 |
| **Styling** | Tailwind CSS |

## Project Structure

```
MasterMindCoaching/
├── src/
│   ├── backend/
│   │   └── MasterMind.API/          # ASP.NET Core 9 Web API
│   │       ├── Controllers/         # API controllers
│   │       ├── Data/                # DbContext
│   │       ├── Models/              # Entity models
│   │       ├── Services/            # Business logic
│   │       ├── Middleware/           # Exception handling
│   │       ├── Validators/          # FluentValidation
│   │       ├── Program.cs           # App entry point
│   │       └── appsettings.json     # Configuration (no secrets)
│   └── frontend/
│       └── mastermind-web/          # Vue.js 3 SPA
│           ├── src/
│           │   ├── views/           # Page components
│           │   ├── services/        # API service layer
│           │   ├── stores/          # Pinia state management
│           │   ├── router/          # Vue Router
│           │   └── config/          # API config
│           ├── vite.config.ts
│           └── package.json
├── .github/workflows/
│   └── azure-deploy.yml            # CI/CD pipeline
├── MasterMindCoaching.sln
└── README.md
```

## Azure Architecture

| Resource | Name | URL |
|----------|------|-----|
| **Backend API** | mastermind-api-2404 | `https://mastermind-api-2404-eadxgpe5f7dch9f6.centralindia-01.azurewebsites.net` |
| **Frontend** | mastermind-web-2404 | `https://victorious-glacier-0e6507000.6.azurestaticapps.net` |
| **SQL Server** | mastermind-db-2404 | `mastermind-db-2404.database.windows.net` |
| **Database** | mastermind | Azure SQL (General Purpose Serverless) |

## Features

**Admin** - Student/Teacher/Class management, Finance (fees, payments, expenses), Attendance, Lead tracking, Dashboard analytics

**Teacher** - View students, Mark attendance, Add remarks, Track performance

**Parent** - View child attendance, Performance reports, Fee status

## Local Development

### Prerequisites
- .NET 9 SDK
- Node.js 20+
- Azure CLI (optional, for deployment)

### Backend
```bash
cd src/backend/MasterMind.API
dotnet restore
dotnet run --environment Development
# API: http://localhost:5000
# Swagger: http://localhost:5000/swagger
```

### Frontend
```bash
cd src/frontend/mastermind-web
npm install --legacy-peer-deps
npm run dev
# App: http://localhost:3000
```

## Configuration

**Secrets are NOT stored in code.** All secrets are configured via:
- **Azure**: App Service Application Settings
- **Local**: `appsettings.Development.json` (gitignored)

Required environment variables for Azure:
```
ConnectionStrings__DefaultConnection  = Azure SQL connection string
Jwt__Secret                          = JWT signing key (min 32 chars)
Sms__ApiKey                          = Fast2SMS API key
Email__Username                      = SMTP email
Email__Password                      = SMTP app password
Cors__AllowedOrigins                 = Comma-separated allowed origins
ASPNETCORE_ENVIRONMENT               = Production
```

## Deployment

Deployments are automated via GitHub Actions on push to `main`. The workflow:
1. Builds and deploys the .NET 9 backend to Azure App Service
2. Builds the Vue.js frontend with production API URL
3. Deploys frontend to Azure Static Web App

### GitHub Secrets Required
- `AZURE_CREDENTIALS` - Service principal JSON
- `AZURE_STATIC_WEB_APPS_API_TOKEN` - Static Web App deployment token

## API Endpoints

| Endpoint | Description |
|----------|-------------|
| `/health` | Health check |
| `/swagger` | API documentation |
| `/api/auth/*` | Authentication (OTP, JWT) |
| `/api/students` | Student management |
| `/api/classes` | Class management |
| `/api/teachers` | Teacher management |
| `/api/attendance` | Attendance tracking |
| `/api/finance/*` | Fee & expense management |
| `/api/sessions` | Academic session management |

## License

MIT License - MasterMind Coaching Classes