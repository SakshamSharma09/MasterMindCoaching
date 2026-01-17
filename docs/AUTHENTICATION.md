# Authentication System Documentation

## Overview

MasterMind Coaching Classes uses a modern OTP-based authentication system with JWT tokens for secure API access. The system supports:

- **OTP-based Login/Registration** via SMS (Fast2SMS/MSG91) and Email
- **JWT Access Tokens** for API authentication
- **Refresh Tokens** for seamless token renewal
- **Role-Based Access Control (RBAC)** with Admin, Teacher, and Parent roles

## Authentication Flow

### 1. Request OTP

```http
POST /api/auth/otp/request
Content-Type: application/json

{
  "identifier": "9876543210",
  "type": "mobile",
  "purpose": "login"
}
```

**Response:**
```json
{
  "success": true,
  "message": "OTP sent to your mobile",
  "maskedIdentifier": "****3210",
  "expirySeconds": 300,
  "isNewUser": false,
  "retryAfterSeconds": 60
}
```

### 2. Verify OTP (Login)

```http
POST /api/auth/otp/verify
Content-Type: application/json

{
  "identifier": "9876543210",
  "otp": "123456",
  "purpose": "login"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Authentication successful",
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "dGhpcyBpcyBhIHJlZnJlc2ggdG9rZW4...",
  "expiresAt": "2026-01-17T07:45:50.548Z",
  "user": {
    "id": 1,
    "email": "user@example.com",
    "mobile": "9876543210",
    "firstName": "John",
    "lastName": "Doe",
    "fullName": "John Doe",
    "isActive": true,
    "isEmailVerified": true,
    "isMobileVerified": true,
    "roles": ["Parent"],
    "createdAt": "2026-01-01T00:00:00Z"
  }
}
```

### 3. Verify OTP (Registration)

For new users, include registration details:

```http
POST /api/auth/otp/verify
Content-Type: application/json

{
  "identifier": "9876543210",
  "otp": "123456",
  "purpose": "registration",
  "registrationDetails": {
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@example.com",
    "role": "Parent"
  }
}
```

### 4. Refresh Token

```http
POST /api/auth/token/refresh
Content-Type: application/json

{
  "refreshToken": "dGhpcyBpcyBhIHJlZnJlc2ggdG9rZW4..."
}
```

### 5. Logout

```http
POST /api/auth/logout
Authorization: Bearer <access_token>
Content-Type: application/json

{
  "refreshToken": "dGhpcyBpcyBhIHJlZnJlc2ggdG9rZW4..."
}
```

### 6. Logout from All Devices

```http
POST /api/auth/logout/all
Authorization: Bearer <access_token>
```

### 7. Get Current User

```http
GET /api/auth/me
Authorization: Bearer <access_token>
```

### 8. Check Authentication

```http
GET /api/auth/check
Authorization: Bearer <access_token>
```

## Using Access Tokens

Include the JWT token in the Authorization header for all protected endpoints:

```http
GET /api/protected-endpoint
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

## Role-Based Access Control

### Available Roles

| Role | Description | Permissions |
|------|-------------|-------------|
| Admin | Full system access | All operations |
| Teacher | Limited access | Manage students, attendance, remarks |
| Parent | View-only access | View student info, attendance, fees |

### Authorization Policies

| Policy | Allowed Roles |
|--------|---------------|
| AdminOnly | Admin |
| TeacherOnly | Teacher |
| ParentOnly | Parent |
| AdminOrTeacher | Admin, Teacher |
| Staff | Admin, Teacher |
| CanManageStudents | Admin, Teacher |
| CanViewReports | Admin, Teacher, Parent |
| CanManageFinance | Admin |
| CanManageTeachers | Admin |

### Using Policies in Controllers

```csharp
[Authorize(Policy = "AdminOnly")]
public async Task<IActionResult> AdminOnlyEndpoint()
{
    // Only admins can access
}

[Authorize(Policy = "CanManageStudents")]
public async Task<IActionResult> ManageStudents()
{
    // Admins and Teachers can access
}
```

## Configuration

### appsettings.json

```json
{
  "Jwt": {
    "Secret": "YourSecretKeyHere-MustBeAtLeast32Characters",
    "Issuer": "MasterMindCoaching",
    "Audience": "MasterMindCoaching",
    "ExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 7
  },
  "Otp": {
    "ExpiryMinutes": 5,
    "Length": 6,
    "MaxAttempts": 3,
    "ResendCooldownSeconds": 60,
    "MaxRequestsPerHour": 5
  },
  "Sms": {
    "Provider": "Fast2SMS",
    "ApiKey": "your-api-key",
    "SenderId": "MSTMND",
    "TemplateId": "",
    "Route": "otp",
    "UseSandbox": true,
    "DefaultCountryCode": "91",
    "OtpMessageTemplate": "Your MasterMind Coaching verification code is {otp}. Valid for 5 minutes."
  }
}
```

### SMS Providers

#### Fast2SMS

1. Sign up at [Fast2SMS](https://www.fast2sms.com/)
2. Get your API key from the dashboard
3. Set `Provider` to `"Fast2SMS"`
4. Set `Route` to `"otp"` for OTP messages

#### MSG91

1. Sign up at [MSG91](https://msg91.com/)
2. Get your API key and create a template
3. Set `Provider` to `"MSG91"`
4. Set `TemplateId` to your DLT-approved template ID

### Sandbox Mode

Set `UseSandbox: true` in SMS settings to test without sending actual SMS. OTPs will be logged to the console.

## Security Features

1. **OTP Hashing**: OTPs are hashed using BCrypt before storage
2. **Rate Limiting**: Maximum 5 OTP requests per hour per identifier
3. **Cooldown Period**: 60 seconds between OTP requests
4. **Max Attempts**: 3 verification attempts per OTP
5. **Token Rotation**: Refresh tokens are rotated on each use
6. **Token Revocation**: All tokens can be revoked on logout
7. **IP Tracking**: Client IP is recorded for security auditing

## Error Codes

| Code | Description |
|------|-------------|
| INVALID_OTP | OTP is incorrect or expired |
| USER_NOT_FOUND | User does not exist |
| ACCOUNT_DEACTIVATED | User account is deactivated |
| REGISTRATION_DETAILS_REQUIRED | Missing registration info |
| INVALID_REFRESH_TOKEN | Refresh token is invalid |
| TOKEN_EXPIRED | Token has expired |
| AUTH_ERROR | General authentication error |
| VALIDATION_ERROR | Request validation failed |

## Database Schema

### Users Table
- Id, Email, Mobile, FirstName, LastName
- PasswordHash (optional, for future password auth)
- IsActive, IsEmailVerified, IsMobileVerified
- LastLoginAt, ProfileImageUrl
- CreatedAt, UpdatedAt, IsDeleted

### Roles Table
- Id, Name, Description
- Seeded: Admin, Teacher, Parent

### UserRoles Table (Junction)
- UserId, RoleId, AssignedAt

### OtpRecords Table
- Id, Identifier, OtpCode (hashed)
- Type (Email/Mobile), Purpose
- ExpiresAt, IsUsed, AttemptCount
- UserId (optional)

### RefreshTokens Table
- Id, Token, ExpiresAt
- IsRevoked, RevokedAt, ReplacedByToken
- ReasonRevoked, CreatedByIp, RevokedByIp
- UserId

## Testing

### Default Admin User

On first run in development mode, an admin user is seeded:
- Email: admin@mastermind-coaching.com
- Mobile: 9999999999
- Role: Admin

### Swagger UI

Access Swagger at `/swagger` to test API endpoints interactively.

1. Request OTP for the admin mobile
2. Check console logs for OTP (sandbox mode)
3. Verify OTP to get tokens
4. Click "Authorize" and enter: `Bearer <your_access_token>`
5. Test protected endpoints
