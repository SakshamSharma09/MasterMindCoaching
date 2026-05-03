using MasterMind.API.Data;
using MasterMind.API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace MasterMind.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TemplateZoneController : ControllerBase
{
    private readonly MasterMindDbContext _context;

    public TemplateZoneController(MasterMindDbContext context)
    {
        _context = context;
    }

    [HttpGet("templates")]
    public async Task<ActionResult<ApiResponse<IEnumerable<MessageTemplate>>>> GetTemplates()
    {
        var data = await _context.MessageTemplates
            .Where(t => !t.IsDeleted)
            .OrderBy(t => t.Type)
            .ThenBy(t => t.Name)
            .ToListAsync();

        return Ok(new ApiResponse<IEnumerable<MessageTemplate>> { Success = true, Data = data, Message = "Templates retrieved successfully" });
    }

    [HttpPost("templates")]
    public async Task<ActionResult<ApiResponse<MessageTemplate>>> CreateTemplate([FromBody] MessageTemplate template)
    {
        template.Id = 0;
        template.CreatedAt = DateTime.UtcNow;
        template.UpdatedAt = null;
        template.IsDeleted = false;
        _context.MessageTemplates.Add(template);
        await _context.SaveChangesAsync();
        return Ok(new ApiResponse<MessageTemplate> { Success = true, Data = template, Message = "Template created successfully" });
    }

    [HttpPut("templates/{id}")]
    public async Task<ActionResult<ApiResponse<MessageTemplate>>> UpdateTemplate(int id, [FromBody] MessageTemplate input)
    {
        var existing = await _context.MessageTemplates.FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
        if (existing == null)
        {
            return NotFound(new ApiResponse<MessageTemplate> { Success = false, Message = "Template not found", Data = null });
        }

        existing.Name = input.Name;
        existing.Type = input.Type;
        existing.Subject = input.Subject;
        existing.Body = input.Body;
        existing.IsActive = input.IsActive;
        existing.AutoReminderDaysBefore = input.AutoReminderDaysBefore;
        existing.Frequency = input.Frequency;
        existing.VariablesJson = input.VariablesJson;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(new ApiResponse<MessageTemplate> { Success = true, Data = existing, Message = "Template updated successfully" });
    }

    [HttpDelete("templates/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteTemplate(int id)
    {
        var existing = await _context.MessageTemplates.FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
        if (existing == null)
        {
            return NotFound(new ApiResponse<bool> { Success = false, Message = "Template not found", Data = false });
        }

        existing.IsDeleted = true;
        existing.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(new ApiResponse<bool> { Success = true, Data = true, Message = "Template deleted successfully" });
    }

    [HttpGet("birthday-reminders")]
    public async Task<ActionResult<ApiResponse<IEnumerable<object>>>> GetBirthdayReminders([FromQuery] int daysAhead = 7)
    {
        if (daysAhead < 0) daysAhead = 0;
        if (daysAhead > 60) daysAhead = 60;

        var today = DateOnly.FromDateTime(DateTime.Today);
        var end = today.AddDays(daysAhead);

        var students = await _context.Students
            .Where(s => !s.IsDeleted && s.IsActive)
            .Select(s => new
            {
                s.Id,
                StudentName = s.FirstName + " " + s.LastName,
                s.DateOfBirth,
                s.ProfileImageUrl,
                ParentName = s.ParentName ?? string.Empty
            })
            .ToListAsync();

        var reminders = students
            .Select(s =>
            {
                var dob = DateOnly.FromDateTime(s.DateOfBirth);
                var next = new DateOnly(today.Year, dob.Month, dob.Day);
                if (next < today) next = next.AddYears(1);
                return new { s.Id, s.StudentName, s.ParentName, s.ProfileImageUrl, DateOfBirth = dob, NextBirthday = next, DaysLeft = next.DayNumber - today.DayNumber };
            })
            .Where(x => x.NextBirthday <= end)
            .OrderBy(x => x.NextBirthday)
            .ToList();

        return Ok(new ApiResponse<IEnumerable<object>> { Success = true, Data = reminders, Message = "Birthday reminders generated successfully" });
    }

    [HttpGet("fee-reminders")]
    public async Task<ActionResult<ApiResponse<IEnumerable<object>>>> GetFeeReminders([FromQuery] string? month = null)
    {
        var targetMonth = string.IsNullOrWhiteSpace(month) ? DateTime.Today.ToString("yyyy-MM") : month;

        var data = await _context.StudentFees
            .Where(sf => sf.Status != FeeStatus.Paid)
            .Select(sf => new
            {
                sf.Id,
                sf.StudentId,
                StudentName = sf.Student.FirstName + " " + sf.Student.LastName,
                ParentName = sf.Student.ParentName,
                ClassName = sf.Student.StudentClasses.Where(sc => sc.IsActive).Select(sc => sc.Class != null ? sc.Class.Name : "N/A").FirstOrDefault(),
                FeeAmount = sf.FinalAmount - sf.PaidAmount,
                DueDate = sf.DueDate,
                JoiningDate = sf.Student.AdmissionDate,
                Frequency = sf.FeeStructure.Frequency.ToString()
            })
            .ToListAsync();

        var filtered = data
            .Where(x => x.DueDate.ToString("yyyy-MM") == targetMonth)
            .OrderBy(x => x.DueDate)
            .Select(x => new
            {
                x.Id,
                x.StudentId,
                x.StudentName,
                x.ParentName,
                ClassName = x.ClassName ?? "N/A",
                x.FeeAmount,
                Month = x.DueDate.ToString("yyyy-MM"),
                DueDate = x.DueDate.ToString("yyyy-MM-dd"),
                JoiningDate = x.JoiningDate.ToString("yyyy-MM-dd"),
                x.Frequency
            })
            .ToList();

        return Ok(new ApiResponse<IEnumerable<object>> { Success = true, Data = filtered, Message = "Fee reminder dataset generated successfully" });
    }

    [HttpGet("fee-receipt-logs")]
    public async Task<ActionResult<ApiResponse<IEnumerable<object>>>> GetFeeReceiptLogs([FromQuery] int take = 100)
    {
        if (take < 1) take = 1;
        if (take > 300) take = 300;

        var logs = await _context.FeeReceipts
            .OrderByDescending(r => r.ReceiptDate)
            .Take(take)
            .Select(r => new
            {
                r.Id,
                r.ReceiptNumber,
                r.StudentName,
                r.ParentName,
                r.FeePeriod,
                r.PaidAmount,
                r.TotalAmount,
                r.PaymentMethod,
                ReceiptDate = r.ReceiptDate.ToString("yyyy-MM-dd HH:mm:ss"),
                r.IsEmailSent,
                r.IsSmsSent
            })
            .ToListAsync();

        return Ok(new ApiResponse<IEnumerable<object>> { Success = true, Data = logs, Message = "Fee receipt logs retrieved successfully" });
    }

    [HttpPost("preview")]
    public async Task<ActionResult<ApiResponse<object>>> PreviewTemplate([FromBody] TemplatePreviewRequest request)
    {
        var template = await _context.MessageTemplates.FirstOrDefaultAsync(t => t.Id == request.TemplateId && !t.IsDeleted);
        if (template == null)
        {
            return NotFound(new ApiResponse<object> { Success = false, Message = "Template not found", Data = null });
        }

        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (request.StudentId.HasValue)
        {
            var student = await _context.Students
                .Include(s => s.StudentClasses).ThenInclude(sc => sc.Class)
                .FirstOrDefaultAsync(s => s.Id == request.StudentId.Value && !s.IsDeleted);
            if (student != null)
            {
                values["StudentName"] = $"{student.FirstName} {student.LastName}";
                values["ParentName"] = student.ParentName ?? "";
                values["ClassName"] = student.StudentClasses.FirstOrDefault(sc => sc.IsActive)?.Class?.Name ?? "";
                values["DOB"] = student.DateOfBirth.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
        }

        if (request.StudentFeeId.HasValue)
        {
            var fee = await _context.StudentFees
                .Include(sf => sf.Student)
                .FirstOrDefaultAsync(sf => sf.Id == request.StudentFeeId.Value && !sf.IsDeleted);
            if (fee != null)
            {
                values["FeeAmount"] = (fee.FinalAmount - fee.PaidAmount).ToString("0.00", CultureInfo.InvariantCulture);
                values["FeePeriod"] = fee.Month ?? fee.AcademicYear;
                values["DueDate"] = fee.DueDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                values["StudentName"] = $"{fee.Student.FirstName} {fee.Student.LastName}";
                values["ParentName"] = fee.Student.ParentName ?? "";
            }
        }

        if (request.FeeReceiptId.HasValue)
        {
            var receipt = await _context.FeeReceipts.FirstOrDefaultAsync(r => r.Id == request.FeeReceiptId.Value && !r.IsDeleted);
            if (receipt != null)
            {
                values["ReceiptNumber"] = receipt.ReceiptNumber;
                values["ReceiptDate"] = receipt.ReceiptDate.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                values["FeeAmount"] = receipt.PaidAmount.ToString("0.00", CultureInfo.InvariantCulture);
                values["StudentName"] = receipt.StudentName;
                values["ParentName"] = receipt.ParentName;
                values["ClassName"] = receipt.StudentClass;
            }
        }

        var renderedSubject = RenderTokens(template.Subject, values);
        var renderedBody = RenderTokens(template.Body, values);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Template preview generated successfully",
            Data = new { template.Id, template.Name, renderedSubject, renderedBody, placeholders = values }
        });
    }

    private static string RenderTokens(string text, Dictionary<string, string> values)
    {
        var output = text;
        foreach (var kvp in values)
        {
            output = output.Replace($"{{{{{kvp.Key}}}}}", kvp.Value ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }
        return output;
    }
}

public class TemplatePreviewRequest
{
    public int TemplateId { get; set; }
    public int? StudentId { get; set; }
    public int? StudentFeeId { get; set; }
    public int? FeeReceiptId { get; set; }
}
