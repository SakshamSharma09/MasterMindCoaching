namespace MasterMind.API.Models.Entities;

/// <summary>
/// Attendance entity for student attendance tracking
/// </summary>
public class Attendance : BaseEntity
{
    public int StudentId { get; set; }
    public int ClassId { get; set; }
    public DateOnly Date { get; set; }
    public AttendanceStatus Status { get; set; }
    public TimeOnly? CheckInTime { get; set; }
    public TimeOnly? CheckOutTime { get; set; }
    public string? Remarks { get; set; }
    public int? MarkedByUserId { get; set; }
    public DateTime MarkedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Student Student { get; set; } = null!;
    public Class Class { get; set; } = null!;
    public User? MarkedByUser { get; set; }
}

public enum AttendanceStatus
{
    Present,
    Absent,
    Late,
    HalfDay,
    Holiday,
    Leave
}
