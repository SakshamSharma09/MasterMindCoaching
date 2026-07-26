using System.Globalization;
using MasterMind.API.Data;
using MasterMind.API.Models.Entities;
using MasterMind.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MasterMind.API.Services.Implementations;

public class TeacherSalaryService : ITeacherSalaryService
{
    private readonly MasterMindDbContext _context;

    public TeacherSalaryService(MasterMindDbContext context)
    {
        _context = context;
    }

    public async Task EnsureMonthlyObligationsAsync(
        int? sessionId = null,
        CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;
        var monthName = CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(today.Month);
        var monthEnd = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));

        var teachers = await _context.Teachers
            .Where(t => !t.IsDeleted && t.IsActive && t.MonthlySalary.HasValue && t.MonthlySalary > 0 &&
                t.JoiningDate <= monthEnd &&
                (!sessionId.HasValue || t.SessionId == sessionId.Value))
            .ToListAsync(cancellationToken);

        if (teachers.Count == 0)
        {
            return;
        }

        var teacherIds = teachers.Select(t => t.Id).ToList();
        var existingTeacherIds = await _context.TeacherSalaries
            .Where(s => !s.IsDeleted && s.Year == today.Year && s.Month == monthName &&
                teacherIds.Contains(s.TeacherId))
            .Select(s => s.TeacherId)
            .ToListAsync(cancellationToken);

        var existing = existingTeacherIds.ToHashSet();
        foreach (var teacher in teachers.Where(t => !existing.Contains(t.Id)))
        {
            var amount = teacher.MonthlySalary!.Value;
            var obligationKey = $"{teacher.Id}:{today.Year}:{today.Month:D2}";
            _context.TeacherSalaries.Add(new TeacherSalary
            {
                TeacherId = teacher.Id,
                BasicSalary = amount,
                NetSalary = amount,
                Month = monthName,
                Year = today.Year,
                ObligationKey = obligationKey,
                Status = SalaryStatus.Pending,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TeacherSalary>> GetObligationsAsync(
        int? sessionId = null,
        CancellationToken cancellationToken = default)
    {
        await EnsureMonthlyObligationsAsync(sessionId, cancellationToken);
        return await _context.TeacherSalaries
            .AsNoTracking()
            .Include(s => s.Teacher)
            .Where(s => !s.IsDeleted && !s.Teacher.IsDeleted &&
                (!sessionId.HasValue || s.Teacher.SessionId == sessionId.Value))
            .OrderByDescending(s => s.Year)
            .ThenByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
