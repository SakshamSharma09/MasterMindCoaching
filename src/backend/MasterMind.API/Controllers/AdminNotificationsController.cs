using MasterMind.API.Data;
using MasterMind.API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MasterMind.API.Controllers;

[ApiController]
[Route("api/admin-notifications")]
[Produces("application/json")]
[Authorize(Roles = "Admin")]
public class AdminNotificationsController : ControllerBase
{
    private readonly MasterMindDbContext _context;
    private readonly ILogger<AdminNotificationsController> _logger;

    public AdminNotificationsController(MasterMindDbContext context, ILogger<AdminNotificationsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> GetNotifications([FromQuery] int? sessionId = null)
    {
        try
        {
            if (!sessionId.HasValue)
            {
                var activeSession = await _context.Sessions.FirstOrDefaultAsync(s => s.IsActive && !s.IsDeleted);
                sessionId = activeSession?.Id;
            }

            var today = DateOnly.FromDateTime(DateTime.Today);
            var birthdayEnd = today.AddDays(7);
            var feeEnd = today.AddDays(7);
            var followupEnd = DateTime.Today.AddDays(3).Date.AddDays(1).AddTicks(-1);

            var studentQuery = _context.Students.Where(s => !s.IsDeleted && s.IsActive);
            if (sessionId.HasValue)
            {
                studentQuery = studentQuery.Where(s => s.SessionId == sessionId.Value);
            }

            var studentRows = await studentQuery
                .Select(s => new
                {
                    s.Id,
                    StudentName = (s.FirstName + " " + s.LastName).Trim(),
                    s.DateOfBirth,
                    s.ParentName,
                    s.ParentMobile
                })
                .ToListAsync();

            var birthdays = studentRows
                .Select(s =>
                {
                    var dob = DateOnly.FromDateTime(s.DateOfBirth);
                    var nextBirthday = GetNextBirthday(today, dob);
                    return new
                    {
                        Type = "Birthday",
                        Priority = nextBirthday == today ? "High" : "Medium",
                        Title = nextBirthday == today ? $"{s.StudentName}'s birthday is today" : $"{s.StudentName}'s birthday is coming",
                        Message = $"Parent: {s.ParentName}. Prepare birthday wish template.",
                        StudentId = s.Id,
                        StudentName = s.StudentName,
                        ParentMobile = s.ParentMobile,
                        DueDate = nextBirthday.ToString("yyyy-MM-dd"),
                        DaysLeft = nextBirthday.DayNumber - today.DayNumber,
                        ActionUrl = "/admin/template-zone"
                    };
                })
                .Where(x => x.DaysLeft >= 0 && x.DaysLeft <= birthdayEnd.DayNumber - today.DayNumber)
                .OrderBy(x => x.DaysLeft)
                .Take(20)
                .ToList();

            var feeQuery = _context.StudentFees
                .Where(sf => !sf.IsDeleted &&
                    sf.Status != FeeStatus.Paid &&
                    sf.Status != FeeStatus.Waived &&
                    sf.Status != FeeStatus.Cancelled &&
                    sf.DueDate <= feeEnd);

            if (sessionId.HasValue)
            {
                feeQuery = feeQuery.Where(sf => sf.Student.SessionId == sessionId.Value);
            }

            var fees = await feeQuery
                .OrderBy(sf => sf.DueDate)
                .Take(30)
                .Select(sf => new
                {
                    Type = "FeeReminder",
                    Priority = sf.DueDate < today ? "High" : "Medium",
                    Title = sf.DueDate < today ? "Overdue fee reminder" : "Upcoming fee reminder",
                    Message = sf.Student.FirstName + " " + sf.Student.LastName + " has pending fee of INR " + (sf.FinalAmount - sf.PaidAmount),
                    StudentId = sf.StudentId,
                    StudentName = sf.Student.FirstName + " " + sf.Student.LastName,
                    ParentMobile = sf.Student.ParentMobile,
                    DueDate = sf.DueDate.ToString("yyyy-MM-dd"),
                    DaysLeft = sf.DueDate.DayNumber - today.DayNumber,
                    BalanceAmount = sf.FinalAmount - sf.PaidAmount,
                    ActionUrl = "/admin/template-zone"
                })
                .ToListAsync();

            var leadQuery = _context.Leads
                .Where(l => !l.IsDeleted &&
                    l.NextFollowupDate.HasValue &&
                    l.NextFollowupDate.Value <= followupEnd &&
                    l.Status != LeadStatus.Converted &&
                    l.Status != LeadStatus.Lost &&
                    l.Status != LeadStatus.NotInterested);

            if (sessionId.HasValue)
            {
                leadQuery = leadQuery.Where(l => l.SessionId == sessionId.Value);
            }

            var leadFollowups = await leadQuery
                .OrderBy(l => l.NextFollowupDate)
                .Take(20)
                .Select(l => new
                {
                    Type = "LeadFollowup",
                    Priority = l.NextFollowupDate!.Value.Date <= DateTime.Today ? "High" : "Low",
                    Title = "Lead follow-up due",
                    Message = l.Name + " - " + (l.InterestedClass ?? "Interested class not set"),
                    LeadId = l.Id,
                    DueDate = l.NextFollowupDate!.Value.ToString("yyyy-MM-dd"),
                    ActionUrl = "/admin/leads"
                })
                .ToListAsync();

            var items = birthdays.Cast<object>()
                .Concat(fees)
                .Concat(leadFollowups)
                .ToList();

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Admin notifications retrieved successfully",
                Data = new
                {
                    TotalCount = items.Count,
                    BirthdayCount = birthdays.Count,
                    FeeReminderCount = fees.Count,
                    LeadFollowupCount = leadFollowups.Count,
                    Items = items
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving admin notifications");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Error retrieving admin notifications"
            });
        }
    }

    private static DateOnly GetNextBirthday(DateOnly today, DateOnly birthday)
    {
        var day = Math.Min(birthday.Day, DateTime.DaysInMonth(today.Year, birthday.Month));
        var next = new DateOnly(today.Year, birthday.Month, day);
        if (next < today)
        {
            var nextYear = today.Year + 1;
            day = Math.Min(birthday.Day, DateTime.DaysInMonth(nextYear, birthday.Month));
            next = new DateOnly(nextYear, birthday.Month, day);
        }

        return next;
    }
}
