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
        var monthEnd = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
        var currentMonth = new DateTime(today.Year, today.Month, 1);

        var teachers = await _context.Teachers
            .Include(t => t.Session)
            .Where(t => !t.IsDeleted && t.IsActive && t.MonthlySalary.HasValue && t.MonthlySalary > 0 &&
                t.JoiningDate <= monthEnd &&
                (!sessionId.HasValue || t.SessionId == sessionId.Value))
            .ToListAsync(cancellationToken);

        if (teachers.Count == 0)
        {
            return;
        }

        var teacherIds = teachers.Select(t => t.Id).ToList();
        var existingRows = await _context.TeacherSalaries
            .Where(s => !s.IsDeleted && teacherIds.Contains(s.TeacherId))
            .Select(s => new { s.TeacherId, s.Month, s.Year })
            .ToListAsync(cancellationToken);

        var existing = existingRows
            .Select(s => SalaryPeriodKey(s.TeacherId, s.Year, s.Month))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var teacher in teachers)
        {
            var startMonth = new DateTime(teacher.JoiningDate.Year, teacher.JoiningDate.Month, 1);
            if (teacher.Session != null)
            {
                var sessionMonth = new DateTime(teacher.Session.StartDate.Year, teacher.Session.StartDate.Month, 1);
                if (sessionMonth > startMonth) startMonth = sessionMonth;
            }

            var finalMonth = currentMonth;
            if (teacher.LeavingDate.HasValue)
            {
                var leavingMonth = new DateTime(
                    teacher.LeavingDate.Value.Year,
                    teacher.LeavingDate.Value.Month,
                    1);
                if (leavingMonth < finalMonth) finalMonth = leavingMonth;
            }

            for (var salaryMonth = startMonth;
                 salaryMonth <= finalMonth;
                 salaryMonth = salaryMonth.AddMonths(1))
            {
                var monthName = CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(salaryMonth.Month);
                var periodKey = SalaryPeriodKey(teacher.Id, salaryMonth.Year, monthName);
                if (existing.Contains(periodKey)) continue;

                var amount = teacher.MonthlySalary!.Value;
                _context.TeacherSalaries.Add(new TeacherSalary
                {
                    TeacherId = teacher.Id,
                    BasicSalary = amount,
                    NetSalary = amount,
                    Month = monthName,
                    Year = salaryMonth.Year,
                    ObligationKey = $"{teacher.Id}:{salaryMonth.Year}:{salaryMonth.Month:D2}",
                    Status = SalaryStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                });
                existing.Add(periodKey);
            }
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

    private static string SalaryPeriodKey(int teacherId, int year, string month) =>
        $"{teacherId}:{year}:{month.Trim()}";
}
