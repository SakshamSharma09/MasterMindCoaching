# MasterMind Coaching - AI Gotchas & Lessons Learned

> **Last Updated**: 2026-06-17 (Android release API base URL fix)

> **Purpose**: This file is automatically updated by AI assistants when bugs require more than one attempt to fix. It serves as a permanent memory system that prevents the same mistakes from being repeated.

## How This Works

1. When an AI assistant encounters a bug that takes multiple attempts to fix, it MUST document the lesson here
2. Before making changes, AI assistants should check this file for relevant gotchas
3. The `gotchas-index.json` file provides quick symptom-based lookup

---

## Backend (ASP.NET Core 9)

### LINQ to SQL Translation Issues
**Symptom**: `500 Internal Server Error` on GET endpoints with complex queries  
**Root Cause**: EF Core cannot translate certain C# expressions to SQL (e.g., `StringComparison.OrdinalIgnoreCase`, complex null-conditional operators in Select projections)  
**Solution**: Load data first with `.ToListAsync()`, then filter/project in memory  
**Files Affected**: `FeesController.cs`, `ExpensesController.cs`  
**Date Learned**: 2026-04-17

```csharp
// ❌ BAD - SQL translation fails
var result = await query
    .Select(x => new Dto {
        Name = x.Related?.SubRelated?.FirstOrDefault()?.Name ?? "N/A"
    })
    .ToListAsync();

// ✅ GOOD - Load first, project in memory
var data = await query.Include(x => x.Related).ToListAsync();
var result = data.Select(x => new Dto {
    Name = x.Related?.SubRelated?.FirstOrDefault()?.Name ?? "N/A"
}).ToList();
```

### Azure SQL: EnsureCreated Does Not Migrate Existing Databases
**Symptom**: `SqlException` (invalid column name, constraint failures) after deploying code that adds properties or relationships; CRUD fails only in Azure SQL production.  
**Root Cause**: `Database.EnsureCreated()` creates a schema once and does not evolve existing databases. Azure SQL used that path while PostgreSQL used raw bootstrap SQL.  
**Solution**: Use EF Core migrations and apply them at startup with `Database.MigrateAsync()` for SQL Server (see `docs/database-migrations.md`). Generate changes with `dotnet ef migrations add`.  
**Files Affected**: `Program.cs`, `Data/Migrations/`  
**Date Learned**: 2026-04-18

### Fee Save Failure Due to Missing/Hardcoded Fee Structures
**Symptom**: `POST /api/finance/fees` fails with `400` and message `Fee structure not found`, especially when frontend sends fixed IDs (like `1..7`) but the database has no fee structure rows.  
**Root Cause**: Frontend had hardcoded fee structure IDs; production DB can have empty or different IDs, so payload references invalid records.  
**Solution**:  
1. Load fee structures dynamically in UI instead of hardcoding IDs.  
2. Add backend compatibility fallback to auto-create a valid fee structure for legacy payloads when ID is missing.  
3. Surface backend message in UI toast for fast diagnosis.  
**Files Affected**: `FinanceController.cs`, `FeesManagementView.vue`  
**Date Learned**: 2026-05-05

### Blob Upload Fails When Connection String Is Set Under Different Azure Key Names
**Symptom**: Student photo upload returns `Error uploading photo: Photo uploads are disabled because Azure Blob Storage is not configured.` even when a storage connection string exists in Azure App Service.  
**Root Cause**: Production configuration can store the blob value under different keys (`App Settings`, `Connection Strings`, or `CUSTOMCONNSTR_*`), while service registration only checked a narrow subset.  
**Solution**: Resolve blob connection string from a broader key list (including `ConnectionStrings:AzureBlobStorageConnectionString` and `CUSTOMCONNSTR_*`), then normalize into `AzureBlobStorage:ConnectionString`. Log the key source (not the secret value).  
**Files Affected**: `Program.cs`, `BlobStorageService.cs`  
**Date Learned**: 2026-05-09

### Authorization Attribute Placement
**Symptom**: API endpoints accessible without authentication (returns 200 instead of 401)  
**Root Cause**: Missing `[Authorize]` attribute on controller class  
**Solution**: Always add `[Authorize]` at class level for protected controllers  
**Files Affected**: All controllers except `AuthController`, `HealthController`  
**Date Learned**: 2026-04-17

```csharp
// ✅ CORRECT
[ApiController]
[Route("api/[controller]")]
[Authorize]  // <-- Required for protected endpoints
public class StudentsController : ControllerBase
```

### Session Status Enum
**Symptom**: `400 Bad Request` when creating sessions with status as string  
**Root Cause**: `SessionStatus` is an enum, not a string  
**Solution**: Use integer values: `1=Planned, 2=Active, 3=Completed, 4=Suspended, 5=Cancelled`  
**Files Affected**: `SessionsController.cs`  
**Date Learned**: 2026-04-17

---

### Session Summary Queries Must Avoid Complex Navigation Translation
**Symptom**: Sessions look deleted or the Sessions page appears empty after adding computed totals.  
**Root Cause**: Complex EF Core summary queries over nested navigation properties can fail in Azure SQL translation, causing the sessions endpoint to fail before returning existing rows.  
**Solution**: Load the small summary source sets first, then compute counts and totals in memory by `SessionId`. Do not let revenue/expense summary failures hide the base sessions list.  
**Files Affected**: `SessionsController.cs`  
**Date Learned**: 2026-06-21

---

### Session Filters Can Hide Existing Data
**Symptom**: Students, classes, teachers, leads, attendance-linked screens, or dashboard counts appear empty even though data still exists in Azure SQL.  
**Root Cause**: Most admin list endpoints filter by `SessionId`. Legacy rows with `NULL SessionId`, or a stale `selectedSessionId` persisted in browser storage, can make valid records invisible.  
**Solution**: Backfill legacy `NULL SessionId` rows to the active session during SQL Server startup compatibility checks, validate incoming `sessionId` query values, and make the frontend session store replace stale selected IDs with the active session.  
**Files Affected**: `Program.cs`, `StudentsController.cs`, `ClassesController.cs`, `TeachersController.cs`, `LeadsController.cs`, `src/frontend/mastermind-web/src/stores/session.ts`  
**Date Learned**: 2026-06-26

---

## Frontend (Vue 3 + Pinia)

### Android Release Build Must Use Production API Base URL Including /api
**Symptom**: Internal testing Android app shows `Network Error` when bundled with localhost API, or `Request failed with status code 404` when bundled with the Azure host but missing `/api`.  
**Root Cause**: Vite loaded the local `.env` during `npm run build:aab`, so the first AAB used `http://localhost:5000/api`. A later override used the Azure host without `/api`, causing calls like `/auth/login` instead of `/api/auth/login`.  
**Solution**: The Android release build script must explicitly set `VITE_API_BASE_URL` to the production API base URL including `/api`, and each Play upload must bump `versionCode`. Verify compiled assets before upload with `rg "mastermind-api-.../api" dist android/app/src/main/assets/public`.  
**Files Affected**: `src/frontend/mastermind-web/scripts/build-android-release.mjs`, `src/frontend/mastermind-web/android/app/build.gradle`  
**Date Learned**: 2026-06-17

### Pinia Persistence vs Manual localStorage
**Symptom**: Redirect loop after login, auth state not persisting  
**Root Cause**: Manual `localStorage.getItem('accessToken')` conflicts with `pinia-plugin-persistedstate` which stores under `mastermind-auth` key  
**Solution**: Let Pinia handle persistence; don't manually read/write localStorage for auth  
**Files Affected**: `stores/auth.ts`, `services/apiService.ts`  
**Date Learned**: 2026-04-17

```typescript
// ❌ BAD - Conflicts with Pinia persistence
const storedToken = localStorage.getItem('accessToken')

// ✅ GOOD - Read from Pinia's persisted key
const authData = localStorage.getItem('mastermind-auth')
const token = JSON.parse(authData)?.accessToken
```

### Backend Returns `roles[]` Array, Frontend Expects `role` String
**Symptom**: User role not recognized after login  
**Root Cause**: Backend returns `{ roles: ["Admin"] }`, frontend expects `{ role: "Admin" }`  
**Solution**: Map roles array to single role in auth store  
**Files Affected**: `stores/auth.ts`, `types/auth.ts`  
**Date Learned**: 2026-04-17

```typescript
// ✅ Map roles array to single role
const userData = {
  ...response.user,
  role: response.user.role || response.user.roles?.[0] || 'Admin'
}
```

---

## Azure Deployment

### Static Web App SPA Routing
**Symptom**: 404 errors on direct URL access (e.g., `/admin/students`)  
**Root Cause**: Missing `staticwebapp.config.json` with SPA fallback  
**Solution**: Add navigation fallback to `index.html`  
**Files Affected**: `staticwebapp.config.json`  
**Date Learned**: 2026-04-17

```json
{
  "navigationFallback": {
    "rewrite": "/index.html",
    "exclude": ["/assets/*", "/*.js", "/*.css"]
  }
}
```

### CORS Configuration
**Symptom**: `CORS policy` errors in browser console  
**Root Cause**: Frontend origin not in allowed origins list  
**Solution**: Add frontend URL to `Cors__AllowedOrigins` in Azure App Settings  
**Files Affected**: Azure App Service Configuration  
**Date Learned**: 2026-04-17

---

## Mobile / Android Builds

### Android AAB Build Fails Due to Java or SDK Path
**Symptom**: `npm run build:aab` fails with `JAVA_HOME is not set and no 'java' command could be found`, missing Android SDK licenses, or `Failed to read or create install properties file` while installing Build Tools.  
**Root Cause**: Java was installed with Android Studio but not available in the terminal environment, and the Android SDK path pointed to `C:\Program Files (x86)\Android\android-sdk`, which can block SDK package/license writes.  
**Solution**: Use the Android Studio bundled JDK for `JAVA_HOME`, use a user-writable SDK at `C:\Users\Saksham\AppData\Local\Android\Sdk`, accept licenses, install required SDK packages, and point `android/local.properties` to that SDK. Keep `npm run build:aab` and `npm run build:apk` behind the local build wrapper so Gradle receives the JDK/SDK paths even when the terminal has no Android environment variables.  
**Files Affected**: `src/frontend/mastermind-web/package.json`, `src/frontend/mastermind-web/scripts/build-android-release.mjs`, `src/frontend/mastermind-web/android/local.properties`, local Windows environment variables  
**Date Learned**: 2026-05-24

```powershell
$env:JAVA_HOME='C:\Program Files\Android\openjdk\jdk-21.0.8'
$env:ANDROID_HOME='C:\Users\Saksham\AppData\Local\Android\Sdk'
$env:ANDROID_SDK_ROOT=$env:ANDROID_HOME
@(1..80 | ForEach-Object { 'y' }) | & 'C:\Program Files (x86)\Android\android-sdk\cmdline-tools\latest\bin\sdkmanager.bat' --sdk_root=$env:ANDROID_SDK_ROOT --licenses
```

---

## Database (Azure SQL)

### DateTime Functions
**Symptom**: `GETUTCDATE()` not recognized  
**Root Cause**: EF Core default value syntax differs between SQL Server versions  
**Solution**: Use `DateTime.UtcNow` in C# code instead of SQL functions  
**Files Affected**: Entity configurations, DbContext  
**Date Learned**: 2026-04-17

---

## Testing Patterns

### API Testing with PowerShell
**Pattern**: Always test protected endpoints both with and without auth token
```powershell
# Test without auth (should return 401)
Invoke-WebRequest -Uri "$backend/api/students" -UseBasicParsing

# Test with auth (should return 200)
$headers = @{ Authorization = "Bearer $token" }
Invoke-WebRequest -Uri "$backend/api/students" -Headers $headers -UseBasicParsing
```

---

## Adding New Gotchas

When you encounter a bug that requires multiple attempts to fix:

1. **Document the symptom** - What error/behavior did you observe?
2. **Identify root cause** - Why did it happen?
3. **Record the solution** - What fixed it?
4. **List affected files** - Where was the fix applied?
5. **Add date** - When was this learned?
6. **Update gotchas-index.json** - Add symptom keywords for quick lookup

**Template:**
```markdown
### [Short Title]
**Symptom**: [What you observed]  
**Root Cause**: [Why it happened]  
**Solution**: [How to fix it]  
**Files Affected**: [List of files]  
**Date Learned**: [YYYY-MM-DD]
```
