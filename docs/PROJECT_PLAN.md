# The MasterMind Coaching Classes - Project Plan

## Project Overview

A comprehensive coaching institute management system with three user roles: **Administrator**, **Teacher**, and **Parent**. Built with a modular, future-proof architecture.

### Technology Stack
- **Backend**: .NET 10 (ASP.NET Core Web API)
- **Frontend**: Vue.js 3 (Composition API + TypeScript)
- **Database**: PostgreSQL (free tier compatible)
- **Containerization**: Docker & Docker Compose
- **Repository**: GitHub
- **Authentication**: OTP-based (Mobile/Email)

---

## Phase Breakdown

### Phase 1: Project Setup & Infrastructure
**Goal**: Set up the complete development environment and project structure

- Initialize GitHub repository
- Create .NET 10 Web API project structure
- Create Vue.js 3 frontend project structure
- Set up Docker configuration
- Configure PostgreSQL database
- Set up project documentation
- Configure CI/CD basics

### Phase 2: Authentication System
**Goal**: Implement OTP-based login for all user types

- User registration and management
- OTP generation and verification (Email/SMS)
- JWT token-based authentication
- Role-based access control (Admin, Teacher, Parent)
- Login/Logout functionality
- Session management

### Phase 3: Student Management (Admin)
**Goal**: Complete student CRUD operations and class management

- Student database and CRUD operations
- Class/Section management
- Student-Class assignment
- Class-wise student views
- Student search and filtering
- Bulk student operations

### Phase 4: Attendance System
**Goal**: Daily attendance tracking with visual reports

- Daily attendance marking
- Attendance history
- Visual attendance reports (charts/graphs)
- Attendance summary by student/class
- Export attendance reports

### Phase 5: Finance Management
**Goal**: Fee tracking for students and salary for teachers

- Student fee structure
- Fee payment tracking
- Payment history
- Outstanding dues management
- Combined financial reports
- Teacher salary management
- Financial dashboards

### Phase 6: Teacher Management
**Goal**: Complete teacher module

- Teacher database and CRUD
- Teacher attendance tracking
- Teacher-Class assignment
- Teacher salary/payment tracking
- Teacher performance metrics

### Phase 7: Lead Management
**Goal**: Inquiry and lead tracking system

- New inquiry registration
- Lead status tracking
- Follow-up management
- Lead conversion tracking
- Lead reports and analytics

### Phase 8: Teacher Portal
**Goal**: Teacher-specific features

- View assigned students
- Student performance tracking
- Chapter-wise remarks
- Student progress reports
- Communication with parents

### Phase 9: Parent Portal
**Goal**: Parent-specific features

- View child's attendance
- View child's performance
- Fee payment status
- Communication with teachers
- Notifications

### Phase 10: Advanced Features & Polish
**Goal**: Additional features and optimization

- Dashboard analytics
- Notification system
- Report generation
- Data export/import
- Performance optimization
- Security hardening

---

## System Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        FRONTEND (Vue.js 3)                       │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐              │
│  │ Admin Portal│  │Teacher Portal│  │Parent Portal│              │
│  └─────────────┘  └─────────────┘  └─────────────┘              │
└─────────────────────────────────────────────────────────────────┘
                              │
                              │ HTTP/REST API
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                    BACKEND (.NET 10 Web API)                     │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │                    API Gateway Layer                      │   │
│  └──────────────────────────────────────────────────────────┘   │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐              │
│  │    Auth     │  │   Student   │  │  Attendance │              │
│  │   Module    │  │   Module    │  │   Module    │              │
│  └─────────────┘  └─────────────┘  └─────────────┘              │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐              │
│  │   Finance   │  │   Teacher   │  │    Lead     │              │
│  │   Module    │  │   Module    │  │   Module    │              │
│  └─────────────┘  └─────────────┘  └─────────────┘              │
└─────────────────────────────────────────────────────────────────┘
                              │
                              │ Entity Framework Core
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                      DATABASE (PostgreSQL)                       │
└─────────────────────────────────────────────────────────────────┘
```

---

## Database Schema Overview

### Core Tables
- **Users** - All user accounts (Admin, Teacher, Parent)
- **Students** - Student information
- **Classes** - Class/Section definitions
- **StudentClasses** - Student-Class mapping

### Attendance Tables
- **Attendance** - Daily attendance records
- **TeacherAttendance** - Teacher attendance

### Finance Tables
- **FeeStructure** - Fee definitions
- **StudentFees** - Student fee records
- **Payments** - Payment transactions
- **TeacherSalary** - Teacher salary records

### Lead Tables
- **Leads** - Inquiry/Lead information
- **LeadFollowups** - Follow-up records

### Teacher-Student Tables
- **TeacherStudents** - Teacher-Student mapping
- **StudentRemarks** - Chapter-wise remarks
- **StudentPerformance** - Performance tracking

---

## Project Structure

```
MasterMindCoaching/
├── docs/                          # Documentation
├── src/
│   ├── backend/
│   │   └── MasterMind.API/        # .NET 10 Web API
│   │       ├── Controllers/
│   │       ├── Services/
│   │       ├── Models/
│   │       ├── Data/
│   │       └── Middleware/
│   └── frontend/
│       └── mastermind-web/        # Vue.js 3 App
│           ├── src/
│           │   ├── components/
│           │   ├── views/
│           │   ├── stores/
│           │   ├── services/
│           │   └── router/
│           └── public/
├── docker/
│   ├── Dockerfile.api
│   ├── Dockerfile.web
│   └── docker-compose.yml
├── database/
│   └── migrations/
├── .github/
│   └── workflows/
├── README.md
└── .gitignore
```

---

## Phase 1 Detailed Tasks

### 1.1 GitHub Repository Setup
- Create repository: `mastermind-coaching-classes`
- Initialize with README
- Add .gitignore for .NET and Vue.js
- Set up branch protection rules

### 1.2 Backend Setup (.NET 10)
- Create ASP.NET Core Web API project
- Configure project structure (Clean Architecture)
- Add required NuGet packages:
  - Entity Framework Core
  - PostgreSQL provider
  - JWT Authentication
  - Swagger/OpenAPI
- Set up configuration files

### 1.3 Frontend Setup (Vue.js 3)
- Create Vue.js 3 project with Vite
- Configure TypeScript
- Add required packages:
  - Vue Router
  - Pinia (state management)
  - Axios (HTTP client)
  - Tailwind CSS (styling)
- Set up project structure

### 1.4 Docker Configuration
- Create Dockerfile for API
- Create Dockerfile for Frontend
- Create docker-compose.yml
- Configure PostgreSQL container
- Set up networking

### 1.5 Database Setup
- Configure Entity Framework Core
- Create initial DbContext
- Set up connection strings
- Create base entity classes

---

## Phase 2 Detailed Tasks

### 2.1 Database Models for Auth
- User entity
- Role entity
- OTP entity
- RefreshToken entity

### 2.2 Backend Auth Services
- OTP generation service
- Email service (for OTP)
- SMS service interface (for future)
- JWT token service
- User service

### 2.3 Auth API Endpoints
- POST /api/auth/request-otp
- POST /api/auth/verify-otp
- POST /api/auth/refresh-token
- POST /api/auth/logout
- GET /api/auth/me

### 2.4 Frontend Auth Components
- Login page (mobile/email input)
- OTP verification page
- Auth store (Pinia)
- Route guards
- Auth service

### 2.5 Role-Based Access
- Admin role setup
- Teacher role setup
- Parent role setup
- Permission middleware

---

## Getting Started (Phase 1)

Once approved, we will:
1. Create the project folder structure
2. Initialize the .NET 10 backend
3. Initialize the Vue.js 3 frontend
4. Set up Docker configuration
5. Configure the database
6. Create initial documentation

---

## Notes

- All phases are designed to be modular and independent
- Each phase builds upon the previous one
- The architecture supports easy addition of new features
- Docker ensures consistent development and deployment
- Free tier compatible with most cloud providers