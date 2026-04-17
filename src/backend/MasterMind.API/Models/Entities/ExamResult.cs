namespace MasterMind.API.Models.Entities;

/// <summary>
/// Exam result entity for tracking student performance in exams
/// </summary>
public class ExamResult : BaseEntity
{
    public int ExamId { get; set; }
    public int StudentId { get; set; }
    public decimal? MarksObtained { get; set; }
    public string? Grade { get; set; } // A+, A, B+, B, C, D, F
    public decimal? Percentage { get; set; }
    public ResultStatus Status { get; set; } = ResultStatus.Pending;
    public bool IsPassed { get; set; }
    public int? Rank { get; set; } // Class rank
    public string? Remarks { get; set; }
    public string? TeacherComments { get; set; }
    public int? EvaluatedByUserId { get; set; }
    public DateTime? EvaluatedAt { get; set; }

    // Navigation properties
    public Exam Exam { get; set; } = null!;
    public Student Student { get; set; } = null!;
    public User? EvaluatedByUser { get; set; }
}

public enum ResultStatus
{
    Pending,      // Not yet evaluated
    Evaluated,    // Marks entered
    Published,    // Results published to students/parents
    OnHold,       // Result on hold for verification
    Absent,       // Student was absent
    Disqualified  // Student disqualified
}
