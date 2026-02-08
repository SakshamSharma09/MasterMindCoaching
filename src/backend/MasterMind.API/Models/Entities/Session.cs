using MasterMind.API.Models.Entities;

namespace MasterMind.API.Models.Entities;

/// <summary>
/// Academic Session entity for managing academic years with comprehensive tracking
/// </summary>
public class Session : BaseEntity
{
    public string Name { get; set; } = string.Empty; // e.g., "2025-26", "2026-27"
    public string DisplayName { get; set; } = string.Empty; // e.g., "Academic Year 2025-26"
    public string? Description { get; set; }
    
    // Academic year information
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string AcademicYear { get; set; } = string.Empty; // e.g., "2025-26"
    
    // Session status
    public bool IsActive { get; set; } = false; // Only one session can be active at a time
    public SessionStatus Status { get; set; } = SessionStatus.Planned;
    
    // Statistics (computed/updated in real-time)
    public int TotalStudents { get; set; } = 0;
    public int ActiveStudents { get; set; } = 0;
    public int TotalClasses { get; set; } = 0;
    public int ActiveClasses { get; set; } = 0;
    public int TotalTeachers { get; set; } = 0;
    public decimal TotalRevenue { get; set; } = 0;
    public decimal TotalExpenses { get; set; } = 0;
    
    // Configuration
    public string? Settings { get; set; } // JSON configuration for session-specific settings
    
    // Navigation properties
    public ICollection<Class> Classes { get; set; } = new List<Class>();
    public ICollection<Student> Students { get; set; } = new List<Student>();
    public ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();
}

/// <summary>
/// Session status enumeration
/// </summary>
public enum SessionStatus
{
    Planned = 1,      // Session is planned but not yet active
    Active = 2,       // Session is currently active
    Completed = 3,    // Session has been completed
    Suspended = 4,    // Session was suspended
    Cancelled = 5     // Session was cancelled
}
