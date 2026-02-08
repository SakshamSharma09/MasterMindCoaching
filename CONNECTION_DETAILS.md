# SQL Server Connection Details

## 🔧 Required Information

To connect your application to SQL Server, you need these details:

### 📊 SQL Server Instance Information:
```
Server Name: [Your SQL Server Instance Name]
Port: 1433 (default)
Database Name: MasterMindCoaching
Authentication: SQL Server Authentication
Username: [Your SQL Server Username]
Password: [Your SQL Server Password]
```

### 🔍 How to Find Your SQL Server Details:

#### **Method 1: Using SQL Server Management Studio**
1. Open **SQL Server Management Studio**
2. Connect to your SQL Server instance
3. In Object Explorer, right-click on your server name
4. Select **Properties**
5. Look for **Server Name** in the General tab
6. Note down the server name (e.g., "localhost", "SQLEXPRESS", "PC-NAME\\SQLEXPRESS")

#### **Method 2: Using SQL Server Configuration Manager**
1. Open **SQL Server Configuration Manager**
2. Go to **SQL Server Services**
3. Find your SQL Server instance name
4. The instance name is usually displayed as "SQL Server (SQLEXPRESS)" or similar

#### **Method 3: Using Command Prompt**
```cmd
sqlcmd -L
```
This will list all SQL Server instances on your machine

## 📝 Configuration File Update

Update your `config/database.js` file with your actual details:

```javascript
module.exports = {
    server: {
        // Replace with your actual SQL Server instance name
        host: 'localhost',           // or 'SQLEXPRESS', 'PC-NAME\\SQLEXPRESS'
        port: 1433,
        database: 'MasterMindCoaching',
        user: 'your-username',     // Replace with your SQL username
        password: 'your-password',    // Replace with your SQL password
        options: {
            encrypt: false,
            trustServerCertificate: true,
            enableArithAbort: true,
            connectionTimeout: 60000,
            requestTimeout: 60000
        }
    }
    // ... rest of configuration
};
```

## 🚀 Common SQL Server Instance Names

### **Default Instances:**
- `localhost` (for default instance)
- `SQLEXPRESS` (for SQL Server Express)
- `PC-NAME\\SQLEXPRESS` (named instance)
- `localhost\\SQLEXPRESS` (local named instance)

### **Connection Examples:**

#### **Default Instance:**
```javascript
host: 'localhost',
user: 'sa',
password: 'YourStrongPassword123'
```

#### **SQL Server Express:**
```javascript
host: 'SQLEXPRESS',
user: 'sa',
password: 'YourStrongPassword123'
```

#### **Named Instance:**
```javascript
host: 'PC-NAME\\SQLEXPRESS',
user: 'sa',
password: 'YourStrongPassword123'
```

## 🔐 Security Recommendations

### **Strong Password Requirements:**
- Minimum 8 characters
- Mix of uppercase, lowercase, numbers, and special characters
- Example: `MasterMind@2024!`

### **User Permissions:**
- **db_owner** role for database operations
- **db_datareader** for read operations
- **db_datawriter** for write operations

## 🧪 Testing Connection

After updating the configuration, test the connection:

```bash
npm run setup
```

This will:
1. Create the database
2. Run all migrations
3. Import 45 students
4. Create admin user
5. Test the connection

## 📞 Troubleshooting

### **Common Connection Issues:**

#### **"Login failed for user 'sa'"**
- Check SQL Server Authentication is enabled
- Verify 'sa' account is enabled
- Confirm password is correct
- Check SQL Server is running

#### **"Cannot connect to server"**
- Verify SQL Server service is running
- Check firewall settings (port 1433)
- Confirm server name is correct
- Try IP address instead of hostname

#### **"Database does not exist"**
- Run the setup script first to create database
- Check database name spelling
- Verify user has CREATE DATABASE permissions

## 🎯 Quick Setup Commands

Once you have your SQL Server details:

```bash
# 1. Update configuration
# Edit config/database.js with your server details

# 2. Install dependencies (if not done)
npm install mssql bcrypt

# 3. Run complete setup
npm run setup

# 4. Start the application
npm start
```

## 📋 What You Need to Provide

Please share these details so I can update your configuration:

1. **SQL Server Instance Name:** _________________________
2. **SQL Server Username:** _________________________
3. **SQL Server Password:** _________________________
4. **Database Name:** MasterMindCoaching (or specify different)

Once you provide these details, I'll update your configuration file and the system will be ready to connect to your SQL Server!
