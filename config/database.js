// SQL Server Configuration for MasterMind Coaching
// Using Windows Integrated Authentication

const sql = require('mssql');

// Database connection configuration using the provided connection string
const dbConfig = {
    server: 'localhost\\SQLEXPRESS',
    database: 'MasterMindCoaching',
    options: {
        encrypt: true,
        trustServerCertificate: true,
        enableArithAbort: true,
        connectionTimeout: 60000,
        requestTimeout: 60000,
        useUTC: false,
        trustedConnection: true
    },
    pool: {
        max: 10,
        min: 0,
        idleTimeoutMillis: 30000
    }
};

// Database connection pool
let pool;

// Initialize database connection
async function connectToDatabase() {
    try {
        if (pool) {
            return pool;
        }
        
        pool = await sql.connect(dbConfig);
        console.log('✅ Connected to SQL Server database successfully');
        return pool;
    } catch (error) {
        console.error('❌ Database connection failed:', error);
        throw error;
    }
}

// Close database connection
async function closeConnection() {
    try {
        if (pool) {
            await pool.close();
            pool = null;
            console.log('✅ Database connection closed');
        }
    } catch (error) {
        console.error('❌ Error closing database connection:', error);
        throw error;
    }
}

// Execute query with error handling
async function executeQuery(query, params = []) {
    try {
        const connection = await connectToDatabase();
        const request = connection.request();
        
        // Add parameters if provided
        params.forEach((param, index) => {
            request.input(`param${index}`, param);
        });
        
        const result = await request.query(query);
        return result.recordset;
    } catch (error) {
        console.error('❌ Query execution failed:', error);
        throw error;
    }
}

// Test database connection
async function testConnection() {
    try {
        const connection = await connectToDatabase();
        const result = await connection.request().query('SELECT @@VERSION as version, DB_NAME() as database');
        console.log('🔍 Database test successful:', result.recordset[0]);
        return true;
    } catch (error) {
        console.error('❌ Database test failed:', error);
        return false;
    }
}

module.exports = {
    config: dbConfig,
    sql,
    connectToDatabase,
    closeConnection,
    executeQuery,
    testConnection
};
