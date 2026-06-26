using System.Text.Json.Serialization;

namespace MasterMind.API.Models.Entities;

public class PaperDocument : BaseEntity
{
    public int? SessionId { get; set; }
    public int UploadedByUserId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/pdf";
    public long SizeBytes { get; set; }
    public string BlobContainer { get; set; } = "paper-source-documents";
    public string BlobName { get; set; } = string.Empty;
    public PaperDocumentStatus Status { get; set; } = PaperDocumentStatus.Uploaded;
    public int? PageCount { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RetainUntil { get; set; }

    [JsonIgnore]
    public Session? Session { get; set; }

    [JsonIgnore]
    public User? UploadedByUser { get; set; }

    [JsonIgnore]
    public ICollection<PaperExtractedQuestion> ExtractedQuestions { get; set; } = new List<PaperExtractedQuestion>();

    [JsonIgnore]
    public ICollection<PaperJobDocument> JobDocuments { get; set; } = new List<PaperJobDocument>();
}

public class PaperExtractedQuestion : BaseEntity
{
    public int? SessionId { get; set; }
    public int? SourceDocumentId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string? Chapter { get; set; }
    public int Marks { get; set; } = 1;
    public PaperQuestionType QuestionType { get; set; } = PaperQuestionType.ShortAnswer;
    public PaperQuestionDifficulty Difficulty { get; set; } = PaperQuestionDifficulty.Medium;
    public string QuestionText { get; set; } = string.Empty;
    public string? AnswerText { get; set; }
    public int? SourcePageNumber { get; set; }
    public string SourceMode { get; set; } = "Generated";

    [JsonIgnore]
    public Session? Session { get; set; }

    [JsonIgnore]
    public PaperDocument? SourceDocument { get; set; }
}

public class PaperGenerationJob : BaseEntity
{
    public int? SessionId { get; set; }
    public int RequestedByUserId { get; set; }
    public PaperGenerationStatus Status { get; set; } = PaperGenerationStatus.Queued;
    public string StatusMessage { get; set; } = "Queued";
    public string ClassName { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? Chapter { get; set; }
    public int TotalMarks { get; set; }
    public int DurationMinutes { get; set; }
    public int RelevancePercentage { get; set; }
    public string SettingsJson { get; set; } = "{}";
    public string? AiModelUsed { get; set; }
    public string? ErrorMessage { get; set; }
    public string? GeneratedPaperBlobContainer { get; set; }
    public string? GeneratedPaperBlobName { get; set; }
    public string? AnswerKeyBlobContainer { get; set; }
    public string? AnswerKeyBlobName { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? RetainUntil { get; set; }

    [JsonIgnore]
    public Session? Session { get; set; }

    [JsonIgnore]
    public User? RequestedByUser { get; set; }

    [JsonIgnore]
    public ICollection<PaperJobDocument> JobDocuments { get; set; } = new List<PaperJobDocument>();
}

public class PaperJobDocument
{
    public int PaperGenerationJobId { get; set; }
    public int PaperDocumentId { get; set; }

    [JsonIgnore]
    public PaperGenerationJob? PaperGenerationJob { get; set; }

    [JsonIgnore]
    public PaperDocument? PaperDocument { get; set; }
}

public enum PaperDocumentStatus
{
    Uploaded,
    Processing,
    Extracted,
    Failed,
    Expired
}

public enum PaperGenerationStatus
{
    Queued,
    Uploaded,
    Ocr,
    QuestionExtraction,
    AiGeneration,
    PdfGeneration,
    Completed,
    Failed
}

public enum PaperQuestionType
{
    Mcq,
    FillBlank,
    OneMark,
    TwoMark,
    FiveMark,
    CaseStudy,
    ShortAnswer
}

public enum PaperQuestionDifficulty
{
    Easy,
    Medium,
    Hard
}
