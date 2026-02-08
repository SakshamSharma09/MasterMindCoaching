namespace MasterMind.API.Models.Entities;

/// <summary>
/// Student entity for student management
/// </summary>
public class Student : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PinCode { get; set; }
    public string? StudentMobile { get; set; }
    public string? StudentEmail { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? AdmissionNumber { get; set; }
    public DateTime AdmissionDate { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    // Parent Information
    public string ParentName { get; set; } = string.Empty;
    public string ParentMobile { get; set; } = string.Empty;
    public string? ParentEmail { get; set; }
    public string? ParentOccupation { get; set; }
    public int? ParentUserId { get; set; }
    public int? SessionId { get; set; }

    // Navigation properties
    public User? ParentUser { get; set; }
    public Session? Session { get; set; }
    public ICollection<StudentClass> StudentClasses { get; set; } = new List<StudentClass>();
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    public ICollection<StudentFee> StudentFees { get; set; } = new List<StudentFee>();
    public ICollection<StudentRemark> StudentRemarks { get; set; } = new List<StudentRemark>();
    public ICollection<StudentPerformance> StudentPerformances { get; set; } = new List<StudentPerformance>();

    // Computed properties
    public string FullName => $"{FirstName} {LastName}";
    public int Age => DateTime.Today.Year - DateOfBirth.Year - 
        (DateTime.Today.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);
}

public enum Gender
{
    Male,
    Female,
    Other
}
