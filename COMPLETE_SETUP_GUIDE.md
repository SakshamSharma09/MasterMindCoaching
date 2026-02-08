# MasterMind Coaching - Complete Database Setup Guide

## 🎯 Overview

Your MasterMind Coaching application is now fully configured with SQL Server database connectivity. This guide will help you set up the complete database and start using your application.

## 📋 Prerequisites

- ✅ SQL Server Express (SQLEXPRESS) installed and running
- ✅ MasterMindCoaching database created
- ✅ Node.js and npm installed
- ✅ All project files ready

## 🗄️ Database Connection Configuration

Your application is configured with the following connection string:
```
Data Source=localhost\SQLEXPRESS;Initial Catalog=MasterMindCoaching;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Command Timeout=0
```

## 🚀 Quick Start Options

### Option 1: Start Application (JSON Fallback Mode)
```bash
npm start
# or
node app-server.js
```

### Option 2: Test Database Connection
```bash
npm run test-db
# or
node test-db-connection.js
```

### Option 3: Complete Database Setup
```bash
npm run setup-db
# or
node database-setup.js
```

## 📊 Database Setup Instructions

### Step 1: Create Database in SQL Server

1. Open **SQL Server Management Studio (SSMS)**
2. Connect to your SQL Server instance
3. Right-click on **Databases** → **New Database**
4. Name it `MasterMindCoaching`
5. Click **OK**

### Step 2: Run Database Setup Script

You have two options to set up the database:

#### Option A: Automatic Setup (if Windows Authentication works)
```bash
npm run setup-db
```

#### Option B: Manual SQL Script Execution

1. Open **SQL Server Management Studio**
2. Connect to the `MasterMindCoaching` database
3. Open the file `complete-database-import.sql`
4. Execute the script (Press F5 or click Execute)

The script will:
- ✅ Create the `students` table with proper schema
- ✅ Create performance indexes
- ✅ Insert all 45 students from your Excel data
- ✅ Verify the import with statistics

### Step 3: Verify Database Setup

Run this query in SSMS to verify:
```sql
SELECT COUNT(*) as TotalStudents FROM students;
SELECT TOP 5 * FROM students ORDER BY id;
```

## 🌐 Application Features

### API Endpoints
- `GET /api/students` - Get all student data
- `GET /api/statistics` - Get student statistics
- `GET /api/status` - Check database connection status

### Web Interface
- Main Application: http://localhost:3000
- Student viewer with search and filtering
- Real-time statistics dashboard

## 📱 Access Your Application

Once the server is running:

1. **Web Interface**: http://localhost:3000
2. **API Documentation**: http://localhost:3000/api/status
3. **Students Data**: http://localhost:3000/api/students
4. **Statistics**: http://localhost:3000/api/statistics

## 🔧 Troubleshooting

### Database Connection Issues

If you get "Login failed" errors:

1. **Check SQL Server Services**:
   - Open SQL Server Configuration Manager
   - Ensure SQL Server (SQLEXPRESS) is running
   - Enable TCP/IP protocol

2. **Windows Authentication**:
   - Ensure your Windows user has SQL Server permissions
   - Try running SSMS as Administrator

3. **Firewall Settings**:
   - Allow SQL Server through Windows Firewall
   - Port 1433 should be open

### Application Issues

1. **Port Already in Use**:
   ```bash
   netstat -ano | findstr :3000
   # Kill the process using the port
   ```

2. **Missing Dependencies**:
   ```bash
   npm install
   ```

3. **Test API Endpoints**:
   ```bash
   npm test
   # or
   node test-api.js
   ```

## 📊 Database Schema

The `students` table includes:
- **Basic Info**: first_name, last_name, email, phone
- **Academic Info**: class_name, current_school, standard, roll_number
- **Family Info**: mother_name, father_name, address
- **Contact Info**: whatsapp_number, text_number
- **Personal Info**: date_of_birth, gender, aadhar_number, caste
- **System Info**: status, created_at, photo

## 🎉 Success Indicators

Your setup is successful when:

- ✅ Server starts without errors
- ✅ API endpoints return data
- ✅ Web interface loads at http://localhost:3000
- ✅ Student data displays correctly
- ✅ Statistics show 45 total students
- ✅ Database connection shows "connected" in status

## 📞 Support Commands

```bash
# Test database connection
npm run test-db

# Check SQL Server availability
npm run check-sql

# Test API endpoints
npm test

# Process Excel data
npm run import-data

# Start application
npm start
```

## 🔄 Next Steps

1. **Explore the Web Interface**: Browse your student data
2. **Test the API**: Use the endpoints for integration
3. **Customize**: Modify the application to your needs
4. **Backup**: Set up regular database backups
5. **Deploy**: Consider production deployment options

---

## 🎯 Quick Summary

1. **Create Database**: `MasterMindCoaching` in SSMS
2. **Run SQL Script**: Execute `complete-database-import.sql`
3. **Start Application**: `npm start`
4. **Access**: http://localhost:3000

Your MasterMind Coaching application is now ready to use! 🚀
