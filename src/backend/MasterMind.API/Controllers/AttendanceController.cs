using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MasterMind.API.Data;
using MasterMind.API.Models.Entities;
using System.Security.Claims;

namespace MasterMind.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AttendanceController : ControllerBase
{
    private readonly MasterMindDbContext _context;

    public AttendanceController(MasterMindDbContext context)
    {
        _context = context;
    }

    // GET: api/Attendance
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<AttendanceDto>>>> GetAttendance(
        [FromQuery] DateOnly? date,
        [FromQuery] int? classId,
        [FromQuery] int? studentId)
    {
        var query = _context.Attendances
            .Include(a => a.Student)
            .Include(a => a.Class)
            .Where(a => !a.IsDeleted);

        if (date.HasValue)
        {
            query = query.Where(a => a.Date == date.Value);
        }

        if (classId.HasValue)
        {
            query = query.Where(a => a.ClassId == classId.Value);
        }

        if (studentId.HasValue)
        {
            query = query.Where(a => a.StudentId == studentId.Value);
        }

        var attendances = await query
            .OrderByDescending(a => a.Date)
            .ThenBy(a => a.Student.FirstName)
            .ToListAsync();

        var dtos = attendances.Select(MapToDto).ToList();

        return Ok(new ApiResponse<IEnumerable<AttendanceDto>>
        {
            Success = true,
            Message = "Attendance records retrieved successfully",
            Data = dtos
        });
    }

    // POST: api/Attendance
    [HttpPost]
    public async Task<ActionResult<ApiResponse<AttendanceDto>>> MarkAttendance(CreateAttendanceDto dto)
    {
        // Validation
        var student = await _context.Students.FindAsync(dto.StudentId);
        if (student == null)
        {
             return NotFound(new ApiResponse<AttendanceDto>
            {
                Success = false,
                Message = "Student not found",
                Data = null
            });
        }

        var classEntity = await _context.Classes.FindAsync(dto.ClassId);
        if (classEntity == null)
        {
             return NotFound(new ApiResponse<AttendanceDto>
            {
                Success = false,
                Message = "Class not found",
                Data = null
            });
        }

        // Check if already exists for this date
        var existing = await _context.Attendances
            .FirstOrDefaultAsync(a => a.StudentId == dto.StudentId && 
                                      a.ClassId == dto.ClassId && 
                                      a.Date == dto.Date &&
                                      !a.IsDeleted);

        if (existing != null)
        {
            return BadRequest(new ApiResponse<AttendanceDto>
            {
                Success = false,
                Message = "Attendance already marked for this student on this date",
                Data = null
            });
        }

        var attendance = new Attendance
        {
            StudentId = dto.StudentId,
            ClassId = dto.ClassId,
            Date = dto.Date,
            Status = dto.Status,
            CheckInTime = dto.CheckInTime,
            CheckOutTime = dto.CheckOutTime,
            Remarks = dto.Remarks,
            MarkedAt = DateTime.UtcNow,
            MarkedByUserId = GetCurrentUserId()
        };

        _context.Attendances.Add(attendance);
        await _context.SaveChangesAsync();

        // Reload to include navigation properties
        await _context.Entry(attendance)
            .Reference(a => a.Student)
            .LoadAsync();
        await _context.Entry(attendance)
            .Reference(a => a.Class)
            .LoadAsync();
            
        return CreatedAtAction(nameof(GetAttendance), new { id = attendance.Id }, new ApiResponse<AttendanceDto>
        {
            Success = true,
            Message = "Attendance marked successfully",
            Data = MapToDto(attendance)
        });
    }

    // PUT: api/Attendance/5
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<AttendanceDto>>> UpdateAttendance(int id, UpdateAttendanceDto dto)
    {
        var attendance = await _context.Attendances
            .Include(a => a.Student)
            .Include(a => a.Class)
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

        if (attendance == null)
        {
            return NotFound(new ApiResponse<AttendanceDto>
            {
                Success = false,
                Message = "Attendance record not found",
                Data = null
            });
        }

        attendance.Status = dto.Status;
        attendance.CheckInTime = dto.CheckInTime;
        attendance.CheckOutTime = dto.CheckOutTime;
        attendance.Remarks = dto.Remarks;
        attendance.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<AttendanceDto>
        {
            Success = true,
            Message = "Attendance updated successfully",
            Data = MapToDto(attendance)
        });
    }

    // DELETE: api/Attendance/5
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteAttendance(int id)
    {
        var attendance = await _context.Attendances.FindAsync(id);
        if (attendance == null)
        {
             return NotFound(new ApiResponse<bool>
            {
                Success = false,
                Message = "Attendance record not found",
                Data = false
            });
        }

        attendance.IsDeleted = true;
        attendance.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<bool>
        {
            Success = true,
            Message = "Attendance deleted successfully",
            Data = true
        });
    }

    private AttendanceDto MapToDto(Attendance entity)
    {
        return new AttendanceDto
        {
            Id = entity.Id,
            StudentId = entity.StudentId,
            StudentName = $"{entity.Student.FirstName} {entity.Student.LastName}",
            ClassId = entity.ClassId,
            ClassName = entity.Class?.Name ?? "Unknown Class",
            Date = entity.Date,
            Status = entity.Status.ToString().ToLower(),
            CheckInTime = entity.CheckInTime,
            CheckOutTime = entity.CheckOutTime,
            Remarks = entity.Remarks
        };
    }

    public class AttendanceDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public DateOnly Date { get; set; }
        public string Status { get; set; } = string.Empty;
        public TimeOnly? CheckInTime { get; set; }
        public TimeOnly? CheckOutTime { get; set; }
        public string? Remarks { get; set; }
    }

    public class CreateAttendanceDto
{
    public int StudentId { get; set; }
    public int ClassId { get; set; }
    public DateOnly Date { get; set; }
    public AttendanceStatus Status { get; set; }
    public TimeOnly? CheckInTime { get; set; }
    public TimeOnly? CheckOutTime { get; set; }
    public string? Remarks { get; set; }
}

public class UpdateAttendanceDto
    {
        public AttendanceStatus Status { get; set; }
        public TimeOnly? CheckInTime { get; set; }
        public TimeOnly? CheckOutTime { get; set; }
        public string? Remarks { get; set; }
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
        {
            return userId;
        }
        return null;
}

    private string GetCurrentUserName()
    {
        return User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
    }
} 