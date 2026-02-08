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
                .ToListAsync();

            // Calculate stats (mock implementation for now)
            var stats = new
            {
                TotalChildren = children.Count,
                ActiveChildren = children.Count(s => s.IsActive),
                // Mock data - implement actual calculations
                AverageAttendance = 92,
                TotalPendingFees = 5000,
                TotalRemarks = 3
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

            // Mock attendance data - implement actual attendance logic
            var attendanceData = new
            {
                TotalClasses = 20,
                Present = 18,
                Absent = 1,
                Late = 1,
                Percentage = 90,
                Records = new[]
                {
                    new { Date = DateTime.Today.AddDays(-1), Subject = "Mathematics", Status = "Present" },
                    new { Date = DateTime.Today.AddDays(-2), Subject = "Science", Status = "Present" },
                    new { Date = DateTime.Today.AddDays(-3), Subject = "English", Status = "Late" }
                }
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

            // Mock fee data - implement actual fee logic
            var feeData = new
            {
                TotalFees = 12000,
                PaidFees = 9500,
                PendingFees = 2500,
                NextDueDate = DateTime.Today.AddDays(15),
                PaymentHistory = new[]
                {
                    new { Date = DateTime.Today.AddDays(-30), Amount = 3000, Status = "Paid", Method = "Online" },
                    new { Date = DateTime.Today.AddDays(-60), Amount = 3000, Status = "Paid", Method = "Cash" }
                }
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

            // Mock performance data - implement actual performance logic
            var performanceData = new
            {
                AverageGrade = "A-",
                OverallPercentage = 85,
                SubjectPerformance = new[]
                {
                    new { Subject = "Mathematics", Grade = "A", Percentage = 92 },
                    new { Subject = "Science", Grade = "B+", Percentage = 78 },
                    new { Subject = "English", Grade = "A-", Percentage = 85 }
                },
                RecentTests = new[]
                {
                    new { Date = DateTime.Today.AddDays(-7), Subject = "Mathematics", Topic = "Algebra", Score = 85, TotalMarks = 100 },
                    new { Date = DateTime.Today.AddDays(-14), Subject = "Science", Topic = "Physics", Score = 78, TotalMarks = 100 }
                }
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
}
