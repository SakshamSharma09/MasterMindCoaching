# MasterMind Coaching - Deployment Guide

## Free/Low-Cost Deployment Options

### 1. Vercel (Frontend) + Railway/Render (Backend) - Recommended

#### Frontend Deployment (Vercel)
- **Cost**: Free tier available
- **Pros**: 
  - Excellent CI/CD integration with GitHub
  - Automatic deployments on push
  - Global CDN
  - Custom domains supported
- **Steps**:
  1. Push code to GitHub repository
  2. Sign up at [vercel.com](https://vercel.com)
  3. Import GitHub repository
  4. Configure root directory: `src/frontend/mastermind-web`
  5. Set build command: `npm run build`
  6. Set output directory: `dist`
  7. Add environment variables in Vercel dashboard

#### Backend Deployment (Railway)
- **Cost**: $5/month after free trial (or Render with $7/month free tier)
- **Pros**:
  - Supports .NET applications
  - Managed PostgreSQL database
  - Easy deployment from GitHub
- **Steps**:
  1. Create a new project on [railway.app](https://railway.app)
  2. Connect GitHub repository
  3. Set root directory: `src/backend`
  4. Add environment variables:
     - `ConnectionStrings__DefaultConnection`: Railway will provide this
     - `JWT__SecretKey`: Generate a secure secret
     - `JWT__Issuer`: Your app name
     - `JWT__Audience`: Your app name
  5. Deploy

### 2. Render (Full Stack)
- **Cost**: Free tier available, $7/month for production
- **Pros**:
  - Host both frontend and backend
  - Built-in PostgreSQL
  - SSL certificates included
- **Steps**:
  1. Create account at [render.com](https://render.com)
  2. Connect GitHub repository
  3. Create two services:
     - Web Service for backend (.NET)
     - Static Site for frontend (Vue.js)

### 3. Azure App Service (Free Tier)
- **Cost**: Free tier available (10 apps, 1GB storage)
- **Pros**:
  - Microsoft's cloud platform
  - Good .NET support
  - Free SSL with custom domains
- **Steps**:
  1. Create free Azure account
  2. Create App Service for backend
  3. Create Static Web App for frontend
  4. Configure GitHub Actions for CI/CD

### 4. Heroku (Low Cost)
- **Cost**: ~$5-7/month after free trial
- **Pros**:
  - Simple deployment
  - Add-ons for database
  - Good .NET support
- **Steps**:
  1. Install Heroku CLI
  2. Create Heroku app
  3. Add PostgreSQL add-on
  4. Deploy using Git

## Environment Variables Configuration

### Frontend (.env)
```env
VITE_API_BASE_URL=https://your-backend-url.com
VITE_APP_NAME=MasterMind Coaching
```

### Backend
```env
ConnectionStrings__DefaultConnection=your-database-connection-string
JWT__SecretKey=your-256-bit-secret-key
JWT__Issuer=MasterMind
JWT__Audience=MasterMind
JWT__ExpirationMinutes=60
ASPNETCORE_ENVIRONMENT=Production
```

## Database Setup

### Option 1: Managed Database (Recommended)
- Railway PostgreSQL
- Render PostgreSQL
- Azure Database for PostgreSQL
- Supabase (Free tier available)

### Option 2: Self-hosted
- Install PostgreSQL on VPS
- Configure backups
- Handle security updates

## Security Considerations

1. **HTTPS**: Always use HTTPS in production
2. **Environment Variables**: Never commit secrets to Git
3. **Database Security**: Use connection strings with limited permissions
4. **CORS**: Configure CORS to allow only your frontend domain
5. **Rate Limiting**: Implement rate limiting on API endpoints

## Pre-deployment Checklist

### Backend
- [ ] Update database connection string
- [ ] Set strong JWT secret key
- [ ] Configure CORS settings
- [ ] Enable HTTPS redirection
- [ ] Set up logging
- [ ] Run database migrations

### Frontend
- [ ] Update API base URL
- [ ] Set environment variables
- [ ] Optimize build for production
- [ ] Test all API calls
- [ ] Verify authentication flow

## Deployment Commands

### Backend (.NET)
```bash
dotnet restore
dotnet build -c Release
dotnet publish -c Release -o ./publish
```

### Frontend (Vue.js)
```bash
npm install
npm run build
```

## Monitoring and Maintenance

1. **Application Monitoring**:
   - Application Insights (Azure)
   - Sentry (Error tracking)
   - LogRocket (User session recording)

2. **Database Backups**:
   - Configure automatic backups
   - Test restore procedures

3. **Performance**:
   - Monitor response times
   - Optimize database queries
   - Use CDN for static assets

## Troubleshooting

### Common Issues
1. **CORS Errors**: Update CORS configuration in backend
2. **Database Connection**: Check connection string and firewall rules
3. **Build Failures**: Verify all dependencies and environment variables
4. **Authentication Issues**: Ensure JWT settings match between frontend and backend

### Deployment Tips
1. Always test in staging environment first
2. Keep rollback plan ready
3. Monitor logs after deployment
4. Document any custom configurations

## Cost Optimization

1. **Use free tiers** when possible
2. **Optimize database queries** to reduce resource usage
3. **Implement caching** where appropriate
4. **Use CDN** for static assets
5. **Monitor resource usage** regularly

## Next Steps

1. Choose deployment platform based on requirements
2. Set up CI/CD pipeline
3. Configure monitoring and alerts
4. Document deployment process
5. Plan for scaling when needed
