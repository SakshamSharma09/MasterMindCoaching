---
description: How to effectively use AI assistants for development, debugging, and documentation
---

# AI-Assisted Development Workflow

This guide helps you effectively use AI assistants (Cursor, Copilot, Claude, Windsurf, Cascade) with the MasterMind Coaching project.

## 🚀 Quick Start

### 1. Before Starting Any Task

Tell the AI to read the documentation first:

```
Before making changes, please read:
1. docs/ai/gotchas-index.json - check for known issues
2. docs/ai/gotchas.md - if symptoms match
3. AGENTS.md - project rules and patterns
```

### 2. For Bug Fixes

**Effective Prompt Template:**
```
I'm seeing [SYMPTOM]. 

Before fixing:
1. Check docs/ai/gotchas-index.json for this symptom
2. If found, apply the documented solution
3. If not found, investigate and document the fix in gotchas.md

Error details: [paste error message]
File: [file path if known]
```

**Example:**
```
I'm seeing a 500 error on GET /api/fees.

Before fixing:
1. Check docs/ai/gotchas-index.json for "500 error"
2. Apply documented solution if found
3. Document new gotcha if this is a new issue

Error: Internal Server Error when calling /api/fees endpoint
```

### 3. For New Features

**Effective Prompt Template:**
```
I need to add [FEATURE].

Please:
1. Check docs/ai/api-endpoint-index.json for similar patterns
2. Follow existing code style in [similar file]
3. Add [Authorize] attribute if it's a protected endpoint
4. Update docs/ai/full-api-reference.md after implementation
```

### 4. For API Work

**Effective Prompt Template:**
```
I need to work on the [endpoint name] endpoint.

Please:
1. Check docs/ai/api-endpoint-index.json for the controller location
2. Read the existing implementation
3. Follow the ApiResponse<T> pattern for responses
4. Ensure [Authorize] is present for protected endpoints
```

---

## 📋 Common Tasks

### Task: Fix a 500 Error

```
The /api/[endpoint] is returning 500.

Steps:
1. Check gotchas-index.json for "500 error" or "LINQ"
2. The most common cause is LINQ-to-SQL translation issues
3. Solution: Load data with ToListAsync() first, then filter/project in memory
4. If this is a new issue, document it in gotchas.md
```

### Task: Add Authentication to Endpoint

```
The /api/[endpoint] is accessible without login.

Steps:
1. Check if [Authorize] attribute is on the controller class
2. Add [Authorize] at class level (not method level)
3. Exception: AuthController and HealthController don't need it
```

### Task: Fix Frontend Auth Issues

```
Users are being redirected to login after logging in.

Steps:
1. Check gotchas.md for "Pinia persistence" issue
2. Don't manually read localStorage for auth tokens
3. Let Pinia persistence plugin handle it
4. Token is stored under 'mastermind-auth' key
```

### Task: Add New API Endpoint

```
I need to add a new endpoint for [feature].

Checklist:
1. Create/update controller in src/backend/MasterMind.API/Controllers/
2. Add [Authorize] attribute at class level
3. Use ApiResponse<T> for consistent responses
4. Update docs/ai/api-endpoint-index.json
5. Update docs/ai/full-api-reference.md
6. Test both with and without auth token
```

---

## 🐛 Debugging Workflow

### Step 1: Identify the Symptom
```
What error are you seeing?
- 500 Internal Server Error
- 401 Unauthorized
- 404 Not Found
- CORS error
- Redirect loop
- Data not loading
```

### Step 2: Check Gotchas Index
```
Ask AI: "Check docs/ai/gotchas-index.json for [symptom]"
```

### Step 3: Apply Known Solution
```
If found: "Apply the solution from gotchas.md section [section name]"
If not found: "Investigate and document the fix"
```

### Step 4: Document New Issues
```
If the fix took multiple attempts:
"Please update docs/ai/gotchas.md with this new lesson"
```

---

## 💡 Pro Tips

### 1. Be Specific About Files
```
❌ "Fix the student API"
✅ "Fix the GetStudents method in src/backend/MasterMind.API/Controllers/StudentsController.cs"
```

### 2. Reference Existing Patterns
```
❌ "Create a new service"
✅ "Create a new service following the pattern in src/backend/MasterMind.API/Services/Implementations/AuthService.cs"
```

### 3. Ask for Minimal Changes
```
"Please make minimal changes - don't refactor unrelated code"
"Just fix this specific issue, don't change the overall structure"
```

### 4. Request Documentation Updates
```
"After fixing, please update the relevant docs/ai/ files"
```

### 5. Test Commands
```
"Provide PowerShell commands to test this endpoint with and without auth"
```

---

## 📁 Key Files Reference

| Purpose | File |
|---------|------|
| AI Rules | `AGENTS.md` |
| Known Issues | `docs/ai/gotchas.md` |
| Issue Lookup | `docs/ai/gotchas-index.json` |
| API Docs | `docs/ai/full-api-reference.md` |
| Route Mapping | `docs/ai/api-endpoint-index.json` |

---

## 🔄 Continuous Improvement

The AI documentation system improves automatically:

1. **AI learns from mistakes** - Every multi-attempt fix gets documented
2. **Knowledge compounds** - Future developers benefit from past lessons
3. **Consistent behavior** - All AI editors follow the same rules
4. **Reduced token usage** - AI reads indexes instead of scanning codebase

### Contributing to the System

When you discover a new gotcha:
1. Add it to `docs/ai/gotchas.md` with the standard template
2. Add symptom keywords to `docs/ai/gotchas-index.json`
3. The next developer (or AI) won't repeat the mistake

---

## 🎯 Example Prompting Session

**User**: "The fees page is showing an error"

**Effective Follow-up**:
```
Please investigate the fees page error.

1. First check docs/ai/gotchas-index.json for "fees" or "500 error"
2. Check the FeesController.cs for LINQ translation issues
3. If it's a new issue, document it after fixing
4. Provide test commands to verify the fix
```

**AI Response Pattern**:
1. Checks gotchas index
2. Finds relevant gotcha or investigates
3. Applies fix
4. Documents if new
5. Provides verification commands
