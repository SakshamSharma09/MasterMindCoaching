using MasterMind.API.Data;
using MasterMind.API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MasterMind.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly MasterMindDbContext _context;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(MasterMindDbContext context, ILogger<DashboardController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get dashboard statistics
    /// </summary>
    /// <returns>Dashboard statistics</returns>
    [HttpGet("stats")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<DashboardStats>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DashboardStats>>> GetStats()
    {
        var totalStudents = await _context.Students.CountAsync(s => !s.IsDeleted);
        var activeStudents = await _context.Students.CountAsync(s => !s.IsDeleted && s.IsActive);
        var totalClasses = await _context.Classes.CountAsync(c => !c.IsDeleted);
        var totalTeachers = await _context.Teachers.CountAsync(t => !t.IsDeleted);

        // Calculate today's attendance (mock for now - you can implement actual attendance logic)
        var todayAttendance = 85; // Placeholder

        // Calculate pending fees (mock for now - you can implement actual fee calculation)
        var pendingFees = 25000; // Placeholder

        var stats = new DashboardStats
        {
            TotalStudents = totalStudents,
            ActiveStudents = activeStudents,
            TotalClasses = totalClasses,
            TotalTeachers = totalTeachers,
            TodayAttendance = todayAttendance,
            PendingFees = pendingFees
        };

        return Ok(new ApiResponse<DashboardStats>
        {
            Success = true,
            Message = "Dashboard stats retrieved successfully",
            Data = stats
        });
    }

    /// <summary>
    /// Get admin dashboard statistics (alias for stats endpoint)
    /// </summary>
    /// <returns>Dashboard statistics</returns>
    [HttpGet("admin-stats")]
    //[Authorize]
    [ProducesResponseType(typeof(ApiResponse<DashboardStats>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DashboardStats>>> GetAdminStats()
    {
        // Reuse the same logic as stats endpoint
        return await GetStats();
    }

    /// <summary>
    /// Get parent dashboard statistics
    /// </summary>
    /// <returns>Parent dashboard statistics</returns>
    [HttpGet("parent-stats")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> GetParentStats()
    {
        try
        {
            // Get current user ID from claims
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var parentUserId))
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            // Get parent's children
            var children = await _context.Students
                .Include(s => s.StudentClasses)
                    .ThenInclude(sc => sc.Class)
                .Where(s => !s.IsDeleted && s.ParentUserId == parentUserId)
                .ToListAsync();

            if (!children.Any())
            {
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "No children found for this parent",
                    Data = new { totalChildren = 0 }
                });
            }

            // Mock stats calculation (implement actual logic)
            var stats = new
            {
                totalChildren = children.Count,
                activeChildren = children.Count(s => s.IsActive),
                averageAttendance = 92, // Mock - calculate from actual attendance
                totalPendingFees = children.Count * 2500, // Mock - calculate from actual fees
                totalRemarks = 5 // Mock - calculate from actual remarks
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
                Message = "Error retrieving parent dashboard stats"
            });
        }
    }

    /// <summary>
    /// Get recent students for dashboard
    /// </summary>
    /// <returns>List of recent students</returns>
    [HttpGet("recent-students")]
    //[Authorize]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<Student>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<Student>>>> GetRecentStudents()
    {
        var recentStudents = await _context.Students
            .Include(s => s.StudentClasses)
                .ThenInclude(sc => sc.Class)
            .Where(s => !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt)
            .Take(5)
            .ToListAsync();

        return Ok(new ApiResponse<IEnumerable<Student>>
        {
            Success = true,
            Message = "Recent students retrieved successfully",
            Data = recentStudents
        });
    }
}

// DTOs
public class DashboardStats
{
    public int TotalStudents { get; set; }
    public int ActiveStudents { get; set; }
    public int TotalClasses { get; set; }
    public int TotalTeachers { get; set; }
    public int TodayAttendance { get; set; }
    public int PendingFees { get; set; }
}

