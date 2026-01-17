namespace MasterMind.API.Models.Entities;

/// <summary>
/// Student performance entity for tracking test scores and performance
/// </summary>
public class StudentPerformance : BaseEntity
{
    public int StudentId { get; set; }
    public int? ClassId { get; set; }
    public string? Subject { get; set; }
    public string TestName { get; set; } = string.Empty;
    public TestType Type { get; set; }
    public decimal Score { get; set; }
    public decimal MaxScore { get; set; }
    public decimal? Percentage { get; set; }
    public string? Grade { get; set; }
    public int? Rank { get; set; }
    public DateOnly TestDate { get; set; }
    public string? Remarks { get; set; }
    public int? EvaluatedByTeacherId { get; set; }

    // Navigation properties
    public Student Student { get; set; } = null!;
    public Class? Class { get; set; }
    public Teacher? EvaluatedByTeacher { get; set; }

    // Computed properties
    public decimal CalculatedPercentage => MaxScore > 0 ? (Score / MaxScore) * 100 : 0;
}

public enum TestType
{
    Quiz,
    ClassTest,
    UnitTest,
    MidTerm,
    Final,
    Practice,
    Assignment,
    Project,
    Other
}
