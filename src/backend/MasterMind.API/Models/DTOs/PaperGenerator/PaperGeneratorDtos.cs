using MasterMind.API.Models.Entities;

namespace MasterMind.API.Models.DTOs.PaperGenerator;

public class PaperDocumentDto
{
    public int Id { get; set; }
    public int? SessionId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public PaperDocumentStatus Status { get; set; }
    public int? PageCount { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime UploadedAt { get; set; }
    public DateTime? RetainUntil { get; set; }
}

public class PaperQuestionDto
{
    public int Id { get; set; }
    public int? SessionId { get; set; }
    public int? SourceDocumentId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string? Chapter { get; set; }
    public int Marks { get; set; }
    public PaperQuestionType QuestionType { get; set; }
    public PaperQuestionDifficulty Difficulty { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string? AnswerText { get; set; }
    public string SourceMode { get; set; } = string.Empty;
}

public class PaperGenerationJobDto
{
    public int Id { get; set; }
    public int? SessionId { get; set; }
    public PaperGenerationStatus Status { get; set; }
    public string StatusMessage { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? Chapter { get; set; }
    public int TotalMarks { get; set; }
    public int DurationMinutes { get; set; }
    public int RelevancePercentage { get; set; }
    public string? AiModelUsed { get; set; }
    public string? ErrorMessage { get; set; }
    public bool HasPaper { get; set; }
    public bool HasAnswerKey { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? RetainUntil { get; set; }
    public IReadOnlyList<PaperDocumentDto> Documents { get; set; } = Array.Empty<PaperDocumentDto>();
}

public class CreatePaperGenerationJobRequest
{
    public int? SessionId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? Chapter { get; set; }
    public int TotalMarks { get; set; } = 80;
    public int DurationMinutes { get; set; } = 180;
    public int McqCount { get; set; } = 10;
    public int OneMarkCount { get; set; } = 10;
    public int TwoMarkCount { get; set; } = 5;
    public int FiveMarkCount { get; set; } = 4;
    public int CaseStudyCount { get; set; } = 1;
    public int EasyPercentage { get; set; } = 20;
    public int MediumPercentage { get; set; } = 50;
    public int HardPercentage { get; set; } = 30;
    public int RelevancePercentage { get; set; } = 80;
    public IReadOnlyList<int> SelectedDocumentIds { get; set; } = Array.Empty<int>();
}

public class PaperGenerationJobSummary
{
    public PaperGenerationJobDto Job { get; set; } = new();
    public int QuestionCount { get; set; }
    public IReadOnlyList<PaperQuestionDto> PreviewQuestions { get; set; } = Array.Empty<PaperQuestionDto>();
}
