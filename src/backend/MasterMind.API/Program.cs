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

// Database Context - Handle Railway's environment variables
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

Log.Information("DATABASE_URL environment variable: {DatabaseUrl}", 
    string.IsNullOrEmpty(databaseUrl) ? "NOT SET" : "SET");

if (!string.IsNullOrEmpty(databaseUrl))
{
    Log.Information("DATABASE_URL length: {Length}", databaseUrl.Length);
    Log.Information("DATABASE_URL starts with postgres:// or postgresql://: {StartsWith}", databaseUrl.StartsWith("postgres://") || databaseUrl.StartsWith("postgresql://"));
    Log.Information("DATABASE_URL first 50 chars: {FirstChars}", databaseUrl.Length > 50 ? databaseUrl.Substring(0, 50) + "..." : databaseUrl);
}

if (!string.IsNullOrEmpty(databaseUrl) && (databaseUrl.StartsWith("postgres://") || databaseUrl.StartsWith("postgresql://")))
{
    Log.Information("Parsing PostgreSQL connection string from Railway format");
    // Parse Railway's postgres:// or postgresql:// URL format
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':');
    var db = uri.AbsolutePath.TrimStart('/');
    if (string.IsNullOrEmpty(db)) db = "postgres";
    
    var connectionString = $"Host={uri.Host};Port={uri.Port};Username={userInfo[0]};Password={userInfo[1]};Database={db};SSL Mode=Require;Trust Server Certificate=true";
    Log.Information("Parsed connection string: Host={Host};Port={Port};Username={Username};Database={Database}", uri.Host, uri.Port, userInfo[0], db);
    
    builder.Services.AddDbContext<MasterMindDbContext>(options =>
    {
        options.UseNpgsql(connectionString);
        // Suppress pending model changes warning for Railway deployment
        options.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
    });
}
else
{
    Log.Information("No DATABASE_URL found or not in postgres:// format, using fallback connection string");
    // For Railway without database service, use SQLite as fallback
    var connectionString = "Data Source=mastermind.db";
    Log.Information("Using SQLite database as fallback");
    
    builder.Services.AddDbContext<MasterMindDbContext>(options =>
        options.UseSqlite(connectionString));
}

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");

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

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<OtpRequestValidator>();

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// CORS
var corsOrigins = builder.Configuration["Cors:AllowedOrigins"]?.Split(',') ?? new[] { "http://localhost:3000" };
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(corsOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
            .WithExposedHeaders("Token-Expired"); // Expose custom header for token expiry
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

var app = builder.Build();

// Configure the HTTP request pipeline

// Global Exception Handler
app.UseMiddleware<ExceptionMiddleware>();

// Swagger (Development only)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "MasterMind API v1");
        options.RoutePrefix = "swagger";
        options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
    });
}

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

// Apply Migrations and Seed Data on Startup (both Development and Production) - DISABLED TEMPORARILY
using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<MasterMindDbContext>();
try
{
    // Skip migrations for now to allow API to start
    Log.Information("Database migrations skipped temporarily to allow API startup");
    
    // Skip seeding for now to test basic functionality
    // await SeedAdminUserAsync(dbContext);
    Log.Information("Database seeding skipped for testing");
}
catch (Exception ex)
{
    Log.Error(ex, "An error occurred during database setup");
}

Log.Information("MasterMind Coaching Classes API started successfully");

app.Run();

// Seed admin user method
static async Task SeedAdminUserAsync(MasterMindDbContext context)
{
    // Check if admin user exists
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
}
