# MasterMind Coaching - Full API Reference

> **Auto-generated documentation for AI assistants**  
> Generated: 2026-04-17 14:58:36  
> Generator: Generate-ApiDocs.ps1

## Overview

| Metric | Value |
|--------|-------|
| Total Controllers | 14 |
| Total Endpoints | 90 |
| Protected Endpoints | 90 |
| Public Endpoints | 0 |

## Base URLs

| Environment | Backend API | Frontend |
|-------------|-------------|----------|
| Production | `https://mastermind-api-2404-eadxgpe5f7dch9f6.centralindia-01.azurewebsites.net` | `https://victorious-glacier-0e6507000.6.azurestaticapps.net` |
| Development | `http://localhost:5000` | `http://localhost:3000` |

---

## Endpoints by Controller

### 

**File:** `src/backend/MasterMind.API/Controllers/AttendanceController.cs`  
**Auth Required:** Yes

| Method | Path | Action |
|--------|------|--------|| GET | `/api/attendance` | GetAttendance |
| POST | `/api/attendance` | MarkAttendance |
| PUT | `/api/attendance/{id}` | UpdateAttendance |
| DELETE | `/api/attendance/{id}` | DeleteAttendance |
| GET | `/api/auth/me` | GetCurrentUser |
| GET | `/api/auth/check` | CheckAuth |
| GET | `/api/auth/devices` | GetUserDevices |
| POST | `/api/auth/otp/request` | RequestOtp |
| POST | `/api/auth/request-otp` | RequestOtp |
| POST | `/api/auth/otp/verify` | VerifyOtp |
| POST | `/api/auth/verify-otp` | VerifyOtp |
| POST | `/api/auth/login` | LoginWithPassword |
| POST | `/api/auth/quick-login` | QuickLogin |
| POST | `/api/auth/set-password` | SetPassword |
| POST | `/api/auth/token/refresh` | RefreshToken |
| POST | `/api/auth/refresh-token` | RefreshToken |
| POST | `/api/auth/logout` | Logout |
| POST | `/api/auth/logout/all` | LogoutAll |
| POST | `/api/auth/device/trust` | TrustDevice |
| POST | `/api/auth/device/revoke` | RevokeDevice |
| GET | `/api/classes` | GetClasses |
| GET | `/api/classes/{id}` | GetClass |
| POST | `/api/classes` | CreateClass |
| PUT | `/api/classes/{id}` | UpdateClass |
| DELETE | `/api/classes/{id}` | DeleteClass |
| GET | `/api/dashboard/stats` | GetStats |
| GET | `/api/dashboard/admin-stats` | GetAdminStats |
| GET | `/api/dashboard/parent-stats` | GetParentStats |
| GET | `/api/dashboard/recent-students` | GetRecentStudents |
| GET | `/api/expenses` | GetExpenses |
| GET | `/api/expenses/categories` | GetExpenseCategories |
| GET | `/api/expenses/summary` | GetExpenseSummary |
| POST | `/api/expenses` | CreateExpense |
| PUT | `/api/expenses/{id}` | UpdateExpense |
| DELETE | `/api/expenses/{id}` | DeleteExpense |
| GET | `/api/feecollection` | GetFeeCollections |
| GET | `/api/feecollection/receipt/{id}` | GetReceipt |
| GET | `/api/feecollection/student/{studentId}/fee-details` | GetStudentFeeDetails |
| POST | `/api/feecollection/setup-student-fee` | SetupStudentFee |
| POST | `/api/feecollection/collect-payment` | CollectPayment |
| POST | `/api/feecollection/receipt/{id}/send-email` | SendReceiptEmail |
| GET | `/api/fees` | GetFees |
| GET | `/api/fees/overdue` | GetOverdueFees |
| GET | `/api/fees/structures` | GetFeeStructures |
| POST | `/api/fees/reminders` | SendReminders |
| POST | `/api/fees/{id}/mark-paid` | MarkFeeAsPaid |
| PUT | `/api/fees/{id}` | UpdateFee |
| DELETE | `/api/fees/{id}` | DeleteFee |
| GET | `/api/finance/summary` | GetFinancialSummary |
| GET | `/api/finance/payments` | GetRecentPayments |
| GET | `/api/finance/payments/history` | GetPaymentHistory |
| GET | `/api/finance/payments/pending` | GetPendingPayments |
| GET | `/api/finance/fees` | GetFees |
| GET | `/api/finance/fees/overdue` | GetOverdueFees |
| GET | `/api/finance/expenses` | GetExpenses |
| GET | `/api/finance/reports` | GetReports |
| POST | `/api/finance/payments` | CreatePayment |
| POST | `/api/finance/reports/generate` | GenerateReport |
| POST | `/api/finance/fees` | CreateFee |
| GET | `/api/parent/children` | GetMyChildren |
| GET | `/api/parent/dashboard/stats` | GetParentDashboardStats |
| GET | `/api/parent/children/{childId}/attendance` | GetChildAttendance |
| GET | `/api/parent/children/{childId}/fees` | GetChildFees |
| GET | `/api/parent/children/{childId}/performance` | GetChildPerformance |
| GET | `/api/sessions` | GetSessions |
| GET | `/api/sessions/active` | GetActiveSession |
| POST | `/api/sessions` | CreateSession |
| PUT | `/api/sessions/{id}/activate` | ActivateSession |
| GET | `/api/students` | GetStudents |
| GET | `/api/students/{id}` | GetStudent |
| GET | `/api/students/available-for-mapping` | GetAvailableStudentsForMapping |
| POST | `/api/students` | CreateStudent |
| POST | `/api/students/{studentId}/classes/{classId}` | MapStudentToClass |
| PUT | `/api/students/{id}` | UpdateStudent |
| DELETE | `/api/students/{studentId}/classes/{classId}` | UnmapStudentFromClass |
| DELETE | `/api/students/{id}` | DeleteStudent |
| GET | `/api/subjects` | GetSubjects |
| GET | `/api/subjects/{id}` | GetSubject |
| GET | `/api/subjects/suggestions` | GetSubjectSuggestions |
| GET | `/api/subjects/by-class/{classId}` | GetSubjectsByClass |
| POST | `/api/subjects` | CreateSubject |
| PUT | `/api/subjects/{id}` | UpdateSubject |
| DELETE | `/api/subjects/{id}` | DeleteSubject |
| GET | `/api/teachers` | GetTeachers |
| GET | `/api/teachers/{id}` | GetTeacher |
| POST | `/api/teachers` | CreateTeacher |
| PUT | `/api/teachers/{id}` | UpdateTeacher |
| DELETE | `/api/teachers/{id}` | DeleteTeacher |
| GET | `/api/test/student-count` | GetStudentCount |
| GET | `/api/test/student-columns` | GetStudentColumns |

---

## Authentication

All protected endpoints require:
```
Authorization: Bearer <access_token>
```

## Error Response Format

```json
{
  "success": false,
  "message": "Error description",
  "errorCode": "ERROR_CODE"
}
```

## Quick Reference

### Common HTTP Status Codes

| Code | Meaning |
|------|---------|
| 200 | Success |
| 201 | Created |
| 400 | Bad Request |
| 401 | Unauthorized |
| 403 | Forbidden |
| 404 | Not Found |
| 500 | Internal Server Error |

---

*This file is auto-generated. Do not edit manually.*
