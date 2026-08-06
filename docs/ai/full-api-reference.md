# MasterMind Coaching - Full API Reference

> **Auto-generated documentation for AI assistants**  
> Generated: 2026-04-17 14:58:36  
> Generator: Generate-ApiDocs.ps1

## Overview

| Metric | Value |
|--------|-------|
| Total Controllers | 14 |
| Total Endpoints | 92 |
| Protected Endpoints | 92 |
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
| GET | `/api/attendance/report` | GetAttendanceReport |
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
| GET | `/api/dashboard/daily-stats` | GetDailyStats |
| GET | `/api/dashboard/parent-stats` | GetParentStats |
| GET | `/api/dashboard/teacher-stats` | GetTeacherStats |
| GET | `/api/dashboard/recent-students` | GetRecentStudents |
| GET | `/api/notifications` | GetNotifications |
| GET | `/api/expenses` | GetExpenses |
| GET | `/api/expenses/categories` | GetExpenseCategories |
| GET | `/api/expenses/summary` | GetExpenseSummary |
| POST | `/api/expenses` | CreateExpense |
| POST | `/api/expenses/{id}/pay` | MarkExpensePaid |
| GET | `/api/expenses/{id}/receipt` | DownloadExpenseReceipt |
| POST | `/api/expenses/salaries/{id}/pay` | MarkSalaryPaid |
| GET | `/api/expenses/salaries/{id}/receipt` | DownloadSalaryReceipt |
| PUT | `/api/expenses/{id}` | UpdateExpense |
| DELETE | `/api/expenses/{id}` | DeleteExpense |
| GET | `/api/feecollection` | GetFeeCollections |
| GET | `/api/feecollection/receipt/{id}` | GetReceipt |
| GET | `/api/feecollection/student/{studentId}/fee-details` | GetStudentFeeDetails |
| POST | `/api/feecollection/setup-student-fee` | SetupStudentFee |
| POST | `/api/feecollection/collect-payment` | CollectPayment |
| POST | `/api/feecollection/receipt/{id}/send-email` | SendReceiptEmail |
| GET | `/api/feecollection/receipt/{id}/pdf` | DownloadReceiptPdf |
| GET | `/api/fees` | GetFees |
| GET | `/api/fees/overdue` | GetOverdueFees |
| GET | `/api/fees/structures` | GetFeeStructures |
| POST | `/api/fees/structures` | CreateFeeStructure |
| PUT | `/api/fees/structures/{id}` | UpdateFeeStructure |
| DELETE | `/api/fees/structures/{id}` | ArchiveFeeStructure |
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

All `/api/parent/*` routes require the `Parent` role. Parent dashboard clients load attendance, fees, and performance independently so one unavailable optional dataset does not hide the others. Recurring schedule-control fee rows are excluded from parent totals.
| GET | `/api/student-remarks` | GetRemarks |
| POST | `/api/student-remarks` | CreateRemark |
| GET | `/api/teacher-portal/classes` | GetMyClasses |
| GET | `/api/teacher-portal/classes/{classId}/students` | GetClassStudents |
| GET | `/api/teacher-portal/classes/{classId}/attendance?date={date}` | GetClassAttendance; Teacher/Admin role, assigned class only |
| POST | `/api/teacher-portal/classes/{classId}/attendance` | SaveClassAttendance; bulk upsert for an assigned class with 3 PM/6 PM defaults |
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
| POST | `/api/teachers/{id}/photo` | UploadTeacherPhoto |
| DELETE | `/api/teachers/{id}` | DeleteTeacher |
| GET | `/api/test/student-count` | GetStudentCount |
| GET | `/api/test/student-columns` | GetStudentColumns |
| GET | `/api/templatezone/templates` | GetTemplates |
| POST | `/api/templatezone/templates` | CreateTemplate |
| PUT | `/api/templatezone/templates/{id}` | UpdateTemplate |
| DELETE | `/api/templatezone/templates/{id}` | DeleteTemplate |
| GET | `/api/templatezone/birthday-reminders` | GetBirthdayReminders |
| GET | `/api/templatezone/fee-reminders` | GetFeeReminders |
| GET | `/api/templatezone/fee-receipt-logs` | GetFeeReceiptLogs |
| POST | `/api/templatezone/preview` | PreviewTemplate |
| GET | `/api/adminnotes` | GetNotes |
| POST | `/api/adminnotes` | Create |
| PUT | `/api/adminnotes/{id}` | Update |
| DELETE | `/api/adminnotes/{id}` | Delete |
| GET | `/api/admin-notifications` | GetNotifications |
| GET | `/api/academicplanner` | GetEntries |
| POST | `/api/academicplanner` | Create |
| PUT | `/api/academicplanner/{id}` | Update |
| DELETE | `/api/academicplanner/{id}` | Delete |
| POST | `/api/paper-generator/documents` | UploadDocuments |
| GET | `/api/paper-generator/documents` | GetDocuments |
| POST | `/api/paper-generator/jobs` | CreateJob |
| GET | `/api/paper-generator/jobs` | GetJobs |
| GET | `/api/paper-generator/jobs/{id}` | GetJob |
| GET | `/api/paper-generator/jobs/{id}/paper` | DownloadPaper |
| GET | `/api/paper-generator/jobs/{id}/answer-key` | DownloadAnswerKey |
| GET | `/api/paper-generator/questions` | GetQuestions |

### AcademicPlannerController

**File:** `src/backend/MasterMind.API/Controllers/AcademicPlannerController.cs`  
**Auth Required:** Yes (`[Authorize(Policy = "Staff")]`)

| Method | Path | Action |
|--------|------|--------|
| GET | `/api/academicplanner` | GetEntries |
| POST | `/api/academicplanner` | Create |
| PUT | `/api/academicplanner/{id}` | Update |
| DELETE | `/api/academicplanner/{id}` | Delete |

---

### PaperGeneratorController

**File:** `src/backend/MasterMind.API/Controllers/PaperGeneratorController.cs`  
**Auth Required:** Yes (`[Authorize(Roles = "Admin")]`)

| Method | Path | Action | Notes |
|--------|------|--------|-------|
| POST | `/api/paper-generator/documents` | UploadDocuments | Multipart upload, up to 5 PDFs, 25 MB each |
| GET | `/api/paper-generator/documents` | GetDocuments | Lists recent PDFs for active/session query |
| POST | `/api/paper-generator/jobs` | CreateJob | Creates paper and answer key PDFs |
| GET | `/api/paper-generator/jobs` | GetJobs | Lists recent generated papers |
| GET | `/api/paper-generator/jobs/{id}` | GetJob | Poll single job status |
| GET | `/api/paper-generator/jobs/{id}/paper` | DownloadPaper | Streams private generated paper PDF |
| GET | `/api/paper-generator/jobs/{id}/answer-key` | DownloadAnswerKey | Streams private answer key PDF |
| GET | `/api/paper-generator/questions` | GetQuestions | Searches session-specific reusable question bank |

---

### Release 1.0.9 Account, Student, and Salary Endpoints

| Method | Path | Auth | Purpose |
|--------|------|------|---------|
| GET | `/api/students/export` | Admin | Download all non-deleted student details across every session as `.xlsx` |
| POST | `/api/students/{id}/parent-invitation` | Admin | Until onboarding is complete, revoke any active invite and return a fresh 72-hour link plus the primary mobile for WhatsApp sharing; email delivery failure returns `EmailSent = false` without losing the WhatsApp link; reject resend after the parent has set a password |
| GET | `/api/auth/invitations/{token}` | Public | Validate a Parent or Teacher invitation and return masked account details plus account type |
| POST | `/api/auth/invitations/accept` | Public | Store the invited Parent/Teacher recovery email and password, synchronize the linked profile, and consume the invitation |
| POST | `/api/teachers/{id}/invitation` | Admin | Provision the mobile-first Teacher account, revoke an earlier unused invite, and return a fresh 72-hour WhatsApp link; email failure is non-fatal |
| POST | `/api/teachers/{id}/photo` | Admin | Upload or replace a Teacher profile photo (JPG, PNG, GIF, or WebP up to 5 MB) and synchronize it to the linked Teacher login profile |
| GET | `/api/account/security` | Parent | Get recovery email plus read-only primary/secondary mobiles |
| PUT | `/api/account/security/email` | Parent | Change the parent-controlled recovery email; primary mobile remains Admin-controlled |
| POST | `/api/account/deletion-request` | Authenticated | Request deletion for the current account |
| POST | `/api/account/public-deletion-request` | Public | Request deletion using a registered email or mobile without exposing account existence |
| GET | `/api/expenses` | Authenticated | Return general expenses and teacher salary obligations with `source`, `status`, and `salaryId` |
| POST | `/api/expenses/salaries/{id}/pay` | Authenticated | Mark a teacher salary obligation paid |
| GET | `/api/expenses/salaries/{id}/receipt` | Authenticated | Download a PDF receipt for a paid teacher salary obligation |

Student create/update payloads persist `motherName`, `fatherName`, `dateOfBirth`, `currentSchool`, `parentMobile`, `secondaryParentMobile`, and `admissionDate` independently. `parentMobile` is required and is the parent account identity. Admin does not supply `parentEmail`; the parent supplies it while accepting the invitation. Attendance records expose the separate parent names for correctly addressed WhatsApp follow-ups.

Monthly `POST /api/finance/fees` schedules installments on the configured start day each month (clamped to the month's last day) through the academic-session end date or an earlier supplied end date. Operational fee routes hide the recurring parent row and expose installments through the end of next month for due-period filtering. Missing installments are generated idempotently when Finance is opened, and student deactivation truncates unpaid future installments while retaining paid history. `DELETE /api/fees/{id}` stops a recurring schedule, deletes its unpaid installments, and retains paid installments; standalone fees with a payment return a clear conflict.

### Release 1.0.15 Login, Teacher DOB, and Parent-Mobile Reuse

| Method | Path | Auth | Purpose |
|--------|------|------|---------|
| GET | `/health/ready` | Public | Open and verify the database connection so login clients can wake an auto-paused database before credentials are submitted |
| GET | `/api/students/{id}/photo` | Authenticated | Stream the stored student photo through the API so generated template canvases can render it without cross-origin Blob Storage failures |

Password and OTP authentication requests have a 9.5-second overall client deadline instead of sequential 90-second waits. Password login uses indexed exact email/mobile candidates and writes last-login plus refresh-token state in one database save. The native Android app rotates its existing seven-day refresh token before access-token expiry and when returning to the foreground; website expiry behavior is unchanged.

Teacher list/detail/create/update now include `dateOfBirth`; new Teacher records require a non-future DOB while legacy records may remain blank until edited. Creating a new Student with the primary parent mobile from a deleted Student reuses the existing Parent account and does not create or reject a duplicate login.

Finance summary now returns `monthlyRecurringRevenue`, `unassignedStudents`, and `activeHouseholds`. `GET /api/finance/fees/unassigned-students?sessionId={id}` lists active students in the selected session who have no non-deleted fee assignment. Fee list and overdue responses include parent-mobile household metadata and student profile photos. Payment collection accepts parents without a recovery email, preserves atomic rollback, and always creates a receipt before a fee is shown as paid. Template Zone fee reminders and fee-receipt logs include student IDs and profile-photo metadata; rendered cards obtain the image from the authenticated student-photo stream so Azure Blob CORS cannot leave the card empty.

Android `1.0.15` targets API 36 and uses `EdgeToEdge.enable()` with a single native WebView system-bar inset listener. Deprecated direct status-bar/navigation-bar color setters were removed to address the Android 15/16 Play pre-launch warnings while retaining safe fixed headers and bottom actions.

Fee Collection operational totals exclude soft-deleted installments, recurring schedule control rows, children of deleted schedules, terminal-status rows, and zero balances. `POST /api/feecollection/collect-payment` accepts a blank `transactionId` and assigns a sequential `MM-PAY-{paymentId}` reference inside the receipt transaction.

The complete payment/reference/receipt transaction runs inside the configured SQL Server execution strategy. Startup compatibility checks non-destructively align legacy `Payments`, `FeeReceipts`, and `FeeReceiptItems` tables with the current EF model before collection is accepted.

Android hotfix `1.0.16` uses versionCode `17` because versionCode `16` was already uploaded to the Play closed-testing draft before the Fee Collection regression was reported.

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

*Auto-generated baseline with manual updates through 2026-08-06 for release 1.0.15 login performance, native session continuity, Teacher DOB, parent-mobile reuse, household Finance workflows, and receipt reliability.*
