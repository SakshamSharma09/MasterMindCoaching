using MasterMind.API.Data;
using MasterMind.API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MasterMind.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ParentController : ControllerBase
{
    private readonly MasterMindDbContext _context;
    private readonly ILogger<ParentController> _logger;

    public ParentController(MasterMindDbContext context, ILogger<ParentController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get children of the currently logged-in parent
    /// </summary>
    /// <returns>List of parent's children</returns>
    [HttpGet("children")]
    public async Task<ActionResult<ApiResponse<IEnumerable<object>>>> GetMyChildren()
    {
        try
        {
            var parentUserId = GetCurrentUserId();
            if (parentUserId == null)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            var children = await _context.Students
                .Include(s => s.StudentClasses)
                    .ThenInclude(sc => sc.Class)
                .Where(s => !s.IsDeleted && s.ParentUserId == parentUserId)
                .Select(s => new
                {
                    s.Id,
                    s.FirstName,
                    s.LastName,
                    s.AdmissionNumber,
                    s.StudentMobile,
                    s.StudentEmail,
                    ClassName = s.StudentClasses
                        .Where(sc => sc.IsActive)
                        .Select(sc => sc.Class.Name)
                        .FirstOrDefault() ?? "Not Assigned",
                    ClassId = s.StudentClasses
                        .Where(sc => sc.IsActive)
                        .Select(sc => sc.Class.Id)
                        .FirstOrDefault(),
                    s.IsActive
                })
                .ToListAsync();

            return Ok(new ApiResponse<IEnumerable<object>>
            {
                Success = true,
                Message = "Children retrieved successfully",
                Data = children
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving parent's children");
            return StatusCode(500, new ApiResponse<IEnumerable<object>>
            {
                Success = false,
                Message = "Error retrieving children",
                Data = new List<object>()
            });
        }
    }

    /// <summary>
    /// Get dashboard statistics for parent
    /// </summary>
    /// <returns>Parent dashboard stats</returns>
    [HttpGet("dashboard/stats")]
    public async Task<ActionResult<ApiResponse<object>>> GetParentDashboardStats()
    {
        try
        {
            var parentUserId = GetCurrentUserId();
            if (parentUserId == null)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            var children = await _context.Students
                .Where(s => !s.IsDeleted && s.ParentUserId == parentUserId)
                .Select(s => s.Id)
                .ToListAsync();

            if (!children.Any())
            {
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Parent dashboard stats retrieved successfully",
                    Data = new
                    {
                        TotalChildren = 0,
                        ActiveChildren = 0,
                        AverageAttendance = 0m,
                        TotalPendingFees = 0m,
                        TotalRemarks = 0
                    }
                });
            }

            var activeChildrenCount = await _context.Students
                .Where(s => !s.IsDeleted && s.ParentUserId == parentUserId && s.IsActive)
                .CountAsync();

            var attendanceRecords = await _context.Attendances
                .Where(a => !a.IsDeleted && children.Contains(a.StudentId))
                .Select(a => a.Status)
                .ToListAsync();

            var presentAttendanceCount = attendanceRecords.Count(status =>
                status == AttendanceStatus.Present ||
                status == AttendanceStatus.Late ||
                status == AttendanceStatus.HalfDay);

            var averageAttendance = attendanceRecords.Count == 0
                ? 0m
                : Math.Round((decimal)presentAttendanceCount * 100m / attendanceRecords.Count, 2);

            var childFees = await _context.StudentFees
                .Where(sf => !sf.IsDeleted && children.Contains(sf.StudentId))
                .Select(sf => new { sf.FinalAmount, sf.PaidAmount })
                .ToListAsync();

            var totalPendingFees = childFees
                .Sum(sf => Math.Max(0m, sf.FinalAmount - sf.PaidAmount));

            var totalRemarks = await _context.StudentRemarks
                .Where(r => !r.IsDeleted && r.IsVisibleToParent && children.Contains(r.StudentId))
                .CountAsync();

            var stats = new
            {
                TotalChildren = children.Count,
                ActiveChildren = activeChildrenCount,
                AverageAttendance = averageAttendance,
                TotalPendingFees = totalPendingFees,
                TotalRemarks = totalRemarks
            };

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Parent dashboard stats retrieved successfully",
                Data = stats
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving parent dashboard stats");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Error retrieving dashboard stats"
            });
        }
    }

    /// <summary>
    /// Get attendance records for a specific child
    /// </summary>
    /// <param name="childId">Child ID</param>
    /// <returns>Attendance records</returns>
    [HttpGet("children/{childId}/attendance")]
    public async Task<ActionResult<ApiResponse<object>>> GetChildAttendance(int childId)
    {
        try
        {
            var parentUserId = GetCurrentUserId();
            if (parentUserId == null)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            // Verify parent owns this child
            var child = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == childId && s.ParentUserId == parentUserId && !s.IsDeleted);

            if (child == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Child not found or access denied"
                });
            }

            var attendanceEntities = await _context.Attendances
                .Include(a => a.Class)
                .Where(a => !a.IsDeleted && a.StudentId == childId)
                .OrderByDescending(a => a.Date)
                .ThenByDescending(a => a.CreatedAt)
                .Take(100)
                .ToListAsync();

            var attendanceRecords = attendanceEntities.Select(a => new
            {
                a.Date,
                Subject = a.Class?.Name ?? "Class",
                Status = a.Status.ToString()
            }).ToList();

            var presentCount = attendanceRecords.Count(r =>
                string.Equals(r.Status, AttendanceStatus.Present.ToString(), StringComparison.OrdinalIgnoreCase) ||
                string.Equals(r.Status, AttendanceStatus.Late.ToString(), StringComparison.OrdinalIgnoreCase) ||
                string.Equals(r.Status, AttendanceStatus.HalfDay.ToString(), StringComparison.OrdinalIgnoreCase));

            var totalClasses = attendanceRecords.Count;
            var absentCount = attendanceRecords.Count(r =>
                string.Equals(r.Status, AttendanceStatus.Absent.ToString(), StringComparison.OrdinalIgnoreCase));
            var lateCount = attendanceRecords.Count(r =>
                string.Equals(r.Status, AttendanceStatus.Late.ToString(), StringComparison.OrdinalIgnoreCase));

            var attendanceData = new
            {
                TotalClasses = totalClasses,
                Present = presentCount,
                Absent = absentCount,
                Late = lateCount,
                Percentage = totalClasses == 0 ? 0m : Math.Round((decimal)presentCount * 100m / totalClasses, 2),
                Records = attendanceRecords.Select(r => new
                {
                    Date = r.Date.ToString("yyyy-MM-dd"),
                    r.Subject,
                    r.Status
                }).ToList()
            };

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Attendance records retrieved successfully",
                Data = attendanceData
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving child attendance");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Error retrieving attendance records"
            });
        }
    }

    /// <summary>
    /// Get fee information for a specific child
    /// </summary>
    /// <param name="childId">Child ID</param>
    /// <returns>Fee information</returns>
    [HttpGet("children/{childId}/fees")]
    public async Task<ActionResult<ApiResponse<object>>> GetChildFees(int childId)
    {
        try
        {
            var parentUserId = GetCurrentUserId();
            if (parentUserId == null)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            // Verify parent owns this child
            var child = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == childId && s.ParentUserId == parentUserId && !s.IsDeleted);

            if (child == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Child not found or access denied"
                });
            }

            var studentFees = await _context.StudentFees
                .Where(sf => !sf.IsDeleted && sf.StudentId == childId)
                .OrderBy(sf => sf.DueDate)
                .ToListAsync();

            var feeIds = studentFees.Select(sf => sf.Id).ToList();

            var paymentHistory = await _context.Payments
                .Where(p => !p.IsDeleted && feeIds.Contains(p.StudentFeeId))
                .OrderByDescending(p => p.PaymentDate)
                .Select(p => new
                {
                    Date = p.PaymentDate,
                    p.Amount,
                    Status = p.Status.ToString(),
                    Method = p.Method.ToString()
                })
                .ToListAsync();

            var totalFees = studentFees.Sum(sf => sf.FinalAmount);
            var paidFees = studentFees.Sum(sf => sf.PaidAmount);
            var pendingFees = studentFees.Sum(sf => Math.Max(0m, sf.FinalAmount - sf.PaidAmount));
            var nextDueDate = studentFees
                .Where(sf => sf.Status != FeeStatus.Paid && sf.Status != FeeStatus.Waived && sf.Status != FeeStatus.Cancelled)
                .OrderBy(sf => sf.DueDate)
                .Select(sf => (DateOnly?)sf.DueDate)
                .FirstOrDefault();

            var feeData = new
            {
                TotalFees = totalFees,
                PaidFees = paidFees,
                PendingFees = pendingFees,
                NextDueDate = nextDueDate.HasValue ? nextDueDate.Value.ToString("yyyy-MM-dd") : string.Empty,
                PaymentHistory = paymentHistory.Select(p => new
                {
                    Date = p.Date.ToString("yyyy-MM-dd"),
                    p.Amount,
                    p.Status,
                    p.Method
                }).ToList()
            };

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Fee information retrieved successfully",
                Data = feeData
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving child fee information");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Error retrieving fee information"
            });
        }
    }

    /// <summary>
    /// Get performance information for a specific child
    /// </summary>
    /// <param name="childId">Child ID</param>
    /// <returns>Performance information</returns>
    [HttpGet("children/{childId}/performance")]
    public async Task<ActionResult<ApiResponse<object>>> GetChildPerformance(int childId)
    {
        try
        {
            var parentUserId = GetCurrentUserId();
            if (parentUserId == null)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            // Verify parent owns this child
            var child = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == childId && s.ParentUserId == parentUserId && !s.IsDeleted);

            if (child == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Child not found or access denied"
                });
            }

            var performanceRecords = await _context.StudentPerformances
                .Where(sp => !sp.IsDeleted && sp.StudentId == childId)
                .OrderByDescending(sp => sp.TestDate)
                .ThenByDescending(sp => sp.CreatedAt)
                .ToListAsync();

            var remarkRecords = await _context.StudentRemarks
                .Include(r => r.Teacher)
                .Where(r => !r.IsDeleted && r.StudentId == childId && r.IsVisibleToParent)
                .OrderByDescending(r => r.RemarkDate)
                .ThenByDescending(r => r.CreatedAt)
                .Take(20)
                .ToListAsync();

            var overallPercentage = performanceRecords.Count == 0
                ? 0m
                : Math.Round(performanceRecords.Average(sp => sp.Percentage ?? sp.CalculatedPercentage), 2);

            var averageGrade = MapPercentageToGrade(overallPercentage);

            var subjectPerformance = performanceRecords
                .GroupBy(sp => string.IsNullOrWhiteSpace(sp.Subject) ? "General" : sp.Subject!)
                .Select(group =>
                {
                    var subjectPercentage = group.Average(sp => sp.Percentage ?? sp.CalculatedPercentage);
                    return new
                    {
                        Subject = group.Key,
                        Grade = MapPercentageToGrade(subjectPercentage),
                        Percentage = Math.Round(subjectPercentage, 2)
                    };
                })
                .OrderByDescending(sp => sp.Percentage)
                .ToList();

            var performanceData = new
            {
                AverageGrade = averageGrade,
                OverallPercentage = overallPercentage,
                SubjectPerformance = subjectPerformance,
                RecentTests = performanceRecords
                    .Take(20)
                    .Select(sp => new
                    {
                        Date = sp.TestDate.ToString("yyyy-MM-dd"),
                        Subject = string.IsNullOrWhiteSpace(sp.Subject) ? "General" : sp.Subject,
                        Topic = sp.TestName,
                        Score = sp.Score,
                        TotalMarks = sp.MaxScore
                    })
                    .ToList(),
                RecentRemarks = remarkRecords.Select(r => new
                {
                    Id = r.Id,
                    Date = r.RemarkDate.ToString("yyyy-MM-dd"),
                    Type = r.Type.ToString(),
                    Content = r.Remarks,
                    Subject = r.Subject,
                    ChapterName = r.ChapterName,
                    TeacherName = r.Teacher != null ? $"{r.Teacher.FirstName} {r.Teacher.LastName}".Trim() : "Teacher"
                }).ToList()
            };

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Performance information retrieved successfully",
                Data = performanceData
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving child performance information");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Error retrieving performance information"
            });
        }
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    private static string MapPercentageToGrade(decimal percentage)
    {
        if (percentage >= 90) return "A+";
        if (percentage >= 80) return "A";
        if (percentage >= 70) return "B+";
        if (percentage >= 60) return "B";
        if (percentage >= 50) return "C";
        if (percentage > 0) return "D";
        return "N/A";
    }
}
