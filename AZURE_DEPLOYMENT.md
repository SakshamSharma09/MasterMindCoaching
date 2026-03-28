# Azure App Service Deployment Guide - Free Tier

## Prerequisites
- Azure Free Account (sign up at [azure.com/free](https://azure.com/free))
- GitHub repository with your code
- Azure CLI installed (optional but recommended)

## 1. Database Setup - Azure Database for PostgreSQL

### Create PostgreSQL Database
1. Go to [Azure Portal](https://portal.azure.com)
2. Click "Create a resource" → "Databases" → "Azure Database for PostgreSQL"
3. Select "Flexible Server" tier
4. Configure:
   - **Server name**: mastermind-db-unique (must be globally unique)
   - **Resource group**: Create new "MasterMind-RG"
   - **Region**: East US (or nearest to you)
   - **PostgreSQL version**: 14
   - **Compute + storage**: "Burstable" → "B1ms" (Free tier eligible)
   - **Storage**: 32 GB (minimum)
   - **Backup retention**: 7 days
5. Click "Review + create" → "Create"

### Configure Database
1. After deployment, go to the database resource
2. Go to "Networking" → "Set public access" → "Allow access from Azure services"
3. Go to "Connection security" → Add your current IP to firewall rules
4. Go to "Database" → Click "Connect" to get connection string

### Run Migrations
1. Use pgAdmin or DBeaver to connect to your database
2. Run the SQL scripts from your project's migration files
3. Or use EF Core migrations if configured

## 2. Backend Deployment - Azure App Service

### Create App Service
1. In Azure Portal, click "Create a resource" → "Web" → "Web App"
2. Configure:
   - **Name**: mastermind-api-unique (must be globally unique)
   - **Publish**: Code
   - **Runtime stack**: .NET 8 (or your .NET version)
   - **Operating System**: Linux
   - **Region**: Same as your database
   - **App Service Plan**: Create new → "Free (F1)"
3. Click "Review + create" → "Create"

### Configure Backend
1. After deployment, go to the App Service
2. Go to "Configuration" → "Application settings"
3. Add these environment variables:
   ```
   ConnectionStrings__DefaultConnection=Host=mastermind-db-unique.postgres.database.azure.com;Database=mastermind;Username=your_username;Password=your_password;
   JWT__SecretKey=your-256-bit-secret-key-here
   JWT__Issuer=MasterMind
   JWT__Audience=MasterMind
   JWT__ExpirationMinutes=60
   ASPNETCORE_ENVIRONMENT=Production
   ```
4. Go to "Settings" → "General settings" → Set "Stack settings from .NET version"
5. Go to "TLS/SSL settings" → Enable "HTTPS Only"

### Deploy Backend via GitHub
1. In your App Service, go to "Deployment Center"
2. Select "GitHub" as source
3. Sign in to GitHub and select your repository
4. Configure:
   - **Branch**: main
   - **Root folder**: src/backend
   - **Build provider**: Azure App Service (Kudu)
5. Click "Save" → This will trigger the first deployment

## 3. Frontend Deployment - Azure Static Web Apps

### Create Static Web App
1. In Azure Portal, click "Create a resource" → "Static Web App"
2. Configure:
   - **Name**: mastermind-web-unique
   - **Resource group**: MasterMind-RG (use existing)
   - **Region**: Same as other resources
   - **SKU**: Free
3. Click "Sign in with GitHub"
4. Configure GitHub settings:
   - **Organization**: Your GitHub username
   - **Repository**: Your repository
   - **Branch**: main
   - **Build preset**: Vue.js
   - **App location**: /src/frontend/mastermind-web
   - **Api location**: /src/backend
   - **Output location**: /dist
5. Click "Review + create" → "Create"

### Configure Frontend
1. After deployment, go to the Static Web App
2. Go to "Configuration" → "Application settings"
3. Add environment variable:
   ```
   VITE_API_BASE_URL=https://mastermind-api-unique.azurewebsites.net
   ```
4. Go to "Custom domains" (optional) → Add your custom domain

## 4. CORS Configuration

### Configure CORS in Backend
1. Go to your backend App Service
2. Go to "Configuration" → "Application settings"
3. Add CORS settings:
   ```
   CORS__AllowedOrigins=https://mastermind-web-unique.azurewebsites.net,https://your-custom-domain.com
   ```
4. Or update your Program.cs to handle CORS properly

## 5. Monitoring and Logs

### Enable Application Logging
1. In both App Services, go to "App Service logs"
2. Enable "Application logging (Filesystem)"
3. Set "Retention Period (Days)" to 7
4. Set "Level" to "Information"

### View Logs
1. Go to "Log stream" to see real-time logs
2. Use "Diagnose and solve problems" for troubleshooting

## 6. Free Tier Limitations

### Important Limits
- **App Service Plan (F1)**:
  - 1 GB storage
  - 60 minutes CPU/day
  - Custom domains supported
  - SSL supported
  
- **Static Web Apps (Free)**:
  - 100 GB bandwidth/month
  - 3 custom domains
  - Always free
  
- **PostgreSQL (B1ms)**:
  - 1 vCPU
  - 2 GB RAM
  - 32 GB storage
  - ~$15/month (not free, but cheapest option)

### Cost Optimization Tips
1. Use Azure SQL Database (Basic tier ~$5/month) instead of PostgreSQL
2. Or use SQLite for truly free (but limited) option
3. Stop services when not in use
4. Monitor usage in Azure Cost Management

## 7. Alternative: Use Azure SQL Database (Cheaper)

### Create Azure SQL Database
1. Create "Azure SQL Database" instead of PostgreSQL
2. Configure:
   - **Database name**: mastermind
   - **Compute**: "Serverless" → "Basic" tier
   - **Storage**: 2 GB
3. This costs ~$5/month vs ~$15 for PostgreSQL

### Update Connection String
```
Server=tcp:your-server.database.windows.net,1433;Initial Catalog=mastermind;Persist Security Info=False;User ID=your_username;Password=your_password;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

## 8. Deployment Commands (Manual)

### If GitHub Actions fail, deploy manually:

#### Backend
```bash
# Install Azure CLI
# Login to Azure
az login

# Deploy backend
az webapp up --resource-group MasterMind-RG --name mastermind-api-unique --runtime "DOTNETCORE|8.0" --location eastus
```

#### Frontend
```bash
# Build frontend
cd src/frontend/mastermind-web
npm install
npm run build

# Deploy to static web app
az staticwebapp upload --name mastermind-web-unique --source ./dist
```

## 9. Troubleshooting

### Common Issues
1. **CORS Errors**: Ensure frontend URL is in CORS allowed origins
2. **Database Connection**: Check firewall rules and connection string
3. **Build Failures**: Check build logs in GitHub Actions
4. **404 Errors**: Verify routing configuration in frontend

### Useful Commands
```bash
# Restart app service
az webapp restart --resource-group MasterMind-RG --name mastermind-api-unique

# View logs
az webapp log tail --resource-group MasterMind-RG --name mastermind-api-unique

# Check app settings
az webapp config appsettings list --resource-group MasterMind-RG --name mastermind-api-unique
```

## 10. Post-Deployment Checklist

- [ ] Test all API endpoints
- [ ] Verify authentication flow
- [ ] Check session management
- [ ] Test database operations
- [ ] Verify HTTPS is working
- [ ] Set up monitoring alerts
- [ ] Document connection strings and passwords
- [ ] Schedule regular backups

## Next Steps

1. Monitor your free tier usage
2. Set up alerts for approaching limits
3. Consider upgrading to paid tiers if needed
4. Implement CI/CD improvements
5. Set up staging environment for testing
