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
}
