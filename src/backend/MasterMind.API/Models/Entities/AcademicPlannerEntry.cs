namespace MasterMind.API.Models.Entities;

public class AcademicPlannerEntry : BaseEntity
{
    public int? SessionId { get; set; }
    public int? StudentId { get; set; }
    public int? ClassId { get; set; }
    public string SchoolName { get; set; } = string.Empty;
    public PlannerExamType ExamType { get; set; } = PlannerExamType.UnitTest;
    public string Subject { get; set; } = string.Empty;
    public string Syllabus { get; set; } = string.Empty;
    public DateOnly ExamDate { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public string? Notes { get; set; }

    public Session? Session { get; set; }
    public Student? Student { get; set; }
    public Class? Class { get; set; }
}

public enum PlannerExamType
{
    UnitTest,
    HalfYearly,
    Yearly,
    Custom
}
