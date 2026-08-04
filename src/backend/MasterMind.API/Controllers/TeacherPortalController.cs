using System.Security.Claims;
using MasterMind.API.Data;
using MasterMind.API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MasterMind.API.Controllers;

[ApiController]
[Route("api/teacher-portal")]
[Produces("application/json")]
[Authorize(Roles = "Teacher,Admin")]
public class TeacherPortalController : ControllerBase
{
    private readonly MasterMindDbContext _context;
    private readonly ILogger<TeacherPortalController> _logger;

    public TeacherPortalController(MasterMindDbContext context, ILogger<TeacherPortalController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("classes")]
    public async Task<ActionResult<ApiResponse<IEnumerable<object>>>> GetMyClasses()
    {
        try
        {
            var teacher = await ResolveTeacherForCurrentUserAsync();
            if (teacher == null)
            {
                return Ok(new ApiResponse<IEnumerable<object>>
                {
                    Success = true,
                    Message = "Teacher profile not found for current user",
                    Data = new List<object>()
                });
            }

            var classes = await _context.TeacherClasses
                .Where(tc => tc.TeacherId == teacher.Id && tc.IsActive && tc.Class != null && !tc.Class.IsDeleted && tc.Class.IsActive)
                .Select(tc => new
                {
                    tc.Class!.Id,
                    tc.Class.Name,
                    tc.Class.Board,
                    tc.Class.Medium
                })
                .OrderBy(c => c.Name)
                .ToListAsync();

            return Ok(new ApiResponse<IEnumerable<object>>
            {
                Success = true,
                Message = "Teacher classes retrieved successfully",
                Data = classes
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving teacher classes");
            return StatusCode(500, new ApiResponse<IEnumerable<object>>
            {
                Success = false,
                Message = "Error retrieving teacher classes",
                Data = new List<object>()
            });
        }
    }

    [HttpGet("classes/{classId:int}/students")]
    public async Task<ActionResult<ApiResponse<IEnumerable<object>>>> GetClassStudents(int classId)
    {
        try
        {
            var teacher = await ResolveTeacherForCurrentUserAsync();
            if (teacher == null)
            {
                return Ok(new ApiResponse<IEnumerable<object>>
                {
                    Success = true,
                    Message = "Teacher profile not found for current user",
                    Data = new List<object>()
                });
            }

            var isAllowed = await _context.TeacherClasses
                .AnyAsync(tc => tc.TeacherId == teacher.Id && tc.ClassId == classId && tc.IsActive);

            if (!isAllowed)
            {
                return Forbid();
            }

            var studentRows = await _context.StudentClasses
                .Where(sc => sc.ClassId == classId && sc.IsActive && sc.Student != null && !sc.Student.IsDeleted && sc.Student.IsActive)
                .Select(sc => new
                {
                    sc.Student.Id,
                    sc.Student.FirstName,
                    sc.Student.LastName,
                    sc.Student.AdmissionNumber,
                    ClassId = classId,
                })
                .ToListAsync();

            var students = studentRows
                .Select(s =>
                {
                    var name = $"{s.FirstName} {s.LastName}".Trim();
                    var initials = $"{(string.IsNullOrWhiteSpace(s.FirstName) ? "" : s.FirstName[0])}{(string.IsNullOrWhiteSpace(s.LastName) ? "" : s.LastName[0])}".ToUpperInvariant();
                    return new
                    {
                        s.Id,
                        Name = string.IsNullOrWhiteSpace(name) ? $"Student {s.Id}" : name,
                        Initials = string.IsNullOrWhiteSpace(initials) ? "NA" : initials,
                        RollNo = s.AdmissionNumber ?? $"STD-{s.Id}",
                        s.ClassId,
                        Attendance = "--",
                        AverageGrade = "N/A"
                    };
                })
                .OrderBy(s => s.Name)
                .ToList();

            return Ok(new ApiResponse<IEnumerable<object>>
            {
                Success = true,
                Message = "Class students retrieved successfully",
                Data = students
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving teacher class students");
            return StatusCode(500, new ApiResponse<IEnumerable<object>>
            {
                Success = false,
                Message = "Error retrieving class students",
                Data = new List<object>()
            });
        }
    }

    [HttpGet("classes/{classId:int}/attendance")]
    public async Task<ActionResult<ApiResponse<IEnumerable<object>>>> GetClassAttendance(int classId, [FromQuery] DateOnly date)
    {
        var teacher = await ResolveTeacherForCurrentUserAsync();
        if (teacher == null)
        {
            return NotFound(new ApiResponse<IEnumerable<object>> { Success = false, Message = "Teacher profile not found for current user" });
        }

        if (!await CanManageClassAsync(teacher.Id, classId)) return Forbid();

        var attendanceRows = await _context.Attendances
            .Where(a => !a.IsDeleted && a.ClassId == classId && a.Date == date)
            .Select(a => new
            {
                a.Id, a.StudentId, a.ClassId, a.Date,
                a.Status,
                a.CheckInTime, a.CheckOutTime, a.Remarks
            })
            .ToListAsync();
        var records = attendanceRows.Select(a => new
        {
            a.Id, a.StudentId, a.ClassId, a.Date,
            Status = a.Status.ToString(),
            a.CheckInTime, a.CheckOutTime, a.Remarks
        }).ToList();

        return Ok(new ApiResponse<IEnumerable<object>>
        {
            Success = true,
            Message = "Class attendance retrieved successfully",
            Data = records
        });
    }

    [HttpPost("classes/{classId:int}/attendance")]
    public async Task<ActionResult<ApiResponse<object>>> SaveClassAttendance(int classId, [FromBody] TeacherAttendanceRequest request)
    {
        if (request.Date == default || request.Records.Count == 0)
        {
            return BadRequest(new ApiResponse<object> { Success = false, Message = "Attendance date and at least one student record are required" });
        }

        var teacher = await ResolveTeacherForCurrentUserAsync();
        if (teacher == null)
        {
            return NotFound(new ApiResponse<object> { Success = false, Message = "Teacher profile not found for current user" });
        }

        if (!await CanManageClassAsync(teacher.Id, classId)) return Forbid();

        var studentIds = request.Records.Select(r => r.StudentId).Distinct().ToList();
        if (studentIds.Count != request.Records.Count)
        {
            return BadRequest(new ApiResponse<object> { Success = false, Message = "Each student can appear only once" });
        }

        var allowedStudentIds = await _context.StudentClasses
            .Where(sc => sc.ClassId == classId && sc.IsActive && studentIds.Contains(sc.StudentId) &&
                sc.Student != null && !sc.Student.IsDeleted && sc.Student.IsActive)
            .Select(sc => sc.StudentId)
            .Distinct()
            .ToListAsync();
        if (allowedStudentIds.Count != studentIds.Count)
        {
            return BadRequest(new ApiResponse<object> { Success = false, Message = "One or more students are not active members of this class" });
        }

        var existing = await _context.Attendances
            .Where(a => !a.IsDeleted && a.ClassId == classId && a.Date == request.Date && studentIds.Contains(a.StudentId))
            .ToDictionaryAsync(a => a.StudentId);
        var markedByUserId = GetCurrentUserId();
        var markedAt = DateTime.UtcNow;

        foreach (var record in request.Records)
        {
            if (!Enum.TryParse<AttendanceStatus>(record.Status, true, out var status))
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = $"Invalid attendance status for student {record.StudentId}" });
            }

            if (!existing.TryGetValue(record.StudentId, out var attendance))
            {
                attendance = new Attendance
                {
                    StudentId = record.StudentId,
                    ClassId = classId,
                    Date = request.Date,
                    CreatedAt = markedAt
                };
                _context.Attendances.Add(attendance);
            }

            var hasClassTime = status is AttendanceStatus.Present or AttendanceStatus.Late or AttendanceStatus.HalfDay;
            attendance.Status = status;
            attendance.CheckInTime = hasClassTime ? new TimeOnly(15, 0) : null;
            attendance.CheckOutTime = hasClassTime ? new TimeOnly(18, 0) : null;
            attendance.Remarks = string.IsNullOrWhiteSpace(record.Remarks) ? null : record.Remarks.Trim();
            attendance.MarkedByUserId = markedByUserId;
            attendance.MarkedAt = markedAt;
            attendance.UpdatedAt = markedAt;
        }

        await _context.SaveChangesAsync();
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = $"Attendance saved for {request.Records.Count} students",
            Data = new { Count = request.Records.Count, request.Date, ClassId = classId }
        });
    }

    private async Task<Teacher?> ResolveTeacherForCurrentUserAsync()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
        {
            var teacherByUser = await _context.Teachers.FirstOrDefaultAsync(t => !t.IsDeleted && t.UserId == userId);
            if (teacherByUser != null)
            {
                return teacherByUser;
            }
        }

        var email = User.FindFirst(ClaimTypes.Email)?.Value?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        return await _context.Teachers.FirstOrDefaultAsync(t => !t.IsDeleted && t.Email.ToLower() == email);
    }

    private async Task<bool> CanManageClassAsync(int teacherId, int classId) =>
        await _context.TeacherClasses.AnyAsync(tc => tc.TeacherId == teacherId && tc.ClassId == classId && tc.IsActive &&
            tc.Class != null && !tc.Class.IsDeleted && tc.Class.IsActive);

    private int? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userId) ? userId : null;
    }
}

public sealed class TeacherAttendanceRequest
{
    public DateOnly Date { get; set; }
    public List<TeacherAttendanceRecordRequest> Records { get; set; } = new();
}

public sealed class TeacherAttendanceRecordRequest
{
    public int StudentId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Remarks { get; set; }
}
