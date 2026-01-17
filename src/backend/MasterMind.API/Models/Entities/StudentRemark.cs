namespace MasterMind.API.Models.Entities;

/// <summary>
/// Student remark entity for chapter-wise remarks by teachers
/// </summary>
public class StudentRemark : BaseEntity
{
    public int StudentId { get; set; }
    public int TeacherId { get; set; }
    public int? ClassId { get; set; }
    public string? Subject { get; set; }
    public string? ChapterName { get; set; }
    public string? TopicName { get; set; }
    public string Remarks { get; set; } = string.Empty;
    public RemarkType Type { get; set; } = RemarkType.General;
    public int? Rating { get; set; } // 1-5 rating
    public DateTime RemarkDate { get; set; } = DateTime.UtcNow;
    public bool IsVisibleToParent { get; set; } = true;

    // Navigation properties
    public Student Student { get; set; } = null!;
    public Teacher Teacher { get; set; } = null!;
    public Class? Class { get; set; }
}

public enum RemarkType
{
    General,
    Academic,
    Behavior,
    Attendance,
    Improvement,
    Achievement,
    Concern
}
