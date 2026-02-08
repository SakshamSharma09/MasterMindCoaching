using MasterMind.API.Models.Entities;

namespace MasterMind.API.Models.Entities;

/// <summary>
/// Subject entity for managing subjects
/// </summary>
public class Subject : BaseEntity
{
    public string Name { get; set; } = string.Empty; // e.g., "Mathematics", "Physics"
    public string? Code { get; set; } // e.g., "MATH101", "PHY201"
    public string? Description { get; set; }
    public string? Category { get; set; } // e.g., "Science", "Commerce", "Arts"
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<ClassSubject> ClassSubjects { get; set; } = new List<ClassSubject>();
}

/// <summary>
/// ClassSubject junction entity for many-to-many relationship between Classes and Subjects
/// </summary>
public class ClassSubject
{
    public int ClassId { get; set; }
    public int SubjectId { get; set; }
    
    // Additional properties for the relationship
    public string? TeacherAssigned { get; set; } // Optional: Teacher assigned for this specific subject in this class
    public int? MaxStudents { get; set; } // Optional: Max students for this specific subject
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;

    // Navigation properties
    public Class Class { get; set; } = null!;
    public Subject Subject { get; set; } = null!;
}
