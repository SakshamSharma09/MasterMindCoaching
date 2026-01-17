# The MasterMind Coaching Classes - System Architecture

## Overview

This document describes the technical architecture of the MasterMind Coaching Classes application, a comprehensive coaching institute management system.

---

## Technology Stack

### Backend
| Component | Technology | Version |
|-----------|------------|---------|
| Framework | ASP.NET Core | .NET 10 |
| ORM | Entity Framework Core | 9.x |
| Database | PostgreSQL | 16.x |
| Authentication | JWT + OTP | - |
| API Documentation | Swagger/OpenAPI | 3.0 |
| Logging | Serilog | - |

### Frontend
| Component | Technology | Version |
|-----------|------------|---------|
| Framework | Vue.js | 3.x |
| Build Tool | Vite | 5.x |
| Language | TypeScript | 5.x |
| State Management | Pinia | 2.x |
| HTTP Client | Axios | 1.x |
| Styling | Tailwind CSS | 3.x |
| Router | Vue Router | 4.x |

### Infrastructure
| Component | Technology |
|-----------|------------|
| Containerization | Docker |
| Orchestration | Docker Compose |
| Version Control | Git/GitHub |
| CI/CD | GitHub Actions |

---

## Architecture Pattern

### Backend: Clean Architecture

```
┌────────────────────────────────────────────────────────────┐
│                    Presentation Layer                       │
│                    (API Controllers)                        │
└────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌────────────────────────────────────────────────────────────┐
│                   Application Layer                         │
│              (Services, DTOs, Interfaces)                   │
└────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌────────────────────────────────────────────────────────────┐
│                     Domain Layer                            │
│              (Entities, Business Logic)                     │
└────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌────────────────────────────────────────────────────────────┐
│                  Infrastructure Layer                       │
│         (Database, External Services, Repositories)         │
└────────────────────────────────────────────────────────────┘
```

### Frontend: Component-Based Architecture

```
┌────────────────────────────────────────────────────────────┐
│                        Views                                │
│              (Page-level components)                        │
└────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌────────────────────────────────────────────────────────────┐
│                      Components                             │
│              (Reusable UI components)                       │
└────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌────────────────────────────────────────────────────────────┐
│                        Stores                               │
│              (Pinia state management)                       │
└────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌────────────────────────────────────────────────────────────┐
│                       Services                              │
│              (API communication layer)                      │
└────────────────────────────────────────────────────────────┘
```

---

## Database Schema

### Entity Relationship Diagram

```
┌─────────────────┐       ┌─────────────────┐
│     Users       │       │     Roles       │
├─────────────────┤       ├─────────────────┤
│ Id (PK)         │──────<│ Id (PK)         │
│ Email           │       │ Name            │
│ Mobile          │       │ Description     │
│ PasswordHash    │       └─────────────────┘
│ FirstName       │
│ LastName        │       ┌─────────────────┐
│ RoleId (FK)     │>──────│   UserRoles     │
│ IsActive        │       ├─────────────────┤
│ CreatedAt       │       │ UserId (FK)     │
│ UpdatedAt       │       │ RoleId (FK)     │
└─────────────────┘       └─────────────────┘
         │
         │
         ▼
┌─────────────────┐       ┌─────────────────┐
│    Students     │       │    Classes      │
├─────────────────┤       ├─────────────────┤
│ Id (PK)         │       │ Id (PK)         │
│ FirstName       │       │ Name            │
│ LastName        │       │ Section         │
│ DateOfBirth     │       │ AcademicYear    │
│ Gender          │       │ IsActive        │
│ Address         │       └─────────────────┘
│ ParentUserId(FK)│               │
│ CreatedAt       │               │
│ UpdatedAt       │               ▼
└─────────────────┘       ┌─────────────────┐
         │                │ StudentClasses  │
         │                ├─────────────────┤
         └───────────────>│ StudentId (FK)  │
                          │ ClassId (FK)    │
                          │ EnrollmentDate  │
                          └─────────────────┘

┌─────────────────┐       ┌─────────────────┐
│   Attendance    │       │TeacherAttendance│
├─────────────────┤       ├─────────────────┤
│ Id (PK)         │       │ Id (PK)         │
│ StudentId (FK)  │       │ TeacherId (FK)  │
│ ClassId (FK)    │       │ Date            │
│ Date            │       │ Status          │
│ Status          │       │ Remarks         │
│ Remarks         │       └─────────────────┘
└─────────────────┘

┌─────────────────┐       ┌─────────────────┐
│   FeeStructure  │       │   StudentFees   │
├─────────────────┤       ├─────────────────┤
│ Id (PK)         │       │ Id (PK)         │
│ ClassId (FK)    │       │ StudentId (FK)  │
│ FeeType         │       │ FeeStructureId  │
│ Amount          │       │ DueDate         │
│ Frequency       │       │ Status          │
└─────────────────┘       └─────────────────┘
         │                        │
         │                        ▼
         │                ┌─────────────────┐
         │                │    Payments     │
         │                ├─────────────────┤
         └───────────────>│ Id (PK)         │
                          │ StudentFeeId(FK)│
                          │ Amount          │
                          │ PaymentDate     │
                          │ PaymentMethod   │
                          │ TransactionId   │
                          └─────────────────┘

┌─────────────────┐       ┌─────────────────┐
│     Leads       │       │  LeadFollowups  │
├─────────────────┤       ├─────────────────┤
│ Id (PK)         │       │ Id (PK)         │
│ Name            │       │ LeadId (FK)     │
│ Mobile          │       │ FollowupDate    │
│ Email           │       │ Notes           │
│ Source          │       │ NextFollowup    │
│ Status          │       │ Status          │
│ InterestedClass │       └─────────────────┘
│ CreatedAt       │
└─────────────────┘

┌─────────────────┐       ┌─────────────────┐
│ StudentRemarks  │       │StudentPerformance│
├─────────────────┤       ├─────────────────┤
│ Id (PK)         │       │ Id (PK)          │
│ StudentId (FK)  │       │ StudentId (FK)   │
│ TeacherId (FK)  │       │ SubjectId (FK)   │
│ ChapterName     │       │ TestName         │
│ Remarks         │       │ Score            │
│ Date            │       │ MaxScore         │
└─────────────────┘       │ Date             │
                          └─────────────────┘
```

---

## API Structure

### Base URL
```
Development: http://localhost:5000/api
Production: https://api.mastermind-coaching.com/api
```

### API Endpoints by Module

#### Authentication Module
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | /auth/request-otp | Request OTP for login |
| POST | /auth/verify-otp | Verify OTP and get token |
| POST | /auth/refresh-token | Refresh JWT token |
| POST | /auth/logout | Logout user |
| GET | /auth/me | Get current user info |

#### Student Module
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /students | Get all students |
| GET | /students/{id} | Get student by ID |
| POST | /students | Create new student |
| PUT | /students/{id} | Update student |
| DELETE | /students/{id} | Delete student |
| GET | /students/class/{classId} | Get students by class |

#### Class Module
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /classes | Get all classes |
| GET | /classes/{id} | Get class by ID |
| POST | /classes | Create new class |
| PUT | /classes/{id} | Update class |
| DELETE | /classes/{id} | Delete class |

#### Attendance Module
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /attendance | Get attendance records |
| POST | /attendance | Mark attendance |
| PUT | /attendance/{id} | Update attendance |
| GET | /attendance/student/{id} | Get student attendance |
| GET | /attendance/class/{id}/date/{date} | Get class attendance |
| GET | /attendance/report | Get attendance report |

#### Finance Module
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /fees | Get fee structures |
| POST | /fees | Create fee structure |
| GET | /fees/student/{id} | Get student fees |
| POST | /payments | Record payment |
| GET | /payments/student/{id} | Get payment history |
| GET | /finance/report | Get financial report |

#### Teacher Module
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /teachers | Get all teachers |
| GET | /teachers/{id} | Get teacher by ID |
| POST | /teachers | Create teacher |
| PUT | /teachers/{id} | Update teacher |
| GET | /teachers/{id}/students | Get teacher's students |
| POST | /teachers/remarks | Add student remarks |

#### Lead Module
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /leads | Get all leads |
| POST | /leads | Create new lead |
| PUT | /leads/{id} | Update lead |
| POST | /leads/{id}/followup | Add followup |
| GET | /leads/report | Get lead report |

---

## Security Architecture

### Authentication Flow

```
┌──────────┐     ┌──────────┐     ┌──────────┐     ┌──────────┐
│  Client  │     │   API    │     │OTP Service│    │ Database │
└────┬─────┘     └────┬─────┘     └────┬─────┘     └────┬─────┘
     │                │                │                │
     │ 1. Request OTP │                │                │
     │───────────────>│                │                │
     │                │ 2. Generate OTP│                │
     │                │───────────────>│                │
     │                │                │ 3. Store OTP   │
     │                │                │───────────────>│
     │                │ 4. Send OTP    │                │
     │<───────────────│ (Email/SMS)    │                │
     │                │                │                │
     │ 5. Submit OTP  │                │                │
     │───────────────>│                │                │
     │                │ 6. Verify OTP  │                │
     │                │───────────────>│                │
     │                │                │ 7. Validate    │
     │                │                │───────────────>│
     │                │ 8. Generate JWT│                │
     │<───────────────│                │                │
     │                │                │                │
```

### JWT Token Structure
```json
{
  "sub": "user-id",
  "email": "user@example.com",
  "role": "Admin",
  "permissions": ["read:students", "write:students"],
  "exp": 1234567890,
  "iat": 1234567890
}
```

### Role-Based Access Control

| Role | Permissions |
|------|-------------|
| Admin | Full access to all modules |
| Teacher | Read students, Write remarks, Read own attendance |
| Parent | Read own child's data only |

---

## Deployment Architecture

### Docker Compose Setup

```
┌─────────────────────────────────────────────────────────────┐
│                     Docker Network                           │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐          │
│  │   nginx     │  │   api       │  │  postgres   │          │
│  │  (reverse   │  │  (.NET 10)  │  │  (database) │          │
│  │   proxy)    │  │             │  │             │          │
│  │  Port: 80   │  │  Port: 5000 │  │  Port: 5432 │          │
│  └──────┬──────┘  └──────┬──────┘  └──────┬──────┘          │
│         │                │                │                  │
│         └────────────────┴────────────────┘                  │
│                          │                                   │
│  ┌─────────────┐         │                                   │
│  │   web       │─────────┘                                   │
│  │  (Vue.js)   │                                             │
│  │  Port: 3000 │                                             │
│  └─────────────┘                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## Folder Structure

### Backend (.NET 10)

```
MasterMind.API/
├── Controllers/
│   ├── AuthController.cs
│   ├── StudentsController.cs
│   ├── ClassesController.cs
│   ├── AttendanceController.cs
│   ├── FinanceController.cs
│   ├── TeachersController.cs
│   └── LeadsController.cs
├── Services/
│   ├── Interfaces/
│   │   ├── IAuthService.cs
│   │   ├── IStudentService.cs
│   │   └── ...
│   └── Implementations/
│       ├── AuthService.cs
│       ├── StudentService.cs
│       └── ...
├── Models/
│   ├── Entities/
│   │   ├── User.cs
│   │   ├── Student.cs
│   │   └── ...
│   ├── DTOs/
│   │   ├── Auth/
│   │   ├── Student/
│   │   └── ...
│   └── Enums/
├── Data/
│   ├── ApplicationDbContext.cs
│   ├── Configurations/
│   └── Migrations/
├── Middleware/
│   ├── ExceptionMiddleware.cs
│   └── JwtMiddleware.cs
├── Helpers/
│   ├── JwtHelper.cs
│   └── OtpHelper.cs
├── appsettings.json
├── appsettings.Development.json
└── Program.cs
```

### Frontend (Vue.js 3)

```
mastermind-web/
├── src/
│   ├── assets/
│   │   ├── images/
│   │   └── styles/
│   ├── components/
│   │   ├── common/
│   │   │   ├── Button.vue
│   │   │   ├── Input.vue
│   │   │   └── Modal.vue
│   │   ├── layout/
│   │   │   ├── Header.vue
│   │   │   ├── Sidebar.vue
│   │   │   └── Footer.vue
│   │   └── modules/
│   │       ├── students/
│   │       ├── attendance/
│   │       └── finance/
│   ├── views/
│   │   ├── auth/
│   │   │   ├── LoginView.vue
│   │   │   └── OtpVerifyView.vue
│   │   ├── admin/
│   │   │   ├── DashboardView.vue
│   │   │   ├── StudentsView.vue
│   │   │   └── ...
│   │   ├── teacher/
│   │   └── parent/
│   ├── stores/
│   │   ├── auth.ts
│   │   ├── students.ts
│   │   └── ...
│   ├── services/
│   │   ├── api.ts
│   │   ├── authService.ts
│   │   └── ...
│   ├── router/
│   │   └── index.ts
│   ├── types/
│   │   └── index.ts
│   ├── utils/
│   │   └── helpers.ts
│   ├── App.vue
│   └── main.ts
├── public/
├── index.html
├── vite.config.ts
├── tailwind.config.js
├── tsconfig.json
└── package.json
```

---

## Environment Configuration

### Backend (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=mastermind;Username=postgres;Password=password"
  },
  "Jwt": {
    "Secret": "your-secret-key",
    "Issuer": "MasterMindCoaching",
    "Audience": "MasterMindCoaching",
    "ExpiryMinutes": 60
  },
  "Otp": {
    "ExpiryMinutes": 5,
    "Length": 6
  },
  "Email": {
    "SmtpServer": "smtp.gmail.com",
    "Port": 587,
    "Username": "",
    "Password": ""
  }
}
```

### Frontend (.env)
```
VITE_API_URL=http://localhost:5000/api
VITE_APP_NAME=MasterMind Coaching Classes
```

---

## Performance Considerations

1. **Database Indexing**: Index frequently queried columns
2. **Pagination**: All list endpoints support pagination
3. **Caching**: Redis for session and frequently accessed data
4. **Lazy Loading**: Frontend components loaded on demand
5. **API Response Compression**: Gzip compression enabled

---

## Future Enhancements

1. **Mobile App**: React Native or Flutter
2. **Real-time Notifications**: SignalR integration
3. **SMS Gateway**: Twilio or MSG91 integration
4. **Payment Gateway**: Razorpay integration
5. **Analytics Dashboard**: Advanced reporting with charts
6. **Multi-tenant Support**: Multiple coaching centers