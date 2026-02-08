# Real-Time OTP Configuration Guide

## 🚀 Current Status: **READY TO CONFIGURE**

Your MasterMind Coaching system is already set up with real-time OTP sending capabilities! Here's how to configure it:

---

## 📧 **Configuration Files**

### 1. **Backend Configuration** (`appsettings.json`)
```json
{
  "Sms": {
    "Provider": "Fast2SMS",
    "ApiKey": "YOUR_FAST2SMS_API_KEY",
    "SenderId": "MSTMND",
    "TemplateId": "",
    "Route": "otp",
    "UseSandbox": true,
    "DefaultCountryCode": "91",
    "OtpMessageTemplate": "Your MasterMind Coaching verification code is {otp}. Valid for 5 minutes. Do not share with anyone."
  },
  "Email": {
    "SmtpServer": "smtp.gmail.com",
    "Port": 587,
    "UseSsl": true,
    "Username": "YOUR_GMAIL_APP_PASSWORD",
    "Password": "YOUR_GMAIL_APP_PASSWORD",
    "FromEmail": "noreply@mastermind-coaching.com",
    "FromName": "MasterMind Coaching Classes"
  }
}
```

---

## 📱 **SMS Configuration Options**

### **Option 1: Fast2SMS (Recommended for India)**
1. **Sign up**: [https://www.fast2sms.com/](https://www.fast2sms.com/)
2. **Get API Key**: Dashboard → API Keys
3. **Update appsettings.json**:
   ```json
   "Sms": {
     "Provider": "Fast2SMS",
     "ApiKey": "your_fast2sms_api_key_here",
     "UseSandbox": false
   }
   ```

### **Option 2: MSG91 (Global Coverage)**
1. **Sign up**: [https://msg91.com/](https://msg91.com/)
2. **Get API Key**: Dashboard → API Keys
3. **Create OTP Template**: Dashboard → SMS Templates
4. **Update appsettings.json**:
   ```json
   "Sms": {
     "Provider": "MSG91",
     "ApiKey": "your_msg91_api_key_here",
     "TemplateId": "your_template_id_here",
     "UseSandbox": false
   }
   ```

---

## 📧 **Email Configuration Options**

### **Option 1: Gmail (Free & Easy)**
1. **Enable 2FA**: Go to Gmail → Settings → Security → 2-Step Verification
2. **Create App Password**: 
   - Go to Google Account → Security → App Passwords
   - Generate new app password
3. **Update appsettings.json**:
   ```json
   "Email": {
     "SmtpServer": "smtp.gmail.com",
     "Port": 587,
     "UseSsl": true,
     "Username": "your_email@gmail.com",
     "Password": "your_16_character_app_password"
   }
   ```

### **Option 2: SendGrid (Professional)**
1. **Sign up**: [https://sendgrid.com/](https://sendgrid.com/)
2. **Get API Key**: Dashboard → Settings → API Keys
3. **Update appsettings.json**:
   ```json
   "Email": {
     "SmtpServer": "smtp.sendgrid.net",
     "Port": 587,
     "UseSsl": true,
     "Username": "apikey",
     "Password": "your_sendgrid_api_key"
   }
   ```

---

## 🛠️ **Setup Steps**

### **Step 1: Configure Backend**
1. Open `src/backend/MasterMind.API/appsettings.json`
2. Replace placeholder values with your actual API keys
3. Set `"UseSandbox": false` for production

### **Step 2: Test SMS**
1. Run the backend API
2. Try login with a mobile number
3. Check logs for SMS delivery status

### **Step 3: Test Email**
1. Try login with an email address
2. Check your inbox/spam folder
3. Verify email template looks good

### **Step 4: Update Frontend CORS**
Make sure your frontend URL is in allowed origins:
```json
"Cors": {
  "AllowedOrigins": "http://localhost:3000,http://localhost:3002,YOUR_DEPLOYED_URL"
}
```

---

## 🧪 **Testing the Configuration**

### **Test Mobile OTP**
```bash
# Test with Indian mobile number
curl -X POST http://localhost:5000/api/auth/otp/request \
  -H "Content-Type: application/json" \
  -d '{"identifier": "9876543210", "type": "mobile"}'
```

### **Test Email OTP**
```bash
# Test with email
curl -X POST http://localhost:5000/api/auth/otp/request \
  -H "Content-Type: application/json" \
  -d '{"identifier": "user@example.com", "type": "email"}'
```

---

## 📊 **Monitoring & Logs**

### **View SMS Logs**
- **Backend Console**: Check console output for SMS status
- **Fast2SMS Dashboard**: Monitor delivery reports
- **MSG91 Dashboard**: View delivery analytics

### **View Email Logs**
- **Backend Console**: Check email sending status
- **Gmail**: Check sent items folder
- **SendGrid**: View email analytics dashboard

---

## 🔒 **Security Best Practices**

### **API Keys**
- ✅ Store API keys in environment variables (not in code)
- ✅ Use different keys for development/production
- ✅ Regenerate keys periodically
- ✅ Monitor API usage and costs

### **Rate Limiting**
- ✅ Built-in: 5 requests per hour per number
- ✅ Cooldown: 60 seconds between requests
- ✅ Max attempts: 3 per OTP

### **OTP Security**
- ✅ 6-digit codes
- ✅ 5-minute expiry
- ✅ BCrypt hashing before storage
- ✅ One-time use only

---

## 🚨 **Troubleshooting**

### **SMS Not Working**
1. Check API key is correct
2. Verify mobile number format (10 digits for India)
3. Check provider account balance
4. Verify `"UseSandbox": false` for production

### **Email Not Working**
1. Check Gmail app password (not regular password)
2. Verify "Less secure app access" is enabled
3. Check SMTP settings and ports
4. Check firewall blocking port 587

### **Common Issues**
- **CORS errors**: Update allowed origins
- **Network timeouts**: Check internet connectivity
- **Invalid credentials**: Verify API keys and passwords

---

## 📈 **Production Deployment**

### **Environment Variables**
```bash
# Set environment variables for production
export SMS_API_KEY="your_production_sms_key"
export EMAIL_USERNAME="your_production_email"
export EMAIL_PASSWORD="your_production_app_password"
```

### **Docker Configuration**
```yaml
# docker-compose.yml environment variables
environment:
  - Sms__ApiKey=${SMS_API_KEY}
  - Email__Username=${EMAIL_USERNAME}
  - Email__Password=${EMAIL_PASSWORD}
  - Sms__UseSandbox=false
```

---

## 💰 **Cost Considerations**

### **SMS Costs**
- **Fast2SMS**: ~₹0.25 per SMS in India
- **MSG91**: ~₹0.20 per SMS globally
- **Bulk discounts**: Available for high volume

### **Email Costs**
- **Gmail**: Free (with daily limits)
- **SendGrid**: Free tier: 100 emails/day
- **AWS SES**: $0.10 per 1000 emails

---

## 🎯 **Quick Start Checklist**

- [ ] Sign up for SMS provider (Fast2SMS/MSG91)
- [ ] Get API keys and update appsettings.json
- [ ] Configure email provider (Gmail/SendGrid)
- [ ] Test SMS OTP with real mobile number
- [ ] Test email OTP with real email address
- [ ] Check logs for successful delivery
- [ ] Set `"UseSandbox": false` for production
- [ ] Update CORS settings for frontend URL
- [ ] Monitor usage and costs

---

## 📞 **Support**

If you need help:
- **Fast2SMS**: support@fast2sms.com
- **MSG91**: support@msg91.com
- **SendGrid**: support@sendgrid.com
- **Project Issues**: Check backend logs and contact support

---

**🎉 Your system is ready for real-time OTP delivery!**
