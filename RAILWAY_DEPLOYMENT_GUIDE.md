# Railway Deployment Guide

## 🚀 Deploy MasterMind Coaching to Railway (Free)

### Prerequisites
- GitHub repository
- Railway account (free)
- Railway CLI (optional)

### Step 1: Push to GitHub
```bash
git add .
git commit -m "Ready for Railway deployment"
git push origin main
```

### Step 2: Set Up Railway
1. Go to [railway.app](https://railway.app)
2. Sign up with GitHub
3. Click "New Project" → "Deploy from GitHub repo"
4. Select your repository

### Step 3: Configure Services
Railway will automatically detect your docker-compose.yml and create:
- **PostgreSQL Database**
- **.NET API Service**
- **Vue.js Frontend Service**

### Step 4: Environment Variables
Set these in Railway dashboard:
```env
# For API Service
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=${{Postgres.URL}}
JWT__Secret=YourSuperSecretKeyHere32CharsMin
JWT__Issuer=MasterMindCoaching
JWT__Audience=MasterMindCoaching

# For Frontend Service
VITE_API_URL=${{API.URL}}/api
VITE_APP_NAME=MasterMind Coaching Classes
```

### Step 5: Deploy
- Railway will build and deploy automatically
- Wait for deployment to complete
- Your app will be available at `your-app-name.railway.app`

### Step 6: Custom Domain (Optional)
1. Go to Settings → Domains
2. Add your custom domain
3. Update DNS records as instructed

## 📊 Monitoring
- Check logs in Railway dashboard
- Monitor resource usage
- Set up alerts (paid feature)

## 🔧 Troubleshooting
- Check build logs for errors
- Verify environment variables
- Ensure database migrations run
- Check network connectivity between services

## 💡 Pro Tips
- Use Railway's GitHub integration for auto-deploys
- Set up staging environment for testing
- Monitor your 500-hour monthly limit
- Consider upgrading for production use
