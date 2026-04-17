# MasterMind Coaching - Full API Reference

> **Auto-generated documentation for AI assistants**  
> Last Updated: 2026-04-17

## Overview

MasterMind Coaching is a full-stack coaching institute management system with:
- **Backend**: ASP.NET Core 9.0 Web API
- **Frontend**: Vue.js 3 + TypeScript + Vite
- **Database**: Azure SQL Database
- **Auth**: JWT + OTP (Email/SMS)

## Base URLs

| Environment | Backend API | Frontend |
|-------------|-------------|----------|
| Production | `https://mastermind-api-2404-eadxgpe5f7dch9f6.centralindia-01.azurewebsites.net` | `https://victorious-glacier-0e6507000.6.azurestaticapps.net` |
| Development | `http://localhost:5000` | `http://localhost:3000` |

---

## Authentication

### POST /api/auth/request-otp
Request OTP for login/registration.

**Request:**
```json
{
  "identifier": "user@example.com",
  "type": "email"
}
```

**Response:** `200 OK`
```json
{
  "success": true,
  "message": "OTP sent successfully",
  "expiresIn": 300
}
```

### POST /api/auth/verify-otp
Verify OTP and get JWT tokens.

**Request:**
```json
{
  "identifier": "user@example.com",
  "otp": "123456",
  "firstName": "John",
  "lastName": "Doe"
}
```

**Response:** `200 OK`
```json
{
  "success": true,
  "accessToken": "eyJhbG...",
  "refreshToken": "abc123...",
  "user": {
    "id": 1,
    "email": "user@example.com",
    "roles": ["Admin"]
  }
}
```

### POST /api/auth/quick-login
Quick login for demo accounts (bypasses OTP).

**Allowed Emails:** `admin@mastermind.com`, `teacher@mastermind.com`, `parent@mastermind.com`

**Request:**
```json
{
  "email": "admin@mastermind.com"
}
```

### POST /api/auth/login-password
Login with email and password (for admin users with set password).

**Request:**
```json
{
  "email": "admin@mastermind.com",
  "password": "securepassword"
}
```

### POST /api/auth/refresh-token
Refresh access token.

**Request:**
```json
{
  "refreshToken": "abc123..."
}
```

---

## Sessions (Academic Years)

### GET /api/sessions
Get all academic sessions.

**Auth Required:** Yes  
**Response:** `200 OK`
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "name": "2025-26",
      "displayName": "Academic Year 2025-26",
      "startDate": "2025-04-01",
      "endDate": "2026-03-31",
      "status": "Active",
      "isActive": true
    }
  ]
}
```

### POST /api/sessions
Create new session.

**Auth Required:** Yes  
**Request:**
```json
{
  "name": "2026-27",
  "displayName": "Academic Year 2026-27",
  "startDate": "2026-04-01",
  "endDate": "2027-03-31",
  "status": 1,
  "academicYear": "2026-27",
  "isActive": false
}
```

**Note:** `status` is an enum integer: `1=Planned, 2=Active, 3=Completed, 4=Suspended, 5=Cancelled`

### PUT /api/sessions/{id}/activate
Activate a session (deactivates others).

---

## Students

### GET /api/students
Get all students with optional filtering.

**Auth Required:** Yes  
**Query Params:** `classId`, `sessionId`, `search`

**Response:** `200 OK`
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "firstName": "John",
      "lastName": "Doe",
      "email": "john@example.com",
      "mobile": "+919876543210",
      "dateOfBirth": "2010-05-15",
      "gender": "Male",
      "address": "123 Main St",
      "guardianName": "Jane Doe",
      "guardianMobile": "+919876543211",
      "classes": [{ "id": 1, "name": "Class 10-A" }]
    }
  ]
}
```

### GET /api/students/{id}
Get student by ID.

### POST /api/students
Create new student.

**Request:**
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john@example.com",
  "mobile": "+919876543210",
  "dateOfBirth": "2010-05-15",
  "gender": "Male",
  "address": "123 Main St",
  "guardianName": "Jane Doe",
  "guardianMobile": "+919876543211",
  "classIds": [1]
}
```

### PUT /api/students/{id}
Update student.

### DELETE /api/students/{id}
Soft delete student.

---

## Teachers

### GET /api/teachers
Get all teachers.

**Auth Required:** Yes

### GET /api/teachers/{id}
Get teacher by ID.

### POST /api/teachers
Create new teacher.

**Request:**
```json
{
  "firstName": "Jane",
  "lastName": "Smith",
  "email": "jane@example.com",
  "mobile": "+919876543210",
  "qualification": "M.Sc Mathematics",
  "specialization": "Mathematics",
  "joiningDate": "2024-01-15",
  "salary": 50000
}
```

### PUT /api/teachers/{id}
Update teacher.

### DELETE /api/teachers/{id}
Soft delete teacher.

---

## Classes

### GET /api/classes
Get all classes.

**Auth Required:** Yes

### GET /api/classes/{id}
Get class by ID with students.

### POST /api/classes
Create new class.

**Request:**
```json
{
  "name": "Class 10-A",
  "description": "10th Grade Section A",
  "sessionId": 1,
  "teacherId": 1,
  "maxStudents": 40,
  "schedule": "Mon-Fri 9:00-14:00"
}
```

### PUT /api/classes/{id}
Update class.

### DELETE /api/classes/{id}
Soft delete class.

---

## Attendance

### GET /api/attendance
Get attendance records.

**Auth Required:** Yes  
**Query Params:** `classId`, `date`, `studentId`

### POST /api/attendance
Mark attendance for a class.

**Request:**
```json
{
  "classId": 1,
  "date": "2026-04-17",
  "records": [
    { "studentId": 1, "status": "Present" },
    { "studentId": 2, "status": "Absent" },
    { "studentId": 3, "status": "Late" }
  ]
}
```

### GET /api/attendance/student/{studentId}
Get attendance history for a student.

### GET /api/attendance/class/{classId}/summary
Get attendance summary for a class.

---

## Subjects

### GET /api/subjects
Get all subjects.

**Auth Required:** Yes

### POST /api/subjects
Create new subject.

**Request:**
```json
{
  "name": "Mathematics",
  "code": "MATH",
  "description": "Mathematics for all grades"
}
```

---

## Finance

### GET /api/fees
Get all student fees.

**Auth Required:** Yes  
**Query Params:** `classId`, `status`, `month`

### GET /api/feecollection
Get fee collection/payment records.

**Auth Required:** Yes

### POST /api/feecollection/setup-student-fee
Setup fee structure for a student.

### POST /api/feecollection/collect
Collect fee payment.

**Request:**
```json
{
  "studentFeeId": 1,
  "amount": 5000,
  "paymentMethod": "Cash",
  "remarks": "Monthly fee for April"
}
```

### GET /api/expenses
Get all expenses.

**Auth Required:** Yes  
**Query Params:** `category`, `startDate`, `endDate`

### POST /api/expenses
Create new expense.

**Request:**
```json
{
  "category": "Utilities",
  "description": "Electricity bill",
  "amount": 5000,
  "paidTo": "Electric Company",
  "expenseDate": "2026-04-15"
}
```

---

## Dashboard

### GET /api/dashboard
Get dashboard statistics.

**Auth Required:** Yes

**Response:** `200 OK`
```json
{
  "success": true,
  "data": {
    "totalStudents": 150,
    "totalTeachers": 10,
    "totalClasses": 15,
    "todayAttendance": 142,
    "pendingFees": 25000,
    "recentActivities": []
  }
}
```

---

## Health Check

### GET /health
Check API health status.

**Auth Required:** No

**Response:** `200 OK`
```json
{
  "status": "Healthy",
  "environment": "Production",
  "timestamp": "2026-04-17T10:30:00Z"
}
```

---

## Error Responses

All endpoints return consistent error format:

```json
{
  "success": false,
  "message": "Error description",
  "errorCode": "ERROR_CODE",
  "errors": ["Validation error 1", "Validation error 2"]
}
```

### Common HTTP Status Codes

| Code | Meaning |
|------|---------|
| 200 | Success |
| 201 | Created |
| 400 | Bad Request (validation error) |
| 401 | Unauthorized (missing/invalid token) |
| 403 | Forbidden (insufficient permissions) |
| 404 | Not Found |
| 500 | Internal Server Error |

---

## Authentication Header

All protected endpoints require:
```
Authorization: Bearer <access_token>
```

---

## File Locations

| Component | Path |
|-----------|------|
| Controllers | `src/backend/MasterMind.API/Controllers/` |
| Services | `src/backend/MasterMind.API/Services/` |
| Models | `src/backend/MasterMind.API/Models/` |
| DbContext | `src/backend/MasterMind.API/Data/MasterMindDbContext.cs` |
| Frontend Views | `src/frontend/mastermind-web/src/views/` |
| Frontend Services | `src/frontend/mastermind-web/src/services/` |
| Frontend Stores | `src/frontend/mastermind-web/src/stores/` |
