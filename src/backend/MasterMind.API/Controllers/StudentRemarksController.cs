using System.Security.Claims;
using MasterMind.API.Data;
using MasterMind.API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MasterMind.API.Controllers;

[ApiController]
[Route("api/student-remarks")]
[Authorize]
public class StudentRemarksController : ControllerBase
{
    private readonly MasterMindDbContext _context;
    private readonly ILogger<StudentRemarksController> _logger;

    public StudentRemarksController(MasterMindDbContext context, ILogger<StudentRemarksController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<ActionResult<ApiResponse<IEnumerable<object>>>> GetRemarks([FromQuery] int? classId = null, [FromQuery] int? studentId = null)
    {
        try
        {
            var teacher = await ResolveTeacherForCurrentUserAsync();
            if (teacher == null)
            {
                return BadRequest(new ApiResponse<IEnumerable<object>>
                {
                    Success = false,
                    Message = "Teacher profile not found for current user",
                    Data = new List<object>()
                });
            }

            var allowedClassIds = await _context.TeacherClasses
                .Where(tc => tc.TeacherId == teacher.Id && tc.IsActive)
                .Select(tc => tc.ClassId)
                .ToListAsync();

            var query = _context.StudentRemarks
                .Include(r => r.Student)
                .Include(r => r.Class)
                .Where(r => !r.IsDeleted && r.TeacherId == teacher.Id);

            if (classId.HasValue)
            {
                query = query.Where(r => r.ClassId == classId.Value);
            }
            else if (allowedClassIds.Count > 0)
            {
                query = query.Where(r => r.ClassId.HasValue && allowedClassIds.Contains(r.ClassId.Value));
            }

            if (studentId.HasValue)
            {
                query = query.Where(r => r.StudentId == studentId.Value);
            }

            var remarks = await query
                .OrderByDescending(r => r.RemarkDate)
                .ThenByDescending(r => r.CreatedAt)
                .Take(200)
                .Select(r => new
                {
                    r.Id,
                    r.StudentId,
                    StudentName = $"{r.Student.FirstName} {r.Student.LastName}".Trim(),
                    ClassId = r.ClassId,
                    ClassName = r.Class != null ? r.Class.Name : "Not Assigned",
                    r.Subject,
                    r.ChapterName,
                    r.TopicName,
                    r.Remarks,
                    Type = r.Type.ToString(),
                    r.Rating,
                    Date = r.RemarkDate.ToString("yyyy-MM-dd"),
                    r.IsVisibleToParent
                })
                .ToListAsync();

            return Ok(new ApiResponse<IEnumerable<object>>
            {
                Success = true,
                Message = "Student remarks retrieved successfully",
                Data = remarks
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving student remarks");
            return StatusCode(500, new ApiResponse<IEnumerable<object>>
            {
                Success = false,
                Message = "Error retrieving student remarks",
                Data = new List<object>()
            });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<ActionResult<ApiResponse<object>>> CreateRemark([FromBody] CreateStudentRemarkRequest request)
    {
        try
        {
            if (request.StudentId <= 0 || string.IsNullOrWhiteSpace(request.Remarks))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Student and remarks are required"
                });
            }

            var teacher = await ResolveTeacherForCurrentUserAsync();
            if (teacher == null)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Teacher profile not found for current user"
                });
            }

            var student = await _context.Students
                .Include(s => s.StudentClasses)
                .FirstOrDefaultAsync(s => s.Id == request.StudentId && !s.IsDeleted);

            if (student == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Student not found"
                });
            }

            var teacherClassIds = await _context.TeacherClasses
                .Where(tc => tc.TeacherId == teacher.Id && tc.IsActive)
                .Select(tc => tc.ClassId)
                .ToListAsync();

            var classId = request.ClassId;
            if (!classId.HasValue)
            {
                classId = student.StudentClasses
                    .Where(sc => sc.IsActive && teacherClassIds.Contains(sc.ClassId))
                    .Select(sc => (int?)sc.ClassId)
                    .FirstOrDefault();
            }

            if (!classId.HasValue || !teacherClassIds.Contains(classId.Value))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Teacher is not mapped to the selected student's class"
                });
            }

            var studentRemark = new StudentRemark
            {
                StudentId = request.StudentId,
                TeacherId = teacher.Id,
                ClassId = classId,
                Subject = request.Subject?.Trim(),
                ChapterName = request.ChapterName?.Trim(),
                TopicName = request.TopicName?.Trim(),
                Remarks = request.Remarks.Trim(),
                Type = request.Type,
                Rating = request.Rating,
                IsVisibleToParent = request.IsVisibleToParent,
                RemarkDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.StudentRemarks.Add(studentRemark);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Remark added successfully",
                Data = new
                {
                    studentRemark.Id,
                    studentRemark.StudentId,
                    studentRemark.ClassId,
                    studentRemark.Remarks,
                    Type = studentRemark.Type.ToString()
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating student remark");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Error creating student remark"
            });
        }
    }

    private async Task<Teacher?> ResolveTeacherForCurrentUserAsync()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
        {
            return await _context.Teachers.FirstOrDefaultAsync(t => !t.IsDeleted && t.UserId == userId);
        }

        var email = User.FindFirst(ClaimTypes.Email)?.Value?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        return await _context.Teachers.FirstOrDefaultAsync(t => !t.IsDeleted && t.Email.ToLower() == email);
    }
}

public class CreateStudentRemarkRequest
{
    public int StudentId { get; set; }
    public int? ClassId { get; set; }
    public string? Subject { get; set; }
    public string? ChapterName { get; set; }
    public string? TopicName { get; set; }
    public string Remarks { get; set; } = string.Empty;
    public RemarkType Type { get; set; } = RemarkType.General;
    public int? Rating { get; set; }
    public bool IsVisibleToParent { get; set; } = true;
}
