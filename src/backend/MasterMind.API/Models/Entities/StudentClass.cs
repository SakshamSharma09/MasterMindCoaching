namespace MasterMind.API.Models.Entities;

/// <summary>
/// Junction table for Student-Class many-to-many relationship
/// </summary>
public class StudentClass
{
    public int StudentId { get; set; }
    public int ClassId { get; set; }
    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Remarks { get; set; }

    // Navigation properties
    public Student Student { get; set; } = null!;
    public Class Class { get; set; } = null!;
}
