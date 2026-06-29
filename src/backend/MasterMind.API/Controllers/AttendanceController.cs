using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MasterMind.API.Data;
using MasterMind.API.Models.Entities;
using MasterMind.API.Models.DTOs.Common;
using System.Security.Claims;

namespace MasterMind.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AttendanceController : ControllerBase
{
    private readonly MasterMindDbContext _context;

    public AttendanceController(MasterMindDbContext context)
    {
        _context = context;
    }

    // GET: api/Attendance
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<ApiResponse<IEnumerable<AttendanceDto>>>> GetAttendance(
        [FromQuery] DateOnly? date,
        [FromQuery] int? classId,
        [FromQuery] int? studentId,
        [FromQuery] int? sessionId)
    {
        if (date.HasValue)
        {
            var rosterDtos = await BuildAttendanceRosterAsync(date.Value, classId, studentId, sessionId);
            return Ok(new ApiResponse<IEnumerable<AttendanceDto>>
            {
                Success = true,
                Message = "Attendance records retrieved successfully",
                Data = rosterDtos
            });
        }

        var query = _context.Attendances
            .Include(a => a.Student)
            .Include(a => a.Class)
            .Where(a => !a.IsDeleted);

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

    // GET: api/Attendance/report
    [HttpGet("report")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<ApiResponse<AttendanceReportDto>>> GetAttendanceReport(
        [FromQuery] DateOnly startDate,
        [FromQuery] DateOnly endDate,
        [FromQuery] int? classId,
        [FromQuery] int? sessionId)
    {
        if (endDate < startDate)
        {
            return BadRequest(new ApiResponse<AttendanceReportDto>
            {
                Success = false,
                Message = "End date must be after start date",
                Data = null
            });
        }

        var days = endDate.DayNumber - startDate.DayNumber + 1;
        if (days > 62)
        {
            return BadRequest(new ApiResponse<AttendanceReportDto>
            {
                Success = false,
                Message = "Attendance reports are limited to 62 days at a time",
                Data = null
            });
        }

        var daily = new List<AttendanceDailySummaryDto>();
        var allRows = new List<AttendanceDto>();

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            var rows = (await BuildAttendanceRosterAsync(date, classId, null, sessionId)).ToList();
            allRows.AddRange(rows);

            var present = rows.Count(r => r.Status is "present" or "late" or "halfday");
            var absent = rows.Count(r => r.Status == "absent");
            var notMarked = rows.Count(r => r.Status == "notmarked");
            var total = rows.Count;

            daily.Add(new AttendanceDailySummaryDto
            {
                Date = date,
                Present = present,
                Absent = absent,
                NotMarked = notMarked,
                Total = total,
                Percentage = total == 0 ? 0 : Math.Round((decimal)present * 100m / total, 2)
            });
        }

        var report = new AttendanceReportDto
        {
            StartDate = startDate,
            EndDate = endDate,
            Present = allRows.Count(r => r.Status is "present" or "late" or "halfday"),
            Absent = allRows.Count(r => r.Status == "absent"),
            NotMarked = allRows.Count(r => r.Status == "notmarked"),
            Total = allRows.Count,
            Daily = daily,
            Records = allRows
        };

        report.Percentage = report.Total == 0 ? 0 : Math.Round((decimal)report.Present * 100m / report.Total, 2);

        return Ok(new ApiResponse<AttendanceReportDto>
        {
            Success = true,
            Message = "Attendance report retrieved successfully",
            Data = report
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
            existing.Status = dto.Status;
            existing.CheckInTime = dto.CheckInTime;
            existing.CheckOutTime = dto.CheckOutTime;
            existing.Remarks = dto.Remarks;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.MarkedByUserId = GetCurrentUserId();

            await _context.SaveChangesAsync();

            await _context.Entry(existing).Reference(a => a.Student).LoadAsync();
            await _context.Entry(existing).Reference(a => a.Class).LoadAsync();

            return Ok(new ApiResponse<AttendanceDto>
            {
                Success = true,
                Message = "Attendance updated successfully",
                Data = MapToDto(existing)
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
            Remarks = entity.Remarks,
            ParentName = entity.Student.ParentName,
            ParentMobile = entity.Student.ParentMobile,
            StudentMobile = entity.Student.StudentMobile,
            IsMarked = true
        };
    }

    private async Task<IEnumerable<AttendanceDto>> BuildAttendanceRosterAsync(DateOnly date, int? classId, int? studentId, int? sessionId)
    {
        var rosterQuery = _context.StudentClasses
            .Include(sc => sc.Student)
            .Include(sc => sc.Class)
            .Where(sc => sc.IsActive &&
                         !sc.Student.IsDeleted &&
                         sc.Student.IsActive &&
                         !sc.Class.IsDeleted &&
                         sc.Class.IsActive);

        if (classId.HasValue)
        {
            rosterQuery = rosterQuery.Where(sc => sc.ClassId == classId.Value);
        }

        if (studentId.HasValue)
        {
            rosterQuery = rosterQuery.Where(sc => sc.StudentId == studentId.Value);
        }

        if (sessionId.HasValue)
        {
            rosterQuery = rosterQuery.Where(sc => sc.Student.SessionId == sessionId.Value || sc.Class.SessionId == sessionId.Value);
        }

        var roster = await rosterQuery
            .OrderBy(sc => sc.Class.Name)
            .ThenBy(sc => sc.Student.FirstName)
            .ThenBy(sc => sc.Student.LastName)
            .ToListAsync();

        var attendanceQuery = _context.Attendances
            .Include(a => a.Student)
            .Include(a => a.Class)
            .Where(a => !a.IsDeleted && a.Date == date);

        if (classId.HasValue)
        {
            attendanceQuery = attendanceQuery.Where(a => a.ClassId == classId.Value);
        }

        if (studentId.HasValue)
        {
            attendanceQuery = attendanceQuery.Where(a => a.StudentId == studentId.Value);
        }

        if (sessionId.HasValue)
        {
            attendanceQuery = attendanceQuery.Where(a => a.Student.SessionId == sessionId.Value || a.Class.SessionId == sessionId.Value);
        }

        var attendances = await attendanceQuery.ToListAsync();
        var attendanceByPair = attendances
            .GroupBy(a => new { a.StudentId, a.ClassId })
            .ToDictionary(g => g.Key, g => g.OrderByDescending(a => a.UpdatedAt ?? a.CreatedAt).First());

        return roster
            .GroupBy(sc => new { sc.StudentId, sc.ClassId })
            .Select(g => g.First())
            .Select(sc =>
            {
                if (attendanceByPair.TryGetValue(new { sc.StudentId, sc.ClassId }, out var attendance))
                {
                    return MapToDto(attendance);
                }

                return new AttendanceDto
                {
                    Id = 0,
                    StudentId = sc.StudentId,
                    StudentName = $"{sc.Student.FirstName} {sc.Student.LastName}",
                    ClassId = sc.ClassId,
                    ClassName = sc.Class.Name,
                    Date = date,
                    Status = "notmarked",
                    CheckInTime = null,
                    CheckOutTime = null,
                    Remarks = null,
                    ParentName = sc.Student.ParentName,
                    ParentMobile = sc.Student.ParentMobile,
                    StudentMobile = sc.Student.StudentMobile,
                    IsMarked = false
                };
            })
            .ToList();
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
        public string? ParentName { get; set; }
        public string? ParentMobile { get; set; }
        public string? StudentMobile { get; set; }
        public bool IsMarked { get; set; }
    }

    public class AttendanceReportDto
    {
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int Present { get; set; }
        public int Absent { get; set; }
        public int NotMarked { get; set; }
        public int Total { get; set; }
        public decimal Percentage { get; set; }
        public List<AttendanceDailySummaryDto> Daily { get; set; } = new();
        public List<AttendanceDto> Records { get; set; } = new();
    }

    public class AttendanceDailySummaryDto
    {
        public DateOnly Date { get; set; }
        public int Present { get; set; }
        public int Absent { get; set; }
        public int NotMarked { get; set; }
        public int Total { get; set; }
        public decimal Percentage { get; set; }
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
