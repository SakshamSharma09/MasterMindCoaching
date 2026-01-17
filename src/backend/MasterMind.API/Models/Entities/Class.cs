namespace MasterMind.API.Models.Entities;

/// <summary>
/// Class entity for class/section management
/// </summary>
public class Class : BaseEntity
{
    public string Name { get; set; } = string.Empty; // e.g., "Class 10", "Class 12"
    public string? Section { get; set; } // e.g., "A", "B", "Science", "Commerce"
    public string? Subject { get; set; } // e.g., "Mathematics", "Physics"
    public string AcademicYear { get; set; } = string.Empty; // e.g., "2024-25"
    public string? Description { get; set; }
    public int? MaxStudents { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public string? DaysOfWeek { get; set; } // e.g., "Mon,Wed,Fri"
    public decimal? MonthlyFee { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<StudentClass> StudentClasses { get; set; } = new List<StudentClass>();
    public ICollection<TeacherClass> TeacherClasses { get; set; } = new List<TeacherClass>();
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    public ICollection<FeeStructure> FeeStructures { get; set; } = new List<FeeStructure>();

    // Computed properties
    public string DisplayName => string.IsNullOrEmpty(Section) 
        ? Name 
        : $"{Name} - {Section}";
}
