namespace MasterMind.API.Models.Entities;

/// <summary>
/// Teacher entity for teacher management
/// </summary>
public class Teacher : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PinCode { get; set; }
    public string? Qualification { get; set; }
    public string? Specialization { get; set; }
    public int? ExperienceYears { get; set; }
    public DateTime JoiningDate { get; set; } = DateTime.UtcNow;
    public DateTime? LeavingDate { get; set; }
    public decimal? MonthlySalary { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? EmployeeId { get; set; }
    public bool IsActive { get; set; } = true;
    public int? UserId { get; set; }

    // Navigation properties
    public User? User { get; set; }
    public ICollection<TeacherClass> TeacherClasses { get; set; } = new List<TeacherClass>();
    public ICollection<TeacherAttendance> TeacherAttendances { get; set; } = new List<TeacherAttendance>();
    public ICollection<TeacherSalary> TeacherSalaries { get; set; } = new List<TeacherSalary>();
    public ICollection<StudentRemark> StudentRemarks { get; set; } = new List<StudentRemark>();

    // Computed properties
    public string FullName => $"{FirstName} {LastName}";
}
