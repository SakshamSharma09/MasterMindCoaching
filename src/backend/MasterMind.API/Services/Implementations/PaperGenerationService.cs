using System.Security.Cryptography;
using System.Text.Json;
using MasterMind.API.Data;
using MasterMind.API.Models.DTOs.PaperGenerator;
using MasterMind.API.Models.Entities;
using MasterMind.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MasterMind.API.Services.Implementations;

public class PaperGenerationService : IPaperGenerationService
{
    private const string GeneratedContainer = "paper-generated";

    private readonly MasterMindDbContext _context;
    private readonly IPaperDocumentStorageService _storage;
    private readonly IOpenRouterPaperService _openRouterPaperService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PaperGenerationService> _logger;

    public PaperGenerationService(
        MasterMindDbContext context,
        IPaperDocumentStorageService storage,
        IOpenRouterPaperService openRouterPaperService,
        IConfiguration configuration,
        ILogger<PaperGenerationService> logger)
    {
        _context = context;
        _storage = storage;
        _openRouterPaperService = openRouterPaperService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<PaperGenerationJobSummary> CreateAndRunJobAsync(CreatePaperGenerationJobRequest request, int adminUserId, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);
        var sessionId = request.SessionId ?? await GetActiveSessionIdAsync(cancellationToken);
        var retentionDays = _configuration.GetValue("PaperGenerator:GeneratedRetentionDays", 30);

        var selectedDocuments = await _context.PaperDocuments
            .Where(d => request.SelectedDocumentIds.Contains(d.Id) && !d.IsDeleted)
            .Where(d => !sessionId.HasValue || d.SessionId == sessionId)
            .ToListAsync(cancellationToken);

        if (request.SelectedDocumentIds.Count > 0 && selectedDocuments.Count != request.SelectedDocumentIds.Count)
        {
            throw new InvalidOperationException("One or more selected PDFs were not found for the active session.");
        }

        var job = new PaperGenerationJob
        {
            SessionId = sessionId,
            RequestedByUserId = adminUserId,
            ClassName = request.ClassName.Trim(),
            Subject = request.Subject.Trim(),
            Chapter = string.IsNullOrWhiteSpace(request.Chapter) ? null : request.Chapter.Trim(),
            TotalMarks = request.TotalMarks,
            DurationMinutes = request.DurationMinutes,
            RelevancePercentage = request.RelevancePercentage,
            SettingsJson = JsonSerializer.Serialize(request),
            Status = PaperGenerationStatus.Queued,
            StatusMessage = "Queued",
            RetainUntil = DateTime.UtcNow.AddDays(retentionDays),
            CreatedAt = DateTime.UtcNow
        };

        foreach (var document in selectedDocuments)
        {
            job.JobDocuments.Add(new PaperJobDocument { PaperDocument = document });
        }

        _context.PaperGenerationJobs.Add(job);
        await _context.SaveChangesAsync(cancellationToken);

        try
        {
            await RunJobAsync(job.Id, request, selectedDocuments, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Paper generation job {JobId} failed", job.Id);
            var failed = await _context.PaperGenerationJobs.FirstAsync(j => j.Id == job.Id, cancellationToken);
            failed.Status = PaperGenerationStatus.Failed;
            failed.StatusMessage = "Failed";
            failed.ErrorMessage = ex.Message;
            failed.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }

        var summary = await BuildJobSummaryAsync(job.Id, adminUserId, cancellationToken);
        if (summary == null)
        {
            throw new InvalidOperationException("Paper generation job could not be loaded after creation.");
        }

        return summary;
    }

    public async Task<PaperGenerationJobDto?> GetJobAsync(int jobId, int adminUserId, CancellationToken cancellationToken = default)
    {
        var job = await LoadJobQuery()
            .FirstOrDefaultAsync(j => j.Id == jobId && j.RequestedByUserId == adminUserId && !j.IsDeleted, cancellationToken);

        return job == null ? null : MapJob(job);
    }

    public async Task<IReadOnlyList<PaperGenerationJobDto>> GetRecentJobsAsync(int? sessionId, int adminUserId, CancellationToken cancellationToken = default)
    {
        var query = LoadJobQuery()
            .Where(j => j.RequestedByUserId == adminUserId && !j.IsDeleted);

        if (sessionId.HasValue)
        {
            query = query.Where(j => j.SessionId == sessionId.Value);
        }

        var jobs = await query
            .OrderByDescending(j => j.CreatedAt)
            .Take(30)
            .ToListAsync(cancellationToken);

        return jobs.Select(MapJob).ToList();
    }

    public async Task<IReadOnlyList<PaperQuestionDto>> SearchQuestionsAsync(int? sessionId, string? subject, string? className, string? chapter, CancellationToken cancellationToken = default)
    {
        var query = _context.PaperExtractedQuestions
            .AsNoTracking()
            .Where(q => !q.IsDeleted);

        if (sessionId.HasValue)
        {
            query = query.Where(q => q.SessionId == sessionId.Value);
        }

        if (!string.IsNullOrWhiteSpace(subject))
        {
            query = query.Where(q => q.Subject == subject.Trim());
        }

        if (!string.IsNullOrWhiteSpace(className))
        {
            query = query.Where(q => q.ClassName == className.Trim());
        }

        if (!string.IsNullOrWhiteSpace(chapter))
        {
            query = query.Where(q => q.Chapter != null && q.Chapter.Contains(chapter.Trim()));
        }

        var questions = await query
            .OrderByDescending(q => q.CreatedAt)
            .Take(100)
            .ToListAsync(cancellationToken);

        return questions.Select(MapQuestion).ToList();
    }

    public async Task<(Stream Content, string FileName)> DownloadGeneratedFileAsync(int jobId, PaperGeneratedFileKind kind, int adminUserId, CancellationToken cancellationToken = default)
    {
        var job = await _context.PaperGenerationJobs
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.Id == jobId && j.RequestedByUserId == adminUserId && !j.IsDeleted, cancellationToken);

        if (job == null)
        {
            throw new KeyNotFoundException("Paper generation job not found.");
        }

        var container = kind == PaperGeneratedFileKind.Paper ? job.GeneratedPaperBlobContainer : job.AnswerKeyBlobContainer;
        var blobName = kind == PaperGeneratedFileKind.Paper ? job.GeneratedPaperBlobName : job.AnswerKeyBlobName;

        if (string.IsNullOrWhiteSpace(container) || string.IsNullOrWhiteSpace(blobName))
        {
            throw new InvalidOperationException("The requested PDF is not ready yet.");
        }

        var stream = await _storage.DownloadAsync(container, blobName, cancellationToken);
        var suffix = kind == PaperGeneratedFileKind.Paper ? "question-paper" : "answer-key";
        var fileName = $"mastermind-{job.Subject}-{job.ClassName}-{suffix}-{job.Id}.pdf".Replace(' ', '-').ToLowerInvariant();
        return (stream, fileName);
    }

    private async Task RunJobAsync(int jobId, CreatePaperGenerationJobRequest request, IReadOnlyList<PaperDocument> documents, CancellationToken cancellationToken)
    {
        if (!_storage.IsConfigured)
        {
            throw new InvalidOperationException("Azure Blob Storage is required for Paper Generator generated PDFs.");
        }

        var job = await _context.PaperGenerationJobs.FirstAsync(j => j.Id == jobId, cancellationToken);
        job.StartedAt = DateTime.UtcNow;
        await UpdateStatusAsync(job, PaperGenerationStatus.Uploaded, "PDFs uploaded", cancellationToken);

        await UpdateStatusAsync(job, PaperGenerationStatus.Ocr, "OCR worker pending; using available text/question bank for V1", cancellationToken);
        foreach (var document in documents)
        {
            document.Status = PaperDocumentStatus.Extracted;
            document.UpdatedAt = DateTime.UtcNow;
        }
        await _context.SaveChangesAsync(cancellationToken);

        await UpdateStatusAsync(job, PaperGenerationStatus.QuestionExtraction, "Building reusable question bank", cancellationToken);
        var selectedQuestions = await SelectQuestionsAsync(request, job.SessionId, cancellationToken);
        var missingCount = Math.Max(0, request.McqCount + request.OneMarkCount + request.TwoMarkCount + request.FiveMarkCount + request.CaseStudyCount - selectedQuestions.Count);
        var generatedQuestions = (await _openRouterPaperService.GenerateQuestionsAsync(request, missingCount, job.SessionId, cancellationToken)).ToList();
        if (generatedQuestions.Count < missingCount)
        {
            generatedQuestions.AddRange(BuildMissingQuestions(request, selectedQuestions.Count + generatedQuestions.Count, job.SessionId));
        }
        if (generatedQuestions.Count > 0)
        {
            _context.PaperExtractedQuestions.AddRange(generatedQuestions);
            await _context.SaveChangesAsync(cancellationToken);
            selectedQuestions.AddRange(generatedQuestions);
        }

        await UpdateStatusAsync(job, PaperGenerationStatus.AiGeneration, "Structuring paper and answer key", cancellationToken);
        job.AiModelUsed = _configuration["OpenRouter:DefaultModel"] ?? "deterministic-v1";
        await _context.SaveChangesAsync(cancellationToken);

        await UpdateStatusAsync(job, PaperGenerationStatus.PdfGeneration, "Generating branded PDFs", cancellationToken);
        var paperPdf = BuildPaperPdf(job, selectedQuestions, false);
        var answerPdf = BuildPaperPdf(job, selectedQuestions, true);
        var baseBlobPath = $"paper-generator/{job.SessionId?.ToString() ?? "global"}/{job.RequestedByUserId}/{job.Id}";

        await _storage.UploadAsync(GeneratedContainer, $"{baseBlobPath}/question-paper.pdf", new MemoryStream(paperPdf), "application/pdf", cancellationToken);
        await _storage.UploadAsync(GeneratedContainer, $"{baseBlobPath}/answer-key.pdf", new MemoryStream(answerPdf), "application/pdf", cancellationToken);

        job.GeneratedPaperBlobContainer = GeneratedContainer;
        job.GeneratedPaperBlobName = $"{baseBlobPath}/question-paper.pdf";
        job.AnswerKeyBlobContainer = GeneratedContainer;
        job.AnswerKeyBlobName = $"{baseBlobPath}/answer-key.pdf";
        job.Status = PaperGenerationStatus.Completed;
        job.StatusMessage = "Paper and answer key ready";
        job.CompletedAt = DateTime.UtcNow;
        job.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<List<PaperExtractedQuestion>> SelectQuestionsAsync(CreatePaperGenerationJobRequest request, int? sessionId, CancellationToken cancellationToken)
    {
        var neededCount = Math.Max(1, request.McqCount + request.OneMarkCount + request.TwoMarkCount + request.FiveMarkCount + request.CaseStudyCount);
        var sourceCount = Math.Clamp(neededCount * request.RelevancePercentage / 100, 0, neededCount);

        if (sourceCount == 0)
        {
            return new List<PaperExtractedQuestion>();
        }

        var query = _context.PaperExtractedQuestions
            .Where(q => !q.IsDeleted && q.Subject == request.Subject.Trim() && q.ClassName == request.ClassName.Trim());

        if (sessionId.HasValue)
        {
            query = query.Where(q => q.SessionId == sessionId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Chapter))
        {
            query = query.Where(q => q.Chapter == request.Chapter.Trim());
        }

        return await query
            .OrderByDescending(q => q.CreatedAt)
            .Take(sourceCount)
            .ToListAsync(cancellationToken);
    }

    private static List<PaperExtractedQuestion> BuildMissingQuestions(CreatePaperGenerationJobRequest request, int existingCount, int? sessionId)
    {
        var desired = new List<(PaperQuestionType Type, int Count, int Marks)>
        {
            (PaperQuestionType.Mcq, request.McqCount, 1),
            (PaperQuestionType.OneMark, request.OneMarkCount, 1),
            (PaperQuestionType.TwoMark, request.TwoMarkCount, 2),
            (PaperQuestionType.FiveMark, request.FiveMarkCount, 5),
            (PaperQuestionType.CaseStudy, request.CaseStudyCount, 5)
        };

        var totalNeeded = desired.Sum(x => x.Count);
        var remaining = Math.Max(0, totalNeeded - existingCount);
        var questions = new List<PaperExtractedQuestion>();
        var serial = existingCount + 1;

        foreach (var (type, count, marks) in desired)
        {
            for (var i = 0; i < count && questions.Count < remaining; i++)
            {
                questions.Add(new PaperExtractedQuestion
                {
                    SessionId = sessionId,
                    Subject = request.Subject.Trim(),
                    ClassName = request.ClassName.Trim(),
                    Chapter = string.IsNullOrWhiteSpace(request.Chapter) ? "General" : request.Chapter.Trim(),
                    Marks = marks,
                    QuestionType = type,
                    Difficulty = PickDifficulty(request, serial),
                    QuestionText = BuildQuestionText(request, type, serial),
                    AnswerText = BuildAnswerText(type, serial),
                    SourceMode = "Generated",
                    CreatedAt = DateTime.UtcNow
                });
                serial++;
            }
        }

        return questions;
    }

    private static PaperQuestionDifficulty PickDifficulty(CreatePaperGenerationJobRequest request, int serial)
    {
        var easyLimit = Math.Clamp(request.EasyPercentage, 0, 100);
        var mediumLimit = Math.Clamp(request.EasyPercentage + request.MediumPercentage, 0, 100);
        var bucket = (serial * 37) % 100;
        if (bucket < easyLimit) return PaperQuestionDifficulty.Easy;
        if (bucket < mediumLimit) return PaperQuestionDifficulty.Medium;
        return PaperQuestionDifficulty.Hard;
    }

    private static string BuildQuestionText(CreatePaperGenerationJobRequest request, PaperQuestionType type, int serial)
    {
        var chapter = string.IsNullOrWhiteSpace(request.Chapter) ? "the selected syllabus" : request.Chapter.Trim();
        return type switch
        {
            PaperQuestionType.Mcq => $"Choose the correct option for concept {serial} from {chapter} in {request.Subject}.",
            PaperQuestionType.CaseStudy => $"Read a short case based on {chapter} and answer the related analytical questions.",
            PaperQuestionType.FiveMark => $"Explain concept {serial} from {chapter} with reasoning, example, and conclusion.",
            PaperQuestionType.TwoMark => $"Write a concise answer for concept {serial} from {chapter}.",
            _ => $"Answer concept {serial} from {chapter} in {request.Subject}."
        };
    }

    private static string BuildAnswerText(PaperQuestionType type, int serial)
    {
        return type == PaperQuestionType.Mcq
            ? $"Correct option depends on source concept {serial}; verify from uploaded material."
            : $"Expected answer should cover the key point, method, and final result for concept {serial}.";
    }

    private static byte[] BuildPaperPdf(PaperGenerationJob job, IReadOnlyList<PaperExtractedQuestion> questions, bool answerKey)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(32);
                page.DefaultTextStyle(x => x.FontFamily(Fonts.Calibri).FontSize(10));

                page.Header().Column(header =>
                {
                    header.Item().Text("MASTERMIND COACHING CLASSES").FontSize(18).Bold().FontColor(Colors.Blue.Darken3).AlignCenter();
                    header.Item().Text(answerKey ? "Answer Key" : "Question Paper").FontSize(13).SemiBold().FontColor(Colors.Amber.Darken2).AlignCenter();
                    header.Item().PaddingTop(8).Row(row =>
                    {
                        row.RelativeItem().Text($"Class: {job.ClassName}").SemiBold();
                        row.RelativeItem().Text($"Subject: {job.Subject}").SemiBold().AlignCenter();
                        row.RelativeItem().Text($"Marks: {job.TotalMarks}").SemiBold().AlignRight();
                    });
                    header.Item().Row(row =>
                    {
                        row.RelativeItem().Text($"Time: {Math.Max(1, job.DurationMinutes / 60)} hour(s) {job.DurationMinutes % 60} min");
                        row.RelativeItem().Text($"Chapter: {job.Chapter ?? "Full syllabus"}").AlignRight();
                    });
                    header.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().PaddingVertical(14).Column(content =>
                {
                    foreach (var section in questions.GroupBy(q => q.QuestionType))
                    {
                        content.Item().PaddingTop(8).Text(GetSectionTitle(section.Key)).FontSize(12).Bold().FontColor(Colors.Blue.Darken2);
                        var index = 1;
                        foreach (var question in section)
                        {
                            content.Item().PaddingTop(6).Column(item =>
                            {
                                item.Item().Text($"{index}. ({question.Marks} mark{(question.Marks == 1 ? string.Empty : "s")}) {question.QuestionText}");
                                if (answerKey)
                                {
                                    item.Item().PaddingLeft(14).PaddingTop(2).Text($"Answer: {question.AnswerText ?? "To be evaluated by teacher."}").FontColor(Colors.Green.Darken2);
                                }
                            });
                            index++;
                        }
                    }
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("MasterMind Coaching Classes");
                    text.Span(" | ");
                    text.Span($"Generated: {DateTime.UtcNow:dd MMM yyyy}");
                });
            });
        }).GeneratePdf();
    }

    private static string GetSectionTitle(PaperQuestionType type) => type switch
    {
        PaperQuestionType.Mcq => "Section A - Multiple Choice Questions",
        PaperQuestionType.OneMark => "Section B - One Mark Questions",
        PaperQuestionType.TwoMark => "Section C - Two Mark Questions",
        PaperQuestionType.FiveMark => "Section D - Long Answer Questions",
        PaperQuestionType.CaseStudy => "Section E - Case Study",
        PaperQuestionType.FillBlank => "Fill in the Blanks",
        _ => "Short Answer Questions"
    };

    private async Task UpdateStatusAsync(PaperGenerationJob job, PaperGenerationStatus status, string message, CancellationToken cancellationToken)
    {
        job.Status = status;
        job.StatusMessage = message;
        job.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<int?> GetActiveSessionIdAsync(CancellationToken cancellationToken)
    {
        return await _context.Sessions
            .Where(s => s.IsActive && !s.IsDeleted)
            .Select(s => (int?)s.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<PaperGenerationJobSummary?> BuildJobSummaryAsync(int jobId, int adminUserId, CancellationToken cancellationToken)
    {
        var job = await LoadJobQuery()
            .FirstOrDefaultAsync(j => j.Id == jobId && j.RequestedByUserId == adminUserId && !j.IsDeleted, cancellationToken);

        if (job == null)
        {
            return null;
        }

        var questions = await _context.PaperExtractedQuestions
            .AsNoTracking()
            .Where(q => q.SessionId == job.SessionId && q.Subject == job.Subject && q.ClassName == job.ClassName && !q.IsDeleted)
            .OrderByDescending(q => q.CreatedAt)
            .Take(10)
            .ToListAsync(cancellationToken);

        return new PaperGenerationJobSummary
        {
            Job = MapJob(job),
            QuestionCount = questions.Count,
            PreviewQuestions = questions.Select(MapQuestion).ToList()
        };
    }

    private IQueryable<PaperGenerationJob> LoadJobQuery()
    {
        return _context.PaperGenerationJobs
            .AsNoTracking()
            .Include(j => j.JobDocuments)
            .ThenInclude(jd => jd.PaperDocument);
    }

    private static void ValidateRequest(CreatePaperGenerationJobRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ClassName)) throw new InvalidOperationException("Class is required.");
        if (string.IsNullOrWhiteSpace(request.Subject)) throw new InvalidOperationException("Subject is required.");
        if (request.TotalMarks <= 0 || request.TotalMarks > 500) throw new InvalidOperationException("Total marks must be between 1 and 500.");
        if (request.DurationMinutes <= 0 || request.DurationMinutes > 360) throw new InvalidOperationException("Duration must be between 1 and 360 minutes.");
        if (request.SelectedDocumentIds.Count > 5) throw new InvalidOperationException("Select at most 5 source PDFs.");
        if (request.EasyPercentage + request.MediumPercentage + request.HardPercentage != 100) throw new InvalidOperationException("Difficulty split must total 100%.");
        if (request.RelevancePercentage is < 0 or > 100) throw new InvalidOperationException("Relevance must be between 0 and 100.");
    }

    private static PaperGenerationJobDto MapJob(PaperGenerationJob job)
    {
        return new PaperGenerationJobDto
        {
            Id = job.Id,
            SessionId = job.SessionId,
            Status = job.Status,
            StatusMessage = job.StatusMessage,
            ClassName = job.ClassName,
            Subject = job.Subject,
            Chapter = job.Chapter,
            TotalMarks = job.TotalMarks,
            DurationMinutes = job.DurationMinutes,
            RelevancePercentage = job.RelevancePercentage,
            AiModelUsed = job.AiModelUsed,
            ErrorMessage = job.ErrorMessage,
            HasPaper = !string.IsNullOrWhiteSpace(job.GeneratedPaperBlobName),
            HasAnswerKey = !string.IsNullOrWhiteSpace(job.AnswerKeyBlobName),
            CreatedAt = job.CreatedAt,
            StartedAt = job.StartedAt,
            CompletedAt = job.CompletedAt,
            RetainUntil = job.RetainUntil,
            Documents = job.JobDocuments.Select(jd => MapDocument(jd.PaperDocument!)).ToList()
        };
    }

    public static PaperDocumentDto MapDocument(PaperDocument document)
    {
        return new PaperDocumentDto
        {
            Id = document.Id,
            SessionId = document.SessionId,
            FileName = document.FileName,
            SizeBytes = document.SizeBytes,
            Status = document.Status,
            PageCount = document.PageCount,
            ErrorMessage = document.ErrorMessage,
            UploadedAt = document.UploadedAt,
            RetainUntil = document.RetainUntil
        };
    }

    private static PaperQuestionDto MapQuestion(PaperExtractedQuestion question)
    {
        return new PaperQuestionDto
        {
            Id = question.Id,
            SessionId = question.SessionId,
            SourceDocumentId = question.SourceDocumentId,
            Subject = question.Subject,
            ClassName = question.ClassName,
            Chapter = question.Chapter,
            Marks = question.Marks,
            QuestionType = question.QuestionType,
            Difficulty = question.Difficulty,
            QuestionText = question.QuestionText,
            AnswerText = question.AnswerText,
            SourceMode = question.SourceMode
        };
    }
}
