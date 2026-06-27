using System.Security.Claims;
using MasterMind.API.Data;
using MasterMind.API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MasterMind.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Produces("application/json")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly MasterMindDbContext _context;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(MasterMindDbContext context, ILogger<NotificationsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> GetNotifications()
    {
        try
        {
            var userId = GetCurrentUserId();
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("role")?.Value ?? string.Empty;

            var items = role.Equals("Parent", StringComparison.OrdinalIgnoreCase)
                ? await GetParentNotificationsAsync(userId)
                : role.Equals("Teacher", StringComparison.OrdinalIgnoreCase)
                    ? await GetTeacherNotificationsAsync(userId)
                    : new List<PortalNotificationDto>();

            var orderedItems = items
                .OrderByDescending(item => item.Priority == "High")
                .ThenBy(item => item.DueDate ?? DateOnly.MaxValue)
                .ThenByDescending(item => item.CreatedAt)
                .Take(50)
                .ToList();

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Notifications retrieved successfully",
                Data = new
                {
                    TotalCount = orderedItems.Count,
                    Items = orderedItems
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving portal notifications");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Error retrieving notifications"
            });
        }
    }

    private async Task<List<PortalNotificationDto>> GetParentNotificationsAsync(int? userId)
    {
        if (!userId.HasValue)
        {
            return new List<PortalNotificationDto>();
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        var children = await _context.Students
            .Where(s => !s.IsDeleted && s.IsActive && s.ParentUserId == userId.Value)
            .Select(s => new { s.Id, StudentName = (s.FirstName + " " + s.LastName).Trim() })
            .ToListAsync();

        var childIds = children.Select(c => c.Id).ToHashSet();
        if (childIds.Count == 0)
        {
            return new List<PortalNotificationDto>();
        }

        var childNames = children.ToDictionary(c => c.Id, c => c.StudentName);
        var notifications = new List<PortalNotificationDto>();

        var fees = await _context.StudentFees
            .Where(sf => !sf.IsDeleted &&
                         childIds.Contains(sf.StudentId) &&
                         sf.Status != FeeStatus.Paid &&
                         sf.Status != FeeStatus.Waived &&
                         sf.Status != FeeStatus.Cancelled &&
                         sf.DueDate <= today.AddDays(7))
            .OrderBy(sf => sf.DueDate)
            .Take(20)
            .Select(sf => new
            {
                sf.Id,
                sf.StudentId,
                sf.DueDate,
                BalanceAmount = sf.FinalAmount - sf.PaidAmount
            })
            .ToListAsync();

        notifications.AddRange(fees.Select(fee =>
        {
            var isOverdue = fee.DueDate < today;
            var days = today.DayNumber - fee.DueDate.DayNumber;
            return new PortalNotificationDto
            {
                Id = $"fee-{fee.Id}",
                Type = "FeeReminder",
                Priority = isOverdue ? "High" : "Medium",
                Title = isOverdue ? "Fee payment is overdue" : "Fee payment due soon",
                Message = $"{GetStudentName(childNames, fee.StudentId)} has pending fee of INR {fee.BalanceAmount:0.##}.",
                ActionUrl = "/parent/fees",
                DueDate = fee.DueDate,
                CreatedAt = DateTime.UtcNow,
                Meta = new { fee.StudentId, fee.BalanceAmount, DaysOverdue = isOverdue ? days : 0 }
            };
        }));

        var remarks = await _context.StudentRemarks
            .Include(r => r.Teacher)
            .Where(r => !r.IsDeleted &&
                        r.IsVisibleToParent &&
                        childIds.Contains(r.StudentId) &&
                        r.RemarkDate >= DateTime.UtcNow.AddDays(-14))
            .OrderByDescending(r => r.RemarkDate)
            .Take(20)
            .Select(r => new
            {
                r.Id,
                r.StudentId,
                r.Type,
                r.Subject,
                r.RemarkDate,
                TeacherName = r.Teacher.FirstName + " " + r.Teacher.LastName
            })
            .ToListAsync();

        notifications.AddRange(remarks.Select(remark => new PortalNotificationDto
        {
            Id = $"remark-{remark.Id}",
            Type = "TeacherFeedback",
            Priority = remark.Type == RemarkType.Concern || remark.Type == RemarkType.Improvement ? "High" : "Medium",
            Title = "New teacher feedback",
            Message = $"{GetStudentName(childNames, remark.StudentId)} has a new {remark.Type} remark from {remark.TeacherName.Trim()}.",
            ActionUrl = "/parent/performance",
            CreatedAt = remark.RemarkDate,
            Meta = new { remark.StudentId, Type = remark.Type.ToString(), remark.Subject }
        }));

        return notifications;
    }

    private async Task<List<PortalNotificationDto>> GetTeacherNotificationsAsync(int? userId)
    {
        var teacher = await ResolveTeacherForCurrentUserAsync(userId);
        if (teacher == null)
        {
            return new List<PortalNotificationDto>();
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        var notifications = new List<PortalNotificationDto>();

        var salaryRows = await _context.TeacherSalaries
            .Where(s => !s.IsDeleted &&
                        s.TeacherId == teacher.Id &&
                        s.Status != SalaryStatus.Paid &&
                        s.Status != SalaryStatus.Cancelled)
            .OrderByDescending(s => s.Year)
            .ThenByDescending(s => s.CreatedAt)
            .Take(10)
            .Select(s => new
            {
                s.Id,
                s.Month,
                s.Year,
                s.NetSalary,
                s.Status,
                s.CreatedAt,
                s.PaymentDate
            })
            .ToListAsync();

        notifications.AddRange(salaryRows.Select(salary => new PortalNotificationDto
        {
            Id = $"salary-{salary.Id}",
            Type = "Salary",
            Priority = salary.Status == SalaryStatus.OnHold ? "High" : "Medium",
            Title = salary.Status == SalaryStatus.OnHold ? "Salary is on hold" : "Salary update pending",
            Message = $"{salary.Month} {salary.Year} salary status: {salary.Status}. Amount INR {salary.NetSalary:0.##}.",
            ActionUrl = "/teacher",
            CreatedAt = salary.PaymentDate ?? salary.CreatedAt,
            Meta = new { salary.NetSalary, Status = salary.Status.ToString() }
        }));

        var classIds = await _context.TeacherClasses
            .Where(tc => tc.TeacherId == teacher.Id && tc.IsActive)
            .Select(tc => tc.ClassId)
            .ToListAsync();

        var studentsWithoutTodayAttendance = await _context.StudentClasses
            .Include(sc => sc.Student)
            .Include(sc => sc.Class)
            .Where(sc => sc.IsActive &&
                         classIds.Contains(sc.ClassId) &&
                         sc.Student.IsActive &&
                         !sc.Student.IsDeleted)
            .Select(sc => new
            {
                sc.ClassId,
                ClassName = sc.Class.Name,
                sc.StudentId,
                StudentName = sc.Student.FirstName + " " + sc.Student.LastName
            })
            .Take(100)
            .ToListAsync();

        var todayAttendanceStudentIds = await _context.Attendances
            .Where(a => !a.IsDeleted &&
                        classIds.Contains(a.ClassId) &&
                        a.Date == today)
            .Select(a => a.StudentId)
            .ToListAsync();

        var pendingAttendance = studentsWithoutTodayAttendance
            .Where(s => !todayAttendanceStudentIds.Contains(s.StudentId))
            .GroupBy(s => new { s.ClassId, s.ClassName })
            .Select(g => new PortalNotificationDto
            {
                Id = $"attendance-{g.Key.ClassId}-{today:yyyyMMdd}",
                Type = "Attendance",
                Priority = "Medium",
                Title = "Attendance pending",
                Message = $"{g.Count()} student attendance entries pending for {g.Key.ClassName}.",
                ActionUrl = "/teacher/attendance",
                DueDate = today,
                CreatedAt = DateTime.UtcNow,
                Meta = new { g.Key.ClassId, PendingStudents = g.Count() }
            })
            .ToList();

        notifications.AddRange(pendingAttendance);

        return notifications;
    }

    private async Task<Teacher?> ResolveTeacherForCurrentUserAsync(int? userId)
    {
        if (userId.HasValue)
        {
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => !t.IsDeleted && t.UserId == userId.Value);
            if (teacher != null) return teacher;
        }

        var email = User.FindFirst(ClaimTypes.Email)?.Value?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        return await _context.Teachers.FirstOrDefaultAsync(t => !t.IsDeleted && t.Email.ToLower() == email);
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    private static string GetStudentName(Dictionary<int, string> names, int studentId)
    {
        return names.TryGetValue(studentId, out var name) && !string.IsNullOrWhiteSpace(name)
            ? name
            : "Student";
    }
}

public class PortalNotificationDto
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium";
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string ActionUrl { get; set; } = string.Empty;
    public DateOnly? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public object? Meta { get; set; }
}
