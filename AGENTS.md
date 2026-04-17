# AI Agent Instructions for MasterMind Coaching

> **This file defines consistent behavior for ALL AI coding assistants working on this project.**  
> Supported: Cursor, GitHub Copilot, Claude Code, Windsurf, Cascade, and others.

## 🎯 Core Principles

1. **Read Before Writing** - Always check `docs/ai/gotchas.md` before making changes
2. **Learn From Mistakes** - If a fix takes >1 attempt, update `docs/ai/gotchas.md`
3. **Minimal Changes** - Prefer narrow, targeted fixes over broad refactoring
4. **Follow Existing Patterns** - Match the codebase style, don't introduce new patterns

---

## 📚 Required Reading Order

Before making ANY code changes, read these files in order:

1. **`docs/ai/gotchas-index.json`** - Quick symptom lookup (2 KB)
2. **`docs/ai/gotchas.md`** - Detailed lessons learned (if symptom matches)
3. **`docs/ai/full-api-reference.md`** - API documentation (if working on endpoints)
4. **`docs/ai/api-endpoint-index.json`** - Route-to-file mapping (if locating code)

---

## 🚨 Mandatory Rules

### Rule 1: Self-Learning Protocol
```
IF bug_fix_attempts > 1:
    MUST update docs/ai/gotchas.md with:
    - Symptom (what you observed)
    - Root Cause (why it happened)
    - Solution (how you fixed it)
    - Files Affected
    - Date Learned
    
    MUST update docs/ai/gotchas-index.json with symptom keywords
```

### Rule 2: Bounded File Reads
```
- Read max 100 lines at a time
- Use api-endpoint-index.json to locate exact files
- Never grep entire codebase - use indexes first
```

### Rule 3: Narrow Change Mode
```
When user says "just use existing pattern" or "keep it simple":
- Stop over-engineering
- Match existing code style exactly
- Make minimal changes only
```

### Rule 4: Authorization Check
```
For ANY new controller:
- MUST add [Authorize] attribute at class level
- Exception: AuthController, HealthController only
```

---

## 🏗️ Project Architecture

### Backend (ASP.NET Core 9)
```
src/backend/MasterMind.API/
├── Controllers/     # API endpoints (add [Authorize]!)
├── Services/        # Business logic (Interfaces/ + Implementations/)
├── Models/          # Entities/ + DTOs/
├── Data/            # MasterMindDbContext.cs
├── Middleware/      # Exception handling
└── Program.cs       # App configuration
```

### Frontend (Vue 3 + TypeScript)
```
src/frontend/mastermind-web/src/
├── views/           # Page components
├── components/      # Reusable components
├── stores/          # Pinia state management
├── services/        # API service layer
├── router/          # Vue Router config
└── types/           # TypeScript interfaces
```

---

## ⚠️ Common Pitfalls (Quick Reference)

| Symptom | Likely Cause | Solution |
|---------|--------------|----------|
| 500 on GET endpoint | LINQ can't translate to SQL | Load data first, project in memory |
| 401 not returned | Missing `[Authorize]` | Add to controller class |
| Redirect loop after login | Pinia persistence conflict | Don't manually read localStorage |
| 404 on direct URL | Missing SPA fallback | Check `staticwebapp.config.json` |
| CORS errors | Origin not allowed | Add to Azure App Settings |
| `roles` undefined | Backend returns array | Map `roles[0]` to `role` |

---

## 🔧 Development Commands

### Backend
```bash
cd src/backend/MasterMind.API
dotnet restore
dotnet build
dotnet run --environment Development
# API: http://localhost:5000
# Swagger: http://localhost:5000/swagger
```

### Frontend
```bash
cd src/frontend/mastermind-web
npm install --legacy-peer-deps
npm run dev
# App: http://localhost:3000
```

### Testing API
```powershell
# Health check
Invoke-WebRequest -Uri "http://localhost:5000/health" -UseBasicParsing

# Protected endpoint (needs token)
$headers = @{ Authorization = "Bearer $token" }
Invoke-WebRequest -Uri "http://localhost:5000/api/students" -Headers $headers
```

---

## 📝 Code Style Guidelines

### C# (Backend)
```csharp
// Controllers: Always include these attributes
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]  // Required for protected endpoints
public class ExampleController : ControllerBase

// Return consistent response format
return Ok(new ApiResponse<T> {
    Success = true,
    Message = "Operation successful",
    Data = result
});
```

### TypeScript (Frontend)
```typescript
// Services: Use apiService for all API calls
import { apiService } from '@/services/apiService'

// Stores: Use Pinia with persistence
export const useExampleStore = defineStore('example', () => {
  // State, getters, actions
}, {
  persist: {
    key: 'mastermind-example',
    storage: localStorage
  }
})
```

---

## 🔄 Git Workflow

1. **Feature branches**: `feature/description`
2. **Bug fixes**: `fix/description`
3. **Commit messages**: Clear, descriptive (e.g., "Fix LINQ translation error in FeesController")
4. **Push to main**: Triggers GitHub Actions deployment

---

## 📊 When to Update Documentation

| Event | Action Required |
|-------|-----------------|
| Bug fix took >1 attempt | Update `gotchas.md` + `gotchas-index.json` |
| New API endpoint added | Update `full-api-reference.md` + `api-endpoint-index.json` |
| New gotcha discovered | Add to both gotchas files |
| Architecture change | Update this file (`AGENTS.md`) |

---

## 🎓 Learning Protocol Example

**Scenario**: AI tries to fix a 500 error, first attempt fails.

**Step 1**: Check `gotchas-index.json` for "500 error" symptom  
**Step 2**: Find matching gotcha in `gotchas.md`  
**Step 3**: Apply documented solution  
**Step 4**: If new issue, document it after fixing

```markdown
### [New Gotcha Title]
**Symptom**: [What you observed]  
**Root Cause**: [Why it happened]  
**Solution**: [How to fix it]  
**Files Affected**: [List of files]  
**Date Learned**: [YYYY-MM-DD]
```

---

## 🤖 AI Editor Compatibility

This file works with:
- ✅ Cursor
- ✅ GitHub Copilot
- ✅ Claude Code
- ✅ Windsurf
- ✅ Cascade
- ✅ Any AI that reads project files

**Key**: All AI assistants read this file and follow the same rules, ensuring consistent behavior across the team.
