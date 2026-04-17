using FluentValidation;
using FluentValidation.AspNetCore;
using MasterMind.API.Data;
using MasterMind.API.Middleware;
using MasterMind.API.Models.Settings;
using MasterMind.API.Services.Implementations;
using MasterMind.API.Services.Interfaces;
using MasterMind.API.Validators;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container

// Configuration Settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.Configure<OtpSettings>(builder.Configuration.GetSection(OtpSettings.SectionName));
builder.Services.Configure<SmsSettings>(builder.Configuration.GetSection(SmsSettings.SectionName));

// Database Context - Support Azure SQL, PostgreSQL (Railway), and SQLite fallback
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
var azureConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");

Log.Information("DATABASE_URL environment variable: {DatabaseUrl}", 
    string.IsNullOrEmpty(databaseUrl) ? "NOT SET" : "SET");
Log.Information("DefaultConnection from config: {ConnStr}", 
    string.IsNullOrEmpty(azureConnectionString) ? "NOT SET" : "SET (length: " + azureConnectionString?.Length + ")");

if (!string.IsNullOrEmpty(databaseUrl) && (databaseUrl.StartsWith("postgres://") || databaseUrl.StartsWith("postgresql://")))
{
    // Railway PostgreSQL URL format
    Log.Information("Parsing PostgreSQL connection string from Railway format");
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':');
    var db = uri.AbsolutePath.TrimStart('/');
    if (string.IsNullOrEmpty(db)) db = "postgres";
    
    var connectionString = $"Host={uri.Host};Port={uri.Port};Username={userInfo[0]};Password={userInfo[1]};Database={db};SSL Mode=Require;Trust Server Certificate=true";
    Log.Information("Using PostgreSQL (Railway): Host={Host};Port={Port};Database={Database}", uri.Host, uri.Port, db);
    
    builder.Services.AddDbContext<MasterMindDbContext>(options =>
    {
        options.UseNpgsql(connectionString);
        options.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
    });
}
else if (!string.IsNullOrEmpty(azureConnectionString) && azureConnectionString.Contains("database.windows.net"))
{
    // Azure SQL Database - use SQL Server provider
    Log.Information("Using Azure SQL Database");
    builder.Services.AddDbContext<MasterMindDbContext>(options =>
    {
        options.UseSqlServer(azureConnectionString);
        options.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
    });
}
else if (!string.IsNullOrEmpty(azureConnectionString) && azureConnectionString.Contains("Server="))
{
    // Standard SQL Server connection string
    Log.Information("Using SQL Server connection string");
    builder.Services.AddDbContext<MasterMindDbContext>(options =>
    {
        options.UseSqlServer(azureConnectionString);
        options.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
    });
}
else if (!string.IsNullOrEmpty(azureConnectionString) && !azureConnectionString.Contains("localhost") && (azureConnectionString.Contains("Host=") || azureConnectionString.Contains("postgresql")))
{
    // PostgreSQL connection string (e.g. Azure PostgreSQL)
    Log.Information("Using configured PostgreSQL connection string");
    builder.Services.AddDbContext<MasterMindDbContext>(options =>
    {
        options.UseNpgsql(azureConnectionString);
        options.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
    });
}
else
{
    Log.Information("No cloud database configured, using SQLite as fallback");
    var connectionString = "Data Source=mastermind.db";
    
    builder.Services.AddDbContext<MasterMindDbContext>(options =>
        options.UseSqlite(connectionString));
}

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["Secret"];

// Use a default secret for development if not configured
if (string.IsNullOrEmpty(secretKey))
{
    Log.Warning("JWT Secret not configured in appsettings.json - using default development key. This is NOT secure for production!");
    secretKey = "ThisIsADefaultDevelopmentKeyThatShouldBeChangedInProduction1234567890";
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Append("Token-Expired", "true");
            }
            return Task.CompletedTask;
        }
    };
});

// Authorization Policies
builder.Services.AddAuthorization(options =>
{
    // Role-based policies
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("TeacherOnly", policy => policy.RequireRole("Teacher"));
    options.AddPolicy("ParentOnly", policy => policy.RequireRole("Parent"));
    
    // Combined role policies
    options.AddPolicy("AdminOrTeacher", policy => policy.RequireRole("Admin", "Teacher"));
    options.AddPolicy("Staff", policy => policy.RequireRole("Admin", "Teacher"));
    
    // Custom policies for specific features
    options.AddPolicy("CanManageStudents", policy => 
        policy.RequireRole("Admin", "Teacher"));
    options.AddPolicy("CanViewReports", policy => 
        policy.RequireRole("Admin", "Teacher", "Parent"));
    options.AddPolicy("CanManageFinance", policy => 
        policy.RequireRole("Admin"));
    options.AddPolicy("CanManageTeachers", policy => 
        policy.RequireRole("Admin"));
});

// Memory Cache for server-side caching
builder.Services.AddMemoryCache();

// Response Caching
builder.Services.AddResponseCaching();

// HTTP Context Accessor (needed for getting client IP)
builder.Services.AddHttpContextAccessor();

// HTTP Client for SMS service
builder.Services.AddHttpClient<ISmsService, SmsService>();

// Register Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddScoped<IFinanceService, FinanceService>();

// Azure Blob Storage for student photos
var blobConnectionString = builder.Configuration["AzureBlobStorage:ConnectionString"];
if (!string.IsNullOrEmpty(blobConnectionString))
{
    builder.Services.AddSingleton<IBlobStorageService, BlobStorageService>();
    Log.Information("Azure Blob Storage configured for student photos");
}
else
{
    Log.Warning("Azure Blob Storage not configured - photo uploads will not work");
}

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<OtpRequestValidator>();

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// CORS - Allow all origins in production for flexibility
var corsOrigins = builder.Configuration["Cors:AllowedOrigins"]?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        if (corsOrigins.Length > 0)
        {
            policy.WithOrigins(corsOrigins)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
                .WithExposedHeaders("Token-Expired");
        }
        else
        {
            // Fallback: Allow any origin if no origins configured (for production)
            policy.SetIsOriginAllowed(_ => true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
                .WithExposedHeaders("Token-Expired");
        }
    });
});

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MasterMind Coaching Classes API",
        Version = "v1",
        Description = "API for MasterMind Coaching Classes Management System - Complete Authentication System with OTP-based login, JWT tokens, and role-based access control.",
        Contact = new OpenApiContact
        {
            Name = "MasterMind Support",
            Email = "support@mastermind-coaching.com"
        }
    });

    // Add JWT Authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by a space and your JWT token.\n\nExample: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Include XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Configure Kestrel - use PORT env var for Azure/Railway, fallback to 5000
var port = Environment.GetEnvironmentVariable("PORT") ?? 
           Environment.GetEnvironmentVariable("WEBSITES_PORT") ?? "5000";
Log.Information("Configuring Kestrel to listen on port {Port}", port);
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(port));
});

var app = builder.Build();

// Configure the HTTP request pipeline

// Global Exception Handler
app.UseMiddleware<ExceptionMiddleware>();

// Swagger - enabled in all environments for API documentation
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "MasterMind API v1");
    options.RoutePrefix = "swagger";
    options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
});

// HTTPS Redirection
app.UseHttpsRedirection();

// CORS
app.UseCors("AllowFrontend");

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map Controllers
app.MapControllers();

// Health Check Endpoint
app.MapGet("/health", () => Results.Ok(new 
{ 
    Status = "Healthy", 
    Timestamp = DateTime.UtcNow,
    Version = "1.0.0",
    Environment = app.Environment.EnvironmentName
}));

// API Status Endpoint for Railway Health Check
app.MapGet("/api/status", () => Results.Ok(new 
{ 
    Status = "Healthy", 
    Timestamp = DateTime.UtcNow,
    Version = "1.0.0",
    Environment = app.Environment.EnvironmentName
}));

// API Info Endpoint
app.MapGet("/", () => Results.Ok(new
{
    Name = "MasterMind Coaching Classes API",
    Version = "1.0.0",
    Documentation = "/swagger",
    Health = "/health"
}));

// Apply Migrations and Seed Data on Startup (both Development and Production)
using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<MasterMindDbContext>();
try
{
    Log.Information("Ensuring database exists and creating tables if needed...");
    
    // Check if we're using PostgreSQL or SQLite
    var isPostgreSql = dbContext.Database.IsNpgsql();
    Log.Information("Database provider: {Provider}", isPostgreSql ? "PostgreSQL" : "SQLite");
    
    if (isPostgreSql)
    {
        // For PostgreSQL, create tables directly using raw SQL
        Log.Information("Creating PostgreSQL tables if they don't exist...");
        
        await dbContext.Database.ExecuteSqlRawAsync(@"
            -- =============================================
            -- USER MANAGEMENT TABLES
            -- =============================================
            
            -- Create Users table
            CREATE TABLE IF NOT EXISTS ""Users"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""Email"" VARCHAR(255),
                ""Mobile"" VARCHAR(20),
                ""FirstName"" VARCHAR(100) NOT NULL,
                ""LastName"" VARCHAR(100) NOT NULL,
                ""PasswordHash"" VARCHAR(500),
                ""ProfileImageUrl"" VARCHAR(500),
                ""IsActive"" BOOLEAN NOT NULL DEFAULT true,
                ""IsEmailVerified"" BOOLEAN NOT NULL DEFAULT false,
                ""IsMobileVerified"" BOOLEAN NOT NULL DEFAULT false,
                ""LastLoginAt"" TIMESTAMP,
                ""CreatedAt"" TIMESTAMP NOT NULL,
                ""UpdatedAt"" TIMESTAMP,
                ""IsDeleted"" BOOLEAN NOT NULL DEFAULT false
            );
            
            -- Create Roles table
            CREATE TABLE IF NOT EXISTS ""Roles"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""Name"" VARCHAR(50) NOT NULL,
                ""Description"" VARCHAR(200)
            );
            
            -- Create UserRole table (many-to-many)
            CREATE TABLE IF NOT EXISTS ""UserRoles"" (
                ""UserId"" INTEGER NOT NULL,
                ""RoleId"" INTEGER NOT NULL,
                ""AssignedAt"" TIMESTAMP NOT NULL,
                PRIMARY KEY (""UserId"", ""RoleId"")
            );
            
            -- Create OtpRecords table
            CREATE TABLE IF NOT EXISTS ""OtpRecords"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""Identifier"" VARCHAR(255) NOT NULL,
                ""OtpCode"" VARCHAR(255) NOT NULL,
                ""Type"" INTEGER NOT NULL,
                ""Purpose"" INTEGER NOT NULL,
                ""ExpiresAt"" TIMESTAMP NOT NULL,
                ""IsUsed"" BOOLEAN NOT NULL DEFAULT false,
                ""AttemptCount"" INTEGER NOT NULL DEFAULT 0,
                ""UserId"" INTEGER,
                ""CreatedAt"" TIMESTAMP NOT NULL,
                ""UpdatedAt"" TIMESTAMP
            );
            
            -- Create RefreshTokens table
            CREATE TABLE IF NOT EXISTS ""RefreshTokens"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""Token"" VARCHAR(500) NOT NULL,
                ""UserId"" INTEGER NOT NULL,
                ""ExpiresAt"" TIMESTAMP NOT NULL,
                ""IsRevoked"" BOOLEAN NOT NULL DEFAULT false,
                ""CreatedAt"" TIMESTAMP NOT NULL,
                ""RevokedAt"" TIMESTAMP
            );
            
            -- Create UserDevices table
            CREATE TABLE IF NOT EXISTS ""UserDevices"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""UserId"" INTEGER NOT NULL,
                ""DeviceId"" VARCHAR(200) NOT NULL,
                ""DeviceName"" VARCHAR(200),
                ""DeviceType"" VARCHAR(50),
                ""IsTrusted"" BOOLEAN NOT NULL DEFAULT false,
                ""LastUsedAt"" TIMESTAMP,
                ""CreatedAt"" TIMESTAMP NOT NULL
            );
            
            -- =============================================
            -- SESSION MANAGEMENT TABLES
            -- =============================================
            
            -- Create Sessions table
            CREATE TABLE IF NOT EXISTS ""Sessions"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""Name"" VARCHAR(50) NOT NULL,
                ""DisplayName"" VARCHAR(100) NOT NULL,
                ""Description"" VARCHAR(500),
                ""StartDate"" DATE NOT NULL,
                ""EndDate"" DATE NOT NULL,
                ""AcademicYear"" VARCHAR(20) NOT NULL,
                ""IsActive"" BOOLEAN NOT NULL DEFAULT false,
                ""Status"" INTEGER NOT NULL DEFAULT 1,
                ""TotalStudents"" INTEGER NOT NULL DEFAULT 0,
                ""ActiveStudents"" INTEGER NOT NULL DEFAULT 0,
                ""TotalClasses"" INTEGER NOT NULL DEFAULT 0,
                ""ActiveClasses"" INTEGER NOT NULL DEFAULT 0,
                ""TotalTeachers"" INTEGER NOT NULL DEFAULT 0,
                ""TotalRevenue"" DECIMAL(18,2) NOT NULL DEFAULT 0,
                ""TotalExpenses"" DECIMAL(18,2) NOT NULL DEFAULT 0,
                ""Settings"" TEXT,
                ""CreatedAt"" TIMESTAMP NOT NULL,
                ""UpdatedAt"" TIMESTAMP
            );
            
            -- =============================================
            -- STUDENT MANAGEMENT TABLES
            -- =============================================
            
            -- Create Students table
            CREATE TABLE IF NOT EXISTS ""Students"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""FirstName"" VARCHAR(100) NOT NULL,
                ""LastName"" VARCHAR(100) NOT NULL,
                ""DateOfBirth"" DATE,
                ""Gender"" INTEGER NOT NULL DEFAULT 0,
                ""Address"" VARCHAR(500),
                ""City"" VARCHAR(100),
                ""State"" VARCHAR(100),
                ""PinCode"" VARCHAR(20),
                ""StudentMobile"" VARCHAR(20),
                ""StudentEmail"" VARCHAR(255),
                ""ProfileImageUrl"" VARCHAR(500),
                ""AdmissionNumber"" VARCHAR(200),
                ""AdmissionDate"" DATE,
                ""IsActive"" BOOLEAN NOT NULL DEFAULT true,
                ""ParentName"" VARCHAR(200) NOT NULL,
                ""ParentMobile"" VARCHAR(20) NOT NULL,
                ""ParentEmail"" VARCHAR(255),
                ""ParentOccupation"" VARCHAR(200),
                ""ParentUserId"" INTEGER,
                ""SessionId"" INTEGER,
                ""CreatedAt"" TIMESTAMP NOT NULL,
                ""UpdatedAt"" TIMESTAMP,
                ""IsDeleted"" BOOLEAN NOT NULL DEFAULT false
            );
            
            -- Create Classes table
            CREATE TABLE IF NOT EXISTS ""Classes"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""Name"" VARCHAR(200) NOT NULL,
                ""Section"" VARCHAR(50),
                ""Medium"" VARCHAR(50),
                ""Board"" VARCHAR(100),
                ""AcademicYear"" VARCHAR(20),
                ""Description"" VARCHAR(500),
                ""MaxStudents"" INTEGER,
                ""StartTime"" TIME,
                ""EndTime"" TIME,
                ""DaysOfWeek"" VARCHAR(100),
                ""MonthlyFee"" DECIMAL(18,2),
                ""IsActive"" BOOLEAN NOT NULL DEFAULT true,
                ""SessionId"" INTEGER,
                ""CreatedAt"" TIMESTAMP NOT NULL,
                ""UpdatedAt"" TIMESTAMP,
                ""IsDeleted"" BOOLEAN NOT NULL DEFAULT false
            );
            
            -- Create Subjects table
            CREATE TABLE IF NOT EXISTS ""Subjects"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""Name"" VARCHAR(200) NOT NULL,
                ""Code"" VARCHAR(50),
                ""Description"" VARCHAR(1000),
                ""IsActive"" BOOLEAN NOT NULL DEFAULT true,
                ""CreatedAt"" TIMESTAMP NOT NULL,
                ""UpdatedAt"" TIMESTAMP
            );
            
            -- Create StudentClasses table (many-to-many)
            CREATE TABLE IF NOT EXISTS ""StudentClasses"" (
                ""StudentId"" INTEGER NOT NULL,
                ""ClassId"" INTEGER NOT NULL,
                ""IsActive"" BOOLEAN NOT NULL DEFAULT true,
                ""EnrollmentDate"" TIMESTAMP NOT NULL,
                PRIMARY KEY (""StudentId"", ""ClassId"")
            );
            
            -- Create ClassSubjects table (many-to-many)
            CREATE TABLE IF NOT EXISTS ""ClassSubjects"" (
                ""ClassId"" INTEGER NOT NULL,
                ""SubjectId"" INTEGER NOT NULL,
                ""TeacherAssigned"" VARCHAR(200),
                ""IsActive"" BOOLEAN NOT NULL DEFAULT true,
                ""CreatedAt"" TIMESTAMP NOT NULL,
                PRIMARY KEY (""ClassId"", ""SubjectId"")
            );
            
            -- =============================================
            -- TEACHER MANAGEMENT TABLES
            -- =============================================
            
            -- Create Teachers table
            CREATE TABLE IF NOT EXISTS ""Teachers"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""FirstName"" VARCHAR(100) NOT NULL,
                ""LastName"" VARCHAR(100) NOT NULL,
                ""Email"" VARCHAR(255) NOT NULL,
                ""Mobile"" VARCHAR(20) NOT NULL,
                ""Address"" VARCHAR(500),
                ""City"" VARCHAR(100),
                ""State"" VARCHAR(100),
                ""PinCode"" VARCHAR(20),
                ""Specialization"" VARCHAR(200),
                ""Qualification"" VARCHAR(500),
                ""Subjects"" VARCHAR(1000),
                ""JoiningDate"" DATE,
                ""Salary"" DECIMAL(18,2),
                ""IsActive"" BOOLEAN NOT NULL DEFAULT true,
                ""ProfileImageUrl"" VARCHAR(500),
                ""CreatedAt"" TIMESTAMP NOT NULL,
                ""UpdatedAt"" TIMESTAMP,
                ""IsDeleted"" BOOLEAN NOT NULL DEFAULT false
            );
            
            -- Create TeacherClasses table (many-to-many)
            CREATE TABLE IF NOT EXISTS ""TeacherClasses"" (
                ""TeacherId"" INTEGER NOT NULL,
                ""ClassId"" INTEGER NOT NULL,
                ""AssignedAt"" TIMESTAMP NOT NULL,
                PRIMARY KEY (""TeacherId"", ""ClassId"")
            );
            
            -- =============================================
            -- ATTENDANCE TABLES
            -- =============================================
            
            -- Create Attendances table
            CREATE TABLE IF NOT EXISTS ""Attendances"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""StudentId"" INTEGER NOT NULL,
                ""ClassId"" INTEGER NOT NULL,
                ""Date"" DATE NOT NULL,
                ""Status"" INTEGER NOT NULL,
                ""Remarks"" VARCHAR(500),
                ""MarkedBy"" INTEGER,
                ""CreatedAt"" TIMESTAMP NOT NULL
            );
            
            -- Create TeacherAttendances table
            CREATE TABLE IF NOT EXISTS ""TeacherAttendances"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""TeacherId"" INTEGER NOT NULL,
                ""Date"" DATE NOT NULL,
                ""Status"" INTEGER NOT NULL,
                ""CheckInTime"" TIME,
                ""CheckOutTime"" TIME,
                ""Remarks"" VARCHAR(500),
                ""MarkedBy"" INTEGER,
                ""CreatedAt"" TIMESTAMP NOT NULL
            );
            
            -- =============================================
            -- FEE MANAGEMENT TABLES
            -- =============================================
            
            -- Create FeeStructures table
            CREATE TABLE IF NOT EXISTS ""FeeStructures"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""ClassId"" INTEGER,
                ""SessionId"" INTEGER,
                ""Name"" VARCHAR(200) NOT NULL,
                ""Description"" VARCHAR(500),
                ""Amount"" DECIMAL(18,2) NOT NULL,
                ""Frequency"" INTEGER NOT NULL,
                ""DueDate"" DATE,
                ""IsActive"" BOOLEAN NOT NULL DEFAULT true,
                ""CreatedAt"" TIMESTAMP NOT NULL,
                ""UpdatedAt"" TIMESTAMP
            );
            
            -- Create StudentFees table
            CREATE TABLE IF NOT EXISTS ""StudentFees"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""StudentId"" INTEGER NOT NULL,
                ""FeeStructureId"" INTEGER,
                ""TotalAmount"" DECIMAL(18,2) NOT NULL,
                ""PaidAmount"" DECIMAL(18,2) NOT NULL DEFAULT 0,
                ""BalanceAmount"" DECIMAL(18,2) NOT NULL,
                ""Status"" INTEGER NOT NULL,
                ""DueDate"" DATE,
                ""CreatedAt"" TIMESTAMP NOT NULL,
                ""UpdatedAt"" TIMESTAMP
            );
            
            -- Create Payments table
            CREATE TABLE IF NOT EXISTS ""Payments"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""StudentFeeId"" INTEGER,
                ""StudentId"" INTEGER NOT NULL,
                ""Amount"" DECIMAL(18,2) NOT NULL,
                ""PaymentMethod"" VARCHAR(50) NOT NULL,
                ""TransactionId"" VARCHAR(200),
                ""ReceiptNumber"" VARCHAR(100),
                ""PaymentDate"" TIMESTAMP NOT NULL,
                ""Remarks"" VARCHAR(500),
                ""CollectedBy"" INTEGER,
                ""CreatedAt"" TIMESTAMP NOT NULL
            );
            
            -- Create FeeReceipts table
            CREATE TABLE IF NOT EXISTS ""FeeReceipts"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""ReceiptNumber"" VARCHAR(100) NOT NULL,
                ""StudentId"" INTEGER NOT NULL,
                ""PaymentId"" INTEGER,
                ""TotalAmount"" DECIMAL(18,2) NOT NULL,
                ""PaymentDate"" TIMESTAMP NOT NULL,
                ""PaymentMethod"" VARCHAR(50),
                ""Notes"" VARCHAR(500),
                ""CreatedBy"" INTEGER,
                ""CreatedAt"" TIMESTAMP NOT NULL
            );
            
            -- Create FeeReceiptItems table
            CREATE TABLE IF NOT EXISTS ""FeeReceiptItems"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""FeeReceiptId"" INTEGER NOT NULL,
                ""FeeName"" VARCHAR(200) NOT NULL,
                ""Amount"" DECIMAL(18,2) NOT NULL
            );
            
            -- Create FeePaymentSchedules table
            CREATE TABLE IF NOT EXISTS ""FeePaymentSchedules"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""StudentFeeId"" INTEGER NOT NULL,
                ""InstallmentNumber"" INTEGER NOT NULL,
                ""Amount"" DECIMAL(18,2) NOT NULL,
                ""DueDate"" DATE NOT NULL,
                ""PaidDate"" TIMESTAMP,
                ""Status"" INTEGER NOT NULL,
                ""CreatedAt"" TIMESTAMP NOT NULL
            );
            
            -- Create FeeInstallments table
            CREATE TABLE IF NOT EXISTS ""FeeInstallments"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""StudentFeeId"" INTEGER NOT NULL,
                ""InstallmentNumber"" INTEGER NOT NULL,
                ""Amount"" DECIMAL(18,2) NOT NULL,
                ""DueDate"" DATE NOT NULL,
                ""PaidAmount"" DECIMAL(18,2) NOT NULL DEFAULT 0,
                ""Status"" INTEGER NOT NULL,
                ""PaidDate"" TIMESTAMP,
                ""CreatedAt"" TIMESTAMP NOT NULL
            );
            
            -- =============================================
            -- EXPENSE MANAGEMENT TABLES
            -- =============================================
            
            -- Create Expenses table
            CREATE TABLE IF NOT EXISTS ""Expenses"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""Category"" VARCHAR(100) NOT NULL,
                ""Description"" VARCHAR(500) NOT NULL,
                ""Amount"" DECIMAL(18,2) NOT NULL,
                ""Date"" DATE NOT NULL,
                ""PaymentMethod"" VARCHAR(50),
                ""ReceiptNumber"" VARCHAR(100),
                ""Notes"" VARCHAR(500),
                ""RecordedBy"" INTEGER,
                ""SessionId"" INTEGER,
                ""CreatedAt"" TIMESTAMP NOT NULL
            );
            
            -- Create BudgetCategories table
            CREATE TABLE IF NOT EXISTS ""BudgetCategories"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""Name"" VARCHAR(100) NOT NULL,
                ""Description"" VARCHAR(500),
                ""BudgetAmount"" DECIMAL(18,2) NOT NULL,
                ""SpentAmount"" DECIMAL(18,2) NOT NULL DEFAULT 0,
                ""SessionId"" INTEGER,
                ""CreatedAt"" TIMESTAMP NOT NULL,
                ""UpdatedAt"" TIMESTAMP
            );
            
            -- =============================================
            -- LEAD MANAGEMENT TABLES
            -- =============================================
            
            -- Create Leads table
            CREATE TABLE IF NOT EXISTS ""Leads"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""Name"" VARCHAR(200) NOT NULL,
                ""Mobile"" VARCHAR(20) NOT NULL,
                ""Email"" VARCHAR(255),
                ""Source"" VARCHAR(100),
                ""Status"" INTEGER NOT NULL,
                ""ClassInterested"" VARCHAR(100),
                ""Notes"" VARCHAR(1000),
                ""AssignedTo"" INTEGER,
                ""FollowUpDate"" DATE,
                ""CreatedAt"" TIMESTAMP NOT NULL,
                ""UpdatedAt"" TIMESTAMP
            );
            
            -- Create LeadFollowups table
            CREATE TABLE IF NOT EXISTS ""LeadFollowups"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""LeadId"" INTEGER NOT NULL,
                ""Date"" DATE NOT NULL,
                ""Notes"" VARCHAR(1000) NOT NULL,
                ""NextFollowUpDate"" DATE,
                ""Status"" INTEGER NOT NULL,
                ""CreatedBy"" INTEGER,
                ""CreatedAt"" TIMESTAMP NOT NULL
            );
            
            -- =============================================
            -- PERFORMANCE & REMARKS TABLES
            -- =============================================
            
            -- Create StudentPerformances table
            CREATE TABLE IF NOT EXISTS ""StudentPerformances"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""StudentId"" INTEGER NOT NULL,
                ""SubjectId"" INTEGER,
                ""ExamName"" VARCHAR(200) NOT NULL,
                ""MaxMarks"" DECIMAL(18,2) NOT NULL,
                ""ObtainedMarks"" DECIMAL(18,2) NOT NULL,
                ""Percentage"" DECIMAL(5,2),
                ""Grade"" VARCHAR(10),
                ""ExamDate"" DATE,
                ""Remarks"" VARCHAR(500),
                ""CreatedAt"" TIMESTAMP NOT NULL
            );
            
            -- Create StudentRemarks table
            CREATE TABLE IF NOT EXISTS ""StudentRemarks"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""StudentId"" INTEGER NOT NULL,
                ""Remark"" VARCHAR(1000) NOT NULL,
                ""RemarkType"" INTEGER NOT NULL,
                ""GivenBy"" INTEGER,
                ""ClassId"" INTEGER,
                ""CreatedAt"" TIMESTAMP NOT NULL
            );
            
            -- =============================================
            -- TEACHER SALARY TABLES
            -- =============================================
            
            -- Create TeacherSalaries table
            CREATE TABLE IF NOT EXISTS ""TeacherSalaries"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""TeacherId"" INTEGER NOT NULL,
                ""Month"" INTEGER NOT NULL,
                ""Year"" INTEGER NOT NULL,
                ""BasicSalary"" DECIMAL(18,2) NOT NULL,
                ""Allowances"" DECIMAL(18,2) NOT NULL DEFAULT 0,
                ""Deductions"" DECIMAL(18,2) NOT NULL DEFAULT 0,
                ""NetSalary"" DECIMAL(18,2) NOT NULL,
                ""Status"" INTEGER NOT NULL,
                ""PaymentDate"" TIMESTAMP,
                ""Remarks"" VARCHAR(500),
                ""ProcessedBy"" INTEGER,
                ""CreatedAt"" TIMESTAMP NOT NULL
            );
            
            -- =============================================
            -- ADD MISSING COLUMNS TO EXISTING TABLES
            -- =============================================
            
            -- Add PasswordHash column if it doesn't exist (check both quoted and unquoted table names)
            DO $$ 
            BEGIN 
                -- Check for Users table (quoted identifier - preserves case)
                IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Users' AND column_name = 'PasswordHash') THEN
                    BEGIN
                        ALTER TABLE ""Users"" ADD COLUMN ""PasswordHash"" VARCHAR(500);
                        RAISE NOTICE 'Added PasswordHash column to Users table';
                    EXCEPTION WHEN OTHERS THEN
                        RAISE NOTICE 'Could not add PasswordHash to Users: %', SQLERRM;
                    END;
                END IF;
                
                -- Also check for users table (lowercase - PostgreSQL default)
                IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'users' AND column_name = 'PasswordHash') THEN
                    BEGIN
                        ALTER TABLE users ADD COLUMN ""PasswordHash"" VARCHAR(500);
                        RAISE NOTICE 'Added PasswordHash column to users table';
                    EXCEPTION WHEN OTHERS THEN
                        RAISE NOTICE 'Could not add PasswordHash to users: %', SQLERRM;
                    END;
                END IF;
            END $$;
            
            -- Ensure Email and Mobile columns have proper length
            DO $
            BEGIN
                -- Alter Email column to VARCHAR(450) if needed
                IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Users' AND column_name = 'Email' AND character_maximum_length < 450) THEN
                    ALTER TABLE ""Users"" ALTER COLUMN ""Email"" TYPE VARCHAR(450);
                END IF;
                IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'users' AND column_name = 'Email' AND character_maximum_length < 450) THEN
                    ALTER TABLE users ALTER COLUMN ""Email"" TYPE VARCHAR(450);
                END IF;
                
                -- Alter Mobile column to VARCHAR(450) if needed
                IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Users' AND column_name = 'Mobile' AND character_maximum_length < 450) THEN
                    ALTER TABLE ""Users"" ALTER COLUMN ""Mobile"" TYPE VARCHAR(450);
                END IF;
                IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'users' AND column_name = 'Mobile' AND character_maximum_length < 450) THEN
                    ALTER TABLE users ALTER COLUMN ""Mobile"" TYPE VARCHAR(450);
                END IF;
            END $;
            
            -- =============================================
            -- ADD IsDeleted COLUMN TO ALL TABLES (from BaseEntity)
            -- =============================================
            
            -- Add IsDeleted to OtpRecords table
            DO $
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'OtpRecords' AND column_name = 'IsDeleted') THEN
                    ALTER TABLE ""OtpRecords"" ADD COLUMN ""IsDeleted"" BOOLEAN NOT NULL DEFAULT false;
                END IF;
            END $;
            
            -- Add IsDeleted to RefreshTokens table and missing columns
            DO $
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'RefreshTokens' AND column_name = 'IsDeleted') THEN
                    ALTER TABLE ""RefreshTokens"" ADD COLUMN ""IsDeleted"" BOOLEAN NOT NULL DEFAULT false;
                END IF;
                IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'RefreshTokens' AND column_name = 'ReplacedByToken') THEN
                    ALTER TABLE ""RefreshTokens"" ADD COLUMN ""ReplacedByToken"" VARCHAR(500);
                END IF;
                IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'RefreshTokens' AND column_name = 'ReasonRevoked') THEN
                    ALTER TABLE ""RefreshTokens"" ADD COLUMN ""ReasonRevoked"" VARCHAR(500);
                END IF;
                IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'RefreshTokens' AND column_name = 'CreatedByIp') THEN
                    ALTER TABLE ""RefreshTokens"" ADD COLUMN ""CreatedByIp"" VARCHAR(45);
                END IF;
            END $;
            
            -- Add IsDeleted to Students table
            DO $
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Students' AND column_name = 'IsDeleted') THEN
                    ALTER TABLE ""Students"" ADD COLUMN ""IsDeleted"" BOOLEAN NOT NULL DEFAULT false;
                END IF;
            END $;
            
            -- Add IsDeleted to Classes table
            DO $
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Classes' AND column_name = 'IsDeleted') THEN
                    ALTER TABLE ""Classes"" ADD COLUMN ""IsDeleted"" BOOLEAN NOT NULL DEFAULT false;
                END IF;
            END $;
            
            -- Add IsDeleted to Teachers table
            DO $
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Teachers' AND column_name = 'IsDeleted') THEN
                    ALTER TABLE ""Teachers"" ADD COLUMN ""IsDeleted"" BOOLEAN NOT NULL DEFAULT false;
                END IF;
            END $;
            
            -- Add IsDeleted to StudentFees table
            DO $
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'StudentFees' AND column_name = 'IsDeleted') THEN
                    ALTER TABLE ""StudentFees"" ADD COLUMN ""IsDeleted"" BOOLEAN NOT NULL DEFAULT false;
                END IF;
            END $;
            
            -- Add IsDeleted to FeeStructures table
            DO $
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'FeeStructures' AND column_name = 'IsDeleted') THEN
                    ALTER TABLE ""FeeStructures"" ADD COLUMN ""IsDeleted"" BOOLEAN NOT NULL DEFAULT false;
                END IF;
            END $;
            
            -- Add IsDeleted to Leads table
            DO $
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Leads' AND column_name = 'IsDeleted') THEN
                    ALTER TABLE ""Leads"" ADD COLUMN ""IsDeleted"" BOOLEAN NOT NULL DEFAULT false;
                END IF;
            END $;
            
            -- Add IsDeleted to LeadFollowups table
            DO $
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'LeadFollowups' AND column_name = 'IsDeleted') THEN
                    ALTER TABLE ""LeadFollowups"" ADD COLUMN ""IsDeleted"" BOOLEAN NOT NULL DEFAULT false;
                END IF;
            END $;
            
            -- Add IsDeleted to StudentPerformances table
            DO $
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'StudentPerformances' AND column_name = 'IsDeleted') THEN
                    ALTER TABLE ""StudentPerformances"" ADD COLUMN ""IsDeleted"" BOOLEAN NOT NULL DEFAULT false;
                END IF;
            END $;
            
            -- Add IsDeleted to StudentRemarks table
            DO $
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'StudentRemarks' AND column_name = 'IsDeleted') THEN
                    ALTER TABLE ""StudentRemarks"" ADD COLUMN ""IsDeleted"" BOOLEAN NOT NULL DEFAULT false;
                END IF;
            END $;
            
            -- Add IsDeleted to Subjects table
            DO $
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Subjects' AND column_name = 'IsDeleted') THEN
                    ALTER TABLE ""Subjects"" ADD COLUMN ""IsDeleted"" BOOLEAN NOT NULL DEFAULT false;
                END IF;
            END $;
            
            -- Add missing columns to UserDevices table
            DO $
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'UserDevices' AND column_name = 'BrowserInfo') THEN
                    ALTER TABLE ""UserDevices"" ADD COLUMN ""BrowserInfo"" VARCHAR(100);
                END IF;
                IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'UserDevices' AND column_name = 'IpAddress') THEN
                    ALTER TABLE ""UserDevices"" ADD COLUMN ""IpAddress"" VARCHAR(45);
                END IF;
                IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'UserDevices' AND column_name = 'Location') THEN
                    ALTER TABLE ""UserDevices"" ADD COLUMN ""Location"" VARCHAR(100);
                END IF;
                IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'UserDevices' AND column_name = 'IsActive') THEN
                    ALTER TABLE ""UserDevices"" ADD COLUMN ""IsActive"" BOOLEAN NOT NULL DEFAULT true;
                END IF;
                IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'UserDevices' AND column_name = 'ExpiresAt') THEN
                    ALTER TABLE ""UserDevices"" ADD COLUMN ""ExpiresAt"" TIMESTAMP;
                END IF;
            END $;
            
            -- Add UserSessions table if it doesn't exist
            DO $
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'UserSessions') THEN
                    CREATE TABLE ""UserSessions"" (
                        ""Id"" SERIAL PRIMARY KEY,
                        ""UserDeviceId"" INTEGER NOT NULL,
                        ""Token"" VARCHAR(500) NOT NULL,
                        ""CreatedAt"" TIMESTAMP NOT NULL,
                        ""ExpiresAt"" TIMESTAMP NOT NULL,
                        ""IsActive"" BOOLEAN NOT NULL DEFAULT true,
                        ""LastActivityAt"" TIMESTAMP
                    );
                END IF;
            END $;
        ");
        
        Log.Information("PostgreSQL tables created/verified successfully");
    }
    else
    {
        // For SQLite, EnsureCreated works fine
        var created = await dbContext.Database.EnsureCreatedAsync();
        Log.Information("Database schema created/verified: {Created}", created);
    }
    
    // Seed initial data
    await SeedInitialDataAsync(dbContext);
    Log.Information("Database seeding completed successfully");
}
catch (Exception ex)
{
    Log.Error(ex, "An error occurred during database setup");
    // Don't throw - allow the API to start even if database setup fails
    // This helps with debugging connection issues
}

Log.Information("MasterMind Coaching Classes API started successfully");

app.Run();

// Seed initial data method
static async Task SeedInitialDataAsync(MasterMindDbContext context)
{
    // Seed Roles
    if (!await context.Roles.AnyAsync())
    {
        var roles = new[]
        {
            new MasterMind.API.Models.Entities.Role { Name = "Admin", Description = "Administrator with full access" },
            new MasterMind.API.Models.Entities.Role { Name = "Teacher", Description = "Teacher with limited access" },
            new MasterMind.API.Models.Entities.Role { Name = "Parent", Description = "Parent with read-only access" }
        };
        context.Roles.AddRange(roles);
        await context.SaveChangesAsync();
        Log.Information("Roles seeded successfully");
    }

    // Seed Admin User
    var adminExists = await context.Users.AnyAsync(u => u.Email == "admin@mastermind-coaching.com");
    if (!adminExists)
    {
        var adminUser = new MasterMind.API.Models.Entities.User
        {
            Email = "admin@mastermind-coaching.com",
            Mobile = "9999999999",
            FirstName = "Admin",
            LastName = "User",
            IsActive = true,
            IsEmailVerified = true,
            IsMobileVerified = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(adminUser);
        await context.SaveChangesAsync();

        // Assign Admin role
        var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        if (adminRole != null)
        {
            context.UserRoles.Add(new MasterMind.API.Models.Entities.UserRole
            {
                UserId = adminUser.Id,
                RoleId = adminRole.Id,
                AssignedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
        }

        Log.Information("Admin user seeded successfully");
    }

    // Seed Sample Session
    if (!await context.Sessions.AnyAsync())
    {
        var session = new MasterMind.API.Models.Entities.Session
        {
            Name = "2024-2025",
            DisplayName = "Academic Year 2024-25",
            StartDate = new DateTime(2024, 4, 1),
            EndDate = new DateTime(2025, 3, 31),
            AcademicYear = "2024-25",
            IsActive = true,
            Status = MasterMind.API.Models.Entities.SessionStatus.Active
        };
        context.Sessions.Add(session);
        await context.SaveChangesAsync();
        Log.Information("Session seeded successfully");
    }

    // Seed Sample Classes
    if (!await context.Classes.AnyAsync())
    {
        var classes = new[]
        {
            new MasterMind.API.Models.Entities.Class { Name = "Class 9 - Science", Medium = "English", Board = "CBSE", AcademicYear = "2024-25", IsActive = true, CreatedAt = DateTime.UtcNow },
            new MasterMind.API.Models.Entities.Class { Name = "Class 10 - Science", Medium = "English", Board = "CBSE", AcademicYear = "2024-25", IsActive = true, CreatedAt = DateTime.UtcNow },
            new MasterMind.API.Models.Entities.Class { Name = "Class 11 - Science", Medium = "English", Board = "CBSE", AcademicYear = "2024-25", IsActive = true, CreatedAt = DateTime.UtcNow },
            new MasterMind.API.Models.Entities.Class { Name = "Class 12 - Science", Medium = "English", Board = "CBSE", AcademicYear = "2024-25", IsActive = true, CreatedAt = DateTime.UtcNow }
        };
        context.Classes.AddRange(classes);
        await context.SaveChangesAsync();
        Log.Information("Classes seeded successfully");
    }

    // Seed Sample Subjects
    if (!await context.Subjects.AnyAsync())
    {
        var subjects = new[]
        {
            new MasterMind.API.Models.Entities.Subject { Name = "Mathematics", Code = "MATH", Description = "Mathematics for all classes", CreatedAt = DateTime.UtcNow },
            new MasterMind.API.Models.Entities.Subject { Name = "Physics", Code = "PHY", Description = "Physics for science stream", CreatedAt = DateTime.UtcNow },
            new MasterMind.API.Models.Entities.Subject { Name = "Chemistry", Code = "CHEM", Description = "Chemistry for science stream", CreatedAt = DateTime.UtcNow },
            new MasterMind.API.Models.Entities.Subject { Name = "Biology", Code = "BIO", Description = "Biology for science stream", CreatedAt = DateTime.UtcNow },
            new MasterMind.API.Models.Entities.Subject { Name = "English", Code = "ENG", Description = "English language", CreatedAt = DateTime.UtcNow }
        };
        context.Subjects.AddRange(subjects);
        await context.SaveChangesAsync();
        Log.Information("Subjects seeded successfully");
    }

    Log.Information("All initial data seeding completed");
}
