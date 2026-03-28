# Quick Deployment Guide

## Your Azure Resources
- Backend API: https://mastermind-api-2404-eadxgpe5f7dch9f6.centralindia-01.azurewebsites.net/
- Frontend Web: https://victorious-glacier-0e6507000.6.azurestaticapps.net/

## Required Actions

### 1. Configure Backend App Settings
In Azure Portal:
1. Go to your App Service: mastermind-api-2404-eadxgpe5f7dch9f6
2. Click "Configuration" → "Application settings"
3. Add these settings:
   ```
   ConnectionStrings__DefaultConnection = [Your SQL connection string]
   JWT__SecretKey = YourSecretKey12345678901234567890
   JWT__Issuer = MasterMind
   JWT__Audience = MasterMind
   ASPNETCORE_ENVIRONMENT = Production
   ```

### 2. Set Up GitHub Secrets
Go to your GitHub repository → Settings → Secrets → Actions:
1. Add `AZURE_CREDENTIALS` (run the PowerShell script to get this)
2. Add `API_BASE_URL` = `https://mastermind-api-2404-eadxgpe5f7dch9f6.centralindia-01.azurewebsites.net`

### 3. Run the Setup Script
1. Open PowerShell as Administrator
2. Navigate to your project folder
3. Run: `.\scripts\setup-deployment.ps1`
4. This will create the service principal and give you the JSON for GitHub secrets

### 4. Deploy
1. Commit and push all changes to GitHub
2. The GitHub Actions will automatically deploy both frontend and backend

### 5. Setup Database
After deployment, you'll need to run database migrations:
1. Use SQL Server Management Studio or Azure Data Studio
2. Connect to your Azure SQL database
3. Run the migration scripts from your project

## Testing
After deployment:
1. Test backend: https://mastermind-api-2404-eadxgpe5f7dch9f6.centralindia-01.azurewebsites.net/api/dashboard/stats
2. Test frontend: https://victorious-glacier-0e6507000.6.azurestaticapps.net/
3. Login and verify all features work
