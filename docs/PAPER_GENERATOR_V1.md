# Paper Generator V1

## What Is Included

Paper Generator is an Admin-only module for creating branded question papers and answer keys from uploaded source PDFs and the reusable question bank.

Current implementation:

- Admin sidebar route: `/admin/paper-generator`
- Private PDF upload endpoints under `/api/paper-generator`
- Azure Blob private source/generated containers
- EF Core tables for uploaded documents, jobs, generated question bank, and job-document links
- QuestPDF-generated question paper and answer key downloads
- Session-specific document/job/question scoping
- Validation for max 5 PDFs and 25 MB per file

## Required Azure Settings

The backend reuses the existing Blob Storage connection string resolution. At least one of these settings must exist on the App Service:

- `AzureBlobStorage__ConnectionString`
- `AzureBlobStorageConnectionString`
- `ConnectionStrings__AzureBlobStorage`
- `CUSTOMCONNSTR_AzureBlobStorage`
- `AzureWebJobsStorage`

The module creates private containers automatically:

- `paper-source-documents`
- `paper-generated`

Recommended Azure lifecycle rules:

- Delete `paper-source-documents/paper-generator/*` after 7 days
- Delete `paper-generated/paper-generator/*` after 30 days

## Database

Migration added:

`src/backend/MasterMind.API/Data/Migrations/20260622025520_AddPaperGeneratorModule.cs`

New tables:

- `PaperDocuments`
- `PaperExtractedQuestions`
- `PaperGenerationJobs`
- `PaperJobDocuments`

The migration was manually narrowed to only these Paper Generator tables to avoid recreating older tables that already exist in production.

## API

All routes require Admin role.

- `POST /api/paper-generator/documents`
- `GET /api/paper-generator/documents`
- `POST /api/paper-generator/jobs`
- `GET /api/paper-generator/jobs`
- `GET /api/paper-generator/jobs/{id}`
- `GET /api/paper-generator/jobs/{id}/paper`
- `GET /api/paper-generator/jobs/{id}/answer-key`
- `GET /api/paper-generator/questions`

## V1 OCR/AI Status

The code includes an OpenRouter service boundary with DeepSeek as the default model and GLM fallback. If `OpenRouter__ApiKey` is not configured or the provider fails, V1 falls back to deterministic question generation so the feature remains runnable.

OCR is still intentionally separated for the next Azure Container App Job pass. V1 records source PDF metadata, reuses the question bank where possible, and generates real branded PDFs.

Next implementation pass:

- Add Azure Container App Job image with `ocrmypdf` and Tesseract
- Add job trigger from `PaperGenerationService`
- Persist OCR page text in `paper-ocr-cache`
- Expand OpenRouter prompts to include extracted OCR text and strict source-grounding

## Verification

Run:

```powershell
dotnet build src\backend\MasterMind.API\MasterMind.API.csproj
cd src\frontend\mastermind-web
npm run build
npm run build:aab
```
