namespace MasterMind.API.Models.Entities;

/// <summary>
/// Junction table for Teacher-Class many-to-many relationship
/// </summary>
public class TeacherClass
{
    public int TeacherId { get; set; }
    public int ClassId { get; set; }
    public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsPrimary { get; set; } = false; // Primary teacher for the class

    // Navigation properties
    public Teacher Teacher { get; set; } = null!;
    public Class Class { get; set; } = null!;
}
