namespace MasterMind.API.Models.Entities;

/// <summary>
/// Exam entity for managing examinations
/// </summary>
public class Exam : BaseEntity
{
    public string Name { get; set; } = string.Empty; // e.g., "Mid-Term Exam", "Final Exam"
    public string? Description { get; set; }
    public int ClassId { get; set; }
    public int? SubjectId { get; set; } // Null for general exams covering all subjects
    public ExamType Type { get; set; } = ExamType.Written;
    public DateTime ExamDate { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public int DurationMinutes { get; set; } = 180; // Default 3 hours
    public decimal MaxMarks { get; set; } = 100;
    public decimal PassingMarks { get; set; } = 33;
    public string? Syllabus { get; set; }
    public string? Instructions { get; set; }
    public string AcademicYear { get; set; } = string.Empty;
    public int? SessionId { get; set; }
    public ExamStatus Status { get; set; } = ExamStatus.Scheduled;
    public int? CreatedByUserId { get; set; }

    // Navigation properties
    public Class Class { get; set; } = null!;
    public Subject? Subject { get; set; }
    public Session? Session { get; set; }
    public User? CreatedByUser { get; set; }
    public ICollection<ExamResult> ExamResults { get; set; } = new List<ExamResult>();
}

public enum ExamType
{
    Written,
    Oral,
    Practical,
    Online,
    Assignment,
    Project,
    Quiz,
    MidTerm,
    Final
}

public enum ExamStatus
{
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
    Postponed
}
