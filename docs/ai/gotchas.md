# MasterMind Coaching - AI Gotchas & Lessons Learned

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

## Frontend (Vue 3 + Pinia)

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
