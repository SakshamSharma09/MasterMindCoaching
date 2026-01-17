namespace MasterMind.API.Models.Entities;

/// <summary>
/// Teacher attendance entity for tracking teacher attendance
/// </summary>
public class TeacherAttendance : BaseEntity
{
    public int TeacherId { get; set; }
    public DateOnly Date { get; set; }
    public AttendanceStatus Status { get; set; }
    public TimeOnly? CheckInTime { get; set; }
    public TimeOnly? CheckOutTime { get; set; }
    public string? Remarks { get; set; }
    public int? MarkedByUserId { get; set; }
    public DateTime MarkedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Teacher Teacher { get; set; } = null!;
    public User? MarkedByUser { get; set; }
}
