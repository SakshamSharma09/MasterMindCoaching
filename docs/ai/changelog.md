# MasterMind Coaching - AI-Friendly Changelog

> **Tracks what changed and why for AI context**  
> Format optimized for AI assistants to understand project evolution

## [2026-05-09] - Play Store Readiness Pass

### Changed
- Hardened Android release config in `android/app/build.gradle`:
  - release minification enabled
  - resource shrinking enabled
  - release signing hooks added via Gradle properties
- Hardened Android manifest security defaults:
  - backup disabled
  - data extraction rules added
  - cleartext traffic disabled
- Added Android backup configuration XML files:
  - `res/xml/backup_rules.xml`
  - `res/xml/data_extraction_rules.xml`
- Improved mobile runtime ergonomics:
  - viewport updated with `viewport-fit=cover`
  - safe-area padding added at app root
- Aligned OTP flow to email-only on frontend auth path
- Upgraded Play Store documentation:
  - `docs/README_PLAYSTORE.md`
  - `docs/PLAYSTORE_RELEASE_CHECKLIST.md`
  - root `README.md` Android section

### Why
- Prepare the app for production-grade Google Play submission.
- Reduce auth confusion by removing mobile OTP path from active frontend flow.
- Improve security defaults for packaged Android builds.
- Provide a repeatable release process for future app updates.

### Files Changed
- `src/frontend/mastermind-web/android/app/build.gradle`
- `src/frontend/mastermind-web/android/app/src/main/AndroidManifest.xml`
- `src/frontend/mastermind-web/android/app/src/main/res/xml/backup_rules.xml`
- `src/frontend/mastermind-web/android/app/src/main/res/xml/data_extraction_rules.xml`
- `src/frontend/mastermind-web/index.html`
- `src/frontend/mastermind-web/src/App.vue`
- `src/frontend/mastermind-web/src/assets/styles/main.css`
- `src/frontend/mastermind-web/src/services/authService.ts`
- `src/frontend/mastermind-web/src/stores/auth.ts`
- `src/frontend/mastermind-web/src/types/auth.ts`
- `src/frontend/mastermind-web/src/views/auth/EnhancedLoginView.vue`
- `src/frontend/mastermind-web/src/views/auth/OtpVerifyView.vue`
- `docs/README_PLAYSTORE.md`
- `docs/PLAYSTORE_RELEASE_CHECKLIST.md`
- `README.md`

## [2026-04-17] - AI Documentation System

### Added
- `docs/ai/gotchas.md` - Lessons learned from bug fixes
- `docs/ai/gotchas-index.json` - Symptom-based quick lookup
- `docs/ai/full-api-reference.md` - Complete API documentation
- `docs/ai/api-endpoint-index.json` - Route-to-file mapping
- `docs/ai/performance-gotchas.md` - Performance best practices
- `docs/ai/database-schema.md` - Entity documentation (auto-generated)
- `AGENTS.md` - AI assistant rules for consistent behavior
- `.windsurf/workflows/ai-assisted-development.md` - User guide
- `scripts/Generate-ApiDocs.ps1` - Auto-generates API documentation
- `scripts/Generate-SchemaDoc.ps1` - Auto-generates schema documentation
- `scripts/Generate-CallGraph.ps1` - Generates dependency analysis
- `scripts/pre-commit-hook.ps1` - Validates code before commits

### Why
- AI assistants were making the same mistakes repeatedly
- Token usage was high due to scanning entire codebase
- Different AI editors behaved inconsistently
- New developers (and AI) had no quick reference for gotchas

---

## [2026-04-17] - Auth & API Fixes

### Fixed
- **FeesController** - LINQ-to-SQL translation errors (500 errors)
- **ExpensesController** - StringComparison not translatable to SQL
- **FeeCollectionController** - Added missing GET endpoint
- **All Controllers** - Added `[Authorize]` attribute for security
- **Frontend auth.ts** - Fixed Pinia persistence conflict
- **Frontend apiService.ts** - Fixed token extraction from localStorage
- **Quick login** - Fixed roles array to single role mapping

### Why
- Protected endpoints were accessible without authentication
- Users experienced redirect loops after login
- Finance endpoints returned 500 errors

### Files Changed
- `src/backend/MasterMind.API/Controllers/FeesController.cs`
- `src/backend/MasterMind.API/Controllers/ExpensesController.cs`
- `src/backend/MasterMind.API/Controllers/FeeCollectionController.cs`
- `src/backend/MasterMind.API/Controllers/StudentsController.cs`
- `src/backend/MasterMind.API/Controllers/TeachersController.cs`
- `src/backend/MasterMind.API/Controllers/ClassesController.cs`
- `src/backend/MasterMind.API/Controllers/AttendanceController.cs`
- `src/frontend/mastermind-web/src/stores/auth.ts`
- `src/frontend/mastermind-web/src/services/apiService.ts`
- `src/frontend/mastermind-web/src/types/auth.ts`

---

## [2026-04-16] - Azure Deployment

### Added
- GitHub Actions CI/CD workflow (`.github/workflows/azure-deploy.yml`)
- Azure App Service configuration
- Azure Static Web App configuration
- `staticwebapp.config.json` for SPA routing

### Changed
- `Program.cs` - Multi-database support (Azure SQL / PostgreSQL / SQLite)
- `MasterMindDbContext.cs` - Replaced `GETUTCDATE()` with `DateTime.UtcNow`
- Frontend `api.ts` - Reads `VITE_API_BASE_URL` environment variable

### Removed
- Hardcoded secrets from `appsettings.json`
- 30+ unused files and old documentation
- Railway deployment configurations

### Why
- Migrating from local development to Azure cloud
- Needed automated deployments on push to main
- Security: secrets should not be in code

---

## How to Use This Changelog

### For AI Assistants
1. Check this file to understand recent changes
2. Look for "Why" sections to understand context
3. "Files Changed" helps locate relevant code

### For Developers
1. Review before starting work to understand recent context
2. Add entries when making significant changes
3. Include "Why" to help future developers (and AI)

### Entry Format
```markdown
## [YYYY-MM-DD] - Short Title

### Added/Changed/Fixed/Removed
- Description of change

### Why
- Reason for the change

### Files Changed (optional)
- List of affected files
```

---

*Update this file when making significant changes to help AI assistants understand project evolution.*
